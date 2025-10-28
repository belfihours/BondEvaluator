using System.Globalization;
using BondEvaluator.Application.Models;
using BondEvaluator.Domain.Models;
using BondEvaluator.Infrastructure.Mappers;
using Microsoft.Extensions.Logging;
using Moq;

namespace BondEvaluator.Infrastructure.Test;

public class StreamMapperTest
{
    private readonly StreamMapper _sut;
    private readonly Mock<ILogger<StreamMapper>> _loggerMock = new();
    private const string InputHeader =
        "BondID;Issuer;Rate;FaceValue;PaymentFrequency;Rating;Type;YearsToMaturity;DiscountFactor;DeskNotes";
    private const string OutputHeader =
        "BondID;Issuer;Type;PresentedValue;Rating;DeskNotes";
    
    
    public StreamMapperTest()
    {
        _sut = new(_loggerMock.Object);
    }
    
    [Fact]
    public void WhenInitializedWithoutLogger_ThenThrows()
    {
        // Act
        var action = () => new StreamMapper(null!);
        
        // Assert
        Assert.Throws<ArgumentNullException>(action);
    }

    [Fact]
    public async Task WhenMappingFromStream_ThenTakeRightValues()
    {
        // Arrange
        string[] rows = [
            "B095;EuroRail;3.10%;100;None;BBB+;Zero-Coupon;4.7;0.81312;Issued at discount",
            "B096;Green Infrastructure;Inflation+0.74%;5000;Quarterly;AA;Inflation-Linked;8.2;0.69702;Indexing details unclear",
            "B097;EuroBank;Inflation+1.44%;5000;Semi-Annual;A-;Inflation-Linked;11.2;0.6108;Indexing details unclear",
            "B098;TelecomNet;2.00%;2000;Annual;A+;Bond;7.9;0.70629;Corporate bond",
        ];
        var stream = GivenStreamFromStrings([InputHeader, ..rows]);
        
        // Act
        var res = (await _sut.ReadStreamAsync(stream, CancellationToken.None)).ToList();
        
        // Assert
        Assert.Equal(res.Count, rows.Length);
        ThenLineIsEqual(res[0]);
        ThenEnumsAreCorrectlyMapped(res);
        ThenBondTypesAreCorrectlyMapped(res);
    }
    
    [Fact]
    public async Task WhenSomeLinesAreCorrupted_ThenSkipsThem()
    {
        // Arrange
        string[] rows = [
            "B095;EuroRail;3.10%;100;None;BBB+;Zero-Coupon;4.7;0.81312;Issued at discount",
            "B096;Green Infrastructure;Inflation+0.74%;500xxx;Quarterly;AA;Inflation-Linked;8.2;0.69702;Indexing details unclear",
            "B097;EuroBank;Inflation+1.44%;5000;Semi-xxx;A-;Inflation-Linked;11.2;0.6108;Indexing details unclear",
            "B098;TelecomNet;2.00%;2000;Annual;A+;Bond-xxx;5.2;0.70629;Corporate bond",
            "B098;TelecomNet;2.00%;2000;Annual;A+;Bond;xxx;0.70629;Corporate bond",
            "B098;TelecomNet;2.00%;2000;Annual;A+;Bond;5.2;xxxx;Corporate bond",
        ];
        var stream = GivenStreamFromStrings([InputHeader, ..rows]);
        
        // Act
        var res = (await _sut.ReadStreamAsync(stream, CancellationToken.None)).ToList();
        
        // Assert
        Assert.Single(res);
        ThenLineIsEqual(res[0]);
        ThenLogsTimes(5);
    }

    [Fact]
    public async Task WhenMappingFromDtoToStream_ThenGenerateRightStream()
    {
        // Arrange
        var bondId = "test-bond-id";
        var issuer = "test-issuer";
        var type = BondType.Bond;
        var pv = 1234.56;
        var rating = "test-rating";
        var notes = "test-notes";
        List<BondOutDto> boundOutDtos = [new(bondId, issuer, type, pv, rating, notes)];
        
        // Act
        var res = await _sut.WriteStreamAsync(boundOutDtos, CancellationToken.None);
        
        // Assert
        using var reader = new StreamReader(res);
        await ThenHeaderMatches(reader);
        var data = await ThenRowCountIsCorrect(reader);
        ThenDataMathces(bondId, data, issuer, type, pv, rating, notes);
    }

    private static void ThenDataMathces(string bondId, string[] data, string issuer, BondType type, double pv,
        string rating, string notes)
    {
        Assert.Equal(bondId, data[0]);
        Assert.Equal(issuer, data[1]);
        Assert.Equal(type.ToString(), data[2]);
        Assert.Equal(pv.ToString(CultureInfo.InvariantCulture), data[3]);
        Assert.Equal(rating, data[4]);
        Assert.Equal(notes, data[5]);
    }

    private static async Task<string[]> ThenRowCountIsCorrect(StreamReader reader)
    {
        var count = 0;
        string[] data = [];
        while (!reader.EndOfStream)
        {
            count++;
            var line = await reader.ReadLineAsync();
            data = line!.Split(";");
        }
        Assert.Equal(1, count);
        return data;
    }

    private static async Task ThenHeaderMatches(StreamReader reader)
    {
        var header = await reader.ReadLineAsync();
        Assert.Equal(OutputHeader, header);
    }

    private void ThenLogsTimes(int times)
    {
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Skipping line: ")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Exactly(times));
    }

    private static void ThenLineIsEqual(BondInDto dto)
    {
        Assert.Equal("B095", dto.BondId);
        Assert.Equal("EuroRail", dto.Issuer);
        Assert.Equal("3.10%", dto.Rate);
        Assert.Equal("BBB+", dto.Rating);
        Assert.Equal(4.7, dto.YearsToMaturity);
        Assert.Equal(0.81312, dto.DiscountFactor);
        Assert.Equal("Issued at discount", dto.DeskNotes);
    }

    private static void ThenBondTypesAreCorrectlyMapped(List<BondInDto> res)
    {
        Assert.Equal(BondType.ZeroCoupon, res[0].Type);
        Assert.Equal(BondType.InflationLinked, res[1].Type);
        Assert.Equal(BondType.Bond, res[3].Type);
    }

    private static void ThenEnumsAreCorrectlyMapped(List<BondInDto> res)
    {
        Assert.Equal(PaymentFrequency.None, res[0].PaymentFrequency);
        Assert.Equal(PaymentFrequency.Quarterly, res[1].PaymentFrequency);
        Assert.Equal(PaymentFrequency.SemiAnnually, res[2].PaymentFrequency);
        Assert.Equal(PaymentFrequency.Annually, res[3].PaymentFrequency);
    }

    private static Stream GivenStreamFromStrings(string[] rows)
    {
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        foreach (var row in rows)
        {
            writer.WriteLine(row);
        }
        writer.Flush();
        stream.Position = 0;
        return stream;
    }
}