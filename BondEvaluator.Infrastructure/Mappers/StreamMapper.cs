using System.ComponentModel;
using System.Globalization;
using System.Text;
using BondEvaluator.Application.Helpers.Interface;
using BondEvaluator.Application.Models;
using BondEvaluator.Domain.Models;

namespace BondEvaluator.Infrastructure.Mappers;

public class StreamMapper : IStreamMapper
{
    private const string ExpectedHeader =
        "BondID;Issuer;Rate;FaceValue;PaymentFrequency;Rating;Type;YearsToMaturity;DiscountFactor;DeskNotes";
    public async Task<IEnumerable<BondInDto>> ReadStreamAsync(Stream stream, CancellationToken ct = default)
    {
        var rows = new List<BondInDto>();

        using var reader = new StreamReader(stream);
        var header = await reader.ReadLineAsync(ct);
        if (header != ExpectedHeader)
            //TODO: create custom exception
            throw new ArgumentException();
        
        while (!reader.EndOfStream)
        {
            ct.ThrowIfCancellationRequested();
            
            var line = await reader.ReadLineAsync(ct);
            if (string.IsNullOrWhiteSpace(line)) continue;
            var values = line.Split(';');
            rows.Add(new BondInDto(
                values[0],
                values[1],
                values[2],
                Int16.Parse(values[3]),
                GetEnumFromDescription<PaymentFrequency>(values[4]),
                values[5],
                GetEnumFromDescription<BondType>(values[6]),
                double.Parse(values[7], NumberStyles.Number, CultureInfo.InvariantCulture),
                double.Parse(values[8], NumberStyles.Number, CultureInfo.InvariantCulture),
                values[9]));
        }

        return rows;
    }
    
    public async Task<Stream> WriteStreamAsync(IEnumerable<BondOutDto> dtos, CancellationToken ct = default)
    {
        var header =
            string.Join(";", typeof(BondOutDto).GetProperties().Select(f => f.Name));
            
        var stream = new MemoryStream();
        await using var writer = new StreamWriter(stream, Encoding.UTF8, leaveOpen: true);

        // 1. Write header
        await writer.WriteLineAsync(header).WaitAsync(ct);

        // 2. Write rows
        foreach (var dto in dtos)
        {
            ct.ThrowIfCancellationRequested();

            var line = $"{dto.BondId};{dto.Issuer};{dto.Type};{Math.Round(dto.PresentedValue, 2)};{dto.DeskNotes}";
            await writer.WriteLineAsync(line).WaitAsync(ct);
        }

        // 3. Flush and rewind
        await writer.FlushAsync(ct).WaitAsync(ct);
        stream.Position = 0;

        return stream;
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
