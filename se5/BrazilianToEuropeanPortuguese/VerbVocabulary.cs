using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;

// Conjugates regular -ar/-er/-ir verbs so one word-list entry (<Verb br="botar" pt="meter" />) covers every tense and person:
// botei → meti, botou → meteu, bote → meta, botando → metendo, botado → metido, bota-me → mete-me, botá-lo → metê-lo.
internal static class VerbConjugator
{
    // Endings per conjugation, in the same slot order so a Brazilian form maps to the European form with the same person/tense.
    private static readonly string[] Ar =
    {
        "ar", "o", "as", "a", "amos", "am",                     // infinitive, present
        "ei", "aste", "ou", "aram",                              // preterite
        "ava", "avas", "ávamos", "avam",                         // imperfect
        "arei", "arás", "ará", "aremos", "arão",                 // future
        "aria", "arias", "aríamos", "ariam",                     // conditional
        "e", "es", "emos", "em",                                 // present subjunctive
        "asse", "asses", "ássemos", "assem",                     // imperfect subjunctive
        "ares", "armos", "arem",                                 // future subjunctive / personal infinitive
        "ando", "ado", "ada", "ados", "adas",                    // gerund, participle
        "ara", "aras", "áramos",                                 // pluperfect
        "ámos",                                                  // European preterite "nós" (made by the "a gente" rule)
    };

    private static readonly string[] Er =
    {
        "er", "o", "es", "e", "emos", "em",
        "i", "este", "eu", "eram",
        "ia", "ias", "íamos", "iam",
        "erei", "erás", "erá", "eremos", "erão",
        "eria", "erias", "eríamos", "eriam",
        "a", "as", "amos", "am",
        "esse", "esses", "êssemos", "essem",
        "eres", "ermos", "erem",
        "endo", "ido", "ida", "idos", "idas",
        "era", "eras", "êramos",
        "emos",
    };

    private static readonly string[] Ir =
    {
        "ir", "o", "es", "e", "imos", "em",
        "i", "iste", "iu", "iram",
        "ia", "ias", "íamos", "iam",
        "irei", "irás", "irá", "iremos", "irão",
        "iria", "irias", "iríamos", "iriam",
        "a", "as", "amos", "am",
        "isse", "isses", "íssemos", "issem",
        "ires", "irmos", "irem",
        "indo", "ido", "ida", "idos", "idas",
        "ira", "iras", "íramos",
        "imos",
    };

    public static bool TryConjugate(string infinitive, out string[] forms)
    {
        forms = Array.Empty<string>();
        if (infinitive.Length < 3 || infinitive.Contains(' '))
        {
            return false;
        }

        var stem = infinitive[..^2];
        var endings = infinitive[^2..] switch { "ar" => Ar, "er" => Er, "ir" => Ir, _ => null };
        if (endings == null)
        {
            return false;
        }

        forms = endings.Select(e => Spell(stem, e, endings == Ar)).ToArray();
        return true;
    }

    // The accented infinitive used before -lo/-la (botá-lo, metê-lo, abri-lo).
    public static string CliticInfinitive(string infinitive) =>
        infinitive[..^2] + infinitive[^2..] switch { "ar" => "á", "er" => "ê", _ => "i" };

    // Keep the consonant sound when the ending changes: xingar → xingue, checar → cheque, começar → comece, aquecer → aqueço, fugir → fujo.
    private static string Spell(string stem, string ending, bool ar)
    {
        var first = ending.Length > 0 ? ending[0] : ' ';
        if (ar && first is 'e' or 'é' or 'ê')
        {
            if (stem.EndsWith('c')) return stem[..^1] + "qu" + ending;
            if (stem.EndsWith('g')) return stem + "u" + ending;
            if (stem.EndsWith('ç')) return stem[..^1] + "c" + ending;
        }
        else if (!ar && first is 'a' or 'á' or 'o')
        {
            if (stem.EndsWith('c')) return stem[..^1] + "ç" + ending;
            if (stem.EndsWith('g')) return stem[..^1] + "j" + ending;
        }

        return stem + ending;
    }
}

internal sealed record VerbEntry(string Br, string Pt, string? After, string? NotAfter, IReadOnlyCollection<string> Skip, IReadOnlyCollection<string> Nouns);

internal sealed class VerbVocabulary
{
    private const string NotWordBefore = @"(?<![\p{L}\p{M}\d_])(?<![\p{L}\p{M}\d_]-)";
    private const string NotWordAfter = @"(?![\p{L}\p{M}\d_-])";
    private const string Clitic = @"(?:me|te|se|nos|vos|lhe|lhes|o|a|os|as|lo|la|los|las|no|na)";

    // Mesoclisis tail: sequestrar-me-ia → raptar-me-ia, botá-lo-ei → metê-lo-ei.
    private const string FutureTail = @"(?:-(?:ei|ás|á|emos|eis|ão|ia|ias|íamos|íeis|iam))?";

