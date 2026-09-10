using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace AIChatLab.Helpers;

public static class Slug
{
    public static string From(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return "";
        var normalized = input.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var c in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark) continue;
            sb.Append(c);
        }
        var s = sb.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
        s = Regex.Replace(s, @"[^a-z0-9]+", "-");
        s = Regex.Replace(s, @"^-+|-+$", "");
        return s.Length > 48 ? s[..48] : s;
    }

    public static bool IsValid(string? slug) =>
        Regex.IsMatch(slug ?? "", @"^[a-z0-9-]{3,48}$") && slug![0] != '-' && slug[^1] != '-';
}
