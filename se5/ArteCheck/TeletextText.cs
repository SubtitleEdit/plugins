using System.Text;
using System.Text.RegularExpressions;

namespace SubtitleEdit.Plugins.ArteCheck;

/// <summary>
/// Text helpers for teletext subtitles as Subtitle Edit holds them: lines separated by newlines,
/// colors as &lt;font color="..."&gt;, boxing as &lt;box&gt;, alignment as {\anN}.
/// </summary>
public static partial class TeletextText
{
    [GeneratedRegex(@"<[^>]*>|\{\\[^}]*\}")]
    private static partial Regex TagRegex();

    [GeneratedRegex(@"</?i\s*>", RegexOptions.IgnoreCase)]
    private static partial Regex ItalicTagRegex();

    [GeneratedRegex(@"</?box\b[^>]*>", RegexOptions.IgnoreCase)]
    private static partial Regex BoxTagRegex();

    [GeneratedRegex(@"<font\b[^>]*\bcolor\s*=", RegexOptions.IgnoreCase)]
    private static partial Regex FontColorRegex();

    [GeneratedRegex(@"<(/?)(font|[a-z]+)\b([^>]*)>|\{\\[^}]*\}", RegexOptions.IgnoreCase)]
    private static partial Regex TokenRegex();

    [GeneratedRegex("""\bcolor\s*=\s*(?:"(?<quoted>[^"]+)"|'(?<single>[^']+)'|(?<bare>[^\s>]+))""", RegexOptions.IgnoreCase)]
    private static partial Regex ColorAttributeRegex();

    public static string NormalizeNewLines(string text) => text.Replace("\r\n", "\n").Replace('\r', '\n');

    public static string[] SplitLines(string text) => NormalizeNewLines(text).Split('\n');

    public static int LineCount(string text) => SplitLines(text).Length;

    public static string RemoveTags(string text) => TagRegex().Replace(text, string.Empty);

    public static bool IsBlank(string? text) => string.IsNullOrWhiteSpace(text == null ? null : RemoveTags(text));

    public static int VisibleCharacterCount(string text) => RemoveTags(text).Count(c => c is not '\r' and not '\n');

    public static string RemoveItalic(string text) => ItalicTagRegex().Replace(text, string.Empty);

    public static bool HasItalic(string text) => ItalicTagRegex().IsMatch(text);

    public static string RemoveBox(string text) => BoxTagRegex().Replace(text, string.Empty);

    public static bool HasBox(string text) => BoxTagRegex().IsMatch(text);

    public static bool HasFontColor(string text) => FontColorRegex().IsMatch(text);

    public static Regex ColorAttribute => ColorAttributeRegex();

    public static string Words(string text) => Regex.Replace(RemoveTags(text), @"\s+", " ").Trim();

