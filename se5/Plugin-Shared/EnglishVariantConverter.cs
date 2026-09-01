using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace SubtitleEdit.Plugins.Shared;

public enum EnglishVariantDirection
{
    UsToBr,
    BrToUs,
}

/// <summary>
/// Converts between American and British English using the bundled WordList.xml
/// (~1850 pairs). Each pair becomes three case-aware regexes: lowercase,
/// UPPERCASE, and Titlecase, all matched as whole words. Picks direction via
/// <see cref="EnglishVariantDirection"/>.
/// </summary>
public sealed class EnglishVariantConverter
{
    private readonly List<(Regex Pattern, string Replacement)> _rules = new();

    /// <summary>
    /// Rule indexes keyed by the first word of the pattern, so a line only pays for the
    /// handful of rules whose word it actually contains instead of all ~5500.
    /// </summary>
    private readonly Dictionary<string, List<int>> _rulesByFirstWord = new(StringComparer.Ordinal);

    private readonly EnglishVariantDirection _direction;

    public EnglishVariantConverter(EnglishVariantDirection direction)
    {
        _direction = direction;
        LoadBuiltInWordList();
    }

    public int RuleCount => _rules.Count;

    public string Convert(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return text;
        }

        var candidates = CollectCandidateRules(text);
        foreach (var index in candidates)
        {
            var (pattern, replacement) = _rules[index];
            text = pattern.Replace(text, replacement);
        }

        return RevertFontColorAttribute(text);
    }

    public bool TryConvert(string text, out string converted)
    {
        converted = Convert(text);
        return !string.Equals(text, converted, StringComparison.Ordinal);
    }

    private void LoadBuiltInWordList()
    {
        using var stream = typeof(EnglishVariantConverter).Assembly
            .GetManifestResourceStream("SubtitleEdit.Plugins.Shared.WordList.xml")
            ?? throw new InvalidOperationException("Embedded WordList.xml not found in Plugin-Shared.");

        var xml = XDocument.Load(stream);
        if (xml.Root?.Name != "Words")
        {
            return;
        }

        foreach (var element in xml.Root.Elements("Word"))
        {
            var us = element.Attribute("us")?.Value;
            var br = element.Attribute("br")?.Value;
            if (string.IsNullOrEmpty(us) || string.IsNullOrEmpty(br) || us!.Length < 2 || br!.Length < 2)
            {
                continue;
            }

            // only="UsToBr" / only="BrToUs" restricts a pair to one direction. Used for
            // vocabulary pairs whose "source" word is also standard in the target dialect
            // (car, film, flat, lift...) - converting those would corrupt correct text.
            var only = element.Attribute("only")?.Value;
            if (only != null && !only.Equals(_direction.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var (from, to) = _direction == EnglishVariantDirection.UsToBr ? (us, br) : (br, us);

            AddRule(from, to);
            AddRule(from.ToUpperInvariant(), to.ToUpperInvariant());
            AddRule(char.ToUpperInvariant(from[0]) + from.Substring(1), char.ToUpperInvariant(to[0]) + to.Substring(1));
        }
    }

    private void AddRule(string from, string to)
    {
        var index = _rules.Count;
        _rules.Add((new Regex("\\b" + Regex.Escape(from) + "\\b", RegexOptions.ExplicitCapture), to));

        var firstWord = FirstWord(from);
        if (!_rulesByFirstWord.TryGetValue(firstWord, out var indexes))
        {
            indexes = new List<int>();
            _rulesByFirstWord.Add(firstWord, indexes);
        }

        indexes.Add(index);
    }

    /// <summary>
    /// Rules whose first word occurs in <paramref name="text"/>, in rule order. A pattern
    /// starts with "\b", so its first word can only match a whole word of the input - a line
    /// without that word can never match the pattern and does not need to run it.
    /// </summary>
    private SortedSet<int> CollectCandidateRules(string text)
    {
        var candidates = new SortedSet<int>();
        var i = 0;
        while (i < text.Length)
        {
            if (!IsWordChar(text[i]))
            {
                i++;
                continue;
            }

            var start = i;
            while (i < text.Length && IsWordChar(text[i]))
            {
                i++;
            }

            if (_rulesByFirstWord.TryGetValue(text.Substring(start, i - start), out var indexes))
            {
                foreach (var index in indexes)
                {
                    candidates.Add(index);
                }
            }
        }

        return candidates;
    }

    private static string FirstWord(string s)
    {
        var start = 0;
        while (start < s.Length && !IsWordChar(s[start]))
        {
            start++;
        }

        var end = start;
        while (end < s.Length && IsWordChar(s[end]))
        {
            end++;
        }

        return s.Substring(start, end - start);
    }

    /// <summary>Matches what "\w" (and therefore "\b") considers a word character.</summary>
    private static bool IsWordChar(char c) => char.IsLetterOrDigit(c) || c == '_';

    /// <summary>
    /// "color" inside &lt;font color="..."&gt; is HTML attribute syntax and must not be Britishized
    /// by the word-list pass — that would corrupt the tag. Undo "colour" back to "color" inside
    /// &lt;font ...&gt;. No-op for BR→US.
    /// </summary>
    private static string RevertFontColorAttribute(string s)
    {
        var tagIndex = s.IndexOf("<font", StringComparison.OrdinalIgnoreCase);
        while (tagIndex >= 0)
        {
            var tagEndIndex = s.IndexOf('>', tagIndex + 5);
            if (tagEndIndex < 0)
            {
                break;
            }

            var tag = s.Substring(tagIndex, tagEndIndex - tagIndex);
            var colourIndex = tag.IndexOf("colour", StringComparison.OrdinalIgnoreCase);
            while (colourIndex >= 0)
            {
                tag = tag.Remove(colourIndex + 4, 1);
                colourIndex = tag.IndexOf("colour", colourIndex + 5, StringComparison.OrdinalIgnoreCase);
            }
            s = s.Remove(tagIndex, tagEndIndex - tagIndex).Insert(tagIndex, tag);
            tagIndex = s.IndexOf("<font", tagIndex + tag.Length + 1, StringComparison.OrdinalIgnoreCase);
        }
        return s;
    }
}
