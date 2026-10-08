namespace SubtitleEdit.Plugins.Transliterate.Engine;

public static class Languages
{
    public static IReadOnlyList<Transliterator> All { get; } = new Transliterator[]
    {
        new SerbianTransliterator(),
        new MontenegrinTransliterator(),
        new MacedonianTransliterator(),
        new RussianTransliterator(),
        new UkrainianTransliterator(),
        new BelarusianTransliterator(),
        new BulgarianTransliterator(),
        new KazakhTransliterator(),
        new UzbekTransliterator(),
        new GreekTransliterator(),
        new KoreanTransliterator(),
    };

    public static Transliterator? Find(string? id) => All.FirstOrDefault(l => l.Id == id);
}

/// <summary>Guesses the language and direction from the letters used in the subtitle.</summary>
public static class ScriptDetector
{
    public static (Transliterator Language, Direction Direction)? Detect(IEnumerable<string> texts)
    {
        var counts = new Dictionary<char, int>();
        var cyrillic = 0;
        var greek = 0;
        var hangul = 0;
        var latin = 0;
        foreach (var text in texts)
        {
            foreach (var c in text.ToLowerInvariant())
            {
                if (c is >= 'Ѐ' and <= 'ӿ')
                {
                    cyrillic++;
                }
                else if (c is >= 'Ͱ' and <= 'Ͽ' or >= 'ἀ' and <= '῿')
                {
                    greek++;
                }
                else if (c is >= '가' and <= '힣')
                {
                    hangul++;
                }
                else if (c is >= 'a' and <= 'z' or >= 'À' and <= 'ɏ' or 'ʻ')
                {
                    latin++;
                }
                else
                {
                    continue;
                }

                counts[c] = counts.TryGetValue(c, out var n) ? n + 1 : 1;
            }
        }

        int Count(string letters) => letters.Sum(c => counts.TryGetValue(c, out var n) ? n : 0);

        var best = new[] { cyrillic, greek, hangul }.Max();
        if (best == 0 || best < latin / 3)
        {
            // Mostly Latin: Serbian (đ, ć) or Uzbek (oʻ, gʻ, q) written in Latin can go back to Cyrillic.
            if (Count("đć") > 0 || (Count("čšž") > 0 && Count("q") == 0))
            {
                return (Languages.Find("sr")!, Direction.FromLatin);
            }

            if (Count("ʻ") > 0)
            {
                return (Languages.Find("uz")!, Direction.FromLatin);
            }

            return null;
        }

        if (hangul == best)
        {
            return (Languages.Find("ko")!, Direction.ToLatin);
        }

        if (greek == best)
        {
            return (Languages.Find("el")!, Direction.ToLatin);
        }

        // Letters only one of the Cyrillic languages has, strongest signal first.
        var scores = new (string Id, int Score)[]
        {
            ("kk", Count("әғқңөұүһ") * 3),
            ("uz", Count("қғҳ") * 3 + Count("ў") - Count("ыі") * 2),
            ("mk", Count("ѓќѕ") * 3),
            ("sr", Count("ђћџ") * 3 + Count("јљњ")),
            ("uk", Count("єїґ") * 3 + Count("і") - Count("ыэёъ") * 2),
            ("be", Count("ў") * 3 + Count("і") * 2 - Count("ищъ") * 2),
            ("ru", Count("ыэё") * 2 + 1 - Count("іўєїґ") * 2),
            ("bg", Count("ъ") * 2 - Count("ыэё") * 2),
        };

        var winner = scores.OrderByDescending(s => s.Score).First(); // ties go to the earlier entry
        return (Languages.Find(winner.Id)!, Direction.ToLatin);
    }
}
