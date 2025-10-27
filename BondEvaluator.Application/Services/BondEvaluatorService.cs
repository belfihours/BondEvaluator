using BondEvaluator.Application.Helpers.Interface;
using BondEvaluator.Application.Models;
using BondEvaluator.Domain.Models;
using Microsoft.Extensions.Logging;

namespace BondEvaluator.Application.Services;

public class BondEvaluatorService :IBondEvaluatorService
{
    private readonly ILogger<BondEvaluatorService> _logger;
    private readonly IStreamMapper _streamMapper;
    private readonly IRateParser _rateParser;
    //TODO: move that to configuration or find somewhere else to find that
    private readonly double _inflationRate = 0.032;

    public BondEvaluatorService(
        ILogger<BondEvaluatorService> logger,
        IStreamMapper streamMapper, IRateParser rateParser)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _streamMapper = streamMapper ??  throw new ArgumentNullException(nameof(streamMapper));
        _rateParser = rateParser ?? throw new ArgumentNullException(nameof(rateParser));
    }

    public async Task<Stream> GetBondEvaluation(Stream stream, CancellationToken ct = default)
    {
        var inDtos = await _streamMapper.ReadStreamAsync(stream, ct);
        List<BondCreationData> list = new();
        List<BondEvaluation> evaluations = new();
        foreach (var inDto in  inDtos)
        {
            var data = GetCreationData(inDto);
            evaluations.Add(BondEvaluation.Create(data));
            list.Add(data);
        }

        var outDtos = evaluations.Select(GetOutDto);
        var outStream = await _streamMapper.WriteStreamAsync(outDtos, ct);
        return outStream;
    }

    private BondCreationData GetCreationData(BondInDto inDto)
    {
        if (!_rateParser.TryParseRate(inDto.Rate, _inflationRate, out var rate))
            throw new ArgumentException();
        
        return new(
            inDto.BondId,
            inDto.Issuer,
            inDto.Type,
            rate,
            (int)inDto.PaymentFrequency,
            inDto.YearsToMaturity,
            inDto.FaceValue,
            inDto.DiscountFactor,
            inDto.DeskNotes);
    }

    private BondOutDto GetOutDto(BondEvaluation bondEvaluation)
    {
        return new BondOutDto(
            bondEvaluation.BondId,
            bondEvaluation.Issuer,
            bondEvaluation.Type,
            bondEvaluation.PresentedValue,
            bondEvaluation.DeskNotes);
    }
}