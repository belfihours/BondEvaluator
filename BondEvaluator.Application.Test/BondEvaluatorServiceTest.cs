using System.Reflection;
using System.Text;
using AutoFixture;
using BondEvaluator.Application.Configuration;
using BondEvaluator.Application.Helpers.Interface;
using BondEvaluator.Application.Models;
using BondEvaluator.Application.Services;
using BondEvaluator.Domain.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace BondEvaluator.Application.Test;

public class BondEvaluatorServiceTest
{
    private readonly BondEvaluatorService _sut;
    private readonly Mock<ILogger<BondEvaluatorService>> _loggerMock = new();
    private readonly Mock<IStreamMapper> _streamMapperMock = new();
    private readonly Mock<IRateParser> _rateParserMock = new();
    private const double TestInflationRate = 0.01;
    private readonly IOptions<BondEvaluatorConfiguration> _options;
    private readonly Fixture _fixture = new();

    public BondEvaluatorServiceTest()
    {
        var config = new BondEvaluatorConfiguration()
        {
            InflationRate = 0.01
        };
        _options = Options.Create(config);
        _sut = new(_loggerMock.Object, _streamMapperMock.Object, _rateParserMock.Object, _options);
    }

    [Fact]
    private void WhenInitializedWithoutLogger_ThenThrows()
    {
        // Act
        var action = () => new BondEvaluatorService(
            null,
            _streamMapperMock.Object,
            _rateParserMock.Object,
            _options);        
        
        // Assert
        Assert.Throws<ArgumentNullException>(action);
    }
    
    [Fact]
    private void WhenInitializedWithoutStreamMapper_ThenThrows()
    {
        // Act
        var action = () => new BondEvaluatorService(
            _loggerMock.Object,
            null,
            _rateParserMock.Object,
            _options);        
        
        // Assert
        Assert.Throws<ArgumentNullException>(action);
    }
    
    [Fact]
    private void WhenInitializedWithoutRateParser_ThenThrows()
    {
        // Act
        var action = () => new BondEvaluatorService(
            _loggerMock.Object,
            _streamMapperMock.Object,
            null,
            _options);        
        
        // Assert
        Assert.Throws<ArgumentNullException>(action);
    }
    
    [Fact]
    private void WhenInitializedWithoutOptions_ThenThrows()
    {
        // Arrange
        
        // Act
        var action = () => new BondEvaluatorService(
            _loggerMock.Object,
            _streamMapperMock.Object,
            _rateParserMock.Object,
            null);        
        
        // Assert
        Assert.Throws<ArgumentNullException>(action);
    }
    
    [Fact]
    public async Task WhenStreamIsValid_ThenReturnRightStream()
    {
        // Arrange
        var bondInDtos = GivenBondInDtos(5);
        var inputStream = new MemoryStream("test-stream-in"u8.ToArray());
        var outputStream = new MemoryStream("test-stream-out"u8.ToArray());
        SetupStreamMapperRead(inputStream, bondInDtos);
        SetupRateParser("0");
        var bondEvaluations = bondInDtos.Select(GetCreationData).Select(BondEvaluation.Create);
        var outDtos = GivenBondOutDtos(bondEvaluations);
        SetupStreamMapperWrite(outDtos, outputStream);

        // Act
        var res = await _sut.GetBondEvaluation(inputStream, CancellationToken.None);

        // Assert
        Assert.Equal(outputStream, res);
        ThenReadIsCalled(inputStream);
        ThenWriteIsCalled(outDtos);
        ThenParseRateIsCalledExactly("0", bondInDtos.Length);
    }
    
    [Fact]
    public async Task WhenOneRateCannotBeParsed_ThenSkipsThatLine()
    {
        // Arrange
        var badRate = "bad-rate";
        var bondInDtos = GivenBondInDtos(5);
        var badBondInDto = _fixture.Build<BondInDto>().With(b => b.Rate, badRate).Create();
        var inputStream = new MemoryStream("test-stream-in"u8.ToArray());
        var outputStream = new MemoryStream("test-stream-out"u8.ToArray());
        SetupStreamMapperRead(inputStream, [..bondInDtos, badBondInDto]);
        SetupRateParser("0");
        SetupRateParser(badRate, false);
        var bondEvaluations = bondInDtos.Select(GetCreationData).Select(BondEvaluation.Create);
        var outDtos = GivenBondOutDtos(bondEvaluations);
        SetupStreamMapperWrite(outDtos, outputStream);

        // Act
        var res = await _sut.GetBondEvaluation(inputStream, CancellationToken.None);

        // Assert
        Assert.Equal(outputStream, res);
        ThenReadIsCalled(inputStream);
        ThenWriteIsCalled(outDtos);
        ThenParseRateIsCalledExactly("0", outDtos.Length);
        ThenParseRateIsCalledExactly(badRate, 1);
    }

    private void ThenParseRateIsCalledExactly(string rate, int count)
    {
        double tryParseRes = 0;
        _rateParserMock
            .Verify(mock=>mock.TryParseRate(rate, TestInflationRate, out tryParseRes), Times.Exactly(count));
    }

    private void ThenWriteIsCalled(BondOutDto[] outDtos)
    {
        _streamMapperMock.Verify(mock=>mock.WriteStreamAsync(outDtos,  CancellationToken.None), Times.Once);
    }

    private void ThenReadIsCalled(MemoryStream inputStream)
    {
        _streamMapperMock.Verify(mock=>mock.ReadStreamAsync(inputStream,  CancellationToken.None), Times.Once);
    }

    private void SetupRateParser(string textRate, bool res = true)
    {
        double rate;
        _rateParserMock.Setup(mock =>
            mock.TryParseRate(textRate, It.IsAny<double>(), out rate)).Returns(res);
    }

    private void SetupStreamMapperWrite(IEnumerable<BondOutDto> outDtos, MemoryStream outputStream)
    {
        _streamMapperMock.Setup(mock => mock.WriteStreamAsync(outDtos, CancellationToken.None))
            .ReturnsAsync(outputStream);
    }

    private void SetupStreamMapperRead(MemoryStream inputStream, BondInDto[] bondInDtos)
    {
        _streamMapperMock.Setup(mock=>mock.ReadStreamAsync(inputStream, CancellationToken.None))
            .ReturnsAsync(bondInDtos);
    }
    
    private static BondOutDto[] GivenBondOutDtos(IEnumerable<BondEvaluation> bondEvaluations)
    {
        return bondEvaluations
            .Select(b => new BondOutDto(b.BondId, b.Issuer, b.Type, b.PresentedValue, b.Rating, b.DeskNotes))
            .ToArray();
    }

    private BondInDto[] GivenBondInDtos(int count)
    {
        return _fixture.Build<BondInDto>()
            .With(b=>b.Rate, "0")
            .CreateMany(count)
            .ToArray();
    }

    private static BondCreationData GetCreationData(BondInDto inDto)
    {
        return new(
            inDto.BondId,
            inDto.Issuer,
            inDto.Type,
            int.Parse(inDto.Rate),
            (int)inDto.PaymentFrequency,
            inDto.YearsToMaturity,
            inDto.FaceValue,
            inDto.DiscountFactor,
            inDto.Rating,
            inDto.DeskNotes);
    }
}