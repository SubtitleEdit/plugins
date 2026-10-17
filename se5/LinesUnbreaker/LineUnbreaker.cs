using System.Text.RegularExpressions;

namespace SubtitleEdit.Plugins.LinesUnbreaker;

public sealed class UnbreakOptions
{
    public const int DefaultMaxLength = 43;

    /// <summary>Join only when the result, without tags, is at most this many characters.</summary>
    public int MaxLength { get; set; } = DefaultMaxLength;
    public bool SkipDialogs { get; set; } = true;
    public bool SkipMoods { get; set; }
    public bool SkipNarrator { get; set; } = true;
}

/// <summary>Joins short multi-line subtitles into one line.</summary>
public static partial class LineUnbreaker
{
    private static readonly string[] JoinedTags = { "</i> <i>", "</b> <b>", "</u> <u>" };

    [GeneratedRegex(@"<\s*(?:/\s*)?(?:i|b|u|p|font)(?:\s[^>]*)?>", RegexOptions.IgnoreCase)]
    private static partial Regex HtmlTagRegex();

    [GeneratedRegex(@"\{[^}]*\}")]
    private static partial Regex AssaTagRegex();

    [GeneratedRegex(" {2,}")]
    private static partial Regex SpacesRegex();

    /// <summary>The text joined to one line, or the text itself when it should stay as it is.</summary>
    public static string Unbreak(string text, UnbreakOptions options)
    {
        var lines = SplitToLines(text).Select(l => new Line(l)).ToArray();
        if (lines.Count(l => l.Content.Trim().Length > 0) < 2)
        {
            return text;
        }

        if ((options.SkipDialogs && lines.Any(l => l.IsDialog)) ||
            (options.SkipMoods && lines.Any(l => l.HasMood)) ||
            (options.SkipNarrator && lines.Any(l => l.HasNarrator)))
        {
            return text;
        }

        var joined = Join(lines.Select(l => l.Content));
        return VisibleLength(joined) <= options.MaxLength ? joined : text;
    }

    /// <summary>The number of characters shown on screen: tags and line breaks are not counted.</summary>
    public static int VisibleLength(string text) =>
        RemoveTags(text).Replace("\r", string.Empty).Replace("\n", string.Empty).Length;

    public static string RemoveTags(string text) => AssaTagRegex().Replace(HtmlTagRegex().Replace(text, string.Empty), string.Empty);

    public static string[] SplitToLines(string text) =>
        text.Replace("\r\r\n", "\n").Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');

    private static string Join(IEnumerable<string> lines)
    {
        var singleLine = SpacesRegex().Replace(string.Join(" ", lines.Select(l => l.Trim()).Where(l => l.Length > 0)), " ");

        // "<i>One</i> <i>two</i>" becomes "<i>One two</i>"
        if (singleLine.Contains("</"))
        {
            foreach (var tag in JoinedTags)
            {
                singleLine = singleLine.Replace(tag, " ");
            }
        }

        return singleLine;
    }
}