    // Many verb forms are also nouns (a bota, o bote, o pulo, o cadastro); after a determiner they are left alone.
    // "Ele a botou" / "Eu o boto" are object pronouns, not articles.
    private const string DeterminerBefore =
        @"(?<!" + NotWordBefore + @"(?<!" + NotWordBefore + @"(?:eu|tu|ele|ela|eles|elas|você|vocês|nós|não|que|se|já|só|nunca|também|quem)\s+)(?:o|a|os|as|um|uma|uns|umas|do|da|dos|das|no|na|nos|nas|num|numa|ao|à|aos|às|pelo|pela|pelos|pelas|" +
        @"meu|minha|meus|minhas|teu|tua|teus|tuas|seu|sua|seus|suas|nosso|nossa|nossos|nossas|esse|essa|esses|essas|este|esta|estes|estas|" +
        @"aquele|aquela|desse|dessa|deste|desta|nesse|nessa|neste|nesta|de|par\s+de|outro|outra|cada|qualquer)\s+)";

    private readonly List<(Regex Regex, Dictionary<string, string> Map, string CliticInfinitive)> _rules = new();

    // Forms that are also nouns (botas, bote, posta) only convert right after a subject, a clitic or "não".
    private const string PronounBefore =
        @"(?<=" + NotWordBefore + @"(?:eu|tu|ele|ela|eles|elas|você|vocês|nós|não|me|te|se|nos|lhe|lhes|já|nunca|também|só)\s+)" +
        @"|(?<=\A[""“«']?|[.!?…]\s+|(?:\A|\n)[-–—]\s*)(?-i:(?=\p{Lu}))";

    // isBlocked: the user ignores the infinitive or has their own entry for it; isTaken: a word-list entry exists for that form.
    public VerbVocabulary(IEnumerable<VerbEntry> entries, Func<string, bool> isBlocked, Func<string, bool> isTaken)
    {
        foreach (var entry in entries)
        {
            if (isBlocked(entry.Br) ||
                !VerbConjugator.TryConjugate(entry.Br, out var br) ||
                !VerbConjugator.TryConjugate(entry.Pt, out var pt))
            {
                continue;
            }

            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var ambiguous = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < br.Length; i++)
            {
                if (map.TryGetValue(br[i], out var existing) && !existing.Equals(pt[i], StringComparison.OrdinalIgnoreCase))
                {
                    ambiguous.Add(br[i]);
                }

                map[br[i]] = pt[i];
            }

            foreach (var form in ambiguous.Concat(entry.Skip))
            {
                map.Remove(form);
            }

            var nounLike = entry.Nouns.Where(map.ContainsKey).ToList();

            // Word-list entries (built-in <Word> or the user's own list) win over generated forms.
            foreach (var form in map.Keys.Where(isTaken).ToList())
            {
                map.Remove(form);
            }

            var cliticInfinitive = VerbConjugator.CliticInfinitive(entry.Br);
            var context = (entry.After is { Length: > 0 } after ? @"(?=\s+(?:" + after + ")" + NotWordAfter + ")" : string.Empty) +
                (entry.NotAfter is { Length: > 0 } notAfter ? @"(?!\s+(?:" + notAfter + ")" + NotWordAfter + ")" : string.Empty);
            var infinitives = map.Keys.Where(f => f.EndsWith('r')).ToList();
            var others = map.Keys.Where(f => !f.EndsWith('r') && !nounLike.Contains(f, StringComparer.OrdinalIgnoreCase)).ToList();
            var ptCliticInfinitive = VerbConjugator.CliticInfinitive(entry.Pt);

            var parts = new List<string>
            {
                NotWordBefore + "(?<ci>" + Regex.Escape(cliticInfinitive) + @")(?=-l[oa]s?" + FutureTail + NotWordAfter + ")",
            };
            if (infinitives.Count > 0)
            {
                parts.Add(NotWordBefore + "(?<f>" + Alternation(infinitives) + ")(?=" + NotWordAfter + "|-" + Clitic + FutureTail + NotWordAfter + ")");
            }

            if (others.Count > 0)
            {
                parts.Add(DeterminerBefore + NotWordBefore + "(?<f>" + Alternation(others) + ")(?=" + NotWordAfter + "|-" + Clitic + NotWordAfter + ")");
            }

            if (nounLike.Count > 0)
            {
                parts.Add("(?:" + PronounBefore + ")(?<f>" + Alternation(nounLike) + ")(?=" + NotWordAfter + "|-" + Clitic + NotWordAfter + ")");
            }

            var pattern = "(?:" + string.Join("|", parts) + ")" + @"(?<cl>-" + Clitic + FutureTail + ")?" + NotWordAfter + context;
            _rules.Add((new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant), map, ptCliticInfinitive));
        }
    }

    public string Apply(string s)
    {
        foreach (var (regex, map, cliticInfinitive) in _rules)
        {
            // "curti-lo" (clitic infinitive) and "curti" (preterite) are the same letters for -ir verbs.
            s = regex.Replace(s, m => (m.Groups["ci"].Success
                ? CaseHelper.MatchCase(m.Groups["ci"].Value, cliticInfinitive)
                : CaseHelper.MatchCase(m.Groups["f"].Value, map[m.Groups["f"].Value])) + m.Groups["cl"].Value);
        }

        return s;
    }

    private static string Alternation(IEnumerable<string> keys) =>
        string.Join("|", keys.OrderByDescending(k => k.Length).Select(Regex.Escape));
}
