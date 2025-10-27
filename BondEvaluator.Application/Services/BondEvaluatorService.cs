using BondEvaluator.Application.Mappers;
using BondEvaluator.Application.Models;
using Microsoft.Extensions.Logging;

namespace BondEvaluator.Application.Services;

public class BondEvaluatorService :IBondEvaluatorService
{
    private readonly ILogger<BondEvaluatorService> _logger;
    private readonly IStreamMapper _streamMapper;

    public BondEvaluatorService(
        ILogger<BondEvaluatorService> logger,
        IStreamMapper streamMapper)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _streamMapper = streamMapper ??  throw new ArgumentNullException(nameof(streamMapper));
    }

    public async Task<Stream> GetBondEvaluation(Stream stream)
    {
        var inDtos = await _streamMapper.ReadStreamAsync(stream);
        var outDtos = inDtos.Select(x => new BondOutDto(x.Id, x.BondType));
        var outStream = await _streamMapper.WriteStreamAsync(outDtos);
        return outStream;
    }
}