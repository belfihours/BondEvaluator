using BondEvaluator.Application.Mappers;
using BondEvaluator.Application.Models;

namespace BondEvaluator.Infrastructure.Mappers;

public class StreamMapper : IStreamMapper
{
    public async Task<IEnumerable<BondInDto>> ReadStreamAsync(Stream stream)
    {
        var rows = new List<string[]>();

        using var reader = new StreamReader(stream);
        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(line)) continue;

            var values = line.Split(';'); // Assumes comma-separated
            rows.Add(values);
        }
        throw new NotImplementedException();
    }

    public Task<Stream> WriteStreamAsync(IEnumerable<BondOutDto> dtos)
    {
        throw new NotImplementedException();
    }
}