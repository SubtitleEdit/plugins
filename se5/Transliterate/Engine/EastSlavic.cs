namespace SubtitleEdit.Plugins.Transliterate.Engine;

/// <summary>Russian → Latin: a readable ASCII system or ISO 9 (one letter per letter, with diacritics).</summary>
public sealed class RussianTransliterator : Transliterator
{
    private const string Vowels = "аеёиоуыэюяъь";

    private static readonly LetterTable Readable = new(
        ("а", "a"), ("б", "b"), ("в", "v"), ("г", "g"), ("д", "d"), ("е", "e"), ("ё", "yo"), ("ж", "zh"), ("з", "z"),
        ("и", "i"), ("й", "y"), ("к", "k"), ("л", "l"), ("м", "m"), ("н", "n"), ("о", "o"), ("п", "p"), ("р", "r"),
        ("с", "s"), ("т", "t"), ("у", "u"), ("ф", "f"), ("х", "kh"), ("ц", "ts"), ("ч", "ch"), ("ш", "sh"),
        ("щ", "shch"), ("ъ", ""), ("ы", "y"), ("ь", ""), ("э", "e"), ("ю", "yu"), ("я", "ya"));

    private static readonly LetterTable Iso9 = new(
        ("а", "a"), ("б", "b"), ("в", "v"), ("г", "g"), ("д", "d"), ("е", "e"), ("ё", "ë"), ("ж", "ž"), ("з", "z"),
        ("и", "i"), ("й", "j"), ("к", "k"), ("л", "l"), ("м", "m"), ("н", "n"), ("о", "o"), ("п", "p"), ("р", "r"),
        ("с", "s"), ("т", "t"), ("у", "u"), ("ф", "f"), ("х", "h"), ("ц", "c"), ("ч", "č"), ("ш", "š"),
        ("щ", "ŝ"), ("ъ", "ʺ"), ("ы", "y"), ("ь", "ʹ"), ("э", "è"), ("ю", "û"), ("я", "â"));

    public override string Id => "ru";
    public override string Name => "Russian";
    public override string NativeName => "Русский";
    public override string Badge => "RU";
    public override string Script => "Cyrillic";
    public override string Sample => "Здравствуйте! Это Юлия и Пётр, ещё раз.";

    public override IReadOnlyList<TransliterationSystem> Systems { get; } = new[]
    {
        new TransliterationSystem("Readable", "zh, kh, ts, ch, sh, shch, yu, ya - plain ASCII, ye at the start of a word"),
        new TransliterationSystem("ISO 9", "ž, h, c, č, š, ŝ, û, â - one Latin letter for each Cyrillic letter"),
    };

    public override bool IsNativeLetter(char c) => IsCyrillic(c);

    public override string ToLatinWord(string word, int system)
    {
        if (system == 1)
        {
            return MapWord(word, Iso9);
        }

        return MapWord(word, Readable, (lower, i) =>
            lower[i] == 'е' && (i == 0 || Vowels.Contains(lower[i - 1])) ? (1, "ye") : null);
    }
}

/// <summary>Ukrainian → Latin with the official national system (Cabinet of Ministers, 2010).</summary>
public sealed class UkrainianTransliterator : Transliterator
{
    private static readonly LetterTable Table = new(
        ("а", "a"), ("б", "b"), ("в", "v"), ("г", "h"), ("ґ", "g"), ("д", "d"), ("е", "e"), ("є", "ie"), ("ж", "zh"),
        ("з", "z"), ("зг", "zgh"), ("и", "y"), ("і", "i"), ("ї", "i"), ("й", "i"), ("к", "k"), ("л", "l"), ("м", "m"),
        ("н", "n"), ("о", "o"), ("п", "p"), ("р", "r"), ("с", "s"), ("т", "t"), ("у", "u"), ("ф", "f"), ("х", "kh"),
        ("ц", "ts"), ("ч", "ch"), ("ш", "sh"), ("щ", "shch"), ("ь", ""), ("ю", "iu"), ("я", "ia"),
        ("'", ""), ("’", ""), ("ʼ", ""), ("‘", ""), ("`", ""));

    private static readonly Dictionary<char, string> WordStart = new()
    {
        ['є'] = "ye", ['ї'] = "yi", ['й'] = "y", ['ю'] = "yu", ['я'] = "ya",
    };

    public override string Id => "uk";
    public override string Name => "Ukrainian";
    public override string NativeName => "Українська";
    public override string Badge => "UK";
    public override string Script => "Cyrillic";
    public override string Sample => "Привіт, Євгене! Ще їжа є? Згода, м’ясо.";

    public override bool IsNativeLetter(char c) => IsCyrillic(c);

    public override string ToLatinWord(string word, int system) =>
        MapWord(word, Table, (lower, i) => i == 0 && WordStart.TryGetValue(lower[0], out var start) ? (1, start) : null);
}

/// <summary>Belarusian → Latin, plain ASCII in the BGN/PCGN style (г = h, ў = w).</summary>
public sealed class BelarusianTransliterator : Transliterator
{
    private const string Vowels = "аеёіоуыэюяьўй'’ʼ";

    private static readonly LetterTable Table = new(
        ("а", "a"), ("б", "b"), ("в", "v"), ("г", "h"), ("ґ", "g"), ("д", "d"), ("е", "e"), ("ё", "yo"), ("ж", "zh"),
        ("з", "z"), ("і", "i"), ("й", "y"), ("к", "k"), ("л", "l"), ("м", "m"), ("н", "n"), ("о", "o"), ("п", "p"),
        ("р", "r"), ("с", "s"), ("т", "t"), ("у", "u"), ("ў", "w"), ("ф", "f"), ("х", "kh"), ("ц", "ts"), ("ч", "ch"),
        ("ш", "sh"), ("ы", "y"), ("ь", ""), ("э", "e"), ("ю", "yu"), ("я", "ya"),
        ("'", ""), ("’", ""), ("ʼ", ""));

    public override string Id => "be";
    public override string Name => "Belarusian";
    public override string NativeName => "Беларуская";
    public override string Badge => "BE";
    public override string Script => "Cyrillic";
    public override string Sample => "Добры дзень! Як справы ў цябе, Алесь?";

    public override bool IsNativeLetter(char c) => IsCyrillic(c);

    public override string ToLatinWord(string word, int system) =>
        MapWord(word, Table, (lower, i) => lower[i] == 'е' && (i == 0 || Vowels.Contains(lower[i - 1])) ? (1, "ye") : null);
}
