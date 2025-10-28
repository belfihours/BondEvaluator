using BondEvaluator.Domain.Models;

namespace BondEvaluator.Application.Models;

public record BondOutDto(
    string BondID,
    string Issuer,
    BondType Type,
    double PresentedValue,
    string Rating,
    string DeskNotes);