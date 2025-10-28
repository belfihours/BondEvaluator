namespace BondEvaluator.Application.Configuration;

public class BondEvaluatorConfiguration
{
    public static readonly string Section = "BondEvaluator";
    public required double InflationRate { get; init; }
}