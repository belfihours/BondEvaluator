using System.Globalization;
using BondEvaluator.Application.Helpers.Interface;

namespace BondEvaluator.Application.Helpers;

public class RateParser : IRateParser
{
    //TODO: TEST THAT
    /// <summary>
    /// Try to parse input rates in final rates
    /// Percentages are interpretated as fractions, so 1.5% => 0.015
    /// </summary>
    /// <param name="text"> Expected to be in form "Inflation+{number}%" or "{number}%" </param>
    /// <param name="inflationBase"> Inflation to add in case of Inflation-linked rates </param>
    /// <param name="result"></param>
    /// <returns></returns>
    public bool TryParseRate(string text, double inflationBase, out double result)
    {
        result = 0.0;
        double prc;
        if (string.IsNullOrWhiteSpace(text)) return false;

        var s = text.Trim();

        var plusIndex = s.IndexOf('+');
        if (plusIndex >= 0)
        {
            var left = s[..plusIndex];
            var right = s[(plusIndex + 1)..];

            if (!left.Equals("Inflation", StringComparison.OrdinalIgnoreCase))
                return false;

            if (!TryParsePercentageNumber(right, out prc)) 
                return false;

            result = inflationBase + prc;
            return true;
        }

        if (!TryParsePercentageNumber(s, out prc)) 
            return false;

        result = prc;
        return true;
    }

    private static bool TryParsePercentageNumber(string num, out double result)
    {
        result = 0.0;
        if (string.IsNullOrWhiteSpace(num)) 
            return false;

        // Remove last character (%)
        if (num.EndsWith('%'))
            num = num[..^1];
        if (!double.TryParse(num, NumberStyles.Number, CultureInfo.InvariantCulture, out var value))
            return false;

        result = value / 100.0;
        return true;
    }
}