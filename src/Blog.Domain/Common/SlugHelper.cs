namespace Blog.Domain.Common;

using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

public static partial class SlugHelper
{
    [GeneratedRegex(@"[^a-z0-9\s-]", RegexOptions.Compiled)]
    private static partial Regex DisallowedCharactersRegex();

    [GeneratedRegex(@"[\s-]+", RegexOptions.Compiled)]
    private static partial Regex WhitespaceAndHyphensRegex();

    public static string Generate(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var normalizedString = text.Trim().Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();

        foreach (var c in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        var cleanText = stringBuilder.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
        cleanText = DisallowedCharactersRegex().Replace(cleanText, "");
        cleanText = WhitespaceAndHyphensRegex().Replace(cleanText, "-");

        return cleanText.Trim('-');
    }
}
