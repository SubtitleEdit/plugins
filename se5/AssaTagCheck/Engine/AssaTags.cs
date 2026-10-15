using System.Globalization;
using System.Text.RegularExpressions;

namespace SubtitleEdit.Plugins.AssaTagCheck;

public enum TagKind
{
    /// <summary>A number: \fs20, \bord2, \frz-5, ...</summary>
    Number,

    /// <summary>A whole number with a fixed set of values: \an1-9, \a, \q, \p.</summary>
    Choice,

    /// <summary>A color: \c&amp;HBBGGRR&amp;</summary>
    Color,

    /// <summary>An alpha value: \alpha&amp;HAA&amp;</summary>
    Alpha,

    /// <summary>Free text: \fnArial, \rStyle.</summary>
    Text,

    /// <summary>Numbers in parentheses: \pos(x,y), \move(...), \fad(in,out), \clip(...).</summary>
    Parenthesis,

    /// <summary>\t(...) - an animation that holds other tags.</summary>
    Transform,
}

/// <summary>How one override tag is written and what it accepts.</summary>
public sealed record TagSpec(string Name, TagKind Kind, bool AllowEmpty, int[]? ArgumentCounts = null, int[]? Choices = null);

/// <summary>The override tags libass and VSFilter know, and how to read their values.</summary>
public static class AssaTags
{
    private static readonly TagSpec[] Specs =
    {
        new("alpha", TagKind.Alpha, true),
        new("1a", TagKind.Alpha, true),
        new("2a", TagKind.Alpha, true),
        new("3a", TagKind.Alpha, true),
        new("4a", TagKind.Alpha, true),
        new("c", TagKind.Color, true),
        new("1c", TagKind.Color, true),
        new("2c", TagKind.Color, true),
        new("3c", TagKind.Color, true),
        new("4c", TagKind.Color, true),
        new("an", TagKind.Choice, false, Choices: new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }),
        new("a", TagKind.Choice, false, Choices: new[] { 1, 2, 3, 5, 6, 7, 9, 10, 11 }),
        new("q", TagKind.Choice, true, Choices: new[] { 0, 1, 2, 3 }),
        new("p", TagKind.Number, false),
        new("pbo", TagKind.Number, false),
        new("b", TagKind.Number, true),
        new("i", TagKind.Number, true),
        new("u", TagKind.Number, true),
        new("s", TagKind.Number, true),
        new("be", TagKind.Number, true),
        new("blur", TagKind.Number, true),
        new("bord", TagKind.Number, true),
        new("xbord", TagKind.Number, true),
        new("ybord", TagKind.Number, true),
        new("shad", TagKind.Number, true),
        new("xshad", TagKind.Number, true),
        new("yshad", TagKind.Number, true),
        new("fs", TagKind.Number, true),
        new("fscx", TagKind.Number, true),
        new("fscy", TagKind.Number, true),
        new("fsp", TagKind.Number, true),
        new("fr", TagKind.Number, true),
        new("frx", TagKind.Number, true),
        new("fry", TagKind.Number, true),
        new("frz", TagKind.Number, true),
        new("fax", TagKind.Number, true),
        new("fay", TagKind.Number, true),
        new("fe", TagKind.Number, true),
        new("k", TagKind.Number, false),
        new("K", TagKind.Number, false),
        new("kf", TagKind.Number, false),
        new("ko", TagKind.Number, false),
        new("kt", TagKind.Number, false),
        new("fn", TagKind.Text, true),
        new("r", TagKind.Text, true),
        new("pos", TagKind.Parenthesis, false, new[] { 2 }),
        new("org", TagKind.Parenthesis, false, new[] { 2 }),
        new("move", TagKind.Parenthesis, false, new[] { 4, 6 }),
        new("fad", TagKind.Parenthesis, false, new[] { 2 }),
        new("fade", TagKind.Parenthesis, false, new[] { 7 }),
        new("clip", TagKind.Parenthesis, false, new[] { 4 }),
        new("iclip", TagKind.Parenthesis, false, new[] { 4 }),
        new("t", TagKind.Transform, false),
    };

    private static readonly Dictionary<string, TagSpec> ByName = Specs.ToDictionary(s => s.Name, StringComparer.Ordinal);

    // Longest first, so \fscx is not read as \fs + "cx" and \be not as \b + "e".
    private static readonly string[] NamesLongestFirst = Specs.Select(s => s.Name).OrderByDescending(n => n.Length).ToArray();

    private static readonly Regex NumberRegex = new(@"^\s*[-+]?(\d+(\.\d*)?|\.\d+)\s*$", RegexOptions.Compiled);
    private static readonly Regex HexRegex = new(@"^\s*&?[Hh]?([0-9A-Fa-f]{1,8})&*\s*$", RegexOptions.Compiled);

    public static TagSpec? Spec(string name) => ByName.GetValueOrDefault(name);

    /// <summary>The known tag name <paramref name="text"/> starts with at <paramref name="index"/> (case-sensitive), or null.</summary>
    public static string? MatchName(string text, int index)
    {
        foreach (var name in NamesLongestFirst)
        {
            if (string.CompareOrdinal(text, index, name, 0, name.Length) == 0 && index + name.Length <= text.Length)
            {
                return name;
            }
        }

        return null;
    }

    public static bool IsNumber(string value) => NumberRegex.IsMatch(value);

    public static bool TryParseNumber(string value, out double number) =>
        double.TryParse(value.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out number) && IsNumber(value);

    /// <summary>Reads &amp;HBBGGRR&amp; the way libass does: optional &amp; and H, then hex.</summary>
    public static bool TryParseHex(string value, out uint number)
    {
        number = 0;
        var match = HexRegex.Match(value);
        return match.Success && uint.TryParse(match.Groups[1].Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out number);
    }

    public static string FormatNumber(double value) => value.ToString("0.###", CultureInfo.InvariantCulture);
}
