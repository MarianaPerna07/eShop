using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace eShop.Ordering.API.Infrastructure.Logging;

public class SensitiveDataJsonConverter : JsonConverter<string>
{
    private static readonly Regex EmailPattern = new(@"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}", RegexOptions.Compiled);
    private static readonly Regex CreditCardPattern = new(@"\b(?:\d{4}[ -]?){3}\d{4}\b", RegexOptions.Compiled);

    private static readonly HashSet<string> SensitivePropertyNames = new()
    {
        "email", "password", "creditcard", "cardnumber", "cvv", "ssn", "phonenumber", "db.user"
    };

    public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.GetString();
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        if (string.IsNullOrEmpty(value))
        {
            writer.WriteStringValue(value);
            return;
        }

        var maskedValue = MaskSensitivePatterns(value);
        writer.WriteStringValue(maskedValue);
    }

    private string MaskSensitivePatterns(string value)
    {
        value = EmailPattern.Replace(value, "***EMAIL***");
        value = CreditCardPattern.Replace(value, "***CARD***");
        return value;
    }
}
