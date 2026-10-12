using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
/// (~1850 pairs), optionally extended/filtered by a <see cref="LocalWordList"/>.
/// Each pair becomes three case-aware regexes: lowercase, UPPERCASE, and Titlecase,
/// all matched as whole words. Picks direction via <see cref="EnglishVariantDirection"/>.
/// Keep phrases (built-in &lt;Keep text="..."/&gt; plus the local list's ignored entries) are left
/// untouched, e.g. the Spanish "por favor" must not become "por favour".
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

    private readonly List<string> _keepPhrases = new();
    private Regex? _keepRegex;

    /// <summary>Private use area chars stand in for kept phrases while the rules run.</summary>
    private const char PlaceholderFirst = '\uE000';
    private const char PlaceholderLast = '\uF8FF';

    public EnglishVariantConverter(EnglishVariantDirection direction, LocalWordList? localWordList = null)
    {
        _direction = direction;

        var ignored = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (localWordList != null)
        {
            // Local pairs first so a user phrase wins over a built-in word inside it, and a
            // local pair for a built-in word replaces the built-in one.
            foreach (var (us, br) in localWordList.Words)
            {
                if (ignored.Add(us))
                {
                    AddPair(us, br);
                }
            }

            foreach (var us in localWordList.Ignored)
            {
                ignored.Add(us);
                _keepPhrases.Add(us);
            }
        }

        LoadBuiltInWordList(ignored);

        if (_keepPhrases.Count > 0)
        {
            // Longest first so the longer of two overlapping phrases wins.
            var alternatives = _keepPhrases
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderByDescending(p => p.Length)
                .Select(p => Regex.Escape(p).Replace("\\ ", "\\s+"));
            _keepRegex = new Regex("\\b(?:" + string.Join("|", alternatives) + ")\\b", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
        }
    }

    public int RuleCount => _rules.Count;

    public string Convert(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return text;
        }

        var kept = MaskKeepPhrases(ref text);

        var candidates = CollectCandidateRules(text);
        foreach (var index in candidates)
        {
            var (pattern, replacement) = _rules[index];
            text = pattern.Replace(text, replacement);
        }

        text = UnmaskKeepPhrases(text, kept);
        return RevertFontColorAttribute(text);
    }

    /// <summary>
    /// Replaces each kept phrase with one private use char (not a word char, so no rule can
    /// match it or across it). Skipped for text that already contains such chars.
    /// </summary>
    private List<string>? MaskKeepPhrases(ref string text)
    {
        if (_keepRegex == null || ContainsPlaceholderChar(text))
        {
            return null;
        }

        var kept = new List<string>();
        text = _keepRegex.Replace(text, m =>
        {
            if (kept.Count > PlaceholderLast - PlaceholderFirst)
            {
                return m.Value;
            }

            kept.Add(m.Value);
            return ((char)(PlaceholderFirst + kept.Count - 1)).ToString();
        });

        return kept.Count > 0 ? kept : null;
    }

    private static string UnmaskKeepPhrases(string text, List<string>? kept)
    {
        if (kept == null)
        {
            return text;
        }

        var sb = new StringBuilder(text.Length + 16);
        foreach (var c in text)
        {
            var index = c - PlaceholderFirst;
            if (index >= 0 && index < kept.Count)
            {
                sb.Append(kept[index]);
            }
            else
            {
                sb.Append(c);
            }
        }

        return sb.ToString();
    }

    private static bool ContainsPlaceholderChar(string text)
    {
        foreach (var c in text)
        {
            if (c >= PlaceholderFirst && c <= PlaceholderLast)
            {
                return true;
            }
        }

        return false;
    }

    public bool TryConvert(string text, out string converted)
    {
        converted = Convert(text);
        return !string.Equals(text, converted, StringComparison.Ordinal);
    }

    private void LoadBuiltInWordList(HashSet<string> ignored)
    {
        using var stream = typeof(EnglishVariantConverter).Assembly
            .GetManifestResourceStream("SubtitleEdit.Plugins.Shared.WordList.xml")
            ?? throw new InvalidOperationException("Embedded WordList.xml not found in Plugin-Shared.");

        var xml = XDocument.Load(stream);
        if (xml.Root?.Name != "Words")
        {
            return;
        }

        foreach (var element in xml.Root.Elements("Keep"))
        {
            var keep = element.Attribute("text")?.Value.Trim();
            if (!string.IsNullOrEmpty(keep))
            {
                _keepPhrases.Add(keep!);
            }
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

            if (ignored.Contains(us!))
            {
                continue;
            }

            AddPair(us!, br!);
        }
    }

    private void AddPair(string us, string br)
    {
        var (from, to) = _direction == EnglishVariantDirection.UsToBr ? (us, br) : (br, us);

        AddRule(from, to);
        AddRule(from.ToUpperInvariant(), to.ToUpperInvariant());
        AddRule(char.ToUpperInvariant(from[0]) + from.Substring(1), char.ToUpperInvariant(to[0]) + to.Substring(1));
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
