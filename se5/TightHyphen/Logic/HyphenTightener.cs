using System.Text.RegularExpressions;

namespace SubtitleEdit.Plugins.TightHyphen.Logic;

/// <summary>Removes the space after a dialog dash at the start of a line: "- Hi!" becomes "-Hi!".</summary>
public static partial class HyphenTightener
{
    [GeneratedRegex(@"<\s*(?:/\s*)?(?:i|b|u|p|font)(?:\s[^>]*)?>|\{[^}]*\}", RegexOptions.IgnoreCase)]
    private static partial Regex TagRegex();

    public static string Tighten(string text)
    {
        var newLine = text.Contains("\r\n") ? "\r\n" : "\n";
        var lines = text.Replace("\r\n", "\n").Split('\n');
        var changed = false;
        for (var i = 0; i < lines.Length; i++)
        {
            var tightened = TightenLine(lines[i]);
            if (tightened != lines[i])
            {
                lines[i] = tightened;
                changed = true;
            }
        }

        return changed ? string.Join(newLine, lines) : text;
    }

    private static string TightenLine(string line)
    {
        // The dash must start the visible text: "- Hi", "<i>- Hi", "{\an8}- Hi".
        var noTags = TagRegex().Replace(line, string.Empty);
        if (!noTags.StartsWith("- ", StringComparison.Ordinal))
        {
            return line;
        }

        var idx = FindLeadingDash(line);
        if (idx < 0)
        {
            return line;
        }

        var rest = line.Substring(idx + 1).TrimStart(' ');

        // "- 5 dollars" would turn into a negative number.
        if (rest.Length == 0 || char.IsDigit(rest[0]))
        {
            return line;
        }

        return line.Substring(0, idx + 1) + rest;
    }

    /// <summary>Index of the dash after any leading tags.</summary>
    private static int FindLeadingDash(string line)
    {
        var i = 0;
        while (i < line.Length)
        {
            var match = TagRegex().Match(line, i);
            if (match.Success && match.Index == i)
            {
                i += match.Length;
                continue;
            }

            return line[i] == '-' ? i : -1;
        }

        return -1;
    }
}
