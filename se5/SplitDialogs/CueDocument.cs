using SubtitleEdit.Plugins.Shared;
using System.Globalization;
using System.Text.RegularExpressions;

namespace SubtitleEdit.Plugins.SplitDialogs;

/// <summary>One subtitle line as Subtitle Edit numbers it (<see cref="Index"/> matches SelectedIndices).</summary>
public sealed class Cue
{
    public required int Index { get; init; }
    public required long StartMs { get; init; }
    public required long EndMs { get; init; }
    public required IReadOnlyList<string> Lines { get; init; }

    /// <summary>False for lines that must stay as they are (ASSA comments, unreadable time codes).</summary>
    public bool CanSplit { get; init; } = true;
}

/// <summary>
/// The subtitle the plugin edits. Subtitle Edit rebuilds its rows from the returned subtitle,
/// so an ASSA/SSA file answered with SubRip would lose every style and actor - those are
/// edited in their native format, everything else as SubRip.
/// </summary>
public abstract class CueDocument
{
    public abstract string FormatName { get; }

    public abstract IReadOnlyList<Cue> Cues { get; }

    /// <summary>The subtitle with each cue in <paramref name="splits"/> replaced by its parts.</summary>
    public abstract string Build(IReadOnlyDictionary<int, IReadOnlyList<SplitPart>> splits);

    public static CueDocument Create(PluginSubtitle subtitle)
    {
        if ((string.Equals(subtitle.Format, AssaCueDocument.AdvancedSubStationAlpha, StringComparison.OrdinalIgnoreCase) ||
             string.Equals(subtitle.Format, AssaCueDocument.SubStationAlpha, StringComparison.OrdinalIgnoreCase)) &&
            AssaCueDocument.TryParse(subtitle.Native, subtitle.Format, out var assa))
        {
            return assa;
        }

        return new SrtCueDocument(subtitle.SubRip);
    }
}

public sealed class SrtCueDocument : CueDocument
{
    private static readonly Regex TimeLine = new(
        @"^\s*(\d+):(\d{1,2}):(\d{1,2})[,.](\d{1,3})\s*-->\s*(\d+):(\d{1,2}):(\d{1,2})[,.](\d{1,3})",
        RegexOptions.Compiled);

    private readonly List<SrtBlock> _blocks;
    private readonly List<Cue> _cues;

    public SrtCueDocument(string srt)
    {
        _blocks = Parse(srt);
        _cues = _blocks.Select((b, i) => new Cue
        {
            Index = i,
            StartMs = b.StartMs,
            EndMs = b.EndMs,
            Lines = b.Text.Split('\n'),
        }).ToList();
    }

    public override string FormatName => "SubRip";

    public override IReadOnlyList<Cue> Cues => _cues;

    public override string Build(IReadOnlyDictionary<int, IReadOnlyList<SplitPart>> splits)
    {
        var blocks = new List<SrtBlock>(_blocks.Count + splits.Count);
        for (var i = 0; i < _blocks.Count; i++)
        {
            if (splits.TryGetValue(i, out var parts))
            {
                blocks.AddRange(parts.Select(p => new SrtBlock { StartMs = p.StartMs, EndMs = p.EndMs, Text = p.Text }));
            }
            else
            {
                blocks.Add(_blocks[i]);
            }
        }

        return SubRipParser.Serialize(blocks);
    }

