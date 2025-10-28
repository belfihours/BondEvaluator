using System.ComponentModel;
using System.Globalization;
using System.Text;
using BondEvaluator.Application.Exceptions;
using BondEvaluator.Application.Helpers.Interface;
using BondEvaluator.Application.Models;
using BondEvaluator.Domain.Models;
using Microsoft.Extensions.Logging;

namespace BondEvaluator.Infrastructure.Mappers;

/// <summary>
/// Mapper class that translate data into Stream and vice versa
/// Right now only the sample format is accepted for simplicity,
/// If multiple sources are accepted a different approach would be required, as accepting different headers
/// or accepting no headers at all
/// </summary>
public class StreamMapper : IStreamMapper
{
    private readonly ILogger<StreamMapper> _logger;
    private const string ExpectedHeader =
        "BondID;Issuer;Rate;FaceValue;PaymentFrequency;Rating;Type;YearsToMaturity;DiscountFactor;DeskNotes";
    public StreamMapper(ILogger<StreamMapper> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<BondInDto>> ReadStreamAsync(Stream stream, CancellationToken ct = default)
    {
        var rows = new List<BondInDto>();

        using var reader = new StreamReader(stream);
        var header = await reader.ReadLineAsync(ct);
        if (!ExpectedHeader.Equals(header, StringComparison.InvariantCultureIgnoreCase))
        {
            _logger.LogError("Expected header to be: {ExpectedHeader}, " +
                             "but received: {ActualHeader}.",
                ExpectedHeader,
                header);
            throw new BondParserException("Expected valid header");
        }
        
        var rowNumber = 0;
        while (!reader.EndOfStream)
        {
            rowNumber++;
            ct.ThrowIfCancellationRequested();
            
            var line = await reader.ReadLineAsync(ct);
            if (string.IsNullOrWhiteSpace(line)) continue;
            var values = line.Split(';');
            try
            {
                var bond = GetBondFromLine(values);
                rows.Add(bond);
            }
            catch (Exception ex)
            {
                if (ex is not (ArgumentException or FormatException)) 
                    throw;
                _logger.LogWarning(ex, "Skipping line: {RowNumber} with bondId: {BondId}," +
                                       " error reading stream.",
                    rowNumber,
                    values[0]);
            }
        }
        return rows;
    }

    public async Task<Stream> WriteStreamAsync(IEnumerable<BondOutDto> dtos, CancellationToken ct = default)
    {
        var header =
            string.Join(";", typeof(BondOutDto).GetProperties().Select(f => f.Name));
            
        var stream = new MemoryStream();
        await using var writer = new StreamWriter(stream, Encoding.UTF8, leaveOpen: true);

        await writer.WriteLineAsync(header).WaitAsync(ct);

        foreach (var dto in dtos)
        {
            ct.ThrowIfCancellationRequested();

            var line = $"{dto.BondId};{dto.Issuer};{dto.Type};{Math.Round(dto.PresentedValue, 2)};{dto.Rating};{dto.DeskNotes}";
            await writer.WriteLineAsync(line).WaitAsync(ct);
        }

        await writer.FlushAsync(ct).WaitAsync(ct);
        stream.Position = 0;

        return stream;
    }
    
    private static BondInDto GetBondFromLine(string[] values)
    {
        return new BondInDto(
            values[0],
            values[1],
            values[2],
            int.Parse(values[3]),
            GetEnumFromDescription<PaymentFrequency>(values[4]),
            values[5],
            GetEnumFromDescription<BondType>(values[6]),
            double.Parse(values[7], NumberStyles.Number, CultureInfo.InvariantCulture),
            double.Parse(values[8], NumberStyles.Number, CultureInfo.InvariantCulture),
            values[9]);
    }
    
    private static T GetEnumFromDescription<T>(string description) where T : Enum
    {
        foreach (var field in typeof(T).GetFields())
        {
            var attr = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
            if ((attr != null && attr.Description.Equals(description, StringComparison.OrdinalIgnoreCase)) ||
                field.Name.Equals(description, StringComparison.OrdinalIgnoreCase))
            {
                return (T)field.GetValue(null)!;
            }
        }
        throw new ArgumentException($"'{description}' does not match any value of enum {typeof(T).Name}");
    }
}
