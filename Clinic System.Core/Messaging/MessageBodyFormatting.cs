using System.Net;
using System.Text.RegularExpressions;

namespace Clinic_System.Core.Messaging;

public static class MessageBodyFormatting
{
    private static readonly Regex HtmlTag = new(@"</?[a-zA-Z][^>]*>", RegexOptions.Compiled);
    private static readonly Regex ScriptLike = new(
        @"<(script|style|iframe|object|embed|form)[\s\S]*?</\1\s*>",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex EventAttr = new(
        @"\son\w+\s*=\s*(""[^""]*""|'[^']*'|[^\s>]+)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex DisallowedTag = new(
        @"</?(?!p|br|div|span|strong|b|em|i|u|s|strike|del|ul|ol|li|a|blockquote|h[1-6])[a-zA-Z][^>]*>",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static bool LooksLikeHtml(string? value) =>
        !string.IsNullOrWhiteSpace(value) && HtmlTag.IsMatch(value);

    public static bool IsBlank(string? value) =>
        string.IsNullOrWhiteSpace(StripToPlainText(value));

    public static string StripToPlainText(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return string.Empty;

        var text = Regex.Replace(html, @"<(br|BR)\s*/?>", "\n");
        text = Regex.Replace(text, @"</(p|div|li|h[1-6]|blockquote)\s*>", "\n", RegexOptions.IgnoreCase);
        text = HtmlTag.Replace(text, string.Empty);
        return WebUtility.HtmlDecode(text).Trim();
    }

    public static string Sanitize(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return string.Empty;
        if (!LooksLikeHtml(html))
            return html.Trim();

        var clean = ScriptLike.Replace(html, string.Empty);
        clean = EventAttr.Replace(clean, string.Empty);
        clean = Regex.Replace(clean, @"javascript\s*:", string.Empty, RegexOptions.IgnoreCase);
        clean = DisallowedTag.Replace(clean, string.Empty);
        return clean.Trim();
    }

    public static string ToPreviewHtml(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        if (LooksLikeHtml(value))
            return Sanitize(value);

        return WebUtility.HtmlEncode(value)
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace("\n", "<br>\n", StringComparison.Ordinal);
    }

    public static string ToWhatsApp(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;
        if (!LooksLikeHtml(value))
            return value.Trim();

        var text = value;
        text = Regex.Replace(text, @"<(br|BR)\s*/?>", "\n");
        text = Regex.Replace(text, @"</(p|div|h[1-6]|blockquote|ul|ol)\s*>", "\n", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"<li[^>]*>", "• ", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"</li\s*>", "\n", RegexOptions.IgnoreCase);
        text = Regex.Replace(
            text,
            @"<a\s[^>]*href\s*=\s*[""']([^""']+)[""'][^>]*>(.*?)</a>",
            match =>
            {
                var inner = StripToPlainText(match.Groups[2].Value);
                var href = match.Groups[1].Value.Trim();
                if (string.IsNullOrWhiteSpace(inner) || inner.Equals(href, StringComparison.OrdinalIgnoreCase))
                    return href;
                return $"{inner} ({href})";
            },
            RegexOptions.IgnoreCase | RegexOptions.Singleline);
        text = WrapMarkers(text, ["strong", "b"], "*");
        text = WrapMarkers(text, ["em", "i"], "_");
        text = WrapMarkers(text, ["s", "strike", "del"], "~");
        text = HtmlTag.Replace(text, string.Empty);
        text = WebUtility.HtmlDecode(text);
        text = Regex.Replace(text, @"[ \t]+\n", "\n");
        text = Regex.Replace(text, @"\n{3,}", "\n\n");
        return text.Trim();
    }

    public static string ApplyTokens(string template, IReadOnlyDictionary<string, string> tokens)
    {
        var result = template ?? string.Empty;
        var encode = LooksLikeHtml(result);
        foreach (var (key, value) in tokens)
        {
            var replacement = encode
                ? WebUtility.HtmlEncode(value ?? string.Empty)
                : value ?? string.Empty;
            result = result.Replace(key, replacement, StringComparison.OrdinalIgnoreCase);
        }

        return result;
    }

    private static string WrapMarkers(string html, string[] tags, string marker)
    {
        foreach (var tag in tags)
        {
            html = Regex.Replace(
                html,
                $@"<{tag}(?:\s[^>]*)?>(.*?)</{tag}\s*>",
                match => marker + match.Groups[1].Value + marker,
                RegexOptions.IgnoreCase | RegexOptions.Singleline);
        }

        return html;
    }
}
