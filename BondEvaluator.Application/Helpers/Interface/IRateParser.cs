namespace BondEvaluator.Application.Helpers.Interface;

public interface IRateParser
{
    bool TryParseRate(string text, double inflationBase, out double result);
}