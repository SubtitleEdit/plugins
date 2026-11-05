using System.Text;
using System.Text.RegularExpressions;

namespace SubtitleEdit.Plugins.FrenchTypography.Logic;

public enum FrenchVariant
{
    /// <summary>France, Belgium, Switzerland: a space before ; : ! ?</summary>
    France,

    /// <summary>Canada (Office québécois de la langue française): a space before : only.</summary>
    Canada,
}

public enum FrenchSpace
{
    /// <summary>U+00A0, shown by every font and player.</summary>
    NoBreak,

    /// <summary>U+202F, the narrow no-break space of print typography. Some fonts lack it.</summary>
    Narrow,
}

public sealed record FrenchTypographyOptions
{
    public FrenchVariant Variant { get; init; } = FrenchVariant.France;
    public FrenchSpace Space { get; init; } = FrenchSpace.NoBreak;

    /// <summary>"texte" becomes « texte ».</summary>
    public bool ConvertQuotes { get; init; } = true;

    /// <summary>... becomes …</summary>
    public bool ConvertEllipsis { get; init; } = true;
}

/// <summary>
/// French punctuation spacing: a no-break space before ; : ! ? (only : in Canada) and inside « »,
/// optionally straight quotes to guillemets and three dots to an ellipsis. Tags (&lt;i&gt;, {\an8})
/// are left alone and skipped when looking at the neighbouring text.
/// </summary>
public static partial class FrenchTypographer
{
    public const char NoBreakSpace = ' ';
    public const char NarrowNoBreakSpace = ' ';

    [GeneratedRegex(@"<\s*(?:/\s*)?[a-z][^<>]*>|\{[^{}]*\}", RegexOptions.IgnoreCase)]
    private static partial Regex TagRegex();

    public static string Fix(string text, FrenchTypographyOptions options)
    {
        var newLine = text.Contains("\r\n") ? "\r\n" : "\n";
        var lines = text.Replace("\r\n", "\n").Split('\n');
        var convertQuotes = options.ConvertQuotes && CanConvertQuotes(text);
        var quoteOpen = false;
        var changed = false;
        for (var i = 0; i < lines.Length; i++)
        {
            var fixedLine = FixLine(lines[i], options, convertQuotes, ref quoteOpen);
            if (fixedLine != lines[i])
            {
                lines[i] = fixedLine;
                changed = true;
            }
        }

        return changed ? string.Join(newLine, lines) : text;
    }

    /// <summary>Straight quotes are only paired when the whole paragraph has an even number of them.</summary>
    private static bool CanConvertQuotes(string text)
    {
        var count = TagRegex().Replace(text, string.Empty).Count(c => c == '"');
        return count > 0 && count % 2 == 0;
    }

    private static string FixLine(string line, FrenchTypographyOptions options, bool convertQuotes, ref bool quoteOpen)
    {
        var space = options.Space == FrenchSpace.Narrow ? NarrowNoBreakSpace : NoBreakSpace;

        // Split into tags (kept as-is) and visible characters.
        var tokens = Tokenize(line);
        if (options.ConvertEllipsis)
        {
            ReplaceEllipsis(tokens);
        }

        var visible = tokens.Where(t => !t.IsTag).ToList();
        if (convertQuotes)
        {
            foreach (var token in visible.Where(t => t.Value == "\""))
            {
                token.Value = quoteOpen ? "»" : "«";
                quoteOpen = !quoteOpen;
            }
        }

        for (var i = 0; i < visible.Count; i++)
        {
            var c = visible[i].Value;
            if (c == "»" || IsSpacedPunctuation(c, options.Variant))
            {
                if (c == ":" && IsColonInWord(visible, i))
                {
                    continue;
                }

                SetSpaceBefore(visible, i, space);
            }
            else if (options.Variant == FrenchVariant.Canada && c is "?" or "!" or ";")
            {
                RemoveSpaceBefore(visible, i);
            }
            else if (c == "«")
            {
                SetSpaceAfter(visible, i, space);
            }
        }

        var sb = new StringBuilder(line.Length + 4);
        foreach (var token in tokens)
        {
            if (!token.Removed)
            {
                sb.Append(token.Prefix).Append(token.Value).Append(token.Suffix);
            }
        }

        return sb.ToString();
    }

