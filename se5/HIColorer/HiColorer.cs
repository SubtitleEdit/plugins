using System.Text.RegularExpressions;

namespace SubtitleEdit.Plugins.HIColorer;

public enum ColorMode
{
    /// <summary>Color narrators, moods and music.</summary>
    Color,

    /// <summary>Remove every &lt;font&gt; tag.</summary>
    Remove,
}

public sealed class ColorOptions
{
    public const string DefaultNarratorColor = "#ffff00";
    public const string DefaultMoodsColor = "#00ffff";
    public const string DefaultMusicColor = "#ff80ff";

    public bool Narrator { get; set; } = true;
    public string NarratorColor { get; set; } = DefaultNarratorColor;
    public bool Moods { get; set; } = true;
    public string MoodsColor { get; set; } = DefaultMoodsColor;
    public bool Music { get; set; } = true;
    public string MusicColor { get; set; } = DefaultMusicColor;
}

/// <summary>Colors hearing-impaired text: narrator labels, moods in ( ) / [ ] and music lines (♪ ♫).</summary>
public static partial class HiColorer
{
    private const string WriteFormat = "<font color=\"{0}\">{1}</font>";

    [GeneratedRegex(@"<\s*/?\s*font(?:\s[^>]*)?>", RegexOptions.IgnoreCase)]
    private static partial Regex FontTagRegex();

    [GeneratedRegex(@"<[^>]*>|\{[^}]*\}")]
    private static partial Regex AnyTagRegex();

    [GeneratedRegex(@"\b(?:by|https?)\b", RegexOptions.IgnoreCase)]
    private static partial Regex NarratorIgnoreRegex();

    public static string Apply(string text, ColorMode mode, ColorOptions options) =>
        mode == ColorMode.Remove ? RemoveColors(text) : Paint(text, options);

    public static string RemoveColors(string text) =>
        text.Contains("<font", StringComparison.OrdinalIgnoreCase) ? FontTagRegex().Replace(text, string.Empty) : text;

    /// <summary>
    /// Colors the text. Old &lt;font&gt; tags are removed first, but only on lines that get colored -
    /// a text with nothing to color is returned as it is.
    /// </summary>
    public static string Paint(string text, ColorOptions options)
    {
        var plain = RemoveColors(text);
        var painted = plain;
        if (options.Moods)
        {
            painted = PaintMoods(painted, options.MoodsColor);
        }

        if (options.Narrator)
        {
            painted = PaintNarrators(painted, options.NarratorColor);
        }

        if (options.Music)
        {
            painted = PaintMusic(painted, options.MusicColor);
        }

        return painted == plain ? text : painted;
    }

    private static string ApplyColor(string color, string text) => string.Format(WriteFormat, color, text.Trim());

    private static string PaintMoods(string text, string color)
    {
        text = PaintMoods(text, color, '(', ')');
        return PaintMoods(text, color, '[', ']');
    }

    private static string PaintMoods(string text, string color, char openBracket, char closeBracket)
    {
        var idx = text.IndexOf(openBracket);
        while (idx >= 0)
        {
            var endIdx = text.IndexOf(closeBracket, idx + 1);

            // mood not closed properly
            if (endIdx < idx + 1)
            {
                break;
            }

            var mood = text.Substring(idx, endIdx - idx + 1);
            var coloredMood = ApplyColor(color, mood);
            text = text.Remove(idx, mood.Length).Insert(idx, coloredMood);
            idx = text.IndexOf(openBracket, idx + coloredMood.Length);
        }

        return text;
    }

    private static string PaintNarrators(string text, string color)
    {
        if (!text.Contains(':'))
        {
            return text;
        }

        var newLine = text.Contains("\r\n") ? "\r\n" : "\n";
        var lines = text.Replace("\r\n", "\n").Split('\n');
        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            var lineNoTags = AnyTagRegex().Replace(line, string.Empty).TrimEnd('"', '\\').TrimEnd();
            if (!ShouldColorNarrator(lineNoTags, isFirstLine: i == 0))
            {
                continue;
            }

            //- MAN: Baby, I put it right over there.
            var colonIdx = IndexOutsideTags(line, ':');
            var start = FirstWordCharOutsideTags(line, colonIdx);
            var end = LastVisibleCharBefore(line, colonIdx);
            if (start < 0 || end < start)
            {
                continue;
            }

            var narrator = line.Substring(start, end - start + 1);
            lines[i] = line.Substring(0, start) + ApplyColor(color, narrator) + line.Substring(end + 1);
        }

        return string.Join(newLine, lines);
    }

    private static bool ShouldColorNarrator(string textNoTags, bool isFirstLine)
    {
        var colonIdx = textNoTags.IndexOf(':');

        // too short
        if (colonIdx <= 1)
        {
            return false;
        }

        // only "Name: text" or "Name:" at the end of the line - not "Re:start"
        if (colonIdx + 1 < textNoTags.Length && !char.IsWhiteSpace(textNoTags[colonIdx + 1]))
        {
            return false;
        }

        // (foobar) foobar: hello world!
        var pre = textNoTags.Substring(0, colonIdx).TrimStart();
        if (pre.IndexOfAny(new[] { ')', ']' }) >= 0)
        {
            return false;
        }

        // ':' as the last character is only a narrator on the first line
        if (!isFirstLine && colonIdx + 1 == textNoTags.Length)
        {
            return false;
        }

        // e.g. "Corrected by:", "https:"
        return !NarratorIgnoreRegex().IsMatch(pre);
    }

    private static string PaintMusic(string text, string color)
    {
        // skip any formatting tag e.g: html or assa
        var i = 0;
        while (i < text.Length && (text[i] == '<' || text[i] == '{'))
        {
            var close = text.IndexOf(text[i] == '<' ? '>' : '}', i + 1);
            if (close < 0)
            {
                return text;
            }

            i = close + 1;
        }

        // the whole text already has a color (e.g. both mood and music in the same text)
        var isMusic = i < text.Length && (text[i] == '♪' || text[i] == '♫');
        return isMusic && !text.StartsWith("<font", StringComparison.OrdinalIgnoreCase) ? ApplyColor(color, text) : text;
    }

    private static int IndexOutsideTags(string text, char ch)
    {
        for (var i = 0; i < text.Length; i++)
        {
            if (TrySkipTag(text, ref i))
            {
                continue;
            }

            if (text[i] == ch)
            {
                return i;
            }
        }

        return -1;
    }

    private static int FirstWordCharOutsideTags(string text, int before)
    {
        for (var i = 0; i < before; i++)
        {
            if (TrySkipTag(text, ref i))
            {
                continue;
            }

            if (char.IsLetterOrDigit(text[i]))
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>The last character before <paramref name="index"/> that is not a tag or a space, so closing tags stay outside the color.</summary>
    private static int LastVisibleCharBefore(string text, int index)
    {
        var i = index - 1;
        while (i >= 0)
        {
            if (text[i] == '>' || text[i] == '}')
            {
                var open = text.LastIndexOf(text[i] == '>' ? '<' : '{', i);
                if (open < 0)
                {
                    return i;
                }

                i = open - 1;
                continue;
            }

            if (!char.IsWhiteSpace(text[i]))
            {
                return i;
            }

            i--;
        }

        return -1;
    }

    private static bool TrySkipTag(string text, ref int i)
    {
        if (text[i] != '<' && text[i] != '{')
        {
            return false;
        }

        var close = text.IndexOf(text[i] == '<' ? '>' : '}', i + 1);
        if (close < 0)
        {
            return false;
        }

        i = close;
        return true;
    }
}
