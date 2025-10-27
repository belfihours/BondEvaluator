using BondEvaluator.Domain.Models;

namespace BondEvaluator.Application.Models;

public record BondInDto(
    string BondId,
    string Issuer,
    string Rate,
    int FaceValue,
    PaymentFrequency PaymentFrequency,
    string Rating,
    BondType Type,
    double YearsToMaturity,
    double DiscountFactor,
    string DeskNotes);

