using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SubtitleEdit.Plugins.WordCensor;

/// <summary>How a matched offensive word should be replaced.</summary>
public enum CensorMode
{
    /// <summary>Replace the first ~50% of letters with random grawlix characters (@#!?$%&amp;).</summary>
    Grawlix = 0,

    /// <summary>Replace the whole match with a user-supplied string, e.g. "[censored]".</summary>
    Replacement = 1,

    /// <summary>Replace the whole match with another (different) random word from the list.</summary>
    Alternative = 2,
}

/// <summary>Per-call options for <see cref="WordCensorEngine.Censor"/>.</summary>
public sealed class CensorOptions
{
    public CensorMode Mode { get; set; } = CensorMode.Grawlix;

    /// <summary>Used when <see cref="Mode"/> is <see cref="CensorMode.Replacement"/>.</summary>
    public string Replacement { get; set; } = "[censored]";

    /// <summary>When true, wrap each replacement in <c>&lt;font color="#ff0000"&gt;...&lt;/font&gt;</c>.</summary>
    public bool ColorRed { get; set; }
}

/// <summary>
/// Censors offensive words. Three modes are supported via <see cref="CensorMode"/>:
/// grawlix masking, fixed-string replacement, or substitution with another random
/// word from the same list. Matches whole words case-insensitively; multi-word
/// phrases in the list are matched verbatim.
/// </summary>
public sealed class WordCensorEngine
{
    private static readonly char[] GrawlixChars = { '@', '#', '!', '?', '$', '%', '&' };
    private const string RedColor = "#ff0000";

    private readonly HashSet<string> _singleWords;
    private readonly List<string> _multiWordPhrases;
    private readonly List<string> _allWords;
    private readonly Random _random;

    public WordCensorEngine(int? randomSeed = null)
    {
        _singleWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        _multiWordPhrases = new List<string>();
        _random = randomSeed.HasValue ? new Random(randomSeed.Value) : new Random();
        LoadBuiltInList();
        _allWords = _singleWords.Concat(_multiWordPhrases).ToList();
    }

    public int WordCount => _allWords.Count;

    /// <summary>Censors all offensive words in <paramref name="text"/> using <paramref name="options"/>.</summary>
    public string Censor(string text, CensorOptions options)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text;
        }

        // First handle multi-word phrases (they need to match before single-word logic eats their pieces).
        foreach (var phrase in _multiWordPhrases)
        {
            text = ReplacePhrase(text, phrase, options);
        }

        // Then walk single words.
        var sb = new StringBuilder(text.Length);
        var i = 0;
        while (i < text.Length)
        {
            if (IsWordChar(text[i]))
            {
                var start = i;
                while (i < text.Length && IsWordChar(text[i]))
                {
                    i++;
                }
                var word = text.Substring(start, i - start);
                if (_singleWords.Contains(word))
                {
                    sb.Append(MaybeColor(BuildReplacement(word, options), options.ColorRed));
                }
                else
                {
                    sb.Append(word);
                }
            }
            else
            {
                sb.Append(text[i]);
                i++;
            }
        }

        return sb.ToString();
    }

    public bool TryCensor(string text, CensorOptions options, out string censored)
    {
        censored = Censor(text, options);
        return !string.Equals(text, censored, StringComparison.Ordinal);
    }

    private string ReplacePhrase(string text, string phrase, CensorOptions options)
    {
        var idx = 0;
        var sb = new StringBuilder(text.Length);
        while (idx < text.Length)
        {
            var hit = text.IndexOf(phrase, idx, StringComparison.OrdinalIgnoreCase);
            if (hit < 0)
            {
                sb.Append(text, idx, text.Length - idx);
                break;
            }

            var startsClean = hit == 0 || !IsWordChar(text[hit - 1]);
            var endsClean = hit + phrase.Length == text.Length || !IsWordChar(text[hit + phrase.Length]);
            sb.Append(text, idx, hit - idx);
            if (startsClean && endsClean)
            {
                var match = text.Substring(hit, phrase.Length);
                sb.Append(MaybeColor(BuildReplacement(match, options), options.ColorRed));
            }
            else
            {
                sb.Append(text, hit, phrase.Length);
            }
            idx = hit + phrase.Length;
        }
        return sb.ToString();
    }

    private string BuildReplacement(string match, CensorOptions options) => options.Mode switch
    {
        CensorMode.Replacement => string.IsNullOrEmpty(options.Replacement) ? "[censored]" : options.Replacement,
        CensorMode.Alternative => PickAlternative(match),
        _ => Grawlix(match),
    };

    private string PickAlternative(string original)
    {
        if (_allWords.Count <= 1)
        {
            return original;
        }

        // Try a few times to avoid the original word; give up if everything resolves to it.
        for (var attempt = 0; attempt < 8; attempt++)
        {
            var pick = _allWords[_random.Next(_allWords.Count)];
            if (!string.Equals(pick, original, StringComparison.OrdinalIgnoreCase))
            {
                return MatchCase(pick, original);
            }
        }
        return original;
    }

    /// <summary>Apply the rough case pattern of <paramref name="template"/> to <paramref name="word"/>.</summary>
    private static string MatchCase(string word, string template)
    {
        if (string.IsNullOrEmpty(template))
        {
            return word;
        }

        var allUpper = template.All(c => !char.IsLetter(c) || char.IsUpper(c));
        if (allUpper && template.Any(char.IsLetter))
        {
            return word.ToUpperInvariant();
        }

        if (char.IsUpper(template[0]))
        {
            if (word.Length == 0)
            {
                return word;
            }
            return char.ToUpperInvariant(word[0]) + word.Substring(1).ToLowerInvariant();
        }

        return word.ToLowerInvariant();
    }

    private string Grawlix(string word)
    {
        if (word.Length <= 1)
        {
            return word;
        }

        var halfLength = (int)Math.Ceiling(word.Length * 0.5);
        var sb = new StringBuilder(word.Length);
        for (var k = 0; k < halfLength; k++)
        {
            if (word[k] == ' ')
            {
                sb.Append(' ');
            }
            else
            {
                sb.Append(GrawlixChars[_random.Next(GrawlixChars.Length)]);
            }
        }
        sb.Append(word, halfLength, word.Length - halfLength);
        return sb.ToString();
    }

    private static string MaybeColor(string word, bool colorRed) =>
        colorRed ? $"<font color=\"{RedColor}\">{word}</font>" : word;

    private static bool IsWordChar(char c) => char.IsLetterOrDigit(c);

    private void LoadBuiltInList()
    {
        using var stream = typeof(WordCensorEngine).Assembly
            .GetManifestResourceStream("SubtitleEdit.Plugins.WordCensor.BadWords.txt")
            ?? throw new InvalidOperationException("Embedded BadWords.txt not found.");
        using var reader = new StreamReader(stream);
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            var w = line.Trim();
            if (w.Length == 0)
            {
                continue;
            }
            if (w.Contains(' '))
            {
                _multiWordPhrases.Add(w);
            }
            else
            {
                _singleWords.Add(w);
            }
        }
    }
}
