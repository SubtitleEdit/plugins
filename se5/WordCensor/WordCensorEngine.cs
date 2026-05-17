using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SubtitleEdit.Plugins.WordCensor;

/// <summary>
/// Censors offensive words by replacing the first ~50% of each match with
/// random "grawlix" characters (@#!?$%&amp;). Optionally wraps the censored
/// word in a red &lt;font&gt; tag. Matches whole words case-insensitively;
/// multi-word phrases in the list are also matched verbatim.
/// </summary>
public sealed class WordCensorEngine
{
    private static readonly char[] GrawlixChars = { '@', '#', '!', '?', '$', '%', '&' };
    private const string RedColor = "#ff0000";

    private readonly HashSet<string> _singleWords;
    private readonly List<string> _multiWordPhrases;
    private readonly Random _random;

    public WordCensorEngine(int? randomSeed = null)
    {
        _singleWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        _multiWordPhrases = new List<string>();
        _random = randomSeed.HasValue ? new Random(randomSeed.Value) : new Random();
        LoadBuiltInList();
    }

    public int WordCount => _singleWords.Count + _multiWordPhrases.Count;

    /// <summary>
    /// Censors all offensive words in <paramref name="text"/>. When <paramref name="colorRed"/> is true,
    /// each censored word is wrapped in &lt;font color="#ff0000"&gt;...&lt;/font&gt;.
    /// </summary>
    public string Censor(string text, bool colorRed)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text;
        }

        // First handle multi-word phrases (they need to match before single-word logic eats their pieces).
        foreach (var phrase in _multiWordPhrases)
        {
            text = ReplacePhrase(text, phrase, colorRed);
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
                    sb.Append(MaybeColor(Grawlix(word), colorRed));
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

    public bool TryCensor(string text, bool colorRed, out string censored)
    {
        censored = Censor(text, colorRed);
        return !string.Equals(text, censored, StringComparison.Ordinal);
    }

    private string ReplacePhrase(string text, string phrase, bool colorRed)
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
                sb.Append(MaybeColor(Grawlix(text.Substring(hit, phrase.Length)), colorRed));
            }
            else
            {
                sb.Append(text, hit, phrase.Length);
            }
            idx = hit + phrase.Length;
        }
        return sb.ToString();
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
