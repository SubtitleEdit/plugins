using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace SubtitleEdit.Plugins.Transliterate.Engine;

/// <summary>
/// Converts subtitle text word by word. Tags (<c>&lt;i&gt;</c>, <c>{\an8}</c>), URLs, e-mail addresses,
/// Roman numerals and the user's keep-list are left as they are.
/// </summary>
public static class TextTransliterator
{
    private static readonly Regex Protected = new(
        @"<[^<>]*>|\{[^{}]*\}|https?://\S+|www\.\S+|[\w.+-]+@[\w-]+\.[\w.]+",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private static readonly Regex RomanNumeral = new(@"^(?=[MDCLXVI])M{0,4}(CM|CD|D?C{0,3})(XC|XL|L?X{0,3})(IX|IV|V?I{0,3})$", RegexOptions.CultureInvariant);

    public static string Convert(string text, Transliterator language, Direction direction, int system, IReadOnlySet<string>? keep = null)
    {
        if (string.IsNullOrEmpty(text) || !language.HasLettersToConvert(text, direction))
        {
            return text;
        }

        var sb = new StringBuilder(text.Length + 8);
        var sentenceStart = true;
        var last = 0;
        foreach (Match m in Protected.Matches(text))
        {
            ConvertPlain(text, last, m.Index, language, direction, system, keep, sb, ref sentenceStart);
            sb.Append(m.Value);
            last = m.Index + m.Length;
        }

        ConvertPlain(text, last, text.Length, language, direction, system, keep, sb, ref sentenceStart);
        return sb.ToString();
    }

    private static void ConvertPlain(string text, int start, int end, Transliterator language, Direction direction, int system,
        IReadOnlySet<string>? keep, StringBuilder sb, ref bool sentenceStart)
    {
        var i = start;
        while (i < end)
        {
            if (!IsWordChar(text[i]))
            {
                var c = language.MapPunctuation(text[i], direction);
                sb.Append(c);
                if (c is '.' or '!' or '?' or '…' or '\n')
                {
                    sentenceStart = true;
                }
                else if (char.IsDigit(c))
                {
                    sentenceStart = false;
                }

                i++;
                continue;
            }

            var wordEnd = i + 1;
            while (wordEnd < end && (IsWordChar(text[wordEnd]) || (IsApostrophe(text[wordEnd]) && wordEnd + 1 < end && IsWordChar(text[wordEnd + 1]))))
            {
                wordEnd++;
            }

            var word = text[i..wordEnd];
            var converted = ShouldKeep(word, language, direction, keep)
                ? word
                : direction == Direction.ToLatin ? language.ToLatinWord(word, system) : language.FromLatinWord(word);

            if (sentenceStart && language.CapitalizeSentences && direction == Direction.ToLatin && converted.Length > 0 && converted != word)
            {
                converted = language.ToUpper(converted[..1]) + converted[1..];
            }

            if (word.Any(char.IsLetterOrDigit))
            {
                sentenceStart = false;
            }

            sb.Append(converted);
            i = wordEnd;
        }
    }

    private static bool ShouldKeep(string word, Transliterator language, Direction direction, IReadOnlySet<string>? keep)
    {
        if (keep != null && keep.Contains(word))
        {
            return true;
        }

        return direction == Direction.FromLatin && (RomanNumeral.IsMatch(word) || language.IsForeignLatinWord(word));
    }

    private static bool IsWordChar(char c)
    {
        if (char.IsLetter(c))
        {
            return true;
        }

        var category = CharUnicodeInfo.GetUnicodeCategory(c);
        return category is UnicodeCategory.NonSpacingMark or UnicodeCategory.SpacingCombiningMark || c is 'ʻ' or 'ʼ';
    }

    public static bool IsApostrophe(char c) => c is '\'' or '’' or '‘' or '`' or 'ʻ' or 'ʼ';
}
