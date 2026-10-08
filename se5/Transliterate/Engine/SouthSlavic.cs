using System.Text;

namespace SubtitleEdit.Plugins.Transliterate.Engine;

/// <summary>Serbian Cyrillic ↔ Gaj's Latin alphabet - one letter to one letter (љ = lj, њ = nj, џ = dž).</summary>
public class SerbianTransliterator : Transliterator
{
    protected static readonly LetterTable CyrillicToLatin = new(
        ("а", "a"), ("б", "b"), ("в", "v"), ("г", "g"), ("д", "d"), ("ђ", "đ"), ("е", "e"), ("ж", "ž"), ("з", "z"),
        ("и", "i"), ("ј", "j"), ("к", "k"), ("л", "l"), ("љ", "lj"), ("м", "m"), ("н", "n"), ("њ", "nj"), ("о", "o"),
        ("п", "p"), ("р", "r"), ("с", "s"), ("т", "t"), ("ћ", "ć"), ("у", "u"), ("ф", "f"), ("х", "h"), ("ц", "c"),
        ("ч", "č"), ("џ", "dž"), ("ш", "š"));

    /// <summary>
    /// Word starts where "nj", "lj" or "dž" are two letters, not one: inJekcija, konJugacija, nadŽiveti.
    /// </summary>
    private static readonly (string Latin, string Cyrillic)[] SplitDigraphPrefixes =
    {
        ("injekc", "инјекц"), ("konjug", "конјуг"), ("konjunk", "конјунк"), ("vanjezi", "ванјези"),
        ("nadživ", "наджив"), ("nadžnj", "наджњ"), ("podžnj", "поджњ"), ("nadždr", "надждр"),
    };

    private readonly LetterTable _toLatin;
    private readonly LetterTable _fromLatin;

    public SerbianTransliterator() : this(CyrillicToLatin)
    {
    }

    protected SerbianTransliterator(LetterTable toLatin)
    {
        _toLatin = toLatin;
        _fromLatin = toLatin.Reverse();
    }

    public override string Id => "sr";
    public override string Name => "Serbian";
    public override string NativeName => "Српски · Srpski";
    public override string Badge => "SR";
    public override string Script => "Cyrillic";
    public override string Sample => "Добро јутро, Љубице! Ђорђе већ чека џип.";
    public override bool CanReverse => true;

    public override bool IsNativeLetter(char c) => IsCyrillic(c);

    public override string ToLatinWord(string word, int system) => MapWord(word, _toLatin);

    public override string FromLatinWord(string word)
    {
        var normalized = word.Normalize(NormalizationForm.FormC);
        return MapWord(normalized, _fromLatin, (lower, i) =>
        {
            if (i == 0)
            {
                foreach (var (latin, cyrillic) in SplitDigraphPrefixes)
                {
                    if (lower.StartsWith(latin, StringComparison.Ordinal))
                    {
                        return (latin.Length, cyrillic);
                    }
                }
            }

            return null;
        });
    }

    /// <summary>q, w, x and y are not in the Serbian Latin alphabet - such words are foreign.</summary>
    public override bool IsForeignLatinWord(string word) => word.Any(c => c is 'q' or 'w' or 'x' or 'y' or 'Q' or 'W' or 'X' or 'Y');
}

/// <summary>Montenegrin: Serbian plus с́ ↔ ś and з́ ↔ ź.</summary>
public sealed class MontenegrinTransliterator : SerbianTransliterator
{
    public MontenegrinTransliterator() : base(CyrillicToLatin.With(("с́", "ś"), ("з́", "ź")))
    {
    }

    public override string Id => "cnr";
    public override string Name => "Montenegrin";
    public override string NativeName => "Црногорски · Crnogorski";
    public override string Badge => "ME";
    public override string Sample => "С́утра ћемо на море, ђе си ти?";
}

/// <summary>
/// Macedonian. To Latin with the scientific (ǵ, ž, ḱ, č, š) or a plain ASCII (gj, zh, kj, ch, sh) system;
/// back to Cyrillic accepts both.
/// </summary>
public sealed class MacedonianTransliterator : Transliterator
{
    private static readonly LetterTable Common = new(
        ("а", "a"), ("б", "b"), ("в", "v"), ("г", "g"), ("д", "d"), ("е", "e"), ("з", "z"), ("ѕ", "dz"), ("и", "i"),
        ("ј", "j"), ("к", "k"), ("л", "l"), ("љ", "lj"), ("м", "m"), ("н", "n"), ("њ", "nj"), ("о", "o"), ("п", "p"),
        ("р", "r"), ("с", "s"), ("т", "t"), ("у", "u"), ("ф", "f"), ("х", "h"), ("ц", "c"));

    private static readonly LetterTable Scientific = Common.With(("ѓ", "ǵ"), ("ж", "ž"), ("ќ", "ḱ"), ("ч", "č"), ("џ", "dž"), ("ш", "š"));
    private static readonly LetterTable Ascii = Common.With(("ѓ", "gj"), ("ж", "zh"), ("ќ", "kj"), ("ч", "ch"), ("џ", "dzh"), ("ш", "sh"));

    private static readonly LetterTable FromLatinTable = Scientific.Reverse().With(
        ("gj", "ѓ"), ("zh", "ж"), ("kj", "ќ"), ("ch", "ч"), ("dzh", "џ"), ("sh", "ш"));

    public override string Id => "mk";
    public override string Name => "Macedonian";
    public override string NativeName => "Македонски · Makedonski";
    public override string Badge => "MK";
    public override string Script => "Cyrillic";
    public override string Sample => "Добро утро! Ќе дојдам кај Ѓорѓи на чај.";
    public override bool CanReverse => true;

    public override IReadOnlyList<TransliterationSystem> Systems { get; } = new[]
    {
        new TransliterationSystem("Scientific", "ǵ, ž, ḱ, č, dž, š - the standard scholarly transliteration"),
        new TransliterationSystem("ASCII", "gj, zh, kj, ch, dzh, sh - no special letters, as on signs and passports"),
    };

    public override bool IsNativeLetter(char c) => IsCyrillic(c);

    public override string ToLatinWord(string word, int system) => MapWord(word, system == 1 ? Ascii : Scientific);

    public override string FromLatinWord(string word) => MapWord(word.Normalize(NormalizationForm.FormC), FromLatinTable);

    public override bool IsForeignLatinWord(string word) => word.Any(c => c is 'q' or 'w' or 'x' or 'y' or 'Q' or 'W' or 'X' or 'Y');
}
