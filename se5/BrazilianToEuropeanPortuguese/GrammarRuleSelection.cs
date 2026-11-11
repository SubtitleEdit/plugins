using System.Text.RegularExpressions;

namespace SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;

internal static class GrammarRuleSelection
{
    internal static readonly (string Id, string Label)[] Groups =
    {
        ("grammar", "Grammar and agreement"),
        ("spelling", "Spelling (including names and unknown words)"),
        ("typography", "Punctuation, dialogue dashes, spacing and typography"),
        ("style", "Style, wordiness and register"),
        ("other", "Other / unclassified checks"),
    };

    internal static string[] Ids(string value) =>
        value.Split(new[] { ',', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();

    internal static string Validate(string value, bool groups = false)
    {
        var ids = Ids(value);
        foreach (var id in ids)
            if (groups ? !Groups.Any(g => g.Id == id) : !Regex.IsMatch(id, @"^[A-Za-z0-9_][A-Za-z0-9_.:-]*$", RegexOptions.CultureInvariant))
                throw new InvalidDataException($"Invalid LanguageTool {(groups ? "group" : "rule ID")}: {id}.");
        return string.Join(",", ids);
    }

    internal static string Group(GrammarMatch match)
    {
        if (match.IssueType == "misspelling" || match.Category == "TYPOS") return "spelling";
        if (match.IssueType is "style" or "register" || match.Category is "STYLE" or "REDUNDANCY") return "style";
        if (match.IssueType is "typographical" or "whitespace" or "formatting" or "characters" or "duplication" ||
            match.Category is "PUNCTUATION" or "TYPOGRAPHY" or "CASING") return "typography";
        return match.IssueType == "grammar" || match.Category == "GRAMMAR" ? "grammar" : "other";
    }

    internal static bool Enabled(GrammarMatch match, ConversionOptions options) =>
        !Ids(options.GrammarDisabledRules).Contains(match.Rule, StringComparer.Ordinal) &&
        !Ids(options.GrammarDisabledGroups).Contains(Group(match), StringComparer.Ordinal);

    internal static string[] ObservedIds(IEnumerable<string?> notes) => notes.Where(n => n != null)
        .SelectMany(n => Regex.Matches(n!, @"LanguageTool \[([A-Za-z0-9_][A-Za-z0-9_.:-]*)\]:")
            .Select(m => m.Groups[1].Value)).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();
}
