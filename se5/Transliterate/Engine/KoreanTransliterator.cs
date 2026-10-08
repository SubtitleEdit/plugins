using System.Text;

namespace SubtitleEdit.Plugins.Transliterate.Engine;

/// <summary>
/// Korean Hangul → Revised Romanization. Each syllable block is split into its letters, and the common
/// sound changes between blocks are applied: linking (한국어 = hangugeo), ㄹ-assimilation (신라 = silla),
/// nasalization (국물 = gungmul) and aspiration (좋고 = joko).
/// </summary>
public sealed class KoreanTransliterator : Transliterator
{
    private const int SyllableBase = 0xAC00;
    private const int SyllableCount = 11172;
    private const int Ieung = 11; // initial ㅇ - silent
    private const int Nieun = 2;
    private const int Rieul = 5;
    private const int Mieum = 6;
    private const int Hieut = 18;

    private static readonly string[] Initials = { "g", "kk", "n", "d", "tt", "r", "m", "b", "pp", "s", "ss", "", "j", "jj", "ch", "k", "t", "p", "h" };

    private static readonly string[] Vowels =
    {
        "a", "ae", "ya", "yae", "eo", "e", "yeo", "ye", "o", "wa", "wae", "oe", "yo", "u", "wo", "we", "wi", "yu", "eu", "ui", "i",
    };

    // Final consonant (batchim) index → how it sounds at the end of a syllable.
    private static readonly string[] Finals =
    {
        "", "k", "k", "k", "n", "n", "n", "t", "l", "k", "m", "l", "l", "l", "p", "l", "m", "p", "p", "t", "t", "ng", "t", "t", "k", "t", "p", "t",
    };

    // Before a silent ㅇ the final moves over: (what stays, what starts the next syllable).
    private static readonly (string Stay, string Move)[] Linked =
    {
        ("", ""), ("", "g"), ("", "kk"), ("k", "s"), ("", "n"), ("n", "j"), ("", "n"), ("", "d"), ("", "r"), ("l", "g"),
        ("l", "m"), ("l", "b"), ("l", "s"), ("l", "t"), ("l", "p"), ("", "r"), ("", "m"), ("", "b"), ("p", "s"), ("", "s"),
        ("", "ss"), ("ng", ""), ("", "j"), ("", "ch"), ("", "k"), ("", "t"), ("", "p"), ("", ""),
    };

    public override string Id => "ko";
    public override string Name => "Korean";
    public override string NativeName => "한국어";
    public override string Badge => "KO";
    public override string Script => "Hangul";
    public override string Sample => "안녕하세요! 한국어를 배워요. 신라 국물이 좋고 맛있어요.";
    public override bool CapitalizeSentences => true;

    public override bool IsNativeLetter(char c) => c is >= '가' and <= '힣';

    public override string ToLatinWord(string word, int system)
    {
        var sb = new StringBuilder();
        var i = 0;
        while (i < word.Length)
        {
            if (!IsNativeLetter(word[i]))
            {
                sb.Append(word[i]);
                i++;
                continue;
            }

            var end = i;
            while (end < word.Length && IsNativeLetter(word[end]))
            {
                end++;
            }

            sb.Append(Romanize(word[i..end]));
            i = end;
        }

        return sb.ToString();
    }

    private static string Romanize(string hangul)
    {
        var blocks = hangul.Select(c =>
        {
            var s = c - SyllableBase;
            return (Initial: s / 588, Vowel: s % 588 / 28, Final: s % 28);
        }).ToArray();

        var sb = new StringBuilder();
        var nextInitial = blocks.Length > 0 ? Initials[blocks[0].Initial] : string.Empty;
        for (var i = 0; i < blocks.Length; i++)
        {
            var (_, vowel, final) = blocks[i];
            sb.Append(nextInitial).Append(Vowels[vowel]);

            if (i + 1 == blocks.Length)
            {
                sb.Append(Finals[final]);
                break;
            }

            var following = blocks[i + 1].Initial;
            var (stay, move) = Join(final, following);
            sb.Append(stay);
            nextInitial = move;
        }

        return sb.ToString();
    }

    /// <summary>The sound of a final consonant and the next initial when they meet.</summary>
    private static (string Stay, string Move) Join(int final, int initial)
    {
        var initialText = Initials[initial];
        if (final == 0)
        {
            return ("", initialText);
        }

        if (initial == Ieung)
        {
            return Linked[final];
        }

        var sound = Finals[final];

        // Aspiration: ㄱ/ㄷ/ㅂ/ㅈ + ㅎ, and ㅎ + ㄱ/ㄷ/ㅈ.
        if (initial == Hieut && final is 1 or 7 or 17 or 22)
        {
            return ("", final switch { 1 => "k", 7 => "t", 17 => "p", _ => "ch" });
        }

        if (final == 27 && initialText is "g" or "d" or "j")
        {
            return ("", initialText switch { "g" => "k", "d" => "t", _ => "ch" });
        }

        // ㄹ-assimilation: ㄴ+ㄹ, ㄹ+ㄴ, ㄹ+ㄹ = ll.
        if ((sound == "n" && initial == Rieul) || (sound == "l" && initial is Nieun or Rieul))
        {
            return ("l", "l");
        }

        // Nasalization before ㄴ/ㅁ (and ㄹ, which then becomes n).
        if (initial is Nieun or Mieum or Rieul)
        {
            var nasal = sound switch { "k" => "ng", "t" => "n", "p" => "m", _ => sound };
            var move = initial == Rieul ? "n" : initialText;
            return (nasal, move);
        }

        return (sound, initialText);
    }
}
