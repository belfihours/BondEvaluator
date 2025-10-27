using BondEvaluator.Application.Models;

namespace BondEvaluator.Application.Helpers.Interface;

public interface IStreamMapper
{
    Task<IEnumerable<BondInDto>> ReadStreamAsync(Stream stream, CancellationToken ct = default);
    Task<Stream> WriteStreamAsync(IEnumerable<BondOutDto> dtos, CancellationToken ct = default);

}