    private static bool IsSpacedPunctuation(string c, FrenchVariant variant) =>
        c == ":" || (variant == FrenchVariant.France && c is "?" or "!" or ";");

    private static bool IsSpace(string c) => c is " " or " " or " " or " ";

    /// <summary>10:30, http://, 16:9 - a colon with no space on either side is not punctuation.</summary>
    private static bool IsColonInWord(List<Token> visible, int i)
    {
        var before = i > 0 ? visible[i - 1].Value : string.Empty;
        var after = i + 1 < visible.Count ? visible[i + 1].Value : string.Empty;
        return before.Length == 1 && !IsSpace(before) && after.Length == 1 && (char.IsLetterOrDigit(after[0]) || after == "/");
    }

    /// <summary>Marks after which no space is added: another mark, an opening bracket or the start of the line.</summary>
    private static bool NeedsNoSpaceAfter(string c) =>
        c is "?" or "!" or ";" or ":" or "." or "…" or "(" or "[" or "-" or "–" or "—";

    private static void SetSpaceBefore(List<Token> visible, int i, char space)
    {
        var j = i - 1;
        while (j >= 0 && IsSpace(visible[j].Value))
        {
            j--;
        }

        if (j < 0 || NeedsNoSpaceAfter(visible[j].Value) || visible[j].Value == "«")
        {
            return;
        }

        // Exactly one space, right before the mark (after any closing tag such as </i>).
        for (var k = j + 1; k < i; k++)
        {
            visible[k].Removed = true;
        }

        visible[i].Prefix = space.ToString();
    }

    private static void SetSpaceAfter(List<Token> visible, int i, char space)
    {
        var j = i + 1;
        while (j < visible.Count && IsSpace(visible[j].Value))
        {
            j++;
        }

        if (j >= visible.Count || visible[j].Value == "»")
        {
            return;
        }

        for (var k = i + 1; k < j; k++)
        {
            visible[k].Removed = true;
        }

        visible[i].Suffix = space.ToString();
    }

    private static void RemoveSpaceBefore(List<Token> visible, int i)
    {
        var j = i - 1;
        while (j >= 0 && IsSpace(visible[j].Value))
        {
            j--;
        }

        if (j < 0)
        {
            return;
        }

        for (var k = j + 1; k < i; k++)
        {
            visible[k].Removed = true;
        }
    }

    private static void ReplaceEllipsis(List<Token> tokens)
    {
        // Exactly three dots in a row; four or more are left alone.
        for (var i = 0; i + 2 < tokens.Count; i++)
        {
            if (tokens[i].Value == "." && tokens[i + 1].Value == "." && tokens[i + 2].Value == "."
                && (i == 0 || tokens[i - 1].Value != ".") && (i + 3 >= tokens.Count || tokens[i + 3].Value != "."))
            {
                tokens[i].Value = "…";
                tokens.RemoveRange(i + 1, 2);
            }
        }
    }

    private static List<Token> Tokenize(string line)
    {
        var tokens = new List<Token>(line.Length);
        var pos = 0;
        foreach (Match match in TagRegex().Matches(line))
        {
            for (; pos < match.Index; pos++)
            {
                tokens.Add(new Token(line[pos].ToString(), false));
            }

            tokens.Add(new Token(match.Value, true));
            pos = match.Index + match.Length;
        }

        for (; pos < line.Length; pos++)
        {
            tokens.Add(new Token(line[pos].ToString(), false));
        }

        return tokens;
    }

    /// <summary>A tag or one visible character, with the space added before/after it.</summary>
    private sealed class Token(string value, bool isTag)
    {
        public string Value { get; set; } = value;
        public bool IsTag { get; } = isTag;
        public string Prefix { get; set; } = string.Empty;
        public string Suffix { get; set; } = string.Empty;
        public bool Removed { get; set; }
    }
}
