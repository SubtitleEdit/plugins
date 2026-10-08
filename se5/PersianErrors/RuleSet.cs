using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace SubtitleEdit.Plugins.PersianErrors;

/// <summary>One find/replace rule from <c>multiple_replace.xml</c>.</summary>
public sealed class ReplaceRule
{
    public ReplaceRule(string findWhat, string replaceWith, Regex? regex, string description)
    {
        FindWhat = findWhat;
        ReplaceWith = replaceWith;
        Regex = regex;
        Description = description;
    }

    public string FindWhat { get; }
    public string ReplaceWith { get; }

    /// <summary>Null for a plain (ordinal) replace.</summary>
    public Regex? Regex { get; }

    public string Description { get; }

    public string Apply(string text)
    {
        if (Regex == null)
        {
            return text.Replace(FindWhat, ReplaceWith, StringComparison.Ordinal);
        }

        try
        {
            return Regex.Replace(text, ReplaceWith);
        }
        catch (RegexMatchTimeoutException exception)
        {
            Debug.WriteLine($"Persian errors: regex timed out ({exception.Pattern})");
            return text;
        }
    }
}

/// <summary>A named group of rules, shown as one option in the UI ("Fix Wrong Chars", "OCR", ...).</summary>
public sealed class RuleGroup
{
    public RuleGroup(string name, IReadOnlyList<ReplaceRule> rules)
    {
        Name = name;
        Rules = rules;
    }

    public string Name { get; }
    public IReadOnlyList<ReplaceRule> Rules { get; }

    public string Apply(string text)
    {
        foreach (var rule in Rules)
        {
            text = rule.Apply(text);
        }

        return text;
    }
}

/// <summary>
/// Loads the rules of the original Persian Subtitle Fixes plugin by Mohammad Sasan (msasanmh).
/// The rules were written against Windows text, so they expect <c>\r\n</c> line breaks; see
/// <see cref="PersianFixer"/> for how text is normalized around them.
/// </summary>
public static class RuleSet
{
    public const string NewLine = "\r\n";
    private static readonly TimeSpan MatchTimeout = TimeSpan.FromSeconds(2);
    private static IReadOnlyList<RuleGroup>? _default;

    /// <summary>The embedded rule groups, in file order (the order they are applied in).</summary>
    public static IReadOnlyList<RuleGroup> Default => _default ??= Load(ReadEmbeddedXml());

    public static IReadOnlyList<RuleGroup> Load(string xml)
    {
        var groups = new List<RuleGroup>();
        var compiled = new Dictionary<string, Regex>(StringComparer.Ordinal);
        var root = XDocument.Parse(xml).Root;
        if (root == null)
        {
            return groups;
        }

        foreach (var group in root.Descendants("Group"))
        {
            var name = group.Element("Name")?.Value.Trim() ?? string.Empty;
            var rules = new List<ReplaceRule>();
            foreach (var item in group.Elements("MultipleSearchAndReplaceItem"))
            {
                if (!string.Equals(item.Element("Enabled")?.Value.Trim(), "True", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var searchType = item.Element("SearchType")?.Value.Trim();
                var isRegex = searchType == "RegularExpression";
                if (!isRegex && searchType != "Normal")
                {
                    continue; // "CaseSensitive" entries are disabled section separators
                }

                var findWhat = FindWhatRule(item.Element("FindWhat")?.Value ?? string.Empty, isRegex);
                var replaceWith = ReplaceWithRule(item.Element("ReplaceWith")?.Value ?? string.Empty);
                if (findWhat.Length == 0)
                {
                    continue;
                }

                Regex? regex = null;
                if (isRegex && !compiled.TryGetValue(findWhat, out regex))
                {
                    regex = new Regex(findWhat, RegexOptions.Multiline, MatchTimeout);
                    compiled.Add(findWhat, regex);
                }

                rules.Add(new ReplaceRule(findWhat, replaceWith, regex, item.Element("Description")?.Value ?? string.Empty));
            }

            if (name.Length > 0)
            {
                groups.Add(new RuleGroup(name, rules));
            }
        }

        return groups;
    }

    // Same escaping as the original plugin's FindWhatRule/ReplaceWithRule, except that quotes are
    // only escaped in regex patterns: escaping them in plain rules made '"' + ZWNJ never match.
    private static string FindWhatRule(string findWhat, bool isRegex)
    {
        if (isRegex)
        {
            findWhat = findWhat.Replace("\"", "\\\"").Replace("\\\\\"", "\\\"");
        }

        return findWhat.Replace("\\r\\n", NewLine).Replace("\\n", NewLine);
    }

    private static string ReplaceWithRule(string replaceWith) => replaceWith
        .Replace("\\r\\n", NewLine)
        .Replace("\\n", NewLine);

    private static string ReadEmbeddedXml()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var name = assembly.GetManifestResourceNames().Single(n => n.EndsWith("multiple_replace.xml", StringComparison.Ordinal));
        using var stream = assembly.GetManifestResourceStream(name)!;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
