using System.Globalization;
using System.Text.RegularExpressions;
using SubtitleEdit.Plugins.Shared;

namespace SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;

public static class SubtitleLayout
{
    private static readonly HashSet<string> Attached = new(StringComparer.OrdinalIgnoreCase)
    {
        "o", "a", "os", "as", "um", "uma", "uns", "umas", "de", "do", "da", "dos", "das", "em", "no", "na", "nos", "nas",
        "ao", "à", "aos", "às", "para", "por", "com", "pelo", "pela", "me", "te", "se", "lhe", "lhes", "vos",
    };
    private static readonly HashSet<string> Auxiliaries = new(StringComparer.OrdinalIgnoreCase)
    {
        "vou", "vais", "vai", "vamos", "vão", "posso", "podes", "pode", "podemos", "podem", "devo", "deves", "deve",
        "devemos", "devem", "quero", "queres", "quer", "queremos", "querem",
    };

    public static string Rewrap(string text, ConversionOptions options)
    {
        if (!options.Rewrap || !options.RuleEnabled("rewrap") || string.IsNullOrWhiteSpace(text)) return text;
        // Formatting and dialogue turns remain verbatim; warnings still apply to their visible text.
        if (Regex.IsMatch(text, @"[<>{}]|(?m)^\s*[-–—]|\\[Nnh]")) return text;
        var words = Regex.Split(text.Trim(), @"\s+");
        if (words.Length > 160) return text;
        var normalized = string.Join(" ", words);
        if (normalized.Length <= options.LineLength) return normalized;
        var cost = new double[options.MaxLines + 1, words.Length + 1];
        var back = new int[options.MaxLines + 1, words.Length + 1];
        for (var l = 0; l <= options.MaxLines; l++)
            for (var j = 0; j <= words.Length; j++) cost[l, j] = double.PositiveInfinity;
        cost[0, 0] = 0;
        for (var line = 1; line <= options.MaxLines; line++)
        {
            for (var end = 1; end <= words.Length; end++)
            {
                var length = 0;
                for (var start = end - 1; start >= 0; start--)
                {
                    length += words[start].Length + (start < end - 1 ? 1 : 0);
                    if (length > options.LineLength) break;
                    var candidate = cost[line - 1, start] + Math.Pow(options.LineLength - length, 2) +
                                    (end < words.Length ? BreakPenalty(words[end - 1], words[end]) : 0);
                    if (candidate >= cost[line, end]) continue;
                    cost[line, end] = candidate;
                    back[line, end] = start;
                }
            }
        }
        var count = Enumerable.Range(1, options.MaxLines).OrderBy(l => cost[l, words.Length]).First();
        if (double.IsPositiveInfinity(cost[count, words.Length])) return text;
        var output = new List<string>();
        var position = words.Length;
        for (var line = count; line > 0; line--)
        {
            var start = back[line, position];
            output.Insert(0, string.Join(" ", words[start..position]));
            position = start;
        }
        return string.Join(text.Contains("\r\n") ? "\r\n" : "\n", output);
    }

    private static double BreakPenalty(string left, string right)
    {
        var word = left.TrimEnd(',', ';', ':', '.', '!', '?');
        if (left.EndsWith(',') || left.EndsWith(';') || left.EndsWith(':')) return 0;
        if (Attached.Contains(word) || Auxiliaries.Contains(word) && PortugueseVerbs.IsKnownInfinitive(right)) return 10000;
        if (char.IsUpper(left[0]) && char.IsUpper(right[0])) return 3000;
        return 50;
    }

    public static string? Review(string text, SrtBlock block, ConversionOptions options)
    {
        if (!options.LayoutWarnings) return null;
        var plain = System.Net.WebUtility.HtmlDecode(Regex.Replace(text, @"<[^<>]*>|\{[^{}]*\}", ""));
        plain = Regex.Replace(plain, @"\\[Nn]", "\n");
        var lines = plain.Replace("\r\n", "\n").Split('\n');
        var notes = new List<string>();
        if (lines.Any(l => new StringInfo(l).LengthInTextElements > options.LineLength))
            notes.Add($"Layout: a line exceeds {options.LineLength} visible characters.");
        if (lines.Length > options.MaxLines) notes.Add($"Layout: more than {options.MaxLines} lines.");
        for (var i = 0; i + 1 < lines.Length; i++)
        {
            var left = Regex.Matches(lines[i], @"\S+").LastOrDefault()?.Value;
            var right = Regex.Matches(lines[i + 1], @"\S+").FirstOrDefault()?.Value;
            if (left != null && right != null && BreakPenalty(left, right) >= 3000)
                notes.Add("Layout: a break may separate an article/clitic, auxiliary/infinitive, or name.");
        }
        var duration = block.EndMs - block.StartMs;
        if (duration <= 0) notes.Add("Layout: cue duration is zero or negative; reading speed cannot be measured.");
        else
        {
            var count = new StringInfo(string.Join(" ", lines)).LengthInTextElements;
            var speed = count * 1000.0 / duration;
            if (speed > options.CharactersPerSecond)
                notes.Add($"Layout: {speed.ToString("0.0", CultureInfo.InvariantCulture)} characters/second exceeds {options.CharactersPerSecond}.");
        }
        return notes.Count == 0 ? null : string.Join("\n", notes);
    }
}
