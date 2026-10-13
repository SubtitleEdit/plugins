using System.Text;

namespace SubtitleEdit.Plugins.FixEncodingErrors;

/// <summary>A single-byte code page that UTF-8 text may have been misread with.</summary>
public sealed record SourceEncoding(int CodePage, string Name)
{
    public override string ToString() => Name;
}

/// <summary>
/// Repairs UTF-8 text that was decoded with a single-byte code page and saved again ("mojibake"):
/// <c>rÃ©sumÃ©</c> → <c>résumé</c>, <c>donâ€™t</c> → <c>don’t</c>. Each character is mapped back to its byte
/// in the code page and runs of bytes that form valid UTF-8 are decoded. Also handles text that was lowercased
/// after it broke (<c>rã©sumã©</c>), a lost <c>0x9D</c> byte (<c>â€</c> → <c>”</c>) and a byte order mark
/// read as text (<c>ï»¿</c>).
/// </summary>
public static class EncodingFixer
{
    public static readonly IReadOnlyList<SourceEncoding> Encodings = new[]
    {
        new SourceEncoding(1252, "Western (Windows-1252)"),
        new SourceEncoding(1250, "Central European (Windows-1250)"),
        new SourceEncoding(1251, "Cyrillic (Windows-1251)"),
        new SourceEncoding(1253, "Greek (Windows-1253)"),
        new SourceEncoding(1254, "Turkish (Windows-1254)"),
        new SourceEncoding(1257, "Baltic (Windows-1257)"),
    };

    private const int MaxPasses = 3;

    // Punctuation that also follows a real accented letter at the end of a word ("Não”", "irmã…"):
    // a two-character match ending in one of these is only fixed when there is other evidence.
    private const string AmbiguousFollowers = "…‘’“”–—«»‹›„‚•· ";

    // Letters (to char.IsLetter) that are still telling as the second character: ú, õ, ö in Western.
    private const string TellingLetters = "ªµº";

    // Share of the code page's non-ASCII characters that must be part of matches before a subtitle counts as
    // broken. Broken text is nearly all matches; real Macedonian has a few look-alikes (ој, ње) among normal letters.
    private const double MinCoverage = 0.3;

    /// <summary>
    /// What a two-byte character decoded from a code page may be: its own script plus Latin-1 symbols
    /// (°, «, », ...), and the lead bytes a lowercased letter is tried as.
    /// </summary>
    private sealed record Script((int From, int To)[] TwoByteRanges, byte[] LowercasedLeads);

    private static readonly Script Latin = new(new[] { (0xA0, 0x24F) }, new byte[] { 0xC3, 0xC4, 0xC5 });

    // Western PCs misread everything, so Windows-1252 also decodes to Greek, Cyrillic, Hebrew and Arabic.
    private static readonly Script Western = new(new[] { (0xA0, 0x24F), (0x370, 0x52F), (0x590, 0x6FF) }, new byte[] { 0xC3, 0xC4, 0xC5 });
    private static readonly Script Cyrillic = new(new[] { (0xA0, 0xBF), (0x400, 0x52F) }, new byte[] { 0xD0, 0xD1 });
    private static readonly Script Greek = new(new[] { (0xA0, 0xBF), (0x370, 0x3FF) }, new byte[] { 0xCE, 0xCF });

    private static Script ScriptOf(int codePage) => codePage switch
    {
        1252 => Western,
        1251 => Cyrillic,
        1253 => Greek,
        _ => Latin,
    };

    private static readonly Dictionary<int, Dictionary<char, byte>> Maps = new();
    private static readonly object MapsLock = new();

    static EncodingFixer() => Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

    /// <summary>
    /// The code page with the most unambiguous matches among those the texts look broken for, or Windows-1252.
    /// </summary>
    public static SourceEncoding Detect(IEnumerable<string> texts)
    {
        var list = texts as IReadOnlyCollection<string> ?? texts.ToList();
        var best = Encodings[0];
        var bestCount = 0;
        foreach (var encoding in Encodings)
        {
            var analysis = Analyze(list, encoding.CodePage);
            if (analysis.IsBroken && analysis.Telling > bestCount)
            {
                best = encoding;
                bestCount = analysis.Telling;
            }
        }

        return best;
    }

