using System.Text.RegularExpressions;

namespace SubtitleEdit.Plugins.HI2UC;

/// <summary>Text helpers carried over from the SE4 plugin's shared code (HtmlUtils, StringUtils, StringExtensions).</summary>
internal static class TextUtils
{
    private static readonly string[] HtmlTags = { "i", "b", "u", "p", "font", "\u0456" };
    private static readonly Regex TagRegex = new(@"<\s*(?:/\s*)?(\w+)[^>]*>", RegexOptions.Compiled);

    public static bool IsOpenTag(char ch) => ch == '<' || ch == '{';

    public static char GetClosingPair(char ch) => ch == '<' ? '>' : '}';

    /// <summary>Removes &lt;i&gt;, &lt;b&gt;, &lt;u&gt;, &lt;font&gt; ... tags, and optionally ASSA {...} tags.</summary>
    public static string RemoveTags(string s, bool alsoSsaTags = false)
    {
        if (s.Length < 3)
        {
            return s;
        }

        if (alsoSsaTags)
        {
            s = RemoveSsaTags(s);
        }

        return s.Contains('<')
            ? TagRegex.Replace(s, m => HtmlTags.Contains(m.Groups[1].Value, StringComparer.OrdinalIgnoreCase) ? string.Empty : m.Value)
            : s;
    }

    public static string RemoveSsaTags(string s)
    {
        var idx = s.IndexOf('{');
        while (idx >= 0)
        {
            var endIdx = s.IndexOf('}', idx + 1);
            if (endIdx < 0)
            {
                break;
            }

            s = s.Remove(idx, endIdx - idx + 1);
            idx = s.IndexOf('{', idx);
        }

        return s;
    }

    public static string[] SplitToLines(this string source) =>
        source.Replace("\r\r\n", "\n").Replace("\r\n", "\n").Replace('\r', '\n').Replace('\u2028', '\n').Split('\n');

    /// <summary>The line break used in the text, so lines are joined back the same way.</summary>
    public static string DetectNewLine(string text) => text.Contains("\r\n") ? "\r\n" : "\n";

    public static bool ContainsAny(this string s, char[] chars) => s.IndexOfAny(chars) >= 0;
}
