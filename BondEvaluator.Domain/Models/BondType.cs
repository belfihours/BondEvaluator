using System.ComponentModel;

namespace BondEvaluator.Domain.Models;

public enum BondType
{
    [Description("Inflation-Linked")]
    InflationLinked,
    [Description("Bond")]
    Bond,
    [Description("Zero-Coupon")]
    ZeroCoupon
}