    /// <summary>
    /// Whether the texts look broken for the code page: unambiguous matches (like <c>Ã©</c> or <c>â€™</c>), and
    /// matches make up a good share of the non-ASCII characters. Then ambiguous matches are fixed too.
    /// </summary>
    public static bool IsBroken(IEnumerable<string> texts, int codePage) => Analyze(texts, codePage).IsBroken;

    /// <summary>Number of unambiguous matches in the texts.</summary>
    public static int CountTelling(IEnumerable<string> texts, int codePage) => Analyze(texts, codePage).Telling;

    private readonly record struct Analysis(int Telling, int MatchedChars, int NonAsciiChars)
    {
        public bool IsBroken => Telling > 0 && MatchedChars >= NonAsciiChars * MinCoverage;
    }

    private static Analysis Analyze(IEnumerable<string> texts, int codePage)
    {
        var map = GetMap(codePage);
        var script = ScriptOf(codePage);
        int telling = 0, matched = 0, nonAscii = 0;
        foreach (var text in texts)
        {
            if (string.IsNullOrEmpty(text))
            {
                continue;
            }

            for (var i = 0; i < text.Length; i++)
            {
                if (text[i] >= 0x80 && map.ContainsKey(text[i]))
                {
                    nonAscii++;
                }

                if (TryMatch(text, i, map, script, out var match))
                {
                    telling += match.Telling ? 1 : 0;
                    matched += match.Length;
                    for (var k = 1; k < match.Length; k++)
                    {
                        nonAscii += text[i + k] >= 0x80 && map.ContainsKey(text[i + k]) ? 1 : 0;
                    }

                    i += match.Length - 1;
                }
            }
        }

        return new Analysis(telling, matched, nonAscii);
    }

