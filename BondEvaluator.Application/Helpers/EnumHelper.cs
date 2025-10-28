using System.ComponentModel;

namespace BondEvaluator.Application.Helpers;

public static class EnumHelper
{
    public static T GetEnumFromDescription<T>(string description) where T : Enum
    {
        foreach (var field in typeof(T).GetFields())
        {
            var attr = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
            if ((attr != null && attr.Description.Equals(description, StringComparison.OrdinalIgnoreCase)) ||
                field.Name.Equals(description, StringComparison.OrdinalIgnoreCase))
            {
                return (T)field.GetValue(null)!;
            }
        }
        throw new ArgumentException($"'{description}' does not match any value of enum {typeof(T).Name}");
    }
}