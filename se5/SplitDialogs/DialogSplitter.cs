using System.Text;
using System.Text.RegularExpressions;

namespace SubtitleEdit.Plugins.SplitDialogs;

/// <summary>
/// Detects dialog subtitles ("- Hi!" / "- Hello.") and splits them into one text per speaker.
/// Detection follows Subtitle Edit's DialogSplitMerge.IsDialog: a line starting with a dash
/// starts a new speaker when the line before it ends a sentence. Unlike SE's check it is not
/// limited to two or three lines, so a subtitle with three speakers splits into three.
/// </summary>
public static class DialogSplitter
{
    // '-' and U+2010 are what SE's dialog helper knows; en dash and em dash are the dialog
    // marks of e.g. Scandinavian and Spanish subtitles.
    private static readonly char[] DashChars = ['-', '‐', '–', '—'];

    private static readonly Regex TagRegex = new(@"<[^<>]*>|\{[^{}]*\}", RegexOptions.Compiled);
    private static readonly Regex HtmlTagRegex = new(@"<(/?)(i|b|u|font)(\s[^<>]*)?>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex AssaOverrideRegex = new(@"\{\\[^{}]*\}", RegexOptions.Compiled);

    /// <summary>
    /// Splits <paramref name="lines"/> into one text per speaker, or returns null when they are
    /// not a dialog. Each text has its dash removed, its lines joined with '\n', and the
    /// formatting that was open at the split re-opened, so every part keeps its styling.
    /// </summary>
    public static List<string>? Split(IReadOnlyList<string> lines, bool requireSentenceEnding)
    {
        if (lines.Count < 2)
        {
            return null;
        }

        var speakers = new List<List<string>>();
        var current = new List<string>();
        for (var i = 0; i < lines.Count; i++)
        {
            var line = lines[i].Trim();
            if (i > 0 && StartsWithDash(line) &&
                (!requireSentenceEnding || HasSentenceEnding(StripTags(lines[i - 1]).TrimEnd())))
            {
                speakers.Add(current);
                current = new List<string>();
            }

            current.Add(line);
        }

        speakers.Add(current);
        if (speakers.Count < 2)
        {
            return null;
        }

        var texts = new List<string>(speakers.Count);
        foreach (var speaker in speakers)
        {
            speaker[0] = RemoveStartDash(speaker[0]);
            var text = string.Join("\n", speaker);
            if (StripTags(text).Trim().Length == 0)
            {
                return null; // "- Hi!" / "-" is not two speakers
            }

            texts.Add(text);
        }

        return CarryFormatting(texts);
    }

    public static string StripTags(string text) => TagRegex.Replace(text, string.Empty);

    /// <summary>Port of SE's StringExtensions.HasSentenceEnding for tag-free text, language neutral.</summary>
    internal static bool HasSentenceEnding(string value)
    {
        var i = value.Length - 1;
        while (i >= 0 && (IsClosingQuote(value[i]) || value[i] == ' ' || value[i] == ' '))
        {
            i--;
        }

        if (i < 0)
        {
            return false;
        }

        var c = value[i];
        if (c == '-')
        {
            return i > 1 && char.IsLetter(value[i - 2]) && value[i - 1] == '-'; // "foobar--"
        }

        if (c == '—')
        {
            return i > 0 && char.IsLetter(value[i - 1]); // "foobar—"
        }

        return ".!?])…♪؟。？".IndexOf(c) >= 0;
    }

    private static bool IsClosingQuote(char c) =>
        c is '"' or '\'' or '“' or '”' or '«' or '»' or '‘' or '’' or '」' or '』';

    private static bool StartsWithDash(string line)
    {
        var s = line.Substring(GetStartTags(line).Length).TrimStart();
        return IsDialogDash(s);
    }

    // A double dash ("--and then") is a continuation mark, not a speaker.
    private static bool IsDialogDash(string s) =>
        s.Length > 0 && Array.IndexOf(DashChars, s[0]) >= 0 && !(s.Length > 1 && s[1] == s[0]);

    private static string RemoveStartDash(string line)
    {
        var startTags = GetStartTags(line);
        var s = line.Substring(startTags.Length).TrimStart();
        return IsDialogDash(s) ? startTags + s.Substring(1).TrimStart() : line;
    }

    private static string GetStartTags(string line)
    {
        var length = 0;
        while (length < line.Length)
        {
            var close = line[length] switch
            {
                '{' => line.IndexOf('}', length),
                '<' => line.IndexOf('>', length),
                _ => -1,
            };

            if (close < 0)
            {
                break;
            }

            length = close + 1;
        }

        return line.Substring(0, length);
    }

    /// <summary>
    /// "&lt;i&gt;- Hi!\n- Hello.&lt;/i&gt;" must become "&lt;i&gt;Hi!&lt;/i&gt;" and "&lt;i&gt;Hello.&lt;/i&gt;": HTML tags
    /// still open at a split are closed there and re-opened in the next part, and ASSA override
    /// blocks ({\an8}, {\i1}, ...) of earlier parts are repeated at the start of later ones.
    /// </summary>
    private static List<string> CarryFormatting(List<string> texts)
    {
        var result = new List<string>(texts.Count);
        var openTags = new List<(string Name, string Tag)>();
        var overrides = new StringBuilder();
        for (var i = 0; i < texts.Count; i++)
        {
            var text = texts[i];
            var prefix = overrides + string.Concat(openTags.Select(t => t.Tag));

            foreach (Match match in HtmlTagRegex.Matches(text))
            {
                var name = match.Groups[2].Value.ToLowerInvariant();
                if (match.Groups[1].Value.Length == 0)
                {
                    openTags.Add((name, match.Value));
                }
                else
                {
                    var index = openTags.FindLastIndex(t => t.Name == name);
                    if (index >= 0)
                    {
                        openTags.RemoveAt(index);
                    }
                }
            }

            foreach (Match match in AssaOverrideRegex.Matches(text))
            {
                overrides.Append(match.Value);
            }

            if (!text.StartsWith(prefix, StringComparison.Ordinal))
            {
                text = prefix + text;
            }

            if (i < texts.Count - 1)
            {
                text += string.Concat(Enumerable.Reverse(openTags).Select(t => "</" + t.Name + ">"));
            }

            result.Add(text);
        }

        return result;
    }
}
