using System.Text;
using System.Text.RegularExpressions;

namespace SubtitleEdit.Plugins.ItalicEachLine;

public enum ItalicMode
{
    /// <summary><c>&lt;i&gt;Line one</c> / <c>Line two&lt;/i&gt;</c> - one tag pair spans the line break.</summary>
    OneTag,

    /// <summary><c>&lt;i&gt;Line one&lt;/i&gt;</c> / <c>&lt;i&gt;Line two&lt;/i&gt;</c> - every line opens and closes its own tag.</summary>
    EachLine,
}

/// <summary>Moves italic tags between "one tag per subtitle" and "a tag on each line".</summary>
public static class ItalicConverter
{
    // "</i>" + line break(s) + "<i>": the seam between two italic lines. Blank lines in between are allowed,
    // as EachLine never tags a blank line.
    private static readonly Regex Seam = new(@"</i>((?:[ \t]*\r?\n)+[ \t]*)<i>", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private static readonly Regex TagName = new(@"^</?\s*([a-zA-Z]+)", RegexOptions.CultureInvariant);

    public static string Convert(string text, ItalicMode mode) =>
        mode == ItalicMode.EachLine ? ToEachLine(text) : ToOneTag(text);

    /// <summary>Joins italic lines into one tag pair: <c>&lt;i&gt;A&lt;/i&gt;\n&lt;i&gt;B&lt;/i&gt;</c> → <c>&lt;i&gt;A\nB&lt;/i&gt;</c>.</summary>
    public static string ToOneTag(string text)
    {
        if (string.IsNullOrEmpty(text) || text.IndexOf('\n') < 0 || text.IndexOf("</i>", StringComparison.OrdinalIgnoreCase) < 0)
        {
            return text;
        }

        return Seam.Replace(text, m => m.Groups[1].Value);
    }

    /// <summary>
    /// Closes italic at the end of each line and reopens it on the next: <c>&lt;i&gt;A\nB&lt;/i&gt;</c> →
    /// <c>&lt;i&gt;A&lt;/i&gt;\n&lt;i&gt;B&lt;/i&gt;</c>. Tags opened inside the italic (<c>&lt;b&gt;</c>, <c>&lt;font&gt;</c>, ...)
    /// are closed and reopened with it so the tags stay nested. Blank lines are left untagged.
    /// </summary>
    public static string ToEachLine(string text)
    {
        if (string.IsNullOrEmpty(text) || text.IndexOf('\n') < 0 || text.IndexOf("<i>", StringComparison.OrdinalIgnoreCase) < 0)
        {
            return text;
        }

        var newLine = text.Contains("\r\n", StringComparison.Ordinal) ? "\r\n" : "\n";
        var lines = text.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
        var italic = false;
        var inner = new List<(string Name, string Tag)>(); // tags opened inside the italic, still open
        var result = new StringBuilder(text.Length + 16);
        for (var lineIndex = 0; lineIndex < lines.Length; lineIndex++)
        {
            var line = lines[lineIndex];
            if (lineIndex > 0)
            {
                result.Append(newLine);
            }

            if (line.Trim().Length == 0)
            {
                result.Append(line);
                continue;
            }

            var sb = new StringBuilder();
            if (italic)
            {
                sb.Append("<i>");
                foreach (var tag in inner)
                {
                    sb.Append(tag.Tag);
                }
            }

            for (var i = 0; i < line.Length; i++)
            {
                var end = line[i] == '<' ? line.IndexOf('>', i) : -1;
                if (end < 0)
                {
                    sb.Append(line[i]);
                    continue;
                }

                var tag = line.Substring(i, end - i + 1);
                i = end;
                if (tag.Equals("<i>", StringComparison.OrdinalIgnoreCase))
                {
                    if (!italic)
                    {
                        italic = true;
                        inner.Clear();
                    }

                    sb.Append("<i>");
                }
                else if (tag.Equals("</i>", StringComparison.OrdinalIgnoreCase))
                {
                    italic = false;
                    inner.Clear();
                    sb.Append("</i>");
                }
                else
                {
                    if (italic && TagName.Match(tag) is { Success: true } name)
                    {
                        if (tag[1] == '/')
                        {
                            var openIndex = inner.FindLastIndex(t => t.Name.Equals(name.Groups[1].Value, StringComparison.OrdinalIgnoreCase));
                            if (openIndex >= 0)
                            {
                                inner.RemoveAt(openIndex);
                            }
                        }
                        else if (!tag.EndsWith("/>", StringComparison.Ordinal))
                        {
                            inner.Add((name.Groups[1].Value.ToLowerInvariant(), tag));
                        }
                    }

                    sb.Append(tag);
                }
            }

            if (italic)
            {
                for (var t = inner.Count - 1; t >= 0; t--)
                {
                    sb.Append("</").Append(inner[t].Name).Append('>');
                }

                sb.Append("</i>");
            }

            result.Append(RemoveEmptyItalic(sb.ToString()));
        }

        return result.ToString();
    }

    /// <summary>Drops the <c>&lt;i&gt;&lt;/i&gt;</c> left when italic starts or ends right at a line break.</summary>
    private static string RemoveEmptyItalic(string line) =>
        line.Replace("<i></i>", string.Empty, StringComparison.Ordinal);
}
