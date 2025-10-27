using System.Runtime.CompilerServices;

namespace BondEvaluator.Domain.Models;

public class BondEvaluation
{
    public string BondId {get; }
    public string Issuer {get; }
    public BondType Type { get; }
    public double PresentedValue {get; }
    public string DeskNotes {get; }
    private BondEvaluation(
        string bondId,
        string issuer,
        BondType type,
        double presentedValue,
        string deskNotes)
    {
        BondId = bondId;
        Issuer = issuer;
        Type = type;
        PresentedValue = presentedValue;
        DeskNotes = deskNotes;
    }

    public static BondEvaluation Create(BondCreationData data)
    {
        if (data.BondId == "B082")
        {
            ;
        }
        var factor = GetFactor(data);
        var pv = factor * data.FaceValue * data.DiscountFactor;
        return new BondEvaluation(
            data.BondId,
            data.Issuer,
            data.Type,
            pv,
            data.DeskNotes);
    }

    private static double GetFactor(BondCreationData data)
    {
        if (data.Type == BondType.ZeroCoupon)
        {
            return Math.Pow((1 + data.Rate), data.YearsToMaturity);
        }
        // Since it is not otherwise specified, I assume I can treat Bond as Inflation-Linked to calculate PV
        var cpp = data.Rate / data.PaymentsPerYear;
        var n = data.YearsToMaturity * data.PaymentsPerYear;
        return Math.Pow((1 + cpp), n);
    }
}