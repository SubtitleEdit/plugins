using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace SubtitleEdit.Plugins.TypewriterEffect;

public sealed class SrtBlock
{
    public long StartMs { get; set; }
    public long EndMs { get; set; }
    public string Text { get; set; } = string.Empty;
}

public static class SubRipParser
{
    // Subtitle Edit writes more than two hour digits past 99 hours and a leading '-' for negative times.
    private static readonly Regex TimeLine = new(
        @"^\s*(-?)(\d+):(\d{1,2}):(\d{1,2})[,.](\d{1,3})\s*-->\s*(-?)(\d+):(\d{1,2}):(\d{1,2})[,.](\d{1,3})",
        RegexOptions.Compiled);

    // Line based rather than split on blank lines: Subtitle Edit writes a line with no text as
    // "N / time / blank / blank", which a blank-line split drops along with the line after it -
    // and every dropped line deletes it from the user's subtitle and shifts SelectedIndices below it.
    public static List<SrtBlock> Parse(string srt)
    {
        var result = new List<SrtBlock>();
        if (string.IsNullOrWhiteSpace(srt))
        {
            return result;
        }

        var lines = srt.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
        SrtBlock? current = null;
        var text = new List<string>();

        for (var i = 0; i < lines.Length; i++)
        {
            // A block starts at a number followed by a time code line - at the start or after a blank line.
            if (i + 1 < lines.Length &&
                (current == null || lines[i - 1].Trim().Length == 0) &&
                long.TryParse(lines[i].Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out _))
            {
                var timeMatch = TimeLine.Match(lines[i + 1]);
                if (timeMatch.Success)
                {
                    Flush();
                    current = new SrtBlock { StartMs = ToMs(timeMatch, 1), EndMs = ToMs(timeMatch, 6) };
                    i++;
                    continue;
                }
            }

            if (current != null)
            {
                text.Add(lines[i]);
            }
        }

        Flush();
        return result;

        void Flush()
        {
            if (current == null)
            {
                return;
            }

            while (text.Count > 0 && text[^1].Trim().Length == 0)
            {
                text.RemoveAt(text.Count - 1);
            }

            current.Text = string.Join("\n", text);
            result.Add(current);
            text.Clear();
        }
    }

    public static string Serialize(IList<SrtBlock> blocks)
    {
        var sb = new StringBuilder();
        for (var i = 0; i < blocks.Count; i++)
        {
            sb.Append(i + 1).Append('\n');
            sb.Append(FormatTime(blocks[i].StartMs)).Append(" --> ").Append(FormatTime(blocks[i].EndMs)).Append('\n');
            sb.Append(blocks[i].Text).Append('\n').Append('\n');
        }

        return sb.ToString().Replace("\n", "\r\n");
    }

    // firstGroup is the sign group, followed by hours, minutes, seconds and milliseconds.
    private static long ToMs(Match m, int firstGroup)
    {
        var h = long.Parse(m.Groups[firstGroup + 1].Value, CultureInfo.InvariantCulture);
        var mn = long.Parse(m.Groups[firstGroup + 2].Value, CultureInfo.InvariantCulture);
        var s = long.Parse(m.Groups[firstGroup + 3].Value, CultureInfo.InvariantCulture);
        var ms = long.Parse(m.Groups[firstGroup + 4].Value.PadRight(3, '0'), CultureInfo.InvariantCulture);
        var total = ((h * 60 + mn) * 60 + s) * 1000 + ms;
        return m.Groups[firstGroup].Length > 0 ? -total : total;
    }

    private static string FormatTime(long ms)
    {
        if (ms < 0)
        {
            ms = 0;
        }
        var h = ms / 3_600_000;
        var mn = (ms / 60_000) % 60;
        var s = (ms / 1000) % 60;
        var msr = ms % 1000;
        return $"{h:D2}:{mn:D2}:{s:D2},{msr:D3}";
    }
}
