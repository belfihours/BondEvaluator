using System.Text;
using AutoFixture;
using BondEvaluator.Application.Helpers.Interface;
using BondEvaluator.Application.Models;
using BondEvaluator.Application.Services;
using BondEvaluator.Domain.Models;
using Microsoft.Extensions.Logging;
using Moq;

namespace BondEvaluator.Application.Test;

public class BondEvaluatorServiceTest
{
    private readonly BondEvaluatorService _sut;
    private readonly Mock<ILogger<BondEvaluatorService>> _loggerMock = new();
    private readonly Mock<IStreamMapper> _streamMapperMock = new();
    private readonly Mock<IRateParser> _rateParserMock = new();
    private readonly Fixture _fixture = new();

    public BondEvaluatorServiceTest()
    {
        _sut = new(_loggerMock.Object, _streamMapperMock.Object, _rateParserMock.Object);
    }
    
    [Fact]
    public void WhenEverythingIsRight_ThenReturnRightStream()
    {
        // Arrange
        var bondInDtos = _fixture.CreateMany<BondInDto>(5); 
        var inputStream = new MemoryStream(Encoding.UTF8.GetBytes("test-stream"));
        _streamMapperMock.Setup(mock=>mock.ReadStreamAsync(inputStream, CancellationToken.None))
            .ReturnsAsync(bondInDtos);
        // var creationalData = bondInDtos.Select(b => new BondCreationData(b.Id, b.BondType));
        
        // Act

        // Assert
    }
}