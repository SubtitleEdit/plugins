using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace SubtitleEdit.Plugins.AmericanToBritish;

/// <summary>
/// Converts US English spellings to UK English using the bundled WordList.xml
/// (~1000 word pairs). Each pair becomes three case-aware regexes: lowercase,
/// UPPERCASE, and Titlecase, all matched as whole words.
/// </summary>
public sealed class AmericanToBritishConverter
{
    private readonly List<(Regex Pattern, string Replacement)> _rules = new();

    public AmericanToBritishConverter()
    {
        LoadBuiltInWordList();
    }

    public int RuleCount => _rules.Count;

    public string Convert(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return text;
        }

        foreach (var (pattern, replacement) in _rules)
        {
            if (pattern.IsMatch(text))
            {
                text = pattern.Replace(text, replacement);
            }
        }

        return RevertColourInFontTags(text);
    }

    /// <summary>
    /// Try converting <paramref name="text"/>; if it changed, return the new text via <paramref name="converted"/>.
    /// </summary>
    public bool TryConvert(string text, out string converted)
    {
        converted = Convert(text);
        return !string.Equals(text, converted, StringComparison.Ordinal);
    }

    private void LoadBuiltInWordList()
    {
        using var stream = typeof(AmericanToBritishConverter).Assembly
            .GetManifestResourceStream("SubtitleEdit.Plugins.AmericanToBritish.WordList.xml")
            ?? throw new InvalidOperationException("Embedded WordList.xml not found.");

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

            AddRule(us, br);
            AddRule(us.ToUpperInvariant(), br.ToUpperInvariant());
            AddRule(char.ToUpperInvariant(us[0]) + us.Substring(1), char.ToUpperInvariant(br[0]) + br.Substring(1));
        }
    }

    private void AddRule(string american, string british)
    {
        _rules.Add((new Regex("\\b" + Regex.Escape(american) + "\\b", RegexOptions.ExplicitCapture | RegexOptions.Compiled), british));
    }

    /// <summary>
    /// "color" inside &lt;font ... color="..."&gt; is HTML attribute syntax and must not be Britishized;
    /// the word-list conversion would otherwise corrupt the tag. Undo "colour" back to "color" inside &lt;font ...&gt;.
    /// </summary>
    private static string RevertColourInFontTags(string s)
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
