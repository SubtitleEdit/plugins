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
/// (~1000 pairs). Each pair becomes three case-aware regexes: lowercase,
/// UPPERCASE, and Titlecase, all matched as whole words. Picks direction via
/// <see cref="EnglishVariantDirection"/>.
/// </summary>
public sealed class EnglishVariantConverter
{
    private readonly List<(Regex Pattern, string Replacement)> _rules = new();
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

        foreach (var (pattern, replacement) in _rules)
        {
            if (pattern.IsMatch(text))
            {
                text = pattern.Replace(text, replacement);
            }
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

            var (from, to) = _direction == EnglishVariantDirection.UsToBr ? (us, br) : (br, us);

            AddRule(from, to);
            AddRule(from.ToUpperInvariant(), to.ToUpperInvariant());
            AddRule(char.ToUpperInvariant(from[0]) + from.Substring(1), char.ToUpperInvariant(to[0]) + to.Substring(1));
        }
    }

    private void AddRule(string from, string to)
    {
        _rules.Add((new Regex("\\b" + Regex.Escape(from) + "\\b", RegexOptions.ExplicitCapture | RegexOptions.Compiled), to));
    }

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
