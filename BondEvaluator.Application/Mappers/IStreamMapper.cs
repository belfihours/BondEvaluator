using BondEvaluator.Application.Models;

namespace BondEvaluator.Application.Mappers;

public interface IStreamMapper
{
    Task<IEnumerable<BondInDto>> ReadStreamAsync(Stream stream);
    Task<Stream> WriteStreamAsync(IEnumerable<BondOutDto> dtos);

}