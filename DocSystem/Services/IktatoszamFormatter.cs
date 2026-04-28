using System.Globalization;
using System.Text.RegularExpressions;
using DocSystem.Models;

namespace DocSystem.Services;

public partial class IktatoszamFormatter(IConfiguration configuration)
{
    private const string DefaultFormat = "{foszam}/{alszam}-{kod}/{evszam}";

    public string Format(Iktatoszam iktatoszam)
    {
        var format = configuration["IktatoszamFormat"];
        if (string.IsNullOrWhiteSpace(format))
            format = DefaultFormat;

        return TokenRegex().Replace(format, match =>
        {
            var token = match.Groups["token"].Value.ToLowerInvariant();
            var customFormat = match.Groups["format"].Success
                ? match.Groups["format"].Value
                : null;

            return token switch
            {
                "foszam" => FormatValue(iktatoszam.Foszam, customFormat),
                "alszam" => FormatValue(iktatoszam.Alszam, customFormat),
                "kod" => iktatoszam.Iktatokonyv.Kod,
                "evszam" => FormatValue(iktatoszam.Iktatokonyv.Evszam, customFormat),
                _ => match.Value
            };
        });
    }

    private static string FormatValue<T>(T value, string? format) where T : IFormattable
    {
        return string.IsNullOrWhiteSpace(format)
            ? value.ToString(null, CultureInfo.InvariantCulture)
            : value.ToString(format, CultureInfo.InvariantCulture);
    }

    [GeneratedRegex(@"\{(?<token>foszam|alszam|kod|evszam)(:(?<format>[^}]+))?\}", RegexOptions.IgnoreCase)]
    private static partial Regex TokenRegex();
}
