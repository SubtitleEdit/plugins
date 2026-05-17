using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace SubtitleEdit.Plugins.Shared;

public sealed class SrtBlock
{
    public long StartMs { get; set; }
    public long EndMs { get; set; }
    public string Text { get; set; } = string.Empty;
}

public static class SubRipParser
{
    private static readonly Regex TimeLine = new(
        @"^(\d{1,2}):(\d{2}):(\d{2})[,.](\d{3})\s*-->\s*(\d{1,2}):(\d{2}):(\d{2})[,.](\d{3})",
        RegexOptions.Compiled);

    public static List<SrtBlock> Parse(string srt)
    {
        var result = new List<SrtBlock>();
        if (string.IsNullOrWhiteSpace(srt))
        {
            return result;
        }

        var normalized = srt.Replace("\r\n", "\n").Trim('\n');
        var rawBlocks = Regex.Split(normalized, @"\n[ \t]*\n");
        foreach (var raw in rawBlocks)
        {
            var lines = raw.Split('\n');
            if (lines.Length < 3)
            {
                continue;
            }

            var timeMatch = TimeLine.Match(lines[1]);
            if (!timeMatch.Success)
            {
                continue;
            }

            var startMs = ToMs(timeMatch, 1);
            var endMs = ToMs(timeMatch, 5);
            var text = string.Join("\n", lines, 2, lines.Length - 2);
            result.Add(new SrtBlock { StartMs = startMs, EndMs = endMs, Text = text });
        }

        return result;
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

    private static long ToMs(Match m, int firstGroup)
    {
        var h = int.Parse(m.Groups[firstGroup].Value, CultureInfo.InvariantCulture);
        var mn = int.Parse(m.Groups[firstGroup + 1].Value, CultureInfo.InvariantCulture);
        var s = int.Parse(m.Groups[firstGroup + 2].Value, CultureInfo.InvariantCulture);
        var ms = int.Parse(m.Groups[firstGroup + 3].Value, CultureInfo.InvariantCulture);
        return ((h * 60L + mn) * 60 + s) * 1000 + ms;
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
