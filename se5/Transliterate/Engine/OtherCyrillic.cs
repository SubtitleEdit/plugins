namespace SubtitleEdit.Plugins.Transliterate.Engine;

/// <summary>Bulgarian → Latin with the official Streamlined System (2009): щ = sht, ъ = a.</summary>
public sealed class BulgarianTransliterator : Transliterator
{
    private static readonly LetterTable Table = new(
        ("а", "a"), ("б", "b"), ("в", "v"), ("г", "g"), ("д", "d"), ("е", "e"), ("ж", "zh"), ("з", "z"), ("и", "i"),
        ("й", "y"), ("к", "k"), ("л", "l"), ("м", "m"), ("н", "n"), ("о", "o"), ("п", "p"), ("р", "r"), ("с", "s"),
        ("т", "t"), ("у", "u"), ("ф", "f"), ("х", "h"), ("ц", "ts"), ("ч", "ch"), ("ш", "sh"), ("щ", "sht"),
        ("ъ", "a"), ("ь", "y"), ("ю", "yu"), ("я", "ya"));

    public override string Id => "bg";
    public override string Name => "Bulgarian";
    public override string NativeName => "Български";
    public override string Badge => "BG";
    public override string Script => "Cyrillic";
    public override string Sample => "Здравейте! България е красива, нали?";

    public override bool IsNativeLetter(char c) => IsCyrillic(c);

    /// <summary>"ия" at the end of a word is "ia" (България → Bulgaria).</summary>
    public override string ToLatinWord(string word, int system) =>
        MapWord(word, Table, (lower, i) => i == lower.Length - 2 && lower.EndsWith("ия", StringComparison.Ordinal) ? (2, "ia") : null);
}

/// <summary>Kazakh → the Latin alphabet of 2021 (ә = ä, ғ = ğ, қ = q, ң = ñ, ө = ö, ұ = ū, ү = ü, ш = ş, і = ı).</summary>
public sealed class KazakhTransliterator : Transliterator
{
    private static readonly LetterTable Table = new(
        ("а", "a"), ("ә", "ä"), ("б", "b"), ("в", "v"), ("г", "g"), ("ғ", "ğ"), ("д", "d"), ("е", "e"), ("ё", "io"),
        ("ж", "j"), ("з", "z"), ("и", "i"), ("й", "i"), ("к", "k"), ("қ", "q"), ("л", "l"), ("м", "m"), ("н", "n"),
        ("ң", "ñ"), ("о", "o"), ("ө", "ö"), ("п", "p"), ("р", "r"), ("с", "s"), ("т", "t"), ("у", "u"), ("ұ", "ū"),
        ("ү", "ü"), ("ф", "f"), ("х", "h"), ("һ", "h"), ("ц", "ts"), ("ч", "ch"), ("ш", "ş"), ("щ", "şş"), ("ъ", ""),
        ("ы", "y"), ("і", "ı"), ("ь", ""), ("э", "e"), ("ю", "iu"), ("я", "ia"));

    public override string Id => "kk";
    public override string Name => "Kazakh";
    public override string NativeName => "Қазақша";
    public override string Badge => "KK";
    public override string Script => "Cyrillic";
    public override string Sample => "Сәлеметсіз бе! Қазақстанға қош келдіңіз.";

    public override bool IsNativeLetter(char c) => IsCyrillic(c);

    public override string ToLatinWord(string word, int system) => MapWord(word, Table);

    /// <summary>Turkic casing: i → İ, ı → I.</summary>
    public override string ToUpper(string s) => s.Replace('i', 'İ').Replace('ı', 'I').ToUpperInvariant();
}

/// <summary>Uzbek Cyrillic ↔ the official Latin alphabet (ў = oʻ, ғ = gʻ, қ = q, ҳ = h, х = x).</summary>
public sealed class UzbekTransliterator : Transliterator
{
    private const string Vowels = "аеёиоуэюяўъь";
    private const char Okina = 'ʻ'; // U+02BB, used in oʻ and gʻ
    private const char Tutuq = 'ʼ'; // U+02BC, the hard sign

    private static readonly LetterTable ToLatin = new(
        ("а", "a"), ("б", "b"), ("в", "v"), ("г", "g"), ("ғ", "gʻ"), ("д", "d"), ("е", "e"), ("ё", "yo"), ("ж", "j"),
        ("з", "z"), ("и", "i"), ("й", "y"), ("к", "k"), ("қ", "q"), ("л", "l"), ("м", "m"), ("н", "n"), ("о", "o"),
        ("п", "p"), ("р", "r"), ("с", "s"), ("т", "t"), ("у", "u"), ("ў", "oʻ"), ("ф", "f"), ("х", "x"), ("ҳ", "h"),
        ("ц", "ts"), ("ч", "ch"), ("ш", "sh"), ("ъ", "ʼ"), ("ь", ""), ("э", "e"), ("ю", "yu"), ("я", "ya"));

    private static readonly LetterTable FromLatin = new(
        ("a", "а"), ("b", "б"), ("v", "в"), ("g", "г"), ("gʻ", "ғ"), ("d", "д"), ("e", "е"), ("ye", "е"), ("yo", "ё"),
        ("j", "ж"), ("z", "з"), ("i", "и"), ("y", "й"), ("k", "к"), ("q", "қ"), ("l", "л"), ("m", "м"), ("n", "н"),
        ("o", "о"), ("oʻ", "ў"), ("p", "п"), ("r", "р"), ("s", "с"), ("t", "т"), ("u", "у"), ("f", "ф"), ("x", "х"),
        ("h", "ҳ"), ("ts", "ц"), ("ch", "ч"), ("sh", "ш"), ("ʼ", "ъ"), ("yu", "ю"), ("ya", "я"));

    public override string Id => "uz";
    public override string Name => "Uzbek";
    public override string NativeName => "Oʻzbekcha · Ўзбекча";
    public override string Badge => "UZ";
    public override string Script => "Cyrillic";
    public override string Sample => "Ассалому алайкум! Ўзбекистонга хуш келибсиз, Ғайрат.";
    public override bool CanReverse => true;

    public override bool IsNativeLetter(char c) => IsCyrillic(c);

    public override string ToLatinWord(string word, int system) =>
        MapWord(word, ToLatin, (lower, i) => lower[i] == 'е' && (i == 0 || Vowels.Contains(lower[i - 1])) ? (1, "ye") : null);

    public override string FromLatinWord(string word)
    {
        // Typed text uses any apostrophe: ' ‘ ’ ` - after o/g it is the oʻ/gʻ mark, elsewhere the hard sign.
        var chars = word.ToCharArray();
        for (var i = 0; i < chars.Length; i++)
        {
            if (TextTransliterator.IsApostrophe(chars[i]))
            {
                chars[i] = i > 0 && chars[i - 1] is 'o' or 'O' or 'g' or 'G' ? Okina : Tutuq;
            }
        }

        // э at the start of a word, е elsewhere.
        return MapWord(new string(chars), FromLatin, (lower, i) => i == 0 && lower[0] == 'e' ? (1, "э") : null);
    }
}
