using BondEvaluator.Domain.Models;

namespace BondEvaluator.Application.Models;

public record BondOutDto(
    string BondId,
    string Issuer,
    BondType Type,
    double PresentedValue,
    string DeskNotes);