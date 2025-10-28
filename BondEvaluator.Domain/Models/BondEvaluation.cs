using System.Runtime.CompilerServices;

namespace BondEvaluator.Domain.Models;

public class BondEvaluation
{
    public string BondId {get; }
    public string Issuer {get; }
    public BondType Type { get; }
    public double PresentedValue {get; }
    public string Rating { get; set; }
    public string DeskNotes {get; }
    private BondEvaluation(
        string bondId,
        string issuer,
        BondType type,
        double presentedValue,
        string rating,
        string deskNotes)
    {
        BondId = bondId;
        Issuer = issuer;
        Type = type;
        PresentedValue = presentedValue;
        Rating = rating;
        DeskNotes = deskNotes;
    }

    public static BondEvaluation Create(BondCreationData data)
    {
        var factor = GetFactor(data);
        var pv = factor * data.FaceValue * data.DiscountFactor;
        return new BondEvaluation(
            data.BondId,
            data.Issuer,
            data.Type,
            pv,
            data.Rating,
            data.DeskNotes);
    }

    private static double GetFactor(BondCreationData data)
    {
        if (data.Type == BondType.ZeroCoupon)
        {
            return Math.Pow((1 + data.Rate), data.YearsToMaturity);
        }
        // Since it is not otherwise specified, I assume I can treat Inflation-Linked as Bond to calculate PV
        var cpp = data.Rate / data.PaymentsPerYear;
        var n = data.YearsToMaturity * data.PaymentsPerYear;
        return Math.Pow((1 + cpp), n);
    }
}