    // Line based rather than SubRipParser.Parse's split on blank lines: SE writes a line with no
    // text as "N / time / blank / blank", which a blank-line split drops along with the line after
    // it - and a dropped line would shift every SelectedIndices entry below it.
    private static List<SrtBlock> Parse(string srt)
    {
        var blocks = new List<SrtBlock>();
        var lines = (srt ?? string.Empty).Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
        SrtBlock? current = null;
        var text = new List<string>();

        for (var i = 0; i < lines.Length; i++)
        {
            if (i + 1 < lines.Length &&
                (current == null || (i > 0 && lines[i - 1].Trim().Length == 0)) &&
                long.TryParse(lines[i].Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out _))
            {
                var match = TimeLine.Match(lines[i + 1]);
                if (match.Success)
                {
                    Flush();
                    current = new SrtBlock { StartMs = ToMs(match, 1), EndMs = ToMs(match, 5) };
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
        return blocks;

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
            blocks.Add(current);
            text.Clear();
        }
    }

    private static long ToMs(Match match, int firstGroup)
    {
        var h = long.Parse(match.Groups[firstGroup].Value, CultureInfo.InvariantCulture);
        var m = long.Parse(match.Groups[firstGroup + 1].Value, CultureInfo.InvariantCulture);
        var s = long.Parse(match.Groups[firstGroup + 2].Value, CultureInfo.InvariantCulture);
        var fraction = match.Groups[firstGroup + 3].Value;
        var ms = long.Parse(fraction.PadRight(3, '0'), CultureInfo.InvariantCulture);
        return ((h * 60 + m) * 60 + s) * 1000 + ms;
    }
}

public sealed class AssaCueDocument : CueDocument
{
    public const string AdvancedSubStationAlpha = "Advanced Sub Station Alpha";
    public const string SubStationAlpha = "Sub Station Alpha";

    private static readonly Regex TimeCode = new(@"^\s*(\d+):(\d{1,2}):(\d{1,2})[.:](\d{1,3})\s*$", RegexOptions.Compiled);
    private static readonly Regex LineBreak = new(@"\\[Nn]", RegexOptions.Compiled);

    private readonly string _formatName;
    private readonly string[] _lines;
    private readonly string _newLine;
    private readonly List<Cue> _cues = new();
    private readonly Dictionary<int, EventLine> _eventsByCue = new();

    private sealed record EventLine(int LineIndex, string Key, string[] Fields, int StartField, int EndField);

    private AssaCueDocument(string formatName, string[] lines, string newLine)
    {
        _formatName = formatName;
        _lines = lines;
        _newLine = newLine;
    }

    public override string FormatName => _formatName;

    public override IReadOnlyList<Cue> Cues => _cues;

    public static bool TryParse(string native, string formatName, out AssaCueDocument document)
    {
        var newLine = native.Contains("\r\n") ? "\r\n" : "\n";
        document = new AssaCueDocument(formatName, native.Replace("\r\n", "\n").Split('\n'), newLine);
        return document.ParseEvents();
    }

    private bool ParseEvents()
    {
        var inEvents = false;
        string[]? format = null;
        var cueIndex = 0;

        for (var i = 0; i < _lines.Length; i++)
        {
            var trimmed = _lines[i].Trim();
            if (trimmed.StartsWith('['))
            {
                inEvents = trimmed.Equals("[Events]", StringComparison.OrdinalIgnoreCase);
                continue;
            }

            if (!inEvents)
            {
                continue;
            }

            if (trimmed.StartsWith("Format:", StringComparison.OrdinalIgnoreCase))
            {
                format = trimmed.Substring(7).Split(',').Select(f => f.Trim()).ToArray();
                continue;
            }

            var colon = _lines[i].IndexOf(':');
            if (colon < 0)
            {
                continue;
            }

            var key = _lines[i].Substring(0, colon);
            var isDialogue = key.Trim().Equals("Dialogue", StringComparison.OrdinalIgnoreCase);
            var isComment = key.Trim().Equals("Comment", StringComparison.OrdinalIgnoreCase);
            if (!isDialogue && !isComment)
            {
                continue;
            }

            if (format == null)
            {
                return false;
            }

            var textField = Array.FindIndex(format, f => f.Equals("Text", StringComparison.OrdinalIgnoreCase));
            var startField = Array.FindIndex(format, f => f.Equals("Start", StringComparison.OrdinalIgnoreCase));
            var endField = Array.FindIndex(format, f => f.Equals("End", StringComparison.OrdinalIgnoreCase));
            if (textField != format.Length - 1 || startField < 0 || endField < 0)
            {
                return false;
            }

            // The text is the last field and may itself contain commas.
            var fields = _lines[i].Substring(colon + 1).Split(',', textField + 1);
            var index = cueIndex++;
            var canSplit = isDialogue && fields.Length == textField + 1;
            long startMs = 0;
            long endMs = 0;
            canSplit = canSplit && TryParseTime(fields[startField], out startMs) && TryParseTime(fields[endField], out endMs);

            _cues.Add(new Cue
            {
                Index = index,
                StartMs = startMs,
                EndMs = endMs,
                Lines = LineBreak.Split(fields[^1]),
                CanSplit = canSplit,
            });

            if (canSplit)
            {
                _eventsByCue[index] = new EventLine(i, key, fields, startField, endField);
            }
        }

        return true;
    }

    public override string Build(IReadOnlyDictionary<int, IReadOnlyList<SplitPart>> splits)
    {
        var replacements = new Dictionary<int, IReadOnlyList<SplitPart>>();
        var events = new Dictionary<int, EventLine>();
        foreach (var (cueIndex, parts) in splits)
        {
            if (_eventsByCue.TryGetValue(cueIndex, out var eventLine))
            {
                replacements[eventLine.LineIndex] = parts;
                events[eventLine.LineIndex] = eventLine;
            }
        }

        var output = new List<string>(_lines.Length + splits.Count);
        for (var i = 0; i < _lines.Length; i++)
        {
            if (!replacements.TryGetValue(i, out var parts))
            {
                output.Add(_lines[i]);
                continue;
            }

            var eventLine = events[i];
            foreach (var part in parts)
            {
                var fields = (string[])eventLine.Fields.Clone();
                fields[eventLine.StartField] = KeepLeadingSpace(fields[eventLine.StartField], FormatTime(part.StartMs));
                fields[eventLine.EndField] = KeepLeadingSpace(fields[eventLine.EndField], FormatTime(part.EndMs));
                fields[^1] = part.Text.Replace("\n", "\\N");
                output.Add(eventLine.Key + ":" + string.Join(",", fields));
            }
        }

        return string.Join(_newLine, output);
    }

    private static string KeepLeadingSpace(string original, string value) =>
        original.Substring(0, original.Length - original.TrimStart().Length) + value;

    private static bool TryParseTime(string text, out long ms)
    {
        ms = 0;
        var match = TimeCode.Match(text);
        if (!match.Success)
        {
            return false;
        }

        var h = long.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
        var m = long.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
        var s = long.Parse(match.Groups[3].Value, CultureInfo.InvariantCulture);
        var fraction = match.Groups[4].Value;
        var fractionMs = long.Parse(fraction.PadRight(3, '0'), CultureInfo.InvariantCulture);
        ms = ((h * 60 + m) * 60 + s) * 1000 + fractionMs;
        return true;
    }

    // ASSA time codes are in centiseconds: "0:00:01.23".
    private static string FormatTime(long ms)
    {
        var cs = (long)Math.Round(Math.Max(0, ms) / 10.0, MidpointRounding.AwayFromZero);
        return string.Create(CultureInfo.InvariantCulture, $"{cs / 360_000}:{cs / 6_000 % 60:00}:{cs / 100 % 60:00}.{cs % 100:00}");
    }
}
