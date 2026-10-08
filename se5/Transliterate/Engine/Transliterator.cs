using System.Globalization;
using System.Text;

namespace SubtitleEdit.Plugins.Transliterate.Engine;

public enum Direction
{
    /// <summary>From the language's own script to Latin.</summary>
    ToLatin,

    /// <summary>From Latin back to the language's own script.</summary>
    FromLatin,
}

/// <summary>A named romanization system, e.g. "ISO 9" for Russian.</summary>
public sealed record TransliterationSystem(string Name, string Description);

/// <summary>
/// One language's script conversion. Works word by word: <see cref="TextTransliterator"/> cuts the
/// text into words and leaves tags, URLs and punctuation alone.
/// </summary>
public abstract class Transliterator
{
    public abstract string Id { get; }
    public abstract string Name { get; }
    public abstract string NativeName { get; }

    /// <summary>Short code shown on the language tile, e.g. "SR".</summary>
    public abstract string Badge { get; }

    /// <summary>The non-Latin script: "Cyrillic", "Greek" or "Hangul".</summary>
    public abstract string Script { get; }

    /// <summary>A sentence in the native script used for the live example.</summary>
    public abstract string Sample { get; }

    public virtual bool CanReverse => false;

    public virtual IReadOnlyList<TransliterationSystem> Systems { get; } = Array.Empty<TransliterationSystem>();

    /// <summary>Capitalize the first letter of each sentence in the output (scripts without case).</summary>
    public virtual bool CapitalizeSentences => false;

    public abstract string ToLatinWord(string word, int system);

    public virtual string FromLatinWord(string word) => word;

    /// <summary>A Latin word that should not be converted to the native script (foreign words).</summary>
    public virtual bool IsForeignLatinWord(string word) => false;

    /// <summary>Punctuation that differs between the scripts, e.g. the Greek question mark ";".</summary>
    public virtual char MapPunctuation(char c, Direction direction) => c;

    /// <summary>True for letters of the native script (used to find lines to convert and to detect the language).</summary>
    public abstract bool IsNativeLetter(char c);

    public virtual string ToUpper(string s) => s.ToUpperInvariant();

    public bool HasLettersToConvert(string text, Direction direction) =>
        direction == Direction.ToLatin ? text.Any(IsNativeLetter) : text.Any(c => c < 0x250 && char.IsLetter(c));

    /// <summary>
    /// Walks <paramref name="word"/> with a longest-match <paramref name="table"/>. <paramref name="rule"/> is asked first at
    /// every position and may return a match of its own (context rules like "ye at the start of a word").
    /// Case follows the source: "Љ" → "Lj", but "ЉУБАВ" → "LJUBAV".
    /// </summary>
    protected string MapWord(string word, LetterTable table, Func<string, int, (int Length, string Output)?>? rule = null)
    {
        var lower = Lower(word);
        var allCaps = IsAllCaps(word);
        var sb = new StringBuilder(word.Length + 4);
        for (var i = 0; i < word.Length;)
        {
            var match = rule?.Invoke(lower, i);
            if (match == null && table.TryMatch(lower, i, out var length, out var output))
            {
                match = (length, output);
            }

            if (match is not { } m || m.Length <= 0)
            {
                sb.Append(word[i]);
                i++;
                continue;
            }

            sb.Append(ApplyCase(word.Substring(i, m.Length), m.Output, allCaps));
            i += m.Length;
        }

        return sb.ToString();
    }

    /// <summary>Lower-cases char by char so indexes stay the same as in the original word.</summary>
    protected static string Lower(string word)
    {
        var chars = word.ToCharArray();
        for (var i = 0; i < chars.Length; i++)
        {
            chars[i] = char.ToLowerInvariant(chars[i]);
        }

        return new string(chars);
    }

    protected static bool IsAllCaps(string word)
    {
        var letters = 0;
        foreach (var c in word)
        {
            if (!char.IsLetter(c))
            {
                continue;
            }

            if (char.IsLower(c))
            {
                return false;
            }

            if (char.IsUpper(c))
            {
                letters++;
            }
        }

        return letters >= 2;
    }

    protected string ApplyCase(string source, string output, bool wordAllCaps)
    {
        if (output.Length == 0 || !source.Any(char.IsUpper))
        {
            return output;
        }

        if (wordAllCaps || (source.Length > 1 && source.All(c => !char.IsLetter(c) || char.IsUpper(c))))
        {
            return ToUpper(output);
        }

        // Title case: upper-case the first letter, keep the rest ("Љ" → "Lj", "Oʻ").
        var first = 0;
        while (first < output.Length && !char.IsLetter(output[first]))
        {
            first++;
        }

        if (first == output.Length)
        {
            return output;
        }

        var end = first + 1;
        while (end < output.Length && CharUnicodeInfo.GetUnicodeCategory(output[end]) == UnicodeCategory.NonSpacingMark)
        {
            end++;
        }

        return output[..first] + ToUpper(output[first..end]) + output[end..];
    }

    protected static bool IsCyrillic(char c) => c is >= 'Ѐ' and <= 'ӿ';
}

/// <summary>Lower-case source sequence → target, matched longest first.</summary>
public sealed class LetterTable
{
    private readonly Dictionary<string, string> _map = new(StringComparer.Ordinal);
    private readonly int _maxLength;

    public LetterTable(params (string From, string To)[] pairs)
    {
        foreach (var (from, to) in pairs)
        {
            _map[from] = to;
            _maxLength = Math.Max(_maxLength, from.Length);
        }
    }

    public LetterTable With(params (string From, string To)[] pairs) =>
        new(_map.Select(p => (p.Key, p.Value)).Where(p => pairs.All(x => x.From != p.Key)).Concat(pairs).ToArray());

    public IEnumerable<string> Keys => _map.Keys;

    public bool TryMatch(string lower, int index, out int length, out string output)
    {
        for (length = Math.Min(_maxLength, lower.Length - index); length > 0; length--)
        {
            if (_map.TryGetValue(lower.Substring(index, length), out output!))
            {
                return true;
            }
        }

        output = string.Empty;
        return false;
    }

    /// <summary>The same table the other way round (first source wins for duplicate targets).</summary>
    public LetterTable Reverse()
    {
        var pairs = new List<(string, string)>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var (from, to) in _map)
        {
            if (to.Length > 0 && seen.Add(to))
            {
                pairs.Add((to, from));
            }
        }

        return new LetterTable(pairs.ToArray());
    }
}
