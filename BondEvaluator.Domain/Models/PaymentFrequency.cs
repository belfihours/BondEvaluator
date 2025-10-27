using System.ComponentModel;

namespace BondEvaluator.Domain.Models;

public enum PaymentFrequency
{
    [Description("None")]
    None = 0,
    [Description("Annual")]
    Annually = 1,
    [Description("Semi-Annual")]
    SemiAnnually = 2,
    [Description("Quarterly")]
    Quarterly = 4
}