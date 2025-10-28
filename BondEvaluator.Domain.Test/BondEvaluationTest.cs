using BondEvaluator.Domain.Models;

namespace BondEvaluator.Domain.Test;

public class BondEvaluationTest
{
    // A tiny tolerance is needed to compare this data since we are working with double
    // If higher precision is needed it is possible to consider using decimals instead of double,
    // but this will result in a loss in terms of performance
    private const double Tolerance = 0.02;
    private const string TestBondId = "B001";
    private const string TestIssuer = "test-issuer";
    private const string TestDeskNotes = "test-desk-notes";
    private const string TestRating = "test-rating";
    
    [Theory]
    [InlineData(BondType.Bond, 0.66408, 0.038, 5000, 2, 9.3, 4712.27)]
    [InlineData(BondType.InflationLinked, 0.66408, 0.038, 5000, 2, 9.3, 4712.27)]
    [InlineData(BondType.InflationLinked, 0.54715, 0.0412, 500, 4, 13.7, 479.68)]
    [InlineData(BondType.ZeroCoupon, 0.80599, 0.039, 100, 0, 4.9, 97.21)]
    public void WhenCreatingBondEvaluation_ThenCalculatesRightPv(
        BondType type,
        double discountFactor,
        double rate,
        int faceValue,
        int paymentsPerYear,
        double yearsToMaturity,
        double expectedPv)
    {
        // Arrange
        BondCreationData data = new BondCreationData(
            Type: type,
            DiscountFactor: discountFactor,
            Rate: rate,
            FaceValue: faceValue,
            PaymentsPerYear: paymentsPerYear,
            YearsToMaturity: yearsToMaturity,
            BondId: TestBondId,
            Issuer: TestIssuer,
            DeskNotes: TestDeskNotes,
            Rating: TestRating);
        
        // Act
        var result = BondEvaluation.Create(data);
        
        // Assert
        Assert.True(Math.Abs(Math.Round(result.PresentedValue, 2) - Math.Round(expectedPv, 2)) < Tolerance);
        Assert.Equal(TestBondId, result.BondId);
        Assert.Equal(TestIssuer, result.Issuer);
        Assert.Equal(TestDeskNotes, result.DeskNotes);
        Assert.Equal(TestRating, result.Rating);
    }
}