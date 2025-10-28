using BondEvaluator.Application.Helpers;

namespace BondEvaluator.Application.Test;

public class RateParserTest
{
    private readonly RateParser _sut = new();
    
    [Theory]
    [InlineData("15%", 0.15)]
    [InlineData("3.8%", 0.038)]
    [InlineData("0.1", 0.001)]
    public void WhenParsingPercentages_TheReturnsRightPercentage(string text, double expected)
    {
        // Act
        var parsed = _sut.TryParseRate(text, 0, out var res);
        
        // Assert
        Assert.True(parsed);
        Assert.Equal(expected, res);
    }
    
    [Theory]
    [InlineData("Inflation+15%", 0.001, 0.151)]
    [InlineData("Inflation+3.8%", 0.01, 0.048)]
    [InlineData("Inflation+0.1", 0.1, 0.101)]
    public void WhenParsingInflationRates_TheReturnsRightRate(string text, double inflationRate, double expected)
    {
        // Act
        var parsed = _sut.TryParseRate(text, inflationRate, out var res);
        
        // Assert
        Assert.True(parsed);
        Assert.Equal(expected, res);
    }
    
    [Theory]
    [InlineData("Infation+15%")]
    [InlineData("Inflation3.8%")]
    [InlineData("Inflation-0.1%")]
    [InlineData("Inflation-0.1")]
    [InlineData("0.1%+Inflation")]
    [InlineData("0.1+Inflation")]
    public void WhenParsingUnexpectedRates_ThenReturnsFalse(string text)
    {
        // Act
        var parsed = _sut.TryParseRate(text, 0, out var res);
        
        // Assert
        Assert.False(parsed);
    }
}