    /// <summary>
    /// Visible characters plus the teletext color control cells each line needs: every colored run
    /// that touches a line costs one cell on that line (the color code written before it).
    /// </summary>
    public static IReadOnlyList<(string Visible, int ColorCells)> MeasureLines(string text)
    {
        var lines = new List<(string, int)>();
        var colorStack = new Stack<string?>();
        string? currentColor = null;
        var visible = new StringBuilder();
        var runs = 0;
        string? lastRunColor = null;
        var normalized = NormalizeNewLines(RemoveItalic(text));
        var position = 0;

        void CountChar(char c)
        {
            if (c == '\n')
            {
                lines.Add((visible.ToString(), runs));
                visible.Clear();
                runs = 0;
                lastRunColor = null;
                return;
            }

            if (currentColor != null && !string.Equals(currentColor, lastRunColor, StringComparison.OrdinalIgnoreCase))
            {
                runs++;
            }

            lastRunColor = currentColor;
            visible.Append(c);
        }

        foreach (Match match in TokenRegex().Matches(normalized))
        {
            for (; position < match.Index; position++)
            {
                CountChar(normalized[position]);
            }

            position = match.Index + match.Length;
            if (!match.Groups[2].Value.Equals("font", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (match.Groups[1].Value == "/")
            {
                currentColor = colorStack.Count > 0 ? colorStack.Pop() : null;
            }
            else
            {
                colorStack.Push(currentColor);
                var color = ColorAttributeRegex().Match(match.Groups[3].Value);
                if (color.Success)
                {
                    currentColor = color.Groups["quoted"].Success ? color.Groups["quoted"].Value :
                        color.Groups["single"].Success ? color.Groups["single"].Value : color.Groups["bare"].Value;
                }
            }
        }

        for (; position < normalized.Length; position++)
        {
            CountChar(normalized[position]);
        }

        lines.Add((visible.ToString(), runs));
        return lines;
    }

    public static bool Fits(string text, int maxCells, int maxLines = 2)
    {
        var lines = MeasureLines(text);
        return lines.Count <= maxLines && lines.All(l => l.Visible.Length + l.ColorCells <= maxCells);
    }

    /// <summary>
    /// Re-wraps <paramref name="text"/> into one or two lines that fit <paramref name="maxCells"/>,
    /// preferring a balanced break after punctuation. Leading alignment tags stay in front.
    /// Returns null when no break fits.
    /// </summary>
    public static string? Rebalance(string text, int maxCells)
    {
        var (prefix, body) = SplitLeadingAlignment(NormalizeNewLines(text));
        var words = body.Replace('\n', ' ').Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length == 0)
        {
            return null;
        }

        var single = prefix + string.Join(' ', words);
        if (Fits(single, maxCells, 1))
        {
            return single;
        }

        string? best = null;
        var bestScore = double.MaxValue;
        for (var i = 1; i < words.Length; i++)
        {
            var first = string.Join(' ', words.Take(i));
            var second = string.Join(' ', words.Skip(i));
            var candidate = prefix + first + "\n" + second;
            if (!Fits(candidate, maxCells))
            {
                continue;
            }

            var firstVisible = RemoveTags(first).TrimEnd();
            double score = Math.Abs(RemoveTags(first).Length - RemoveTags(second).Length);
            if (firstVisible.EndsWith('.') || firstVisible.EndsWith('!') || firstVisible.EndsWith('?') || firstVisible.EndsWith('…'))
            {
                score -= 25;
            }
            else if (firstVisible.EndsWith(',') || firstVisible.EndsWith(':') || firstVisible.EndsWith(';'))
            {
                score -= 15;
            }

            // Subtitlers break at clause ends when they can; otherwise as balanced as possible.
            // A short first line reads worse than a short second line (pyramid shape).
            if (RemoveTags(first).Length > RemoveTags(second).Length)
            {
                score += 0.5;
            }

            if (score < bestScore)
            {
                bestScore = score;
                best = candidate;
            }
        }

        return best;
    }

    /// <summary>
    /// Splits text that cannot fit two lines into the fewest subtitles that each fit, cutting after
    /// a sentence end where possible. Returns null when no split fits.
    /// </summary>
    public static List<string>? SplitToFit(string text, int maxCells)
    {
        var (prefix, body) = SplitLeadingAlignment(NormalizeNewLines(text));
        var words = body.Replace('\n', ' ').Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var parts = new List<string>();
        var start = 0;
        while (start < words.Length)
        {
            var end = start;
            var lastSentenceEnd = -1;
            while (end < words.Length && Rebalance(string.Join(' ', words[start..(end + 1)]), maxCells) != null)
            {
                var visible = RemoveTags(words[end]);
                if (visible.EndsWith('.') || visible.EndsWith('!') || visible.EndsWith('?') || visible.EndsWith('…'))
                {
                    lastSentenceEnd = end;
                }

                end++;
            }

            if (end == start)
            {
                return null; // a single word that does not fit
            }

            // Prefer ending the part on a sentence when that keeps at least 40% of what would fit.
            if (end < words.Length && lastSentenceEnd >= start && lastSentenceEnd + 1 - start >= (end - start) * 0.4)
            {
                end = lastSentenceEnd + 1;
            }

            var part = Rebalance(string.Join(' ', words[start..end]), maxCells);
            if (part == null)
            {
                return null;
            }

            parts.Add(prefix + part);
            start = end;
        }

        return parts.Count >= 2 ? parts : null;
    }

    private static (string Prefix, string Body) SplitLeadingAlignment(string text)
    {
        var match = Regex.Match(text, @"^(\{\\an\d\})");
        return match.Success ? (match.Value, text[match.Length..]) : (string.Empty, text);
    }

    public static string TrimLines(string text) =>
        string.Join("\n", SplitLines(text).Select(line => line.Trim()));

    /// <summary>Same mapping as Subtitle Edit's Ebu.GetNearestColorName: the teletext color a color is nearest to.</summary>
    public static string? NearestTeletextColor(string color)
    {
        color = color.Trim().TrimStart('#').ToLowerInvariant();
        switch (color)
        {
            case "black" or "000000": return "Black";
            case "red" or "ff0000": return "Red";
            case "green" or "00ff00": return "Green";
            case "yellow" or "ffff00": return "Yellow";
            case "blue" or "0000ff": return "Blue";
            case "magenta" or "ff00ff": return "Magenta";
            case "cyan" or "00ffff": return "Cyan";
            case "white" or "ffffff": return "White";
        }

        if (color.Length != 6 || !Regex.IsMatch(color, "^[0-9a-f]{6}$"))
        {
            return null;
        }

        const int maxDiff = 130;
        var r = Convert.ToInt32(color[..2], 16);
        var g = Convert.ToInt32(color.Substring(2, 2), 16);
        var b = Convert.ToInt32(color.Substring(4, 2), 16);
        bool Low(int v) => v < maxDiff;
        bool High(int v) => v > 255 - maxDiff;
        if (Low(r) && Low(g) && Low(b)) return "Black";
        if (High(r) && Low(g) && Low(b)) return "Red";
        if (Low(r) && High(g) && Low(b)) return "Green";
        if (High(r) && High(g) && Low(b)) return "Yellow";
        if (Low(r) && Low(g) && High(b)) return "Blue";
        if (High(r) && Low(g) && High(b)) return "Magenta";
        if (Low(r) && High(g) && High(b)) return "Cyan";
        if (High(r) && High(g) && High(b)) return "White";
        return null;
    }
}
