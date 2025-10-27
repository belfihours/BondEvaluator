namespace BondEvaluator.Domain.Models;

public record BondCreationData(
    string BondId,
    string Issuer,
    BondType Type,
    double Rate,
    int PaymentsPerYear,
    double YearsToMaturity,
    int FaceValue,
    double DiscountFactor,
    string DeskNotes);