    /// <summary>
    /// Fixes one text. With <paramref name="confirmed"/> (the subtitle <see cref="IsBroken"/>) ambiguous matches are
    /// fixed as well; without, only the unambiguous ones.
    /// Repeats for text that was broken more than once.
    /// </summary>
    public static string Fix(string text, int codePage, bool confirmed)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text;
        }

        var map = GetMap(codePage);
        var script = ScriptOf(codePage);
        for (var pass = 0; pass < MaxPasses; pass++)
        {
            // Text broken twice is broken once after the first pass; go again only when that is clear,
            // so correct text ("Всё") is never decoded a second time.
            if (pass > 0 && CountTelling(new[] { text }, codePage) == 0)
            {
                break;
            }

            var fixedText = FixOnce(text, map, script, confirmed);
            if (fixedText == text)
            {
                break;
            }

            text = fixedText;
        }

        return text;
    }

    private static string FixOnce(string text, Dictionary<char, byte> map, Script script, bool confirmed)
    {
        StringBuilder? sb = null;
        var copied = 0;
        for (var i = 0; i < text.Length; i++)
        {
            if (!TryMatch(text, i, map, script, out var match))
            {
                continue;
            }

            if (!match.Telling && !confirmed)
            {
                continue;
            }

            sb ??= new StringBuilder(text.Length);
            sb.Append(text, copied, i - copied).Append(match.Replacement);
            i += match.Length - 1;
            copied = i + 1;
        }

        if (sb == null)
        {
            return text;
        }

        sb.Append(text, copied, text.Length - copied);
        return sb.ToString();
    }

    private readonly record struct Match(int Length, string Replacement, bool Telling);

    private static bool TryMatch(string text, int i, Dictionary<char, byte> map, Script script, out Match match)
    {
        match = default;
        var c = text[i];
        if (c < 0x80 || !map.TryGetValue(c, out var lead))
        {
            return false;
        }

        if (lead >= 0xC2 && lead <= 0xF4 && TryDecode(text, i, lead, map, script, lowercased: false, out match))
        {
            return true;
        }

        // Lowercased after it broke: "Ã©" → "ã©". Only the leads of the code page's letters are tried, and the
        // result must be a letter.
        var upper = char.ToUpperInvariant(c);
        if (upper != c && map.TryGetValue(upper, out var upperLead) && Array.IndexOf(script.LowercasedLeads, upperLead) >= 0 &&
            TryDecode(text, i, upperLead, map, script, lowercased: true, out match) && char.IsLetter(match.Replacement, 0))
        {
            return true;
        }

        // "â€" with the third byte (0x9D, undefined in Windows-1252) lost: the closing quote ”.
        if (lead == 0xE2 && i + 1 < text.Length && map.TryGetValue(text[i + 1], out var second) && second == 0x80 &&
            (i + 2 >= text.Length || !map.TryGetValue(text[i + 2], out var third) || third < 0x80 || third > 0xBF))
        {
            match = new Match(2, "”", true);
            return true;
        }

        // "Ã " / "Â " where the second byte (A0, a no-break space) became a normal space: "à", " ".
        if (c is 'Â' or 'Ã' && lead is 0xC2 or 0xC3 && i + 1 < text.Length && text[i + 1] == ' ')
        {
            match = new Match(2, c == 'Â' ? " " : "à", false);
            return true;
        }

        return false;
    }

    private static bool TryDecode(string text, int i, byte lead, Dictionary<char, byte> map, Script script, bool lowercased, out Match match)
    {
        match = default;
        var continuationCount = lead >= 0xF0 ? 3 : lead >= 0xE0 ? 2 : 1;
        if (i + continuationCount >= text.Length)
        {
            return false;
        }

        var codePoint = lead & (0x3F >> continuationCount);
        for (var k = 1; k <= continuationCount; k++)
        {
            if (!map.TryGetValue(text[i + k], out var b) || b < 0x80 || b > 0xBF)
            {
                return false;
            }

            codePoint = (codePoint << 6) | (b & 0x3F);
        }

        string replacement;
        switch (continuationCount)
        {
            case 1:
                if (!script.TwoByteRanges.Any(r => codePoint >= r.From && codePoint <= r.To))
                {
                    return false; // C1 controls, or another script: real "ој" is not Greek "μ"
                }

                replacement = ((char)codePoint).ToString();
                break;
            case 2:
                if (codePoint == 0xFEFF)
                {
                    replacement = string.Empty; // byte order mark
                }
                else if (codePoint < 0x800 || codePoint >= 0x2E80 || codePoint is >= 0xD800 and <= 0xDFFF)
                {
                    // Not valid, or CJK and beyond: a real "é…" is far likelier than a lone CJK character.
                    return false;
                }
                else
                {
                    replacement = ((char)codePoint).ToString();
                }

                break;
            default:
                if (codePoint is < 0x1F000 or > 0x1FAFF)
                {
                    return false; // only emoji
                }

                replacement = char.ConvertFromUtf32(codePoint);
                break;
        }

        if (lowercased)
        {
            replacement = replacement.ToLowerInvariant();
        }

        var follower = text[i + 1];
        var telling = continuationCount > 1 ||
                      (!char.IsLetter(follower) || TellingLetters.Contains(follower)) &&
                      !AmbiguousFollowers.Contains(follower) && !char.IsWhiteSpace(follower);
        match = new Match(continuationCount + 1, replacement, telling);
        return true;
    }

    private static Dictionary<char, byte> GetMap(int codePage)
    {
        lock (MapsLock)
        {
            if (Maps.TryGetValue(codePage, out var cached))
            {
                return cached;
            }

            var encoding = Encoding.GetEncoding(codePage, EncoderFallback.ExceptionFallback, DecoderFallback.ReplacementFallback);
            var map = new Dictionary<char, byte>();
            for (var b = 0x80; b <= 0xFF; b++)
            {
                var decoded = encoding.GetString(new[] { (byte)b });
                if (decoded.Length == 1 && decoded[0] != '�')
                {
                    map.TryAdd(decoded[0], (byte)b);
                }
            }

            // Bytes the code page leaves undefined are often decoded as Latin-1 C1 controls (Windows-1252 0x9D "”").
            for (var b = 0x80; b <= 0x9F; b++)
            {
                map.TryAdd((char)b, (byte)b);
            }

            Maps[codePage] = map;
            return map;
        }
    }
}
