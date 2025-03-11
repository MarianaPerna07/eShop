using System.Diagnostics;
using System.Text.RegularExpressions;
using OpenTelemetry;
using OpenTelemetry.Trace;

namespace eShop.Ordering.API.Infrastructure.Telemetry;

public class SensitiveDataMaskingProcessor : BaseProcessor<Activity>
{
    private static readonly Regex EmailPattern = new(@"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}", RegexOptions.Compiled);
    private static readonly Regex CreditCardPattern = new(@"\b(?:\d{4}[ -]?){3}\d{4}\b", RegexOptions.Compiled);
    
    private static readonly HashSet<string> SensitiveKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "email",
        "password",
        "creditcard",
        "cardnumber",
        "cvv",
        "ssn",
        "firstname",
        "lastname",
        "address",
        "phonenumber",
        "db.user",
        "db.connection_string"
    };

    public override void OnEnd(Activity activity)
    {
        if (activity != null)
        {
            MaskSensitiveData(activity);
        }
        
        base.OnEnd(activity);
    }

    private void MaskSensitiveData(Activity activity)
    {
        foreach (var tag in activity.Tags.ToList()) 
        {
            if (IsSensitiveKey(tag.Key))
            {
                activity.SetTag(tag.Key, "***REDACTED***");
            }
            else if (!string.IsNullOrEmpty(tag.Value?.ToString()))
            {
                string maskedValue = MaskPatterns(tag.Value.ToString());
                if (maskedValue != tag.Value.ToString())
                {
                    activity.SetTag(tag.Key, maskedValue);
                }
            }
        }
    }

    private bool IsSensitiveKey(string key)
    {
        return SensitiveKeys.Any(k => key.Contains(k, StringComparison.OrdinalIgnoreCase));
    }

    private string MaskPatterns(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        value = EmailPattern.Replace(value, "***EMAIL***");
        value = CreditCardPattern.Replace(value, "***CARD***");

        return value;
    }
}