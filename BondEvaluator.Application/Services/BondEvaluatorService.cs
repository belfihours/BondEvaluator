using BondEvaluator.Application.Configuration;
using BondEvaluator.Application.Exceptions;
using BondEvaluator.Application.Helpers.Interface;
using BondEvaluator.Application.Models;
using BondEvaluator.Domain.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BondEvaluator.Application.Services;

public class BondEvaluatorService :IBondEvaluatorService
{
    private readonly ILogger<BondEvaluatorService> _logger;
    private readonly IStreamMapper _streamMapper;
    private readonly IRateParser _rateParser;
    private readonly BondEvaluatorConfiguration _configuration;

    public BondEvaluatorService(
        ILogger<BondEvaluatorService> logger,
        IStreamMapper streamMapper,
        IRateParser rateParser,
        IOptions<BondEvaluatorConfiguration> options)
    {
        ArgumentNullException.ThrowIfNull(options?.Value, nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _streamMapper = streamMapper ??  throw new ArgumentNullException(nameof(streamMapper));
        _rateParser = rateParser ?? throw new ArgumentNullException(nameof(rateParser));
        _configuration = options.Value;
    }

    public async Task<Stream> GetBondEvaluation(Stream stream, CancellationToken ct = default)
    {
        _logger.LogInformation("Starting bond evaluation");
        var inDtos = await _streamMapper.ReadStreamAsync(stream, ct);
        var outDtos = GetCreationData(inDtos)
            .Select(BondEvaluation.Create)
            .Select(GetOutDto);
        var outStream = await _streamMapper.WriteStreamAsync(outDtos, ct);
        _logger.LogInformation("Ended bond evaluation");
        return outStream;
    }

    private List<BondCreationData> GetCreationData(IEnumerable<BondInDto> inDtos)
    {
        List<BondCreationData> creationData = [];
        foreach (var inDto in inDtos)
        {
            try
            {
                creationData.Add(GetCreationData(inDto));
            }
            catch (BondParserException)
            {
                _logger.LogWarning("Skipping line with BondId: {BondId}, " +
                                   "expected valid rate, but received: {Rate}.",
                    inDto.BondId,
                    inDto.Rate);
            }
        }
        return creationData;
    }

    private BondCreationData GetCreationData(BondInDto inDto)
    {
        if (_rateParser.TryParseRate(inDto.Rate, _configuration.InflationRate, out var rate))
            return new(
                inDto.BondId,
                inDto.Issuer,
                inDto.Type,
                rate,
                (int)inDto.PaymentFrequency,
                inDto.YearsToMaturity,
                inDto.FaceValue,
                inDto.DiscountFactor,
                inDto.Rating,
                inDto.DeskNotes);
        
        _logger.LogWarning("Expected a valid Rate for BondId: {BondId}, " +
                         "received rate: {Rate}.", inDto.BondId, inDto.Rate);
        throw new BondParserException($"Rate {inDto.Rate} is not a valid rate");
    }

    private static BondOutDto GetOutDto(BondEvaluation bondEvaluation)
    {
        return new BondOutDto(
            bondEvaluation.BondId,
            bondEvaluation.Issuer,
            bondEvaluation.Type,
            bondEvaluation.PresentedValue,
            bondEvaluation.Rating,
            bondEvaluation.DeskNotes);
    }
}