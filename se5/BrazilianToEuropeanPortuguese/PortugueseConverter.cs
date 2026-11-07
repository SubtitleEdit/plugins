using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;

/// <summary>
/// Converts Brazilian Portuguese subtitle text to European Portuguese.
/// Rule groups (see <see cref="ConversionOptions"/>) run in this order:
/// colloquial → você/tu → progressive (estar + gerúndio) → pronoun placement → vocabulary/spelling words →
/// spelling prefixes → spelling patterns (ô/ê accents, pre-1990 spellings).
/// HTML/ASS tags and "keep" phrases are masked so they are never altered.
/// </summary>
public sealed class PortugueseConverter
{
    private const RegexOptions Ci = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;
    private const RegexOptions Cs = RegexOptions.CultureInvariant;
    private const string W = @"[\p{L}\p{M}]";
    private const string NotWordBefore = @"(?<![\p{L}\p{M}\d_])(?<![\p{L}\p{M}\d_]-)";
    private const string NotWordAfter = @"(?![\p{L}\p{M}\d_-])";
    private const char PlaceholderBase = '\uE000';

    private static readonly Lazy<BuiltInList> BuiltIn = new(LoadBuiltIn);

    private static readonly Regex BringHimIn = new(@"(?<![\p{L}\p{M}])mande\s+entrar(?![\p{L}\p{M}])", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    private readonly ConversionOptions _options;
    private readonly Regex? _keepRegex;
    private readonly Regex? _ownOutputRegex;
    private readonly Regex? _wordRegex;
    private readonly Regex? _distantAgreementRegex;
    private readonly Dictionary<string, char> _changedGender = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, WordEntry> _words = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _localClothingWords = new(StringComparer.OrdinalIgnoreCase);
    private readonly Regex? _prefixRegex;
    private readonly Dictionary<string, string> _prefixes = new(StringComparer.OrdinalIgnoreCase);
    private readonly VerbVocabulary? _verbs;

    public PortugueseConverter(ConversionOptions? options = null, PortugueseLocalWordList? localWordList = null)
    {
        _options = options ?? ConversionOptions.Default;
        var builtIn = BuiltIn.Value;
        var ignored = new HashSet<string>(localWordList?.Ignored ?? new List<string>(), StringComparer.OrdinalIgnoreCase);

        foreach (var entry in builtIn.Words)
        {
            if (!ignored.Contains(entry.Br) && (entry.IsSpelling ? _options.Spelling : _options.Vocabulary))
            {
                _words[entry.Br] = entry;
            }
        }

        if (localWordList != null)
        {
            foreach (var (br, pt) in localWordList.Words)
            {
                _localClothingWords.Add(NormalizeSpaces(br));
                _localClothingWords.Add(NormalizeSpaces(pt));
                _words[NormalizeSpaces(br)] = new WordEntry(NormalizeSpaces(br), pt, false, null, null, null);
            }
        }

        foreach (var entry in builtIn.Prefixes)
        {
            if (!ignored.Contains(entry.Br) && (entry.IsSpelling ? _options.Spelling : _options.Vocabulary))
            {
                _prefixes[entry.Br] = entry.Pt;
            }
        }

        if (_options.Vocabulary)
        {
            var ownWords = new HashSet<string>(localWordList?.Words.Select(w => NormalizeSpaces(w.Item1)) ?? Enumerable.Empty<string>(), StringComparer.OrdinalIgnoreCase);
            _verbs = new VerbVocabulary(
                builtIn.Verbs,
                infinitive => ignored.Contains(infinitive) || ownWords.Contains(infinitive),
                form => _words.ContainsKey(form) || ignored.Contains(form));
        }

        var keep = builtIn.Keep.Concat(ignored).Where(k => k.Length > 0).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        if (keep.Count > 0)
        {
            _keepRegex = new Regex(NotWordBefore + "(?:" + Alternation(keep) + ")" + NotWordAfter, Ci);
        }

        // Words the converter itself outputs that are also Brazilian source keys ("terno" → "fato", but "fato" → "facto").
        // The first pass has already converted every original occurrence, so later passes must leave them alone.
        var ownOutputs = _words.Values
            .SelectMany(e => Regex.Matches(e.Pt, @"[\p{L}\p{M}]+(?:-[\p{L}\p{M}]+)*").Select(m => m.Value))
            .Where(word => _words.ContainsKey(word))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        if (ownOutputs.Count > 0)
        {
            _ownOutputRegex = new Regex(NotWordBefore + "(?:" + Alternation(ownOutputs) + ")" + NotWordAfter, Ci);
        }

        foreach (var entry in _words.Values.Where(e => e.TargetGender != null))
        {
            foreach (var word in new[] { entry.Pt, entry.Plural }.Where(w => w != null && !w.Contains(' ')))
            {
                _changedGender[word!] = entry.TargetGender!.Value;
            }
        }

        if (_changedGender.Count > 0)
        {
            // "a conduta de ventilação está entupido" → "entupida": the noun that changed gender is still the subject.
            _distantAgreementRegex = new Regex(
                NotWordBefore + "(?<n>" + Alternation(_changedGender.Keys) + ")" + NotWordAfter +
                @"(?<pre>\s+d(?:e|o|a|os|as)\s+[\p{L}\p{M}-]+(?:\s+[\p{L}\p{M}-]+)?\s+(?:(?:ainda|já|não|também|sempre|nunca)\s+)?" +
                @"(?:está|estava|estão|estavam|é|era|são|eram|foi|foram|ficou|ficaram|parece|parecem|continua|continuam)\s+)(?<chain>" + AgreeingAdjectiveChain + ")",
                Ci);
        }

        if (_words.Count > 0)
        {
            _wordRegex = CreateWordRegex(_words.Keys);
        }

        if (_prefixes.Count > 0)
        {
            _prefixRegex = new Regex(NotWordBefore + "(?<key>" + Alternation(_prefixes.Keys) + @")(?=\p{L})", Ci);
        }
    }

    public bool TryConvert(string text, out string converted, string? previousText = null, ReferenceHint? reference = null)
    {
        converted = Convert(text, previousText, reference);
        return !string.Equals(converted, text, StringComparison.Ordinal);
    }

    // A hyphenated o/a/os/as changes form after some verb endings: nasal "Põe-o" → "Põe-no", "dão-a" → "dão-na",
    // and a dropped -z/-s/-r gives "faz-o" → "fá-lo", "Traz-o" → "Trá-lo", "fizemos-o" → "fizemo-lo".
    internal static class CliticAllomorphs
    {
        private static readonly Regex Nasal = new(@"(?<![\p{L}\p{M}-])(?<v>\p{L}+(?:ão|õe|ãe|am|em))-(?<c>os|as|o|a)(?![\p{L}\p{M}-])", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        private static readonly Regex Consonant = new(@"(?<![\p{L}\p{M}-])(?<v>(?:des|re)?faz|(?:dis|con|re|ab|sub)?traz|diz|fez|fiz|quis|pôs|\p{L}+mos)-(?<c>os|as|o|a)(?![\p{L}\p{M}-])", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        private static readonly Dictionary<string, string> Stems = new(StringComparer.OrdinalIgnoreCase)
        {
            ["faz"] = "fá", ["traz"] = "trá", ["diz"] = "di", ["fez"] = "fê", ["fiz"] = "fi", ["quis"] = "qui", ["pôs"] = "pô",
        };

        // "ouví-lo" → "ouvi-lo": the accent only belongs on a hiatus ("possuí-lo", "atraí-lo").
        private static readonly Regex StrayAccent = new(@"(?<=\p{L}[b-df-hj-np-tv-zç])í-(?=l[oa]s?(?![\p{L}\p{M}-]))", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        public static string Apply(string s)
        {
            s = StrayAccent.Replace(s, "i-");
            s = Nasal.Replace(s, m => m.Groups["v"].Value + "-" + Prefix(m.Groups["c"].Value, "n"));
            return Consonant.Replace(s, m =>
            {
                var v = m.Groups["v"].Value;
                var key = Stems.Keys.FirstOrDefault(k => v.EndsWith(k, StringComparison.OrdinalIgnoreCase));
                var stem = key != null
                    ? v[..^key.Length] + CaseHelper.MatchCase(v[^key.Length..], Stems[key])
                    : v[..^1];
                return stem + "-" + Prefix(m.Groups["c"].Value, "l");
            });
        }

        private static string Prefix(string clitic, string consonant) =>
            (char.IsUpper(clitic[0]) ? consonant.ToUpperInvariant() : consonant) + clitic;
    }

    // "levou-o até o stock" / "lutou até o fim" / "até as 19h" → "até ao stock" / "até ao fim" / "até às 19h": a destination
    // or a limit takes "a" + article in Portugal. "Até o pai sabe" (= even) and "até o Farooq aparecer" are kept.
    internal static class UntilContraction
    {
        private const string Motion =
            @"(?:vai|vou|vais|vamos|vão|ia|ias|iam|íamos|foi|fui|foste|fomos|foram|irá|irão|ir|iremos|vá|vão|vem|venho|vens|vim|veio|vieram|vir|" +
            @"lev\p{L}*|traz\p{L}*|trag\p{L}*|trouxe\p{L}*|volt\p{L}*|regress\p{L}*|corr\p{L}*|desc[eiê]\p{L}*|sub\p{L}+|cheg\p{L}*|caminh\p{L}*|" +
            @"and[aeoá]\p{L}*|acompanh\p{L}*|segu\p{L}*|sig[ao]\p{L}*|conduz\p{L}*|rastre\p{L}*|cambale\p{L}*|arrast\p{L}*|empurr\p{L}*|" +
            @"estend\p{L}*|nad[aeo]\p{L}*|voa\p{L}*|voo|voou|dirig\p{L}*|deslo[cq]\p{L}*|daqui|dali|de\s+casa|caminho(?:\s+inteiro)?)";

        private static readonly Regex AfterMotion = new(
            @"(?<![\p{L}\p{M}-])(?<head>" + Motion + @"(?:-(?:me|te|o|a|os|as|lo|la|los|las|nos|se))?(?:\s+(?!até(?![\p{L}\p{M}]))[\p{L}\p{M}'-]+){0,3}?\s+)" +
            @"(?<ate>até)\s+(?<art>o|a|os|as)(?=\s+(?<rest>[^.!?…\n,;:]*))", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        private static readonly Regex Limit = new(
            @"(?<![\p{L}\p{M}-])(?<ate>até)\s+(?<art>o|a|os|as)(?=\s+(?:fim|final|morte|limite|meio-dia|meia-noite|próxim[oa]s?|últim[oa]|talo|osso|ossos|\d))",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        public static string Apply(string s)
        {
            s = Limit.Replace(s, Contract);
            return AfterMotion.Replace(s, m =>
            {
                // "vai até o Farooq aparecer": the noun is the subject of the infinitive, no contraction.
                var rest = m.Groups["rest"].Value;
                if (Regex.IsMatch(rest, @"^(?:[\p{L}\p{M}'-]+\s+){1,3}?(?:\p{Ll}+(?:ar|er|ir|arem|erem|irem))(?![\p{L}\p{M}])") &&
                    !Regex.IsMatch(rest, @"^(?:de|do|da)\s", RegexOptions.IgnoreCase))
                {
                    return m.Value;
                }

                return m.Groups["head"].Value + Contract(m);
            });
        }

        private static string Contract(Match m)
        {
            var art = m.Groups["art"].Value;
            var lower = art.ToLowerInvariant();
            var contracted = lower switch { "o" => "ao", "os" => "aos", "a" => "à", _ => "às" };
            return m.Groups["ate"].Value + " " + (CaseHelper.IsAllUpper(art) && art.Length > 0 && char.IsUpper(art[0]) && m.Groups["ate"].Value.All(c => !char.IsLetter(c) || char.IsUpper(c)) ? contracted.ToUpperInvariant() : contracted);
        }
    }

    // A preposition or article typed twice, often across the line break: "o merecimento de\nde serem" → "o mérito\nde serem".
    // "a" and "o" are left out ("de A a Z"), and so is "para" ("ele para para pensar").
    internal static class DoubledWords
    {
        private const string Words = "de|da|do|das|dos|em|no|na|nos|nas|com|por|pelo|pela|pelos|pelas|os|as|um|uma|que";

        private static readonly Regex SameLine = new(
            @"(?<![\p{L}\p{M}-])(?<w>" + Words + @")[ \t]+\k<w>(?![\p{L}\p{M}-])", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        private static readonly Regex AcrossLines = new(
            @"(?<=[\p{L}\p{M}])[ \t]+(?<w>" + Words + @")[ \t]*(?<nl>\r?\n)[ \t]*(?<w2>\k<w>)(?![\p{L}\p{M}-])", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        public static string Apply(string s)
        {
            s = AcrossLines.Replace(s, m => m.Groups["nl"].Value + m.Groups["w2"].Value);
            return SameLine.Replace(s, m => m.Groups["w"].Value);
        }
    }

    internal static class MissingInfinitiveEnding
    {
        // Only unambiguous ir auxiliaries: "quero ajuda" is a noun, and "foi luta" may use ser.
        private static readonly Regex Pattern = new(
            NotWordBefore + @"(?<pre>(?:vou|vais|vai|vamos|vão|ia|ias|íamos|iam|irei|irás|irá|iremos|irão|iria|irias|iríamos|iriam)\s+" +
            @"(?:(?:já|ainda|também|só|sempre|mesmo|realmente|agora|nunca|não|mais|me|te|se|lhe|lhes|nos|vos)\s+){0,3})" +
            @"(?<verb>[\p{L}\p{M}]+)" + NotWordAfter, Ci);

        public static string Apply(string text) => Pattern.Replace(text, m =>
            PortugueseVerbs.TryRestoreInfinitiveEnding(m.Groups["verb"].Value, out var infinitive)
                ? m.Groups["pre"].Value + infinitive
                : m.Value);

        public static IEnumerable<(string Phrase, string Correction)> Find(string text)
        {
            foreach (Match m in Pattern.Matches(text))
            {
                if (PortugueseVerbs.TryRestoreInfinitiveEnding(m.Groups["verb"].Value, out var infinitive))
                {
                    yield return (m.Value, m.Groups["pre"].Value + infinitive);
                }
            }
        }
    }

    // "Eu nunca sair da linha" is a bad translation: a subject needs a conjugated verb. The English reference gives the tense
    // ("I never stepped" → "Eu nunca saí", "I never step" → "Eu nunca saio"); without it the line is only flagged for review.
    internal static class BareInfinitive
    {
        internal static readonly Regex Pattern = new(
            @"(?<pre>(?:^|[.!?…:;,]\s+|\n|(?<![\p{L}\p{M}])[-–—]\s*|[\uE000-\uF8FF]\s*|(?<![\p{L}\p{M}])(?:e|mas)\s+)[""“«']?(?:<i>)?)" +
            @"(?<subj>eu|ele|ela|nós|eles|elas)(?<adv>(?:\s+(?:nunca|não|já|também|só|sempre|ainda|realmente|apenas|jamais|nem)){0,2})\s+" +
            @"(?<inf>\p{Ll}+(?:ar|er|ir)|pôr)" + NotWordAfter + @"(?![^.!?\n]*\?)", Ci);

        private static readonly HashSet<string> NotVerbs = new(StringComparer.OrdinalIgnoreCase)
        {
            "sequer", "qualquer", "mulher", "colher", "lugar", "par", "mar", "lar", "bar", "prazer", "talher", "poder", "dever", "olhar",
            // Already conjugated: "quer", and future subjunctives ("quando eu estiver").
            "quer", "requer", "estiver", "tiver", "mantiver", "obtiver", "detiver", "contiver", "puder", "fizer", "desfizer", "souber",
            "quiser", "vier", "der", "disser", "trouxer", "houver", "couber", "puser", "propuser", "compuser", "supuser", "for",
        };

        // Preterite eu/ele/nós/eles, then present eu/ele/nós/eles.
        private static readonly Dictionary<string, string[]> Irregular = new(StringComparer.OrdinalIgnoreCase)
        {
            ["ser"] = new[] { "fui", "foi", "fomos", "foram", "sou", "é", "somos", "são" },
            ["ir"] = new[] { "fui", "foi", "fomos", "foram", "vou", "vai", "vamos", "vão" },
            ["estar"] = new[] { "estive", "esteve", "estivemos", "estiveram", "estou", "está", "estamos", "estão" },
            ["ter"] = new[] { "tive", "teve", "tivemos", "tiveram", "tenho", "tem", "temos", "têm" },
            ["fazer"] = new[] { "fiz", "fez", "fizemos", "fizeram", "faço", "faz", "fazemos", "fazem" },
            ["dizer"] = new[] { "disse", "disse", "dissemos", "disseram", "digo", "diz", "dizemos", "dizem" },
            ["ver"] = new[] { "vi", "viu", "vimos", "viram", "vejo", "vê", "vemos", "veem" },
            ["vir"] = new[] { "vim", "veio", "viemos", "vieram", "venho", "vem", "vimos", "vêm" },
            ["querer"] = new[] { "quis", "quis", "quisemos", "quiseram", "quero", "quer", "queremos", "querem" },
            ["saber"] = new[] { "soube", "soube", "soubemos", "souberam", "sei", "sabe", "sabemos", "sabem" },
            ["pôr"] = new[] { "pus", "pôs", "pusemos", "puseram", "ponho", "põe", "pomos", "põem" },
            ["trazer"] = new[] { "trouxe", "trouxe", "trouxemos", "trouxeram", "trago", "traz", "trazemos", "trazem" },
            ["dar"] = new[] { "dei", "deu", "demos", "deram", "dou", "dá", "damos", "dão" },
            ["ler"] = new[] { "li", "leu", "lemos", "leram", "leio", "lê", "lemos", "leem" },
            ["crer"] = new[] { "cri", "creu", "cremos", "creram", "creio", "crê", "cremos", "creem" },
            ["ouvir"] = new[] { "ouvi", "ouviu", "ouvimos", "ouviram", "ouço", "ouve", "ouvimos", "ouvem" },
            ["pedir"] = new[] { "pedi", "pediu", "pedimos", "pediram", "peço", "pede", "pedimos", "pedem" },
            ["perder"] = new[] { "perdi", "perdeu", "perdemos", "perderam", "perco", "perde", "perdemos", "perdem" },
            ["caber"] = new[] { "coube", "coube", "coubemos", "couberam", "caibo", "cabe", "cabemos", "cabem" },
            ["valer"] = new[] { "vali", "valeu", "valemos", "valeram", "valho", "vale", "valemos", "valem" },
            ["dormir"] = new[] { "dormi", "dormiu", "dormimos", "dormiram", "durmo", "dorme", "dormimos", "dormem" },
            ["subir"] = new[] { "subi", "subiu", "subimos", "subiram", "subo", "sobe", "subimos", "sobem" },
            ["fugir"] = new[] { "fugi", "fugiu", "fugimos", "fugiram", "fujo", "foge", "fugimos", "fogem" },
            ["odiar"] = new[] { "odiei", "odiou", "odiámos", "odiaram", "odeio", "odeia", "odiamos", "odeiam" },
        };

        // -ir verbs that change e → i in "eu" of the present: sentir → sinto, seguir → sigo.
        private static readonly Regex EToI = new(@"(?:sentir|mentir|seguir|servir|vestir|despir|repetir|preferir|sugerir|ferir|competir|conseguir|perseguir|prosseguir|divertir|convertir|advertir|investir|consentir|ressentir|pressentir|digerir|inserir|interferir|referir|transferir|conferir)$", Ci);

        public static string Apply(string s, bool past) => Pattern.Replace(s, m =>
        {
            var form = Starts(m) ? Conjugate(m.Groups["inf"].Value, Person(m.Groups["subj"].Value), past) : null;
            return form == null
                ? m.Value
                : m.Groups["pre"].Value + m.Groups["subj"].Value + m.Groups["adv"].Value + " " + CaseHelper.MatchCase(m.Groups["inf"].Value, form);
        });

        /// <summary>The bare infinitive in a line, if any, for a review note when there is no English tense to fix it.</summary>
        public static (string Infinitive, string? Past, string? Present)? Find(string s)
        {
            foreach (Match m in Pattern.Matches(s))
            {
                if (!Starts(m)) continue;
                var person = Person(m.Groups["subj"].Value);
                var inf = m.Groups["inf"].Value;
                var past = Conjugate(inf, person, true);
                if (past != null) return (inf, past, Conjugate(inf, person, false));
            }
            return null;
        }

        // A cue starting in lowercase continues the previous one ("…não admira / ele sair no momento"): leave it.
        private static bool Starts(Match m) => m.Index > 0 || m.Groups["pre"].Length > 0 || !char.IsLower(m.Groups["subj"].Value[0]);

        private static int Person(string subject) => subject.ToLowerInvariant() switch
        {
            "eu" => 0, "nós" => 2, "eles" or "elas" => 3, _ => 1,
        };

        internal static string? Conjugate(string infinitive, int person, bool past)
        {
            var inf = infinitive.ToLowerInvariant();
            if (NotVerbs.Contains(inf) || !(Irregular.ContainsKey(inf) || TuImperative.IsInfinitive(inf)))
            {
                return null;
            }

            if (Irregular.TryGetValue(inf, out var forms))
            {
                return forms[person + (past ? 0 : 4)];
            }

            var stem = inf[..^2];
            var type = inf[^2..];
            // sair, cair, trair, construir, possuir: the "i" of the ending takes an accent (saí, saímos) and the present is sai/saem.
            if (type == "ir" && stem.Length > 0 && "aeou".Contains(stem[^1]))
            {
                return past
                    ? new[] { stem + "í", stem + "iu", stem + "ímos", stem + "íram" }[person]
                    : new[] { stem + "io", stem + "i", stem + "ímos", stem + "em" }[person];
            }

            if (!VerbConjugator.TryConjugate(inf, out var regular))
            {
                return null;
            }

            if (past)
            {
                // European "nós" preterite: falámos, comemos, partimos.
                return person switch { 0 => regular[6], 1 => regular[8], 2 => regular[42], _ => regular[9] };
            }

            if (person == 0 && type == "ir" && EToI.IsMatch(inf))
            {
                var i = stem.LastIndexOf('e');
                return VerbConjugator.TryConjugate(stem[..i] + "i" + stem[(i + 1)..] + "ir", out var changed) ? changed[1] : null;
            }

            return person switch { 0 => regular[1], 1 => regular[3], 2 => regular[4], _ => regular[5] };
        }
    }

    public string Convert(string text, string? previousText = null, ReferenceHint? reference = null,
        ICollection<RuleChange>? trace = null) => ConvertCore(text, previousText, reference, trace, false);

    internal string ConvertContinuation(string text, string? previousText, ReferenceHint? reference,
        ICollection<RuleChange> trace) => ConvertCore(text, previousText, reference, trace, true);

    private string ConvertCore(string text, string? previousText, ReferenceHint? reference,
        ICollection<RuleChange>? trace, bool protectOutputs)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return text;
        }

        // One rule can produce input for an earlier rule ("Me coloque na espera" → "Coloque-me na espera"
        // → "Coloque-me em espera"), so repeat until stable; running the plugin twice then changes nothing.
        var current = text;
        for (var pass = 0; pass < MaxPasses; pass++)
        {
            var next = ConvertOnce(current, previousText, reference, laterPass: pass > 0 || protectOutputs, trace);
            if (string.Equals(next, current, StringComparison.Ordinal))
            {
                break;
            }

            current = next;
        }

        if (reference is { Officer: true } && _options.Vocabulary) current = OfficerAsAgente(current);
        var restored = RestoreLineBreaks(text, current);
        var balanced = _options.RuleEnabled("line-break-repair") ? BalanceLineBreaks(text, restored) : restored;
        if (balanced != current) trace?.Add(new RuleChange("line-break-repair", "Restore/balance original line breaks"));
        return balanced;
    }

    private static readonly Regex PoliceOfficer = new(
        NotWordBefore + @"(?:(?<d>o|os|um|uns|esse|este|aquele|nosso|meu|teu|seu|a|as|uma|essa|esta|aquela|nossa|minha|tua|sua)\s+)?(?<w>polícias?|policiais?)" + NotWordAfter, Ci);

    /// <summary>With "officer" in the English, a person called "polícia/policial" is an "agente" ("a polícia" as the force is untouched).</summary>
    private static string OfficerAsAgente(string s) => PoliceOfficer.Replace(s, m =>
    {
        var d = m.Groups["d"].Value;
        var w = m.Groups["w"].Value.ToLowerInvariant();
        var plural = w.EndsWith('s');
        var agent = CaseHelper.MatchCase(m.Groups["w"].Value, plural ? "agentes" : "agente");
        if (d.Length == 0) return w.StartsWith("polícia") && !w.StartsWith("policiai") && w != "polícias" ? m.Value : agent;
        var article = d.ToLowerInvariant();
        // "a polícia" (feminine, singular) is the force; "o/um polícia" and plurals are people.
        if (w == "polícia" && article is "a" or "uma" or "essa" or "esta" or "aquela" or "nossa" or "minha" or "tua" or "sua") return m.Value;
        return d + " " + agent;
    });

    private const int MaxLineLength = 43;

    // Words that cannot end a subtitle line: a pronoun before its verb, a preposition or an article before its noun.
    private static readonly HashSet<string> DanglingWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "me", "te", "se", "lhe", "lhes", "nos", "vos", "o", "a", "os", "as", "um", "uma", "para", "de", "do", "da", "dos", "das",
        "em", "no", "na", "nos", "nas", "ao", "à", "aos", "às", "com", "por", "pelo", "pela", "num", "numa",
    };

    /// <summary>
    /// A rule that moves a word ("ligou\npara ele" → "lhe\nligou", "disse\npara ela" → "disse-lhe para\n") can leave a pronoun,
    /// preposition or article at the end of a line. The word goes down to the next line, or words from the next line come up,
    /// whichever keeps the longest line shorter. Lines that already ended like that in the original are the translator's choice.
    /// </summary>
    internal static string BalanceLineBreaks(string original, string converted)
    {
        if (!converted.Contains('\n') || converted.IndexOfAny(['<', '{']) >= 0)
        {
            return converted;
        }

        var newline = converted.Contains("\r\n") ? "\r\n" : "\n";
        var originalLines = original.Replace("\r\n", "\n").Split('\n');
        var lines = converted.Replace("\r\n", "\n").Split('\n');
        if (lines.Length != originalLines.Length)
        {
            return converted;
        }

        var changed = false;
        for (var i = 0; i < lines.Length - 1; i++)
        {
            if (!EndsDangling(lines[i]) || EndsDangling(originalLines[i]) || Regex.IsMatch(lines[i + 1], @"^\s*[-–—]"))
            {
                continue;
            }

            var limit = Math.Max(MaxLineLength, originalLines.Max(l => l.Length));
            var top = lines[i].TrimEnd().Split(' ').ToList();
            var bottom = lines[i + 1].TrimStart().Split(' ').ToList();

            var downTop = top.ToList();
            var downBottom = bottom.ToList();
            while (downTop.Count > 1 && DanglingWords.Contains(downTop[^1]))
            {
                downBottom.Insert(0, downTop[^1]);
                downTop.RemoveAt(downTop.Count - 1);
            }

            var upTop = top.ToList();
            var upBottom = bottom.ToList();
            while (upBottom.Count > 1 && DanglingWords.Contains(upTop[^1]))
            {
                upTop.Add(upBottom[0]);
                upBottom.RemoveAt(0);
            }

            var down = (string.Join(' ', downTop), string.Join(' ', downBottom));
            var up = (string.Join(' ', upTop), string.Join(' ', upBottom));
            var downOk = downTop.Count > 0 && !DanglingWords.Contains(downTop[^1]);
            var upOk = !DanglingWords.Contains(upTop[^1]);
            var downLength = Math.Max(down.Item1.Length, down.Item2.Length);
            var upLength = Math.Max(up.Item1.Length, up.Item2.Length);
            // Moving the word down keeps the break where the translator put it; words only come up when the next line gets too long.
            // "Nunca\nnos contaste sobre isso" leaves one word alone: "Nunca nos contaste\nsobre isso" reads better.
            var lonely = downTop.Count == 1 && upOk && upBottom.Count >= 2 && upLength <= limit;
            (string, string)? best = downOk && !lonely && (downLength <= limit || !upOk || downLength <= upLength) ? down : upOk ? up : null;
            if (best == null || Math.Max(best.Value.Item1.Length, best.Value.Item2.Length) > limit)
            {
                continue;
            }

            lines[i] = best.Value.Item1;
            lines[i + 1] = best.Value.Item2;
            changed = true;
        }

        return changed ? string.Join(newline, lines) : converted;
    }

    private static bool EndsDangling(string line)
    {
        var m = Regex.Match(line, @"(?<![\p{L}\p{M}-])(?<w>\p{L}+)\s*$");
        return m.Success && DanglingWords.Contains(m.Groups["w"].Value);
    }

    /// <summary>
    /// Rules that rewrite a phrase ("cheiro de" → "cheiro a", "por que" → "porque") may swallow the line break inside it;
    /// a subtitle line must not be merged, so the lost break goes back to the space nearest to where it was.
    /// </summary>
    internal static string RestoreLineBreaks(string original, string converted)
    {
        var newline = original.Contains("\r\n") ? "\r\n" : "\n";
        var originalLines = original.Replace("\r\n", "\n").Split('\n');
        var lines = converted.Replace("\r\n", "\n").Split('\n').ToList();
        if (lines.Count >= originalLines.Length)
        {
            return converted;
        }

        while (lines.Count < originalLines.Length)
        {
            var i = lines.Count == 1 ? 0 : lines.IndexOf(lines.OrderByDescending(l => l.Length).First());
            var line = lines[i];
            var ratio = lines.Count == 1 && originalLines.Length == 2
                ? (double)originalLines[0].Length / Math.Max(1, originalLines[0].Length + originalLines[1].Length)
                : 0.5;
            var target = (int)Math.Round(line.Length * ratio);
            var best = -1;
            for (var p = 0; p < line.Length; p++)
            {
                if (line[p] == ' ' && !IsInsideTag(line, p) && (best < 0 || Math.Abs(p - target) < Math.Abs(best - target)))
                {
                    best = p;
                }
            }

            if (best < 0)
            {
                return converted;
            }

            lines[i] = line[..best];
            lines.Insert(i + 1, line[(best + 1)..]);
        }

        return string.Join(newline, lines);
    }

    private static bool IsInsideTag(string line, int position)
    {
        var open = line.LastIndexOfAny(new[] { '<', '{' }, position);
        if (open < 0)
        {
            return false;
        }

        var close = line.IndexOfAny(new[] { '>', '}' }, open);
        return close > position;
    }

    private const int MaxPasses = 4;

    private string ConvertOnce(string text, string? previousText, ReferenceHint? reference, bool laterPass,
        ICollection<RuleChange>? trace)
    {
        var masks = new List<string>();
        var s = Mask(text, masks, laterPass);
        var checkpoint = s;
        void Track(string id, string description)
        {
            if (!_options.RuleEnabled(id)) s = checkpoint;
            else if (s != checkpoint)
                trace?.Add(new RuleChange(id, description, id switch
                {
                    "missing-infinitive" => "Dictionary-validated",
                    "word-list" or "orthography" => "Dictionary/rule-based",
                    "address" or "context-phrases" or "inclusive-hearing" or "verb-vocabulary" or "reference-clitics" or
                    "construction-vocabulary" or "subject-continuity" or "reference-idioms" =>
                        "Context-sensitive; reference-assisted when available",
                    _ => "Rule-based",
                }));
            checkpoint = s;
        }
        if (_options.Colloquial && reference is { InclusiveContinuation: true })
        {
            s = InclusiveContinuation.Replace(s, m =>
                m.Groups["pre"].Value + CaseHelper.MatchCase(m.Groups["v"].Value, m.Groups["v"].Value.ToLowerInvariant() switch
                { "acha" => "achamos", "pensa" => "pensamos", _ => "imaginamos" }) +
                m.Groups["que"].Value + "vá" + m.Groups["event"].Value + "connosco");
        }
        if (_options.Vocabulary && _options.Pronouns && reference is { CatchUpLater: true })
        {
            s = CatchUpLater.Replace(s, m => m.Groups["pre"].Value + CaseHelper.MatchCase(m.Groups["v"].Value,
                "depois " + (m.Groups["p"].Value.Equals("vocês", StringComparison.OrdinalIgnoreCase) ? "apanho-vos" : "apanho-te")));
        }
        if (_options.Pronouns)
        {
            s = RelativeFalar.Replace(s, m =>
                m.Groups["pre"].Value + (CaseHelper.IsAllUpper(m.Value) ? "DE " : "de ") + m.Groups["que"].Value +
                m.Groups["sp"].Value + CaseHelper.MatchCase(m.Groups["p"].Value,
                    m.Groups["p"].Value.ToLowerInvariant() switch { "vocês" => "vos", "ele" or "ela" or "você" => "lhe", "eles" or "elas" => "lhes", "nós" => "nos", "mim" => "me", _ => "te" }) +
                " " + m.Groups["v"].Value);
        }

        if (_options.Colloquial)
        {
            s = ColloquialRules.Apply(s);
            if (reference is { NeverAnyObject: true })
            {
                s = NeverIndefiniteObject.Replace(s, m =>
                    PortugueseVerbs.IsKnownVerbForm(m.Groups["v"].Value) ||
                    PortugueseVerbs.IsSecondPersonForm(m.Groups["v"].Value) ||
                    PortugueseVerbs.TryGuessThirdToSecond(m.Groups["v"].Value, out _)
                        ? m.Groups["pre"].Value + m.Groups["v"].Value + m.Groups["sp"].Value +
                          CaseHelper.MatchCase(m.Groups["det"].Value, m.Groups["det"].Value.EndsWith("a", StringComparison.OrdinalIgnoreCase) ? "uma" : "um")
                        : m.Value);
            }
        }

        Track("context-phrases", "Context/relative-clause and colloquial phrases");
        s = MissingInfinitiveEnding.Apply(s);
        Track("missing-infinitive", "Dictionary-validated infinitive after ir");

        if (_options.Colloquial)
        {
            if (_options.Vocabulary)
            {
                s = HearingVocabulary.Apply(s, reference is { HearAbout: true });
            }
            s = WeRules.Apply(s);
        }
        else if (_options.Vocabulary)
        {
            s = HearingVocabulary.Apply(s, reference is { HearAbout: true });
        }
        Track("inclusive-hearing", "Inclusive nós and hearing vocabulary");

        var informalConstructions = _options.YouToTu && (reference is { ForceInformal: true } ||
            reference is not { FormalAddress: true } && !YouToTuRules.IsFormal(s));
        s = ClothingConstructions.PeopleAndClothes(s, reference, _options.Vocabulary, _options.Pronouns);
        Track("clothing-complements", "Person-to-ground and owner-aware clothing complements");
        if (_options.Pronouns) s = ConversionConstructions.Government(s);
        Track("verb-government", "Recollection and dating complements");
        if (_options.Vocabulary)
            s = ConversionConstructions.Vocabulary(s, _options.Pronouns, informalConstructions);
        Track("construction-vocabulary", "Complement-qualified vocabulary senses");
        if (_options.YouToTu) s = ConversionConstructions.PersonalInfinitives(s, informalConstructions);
        Track("personal-infinitives", "Explicit-subject personal infinitives");
        if (_options.Pronouns) s = ConversionConstructions.FamilySubjects(s, reference);
        Track("family-subjects", "Reference-confirmed family subjects with definite articles");
        if (_options.Pronouns && informalConstructions)
            s = ConversionConstructions.ReferenceObjectClitics(s, reference, _options.Vocabulary);
        Track("reference-object-clitics", "Reference-confirmed informal past direct objects");
        if (informalConstructions) s = ConversionConstructions.Clauses(s, reference);
        Track("subject-continuity", "Controlled and reference-confirmed clause subjects");
        if (_options.Colloquial && _options.Progressive && _options.Pronouns)
            s = ConversionConstructions.HappeningQuestions(s, reference);
        Track("happening-questions", "Impersonal questions about ongoing events");
        if (_options.YouToTu && reference is not { FormalAddress: true } && !YouToTuRules.IsFormal(s))
            s = ConversionConstructions.WaitUntil(s, reference);
        Track("wait-until", "Reference-confirmed personal infinitives after esperar até");
        if (_options.Vocabulary) s = ConversionConstructions.AcquisitionBeneficiaries(s, reference);
        Track("acquisition-beneficiaries", "Para for reference-confirmed beneficiaries rather than substitution");
        if (_options.Vocabulary && _options.Colloquial) s = ConversionConstructions.ExplanatoryIntroductions(s, reference);
        Track("explanatory-introductions", "Reference-confirmed explanatory thing is constructions");
        if (_options.Vocabulary) s = ConversionConstructions.EmergencyConstructions(s, reference);
        Track("emergency-constructions", "Reference-confirmed incident response and medical arrest");
        if (_options.Vocabulary) s = ConversionConstructions.TravelStops(s, reference);
        Track("travel-stops", "Reference-confirmed stopping places and brief journey stops");
        if (_options.Colloquial && _options.Vocabulary) s = ConversionConstructions.Idioms(s, reference);
        Track("reference-idioms", "Brazilian expressions and English-confirmed literal idioms");

        if (_options.Colloquial && _options.Vocabulary) s = ConversionConstructions.SharedResidence(s, reference);
        Track("shared-residence", "English-confirmed shared residence instead of neighbour paraphrases");

        if (_options.YouToTu)
        {
            s = YouToTuRules.Apply(s, previousText, reference, _options.Pronouns && _options.Colloquial);
        }
        Track("address", "Address, conjugation and possessives");

        if (_options.Progressive)
        {
            s = ProgressiveRules.Apply(s);
        }
        Track("progressive", "Progressive construction");

        if (_options.Pronouns)
        {
            s = PronounRules.Apply(s);
        }
        Track("pronouns", "Pronoun placement");

        if (_options.Vocabulary)
        {
            s = CatchTransport.Replace(s, m =>
                CaseHelper.MatchCase(m.Groups["stem"].Value, "apanh") + m.Groups["end"].Value);
        }

        if (_options.Vocabulary)
        {
            s = Drive.Replace(s, m => CaseHelper.MatchCase(m.Value, DriveForms[m.Value.ToLowerInvariant()]));
        }

        if (_options.Vocabulary)
        {
            s = Grab.Apply(s);
            s = VocabularyRules.Apply(s);
            s = VocabularyRules.ReplaceShutUp(s, _options.YouToTu && reference is not { FormalAddress: true });
            s = VocabularyRules.ReplaceBareReflexive(s);
            s = DepartingBarca.Replace(s, ReplaceDepartingBarca);
            s = _verbs?.Apply(s) ?? s;
            s = YoungPerson.Replace(s, m =>
                _words.TryGetValue(m.Groups["noun"].Value, out var entry) && entry.Pt.StartsWith("miúd", StringComparison.OrdinalIgnoreCase)
                    ? ReplaceYoungPerson(m, reference)
                    : m.Value);
        }
        Track("verb-vocabulary", "Contextual vocabulary and verb inflections");

        if (_wordRegex != null)
        {
            s = _wordRegex.Replace(s, ReplaceWord);
        }
        if (_options.Vocabulary) s = ClothingConstructions.PluralUnderwear(s);

        if (_distantAgreementRegex != null)
        {
            s = _distantAgreementRegex.Replace(s, m =>
                m.Groups["n"].Value + m.Groups["pre"].Value + Regex.Replace(m.Groups["chain"].Value, @"[\p{L}]+",
                    a => NotAgreeing.Contains(a.Value) ? a.Value : SwapAdjectiveGender(a.Value, _changedGender[m.Groups["n"].Value])));
        }
        Track("word-list", "Word-list spelling/vocabulary and gender agreement");
        if (_options.Vocabulary) s = ClothingConstructions.Sportswear(s, reference,
            key => !_localClothingWords.Contains(NormalizeSpaces(key)));
        Track("sports-clothing", "English-confirmed specialist clothing with gender agreement");

        if (_options.Pronouns)
        {
            s = FutureSubjunctiveClitic.Replace(s, ReplaceFutureSubjunctiveClitic);
        }

        if (reference is { This: true })
        {
            s = NearDemonstrative.Replace(s, m => CaseHelper.MatchCase(m.Value, NearDemonstratives[m.Value]));
        }
        else if (ReferentialThat.IsMatch(reference?.EnglishText ?? "") &&
                 !Regex.IsMatch(reference?.EnglishText ?? "", @"\b(?:this|these|those)\b", Ci))
        {
            s = DistalDemonstrative.Replace(s, m => CaseHelper.MatchCase(m.Value, DistalDemonstratives[m.Value]));
        }
        Track("reference-clitics", "Future-subjunctive clitics and reference demonstratives");

        if (_options.Progressive && _wordRegex != null)
        {
            // Word-list replacements can produce new gerunds ("rolando" → "acontecendo").
            s = ProgressiveRules.Apply(s);
        }

        if (_options.YouToTu && reference is not { FormalAddress: true })
        {
            s = BringHimIn.Replace(s, m => CaseHelper.MatchCase(m.Value, "mando-o entrar"));
        }

        if (_options.YouToTu)
        {
            s = TuImperative.Apply(s, reference is { IgnorePreviousAddress: true } ? null : previousText,
                reference is { FormalAddress: true }, reference is { ForceInformal: true });
        }
        Track("progressive-command", "Progressive follow-up and imperative address");

        if (_options.Progressive)
        {
            s = ImmediateProgressive.Apply(s);
        }

        if (_options.Pronouns)
        {
            s = PronominalVerbs.Apply(s, _options.YouToTu && reference is not { FormalAddress: true });
            s = CliticAllomorphs.Apply(s);
        }
        Track("reflexive-clitics", "Reflexives and clitic allomorphs");

        if (_options.YouToTu && _options.Colloquial && reference is not { FormalAddress: true } &&
            reference is not { IgnorePreviousAddress: true })
            s = ConversionConstructions.ActionQuestions(s, previousText, reference, _options.Pronouns);
        Track("action-questions", "Reference-confirmed self-directed action questions");

        if (_options.Colloquial && _options.Vocabulary)
            s = ConversionConstructions.SpokenInformation(s, previousText, reference);
        Track("spoken-information", "English-confirmed figurative give me requests about information");

        if (_options.Colloquial) s = ConversionConstructions.NaturalWordOrder(s, reference);
        Track("natural-word-order", "Habitual sempre after lexical verbs and recoverable progressive subjects");
        if (_options.Colloquial && _options.Pronouns) s = ConversionConstructions.NaturalDialogue(s, reference, _options.Vocabulary);
        Track("natural-dialogue", "Departure commands and reference-confirmed departure slang and certainty questions");

        if (_options.Colloquial) s = ConversionConstructions.PhysicalDescriptions(s);
        Track("physical-descriptions", "Com in person height and weight descriptions");
        if (_options.Pronouns) s = ConversionConstructions.PersonalNameComplements(s);
        Track("personal-name-complements", "Definite articles in pensar em complements with reviewed personal names");

        if (_options.Vocabulary && _options.Pronouns) s = ConversionConstructions.Complaints(s, informalConstructions);
        Track("construction-vocabulary", "Complement-qualified vocabulary senses");
        if (_options.Conversational) s = ConversionConstructions.Conversational(s, reference);
        Track("conversational-style", "Optional future paraphrase and noncontrastive subject omission");

        if (_prefixRegex != null)
        {
            s = _prefixRegex.Replace(s, m => CaseHelper.MatchCase(m.Value, _prefixes[m.Value]));
        }

        if (_options.Spelling)
        {
            s = SpellingRules.Apply(s);
            s = PortugueseOrthography.Apply(s, _options.PostAgreement);
        }
        Track("orthography", "Prefixes and selected Portuguese orthography");

        s = DoubledWords.Apply(s);
        s = UntilContraction.Apply(s);
        if (reference is { Past: true } or { Present: true })
        {
            s = BareInfinitive.Apply(s, reference.Past);
        }
        s = AutomaticGrammar.Apply(s, _options);
        Track("final-grammar", "Agreement, ordinal/preposition cleanup and reference-guided finite verb");

        return Unmask(s, masks);
    }

    // Brazilian "se você pegá-lo" is the future subjunctive with an attached pronoun; Portugal puts the pronoun before it:
    // "se você o apanhar", "quando eles fazê-lo" → "quando eles o fizerem". Without a subject, only a clause-initial "se"
    // ("Se vê-lo, liga-me"), because "Não sei se fazê-lo" / "Sei quando apanhá-lo" are infinitives.
    private static readonly Regex FutureSubjunctiveClitic = new(
        @"(?:(?<lead>" + NotWordBefore + @"(?:se|quando|assim\s+que|logo\s+que|sempre\s+que|enquanto))\s+" +
        @"(?<subj>você|vocês|ele|ela|eles|elas|nós|a\s+gente|tu|eu|o\s+senhor|a\s+senhora|os\s+senhores|as\s+senhoras)" +
        @"|(?<lead>(?:^|(?<=[,;:.!?¿¡\-–—]\s*)|(?<=" + NotWordBefore + @"(?:e|mas|só)\s+))se))\s+" +
        NotWordBefore + @"(?<stem>\p{L}+?)(?<v>[áêô]|i)-(?<cl>l[oa]s?)" + NotWordAfter,
        Ci | RegexOptions.Multiline);

    private static readonly Regex NeverIndefiniteObject = new(
        NotWordBefore + @"(?<pre>nunca\s+(?:(?:me|te|se|nos|lhe|lhes)\s+)?)(?<v>\p{L}+)(?<sp>\s+)(?<det>nenhum|nenhuma)(?=\s+\p{L})",
        Ci);

    private static readonly Regex InclusiveContinuation = new(
        @"(?<pre>^\s*mas\s+nunca\s+)(?<v>acha|pensa|imagina)(?<que>\s+que\s+)vai(?<event>\s+acontecer\s+)(?:contigo|com\s+você)" + NotWordAfter, Ci);
    private static readonly Regex CatchUpLater = new(
        @"(?<pre>(?:^|[.!?]\s+)(?:eu\s+)?)(?<v>alcanço)\s+(?<p>vocês|você)\s+(?:depois|mais\s+tarde)" + NotWordAfter, Ci);
    private static readonly Regex RelativeFalar = new(
        @"(?<pre>(?<![\p{L}\p{M}])(?:este|esse|aquele|esta|essa|aquela)\s+é\s+(?:o|a)\s+[\p{L}\p{M}]+(?:\s+[\p{L}\p{M}]+){0,2}\s+)" +
        @"(?<que>que)(?<sp>\s+)(?<v>falei|falaste|falou|falámos|falamos|falaram|falava|falávamos)\s+(?:para|pra)\s+(?<p>vocês|você|ele|ela|eles|elas|nós|mim|ti)" + NotWordAfter, Ci);

    private static readonly Dictionary<string, string> IrregularFutureSubjunctive = new(StringComparer.OrdinalIgnoreCase)
    {
        ["fazer"] = "fizer", ["refazer"] = "refizer", ["desfazer"] = "desfizer", ["satisfazer"] = "satisfizer",
        ["dizer"] = "disser", ["trazer"] = "trouxer", ["ver"] = "vir", ["rever"] = "revir", ["prever"] = "previr",
        ["ter"] = "tiver", ["manter"] = "mantiver", ["obter"] = "obtiver", ["conter"] = "contiver", ["deter"] = "detiver",
        ["querer"] = "quiser", ["pôr"] = "puser", ["repor"] = "repuser", ["propor"] = "propuser", ["saber"] = "souber",
        ["poder"] = "puder", ["haver"] = "houver", ["estar"] = "estiver", ["dar"] = "der",
    };

    private static string ReplaceFutureSubjunctiveClitic(Match m)
    {
        var stem = m.Groups["stem"].Value;
        var infinitive = stem + (m.Groups["v"].Value.ToLowerInvariant() switch { "á" => "ar", "ê" => "er", "ô" => "ôr", _ => "ir" });
        if (infinitive.Equals("pôr", StringComparison.OrdinalIgnoreCase) is false && infinitive.EndsWith("ôr", StringComparison.OrdinalIgnoreCase))
        {
            infinitive = infinitive[..^2] + "or";
        }

        var verb = IrregularFutureSubjunctive.TryGetValue(infinitive, out var irregular) ? CaseHelper.MatchCase(stem, irregular) : infinitive;
        var subject = Regex.Replace(m.Groups["subj"].Value.ToLowerInvariant(), @"\s+", " ");
        verb += subject switch
        {
            "tu" => "es",
            "nós" => "mos",
            "vocês" or "eles" or "elas" or "os senhores" or "as senhoras" => "em",
            _ => string.Empty,
        };
        var clitic = m.Groups["cl"].Value[1..];
        clitic = CaseHelper.IsAllUpper(stem) ? clitic.ToUpperInvariant() : clitic.ToLowerInvariant();
        var lead = m.Groups["subj"].Success ? m.Value[..(m.Groups["subj"].Index + m.Groups["subj"].Length - m.Index)] : m.Groups["lead"].Value;
        return lead + " " + clitic + " " + verb;
    }

    // Brazil uses "isso/esse" for both "this" and "that"; Portugal says "isto/este" for "this":
    // "Eu cuido disso." / "I got this one." → "Eu cuido disto." Only when the aligned English says "this/these"
    // and not "that/those". "desse(s)" is left alone (also "se ele desse", from "dar"), and so is the idiom "por isso".
    private static readonly Dictionary<string, string> NearDemonstratives = new(StringComparer.OrdinalIgnoreCase)
    {
        ["isso"] = "isto", ["disso"] = "disto", ["nisso"] = "nisto",
        ["esse"] = "este", ["essa"] = "esta", ["esses"] = "estes", ["essas"] = "estas",
        ["nesse"] = "neste", ["nessa"] = "nesta", ["nesses"] = "nestes", ["nessas"] = "nestas",
        ["dessa"] = "desta", ["dessas"] = "destas",
    };

    private static readonly Regex NearDemonstrative = new(
        NotWordBefore + @"(?<!\bpor\s)(?:" + string.Join("|", NearDemonstratives.Keys.OrderByDescending(k => k.Length)) + ")" + NotWordAfter, Ci);

    private static readonly Dictionary<string, string> DistalDemonstratives = new(StringComparer.OrdinalIgnoreCase)
    {
        ["isto"] = "isso", ["disto"] = "disso", ["nisto"] = "nisso",
    };
    private static readonly Regex DistalDemonstrative = new(
        NotWordBefore + @"(?:isto|disto|nisto)" + NotWordAfter, Ci);
    private static readonly Regex ReferentialThat = new(
        @"\b(?:(?:would|will|could|can|does|did|should|might)\s+that\s+" +
        @"(?:work|help|change|matter|mean|make|be|have|do|happen|affect|cost|take|sound|seem)\b|" +
        @"what(?:['’]s|\s+(?:is|does|did))\s+that(?=\s+(?:got|have|has|mean|do)\b|\s*[,;.!?]|$)|" +
        @"that(?:['’]s|\s+(?:is|was|has|had|will|would|could|can|does|did|should|might|works?|worked|sounds?|seems?))\b|" +
        @"(?:with|about|of|for|on|at|to|from)\s+that(?=\s*[,;.!?]|$)|" +
        @"(?:do|does|did|doing|done|try|tried|mean|means|meant|said|say|saying|know|knew|believe)\s+that(?=\s*[,;.!?]|$))",
        Ci);

    // "pegar o ônibus/trem/táxi" → "apanhar o autocarro/comboio/táxi" ("pegar" alone means "to hold/grab" in Portugal).
    private static readonly Regex CatchTransport = new(
        NotWordBefore + @"(?<v>(?<stem>pegu|peg)(?<end>o|as|a|amos|am|ei|aste|ou|ámos|aram|ava|avas|ávamos|avam|ar|e|es|emos|em|asse|ares|armos|arem))" +
        @"(?=\s+(?:o|um|os|uns|esse|este|aquele|outro|o\s+próximo|o\s+primeiro|o\s+último|o\s+mesmo)\s+(?:ônibus|busão|busões|autocarros?|trem|trens|comboios?|metrô|metro|táxi|táxis|taxi|avião|voo|vôo|barco|ferry|elevador|bonde|elétrico)" + NotWordAfter + ")", Ci);

    private static readonly Regex DepartingBarca = new(
        NotWordBefore + @"(?<det>(?:(?:" + Determiners.Pattern + @")\s+)?)" +
        @"(?<noun>barcas?)(?=\s+(?:parte|partes|partem|partiu|partiram|partia|partiam|partirá|partirão|partiria|partiriam|parta|partam|partisse|partissem|partir|partindo|sai|saem|saiu|saíram|saía|saíam|sairá|sairão|sairia|sairiam|saia|saiam|saísse|saíssem|sair|saindo)" + NotWordAfter + ")", Ci);

    private static readonly Dictionary<string, string> DriveForms = new(StringComparer.OrdinalIgnoreCase)
    {
        ["dirijo"] = "conduzo", ["diriges"] = "conduzes", ["dirige"] = "conduz", ["dirigimos"] = "conduzimos", ["dirigem"] = "conduzem",
        ["dirigi"] = "conduzi", ["dirigiste"] = "conduziste", ["dirigiu"] = "conduziu", ["dirigiram"] = "conduziram",
        ["dirigia"] = "conduzia", ["dirigias"] = "conduzias", ["dirigiam"] = "conduziam", ["dirigir"] = "conduzir",
        ["dirigires"] = "conduzires", ["dirigirem"] = "conduzirem", ["dirigindo"] = "conduzindo", ["dirija"] = "conduza",
        ["dirijas"] = "conduzas", ["dirijam"] = "conduzam", ["dirigisse"] = "conduzisse", ["dirigiria"] = "conduziria",
        ["dirigirá"] = "conduzirá", ["dirigirei"] = "conduzirei",
    };

    // "dirigir" meaning "to drive": followed by a vehicle, or used without an object ("Sabes dirigir?", "Ele dirige mal").
    private static readonly Regex Drive = new(
        NotWordBefore + "(?:" + string.Join("|", DriveForms.Keys.OrderByDescending(k => k.Length)) + ")" + NotWordAfter +
        @"(?=\s*(?:[.!?,…;:]|$)|\s+(?:bem|mal|devagar|rápido|depressa|muito|bêbad[oa]s?|para\s+casa|até|pela|pelo|na\s+estrada|à\s+noite|um\s+pouco|agora)" + NotWordAfter +
        @"|\s+(?:o|um|a|uma|esse|este|essa|esta|aquele|aquela|meu|minha|teu|tua|seu|sua|nosso|nossa|o\s+meu|o\s+teu|o\s+seu|a\s+minha|a\s+tua|a\s+sua)\s+(?:carro|carros|caminhão|camião|ônibus|autocarro|táxi|moto|mota|motocicleta|van|carrinha|ambulância|jipe|limusine|trator|tractor|camioneta|viatura|veículo|Uber)" + NotWordAfter + ")", Ci);

    /// <summary>
    /// Brazilian "pegar" + object is "pegar em" (grab/hold) or "apanhar" (catch, get caught, catch an illness) in Portugal.
    /// "Pega a arma" → "Pega na arma", "pegou ele" (→ "pegou-o") → "apanhou-o", "fui pego" → "fui apanhado".
    /// </summary>
    internal static class Grab
    {
        private const string Forms =
            "pego|pegas|pega|pegamos|pegam|peguei|pegaste|pegou|pegámos|pegaram|pegava|pegavas|pegávamos|pegavam|pegar|pegue|pegues|" +
            "peguemos|peguem|pegasse|pegasses|pegassem|pegares|pegarmos|pegarem|pegando|pegarei|pegarás|pegará|pegaremos|pegarão|" +
            "pegaria|pegarias|pegaríamos|pegariam";

        private static readonly (string From, string To)[] Contractions =
        {
            ("o", "no"), ("a", "na"), ("os", "nos"), ("as", "nas"), ("um", "num"), ("uma", "numa"), ("uns", "nuns"), ("umas", "numas"),
            ("este", "neste"), ("esta", "nesta"), ("estes", "nestes"), ("estas", "nestas"), ("esse", "nesse"), ("essa", "nessa"),
            ("esses", "nesses"), ("essas", "nessas"), ("aquele", "naquele"), ("aquela", "naquela"), ("aqueles", "naqueles"),
            ("aquelas", "naquelas"), ("isto", "nisto"), ("isso", "nisso"), ("aquilo", "naquilo"), ("ele", "nele"), ("ela", "nela"),
("meu", "no meu"), ("minha", "na minha"), ("meus", "nos meus"), ("minhas", "nas minhas"),
            ("teu", "no teu"), ("tua", "na tua"), ("seu", "no seu"), ("sua", "na sua"), ("seus", "nos seus"), ("suas", "nas suas"),
            ("nosso", "no nosso"), ("nossa", "na nossa"), ("nossos", "nos nossos"), ("nossas", "nas nossas"),
        };

        private static readonly Dictionary<string, string> ContractionMap = Contractions.ToDictionary(c => c.From, c => c.To, StringComparer.OrdinalIgnoreCase);

        // Catch an illness: "pegou uma gripe" → "apanhou uma gripe".
        private static readonly Regex CatchIllness = new(
            NotWordBefore + @"(?<stem>pegu|peg)(?=(?:o|as|a|amos|am|ei|aste|ou|ámos|aram|ava|avam|ar|e|es|em|ando|aria|ará)\s+(?:uma\s+|um\s+|a\s+|o\s+)?(?:gripe|constipação|resfriado|doença|infeção|infecção|vírus|virose|sol|onda|bronze)" + NotWordAfter + @"|(?:o|as|a|amos|am|ei|aste|ou|ámos|aram|ava|avam|ar|e|es|em|ando|aria|ará)\s+(?:a|o|uma|um|esta|essa|aquela|este|esse|aquele|outra|outro)\s+(?:estrada|autoestrada|rodovia|saída|caminho|atalho|rua|avenida|fila|onda|boleia|carona)" + NotWordAfter + ")", Ci);

        private static readonly Regex WithObject = new(
            NotWordBefore + @"(?<v>" + Forms + @")\s+(?<d>" + string.Join("|", Contractions.Select(c => c.From).OrderByDescending(c => c.Length)) + ")" + NotWordAfter + @"(?!\s+(?:é|está|foi|era)" + NotWordAfter + ")", Ci);

        private static readonly Regex WithEnclitic = new(NotWordBefore + @"(?<stem>pegu|peg)(?=\p{L}*-(?:o|a|os|as|lo|la|los|las|no|na|nos|me|te)" + NotWordAfter + ")", Ci);

        private static readonly Regex CollectOrCatch = new(
            NotWordBefore + @"(?<v>" + Forms + ")" + NotWordAfter +
            @"(?=\s+(?:(?:o|a|os|as|um|uma|uns|umas|muito|muita|muitos|muitas|vários|várias|alguns|algumas|poucos|poucas)\s+)?" +
            @"(?:coisas?|objetos?|peixes?|trutas?|salmão|salmões|frutas?|frutos?|flores?|folhas?|cogumelos?|moedas?|bolas?|lixo|encomendas?)" + NotWordAfter + ")", Ci);

        private static readonly Regex CatchPerson = new(
            NotWordBefore + @"(?<v>" + Forms + ")" + NotWordAfter +
            @"(?=\s+(?:(?:o|a|os|as|um|uma|uns|umas|este|esta|estes|estas|esse|essa|esses|essas|aquele|aquela|aqueles|aquelas|nenhum|nenhuma|algum|alguma|alguns|algumas)\s+)?" +
            @"(?:caras?|tipos?|gajos?|gajas?|suspeitos?|suspeitas?|criminosos?|criminosas?|bandidos?|bandidas?|ladrão|ladrões|ladra|ladras|assassinos?|assassinas?|fugitivos?|fugitivas?)" + NotWordAfter +
            @"(?!\s+de" + NotWordAfter + "))", Ci);

        private static readonly Regex Caught = new(
            NotWordBefore + @"(?<aux>(?:fui|foi|foste|fomos|foram|ser|sermos|serem|é|são|sou|era|eram|seja|sejam|será|serão|seria|fosse|fossem|for|forem|foi|serás|ficou|fiquei)\s+)peg(?<e>o|a|os|as)" + NotWordAfter, Ci);

        public static string Apply(string s)
        {
            s = CatchPerson.Replace(s, ReplaceCatch);
            s = CollectOrCatch.Replace(s, ReplaceCatch);
            s = CatchIllness.Replace(s, m => CaseHelper.MatchCase(m.Value, "apanh"));
            s = Caught.Replace(s, m => m.Groups["aux"].Value + (CaseHelper.IsAllUpper(m.Value) ? "APANHAD" : "apanhad") + m.Groups["e"].Value);
            s = WithEnclitic.Replace(s, m => CaseHelper.MatchCase(m.Value, "apanh"));
            s = WithObject.Replace(s, m =>
            {
                var d = m.Groups["d"].Value;
                if (d.Equals("a", StringComparison.Ordinal) && Regex.IsMatch(s.Substring(m.Index + m.Length), @"^\s+\p{L}+(?:ar|er|ir)" + NotWordAfter))
                {
                    return m.Value; // "pegar a fazer" is not an object
                }

                var to = ContractionMap[d];
                return m.Groups["v"].Value + " " + (CaseHelper.IsAllUpper(d) ? to.ToUpperInvariant() : char.IsUpper(d[0]) ? CaseHelper.Capitalize(to) : to);
            });
            return s;
        }

        private static string ReplaceCatch(Match m)
        {
            var verb = m.Groups["v"].Value;
            var stemLength = verb.StartsWith("pegu", StringComparison.OrdinalIgnoreCase) ? 4 : 3;
            return CaseHelper.MatchCase(verb[..stemLength], "apanh") + verb[stemLength..];
        }
    }

    internal static class HearingVocabulary
    {
        private static readonly Dictionary<string, string> Forms = new(StringComparer.OrdinalIgnoreCase)
        {
            ["escutar"] = "ouvir", ["escuto"] = "ouço", ["escutas"] = "ouves", ["escuta"] = "ouve",
            ["escutamos"] = "ouvimos", ["escutam"] = "ouvem", ["escutei"] = "ouvi", ["escutaste"] = "ouviste",
            ["escutou"] = "ouviu", ["escutámos"] = "ouvimos", ["escutaram"] = "ouviram",
            ["escutava"] = "ouvia", ["escutavas"] = "ouvias", ["escutávamos"] = "ouvíamos", ["escutavam"] = "ouviam",
            ["escutando"] = "ouvindo", ["escutado"] = "ouvido",
            ["escute"] = "ouça", ["escutes"] = "ouças", ["escutemos"] = "ouçamos", ["escutem"] = "ouçam",
            ["escutarei"] = "ouvirei", ["escutarás"] = "ouvirás", ["escutará"] = "ouvirá", ["escutaremos"] = "ouviremos", ["escutarão"] = "ouvirão",
            ["escutaria"] = "ouviria", ["escutarias"] = "ouvirias", ["escutaríamos"] = "ouviríamos", ["escutariam"] = "ouviriam",
            ["escutasse"] = "ouvisse", ["escutasses"] = "ouvisses", ["escutássemos"] = "ouvíssemos", ["escutassem"] = "ouvissem",
            ["escutares"] = "ouvires", ["escutarmos"] = "ouvirmos", ["escutarem"] = "ouvirem",
        };
        private static readonly Regex Verb = new(
            NotWordBefore + "(?:" + Alternation(Forms.Keys) + ")" + NotWordAfter, Ci);
        private static readonly Regex About = new(@"^\s+(?:(?:sempre|muitas\s+vezes|às\s+vezes|nunca)\s+)?sobre" + NotWordAfter, Ci);

        internal static string Apply(string text, bool hearAbout) => Verb.Replace(text, m =>
            hearAbout || About.IsMatch(text[(m.Index + m.Length)..])
                ? CaseHelper.MatchCase(m.Value, Forms[m.Value])
                : m.Value);
    }

    private string Mask(string text, List<string> masks, bool laterPass = false)
    {
        string Add(Match m)
        {
            masks.Add(m.Value);
            return ((char)(PlaceholderBase + masks.Count - 1)).ToString();
        }

        var s = Regex.Replace(text, @"<[^<>]*>|\{[^{}]*\}", Add);
        if (_keepRegex != null)
        {
            s = _keepRegex.Replace(s, Add);
        }

        if (laterPass && _ownOutputRegex != null)
        {
            s = _ownOutputRegex.Replace(s, Add);
        }

        return s;
    }

    private static string Unmask(string text, List<string> masks)
    {
        if (masks.Count == 0)
        {
            return text;
        }

        var sb = new StringBuilder(text.Length + 32);
        foreach (var c in text)
        {
            var index = c - PlaceholderBase;
            sb.Append(index >= 0 && index < masks.Count ? masks[index] : c.ToString());
        }

        return sb.ToString();
    }

    private string ReplaceWord(Match m)
    {
        var keyText = m.Groups["key"].Value;
        if (!_words.TryGetValue(NormalizeSpaces(keyText), out var entry))
        {
            return m.Value;
        }

        return ReplaceWordEntry(m, entry);
    }

    internal static Regex CreateWordRegex(IEnumerable<string> keys) => new(
        "(?<det>" + NotWordBefore + "(?:" + Determiners.Pattern + @")\s+(?:(?:" + Determiners.Pattern + @")\s+)?)?" +
        NotWordBefore + "(?<key>" + Alternation(keys) + ")" + NotWordAfter +
        @"(?<adj>\s+(?:(?:está|estava|estão|estavam|é|era|são|eram|foi|foram|ficou|ficaram|parece|parecem|continua|continuam)\s+)?" +
        AgreeingAdjectiveChain + ")?", Ci);

    internal static string ReplaceWordEntry(Match m, WordEntry entry)
    {
        var keyText = m.Groups["key"].Value;
        var detText = m.Groups["det"].Value;
        var adjText = m.Groups["adj"].Value;
        var dets = Regex.Matches(detText, @"[\p{L}]+").Select(x => x.Value).ToList();
        var lastDet = dets.Count > 0 ? dets[^1] : null;

        if (entry.RequiredDeterminerGender is { } required &&
            (lastDet == null || Determiners.GenderOf(lastDet) != required))
        {
            return m.Value;
        }

        var pt = entry.Pt;
        if (entry.Plural != null && lastDet != null && Determiners.IsPlural(lastDet))
        {
            pt = entry.Plural;
        }

        pt = CaseHelper.MatchCase(keyText, pt);

        if (entry.TargetGender is { } gender)
        {
            if (detText.Length > 0)
            {
                detText = Regex.Replace(detText, @"[\p{L}]+", d => Determiners.ToGender(d.Value, gender));
            }

            if (adjText.Length > 0)
            {
                adjText = Regex.Replace(adjText, @"[\p{L}]+", a => NotAgreeing.Contains(a.Value) ? a.Value : SwapAdjectiveGender(a.Value, gender));
            }
        }

        return detText + pt + adjText;
    }

    // "garoto" is "miúdo" (a kid) by default, but "rapaz" for a young man: "o seu garoto é o detetive chefe"
    // (English "your boy") → "o seu rapaz". Without a reference, "meu/teu/seu/nosso garoto" ("my boy") is "rapaz".
    private static readonly Regex YoungPerson = new(
        NotWordBefore + @"(?<poss>(?:(?:meu|teu|seu|nosso|vosso|meus|teus|seus|nossos|vossos)\s+)?)(?<noun>garot(?:o|a|os|as))" + NotWordAfter, Ci);

    private static string ReplaceYoungPerson(Match m, ReferenceHint? reference)
    {
        var noun = m.Groups["noun"].Value;
        var lower = noun.ToLowerInvariant();
        var feminine = lower[^1] == 'a' || lower.EndsWith("as", StringComparison.Ordinal);
        var plural = lower.EndsWith('s');
        var adult = reference is { Kid: false } && (feminine ? reference.Girl : reference.Boy) ||
                    reference is not { Kid: true } && !feminine && m.Groups["poss"].Length > 0;
        if (!adult)
        {
            return m.Value;
        }

        var replacement = feminine ? plural ? "raparigas" : "rapariga" : plural ? "rapazes" : "rapaz";
        return m.Groups["poss"].Value + CaseHelper.MatchCase(noun, replacement);
    }

    private static string ReplaceDepartingBarca(Match m)
    {
        var determiner = Regex.Replace(m.Groups["det"].Value, @"[\p{L}]+", d => Determiners.ToGender(d.Value, 'm'));
        var noun = m.Groups["noun"].Value;
        var replacement = noun.EndsWith("s", StringComparison.OrdinalIgnoreCase) ? "barcos" : "barco";
        return determiner + CaseHelper.MatchCase(noun, replacement);
    }

    // ------------------------------------------------------------------ helpers

    private const string AgreeingAdjectivesPattern =
        "(?:(?:tod|inteir|nov|velh|pequen|limp|suj|própri|cert|errad|vazi|chei|fechad|abert|lotad|quebrad|antig|bonit|lind|car|barat|escur|molhad|ocupad|" +
        "gast|pret|branc|vermelh|amarel|rox|fei|pront|curt|comprid|larg|apertad|gross|fin|perfeit|estranh|esquisit|maluc|louc|lent|rápid|baix|alt|" +
        "gord|magr|hediond|terrível|frit|cozid|assad|coletiv|horrível|ótim|óptim|péssim|únic|rot|furad|manchad|amassad|gelad|quent|fri|noturn|diurn|antialérgic)(?:o|a|os|as)?|bom|boa|bons|boas|mau|má|maus|más|" +
        // Participles agree too (rasgada → rasgado), except common nouns and fixed words that end the same way.
        @"(?!(?:nad|cad|obrigad|cuidad|chegad|comid|bebid|partid|entrad|estrad|namorad|advogad|soldad|estad|passad|mercad|piad|parad|salad|" +
        @"medid|ferid|corrid|jornad|morad|camad|fachad|pomad|pecad|almofad|calçad|escad|facad|porrad|pancad|rodad|temporad|madrugad|" +
        @"cidad|vid|dúvid|ajud|mud|saúd)(?:o|a|os|as)" + NotWordAfter + @")\p{L}{2,}(?:ad|id)(?:o|a|os|as))" + NotWordAfter;

    private const string AdjectiveModifier = @"(?:(?:muito|tão|todo|toda|todos|todas|mais|meio|bem|bastante|super|um\s+pouco)\s+)?";

    // "jaqueta gasta, meio velha" → "casaco gasto, meio velho": up to three adjectives joined by spaces or "e"; after a
    // comma only with a modifier (meio, muito...) and when the adjective ends the phrase, so "a encrenca, cara." is not touched.
    private const string AgreeingAdjectiveChain =
        AdjectiveModifier + AgreeingAdjectivesPattern +
        @"(?:(?:\s+e\s+|\s+)" + AdjectiveModifier + AgreeingAdjectivesPattern +
        @"|,\s*" + AdjectiveModifierAfterComma + AgreeingAdjectivesPattern + @"(?=\s*(?:[,.!?;:…]|$))){0,2}";

    private const string AdjectiveModifierAfterComma = @"(?:muito|tão|todo|toda|todos|todas|mais|meio|bem|bastante|super|um\s+pouco)\s+";

    // Words inside the adjective group that never change gender.
    private static readonly HashSet<string> NotAgreeing = new(StringComparer.OrdinalIgnoreCase)
    {
        "está", "estava", "estão", "estavam", "é", "era", "são", "eram", "foi", "foram", "ficou", "ficaram", "parece", "parecem", "continua", "continuam",
        "muito", "tão", "mais", "meio", "bem", "bastante", "super", "um", "pouco", "e",
    };

    private static string SwapAdjectiveGender(string word, char gender)
    {
        var irregular = word.ToLowerInvariant() switch
        {
            "bom" or "boa" => gender == 'f' ? "boa" : "bom",
            "bons" or "boas" => gender == 'f' ? "boas" : "bons",
            "mau" or "má" => gender == 'f' ? "má" : "mau",
            "maus" or "más" => gender == 'f' ? "más" : "maus",
            _ => null,
        };
        if (irregular != null)
        {
            return CaseHelper.MatchCase(word, irregular);
        }

        string Swap(string ending, string replacement) =>
            word.EndsWith(ending, StringComparison.OrdinalIgnoreCase)
                ? word[..^ending.Length] + (char.IsUpper(word[^1]) ? replacement.ToUpperInvariant() : replacement)
                : word;

        return gender == 'f'
            ? (word.EndsWith("os", StringComparison.OrdinalIgnoreCase) ? Swap("os", "as") : Swap("o", "a"))
            : (word.EndsWith("as", StringComparison.OrdinalIgnoreCase) ? Swap("as", "os") : Swap("a", "o"));
    }

    internal static string NormalizeSpaces(string s) => Regex.Replace(s.Trim(), @"\s+", " ");

    private static string Alternation(IEnumerable<string> keys) =>
        string.Join("|", keys
            .Select(NormalizeSpaces)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderByDescending(k => k.Length)
            .Select(k => Regex.Escape(k).Replace(@"\ ", @"\s+")));

    // ------------------------------------------------------------------ built-in list

    internal sealed record WordEntry(string Br, string Pt, bool IsSpelling, char? TargetGender, char? RequiredDeterminerGender, string? Plural);

    private sealed record PrefixEntry(string Br, string Pt, bool IsSpelling);

    private sealed record BuiltInList(List<WordEntry> Words, List<PrefixEntry> Prefixes, List<string> Keep, List<VerbEntry> Verbs);

    private static BuiltInList LoadBuiltIn()
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("PortugueseWordList.xml")
            ?? throw new InvalidOperationException("Embedded resource PortugueseWordList.xml not found.");
        var xml = XDocument.Load(stream);
        var words = new List<WordEntry>();
        var prefixes = new List<PrefixEntry>();
        var keep = new List<string>();
        var verbs = new List<VerbEntry>();

        static char? Char(XElement e, string name) =>
            e.Attribute(name)?.Value is { Length: > 0 } v ? char.ToLowerInvariant(v[0]) : null;

        foreach (var e in xml.Root!.Elements())
        {
            var isSpelling = e.Attribute("type")?.Value == "spelling";
            switch (e.Name.LocalName)
            {
                case "Word":
                    words.Add(new WordEntry(
                        NormalizeSpaces(e.Attribute("br")!.Value),
                        e.Attribute("pt")!.Value,
                        isSpelling,
                        Char(e, "g"),
                        Char(e, "det"),
                        e.Attribute("pl")?.Value));
                    break;
                case "Prefix":
                    prefixes.Add(new PrefixEntry(e.Attribute("br")!.Value, e.Attribute("pt")!.Value, isSpelling));
                    break;
                case "Verb":
                    verbs.Add(new VerbEntry(
                        e.Attribute("br")!.Value,
                        e.Attribute("pt")!.Value,
                        e.Attribute("after")?.Value,
                        e.Attribute("notAfter")?.Value,
                        (e.Attribute("skip")?.Value ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries),
                        (e.Attribute("noun")?.Value ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries)));
                    break;
                case "Keep":
                    keep.Add(NormalizeSpaces(e.Attribute("text")!.Value));
                    break;
            }
        }

        return new BuiltInList(words, prefixes, keep, verbs);
    }

    internal static IReadOnlyList<WordEntry> BuiltInWords => BuiltIn.Value.Words;

    // ================================================================== determiners

    private static class Determiners
    {
        private static readonly (string M, string F)[] Pairs =
        {
            ("o", "a"), ("os", "as"), ("um", "uma"), ("uns", "umas"), ("do", "da"), ("dos", "das"), ("no", "na"),
            ("nos", "nas"), ("ao", "à"), ("aos", "às"), ("pelo", "pela"), ("pelos", "pelas"), ("num", "numa"),
            ("nuns", "numas"), ("dum", "duma"), ("duns", "dumas"), ("meu", "minha"), ("meus", "minhas"), ("teu", "tua"),
            ("teus", "tuas"), ("seu", "sua"), ("seus", "suas"), ("nosso", "nossa"), ("nossos", "nossas"),
            ("vosso", "vossa"), ("vossos", "vossas"), ("este", "esta"), ("estes", "estas"), ("esse", "essa"),
            ("esses", "essas"), ("aquele", "aquela"), ("aqueles", "aquelas"), ("deste", "desta"), ("destes", "destas"),
            ("desse", "dessa"), ("desses", "dessas"), ("daquele", "daquela"), ("daqueles", "daquelas"),
            ("neste", "nesta"), ("nestes", "nestas"), ("nesse", "nessa"), ("nesses", "nessas"), ("naquele", "naquela"),
            ("naqueles", "naquelas"), ("àquele", "àquela"), ("outro", "outra"), ("outros", "outras"), ("algum", "alguma"),
            ("alguns", "algumas"), ("nenhum", "nenhuma"), ("primeiro", "primeira"), ("último", "última"),
            ("mesmo", "mesma"), ("próprio", "própria"), ("todo", "toda"), ("todos", "todas"), ("dois", "duas"),
            ("vários", "várias"), ("muitos", "muitas"), ("poucos", "poucas"), ("tantos", "tantas"), ("quantos", "quantas"),
            ("novo", "nova"), ("velho", "velha"), ("grande", "grande"), ("pequeno", "pequena"), ("bom", "boa"),
            ("ótimo", "ótima"), ("lindo", "linda"), ("belo", "bela"), ("único", "única"), ("próximo", "próxima"),
            ("bons", "boas"), ("maus", "más"), ("três", "três"), ("quatro", "quatro"), ("cinco", "cinco"), ("seis", "seis"),
            ("sete", "sete"), ("oito", "oito"), ("nove", "nove"), ("dez", "dez"), ("onze", "onze"), ("doze", "doze"),
            ("quinze", "quinze"), ("vinte", "vinte"), ("trinta", "trinta"), ("cem", "cem"),
        };

        private static readonly Dictionary<string, (char Gender, string Other)> Map = Build();

        public static readonly string Pattern = string.Join("|", Map.Keys.OrderByDescending(k => k.Length).Select(Regex.Escape));

        private static Dictionary<string, (char, string)> Build()
        {
            var map = new Dictionary<string, (char, string)>(StringComparer.OrdinalIgnoreCase);
            foreach (var (m, f) in Pairs)
            {
                map[m] = ('m', f);
                if (f != m)
                {
                    map[f] = ('f', m);
                }
            }

            return map;
        }

        public static char? GenderOf(string det) =>
            Map.TryGetValue(det, out var x) && !det.Equals("grande", StringComparison.OrdinalIgnoreCase) ? x.Gender : null;

        public static bool IsPlural(string det) =>
            det.EndsWith("s", StringComparison.OrdinalIgnoreCase) && Map.ContainsKey(det);

        public static string ToGender(string det, char gender)
        {
            if (!Map.TryGetValue(det, out var x) || x.Gender == gender)
            {
                return det;
            }

            return CaseHelper.MatchCase(det, x.Other);
        }
    }

    // ================================================================== colloquial

    /// <summary>
    /// "a gente" (we) is Brazilian; European Portuguese uses "nós" with a 1st person plural verb, usually without the pronoun.
    /// "A gente faz isso" → "Fazemos isso", "a gente se divertiu" → "divertimo-nos", "com a gente" → "connosco".
    /// </summary>
    internal static class WeRules
    {
        private const string Attractors = "que|se|quando|porque|onde|como|quem|enquanto|embora|caso|talvez|quanto|qual|se|até\\s+que|antes\\s+que|depois\\s+que|já\\s+que|para\\s+que|mas\\s+se|e\\s+se";
        private const string Adverbs = "não|nunca|jamais|também|já|ainda|sempre|só|realmente|simplesmente|provavelmente|nem|apenas|agora|quase|até|mesmo|meio|tipo|só\\s+que|então|nunca\\s+mais";
        private const string ProclisisAdverbs = "não|nunca|jamais|também|já|ainda|sempre|só|nem|apenas|quase|até|mesmo";

        private static readonly Regex PersonalInfinitive = new(
            NotWordBefore + @"(?<p>para|pra|sem|antes\s+de|depois\s+de|até|ao|de|por)\s+a\s+gente\s+(?:(?<cl>se|nos)\s+)?(?<v>\p{Ll}*(?:ar|er|ir|ôr|or))" + NotWordAfter + @"(?!-)", Ci);

        private static readonly Regex Subject = new(
            @"(?<sub>" + NotWordBefore + @"(?:" + Attractors + @")\s+)?" +
            @"(?:(?<dash>(?<![\p{L}\p{M}\d_-])-\s*)|" + NotWordBefore + @")(?<!toda )(?<we>a\s+gente)(?<adv>(?:\s+(?:" + Adverbs + @")" + NotWordAfter + @"){0,2})\s+" +
            @"(?:(?<cl>se|me|te|lhe|lhes|o|a|os|as|nos)\s+)?(?<v>\p{L}+)" + NotWordAfter + @"(?!-)", Ci);

        private static readonly Regex PredicateFriend = new(
            NotWordBefore + @"(?<copula>somos|estamos|ficamos|parecemos|continuamos)(?<sp>\s+)(?<adj>amigo|amiga)" + NotWordAfter, Ci);

        private static readonly Regex With = new(NotWordBefore + @"com\s+a\s+gente" + NotWordAfter, Ci);

        private static readonly Regex Preposition = new(NotWordBefore + @"(?<p>para|pra|por|sem|entre|sobre|contra|de|perante|até)\s+a\s+gente" + NotWordAfter, Ci);

        private static readonly Regex Contracted = new(NotWordBefore + @"(?<p>da|pela|na)\s+gente" + NotWordAfter + @"(?!\s+(?:que|de|como|boa|nova|grande|daqui|dali)" + NotWordAfter + ")", Ci);

        private static readonly Regex Object = new(NotWordBefore + @"(?<v>\p{L}+)\s+a\s+gente(?=\s*[.!?,…]|\s*$)", Ci);

        private static readonly Regex Identity = new(
            NotWordBefore + @"(?<v>é|foi|era|será|seria|seja|fosse|for)(?<sp>\s+)a\s+gente(?=\s*(?:[.!?,…;:]|$))", Ci);

        public static string Apply(string s)
        {
            if (s.IndexOf("gente", StringComparison.OrdinalIgnoreCase) < 0)
            {
                return s;
            }

            s = Identity.Replace(s, m =>
            {
                PortugueseVerbs.TryThirdToFirstPlural(m.Groups["v"].Value, out var plural);
                var result = CaseHelper.MatchCase(m.Groups["v"].Value, plural) + m.Groups["sp"].Value + "nós";
                return CaseHelper.IsAllUpper(m.Value) ? result.ToUpperInvariant() : result;
            });
            s = With.Replace(s, m => CaseHelper.MatchCase(m.Value, "connosco"));
            s = PersonalInfinitive.Replace(s, ReplaceInfinitive);
            s = Subject.Replace(s, ReplaceSubject);
            s = PredicateFriend.Replace(s, m =>
                m.Groups["copula"].Value + m.Groups["sp"].Value + CaseHelper.MatchCase(m.Groups["adj"].Value, "amigos"));
            s = Object.Replace(s, m =>
            {
                var verb = m.Groups["v"].Value;
                return PortugueseVerbs.IsTransitiveForm(verb) && !PortugueseVerbs.IsInfinitive(verb)
                    ? PortugueseVerbs.AttachEnclitic(verb, CaseHelper.IsAllUpper(verb) ? "NOS" : "nos")
                    : m.Value;
            });
            s = Preposition.Replace(s, m =>
            {
                var p = m.Groups["p"].Value;
                if (p.Equals("pra", StringComparison.OrdinalIgnoreCase))
                {
                    p = CaseHelper.MatchCase(p, "para");
                }

                return p + (CaseHelper.IsAllUpper(m.Value) ? " NÓS" : " nós");
            });
            s = Contracted.Replace(s, m => CaseHelper.MatchCase(m.Groups["p"].Value, m.Groups["p"].Value.ToLowerInvariant() switch { "da" => "de", "pela" => "por", _ => "em" }) +
                                           (CaseHelper.IsAllUpper(m.Value) ? " NÓS" : " nós"));
            return s;
        }

        private static string ReplaceInfinitive(Match m)
        {
            var verb = m.Groups["v"].Value;
            if (!PortugueseVerbs.IsInfinitive(verb))
            {
                return m.Value;
            }

            var p = m.Groups["p"].Value;
            if (p.Equals("pra", StringComparison.OrdinalIgnoreCase))
            {
                p = CaseHelper.MatchCase(p, "para");
            }

            var clitic = m.Groups["cl"].Success ? "nos " : string.Empty;
            var result = p + " " + clitic + verb + "mos";
            return CaseHelper.IsAllUpper(m.Value) ? result.ToUpperInvariant() : result;
        }

        private static string ReplaceSubject(Match m)
        {
            var verb = m.Groups["v"].Value;
            if (!PortugueseVerbs.TryThirdToFirstPlural(verb, out var plural))
            {
                return m.Value;
            }

            var we = m.Groups["we"].Value;
            var adv = m.Groups["adv"].Value.TrimStart();
            var advPart = adv.Length > 0 ? adv + " " : string.Empty;
            var clitic = m.Groups["cl"].Value;
            if (clitic.Equals("se", StringComparison.OrdinalIgnoreCase))
            {
                clitic = CaseHelper.MatchCase(clitic, "nos");
            }

            string result;
            if (clitic.Length == 0)
            {
                result = adv.Equals("sempre", StringComparison.OrdinalIgnoreCase)
                    ? plural + " " + adv.ToLowerInvariant()
                    : advPart + plural;
            }
            else
            {
                var proclisis = m.Groups["sub"].Success || Regex.IsMatch(adv, @"(?:^|\s)(?:" + ProclisisAdverbs + @")(?:$|\s)", Ci) || PortugueseVerbs.IsFutureOrConditional(plural);
                result = proclisis ? advPart + clitic.ToLowerInvariant() + " " + plural : advPart + PortugueseVerbs.AttachEnclitic(plural, clitic.ToLowerInvariant());
            }

            if (CaseHelper.IsAllUpper(we))
            {
                result = result.ToUpperInvariant();
            }
            else if (char.IsUpper(we[0]) && !m.Groups["sub"].Success)
            {
                result = CaseHelper.Capitalize(result);
            }

            return m.Groups["sub"].Value + m.Groups["dash"].Value + result;
        }
    }

    /// <summary>Vocabulary that needs context (law words, following preposition, gender of the noun...).</summary>
    internal static class VocabularyRules
    {
        private static readonly Regex Toilet = new(
            NotWordBefore + @"(?<det>a|as|uma|umas|na|nas|numa|numas|da|das|pela|pelas|à|às|essa|essas|esta|estas)\s+(?<w>privada|privadas)" + NotWordAfter +
            @"(?=\s*(?:[.!?,…;:]|$)|\s+(?:está|estão|estava|estavam|ficou|ficaram|é|são|entupida|entupidas|suja|sujas|limpa|limpas|quebrada|quebradas)" + NotWordAfter + ")", Ci);

        private static readonly Regex Legal = new(NotWordBefore + @"legal(?<s>is)?" + NotWordAfter, Ci);

        private static readonly Regex LawContext = new(
            NotWordBefore + @"(?:lei|leis|advogad\p{L}*|tribunal|juiz|juíza|ilegal|ilegais|contrato|processo|polícia|direitos?|jurídic\p{L}*|crime|documentos?|tutor|tutela|guarda|idade|casamento|legalmente|autorização|licença|maioridade|menoridade|distância|limite|prazo|mínim\p{L}*|máxim\p{L}*|representante|custódia|porte|arma|armas|testamento|herdeir\p{L}*|estatuto|residência|defesa|requisitos?|procedimentos?)" + NotWordAfter, Ci);

        // "Isso é ruim" → "Isso é mau", "uma notícia ruim" → "uma notícia má".
        private static readonly Regex Bad = new(NotWordBefore + @"(?:(?<prev>\p{L}+)\s+)?(?:(?<cop>é|está|era|estava|foi|ficou|fica|parece|são|estão|eram|seja|ser|estar|tão|muito|bem|meio|nada|mais|menos|super)\s+)?(?<w>ruim|ruins)" + NotWordAfter, Ci);

        // "Legal demais" → "Muito fixe", "bom demais" → "muito bom" (not "os demais", "é demais", "comi demais").
        private static readonly Regex TooMuch = new(NotWordBefore + @"(?<adj>\p{L}+)\s+demais" + NotWordAfter, Ci);

        private static readonly Regex SubjectBefore = new(NotWordBefore + @"(?:você|vocês|ele|ela|eles|elas|eu|tu|nós|gente|não|nunca|sempre|já)\s+$", Ci);

        private static readonly Regex SentenceStartBefore = new(@"(?:^|[.!?…]\s+|(?<![\p{L}\p{M}])[-–—]\s*|\n)[""“«']?$", Ci);

        private static readonly HashSet<string> NotAdjectives = new(StringComparer.OrdinalIgnoreCase)
        {
            "os", "as", "aos", "às", "dos", "das", "nos", "nas", "pelos", "pelas", "e", "é", "são", "foi", "era", "eram", "isso", "isto",
            "aquilo", "tudo", "nada", "coisa", "muito", "pouco", "um", "uma", "de", "em", "que", "já", "não", "sou", "és", "somos",
            "está", "estão", "estou", "estás", "estava", "ser", "estar", "ficou", "fica", "fiquei", "ficar", "foste", "fui", "seria", "será",
            "bebeu", "bebi", "comi", "comeu", "falou", "falei", "fala", "falas", "trabalha", "trabalhas", "pensa", "pensas", "ama", "amo",
            "gosto", "gosta", "gostas", "quero", "quer", "queres", "isso", "ele", "ela", "eles", "elas", "eu", "tu", "nós", "me", "te", "se", "lhe",
            "tempo", "gente", "dinheiro", "vezes", "coisas", "demais", "ou", "mas", "também", "bem", "mal", "longe", "cedo", "tarde",
        };

        // "assistir (a) um filme" → "ver um filme".
        private static readonly Dictionary<string, string> WatchForms = new(StringComparer.OrdinalIgnoreCase)
        {
            ["assisto"] = "vejo", ["assistes"] = "vês", ["assiste"] = "vê", ["assistimos"] = "vemos", ["assistem"] = "veem", ["assisti"] = "vi",
            ["assististe"] = "viste", ["assistiu"] = "viu", ["assistiram"] = "viram", ["assistia"] = "via", ["assistias"] = "vias",
            ["assistíamos"] = "víamos", ["assistiam"] = "viam", ["assistir"] = "ver", ["assistires"] = "veres", ["assistirmos"] = "vermos",
            ["assistirem"] = "verem", ["assistindo"] = "vendo", ["assista"] = "veja", ["assistam"] = "vejam", ["assistirei"] = "verei",
            ["assistirá"] = "verá", ["assistiria"] = "veria", ["assistisse"] = "visse",
        };

        private static readonly Regex Watch = new(
            NotWordBefore + @"(?<v>" + string.Join("|", WatchForms.Keys.OrderByDescending(k => k.Length)) + @")(?:\s+(?<a>ao|à|aos|às|a)(?=\s))?" +
            @"(?=\s+(?:o|a|os|as|um|uma|uns|umas|esse|essa|este|esta|aquele|aquela|isso|isto|tv|televisão|filmes?|séries?|jogos?|novelas?|programas?|vídeos?|episódios?|netflix|tudo|mais|ao|à|aos|às)" + NotWordAfter + @"|\s*[.!?,…]|\s*$)", Ci);

        private static readonly Regex LookAround = new(
            NotWordBefore + @"(?<v>dou|dás|dá|damos|dão|dei|deste|deu|demos|deram|dava|davam|daria|dariam|dar|darem|dê|deem|dêem)\s+uma\s+olhada" +
            @"(?<prep>\s+(?<p>no|na|nos|nas|num|numa|em|nesse|nessa|nisso|neste|nesta|nisto))?" + NotWordAfter, Ci);

        private static readonly Dictionary<string, string> LookAroundPrepositions = new(StringComparer.OrdinalIgnoreCase)
        {
            ["no"] = "ao", ["na"] = "à", ["nos"] = "aos", ["nas"] = "às", ["num"] = "num", ["numa"] = "numa", ["em"] = "a",
            ["nesse"] = "a esse", ["nessa"] = "a essa", ["nisso"] = "a isso", ["neste"] = "a este", ["nesta"] = "a esta", ["nisto"] = "a isto",
        };

        private static readonly Regex ControlContext = new(
            NotWordBefore + @"(?<p>sob\s+|centro\s+de\s+|atenção,\s*)(?<w>controles?)" + NotWordAfter, Ci);

        // "Vou pegar uma bebida" → "Vou buscar uma bebida".
        private static readonly Regex Fetch = new(
            NotWordBefore + @"(?<aux>vou|vais|vai|vamos|vão|ir|fui|foi|foste|fomos|foram|ia|iam|venho|vim|veio|vem|vens|vá|vai\s+lá|vou\s+lá)\s+(?<p>pegar)(?=\s+(?:o|a|os|as|um|uma|uns|umas|mais|outro|outra|algo|alguma|algum|qualquer|no|na|nos|nas|num|numa|nuns|numas|isso|isto|aquilo|ele|ela|eles|elas|comida|água|chá|café|bebida|bebidas|dinheiro|gelo|roupa|coisas|ajuda|reforços|lá|aí|ali)" + NotWordAfter + @"|\s*[.!]|\s*$)", Ci);

        // "pegar no teu pé" (to nag) → "implicar contigo".
        private static readonly Regex Nag = new(
            NotWordBefore + @"(?<v>peg(?:ar|o|a|as|amos|am|ou|uei|aste|aram|ue|uem|ava|avam|ando))\s+no\s+(?:(?<pos>teu|meu|seu|nosso)\s+pé|pé\s+(?<of>d(?:e|o|a|os|as|ele|ela|eles|elas))" + NotWordAfter + ")", Ci);

        private static string ReplaceNag(Match m)
        {
            var v = m.Groups["v"].Value;
            var rest = v[3..];
            var verb = CaseHelper.MatchCase(v[..3], "imp") + (CaseHelper.IsAllUpper(v) ? "LI" : "li") +
                (rest.StartsWith("u", StringComparison.OrdinalIgnoreCase) ? (CaseHelper.IsAllUpper(v) ? "QU" : "qu") + rest[1..] : (CaseHelper.IsAllUpper(v) ? "C" : "c") + rest);
            if (m.Groups["pos"].Success)
            {
                return verb + " " + m.Groups["pos"].Value.ToLowerInvariant() switch
                {
                    "teu" => "contigo",
                    "meu" => "comigo",
                    "nosso" => "connosco",
                    _ => "consigo",
                };
            }

            var of = m.Groups["of"].Value.ToLowerInvariant();
            return verb + " com" + (of == "de" ? string.Empty : " " + of[1..]);
        }

        private static readonly Regex FetchIn = new(NotWordBefore + @"(?<p>buscar)\s+(?<d>no|na|nos|nas|num|numa|nuns|numas)" + NotWordAfter, Ci);

        // "Atirou nele" / "Atirem!" → "Disparou contra ele" / "Disparem!" ("atirar" alone means "to throw" in Portugal).
        private static readonly Regex Shoot = new(
            NotWordBefore + @"(?<v>atir(?:ar|ou|ei|aram|a|e|em|o|amos|ámos|ando|ava|avam|aria|asse|ou-se)?)" + NotWordAfter +
            @"(?:(?<sp>\s+)(?<p>em|no|na|nos|nas|num|numa|nele|nela|neles|nelas|nisso|nisto)" + NotWordAfter + @"|(?=\s*[.!]|\s*$))", Ci);

        private static string ReplaceShoot(Match m)
        {
            var v = m.Groups["v"].Value;
            var shoot = CaseHelper.MatchCase(v[..4], "disp") + (CaseHelper.IsAllUpper(v) ? "AR" : "ar") + v[4..];
            if (!m.Groups["p"].Success)
            {
                return shoot;
            }

            var p = m.Groups["p"].Value.ToLowerInvariant();
            var obj = p switch
            {
                "em" => "contra",
                "no" => "contra o",
                "na" => "contra a",
                "nos" => "contra os",
                "nas" => "contra as",
                "num" => "contra um",
                "numa" => "contra uma",
                "nisso" => "contra isso",
                "nisto" => "contra isto",
                _ => "contra " + p[1..],
            };
            return shoot + m.Groups["sp"].Value + obj;
        }

        // "Vivemos junto" → "Vivemos juntos".
        private static readonly Regex Together = new(
            NotWordBefore + @"(?<v>viver|vivemos|vivem|viveram|vivíamos|viviam|vivendo|ficar|ficamos|ficámos|ficaram|ficavam|estamos|estão|estávamos|estavam|estar|morar|moramos|moram|dormir|dormimos|trabalhar|trabalhamos|trabalham|crescemos|crescer|sair|saímos|saem|estudar|estudamos|estudámos|envelhecer|ir|vamos|fomos|vão)\s+" +
            @"(?<j>junto)(?!\s+(?:a|ao|à|aos|às|de|do|da|dos|das|com)" + NotWordAfter + ")" + NotWordAfter, Ci);

        // "Vou achar" (find) → "Vou encontrar" ("achar que" = to think stays).
        private static readonly Regex Find = new(
            NotWordBefore + @"(?<aux>vou|vais|vai|vamos|vão|consegue|consegues|conseguir|consigo|conseguimos|conseguiu|conseguiste|conseguem|tentar|tento|tentas|tenta|tentamos|de|pode|podes|posso|podemos|quero|queres|quer|ajudar\s+a|a|para|que|nunca\s+vou|não\s+vou|não\s+consigo|não\s+consegue)\s+" +
            @"(?<f>achar|achá(?=-l))(?!\s+(?:que|graça|bom|bem|mal|ruim|mau|estranho|engraçado|melhor|pior|isso|isto|normal|ótimo|óptimo|lindo|bonito|giro|fixe|interessante|importante|incrível|que)" + NotWordAfter + @")(?!-(?:me|te|se|nos)" + NotWordAfter + ")", Ci);

        private static readonly Dictionary<string, string> FindObjectForms = new(StringComparer.OrdinalIgnoreCase)
        {
            ["acho"] = "encontro", ["achas"] = "encontras", ["acha"] = "encontra", ["achamos"] = "encontramos",
            ["acham"] = "encontram", ["achei"] = "encontrei", ["achaste"] = "encontraste", ["achou"] = "encontrou",
            ["achámos"] = "encontrámos", ["acharam"] = "encontraram", ["achava"] = "encontrava", ["achavam"] = "encontravam",
            ["achar"] = "encontrar", ["achado"] = "encontrado", ["achada"] = "encontrada", ["achados"] = "encontrados",
            ["achadas"] = "encontradas",
        };

        // Restrict "achar" to concrete search targets; opinions ("acho o filme bom") remain unchanged.
        private static readonly Regex FindObject = new(
            NotWordBefore + @"(?<v>" + Alternation(FindObjectForms.Keys) + @")(?=\s+(?:(?:o|a|os|as|um|uma|uns|umas|meu|minha|teu|tua|seu|sua)\s+){1,2}" +
            @"(?:iPod|corpo|corpos|cadáver|cadáveres|carro|carros|carrinha|carrinhas|van|vans|faca|facas|arma|armas|chave|chaves|mochila|mochilas|carteira|carteiras)" +
            NotWordAfter + @"(?=\s*(?:[.!?,…]|$)|\s+(?:de|do|da|dos|das|em|no|na|nos|nas|num|numa|aqui|ali|lá|aí|onde|quando|que|estacionado|abandonado|enterrado)" +
            NotWordAfter + "))", Ci);

        private static readonly Regex FoundPassive = new(
            NotWordBefore + @"(?<aux>foi|foram|era|eram|ser|sido|será|serão)(?<sp>\s+)(?<v>achado|achada|achados|achadas)" + NotWordAfter +
            @"(?=\s*(?:[.!?,…]|$)|\s+(?:de|do|da|dos|das|em|no|na|nos|nas|num|numa|aqui|ali|lá|aí|onde|quando|que|próximo|próxima|perto|junto)" + NotWordAfter + ")", Ci);

        private static readonly Regex Fingerprints = new(
            NotWordBefore + @"(?<p>sem|tem|temos|tenho|tinham|tinha|encontramos|encontraram|encontrei|encontrou|procurar|procurei|análise\s+de|recolher|recolheu)(?<sp>\s+)(?<det>as\s+|suas\s+|umas\s+)?(?<w>digitais)" + NotWordAfter, Ci);

        private static readonly Regex CheckCommand = new(
            @"(?<pre>(?:^|[.!?…]\s+|\n|(?<![\p{L}\p{M}])[-–—]\s*))(?<v>cheque|checa)(?=\s+(?:o|a|os|as|um|uma)\s+(?:vídeos?|placas?|registos?|registros?|moradas?|endereços?|ficheiros?|arquivos?)" + NotWordAfter + ")", Ci);

        private static readonly Regex VanContext = new(
            NotWordBefore + @"(?<w>van)(?=\s+(?:branca|azul|preta|vermelha|cinzenta|estacionada|abandonada)" + NotWordAfter + ")", Ci);

        private static readonly Regex Beneath = new(NotWordBefore + @"embaixo(?=\s+d(?:e|o|a|os|as|ele|ela|eles|elas|isso|isto|aquilo)" + NotWordAfter + ")", Ci);

        private static readonly Regex Valeu = new(NotWordBefore + @"valeu\s+(?=(?:por|pela|pelo)" + NotWordAfter + ")", Ci);

        // Brazilian slang "tira" (cop) → "polícia". "Tira" is also the verb "tirar" (Tira a mão, ele tira nota A, se tira a
        // imunidade), so only the noun: after a determiner, number, "de", or a form of "ser", or with a typical police word after it.
        // "tiras e promotores", "polícias e ladrões": other law-and-order people beside "tira(s)".
        private const string CopPartners = @"procurador|procuradores|promotor|promotores|detetives?|bombeiros?|agentes|ladrões|bandidos|criminosos|juízes|advogados";

        private static readonly Regex Cop = new(
            @"(?<pre>" + NotWordBefore +
            @"(?:(?<!" + NotWordBefore + @"(?:eu|tu|ele|ela|eles|elas|você|vocês|nós|se|que|não|me|te|lhe|nos|quem|já|só|também|nunca|sempre)\s+)(?:o|a|os|as)" +
            @"|um|uma|uns|umas|do|da|dos|das|ao|à|aos|às|pelo|pela|pelos|pelas|no|na|nos|nas|num|numa|de|esse|essa|esses|essas|este|esta|estes|estas|" +
            @"aquele|aquela|aqueles|aquelas|desse|dessa|deste|desta|daquele|daquela|meu|minha|meus|minhas|teu|tua|teus|tuas|seu|sua|seus|suas|" +
            @"nosso|nossa|nossos|nossas|outro|outra|outros|outras|muitos|muitas|poucos|vários|alguns|nenhum|nenhuma|cada|todo|todos|qualquer|" +
            @"dois|duas|três|quatro|cinco|seis|dez|cem|mil|bom|boa|bons|mau|grande|velho|novo|ser|sou|és|é|somos|são|era|eram|fui|foi|foram|sendo|seja|" +
            @"como\s+um|e\s+não)(?:\s+(?:bom|boa|bons|boas|mau|má|maus|grande|velho|velha|novo|nova|outro|outra|único|única|melhor|pior|verdadeiro))?\s+)" +
            @"(?<w>tira|tiras)" + NotWordAfter +
            @"|(?<pre>(?:^|[.!?…]\s+|(?<![\p{L}\p{M}])[-–—]\s*|\n)[""“«']?)(?<w>tiras)(?=\s+(?:de\s+rua|de\s+patrulha|e\s+)" + NotWordAfter + ")" +
            @"|(?<pre>)(?<w>tira|tiras)(?=\s+(?:corrupto|corruptos|corrupta|recruta|recrutas|novato|novatos|disfarçado|disfarçados|à\s+paisana|de\s+patrulha|de\s+rua|morto|morta|mortos|e\s+(?:" + CopPartners + "))" + NotWordAfter + ")" +
            @"|(?<pre>(?<=" + NotWordBefore + "(?:" + CopPartners + @")\s+e\s+))(?<w>tira|tiras)" + NotWordAfter,
            Ci | RegexOptions.Multiline);

        // "surpreso" is Brazilian; Portugal says "surpreendido". "surpresa" is also the noun, so only after estar/ficar/parecer.
        private static readonly Regex Surprised = new(
            NotWordBefore + @"(?<w>surpreso|surpresos)" + NotWordAfter +
            @"|(?<=" + NotWordBefore + @"(?:estou|está|estás|estava|estavas|estávamos|estavam|estão|estamos|estive|estiveste|esteve|estiveram|estar|" +
            @"estaria|estará|esteja|estejas|fico|fica|ficas|ficou|fiquei|ficaste|ficámos|ficamos|ficaram|ficar|ficaria|ficará|fique|fiques|ficava|" +
            @"ficavam|ficam|pareço|parece|pareces|parecia|pareceu|parecem|pareceram|parecer)" +
            @"(?:\s+(?:muito|tão|bem|mesmo|bastante|realmente|tanto|um\s+pouco|nada))?\s+)(?<w>surpresa|surpresas)" + NotWordAfter, Ci);

        private static string ReplaceSurprised(Match m)
        {
            var w = m.Groups["w"].Value;
            var to = w.ToLowerInvariant() switch
            {
                "surpreso" => "surpreendido",
                "surpresos" => "surpreendidos",
                "surpresa" => "surpreendida",
                _ => "surpreendidas",
            };
            return CaseHelper.MatchCase(w, to);
        }

        // "Cale a boca!" / "Cala a tua boca" → "Cala-te!": Portugal uses the reflexive verb, not "calar a boca".
        private static readonly Regex ShutUp = new(
            @"(?<=^|[.!?…]\s+|(?<![\p{L}\p{M}])[-–—]\s*|,\s+|[""“«]|" + NotWordBefore + @"e\s+)(?<v>cala|cale|calem|calai)\s+(?:(?:a|essa)\s+)?" +
            @"(?:(?:tua|sua|vossa)\s+)?boca(?:\s+grande)?" + NotWordAfter, Ci | RegexOptions.Multiline);

        public static string ReplaceShutUp(string s, bool tu) => ShutUp.Replace(s, m =>
        {
            var v = m.Groups["v"].Value;
            var to = v.ToLowerInvariant() switch
            {
                "cala" => "cala-te",
                "cale" => tu ? "cala-te" : "cale-se",
                "calem" => "calem-se",
                _ => "calai-vos",
            };
            return char.IsUpper(v[0]) ? CaseHelper.Capitalize(to) : to;
        });

        // Brazilian bare commands of pronominal verbs: "Levanta!" / "Levante!" → "Levanta-te!" / "Levante-se!".
        // Only when the verb is the whole command (an optional "já/daí/aí/lá/agora" and the end of the sentence follow).
        // "abaixar" (to duck) is "baixar-se" in Portugal. Ambiguous forms (sente: sentar/sentir, vira: to turn a car) are left out.
        private static readonly Dictionary<string, string> BareReflexive = BuildBareReflexive();

        private static Dictionary<string, string> BuildBareReflexive()
        {
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var stem in new[] { "levant", "deit", "acalm", "afast", "aproxim", "ajoelh", "despach", "apress", "comport", "agach" })
            {
                map[stem + "a"] = stem + "a-te";
                map[stem + "e"] = stem + "e-se";
                map[stem + "em"] = stem + "em-se";
            }
            map["senta"] = "senta-te";
            map["cala"] = "cala-te";
            map["cale"] = "cale-se";
            map["calem"] = "calem-se";
            foreach (var stem in new[] { "mex", "escond" })
            {
                map[stem + "e"] = stem + "e-te";
                map[stem + "a"] = stem + "a-se";
                map[stem + "am"] = stem + "am-se";
            }
            map["abaixa"] = "baixa-te";
            map["abaixe"] = "baixe-se";
            map["abaixem"] = "baixem-se";
            return map;
        }

        private static readonly Regex BareReflexiveCommand = new(
            @"(?<=^|[.!?…]\s+|(?<![\p{L}\p{M}])[-–—]\s*|[""“«]|,\s+)(?<v>" + string.Join("|", BareReflexive.Keys.OrderByDescending(k => k.Length)) + @")" +
            @"(?=(?:\s+(?:já|daí|aí|lá|agora|logo|depressa|devagar|rápido))?(?:\s*[!.…]|\s*$))", Ci | RegexOptions.Multiline);

        // "Dê pé!" (a typo for "De pé!", stand up) on its own.
        private static readonly Regex StandUp = new(@"(?<=^|[.!?…]\s+|(?<![\p{L}\p{M}])[-–—]\s*|[""“«])(?<d>D)ê(?<p>\s+pé)(?=\s*[!.…,]|\s*$)",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Multiline);

        public static string ReplaceBareReflexive(string s)
        {
            s = StandUp.Replace(s, m => m.Groups["d"].Value + (m.Value[1] == 'Ê' ? "E" : "e") + m.Groups["p"].Value);
            return BareReflexiveCommand.Replace(s, m =>
            {
                var v = m.Groups["v"].Value;
                var to = BareReflexive[v];
                return CaseHelper.IsAllUpper(v) && v.Length > 1 ? to.ToUpperInvariant() : char.IsUpper(v[0]) ? CaseHelper.Capitalize(to) : to;
            });
        }

        // Portugal has no "Corregedoria": the police unit is "Assuntos Internos" (masculine plural), the courts' one "Inspeção".
        private static readonly Regex InternalAffairs = new(
            @"(?:" + NotWordBefore + @"(?<det>a|da|na|pela|à|esta|essa|desta|dessa|nesta|nessa|sua|tua|nossa)\s+)?" + NotWordBefore +
            @"(?<w>corregedoria)(?<judicial>(?:-geral|\s+geral)?\s+(?:da\s+justiça|de\s+justiça|do\s+tribunal|dos\s+tribunais|judicial|do\s+ministério|da\s+magistratura))?" +
            @"(?<after>" + NotWordAfter + @"(?:\s+(?<v>[\p{L}]+))?)", Ci);

        // "investigador da Assuntos Internos" / "do Assuntos Internos" → "dos Assuntos Internos".
        private static readonly Regex InternalAffairsArticle = new(
            NotWordBefore + @"(?<det>da|do|na|no|pela|pelo)(?<sp>\s+)(?=Assuntos\s+Internos" + NotWordAfter + ")", Ci);

        private static readonly Dictionary<string, string> PluralVerbs = new(StringComparer.OrdinalIgnoreCase)
        {
            ["é"] = "são", ["está"] = "estão", ["vai"] = "vão", ["tem"] = "têm", ["vem"] = "vêm", ["foi"] = "foram", ["quer"] = "querem",
            ["pode"] = "podem", ["sabe"] = "sabem", ["acha"] = "acham", ["precisa"] = "precisam", ["estava"] = "estavam", ["teve"] = "tiveram",
            ["fez"] = "fizeram", ["disse"] = "disseram", ["deu"] = "deram", ["pôs"] = "puseram", ["veio"] = "vieram", ["esteve"] = "estiveram",
            ["era"] = "eram", ["ia"] = "iam", ["tinha"] = "tinham", ["queria"] = "queriam", ["podia"] = "podiam",
            ["investiga"] = "investigam", ["descobriu"] = "descobriram", ["abriu"] = "abriram", ["chamou"] = "chamaram",
            ["ligou"] = "ligaram", ["quis"] = "quiseram", ["anda"] = "andam", ["fica"] = "ficam", ["ficou"] = "ficaram", ["acabou"] = "acabaram",
            ["mandou"] = "mandaram", ["deve"] = "devem", ["devia"] = "deviam", ["vai-se"] = "vão-se", ["irá"] = "irão", ["vai-te"] = "vão-te",
        };

        private static string ReplaceInternalAffairs(Match m)
        {
            var w = m.Groups["w"].Value;
            var det = m.Groups["det"].Value;
            var detPart = det.Length > 0 ? det + " " : string.Empty;
            if (m.Groups["judicial"].Success)
            {
                return detPart + CaseHelper.MatchCase(w, "inspeção") + m.Groups["judicial"].Value + m.Groups["after"].Value;
            }

            var newDet = det.ToLowerInvariant() switch
            {
                "" => "",
                "a" => "os", "da" => "dos", "na" => "nos", "pela" => "pelos", "à" => "aos", "esta" => "estes", "essa" => "esses",
                "desta" => "destes", "dessa" => "desses", "nesta" => "nestes", "nessa" => "nesses", "sua" => "seus", "tua" => "teus", _ => "nossos",
            };
            var result = newDet.Length > 0 ? CaseHelper.MatchCase(det, newDet) + " " : string.Empty;
            result += CaseHelper.IsAllUpper(w) && w.Length > 1 ? "ASSUNTOS INTERNOS" : "Assuntos Internos";
            var after = m.Groups["after"].Value;
            var v = m.Groups["v"];
            // Only the subject agrees: "A Corregedoria vai" / "que a corregedoria quer", not "pela Corregedoria".
            if (v.Success && det.ToLowerInvariant() is "" or "a" or "esta" or "essa" or "sua" or "tua" or "nossa" && PluralVerbs.TryGetValue(v.Value, out var plural))
            {
                after = after[..(v.Index - m.Groups["after"].Index)] + CaseHelper.MatchCase(v.Value, plural);
            }

            return result + after;
        }

        // "sair da cadeia" / "ir para a cadeia" → "prisão"; "cadeia de comando" / "cadeia alimentar" (a chain) are kept.
        private static readonly Regex Jail = new(
            NotWordBefore + @"(?<pre>(?:da|na|à|a|pela|numa|uma|desta|dessa|nesta|nessa)\s+)(?<w>cadeia|cadeias)" + NotWordAfter +
            @"(?!\s+(?:de|do|da|dos|das|alimentar|alimentares|hoteleira|produtiva|logística|montanhosa|nacional|nacionais|internacional|perpétua|perpétuas)" + NotWordAfter + ")", Ci);

        // Brazilian "chegar em casa / no hospital / na escola" → "chegar a casa / ao hospital / à escola".
        // Times are kept: "chegou em dez minutos", "chegou na hora", "chegou no sábado".
        private const string Arrive = @"(?<v>cheg(?:o|as|a|amos|am|uei|aste|ou|ámos|aram|ava|avas|ávamos|avam|ar|ares|armos|arem|ará|arás|aremos|arão|arei|aria|arias|aríamos|ariam|ue|ues|uemos|uem|ando|ado|asse|asses|assem|aste))" +
            @"(?<adv>\s+(?:bem|já|cedo|tarde|hoje|ontem|agora|logo|primeiro|sozinho|sozinha))?\s+";

        private static readonly Regex ArriveIn = new(
            NotWordBefore + Arrive + @"(?:(?<em>em)\s+(?=casa" + NotWordAfter + @"|tempo" + NotWordAfter + @"|(?-i:\p{Lu}))|(?<em>em\s+um|num)\s+(?!(?:instante|minuto|segundo|momento|piscar|pulo|salto|ápice)" + NotWordAfter + @")|(?<em>em\s+uma|numa)\s+(?!(?:hora|semana|questão)" + NotWordAfter + @")|(?<c>no|na|nos|nas)\s+" +
            @"(?!(?:hora|horas|momento|tempo|dia|dias|semana|mês|ano|noite|manhã|tarde|véspera|altura|mesma|mesmo|sábado|domingo|segunda|terça|quarta|quinta|sexta|" +
            @"último|última|próximo|próxima|início|minuto|segundo|instante|prazo)" + NotWordAfter + "))", Ci);

        public static string ApplyArrive(string s) => ArriveIn.Replace(s, m =>
        {
            var head = m.Groups["v"].Value + m.Groups["adv"].Value + " ";
            if (m.Groups["c"].Success)
            {
                var c = m.Groups["c"].Value;
                var to = c.ToLowerInvariant() switch { "no" => "ao", "na" => "à", "nos" => "aos", _ => "às" };
                return head + CaseHelper.MatchCase(c, to) + " ";
            }

            var em = m.Groups["em"].Value.ToLowerInvariant();
            var repl = em is "em um" or "num" ? "a um " : em is "em uma" or "numa" ? "a uma " : "a ";
            return head + (char.IsUpper(m.Groups["em"].Value[0]) ? CaseHelper.Capitalize(repl) : repl);
        });

        // Brazilian "ir na padaria / no supermercado / na casa da Ana" (em) → "ir à padaria / ao supermercado / à casa da Ana" (a).
        // Only destinations: "ir no carro", "ir na frente" or "ir no sábado" are not movement to a place.
        private const string GoVerbs = @"(?:v(?:ou|ais|ai|amos|ão|á|ás|ades)|ia|ias|íamos|iam|ir|ires|irmos|irem|ido|indo|fui|foste|foi|fomos|foram|fora|fosse|fossem|" +
            @"irei|irás|irá|iremos|irão|iria|irias|iríamos|iriam|volt(?:o|as|a|amos|am|ei|aste|ou|aram|ava|avam|ar|ando|ado)|regress(?:o|as|a|amos|am|ei|ou|aram|ar))";

        private static readonly Regex GoTo = new(
            NotWordBefore + @"(?<v>" + GoVerbs + @")(?<adv>\s+(?:já|agora|lá|também|sempre|logo|hoje|ontem|amanhã|sozinho|sozinha|depois|novamente|hoje\s+à\s+noite))?\s+" +
            @"(?<c>no|na|nos|nas)\s+(?=(?:padaria|supermercado|mercado|banco|médico|médica|dentista|advogado|hospital|escola|universidade|faculdade|igreja|cinema|teatro|praia|parque|" +
            @"banheiro|casa|cozinha|quarto|sala|escritório|delegacia|esquadra|tribunal|aeroporto|estação|hotel|restaurante|bar|loja|farmácia|shopping|biblioteca|museu|cemitério|" +
            @"prisão|cadeia|jardim|festa|reunião|casamento|funeral|enterro|missa|aula|consulta|trabalho|laboratório|necrotério|ginásio|academia|piscina|clube|discoteca|cafetaria|café|barbeiro|cabeleireiro|" +
            @"lavandaria|correios|prefeitura|câmara|consulado|embaixada|quartel|base|aldeia|cidade|campo|fazenda|montanha|floresta|lago|rio|mar|bosque|armazém|garagem|porão|cave|sótão|varanda|terraço|pátio)" + NotWordAfter + ")", Ci);

        public static string ApplyGo(string s) => GoTo.Replace(s, m =>
        {
            var c = m.Groups["c"].Value;
            var to = c.ToLowerInvariant() switch { "no" => "ao", "na" => "à", "nos" => "aos", _ => "às" };
            return m.Groups["v"].Value + m.Groups["adv"].Value + " " + CaseHelper.MatchCase(c, to) + " ";
        });

        // Brazilian "botar o meu na reta" (to put oneself on the line) → "arriscar o pescoço".
        private static readonly Dictionary<string, string> OnTheLineVerbs = BuildOnTheLineVerbs();

        private static Dictionary<string, string> BuildOnTheLineVerbs()
        {
            var forms = new (string Arriscar, string[] Brazilian)[]
            {
                ("arrisco", new[] { "boto", "ponho", "coloco", "meto" }),
                ("arrisca", new[] { "bota", "põe", "coloca", "mete" }),
                ("arriscas", new[] { "botas", "pões", "colocas", "metes" }),
                ("arriscamos", new[] { "botamos", "pomos", "colocamos", "metemos" }),
                ("arriscam", new[] { "botam", "põem", "colocam", "metem" }),
                ("arrisquei", new[] { "botei", "pus", "coloquei", "meti" }),
                ("arriscaste", new[] { "botaste", "puseste", "colocaste", "meteste" }),
                ("arriscou", new[] { "botou", "pôs", "colocou", "meteu" }),
                ("arriscaram", new[] { "botaram", "puseram", "colocaram", "meteram" }),
                ("arriscava", new[] { "botava", "punha", "colocava", "metia" }),
                ("arriscar", new[] { "botar", "pôr", "colocar", "meter" }),
                ("arriscando", new[] { "botando", "pondo", "colocando", "metendo" }),
                ("arriscado", new[] { "botado", "posto", "colocado", "metido" }),
                ("arrisque", new[] { "bote", "ponha", "coloque", "meta" }),
                ("arrisquem", new[] { "botem", "ponham", "coloquem", "metam" }),
                ("arriscasse", new[] { "botasse", "pusesse", "colocasse", "metesse" }),
                ("arriscaria", new[] { "botaria", "poria", "colocaria", "meteria" }),
                ("arriscarei", new[] { "botarei", "porei", "colocarei", "meterei" }),
                ("arriscará", new[] { "botará", "porá", "colocará", "meterá" }),
            };
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var (to, from) in forms)
                foreach (var verb in from) map[verb] = to;
            return map;
        }

        private static readonly Regex OnTheLine = new(
            NotWordBefore + @"(?<v>" + string.Join("|", OnTheLineVerbs.Keys.OrderByDescending(k => k.Length)) + @")" +
            @"(?<cl>-(?:me|te|se|nos))?\s+(?:(?:o|a|os|as)\s+)?(?:meu|minha|meus|minhas|seu|sua|seus|suas|teu|tua|teus|tuas|nosso|nossa|nossos|nossas|dele|dela)\s+na\s+reta" + NotWordAfter, Ci);

        public static string Apply(string s)
        {
            s = OnTheLine.Replace(s, m => CaseHelper.MatchCase(m.Groups["v"].Value, OnTheLineVerbs[m.Groups["v"].Value]) + " o pescoço");
            s = ApplyArrive(s);
            s = ApplyGo(s);
            s = Jail.Replace(s, m => m.Groups["pre"].Value + CaseHelper.MatchCase(m.Groups["w"].Value,
                m.Groups["w"].Value.EndsWith("s", StringComparison.OrdinalIgnoreCase) ? "prisões" : "prisão"));
            s = InternalAffairs.Replace(s, ReplaceInternalAffairs);
            s = InternalAffairsArticle.Replace(s, m => CaseHelper.MatchCase(m.Groups["det"].Value, m.Groups["det"].Value.ToLowerInvariant() switch
            {
                "da" or "do" => "dos", "na" or "no" => "nos", _ => "pelos",
            }) + m.Groups["sp"].Value);
            s = Surprised.Replace(s, ReplaceSurprised);
            s = Cop.Replace(s, m => m.Groups["pre"].Value + CaseHelper.MatchCase(m.Groups["w"].Value,
                m.Groups["w"].Value.EndsWith("s", StringComparison.OrdinalIgnoreCase) ? "polícias" : "polícia"));
            s = Toilet.Replace(s, m => m.Groups["det"].Value + " " +
                CaseHelper.MatchCase(m.Groups["w"].Value, m.Groups["w"].Value.EndsWith("s", StringComparison.OrdinalIgnoreCase) ? "sanitas" : "sanita"));
            if (!LawContext.IsMatch(s))
            {
                s = Legal.Replace(s, m => CaseHelper.MatchCase(m.Value, m.Groups["s"].Success ? "fixes" : "fixe"));
            }

            s = Bad.Replace(s, ReplaceBad);
            s = TooMuch.Replace(s, m =>
            {
                var adj = m.Groups["adj"].Value;
                if (NotAdjectives.Contains(adj) || PortugueseVerbs.IsKnownVerbForm(adj) || PortugueseVerbs.ThirdToSecond.ContainsKey(adj.ToLowerInvariant()) ||
                    PortugueseVerbs.IsInfinitive(adj) || Regex.IsMatch(adj, @"(?:ou|ei|eu|iu|am|em|aste|este|iste|ste|mos)$", Ci) || adj.Length < 3 ||
                    // Right after a subject the word is a verb: "Você planeja demais", "Ele reclama demais".
                    SubjectBefore.IsMatch(s.AsSpan(0, m.Index).ToString()) ||
                    // A sentence-initial "-s" word is a "tu" verb once "você" was dropped: "Planeias demais", "Falas demais".
                    (adj.EndsWith("s", StringComparison.OrdinalIgnoreCase) && SentenceStartBefore.IsMatch(s.AsSpan(0, m.Index).ToString())))
                {
                    return m.Value;
                }

                var muito = CaseHelper.IsAllUpper(adj) ? "MUITO " : char.IsUpper(adj[0]) ? "Muito " : "muito ";
                return muito + (char.IsUpper(adj[0]) && !CaseHelper.IsAllUpper(adj) ? char.ToLowerInvariant(adj[0]) + adj.Substring(1) : adj);
            });
            s = Watch.Replace(s, m =>
            {
                var verb = CaseHelper.MatchCase(m.Groups["v"].Value, WatchForms[m.Groups["v"].Value]);
                if (!m.Groups["a"].Success)
                {
                    return verb;
                }

                var a = m.Groups["a"].Value.ToLowerInvariant() switch { "ao" => "o", "à" => "a", "aos" => "os", "às" => "as", _ => string.Empty };
                return a.Length == 0 ? verb : verb + " " + CaseHelper.MatchCase(m.Groups["a"].Value, a);
            });
            s = LookAround.Replace(s, m =>
            {
                var prep = m.Groups["p"].Success ? " " + LookAroundPrepositions[m.Groups["p"].Value] : string.Empty;
                return CaseHelper.MatchCase(m.Value, m.Groups["v"].Value + " uma vista de olhos" + prep);
            });
            s = ControlContext.Replace(s, m => m.Groups["p"].Value + CaseHelper.MatchCase(m.Groups["w"].Value,
                m.Groups["w"].Value.EndsWith("s", StringComparison.OrdinalIgnoreCase) ? "controlos" : "controlo"));
            s = Beneath.Replace(s, m => CaseHelper.MatchCase(m.Value, "debaixo"));
            s = Shoot.Replace(s, ReplaceShoot);
            s = Together.Replace(s, m => m.Groups["v"].Value + " " + CaseHelper.MatchCase(m.Groups["j"].Value, "juntos"));
            s = Find.Replace(s, m => m.Groups["aux"].Value + " " + CaseHelper.MatchCase(m.Groups["f"].Value, m.Groups["f"].Value.ToLowerInvariant() == "achar" ? "encontrar" : "encontrá"));
            s = FindObject.Replace(s, m => CaseHelper.MatchCase(m.Value, FindObjectForms[m.Value]));
            s = FoundPassive.Replace(s, m => m.Groups["aux"].Value + m.Groups["sp"].Value + CaseHelper.MatchCase(m.Groups["v"].Value, FindObjectForms[m.Groups["v"].Value]));
            s = Fingerprints.Replace(s, m => m.Groups["p"].Value + m.Groups["sp"].Value + m.Groups["det"].Value + CaseHelper.MatchCase(m.Groups["w"].Value, "impressões digitais"));
            s = CheckCommand.Replace(s, m => m.Groups["pre"].Value + CaseHelper.MatchCase(m.Groups["v"].Value,
                m.Groups["v"].Value.Equals("cheque", StringComparison.OrdinalIgnoreCase) ? "verifique" : "verifica"));
            s = VanContext.Replace(s, m => CaseHelper.MatchCase(m.Value, "carrinha"));
            s = Nag.Replace(s, ReplaceNag);
            s = Fetch.Replace(s, m => m.Groups["aux"].Value + " " + CaseHelper.MatchCase(m.Groups["p"].Value, "buscar"));
            s = FetchIn.Replace(s, m => m.Groups["p"].Value + " " + CaseHelper.MatchCase(m.Groups["d"].Value, m.Groups["d"].Value.ToLowerInvariant().Substring(1)));
            s = Valeu.Replace(s, m => CaseHelper.MatchCase(m.Value.TrimEnd(), "obrigado") + " ");
            return s;
        }

        private static string ReplaceBad(Match m)
        {
            var prev = m.Groups["prev"].Value.ToLowerInvariant();
            var plural = m.Groups["w"].Value.EndsWith("s", StringComparison.OrdinalIgnoreCase);
            var feminine = prev is not ("polícia" or "polícias" or "dia" or "dias") && (prev is "ela" or "elas" or "coisa" or "coisas" or "gente" or "ideia" or "ideias" or "notícia" or "notícias" or "pessoa" or "pessoas" or "uma" ||
                           (!m.Groups["cop"].Success && prev.Length > 2 && Regex.IsMatch(prev, @"(?:a|as|ção|ções|dade|dades|gem|gens)$") && !Regex.IsMatch(prev, @"(?:ma|mas|ta|tas|dia|dias)$")));
            var word = plural ? (feminine ? "más" : "maus") : (feminine ? "má" : "mau");
            var result = m.Value.Substring(0, m.Groups["w"].Index - m.Index) + CaseHelper.MatchCase(m.Groups["w"].Value, word);
            return result;
        }
    }

    internal static class ColloquialRules
    {
        private static string Masculine(string article) =>
            article.Trim().ToLowerInvariant() switch
            {
                "uma" => "um ", "alguma" => "algum ", "a" => "o ", "sua" => "seu ", "tua" => "teu ", "minha" => "meu ", "nossa" => "nosso ", _ => article,
            };

        private static string CorrespondencePrep(string p) =>
            p.ToLowerInvariant().Replace("com o", "no").Replace("com a", "na").Replace("com os", "nos").Replace("com as", "nas");

        private static string ExistsButNoMatch(Match m)
        {
            var parts = m.Groups["o"].Value.Split(' ', 2);
            var det = parts[0].ToLowerInvariant() switch { "um" => "o", "uma" => "a", var d => d };
            return char.ToUpperInvariant(det[0]) + det[1..] + " " + parts[1] + " existe, mas " +
                (m.Groups["n"].Success ? "não " : "") + "tem correspondência " + CorrespondencePrep(m.Groups["p"].Value) + " " + m.Groups["d"].Value;
        }

        private const string SentenceStart = @"(?<pre>(?:^|[.!?…:;]\s+|\n|(?<![\p{L}\p{M}])[-–—]\s*|[\uE000-\uF8FF]\s*)[""“«']?)";

        // A line break after a word is not a sentence start: "atrás desse\ncara, por causa..." keeps "cara".
        private const string MidSentenceBreak = @"(?<![\p{L}\p{N},][ \t]*\n[""“«']?)";

        private static readonly (Regex Regex, MatchEvaluator Evaluator)[] Rules =
        {
            // "Gostou que eu conduzi/paguei" → "Gostou de ser eu a conduzir/pagar".
            (new Regex(NotWordBefore +
                @"(?<v>gosto|gostas|gosta|gostamos|gostámos|gostam|gostava|gostavas|gostávamos|gostavam|gostei|gostou|gostaram)\s+que\s+eu\s+(?<action>\p{L}+)" +
                NotWordAfter, Ci),
                m => PortugueseVerbs.TryGetFirstPersonPastInfinitive(m.Groups["action"].Value, out var infinitive)
                    ? m.Groups["v"].Value + " de ser eu a " + infinitive
                    : m.Value),
            // Explicit calls/dials to Brazilian or US emergency services use the European emergency number.
            (new Regex(NotWordBefore +
                @"(?<pre>(?:lig(?:ue|uem|a|amos|uei|aram|ava|avam|ar|ando|ou|ará|aria)|" +
                @"cham(?:ou|a|e|em|amos|ei|aram|ava|avam|ar|ando|ará|aria)|" +
                @"telefon(?:ou|a|e|em|amos|ei|aram|ava|avam|ar|ando|ará|aria)|" +
                @"disc(?:ou|a|e|em|amos|ei|aram|ava|avam|ar|ando|ará|aria))\s+(?:(?:para|ao|a|o)\s+)?(?:o\s+)?)" +
                @"(?:190|9[\s-]?1[\s-]?1)" + NotWordAfter, Ci),
                m => m.Groups["pre"].Value + "112"),
            // In this temporal phrase, prefer "logo depois" in European Portuguese.
            (new Regex(NotWordBefore + @"(?<p>logo)(?<sp>\s+)(?<w>após)" + NotWordAfter, Ci),
                m => CaseHelper.MatchCase(m.Groups["p"].Value, "logo") + m.Groups["sp"].Value +
                     CaseHelper.MatchCase(m.Groups["w"].Value, "depois")),
            // "Que droga!" / "É uma droga." → "Que porcaria!" / "É uma porcaria." ("Droga!" alone → "Bolas!" further down).
            (new Regex(NotWordBefore + @"(?<w>que\s+droga)(?=\s*[,!.?…]|\s*$)", Ci),
                m => CaseHelper.MatchCase(m.Groups["w"].Value, "que porcaria")),
            (new Regex(NotWordBefore + @"(?<p>é|era|foi|isto\s+é|isso\s+é)\s+(?<w>uma\s+droga)(?=\s*[!.…]|\s*$)", Ci),
                m => m.Groups["p"].Value + " " + CaseHelper.MatchCase(m.Groups["w"].Value, "uma porcaria")),
            (new Regex(NotWordBefore + @"pare\s+de\s+drama(?:s)?\s*,\s*só\s+me\s+diga(?=$|[.!?…])", Ci),
                m => CaseHelper.MatchCase(m.Value, "Deixe-se de dramas, diga-me só")),
            (new Regex(NotWordBefore + @"(?<v>(?:coloque|coloca|ponha|põe)-(?:me|nos|o|a|os|as))(?<s1>\s+)(?<na>na)(?<s2>\s+)espera" + NotWordAfter, Ci),
                m => m.Groups["v"].Value + m.Groups["s1"].Value + CaseHelper.MatchCase(m.Groups["na"].Value, "em") +
                     m.Groups["s2"].Value + m.Value[^6..]),
            (new Regex(NotWordBefore + @"(?<pre>\b(?:é|e\s+não\s+é)\s+)?(?<dem>esse|essa|esses|essas)(?=\s+\p{L}+(?:\s+\p{L}+)?\s+aqui" + NotWordAfter + ")" , Ci),
                m => m.Groups["pre"].Value + CaseHelper.MatchCase(m.Groups["dem"].Value, m.Groups["dem"].Value.ToLowerInvariant() switch
                {
                    "esse" => "este", "essa" => "esta", "esses" => "estes", _ => "estas",
                })),
            (new Regex(NotWordBefore + @"(?<p>mas)\s+não\s+(?<t>hoje)(?=[.!?…]|\s*$)", Ci),
                m => m.Groups["p"].Value + " " + m.Groups["t"].Value + " não"),
            (new Regex(NotWordBefore + @"(?<p>acordas\b[^.!?…]*?\s+e\s+)vira(?=\s+um\s+pervertido)" , Ci),
                m => m.Groups["p"].Value + "tornas-te"),
            (new Regex(NotWordBefore + @"(?<p>numa)\s+batida\s+que\s+deu\s+errado" + NotWordAfter, Ci),
                m => m.Groups["p"].Value + " pancada que correu mal"),
            // A bare "policial" as the person involved ("tiroteio envolvendo policial", "baleado por policial") is "um agente" / "agentes".
            (new Regex(NotWordBefore + @"(?<v>envolver|envolve|envolvem|envolveu|envolveram|envolvendo|envolvid[oa]s?|baleado\s+por|baleada\s+por|morto\s+por|morta\s+por|ferido\s+por|ferida\s+por|atingido\s+por|atingida\s+por|agredido\s+por|agredida\s+por|atacado\s+por|atacada\s+por|baleou|baleiam|balearam|matar|mata|matou|mataram|ferir|feriu|feriram|agredir|agrediu|agrediram|atacar|atacou|atacaram|atingiu|atingiram|contra)\s+(?<w>policial|policiais)" + NotWordAfter, Ci),
                m => m.Groups["v"].Value + " " + (m.Groups["w"].Value.Equals("policiais", StringComparison.OrdinalIgnoreCase) ? "agentes" : "um agente")),
            // "A policial Janko" → "A agente Janko"; "melhor policial", "como policial", "disfarçado de policial" → "polícia".
            (new Regex(NotWordBefore + @"(?<a>o|um|a|uma|este|esta|esse|essa|aquele|aquela|nosso|nossa|meu|minha|teu|tua|seu|sua)\s+policial(?=\s+(?-i:\p{Lu}\p{Ll}))", Ci),
                m => m.Groups["a"].Value + " " + (char.IsUpper(m.Value[m.Groups["a"].Length + 1]) ? "Agente" : "agente")),
            (new Regex(NotWordBefore + @"(?<adj>melhor|pior|bom|boa|mau|má|grande|excelente|ótimo|corrupto|honesto|verdadeiro|verdadeira|novo|nova|velho|velha|jovem|como|enquanto|de|disfarçado\s+de|disfarçada\s+de)\s+(?<w>policial)(?!\s+(?:do|da|dos|das)\s+(?:bairro|cidade))" + NotWordAfter, Ci),
                m => m.Groups["adj"].Value + " " + CaseHelper.MatchCase(m.Groups["w"].Value, "polícia")),
            (new Regex(NotWordBefore + @"(?<p>ser|seres|sermos|serem|sou|és|é|somos|são|era|eram|fui|foi|foram|seja|sejas|sejam|fosse|fosses|fossem|sendo|sido|serei|será|serão|seria|seriam)\s+(?<w>policial|policiais)" + NotWordAfter, Ci),
                m => m.Groups["p"].Value + " " + CaseHelper.MatchCase(m.Groups["w"].Value,
                    m.Groups["w"].Value.Equals("policiais", StringComparison.OrdinalIgnoreCase) ? "polícias" : "polícia")),
            (new Regex(NotWordBefore + @"(?<p>de|dos|aos|pelos)(?<sp>\s+)(?<w>policiais)" + NotWordAfter, Ci),
                m => m.Groups["p"].Value + m.Groups["sp"].Value + CaseHelper.MatchCase(m.Groups["w"].Value, "polícias")),
            (new Regex(SentenceStart + @"(?<w>policial)(?=\s*,)", Ci),
                m => m.Groups["pre"].Value + CaseHelper.MatchCase(m.Groups["w"].Value, "polícia")),
            (new Regex(NotWordBefore + @"(?<w>dei|deu|deram|demos|dás|dou)\s+sorte" + NotWordAfter, Ci),
                m => CaseHelper.MatchCase(m.Value, m.Groups["w"].Value.ToLowerInvariant() switch
                {
                    "dei" => "tive sorte", "deu" => "teve sorte", "deram" => "tiveram sorte", "demos" => "tivemos sorte",
                    "dás" => "tens sorte", _ => "tenho sorte",
                })),
            (new Regex(NotWordBefore + @"(?<trig>(?:não|nunca|jamais|já|que|também)\s+)?(?<cl>me\s+)?(?<w>esqueci)(?<sp>\s+)(?<p>de|disso|disto|que)" + NotWordAfter, Ci),
                m => m.Groups["cl"].Success ? m.Value :
                     (m.Groups["trig"].Success ? m.Groups["trig"].Value + (CaseHelper.IsAllUpper(m.Value) ? "ME " : "me ") + m.Groups["w"].Value :
                     CaseHelper.MatchCase(m.Groups["w"].Value, "esqueci-me")) + m.Groups["sp"].Value +
                     (m.Groups["p"].Value.Equals("que", StringComparison.OrdinalIgnoreCase) ? (CaseHelper.IsAllUpper(m.Value) ? "DE " : "de ") : string.Empty) + m.Groups["p"].Value),
            (new Regex(NotWordBefore + @"(?<w>tudo)(?<sp>\s+)que(?=\s+(?!é" + NotWordAfter + @")\p{L})", Ci),
                m => m.Groups["w"].Value + m.Groups["sp"].Value + (CaseHelper.IsAllUpper(m.Value) ? "O QUE" : "o que")),
            (new Regex(@"(?<p>,\s*)(?<w>mesma\s+idade)(?=\s+d[aeo]\s+)", Ci),
                m => m.Groups["p"].Value + "da " + m.Groups["w"].Value),
            // "Tá bom." / "Está bom!" as an interjection → "Está bem."
            (new Regex(SentenceStart + @"(?<w>tá|está|ta)\s+bom(?=\s*[.!?…,]|\s*$)", Ci), m => m.Groups["pre"].Value + CaseHelper.MatchCase(m.Groups["w"].Value, "está") + " bem"),
            (new Regex(@"(?<pre>,\s*|^|[.!?…:;]\s+|\n|(?<![\p{L}\p{M}])[-–—]\s*|[\uE000-\uF8FF]\s*)(?<w>tá)(?=\s*[?.,!…]|\s*$)", Ci), m => m.Groups["pre"].Value + CaseHelper.MatchCase(m.Groups["w"].Value, "está") + " bem"),
            (new Regex(NotWordBefore + @"(?:tá|tah)" + NotWordAfter, Ci), m => CaseHelper.MatchCase(m.Value, "está")),
            (new Regex(NotWordBefore + @"tô" + NotWordAfter, Ci), m => CaseHelper.MatchCase(m.Value, "estou")),
            (new Regex(NotWordBefore + @"tava(?<s>s|m)?" + NotWordAfter, Ci), m => CaseHelper.MatchCase(m.Value, "estava" + m.Groups["s"].Value.ToLowerInvariant())),
            (new Regex(NotWordBefore + @"tamos?" + NotWordAfter, Ci), m => CaseHelper.MatchCase(m.Value, "estamos")),
            (new Regex(NotWordBefore + @"tão\s+(?=a\s+\p{Ll}+r" + NotWordAfter + ")", Ci), m => CaseHelper.MatchCase(m.Value, "estão ")),
            (new Regex(NotWordBefore + @"praquel(?<e>e|a|es|as)" + NotWordAfter, Ci), m => CaseHelper.MatchCase(m.Value, "para aquel" + m.Groups["e"].Value)),
            (new Regex(NotWordBefore + @"pra" + NotWordAfter, Ci), m => CaseHelper.MatchCase(m.Value, "para")),
            (new Regex(NotWordBefore + @"pr(?<a>os|as|o|a)" + NotWordAfter + @"(?=\s+\p{L})", Ci), m => CaseHelper.MatchCase(m.Value, "para " + m.Groups["a"].Value.ToLowerInvariant())),
            (new Regex(NotWordBefore + @"pro" + NotWordAfter, Ci), m => CaseHelper.MatchCase(m.Value, "para o")),
            (new Regex(NotWordBefore + @"né" + NotWordAfter, Ci), m => CaseHelper.MatchCase(m.Value, "não é")),
            (new Regex(NotWordBefore + @"cê" + NotWordAfter, Ci), m => CaseHelper.MatchCase(m.Value, "você")),
            (new Regex(@"(?<pre>(?:^|\n|[.!?…]\s+|(?<![\p{L}\p{M}])[-–—]\s*)(?:entra|sai|anda|vem|despacha-te|senta-te|ouve|olha|fala|faz|tenta|continua)" +
                NotWordAfter + @"[^.!?\n]{0,60},\s*)(?<w>vai)(?=\s*[.!…]|\s*$)", Ci),
                m => m.Groups["pre"].Value + CaseHelper.MatchCase(m.Groups["w"].Value, "vá")),
            (new Regex(NotWordBefore + @"(?<a>a)(?<sp>\s+)(?<det>essa|esta)(?=\s+hora" + NotWordAfter + ")", Ci),
                m => CaseHelper.MatchCase(m.Groups["a"].Value, "a") + m.Groups["sp"].Value + CaseHelper.MatchCase(m.Groups["det"].Value, "esta")),
            (new Regex(NotWordBefore + @"cadê\s+(?<x>(?:os|as|eles|elas|vocês|meus|minhas|teus|tuas|seus|suas|nossos|nossas|estes|estas|esses|essas|aqueles|aquelas)" + NotWordAfter + ")", Ci), m => CaseHelper.MatchCase(m.Value, "onde estão ") + m.Groups["x"].Value),
            (new Regex(NotWordBefore + @"cadê\s+(?:você|tu)" + NotWordAfter, Ci), m => CaseHelper.MatchCase(m.Value, "onde estás")),
            (new Regex(NotWordBefore + @"cadê" + NotWordAfter, Ci), m => CaseHelper.MatchCase(m.Value, "onde está")),
            (new Regex(NotWordBefore + @"(?<v>deixa|deixe|deixem)\s+eu" + NotWordAfter, Ci), m => m.Groups["v"].Value + (CaseHelper.IsAllUpper(m.Groups["v"].Value) ? "-ME" : "-me")),
            // Seen often in real pt-BR → pt-PT subtitle pairs.
            (new Regex(SentenceStart + @"(?<w>me\s+desculp(?<e>a|e|em))" + NotWordAfter + @"(?!\s*(?:por|pelo|pela|se|de|ter)" + NotWordAfter + ")", Ci),
                m => m.Groups["pre"].Value + CaseHelper.MatchCase(m.Groups["w"].Value, "desculp" + m.Groups["e"].Value.ToLowerInvariant())),
            (new Regex(NotWordBefore + @"(?:está|tá)\s+legal" + NotWordAfter, Ci), m => CaseHelper.MatchCase(m.Value, "está bem")),
            (new Regex(SentenceStart + @"(?<w>jura|sério\s+mesmo)(?=\s*\?)", Ci), m => m.Groups["pre"].Value + CaseHelper.MatchCase(m.Groups["w"].Value, "a sério")),
            (new Regex(SentenceStart + @"(?<w>valeu)(?!\s+a\s+pena)" + NotWordAfter, Ci), m => m.Groups["pre"].Value + CaseHelper.MatchCase(m.Groups["w"].Value, "obrigado")),
            (new Regex(@"(?<pre>,\s+)(?<w>valeu)(?=\s*[.!…]|\s*$)", Ci), m => m.Groups["pre"].Value + CaseHelper.MatchCase(m.Groups["w"].Value, "obrigado")),
            (new Regex(SentenceStart + @"(?<w>(?:eu\s+)?te\s+vejo)(?=\s+(?:lá|aí|amanhã|depois|logo|mais\s+tarde|em\s+breve|no|na|nos|nas|do\s+outro|à\s+noite|hoje|daqui|segunda|terça|quarta|quinta|sexta|sábado|domingo)" + NotWordAfter + @"|\s*[.!]|\s*$)", Ci),
                m => m.Groups["pre"].Value + CaseHelper.MatchCase(m.Groups["w"].Value, "vemo-nos")),
            (new Regex(@"(?<pre>(?:^|\n|,\s+)[""“«']?)(?<w>(?-i:falando))(?=\s+(?:em|de|do|da|dos|das|sobre|com)" + NotWordAfter + ")", Ci), m => m.Groups["pre"].Value + "a falar"),
            (new Regex(SentenceStart + @"(?<w>(?-i:Falando|FALANDO))(?=\s+(?:nisso|nisto|em|de|do|da|dos|das|no|na|nos|nas|sobre)" + NotWordAfter + ")", Ci), m => m.Groups["pre"].Value + CaseHelper.MatchCase(m.Groups["w"].Value, "por falar")),
            (new Regex(SentenceStart + @"(?<w>imagina)(?=\s*[.!]|\s*$)", Ci), m => m.Groups["pre"].Value + CaseHelper.MatchCase(m.Groups["w"].Value,
                Regex.IsMatch(m.Result("$`"), @"obrigad|valeu", RegexOptions.IgnoreCase) ? "de nada" : "não faz mal")),
            (new Regex(@"(?<pre>(?:^|[.!?…:;]\s+|\n|(?<![\p{L}\p{M}])[-–—]\s*|[\uE000-\uF8FF]\s*|" + NotWordBefore + @"(?:que|não|se|ainda|já|porque|onde|mas|e|aqui|ali|lá|aí)\s+)[""“«']?)(?<w>tem)(?=\s+(?:gente|alguém|ninguém)" + NotWordAfter + ")", Ci),
                m => m.Groups["pre"].Value + CaseHelper.MatchCase(m.Groups["w"].Value, "há")),
            (new Regex(SentenceStart + @"(?<w>ok|okay|okey)(?=\s*[.!?,…]|\s*$)", Ci), m => m.Groups["pre"].Value + CaseHelper.MatchCase(m.Groups["w"].Value, "está bem")),
            (new Regex(@"(?<pre>,\s*)(?<w>ok|okay)(?=\s*[?.!]|\s*$)", Ci), m => m.Groups["pre"].Value + "está bem"),
            (new Regex(@"(?<pre>,\s*)(?<w>hein)(?=\s*\?)", Ci), m => m.Groups["pre"].Value + CaseHelper.MatchCase(m.Groups["w"].Value, "não")),
            (new Regex(SentenceStart + @"(?<w>hein)(?=\s*\?)", Ci), m => m.Groups["pre"].Value + CaseHelper.MatchCase(m.Groups["w"].Value, "o quê")),
            (new Regex(SentenceStart + @"(?<w>ei)(?=\s*,)", Ci), m => m.Groups["pre"].Value + CaseHelper.MatchCase(m.Groups["w"].Value, "olha")),
            (new Regex(SentenceStart + @"(?<w>lógico|beleza)(?=\s*[.!]|\s*$)", Ci), m => m.Groups["pre"].Value + CaseHelper.MatchCase(m.Groups["w"].Value,
                m.Groups["w"].Value.Equals("lógico", StringComparison.OrdinalIgnoreCase) ? "claro" : "fixe")),
            (new Regex(NotWordBefore + @"(?<v>ver|vendo|vejo|vês|vê|vemos|veem|via|vias|viam|vi|viu|viste|vimos|viram|estar\s+a\s+ver|a\s+ver)(?<t>\s+tudo)?\s+(?<w>dobrado|dobrada|dobrados|dobradas|duplo|em\s+dobro)" + NotWordAfter, Ci),
                m => m.Groups["v"].Value + m.Groups["t"].Value + " " + CaseHelper.MatchCase(m.Groups["w"].Value, "a dobrar")),
            (new Regex(NotWordBefore + @"(?<w>puta|puto|putas|putos)\s+da\s+vida" + NotWordAfter, Ci), m => CaseHelper.MatchCase(m.Groups["w"].Value,
                m.Groups["w"].Value.ToLowerInvariant() switch { "puta" => "furiosa", "puto" => "furioso", "putas" => "furiosas", _ => "furiosos" })),
            (new Regex(NotWordBefore + @"(?<w>puta\s+merda)" + NotWordAfter, Ci), m => CaseHelper.MatchCase(m.Groups["w"].Value, "raios")),
            (new Regex(SentenceStart + @"(?<w>ache|achem)(?=\s+(?:o|a|os|as|um|uma|uns|umas|alguém|algo|alguma|algum)" + NotWordAfter + ")", Ci),
                m => m.Groups["pre"].Value + CaseHelper.MatchCase(m.Groups["w"].Value, m.Groups["w"].Value.ToLowerInvariant() == "ache" ? "encontre" : "encontrem")),
            (new Regex(@"(?<w>(?:ah,\s*)?puta\s+que\s+(?:o\s+|te\s+)?pariu)", Ci), m => CaseHelper.MatchCase(m.Groups["w"].Value, "merda")),
            (new Regex(NotWordBefore + @"(?<d>um|uma|numa|num|que|essa|esse|nessa|nesse|dessa|desse)\s+(?<w>puta)\s+(?=\p{Ll}{3,}(?<!merda|vida|madre)" + NotWordAfter + @"(?!\s+(?:que|de|da|do))[^.!?]*)", Ci),
                m => m.Groups["d"].Value + " " + CaseHelper.MatchCase(m.Groups["w"].Value, "grande") + " "),
            (new Regex(NotWordBefore + @"(?<q>onde|aonde|o\s+que|quando|como|quem|para\s+onde|porque|por\s+que|que\s+horas)\s+(?<p>ele|ela|eles|elas)\s+(?<v>está|estão|estava|estavam|é|são|era|eram|foi|foram|vai|vão|ia|iam|fica|ficam|ficou|mora|moram|vive|vivem|trabalha|trabalham)(?=\s*\?)", Ci),
                m => m.Groups["q"].Value + " " + m.Groups["v"].Value + " " + m.Groups["p"].Value),
            (new Regex(NotWordBefore + @"(?<v>estou|estás|está|estamos|estão|estava|estavas|estávamos|estavam)\s+com\s+(?<s>saudade|saudades)" + NotWordAfter, Ci),
                m => CaseHelper.MatchCase(m.Groups["v"].Value, m.Groups["v"].Value.ToLowerInvariant() switch
                {
                    "estou" => "tenho", "estás" => "tens", "está" => "tem", "estamos" => "temos", "estão" => "têm",
                    "estava" => "tinha", "estavas" => "tinhas", "estávamos" => "tínhamos", _ => "tinham",
                }) + " saudades"),
            (new Regex(SentenceStart + @"(?:(?<a>aqui|toma)\s*,\s*)?(?<w>pega|pegue|peguem)(?=\s*[.!]|\s*$)", Ci),
                m => m.Groups["pre"].Value + (m.Groups["a"].Success ? m.Groups["a"].Value + ", " : string.Empty) +
                    CaseHelper.MatchCase(m.Groups["w"].Value, m.Groups["w"].Value.ToLowerInvariant() switch { "pega" => "toma", "pegue" => "tome", _ => "tomem" })),
            (new Regex(NotWordBefore + @"(?<v>vai|vá|vão|vai\s+lá)\s+(?:se|te)\s+(?<w>foder|ferrar|danar|catar|lascar|ferrar)" + NotWordAfter, Ci),
                m =>
                {
                    var v = m.Groups["v"].Value;
                    var lower = v.ToLowerInvariant();
                    var verb = lower == "vão" ? v + "-se" : lower.StartsWith("vai") && lower.Length > 3 ? CaseHelper.MatchCase(v[..3], "vai") + "-te" + v[3..] : CaseHelper.MatchCase(v, "vai") + "-te";
                    return verb + " " + CaseHelper.MatchCase(m.Groups["w"].Value, m.Groups["w"].Value.ToLowerInvariant() == "foder" ? "foder" : "lixar");
                }),
            (new Regex(NotWordBefore + @"(?<v>está|tá|estava|tudo)\s+(?<w>beleza)" + NotWordAfter, Ci),
                m => CaseHelper.MatchCase(m.Groups["v"].Value, m.Groups["v"].Value.ToLowerInvariant() == "tudo" ? "tudo" : m.Groups["v"].Value.ToLowerInvariant() == "estava" ? "estava" : "está") + " bem"),
            (new Regex(@"(?<pre>,\s+)(?<w>beleza)(?=\s*[.!…]|\s*$)", Ci), m => m.Groups["pre"].Value + CaseHelper.MatchCase(m.Groups["w"].Value, "tudo bem")),
            (new Regex(SentenceStart + @"(?<w>beleza)(?=\s*(?:,\s*\p{L}+\s*)?\?)", Ci), m => m.Groups["pre"].Value + CaseHelper.MatchCase(m.Groups["w"].Value, "tudo bem")),
            (new Regex(NotWordBefore + @"(?<v>cai|caia|caiam|cair|caí|caiu|caímos)\s+(?<w>fora)" + NotWordAfter + @"(?<from>(?=\s+(?:daqui|dali|daí|de|da|do|das|dos|desta|deste|dessa|desse)" + NotWordAfter + "))?", Ci),
                m => CaseHelper.MatchCase(m.Groups["v"].Value, m.Groups["v"].Value.ToLowerInvariant() switch
                {
                    "cai" => "sai", "caia" => "saia", "caiam" => "saiam", "cair" => "sair", "caí" => "saí", "caiu" => "saiu", _ => "saímos",
                }) + (m.Groups["from"].Success ? string.Empty : " daqui")),
            (new Regex(SentenceStart + MidSentenceBreak + @"(?<w>cara)(?=\s*,)", Ci), m => m.Groups["pre"].Value + CaseHelper.MatchCase(m.Groups["w"].Value, "meu")),
            (new Regex(@"(?<pre>,\s*)(?<w>cara)(?=\s*[.!?…]|\s*$)", Ci), m => m.Groups["pre"].Value + CaseHelper.MatchCase(m.Groups["w"].Value, "meu")),
            (new Regex(SentenceStart + MidSentenceBreak + @"(?<w>gente)(?=\s*[,!?])", Ci), m => m.Groups["pre"].Value + CaseHelper.MatchCase(m.Groups["w"].Value, "pessoal")),
            (new Regex(@"(?<pre>,\s*)(?<w>gente)(?=\s*[.!?…]|\s*$)", Ci), m => m.Groups["pre"].Value + CaseHelper.MatchCase(m.Groups["w"].Value, "pessoal")),
            (new Regex(SentenceStart + @"(?<w>não\s+precisa)(?=\s*[.!,…]|\s*$)", Ci), m => m.Groups["pre"].Value + CaseHelper.MatchCase(m.Groups["w"].Value, "não é preciso")),
            (new Regex(SentenceStart + @"(?<w>escuta|escute)(?=\s*[,.!])", Ci), m => m.Groups["pre"].Value + CaseHelper.MatchCase(m.Groups["w"].Value, m.Groups["w"].Value.EndsWith("a", StringComparison.OrdinalIgnoreCase) ? "ouve" : "ouça")),
            (new Regex(SentenceStart + @"(?<w>senta)(?:\s+(?:aí|ali|lá))?(?=\s*[,.!]|\s+aqui" + NotWordAfter + ")", Ci), m => m.Groups["pre"].Value + CaseHelper.MatchCase(m.Groups["w"].Value, "senta-te")),
            (new Regex(NotWordBefore + @"(?<v>pega|pegue)\s+leve" + NotWordAfter, Ci), m => CaseHelper.MatchCase(m.Groups["v"].Value, m.Groups["v"].Value.EndsWith("a", StringComparison.OrdinalIgnoreCase) ? "vai" : "vá") + " com calma"),
            (new Regex(NotWordBefore + @"tomara\s+que" + NotWordAfter, Ci), m => CaseHelper.MatchCase(m.Value, "espero que")),
            (new Regex(SentenceStart + @"(?<w>tomara)(?=\s*[!.])", Ci), m => m.Groups["pre"].Value + CaseHelper.MatchCase(m.Groups["w"].Value, "oxalá")),
            (new Regex(NotWordBefore + @"todo\s+(?:o\s+)?mundo" + NotWordAfter, Ci), m => CaseHelper.MatchCase(m.Value, "toda a gente")),
            (new Regex(NotWordBefore + @"pelo\s+jeito" + NotWordAfter, Ci), m => CaseHelper.MatchCase(m.Value, "pelos vistos")),
            (new Regex(NotWordBefore + @"do\s+jeito\s+que" + NotWordAfter, Ci), m => CaseHelper.MatchCase(m.Value, "como")),
            (new Regex(NotWordBefore + @"(?<d>o|um|de\s+um|do|no|esse|desse|nesse|este|deste|neste|que|qualquer|outro|de\s+outro|do\s+mesmo|o\s+mesmo|de\s+qualquer)\s+jeito(?:\s+(?<adj>certo|errado|certeiro|estranho|engraçado|esquisito|novo|velho|perfeito|honesto|educado|bonito|lindo|fofo|louco|maluco|antigo|único|próprio|exato|tradicional|normal|rápido|direto|chato|típico))?" + NotWordAfter, Ci), m =>
            {
                var d = m.Groups["d"].Value;
                var fem = Regex.Replace(d.ToLowerInvariant(), @"\b(?:o|um|do|no|esse|desse|nesse|este|deste|neste|outro|mesmo)\b", w => w.Value switch
                {
                    "o" => "a", "um" => "uma", "do" => "da", "no" => "na", "esse" => "essa", "desse" => "dessa", "nesse" => "nessa",
                    "este" => "esta", "deste" => "desta", "neste" => "nesta", "outro" => "outra", _ => "mesma",
                });
                var adj = m.Groups["adj"].Value;
                if (adj.Length > 0 && adj.EndsWith("o", StringComparison.Ordinal))
                {
                    adj = adj[..^1] + "a";
                }

                return CaseHelper.MatchCase(d, fem) + " forma" + (adj.Length > 0 ? " " + adj : string.Empty);
            }),
            (new Regex(NotWordBefore + @"(?<v>dar|dá|dou|deu|dei|vou\s+dar|vim\s+dar)\s+(?:um\s+)?oi" + NotWordAfter, Ci), m => CaseHelper.MatchCase(m.Groups["v"].Value, m.Groups["v"].Value.ToLowerInvariant() switch
            {
                "dar" => "dizer", "dá" => "diz", "dou" => "digo", "deu" => "disse", "dei" => "disse", "vou dar" => "vou dizer", _ => "vim dizer",
            }) + " olá"),
            (new Regex(NotWordBefore + @"(?<v>manda|mande)\s+(?:um\s+)?(?:oi|beijo|abraço)" + NotWordAfter, Ci), m => m.Groups["v"].Value + " cumprimentos"),
            (new Regex(NotWordBefore + @"bater\s+(?:um\s+)?papo" + NotWordAfter, Ci), m => CaseHelper.MatchCase(m.Value, "conversar")),
            (new Regex(SentenceStart + @"(?<w>oi|oie)" + NotWordAfter, Ci), m => m.Groups["pre"].Value + CaseHelper.MatchCase(m.Groups["w"].Value, "olá")),
            (new Regex(SentenceStart + @"(?<w>alô)(?=\s*[?!.,…])", Ci), m => m.Groups["pre"].Value + CaseHelper.MatchCase(m.Groups["w"].Value, "estou")),
            (new Regex(SentenceStart + @"(?<w>(?:minha\s+)?nossa(?:\s+senhora)?)(?=\s*[!,.…?])", Ci), m => m.Groups["pre"].Value + CaseHelper.MatchCase(m.Groups["w"].Value, "caramba")),
            (new Regex(SentenceStart + @"(?<w>poxa(?:\s+vida)?|puxa\s+vida|puxa(?=\s*[!,.…?]|\s*$))" + NotWordAfter, Ci), m => m.Groups["pre"].Value + CaseHelper.MatchCase(m.Groups["w"].Value, "bolas")),
            (new Regex(SentenceStart + @"(?<w>droga)(?=\s*[!.…]|,\s+\p{Lu})", Ci), m => m.Groups["pre"].Value + CaseHelper.MatchCase(m.Groups["w"].Value, "bolas")),
            (new Regex(NotWordBefore + @"que\s+droga" + NotWordAfter, Ci), m => CaseHelper.MatchCase(m.Value, "que chatice")),
            (new Regex(NotWordBefore + @"(?:fala|falando|falo)\s+sério" + NotWordAfter, Ci), m => CaseHelper.MatchCase(m.Value, "a sério")),
            // "Sério?" / "Sério mesmo?" / "Sério, ..." → "A sério?"
            (new Regex(SentenceStart + @"(?<w>sério)(?:\s+mesmo)?(?=\s*[?!.,…]|\s*$)", Ci), m => m.Groups["pre"].Value + CaseHelper.MatchCase(m.Groups["w"].Value, "a sério")),
            (new Regex(SentenceStart + @"(?<w>e\s+aí|ué)(?=\s*[?!,.…])", Ci), m => m.Groups["pre"].Value + CaseHelper.MatchCase(m.Groups["w"].Value, "então")),
            (new Regex(@"(?<pre>(?:^|[.!?…:;]\s+|\n|(?<![\p{L}\p{M}])[-–—]\s*|[\uE000-\uF8FF]\s*|,\s*))(?<w>beleza)(?=\s*[?])", Ci), m => m.Groups["pre"].Value + CaseHelper.MatchCase(m.Groups["w"].Value, "tudo bem")),
            // Reassurance "Tudo bem!" / "Tudo bem, tudo bem." → "Está tudo bem!" ("Tudo bem?" as a greeting is unchanged).
            (new Regex(SentenceStart + @"(?<w>tudo\s+bem)(?=\s*(?:!|,\s*tudo\s+bem))", Ci), m => m.Groups["pre"].Value + CaseHelper.MatchCase(m.Groups["w"].Value, "está tudo bem")),
            (new Regex(@"(?<pre>(?i:está\s+tudo\s+bem),\s*)(?<w>tudo\s+bem)" + NotWordAfter, Cs), m => m.Groups["pre"].Value + CaseHelper.MatchCase(m.Groups["w"].Value, "está tudo bem")),
            (new Regex(SentenceStart + @"(?<w>pô|pôxa)" + NotWordAfter, Ci), m => m.Groups["pre"].Value + CaseHelper.MatchCase(m.Groups["w"].Value, "bolas")),
            (new Regex(SentenceStart + @"(?<w>tchau|tchauzinho)" + NotWordAfter, Ci), m => m.Groups["pre"].Value + CaseHelper.MatchCase(m.Groups["w"].Value, "adeus")),
            (new Regex(SentenceStart + @"(?<w>pois\s+não)(?=\s*\?)", Ci), m => m.Groups["pre"].Value + CaseHelper.MatchCase(m.Groups["w"].Value, "diga")),
            (new Regex(NotWordBefore + @"que\s+saco" + NotWordAfter, Ci), m => CaseHelper.MatchCase(m.Value, "que seca")),
            (new Regex(NotWordBefore + @"(?:tá|está)\s+ligado(?=\s*\?)", Ci), m => CaseHelper.MatchCase(m.Value, "percebes")),
            (new Regex(NotWordBefore + @"sacou(?=\s*\?)", Ci), m => CaseHelper.MatchCase(m.Value, "percebeste")),
            (new Regex(NotWordBefore + @"(?<v>fica|fique|fiquem)\s+frio" + NotWordAfter, Ci), m => CaseHelper.MatchCase(m.Groups["v"].Value, m.Groups["v"].Value.ToLowerInvariant() switch { "fica" => "tem", "fique" => "tenha", _ => "tenham" }) + " calma"),
            (new Regex(NotWordBefore + @"de\s+jeito\s+nenhum" + NotWordAfter, Ci), m => CaseHelper.MatchCase(m.Value, "de maneira nenhuma")),
            (new Regex(NotWordBefore + @"(?<v>estou|estás|está|estamos|estão|estava|estavas|estávamos|estavam|tô|tá|fiquei|ficou)\s+a\s+fim\s+de" + NotWordAfter, Ci), m => m.Groups["v"].Value + " com vontade de"),
            (new Regex(NotWordBefore + @"que\s+legal" + NotWordAfter, Ci), m => CaseHelper.MatchCase(m.Value, "que fixe")),
            (new Regex(SentenceStart + @"(?<w>legal)(?=\s*[!.…])", Ci), m => m.Groups["pre"].Value + CaseHelper.MatchCase(m.Groups["w"].Value, "fixe")),
            (new Regex(SentenceStart + @"(?<w>beleza)(?=\s*[!?.…])", Ci), m => m.Groups["pre"].Value + CaseHelper.MatchCase(m.Groups["w"].Value, "combinado")),
            (new Regex(SentenceStart + @"(?<w>valeu)(?=\s*[!.,…])", Ci), m => m.Groups["pre"].Value + CaseHelper.MatchCase(m.Groups["w"].Value, "obrigado")),
            // ter que + infinitivo → ter de + infinitivo ("tem que ver com" → "tem a ver com")
            (new Regex(NotWordBefore + @"(?<t>tem|tinha|têm|tenha|ter)\s+que\s+ver\s+com" + NotWordAfter, Ci), m => m.Groups["t"].Value + " a ver com"),
            (new Regex(NotWordBefore + @"(?<t>tenho|tens|tem|temos|têm|tinha|tinhas|tínhamos|tinham|tive|tiveste|teve|tivemos|tiveram|terei|terás|terá|teremos|terão|teria|terias|teríamos|teriam|ter|tenha|tenhas|tenhamos|tenham|tendo|tiver|tiveres|tivermos|tiverem|tivesse|tivesses|tivéssemos|tivessem)\s+que(?=\s+(?:me\s+|te\s+|se\s+|nos\s+|lhe\s+|o\s+|a\s+)?(?:\p{L}*(?:ar|er|ir|ôr|or)" + NotWordAfter + @"|\p{L}+-(?:l[oa]s?|me|te|se|nos|vos|lhes?)" + NotWordAfter + "))", Ci),
                m => m.Groups["t"].Value + (CaseHelper.IsAllUpper(m.Value) ? " DE" : " de")),
            (new Regex(NotWordBefore + @"(?<t>tenho|tens|tem|temos|têm|tinha|tinhas|tínhamos|tinham|tive|tiveste|teve|tivemos|tiveram|ter|tenha|tenhas|tiver|tivesse)\s+(?<c>certeza)" + NotWordAfter, Ci),
                m => m.Groups["t"].Value + (CaseHelper.IsAllUpper(m.Value) ? " A " : " a ") + m.Groups["c"].Value),
            // precisar + infinitivo → precisar de + infinitivo
            (new Regex(NotWordBefore + @"(?<p>preciso|precisas|precisa|precisamos|precisam|precisava|precisavas|precisávamos|precisavam|precisei|precisaste|precisou|precisámos|precisaram|precisar|precise|precises|precisem|precisaria|precisarias|precisariam|precisará|precisarás|precisarão|precisasse|precisares|precisarem)\s+(?=(?:(?:me|te|se|nos|lhe|o|a)\s+)?\p{L}*(?:ar|er|ir|ôr)(?!\p{L}))", Ci),
                m => m.Groups["p"].Value + (CaseHelper.IsAllUpper(m.Groups["p"].Value) ? " DE " : " de ")),
            (new Regex(SentenceStart + @"(?<w>por\s+(?:isso|isto|aquilo))(?<sp>\s+)(?<que>que)" + NotWordAfter, Ci),
                m => m.Groups["pre"].Value + m.Groups["w"].Value + m.Groups["sp"].Value +
                     (CaseHelper.IsAllUpper(m.Groups["w"].Value) ? "É " : "é ") + m.Groups["que"].Value),
            // "Por quê?" → "Porquê?", "Por que não vens?" → "Porque não vens?" ("o motivo por que" keeps the relative).
            (new Regex(@"(?<sp>[ \t]*)" + NotWordBefore + @"(?<w>por\s+quê)(?=\s*[?!.…,]|\s*$)", Ci), m => JoinWords(m, "porquê")),
            (new Regex(@"(?<sp>[ \t]*)(?<!(?:motivo|motivos|razão|razões|causa|causas|luta|lutas|lutar|lutou|luto|passou|passei|passar|passaram|passámos)\s+)" + NotWordBefore + @"(?<w>por\s+que)" + NotWordAfter, Ci),
                m => JoinWords(m, "porque")),
            // Existential "tem" → "há": "Tem um tipo lá fora" → "Há um tipo lá fora", "Não tem como" → "Não há forma de".
            (new Regex(NotWordBefore + @"(?<no>não\s+)?(?<w>tem)\s+como(?=\s+\p{L})", Ci),
                m => m.Groups["no"].Value + CaseHelper.MatchCase(m.Groups["w"].Value, "há") + (CaseHelper.IsAllUpper(m.Value) ? " FORMA DE" : " forma de")),
            (new Regex(SentenceStart + @"(?<no>não\s+)?(?<w>tem)(?=\s+problema" + NotWordAfter + ")", Ci),
                m => m.Groups["pre"].Value + m.Groups["no"].Value + CaseHelper.MatchCase(m.Groups["w"].Value, "há")),
            (new Regex(SentenceStart + @"(?<w>sempre)\s+(?<t>tem)(?=\s+(?:um|uma|uns|umas|algum|alguma|alguém|algo|outro|outra|outros|outras|mais|gente)" + NotWordAfter + ")", Ci),
                m => m.Groups["pre"].Value + CaseHelper.MatchCase(m.Groups["w"].Value, "há sempre")),
            (new Regex(SentenceStart + @"(?<a>(?:mas|e|agora|aqui|ali|lá|então)\s*,?\s+)?(?<no>não\s+)?(?<w>tem)(?=\s+(?:um|uma|uns|umas|algum|alguma|alguns|algumas|muito|muita|muitos|muitas|outro|outra|outros|outras|algo|nada|várias|vários|alguém|ninguém|nenhum|nenhuma|tanta|tanto|tantas|tantos)" + NotWordAfter +
                @"(?!\s+(?:(?:reunião|reuniões|ligação|ligações|chamada|chamadas|consulta|consultas|encontro|encontros|entrevista|entrevistas|audiência|audiências|compromisso|compromissos|sessão|sessões|briefing|briefings|aula|aulas|exame|exames|jantar|jantares|voo|voos|apresentação|apresentações|visita|visitas)" + NotWordAfter + @"(?=\s+(?:com|às|as|à|hoje|amanhã|depois|marcad\p{L}+|agendad\p{L}+|para\s+(?:hoje|amanhã|as|às))" + NotWordAfter + @")))(?!\s+(?:bom|boa|óptimo|ótimo|ótima|óptima|excelente|feliz|resto)" + NotWordAfter + @")(?!\s+(?:filho|filha|filhos|filhas|irmão|irmã|irmãos|irmãs|namorado|namorada|mulher|marido|pai|mãe|casa|carro|emprego|cão|gato|ideia|plano|minuto|segundo)" + NotWordAfter + @"))(?![^.!?\n]*\?)(?!\s+nada\s+(?:a|que)\s+ver" + NotWordAfter + ")", Ci),
                m => m.Groups["pre"].Value + m.Groups["a"].Value + m.Groups["no"].Value + CaseHelper.MatchCase(m.Groups["w"].Value, "há")),
            // "Que tipo de cara" → "Que tipo de homem"; the vocative ", cara," → ", pá,"; a one-word "Que?" → "Quê?".
            (new Regex(NotWordBefore + @"(?<t>tipo\s+de)\s+(?<w>cara)" + NotWordAfter, Ci),
                m => m.Groups["t"].Value + " " + CaseHelper.MatchCase(m.Groups["w"].Value, "homem")),
            (new Regex(@"(?<=,[ \t]+)cara(?=[ \t]*[,!?.…])"), m => "pá"),
            (new Regex(SentenceStart + @"(?<w>Que)(?=\?)"), m => m.Groups["pre"].Value + "Quê"),
            // "para mim fazer" → "para eu fazer", "ao invés de" → "em vez de".
            (new Regex(NotWordBefore + @"(?<p>para|pra)\s+mim(?<s>\s+)(?<v>\p{L}+(?:ar|er|ir|or|ôr))" + NotWordAfter + @"(?!-)", Ci),
                m => PortugueseVerbs.IsInfinitive(m.Groups["v"].Value) && !PortugueseVerbs.IsFutureOrConditional(m.Groups["v"].Value)
                    ? CaseHelper.MatchCase(m.Groups["p"].Value, "para") + " " + (CaseHelper.IsAllUpper(m.Groups["p"].Value) ? "EU" : "eu") + m.Groups["s"].Value + m.Groups["v"].Value
                    : m.Value),
            (new Regex(NotWordBefore + @"ao\s+invés(?=\s+(?:de|d[aeo]s?|disso|disto|daquilo)" + NotWordAfter + ")", Ci),
                m => CaseHelper.MatchCase(m.Value, "em vez")),
            // Unaccented "Ta/Esta" before a gerund is the verb ("Ta brincando?" → "Está brincando", then "Estás a brincar").
            (new Regex(NotWordBefore + @"(?<w>ta|esta)(?=\s+(?!(?:lindo|segundo|fundo|mundo|quando|profundo|redondo|comando|bando|rondo)" + NotWordAfter + @")\p{L}+(?:ando|endo|indo)" + NotWordAfter + ")", Ci),
                m => CaseHelper.MatchCase(m.Value, "está")),
            // "pegar leve (com)" → "ir com calma" / "ir devagar com" (take it easy / go easy on).
            (new Regex(NotWordBefore + @"(?<ir>(?:vou|vais|vai|vamos|vão|ir|foi|fui|ia|iam)\s+)?(?<v>pegando|pegou|pegaram|pego|pega|pegam|pegava|pegavam|pegar|pegue|peguem)\s+leve(?<c>\s+com)?" + NotWordAfter, Ci),
                m => m.Groups["ir"].Value + CaseHelper.MatchCase(m.Groups["v"].Value, m.Groups["ir"].Success && m.Groups["v"].Value.Equals("pegar", StringComparison.OrdinalIgnoreCase)
                    ? "ter"
                    : m.Groups["v"].Value.ToLowerInvariant() switch
                    {
                        "pegando" => "a ir", "pegou" => "foi", "pegaram" => "foram", "pego" => "vou", "pega" => "vai", "pegam" => "vão",
                        "pegava" => "ia", "pegavam" => "iam", "pegar" => "ir", "pegue" => "vá", _ => "vão",
                    }) + (m.Groups["ir"].Success && m.Groups["v"].Value.Equals("pegar", StringComparison.OrdinalIgnoreCase)
                        ? (m.Groups["c"].Success ? " calma com" : " calma")
                        : m.Groups["c"].Success ? " devagar com" : " com calma")),
            // Plural BR "caras" (guys) → "tipos"; vocative "Caras," → "Rapazes,".
            (new Regex(@"(?<=(?<![\p{L}\p{M}])(?:bando\s+de|grupo\s+de|vários|alguns|muitos|outros|uns|uns\s+quantos|dois|três|quatro|cinco|os|esses|estes|bons|maus|tais|enviou|mandou|chamou|trouxe|viu|vi|conheço|conheces|conhece)\s+)caras(?![\p{L}\p{M}])", Ci),
                m => CaseHelper.MatchCase(m.Value, "tipos")),
            (new Regex(@"(?<pre>^|[-–—]\s*|[.!?…]\s+)Caras(?=\s*[,!])", Cs), m => m.Groups["pre"].Value + "Rapazes"),
            // "cinco quadras da casa" → "cinco quarteirões da casa" (not sports courts).
            (new Regex(NotWordBefore + @"(?<d>uma|umas|duas|três|quatro|cinco|seis|sete|oito|nove|dez|algumas|poucas|várias|muitas)\s+(?<q>quadras?)(?!\s+de\s+(?:tênis|ténis|basquete|basquetebol|vôlei|voleibol|futebol|squash|padel|ginástica|esportes|desportos))" + NotWordAfter, Ci),
                m =>
                {
                    var plural = m.Groups["q"].Value.EndsWith('s') || m.Groups["q"].Value.EndsWith('S');
                    if (!plural && !Regex.IsMatch(m.Result("$'"), @"^\s+(?:daqui|dali|de\s+distância|abaixo|acima|adiante|da|do)" + NotWordAfter, Ci)) return m.Value;
                    return Determiners.ToGender(m.Groups["d"].Value, 'm') + " " + CaseHelper.MatchCase(m.Groups["q"].Value, plural ? "quarteirões" : "quarteirão");
                }),
            // A telephone "ligação" is a "chamada" ("Ligação anónima", "rastrear a ligação"); "ligação entre" stays a connection.
            (new Regex(NotWordBefore + @"(?<w>ligaç(?:ão|ões))(?=\s+(?:de\s+emergência|anónim\p{L}+|anônim\p{L}+|telefónic\p{L}+|telefônic\p{L}+|perdid\p{L}+|motivacion\p{L}+)" + NotWordAfter + ")", Ci),
                m => CaseHelper.MatchCase(m.Groups["w"].Value, m.Groups["w"].Value.EndsWith('s') || m.Groups["w"].Value.EndsWith('S') ? "chamadas" : "chamada")),
            (new Regex(NotWordBefore + @"(?<v>rastrear|rastreia|rastreie|rastreou|rastreiam|atender|atende|atendeu|atendi|atenderam|receber|recebeu|recebi|recebemos|devolver|devolveu|devolvi|responder|respondeu|cada|última|primeira)\s+(?:(?<d>uma|a|essa|esta|aquela|cada|nossa)\s+)?(?<w>ligaç(?:ão|ões))(?!\s+(?:entre|com|ao|à|aos|às)" + NotWordAfter + ")", Ci),
                m => m.Groups["v"].Value + " " + (m.Groups["d"].Success ? m.Groups["d"].Value + " " : string.Empty) +
                     CaseHelper.MatchCase(m.Groups["w"].Value, m.Groups["w"].Value.EndsWith('s') || m.Groups["w"].Value.EndsWith('S') ? "chamadas" : "chamada")),
            // "A vítima tem ficha?" (a record): police → "registo criminal"; with hospital words in the cue → "historial clínico".
            (new Regex(@"(?<=(?<![\p{L}\p{M}])(?:paciente|pacientes|hospital|médic\p{L}*|clínic\p{L}*|doente|doentes|enfermeir\p{L}*|internad\p{L}*|cirurgia|diagnóstico|alergia\p{L}*|exames?)(?![\p{L}\p{M}])[^\n]{0,80})" +
                NotWordBefore + @"(?<v>tem|tens|tenho|temos|têm|tinha|tinham|teve|ter|tiver|tenha|tenhas)\s+(?<d>(?:uma|alguma)\s+)?ficha(?=\s*(?:[.!?…,]|$)|\s+(?:aqui|no\s+sistema|anterior)" + NotWordAfter + ")", Ci),
                m => m.Groups["v"].Value + " " + Masculine(m.Groups["d"].Value) + "historial clínico"),
            (new Regex(NotWordBefore + @"(?<v>tem|tens|tenho|temos|têm|tinha|tinham|teve|ter|tiver|tenha|tenhas)\s+(?<d>(?:uma|alguma)\s+)?ficha(?=\s*(?:[.!?…,]|$|\s)[^\n]{0,80}(?<![\p{L}\p{M}])(?:paciente|pacientes|hospital|médic\p{L}*|clínic\p{L}*|doente|doentes|enfermeir\p{L}*|internad\p{L}*|cirurgia|diagnóstico)(?![\p{L}\p{M}]))", Ci),
                m => m.Groups["v"].Value + " " + Masculine(m.Groups["d"].Value) + "historial clínico"),
            (new Regex(NotWordBefore + @"(?<v>tem|tens|tenho|temos|têm|tinha|tinham|teve|ter|tiver|tenha|tenhas)\s+(?<d>(?:uma|alguma)\s+)?ficha(?=\s*(?:[.!?…,]|$)|\s+(?:aqui|no\s+sistema|anterior|na\s+polícia|na\s+policia)" + NotWordAfter + ")", Ci),
                m => m.Groups["v"].Value + " " + Masculine(m.Groups["d"].Value) + "registo criminal"),
            // "ficha na polícia / ficha policial / ficha suja / ficha limpa" → criminal record.
            (new Regex(NotWordBefore + @"(?<d>a|uma|sua|tua|minha|nossa)\s+ficha\s+(?:na\s+pol[ií]cia|policial)" + NotWordAfter, Ci),
                m => Masculine(m.Groups["d"].Value) + "registo criminal"),
            (new Regex(NotWordBefore + @"(?<d>a|uma|sua|tua|minha|nossa|com)\s+ficha\s+(?<w>limpa|suja)" + NotWordAfter, Ci),
                m =>
                {
                    var d = m.Groups["d"].Value;
                    var masculine = d.ToLowerInvariant() switch { "a" => "o", "uma" => "um", "sua" => "seu", "tua" => "teu", "minha" => "meu", "nossa" => "nosso", _ => d };
                    var adj = m.Groups["w"].Value.ToLowerInvariant() == "limpa" ? "limpo" : "sujo";
                    return CaseHelper.MatchCase(d, masculine) + " registo criminal " + adj;
                }),
            // "Tem uma ligação com o chefe" (you have a call) → "chamada".
            (new Regex(NotWordBefore + @"(?<v>há|havia|tem|tens|tenho|temos|têm|tinha|tinham|teve|tiver|tenha|tenhas|ter|espero|esperamos|esperava|esperávamos|esperar)\s+(?<d>uma|duas|várias|algumas)\s+(?<w>ligaç(?:ão|ões))(?=\s+(?:com|para|de|do|da|às|as|à|hoje|amanhã|urgente|importante|naquela|nessa|furios\p{L}+|importante)" + NotWordAfter + "|\\s*[.!?…,]|\\s*$)", Ci),
                m => m.Groups["v"].Value + " " + m.Groups["d"].Value + " " +
                     CaseHelper.MatchCase(m.Groups["w"].Value, m.Groups["w"].Value.EndsWith('s') || m.Groups["w"].Value.EndsWith('S') ? "chamadas" : "chamada")),
            // "comprar uma passagem para" → "comprar um bilhete para".
            (new Regex(NotWordBefore + @"(?<d>uma|a)\s+(?<w>passagem)(?=\s+(?:para|de\s+(?:ida|volta|avião|ônibus|autocarro|comboio|trem|primeira|segunda|classe))" + NotWordAfter + ")", Ci),
                m => CaseHelper.MatchCase(m.Groups["d"].Value, m.Groups["d"].Value.Equals("uma", StringComparison.OrdinalIgnoreCase) ? "um" : "o") + " " + CaseHelper.MatchCase(m.Groups["w"].Value, "bilhete")),
            // A car's "motorista" is its "condutor"; bus, taxi and lorry drivers keep the word.
            (new Regex(NotWordBefore + @"(?<d>o|do|ao|um|pelo|no|esse|este|aquele|desse|deste|daquele)\s+(?<w>motorista)(?!\s+(?:de\s+(?:autocarro|ônibus|camião|caminhão|táxi|taxi|limusine|limusina|aplicativo|app|uber|carrinha|ambulância|aluguer|aluguel)|do\s+(?:autocarro|ônibus|camião|caminhão|táxi|taxi|presidente|prefeito|presidente)|da\s+(?:limusine|limusina|ambulância)))" + NotWordAfter, Ci),
                m => m.Groups["d"].Value + " " + CaseHelper.MatchCase(m.Groups["w"].Value, "condutor")),
            // "toda hora" → "a toda a hora"; "gota d'água" → "gota de água"; "esse ano" → "este ano" (the current year, month or week).
            (new Regex(NotWordBefore + @"(?<a>a\s+)?(?<t>toda)\s+hora(?!\s+(?:é|de)" + NotWordAfter + ")" + NotWordAfter, Ci),
                m => CaseHelper.MatchCase(m.Value, "a toda a hora")),
            (new Regex(NotWordBefore + @"gota\s+d['’]água" + NotWordAfter, Ci), m => CaseHelper.MatchCase(m.Value, "gota de água")),
            (new Regex(NotWordBefore + @"(?<d>esse|essa)(?<s>\s+)(?<n>ano|mês|semana|fim\s+de\s+semana)(?=\s*[?!.,…]|\s+(?:vou|vai|vamos|vão|estou|está|estamos|estão|tenho|tem|temos|tive|foi|fui|é|já|ainda|não|só|também)" + NotWordAfter + @"|\s*$)", Ci),
                m => CaseHelper.MatchCase(m.Groups["d"].Value, m.Groups["d"].Value.Equals("esse", StringComparison.OrdinalIgnoreCase) ? "este" : "esta") + m.Groups["s"].Value + m.Groups["n"].Value),
            // "pensar de ir" → "pensar em ir": pt-PT thinks "em" + infinitive ("pensar de" is only "to have an opinion of").
            (new Regex(NotWordBefore + @"(?<!(?:que|como|quem)\s+(?:(?:você|vocês|tu|ele|ela|eles|elas|eu|nós|se)\s+)?)(?<v>pens(?:ar|ando|ado|o|ei|ou|a|as|am|amos|ava|avas|avam|ámos|aram|e|es|em|asse|ara|aria))(?<s>\s+)de(?<s2>\s+)(?<i>\p{L}+(?:ar|er|ir|ôr)|ir)" + NotWordAfter, Ci),
                m => m.Groups["i"].Value.Equals("ir", StringComparison.OrdinalIgnoreCase) || PortugueseVerbs.IsInfinitive(m.Groups["i"].Value)
                    ? m.Groups["v"].Value + m.Groups["s"].Value + "em" + m.Groups["s2"].Value + m.Groups["i"].Value
                    : m.Value),
            // "ir de topless" → "ir em topless": topless is a state ("em topless"), not an outfit ("de fato").
            (new Regex(NotWordBefore + @"(?<v>ir|vai|vais|vou|vão|vamos|foi|fui|ia|iam|andar|anda|andas|andam|andou|andava|estar|está|estás|estão|estava|estive|esteve|aparecer|apareceu|aparecia|chegar|chegou|ficar|ficou|nadar|nadou|dormir|dormiu|posar|posou|tomar\s+sol|passear|passeou|correr|correu)(?<s>\s+)de\s+(?<t>topless)" + NotWordAfter, Ci),
                m => m.Groups["v"].Value + m.Groups["s"].Value + "em " + m.Groups["t"].Value),
            // "Não leve pro pessoal" is "take it personally": pt-PT says "levar para o lado pessoal".
            (new Regex(NotWordBefore + @"(?<v>lev\p{L}*)(?<x>\s+(?:isso|isto|tudo|nada|aquilo))?\s+(?:pro|pra\s+o|para\s+o)\s+pessoal(?!\s+(?:d[aeo]s?|que|do|de)" + NotWordAfter + ")" + NotWordAfter, Ci),
                m => m.Groups["v"].Value + m.Groups["x"].Value + " para o lado pessoal"),
            // "Laboratório voltou com o resultado" → "O Laboratório devolveu o resultado"; "voltar com" is a BR calque for returning results.
            (new Regex(NotWordBefore + @"(?<v>voltou|voltaram|volta|voltam|voltava|voltavam)\s+com\s+(?<o>(?:o|os|um|uns|a|as|uma|umas)\s+(?:resultados?|relatórios?|laudos?|exames?|análises?|respostas?|ficheiros?|provas?))" + NotWordAfter, Ci),
                m => CaseHelper.MatchCase(m.Groups["v"].Value, m.Groups["v"].Value.ToLowerInvariant() switch
                {
                    "voltou" => "devolveu", "voltaram" => "devolveram", "volta" => "devolve", "voltam" => "devolvem",
                    "voltava" => "devolvia", _ => "devolviam",
                }) + " " + m.Groups["o"].Value),
            // A bare institutional subject at the start of a sentence needs the article in pt-PT.
            (new Regex(@"(?<pre>^|[.!?…]\s+|^-\s*)(?<n>Laborat[óo]rio|Departamento|Tribunal|Hospital|Banco)(?=\s+(?:voltou|devolveu|confirmou|enviou|ligou|disse|diz|encontrou|identificou|analisou|respondeu|mandou|já|ainda|acabou)" + NotWordAfter + ")", RegexOptions.CultureInvariant),
                m => m.Groups["pre"].Value + "O " + m.Groups["n"].Value),
            // "Tem a amostra mas não bate no sistema" → "A amostra existe, mas não tem correspondência no sistema."
            (new Regex(NotWordBefore + @"(?:tem|temos|tenho|há)\s+(?<o>(?:a|o|uma|um)\s+\p{L}+),?\s+mas\s+(?<n>não\s+)?(?:bate|batem|bateu|bateram)\s+(?<p>no|na|nos|nas|com\s+(?:o|a|os|as))\s+(?<d>\p{L}+(?:\s+de\s+\p{L}+)?)", Ci),
                m => ExistsButNoMatch(m)),
            // "não bate no sistema" → "não tem correspondência no sistema" ("bater" is not "to match" in pt-PT).
            (new Regex(NotWordBefore + @"(?<n>não\s+)(?<v>bate|batem|bateu|bateram|batia|batiam)\s+(?<p>no|na|nos|nas|com\s+(?:o|a|os|as))\s+(?<d>sistema|banco\s+de\s+dados|base\s+de\s+dados|registos?|arquivos?|ficheiros?|computador|banco|cadastro)" + NotWordAfter, Ci),
                m => m.Groups["n"].Value + (m.Groups["v"].Value.ToLowerInvariant() switch
                {
                    "bate" => "tem", "batem" => "têm", "bateu" => "teve", "bateram" => "tiveram", "batia" => "tinha", _ => "tinham",
                }) + " correspondência " + CorrespondencePrep(m.Groups["p"].Value) + " " + m.Groups["d"].Value),
            // "$ 200" / "$ 10 mil" / "$ 1 milhão" → "200 dólares" / "10 mil dólares" / "1 milhão de dólares".
            (new Regex(@"(?<![\p{L}\p{N}$])\$\s?(?<n>\d(?:[\d.,]*\d)?)(?:\s+(?<u>mil|milhão|milhões|bilião|biliões|bilhão|bilhões))?(?![\p{L}\p{N}])", Cs),
                m =>
                {
                    var u = m.Groups["u"].Value;
                    var big = u is "milhão" or "milhões" or "bilião" or "biliões" or "bilhão" or "bilhões";
                    var one = m.Groups["n"].Value == "1";
                    var unit = u switch
                    {
                        "bilhão" => "bilião", "bilhões" => "biliões", "" => "", _ => u,
                    };
                    return m.Groups["n"].Value + (unit.Length > 0 ? " " + unit : "") + (big ? " de" : "") + (one && unit.Length == 0 ? " dólar" : " dólares");
                }),
            // "Time 2" (a numbered team, as in a police operation) → "Equipa 2".
            (new Regex(@"(?<![\p{L}\p{M}])Time(?=\s+(?:\d+|[A-Z])(?![\p{L}\p{N}]))", Cs), m => "Equipa"),
            // "dar ré" (reverse gear) → "dar marcha-atrás".
            (new Regex(NotWordBefore + @"(?<v>dá|dás|dou|damos|dão|deu|deram|dei|dava|davam|dar|dê|dando)\s+ré" + NotWordAfter, Ci),
                m => m.Value[..^2] + "marcha-atrás"),
            (new Regex(NotWordBefore + @"(?<v>sabia|sabias|sabe|sabes|soube|soubeste|souberam|sabiam|sabiam)\s+sobre\s+(?:isso|isto)" + NotWordAfter, Ci),
                m => m.Groups["v"].Value + " disto"),
            // "Policial Kevin Olivet" / "Obrigada, policial." → "Agente …" / "…, agente."
            (new Regex(@"(?<![\p{L}\p{M}])Policial(?=\s+\p{Lu}\p{Ll})", Cs), m => "Agente"),
            (new Regex(@"(?<=(?:^|[,.!?…]\s+|[-–—]\s*))policial(?=\s*(?:[.!?…]|,\s|$))", Ci), m => CaseHelper.MatchCase(m.Value, "agente")),
            (new Regex(NotWordBefore + @"(?<neg>n[ãa]o\s+(?:\p{L}+\s+){0,2}?)policial\s+algum" + NotWordAfter, Ci),
                m => m.Groups["neg"].Value + "nenhum polícia"),
            // Brazilian "jogar" for throwing a concrete object → "atirar" ("jogou uma garrafa na rua").
            (new Regex(NotWordBefore + @"(?<v>jogo|jogas|joga|jogamos|jogam|jogou|jogaram|jogava|jogavam|joguei|jogar|jogue|joguem|jogando)(?<cl>-(?:o|a|os|as|lhe|lhes))?" +
                @"(?<o>\s+(?:o|a|os|as|um|uma|uns|umas|aquele|aquela|esta|este|essa|esse)\s+(?:garrafas?|pedras?|tijolos?|copos?|latas?|bombas?|granadas?|ovos?|sapatos?|livros?|cadeiras?|mesas?|corpos?|juiz|arma|armas|telemóvel|chaves?|bola\s+de\s+neve|cadáver|saco|sacos|fruta|tomates?|pratos?)\s+" +
                @"(?:para|sobre|em|contra|pela|pelo|na|no|nas|nos|fora|ao|à|por\s+cima))", Ci),
                m => CaseHelper.MatchCase(m.Groups["v"].Value, m.Groups["v"].Value.ToLowerInvariant() switch
                {
                    "jogo" => "atiro", "jogas" => "atiras", "joga" => "atira", "jogamos" => "atiramos", "jogam" => "atiram",
                    "jogou" => "atirou", "jogaram" => "atiraram", "jogava" => "atirava", "jogavam" => "atiravam", "joguei" => "atirei",
                    "jogar" => "atirar", "jogue" => "atire", "joguem" => "atirem", _ => "atirando",
                }) + m.Groups["cl"].Value + m.Groups["o"].Value),
            (new Regex(NotWordBefore + @"(?<v>jogar|joga|jogou|jogaram|jogam)\s+a\s+toalha" + NotWordAfter, Ci),
                m => CaseHelper.MatchCase(m.Groups["v"].Value, m.Groups["v"].Value.ToLowerInvariant() switch
                {
                    "jogar" => "atirar", "joga" => "atira", "jogou" => "atirou", "jogaram" => "atiraram", _ => "atiram",
                }) + " a toalha ao chão"),
            // Stand-alone "Cara." / "Cara," (BR "dude") → "Pá."; "Cara esperto" → "Tipo esperto".
            (new Regex(@"(?<pre>^|[-–—]\s*|[.!?…]\s+)Cara(?=\s*[.!?,…]|$)", Cs), m => m.Groups["pre"].Value + "Pá"),
            // "o cara do FedEx" already becomes "o tipo"; also "cara do FedEx" without an article.
            (new Regex(@"(?<![\p{L}\p{M}])(?<c>cara)(?=\s+d[oa]s?\s+\p{Lu})", Ci), m => CaseHelper.MatchCase(m.Groups["c"].Value, "tipo")),
            // "ele nunca furaria comigo" (let someone down) → "nunca falharia comigo"; "furar a fila" is untouched.
            (new Regex(NotWordBefore + @"fur(?<s>o|as|a|amos|am|ou|aram|ava|avam|aria|ariam|arei|ará|arão|ei|aste|e|es|em|ar|ado|ando)(?<cl>-(?:me|te|nos|se|lhe))?(?=\s+(?:comigo|contigo|connosco|convosco|com\s+(?:ele|ela|eles|elas|nós|vós|você|vocês|o\s+senhor|a\s+senhora|a\s+gente|todos|ninguém|alguém))" + NotWordAfter + ")", Ci),
                m => CaseHelper.MatchCase(m.Value, "falh" + m.Groups["s"].Value + m.Groups["cl"].Value)),
            // "isso aqui" / "esse aqui" (BR proximity) → "isto" / "este": pt-PT uses este/isto for what is near the speaker.
            (new Regex(@"(?<=^|[.!?,;:–-]\s*|\b(?:é|era|foi|são|que|de|do|da|com|sobre|por|para|em|ver|olha|olhe)\s+)(?<d>isso|esse|essa|esses|essas)\s+aqui" + NotWordAfter, Ci | RegexOptions.Multiline),
                m => CaseHelper.MatchCase(m.Groups["d"].Value, m.Groups["d"].Value.ToLowerInvariant() switch
                {
                    "isso" => "isto", "esse" => "este", "essa" => "esta", "esses" => "estes", _ => "estas",
                })),
            // "kit estupro" (calque of "rape kit") → "kit de violação"; a bare noun after "kit" needs "de" in pt-PT.
            (new Regex(NotWordBefore + @"(?<k>kits?)\s+(?<n>estupros?|violaç(?:ão|ões)|abusos?|dna|drogas?|testes?|sobrevivência|ferramentas?|maquilhagem|maquiagem|costura|limpeza|higiene|emergência|reparos?|reparação|cirurgia|tortura|assassinato|suicídio|bombas?|veneno|overdose|autópsia|identificação|evidências?|provas?|coleta|recolha|primeiros[- ]socorros|socorros)" + NotWordAfter, Ci),
                m => m.Groups["k"].Value + " de " + CaseHelper.MatchCase(m.Groups["n"].Value,
                    m.Groups["n"].Value.ToLowerInvariant() switch
                    {
                        "estupro" => "violação", "estupros" => "violações", "maquiagem" => "maquilhagem", "reparo" => "reparação",
                        "reparos" => "reparações", "coleta" => "recolha", _ => m.Groups["n"].Value.ToLowerInvariant(),
                    })),
            // "dá com os ombros" is not Portuguese idiom; pt-PT says "abana os ombros".
            (new Regex(NotWordBefore + @"(?<v>dá|deu|dava|davam|dão|deram|dei|damos|dar|dê|dem)\s+com\s+(?<o>os\s+ombros)" + NotWordAfter, Ci),
                m => CaseHelper.MatchCase(m.Groups["v"].Value, m.Groups["v"].Value.ToLowerInvariant() switch
                {
                    "dá" => "abana", "deu" => "abanou", "dava" => "abanava", "davam" => "abanavam", "dão" => "abanam", "deram" => "abanaram",
                    "dei" => "abanei", "damos" => "abanamos", "dar" => "abanar", "dê" => "abane", _ => "abanem",
                }) + " " + m.Groups["o"].Value),
            // "chamou-me de idiota" → "chamou-me idiota"
            (new Regex(NotWordBefore + @"(?<v>cham(?:ar|ava|avam|ando|aram|ara|ará|arão|aria|ou|ei|aste|amos|ámos|am|as|a|e|em)(?:-(?:me|te|o|a|nos|os|as|lhe|lhes))?|chamá-(?:lo|la|los|las))\s+de\s+(?<w>idiota|burro|burra|mentiroso|mentirosa|louco|louca|maluco|maluca|covarde|gordo|gorda|feio|feia|estúpido|estúpida|imbecil|cretino|cretina|ladrão|assassino|assassina|racista|machista|nazi|parvo|parva|bêbado|bêbada|inflexível|puta|cabrão)" + NotWordAfter, Ci),
                m => m.Groups["v"].Value + " " + m.Groups["w"].Value),
            // "Comece descobrindo" → "Comece por descobrir", "Nem fodendo" → "Nem pensar".
            (new Regex(NotWordBefore + @"(?<v>comece|começa|comecem|começar|começou|comecei|começamos|começámos|começaram|começo|começas)\s+(?!(?:quando|mundo|segundo|fundo)\b)(?<g>\p{Ll}+(?:ando|endo|indo))" + NotWordAfter, Ci),
                m => m.Groups["v"].Value + (CaseHelper.IsAllUpper(m.Value) ? " POR " : " por ") + m.Groups["g"].Value[..^3] + (m.Groups["g"].Value.EndsWith("O") ? "R" : "r")),
            (new Regex(NotWordBefore + @"nem\s+(?:fodendo|ferrando|a\s+pau|morto|morta)(?=\s*[.!…,]|\s*$)", Ci), m => CaseHelper.MatchCase(m.Value, "nem pensar")),
            (new Regex(NotWordBefore + @"(?<w>dessa|nessa)(?<sp>\s+)(?<v>vez)" + NotWordAfter, Ci),
                m => CaseHelper.MatchCase(m.Groups["w"].Value, m.Groups["w"].Value.ToLowerInvariant() == "dessa" ? "desta" : "nesta") + m.Groups["sp"].Value + m.Groups["v"].Value),
            // "Eu falei que..." → "Eu disse que...", "Eu falei!" → "Eu disse!".
            (new Regex(@"(?<!(?:se|quando|caso|enquanto|até|se\s+não)\s+)" + NotWordBefore + @"(?<v>falei|falou|falaste|falaram|falamos|falámos|falar|fala|falam)(?<cl>-(?:me|te|lhe|nos|lhes))?(?=\s+que" + NotWordAfter + ")", Ci),
                m => CaseHelper.MatchCase(m.Groups["v"].Value, m.Groups["v"].Value.ToLowerInvariant() switch
                {
                    "falei" => "disse", "falou" => "disse", "falaste" => "disseste", "falaram" => "disseram", "falar" => "dizer",
                    "fala" => "diz", "falas" => "dizes", "falam" => "dizem", _ => "dissemos",
                }) + m.Groups["cl"].Value),
            (new Regex(NotWordBefore + @"(?<p>eu|te|lhe|vos)\s+(?<v>falei)(?=\s*[.!…]|\s*$)", Ci),
                m => Regex.IsMatch(m.Result("$`"), @"\bde\s+que\s*$", Ci) ? m.Value :
                    m.Groups["p"].Value + " " + CaseHelper.MatchCase(m.Groups["v"].Value, "disse")),
            (new Regex(@"(?<pre>(?:^|[.!?…:;,]\s+|\n|(?<![\p{L}\p{M}])[-–—]\s*|[\uE000-\uF8FF]\s*)[""“«']?)(?<w>foi\s+mal)(?=\s*[.!…,]|\s*$)", Ci),
                m => m.Groups["pre"].Value + CaseHelper.MatchCase(m.Groups["w"].Value, "desculpa")),
            (new Regex(@"(?<pre>(?:^|[.!?…:;,]\s+|\n|(?<![\p{L}\p{M}])[-–—]\s*|[\uE000-\uF8FF]\s*)[""“«']?)(?<w>é\s+sério)(?=\s*[?!])", Ci),
                m => m.Groups["pre"].Value + CaseHelper.MatchCase(m.Groups["w"].Value, "a sério")),
            (new Regex(NotWordBefore + @"(?:de\s+verdade|pra\s+valer|para\s+valer)" + NotWordAfter, Ci), m => CaseHelper.MatchCase(m.Value, "a sério")),
            (new Regex(NotWordBefore + @"(?<w>o\s+que)(?<sp>\s+)(?<v>houve)" + NotWordAfter, Ci),
                m => m.Groups["w"].Value + m.Groups["sp"].Value + CaseHelper.MatchCase(m.Groups["v"].Value, "se passou")),
            (new Regex(NotWordBefore + @"(?<w>na)(?=\s+(?:frente|porta)" + NotWordAfter + ")", Ci), m => CaseHelper.MatchCase(m.Groups["w"].Value, "à")),
            (new Regex(NotWordBefore + @"(?<p>para|pra)\s+(?<w>frente)" + NotWordAfter, Ci),
                m => CaseHelper.MatchCase(m.Groups["p"].Value, "para") + (CaseHelper.IsAllUpper(m.Value) ? " A " : " a ") + m.Groups["w"].Value),
            (new Regex(NotWordBefore + @"(?<p>nós|vocês)\s+(?<n>dois|duas|três)" + NotWordAfter, Ci),
                m => m.Groups["p"].Value + " " + (m.Groups["n"].Value.ToLowerInvariant() switch
                {
                    "duas" => CaseHelper.IsAllUpper(m.Value) ? "AS" : "as",
                    _ => CaseHelper.IsAllUpper(m.Value) ? "OS" : "os",
                }) + " " + m.Groups["n"].Value),
            (new Regex(NotWordBefore + @"(?<w>cheiro)\s+de(?=\s+\p{L})", Ci), m => m.Groups["w"].Value + (CaseHelper.IsAllUpper(m.Value) ? " A" : " a")),
            (new Regex(NotWordBefore + @"ao\s+redor" + NotWordAfter, Ci), m => CaseHelper.MatchCase(m.Value, "à volta")),
            (new Regex(NotWordBefore + @"(?<w>lugar)(?=\s+(?:nenhum|algum)" + NotWordAfter + ")", Ci), m => CaseHelper.MatchCase(m.Groups["w"].Value, "lado")),
            (new Regex(NotWordBefore + @"(?<v>peço|pedir|pede|pedi|pediu|pedimos|peça|peças|pedindo|pedes|pedem|pediram)\s+(?<w>desculpas)" + NotWordAfter, Ci),
                m => m.Groups["v"].Value + " " + CaseHelper.MatchCase(m.Groups["w"].Value, "desculpa")),
            (new Regex(NotWordBefore + @"(?<a>feliz|felizes|contente|contentes)\s+em(?=\s+(?:\p{L}+(?:ar|er|ir|ôr)" + NotWordAfter + @"|\p{L}+-(?:l[oa]s?|me|te|se|nos|vos|lhes?)" + NotWordAfter + "))", Ci),
                m => m.Groups["a"].Value + (CaseHelper.IsAllUpper(m.Value) ? " POR" : " por")),
            (new Regex(NotWordBefore + @"enquanto\s+isso" + NotWordAfter, Ci), m => CaseHelper.MatchCase(m.Value, "entretanto")),
            (new Regex(NotWordBefore + @"(?<w>faz)\s+(?:muito\s+)?tempo" + NotWordAfter, Ci), m => CaseHelper.MatchCase(m.Value, "há muito tempo")),
            (new Regex(NotWordBefore + @"(?<w>faz)\s+um\s+tempo" + NotWordAfter, Ci), m => CaseHelper.MatchCase(m.Value, "há algum tempo")),
            (new Regex(NotWordBefore + @"(?<w>faz)(?=\s+(?:\d+|um|uma|dois|duas|três|quatro|cinco|seis|sete|oito|nove|dez|vinte|trinta|uns|umas|alguns|algumas|muitos|muitas|vários|várias)\s+(?:\p{L}+\s+)?(?:anos|ano|meses|mês|semanas|semana|dias|dia)(?:\s*[.,!…]|\s+que" + NotWordAfter + @"|\s*$))", Ci),
                m => CaseHelper.MatchCase(m.Groups["w"].Value, "há")),
            (new Regex(NotWordBefore + @"(?<w>faz)(?=\s+(?:anos|meses|séculos|semanas|dias)(?:\s*[.,!…]|\s+que" + NotWordAfter + @"|\s*$))", Ci),
                m => CaseHelper.MatchCase(m.Groups["w"].Value, "há")),
            (new Regex(NotWordBefore + @"(?<w>na|naquela|nessa|desta|daquela|dessa)(?<sp>\s+)(?<e>época)" + NotWordAfter, Ci),
                m => m.Groups["w"].Value + m.Groups["sp"].Value + CaseHelper.MatchCase(m.Groups["e"].Value, "altura")),
            // "a se reconciliar" → "a reconciliar-se".
            (new Regex(NotWordBefore + @"(?<a>a)\s+(?<cl>se|me|te|nos)\s+(?<inf>\p{Ll}+(?:ar|er|ir))" + NotWordAfter + "(?!-)", Ci),
                m => m.Groups["a"].Value + " " + m.Groups["inf"].Value + "-" + m.Groups["cl"].Value),
            // "Ela devia ter me deixado" → "ter-me deixado", "Ele não tinha me dito" → "não me tinha dito".
            (new Regex(NotWordBefore + @"(?<trig>(?:não|nunca|já|que|quem|também|só|ainda|jamais|se|quando|porque|onde|como)\s+)?(?<!você\s+)(?<aux>ter|tenho|tens|tem|temos|têm|tinha|tinhas|tínhamos|tinham|teria|terias|teríamos|teriam|tenha|tenhas|tenham|tivesse|tivesses|tivessem|tiver|terá|terás|terão|terei|teremos)\s+(?<cl>me|te|se|nos|lhe|lhes)\s+(?<pp>\p{Ll}+(?:ado|ido)|dito|feito|visto|posto|escrito|aberto|coberto|morto|pago|ganho|gasto|entregue|preso|solto|eleito|aceite|aceito|desfeito|refeito|prometido)" + NotWordAfter, Ci),
                m =>
                {
                    var aux = m.Groups["aux"].Value;
                    var cl = m.Groups["cl"].Value;
                    var pp = m.Groups["pp"].Value;
                    // Tu forms take the tu reflexive: "Tens se metido" → "Tens-te metido".
                    if (cl.Equals("se", StringComparison.OrdinalIgnoreCase) && Regex.IsMatch(aux, "^(?:tens|tinhas|terias|tenhas|tivesses|terás)$", RegexOptions.IgnoreCase))
                    {
                        cl = "te";
                    }

                    // The subjunctive always takes proclisis: "que tivesses me deixado" → "que me tivesses deixado".
                    var subjunctive = Regex.IsMatch(aux, "^(?:tenha|tenhas|tenham|tivesse|tivesses|tivessem|tiver)$", RegexOptions.IgnoreCase);
                    if (m.Groups["trig"].Success || subjunctive && !char.IsUpper(aux[0]))
                    {
                        return m.Groups["trig"].Value + cl + " " + aux + " " + pp;
                    }

                    // Conditional and future take mesoclisis: "teria se considerado" → "ter-se-ia considerado".
                    var future = Regex.Match(aux, "^(?<s>ter)(?<e>ia|ias|íamos|iam|á|ás|ão|ei|emos)$", RegexOptions.IgnoreCase);
                    return future.Success
                        ? future.Groups["s"].Value + "-" + cl + "-" + future.Groups["e"].Value + " " + pp
                        : aux + "-" + cl + " " + pp;
                }),
            // Interjections and slang: "Opa!" → "Epá!", "Ô, Noah!" → "Ó, Noah!", "Cansei de esperar" → "Fartei-me de esperar".
            (new Regex(SentenceStart + @"(?<w>opa)(?=\s*[!,.…]|\s*$)", Ci), m => m.Groups["pre"].Value + CaseHelper.MatchCase(m.Groups["w"].Value, "epá")),
            (new Regex(SentenceStart + @"(?<w>ô)(?=\s*[,!]|\s+(?-i:\p{Lu}))", Ci), m => m.Groups["pre"].Value + CaseHelper.MatchCase(m.Groups["w"].Value, "ó")),
            (new Regex(@"(?<!(?:me|não|nunca|já|te)\s+)" + NotWordBefore + @"(?<w>cansei)(?=\s+d(?:e|as?|os?)" + NotWordAfter + @"|\s*[.!…]|\s*$)", Ci),
                m => CaseHelper.MatchCase(m.Groups["w"].Value, "fartei-me")),
            // "Lógico, mas…" / "É cruel, lógico." → "Claro, mas…" / "É cruel, claro." ("raciocínio lógico" is kept).
            (new Regex(@"(?<pre>(?:^|[.!?…:;,]\s*|\n|(?<![\p{L}\p{M}])[-–—]\s*)[""“«']?(?:<i>)?)(?<w>lógico)(?=\s*[,.!?…]|\s*</i>|\s*$)", Ci),
                m => m.Groups["pre"].Value + CaseHelper.MatchCase(m.Groups["w"].Value, "claro")),
            // "O álibi confere." / "Nome e morada conferem com…" → "O álibi bate certo." / "…batem certo com…".
            (new Regex(@"(?<=[\p{L}\p{M}][\p{L}\p{M}]*\s+)(?<w>confere|conferem|conferia|conferiam)(?=\s*(?:[.,!?…;]|</i>|$)|\s+com" + NotWordAfter + ")", Ci),
                m => CaseHelper.MatchCase(m.Groups["w"].Value, m.Groups["w"].Value.ToLowerInvariant() switch
                {
                    "confere" => "bate certo", "conferem" => "batem certo", "conferia" => "batia certo", _ => "batiam certo",
                })),
            (new Regex(NotWordBefore + @"(?<p>é|foi|era|muito|tão|mais|super)\s+da\s+hora" + NotWordAfter, Ci), m => m.Groups["p"].Value + (CaseHelper.IsAllUpper(m.Value) ? " FIXE" : " fixe")),
            (new Regex(@"(?<pre>(?:^|[.!?…:;,]\s+|\n|(?<![\p{L}\p{M}])[-–—]\s*|[\uE000-\uF8FF]\s*)[""“«']?)(?<w>já\s+era)(?=\s*[.!…]|\s*$)", Ci),
                m => m.Groups["pre"].Value + CaseHelper.MatchCase(m.Groups["w"].Value, "acabou")),
            // "Estou de boa." → "Estou na boa.", but not "o tipo de boa vizinha".
            (new Regex(NotWordBefore + @"(?<w>de\s+boa)(?=\s*[.,!?…;:]|\s*$)", Ci), m => CaseHelper.MatchCase(m.Groups["w"].Value, "na boa")),
            // ficar/estar bravo → zangado, puto → fulo
            (new Regex(@"(?<v>(?:est|fic)\p{L}*\s+(?:muito\s+|tão\s+|mesmo\s+)?)brav(?<e>o|a|os|as)" + NotWordAfter, Ci), m => m.Groups["v"].Value + "zangad" + m.Groups["e"].Value),
            (new Regex(@"(?<v>(?:est|fic)\p{L}*\s+(?:muito\s+|tão\s+|mesmo\s+)?)put(?<e>o|a|os|as)" + NotWordAfter, Ci), m => m.Groups["v"].Value + "fulo" + (m.Groups["e"].Value.EndsWith("s") ? "s" : "")),
        };

        // "Pergunto-me por\nque ela…" → "Pergunto-me\nporque ela…": joining two words keeps the subtitle's line break.
        private static string JoinWords(Match m, string joined)
        {
            var w = m.Groups["w"].Value;
            var result = CaseHelper.MatchCase(w, joined);
            return w.Contains('\n') ? "\n" + result : m.Groups["sp"].Value + result;
        }

        public static string Apply(string s)
        {
            foreach (var (regex, evaluator) in Rules)
            {
                s = regex.Replace(s, evaluator);
            }

            return s;
        }
    }

    // ================================================================== você → tu

    internal static class YouToTuRules
    {
        private const string You = "você";
        private const string Adverbs = "não|nunca|jamais|também|já|ainda|sempre|só|realmente|simplesmente|provavelmente|certamente|nem|apenas|agora|quase";
        internal const string ProclisisTriggers = "não|nunca|jamais|também|já|ainda|sempre|só|nem|apenas|quase|talvez|que|quem|ninguém|alguém|tudo|todos";

        private static readonly Regex Contigo = new(NotWordBefore + @"com\s+você" + NotWordAfter, Ci);
        private static readonly Regex Preposition = new(NotWordBefore + @"(?<p>para|pra|de|a|por|sem|em|sobre|até|contra|perante|entre\s+mim\s+e)\s+você" + NotWordAfter, Ci);
        // "É você quem precisa" → "És tu que precisas" (the verb after "quem/que" agrees with "tu").
        private static readonly Regex ItIsYouWho = new(
            NotWordBefore + @"(?<be>é|foi|era|será|seria|fosse)\s+(?<you>você)\s+(?:quem|que)\s+(?<adv>(?:(?:não|já|nunca|também|sempre|ainda|só)\s+)*)(?:(?<cl>se|me|te|nos|o|a|os|as|lhe)\s+)?(?<v>\p{L}+)" + NotWordAfter + @"(?!-)", Ci);

        private static string ReplaceItIsYouWho(Match m)
        {
            var verb = m.Groups["v"].Value;
            if (!PortugueseVerbs.TryThirdToSecond(verb, out var v2) && !PortugueseVerbs.TryGuessThirdToSecond(verb, out v2))
            {
                return m.Value;
            }

            PortugueseVerbs.TryThirdToSecond(m.Groups["be"].Value, out var be);
            be = CaseHelper.MatchCase(m.Groups["be"].Value, be);
            var upper = CaseHelper.IsAllUpper(m.Value);
            var clitic = m.Groups["cl"].Value;
            if (clitic.Equals("se", StringComparison.OrdinalIgnoreCase) || clitic.Equals("lhe", StringComparison.OrdinalIgnoreCase))
            {
                clitic = upper ? "TE" : "te";
            }

            var result = be + " tu que " + m.Groups["adv"].Value + (clitic.Length > 0 ? clitic + " " : string.Empty) + CaseHelper.MatchCase(verb, v2);
            return upper ? result.ToUpperInvariant() : result;
        }

        // "Me ouça você!" / "Olha você!": after a command "você" is the emphatic subject → "Ouve-me tu!" / "Olha tu!", not "-te".
        private static readonly Regex EmphaticYou = new(
            @"(?<pre>(?:^|[.!?…:;]\s+|\n|(?<![\p{L}\p{M}])[-–—]\s*|[\uE000-\uF8FF]\s*)[""“«']?(?:<i>)?)(?<cl>(?:me|nos)\s+)?(?<v>\p{L}+)\s+(?<w>você)(?=\s*(?<q>\?)|\s*[!.,…]|\s*$)", Ci);

        private static readonly HashSet<string> TuCommands = new(StringComparer.OrdinalIgnoreCase)
        {
            "olha", "escuta", "ouve", "vê", "espera", "cala", "anda", "vai", "vem", "faz", "diz", "fala", "pára", "para", "toma", "ajuda", "deixa",
        };

        private static string ReplaceEmphaticYou(Match m)
        {
            var v = m.Groups["v"].Value;
            // "Me ama você?" → "Amas-me?"
            if (m.Groups["q"].Success)
            {
                return m.Groups["cl"].Success && PortugueseVerbs.TryThirdToSecond(v, out var second)
                    ? m.Groups["pre"].Value + PortugueseVerbs.AttachEnclitic(CaseHelper.MatchCase(m.Groups["cl"].Value, second), m.Groups["cl"].Value.Trim().ToLowerInvariant())
                    : m.Value;
            }

            var command = TuImperative.ToTu(v) != null || TuCommands.Contains(v) ||
                m.Groups["cl"].Success && Regex.IsMatch(v, "(?:a|e|ê)$", Ci);
            return command
                ? m.Groups["pre"].Value + m.Groups["cl"].Value + v + " " + CaseHelper.MatchCase(m.Groups["w"].Value, "tu")
                : m.Value;
        }

        private static readonly Regex BeYou = new(NotWordBefore + @"(?<v>é|foi|era|será|seria|fosse|ser)\s+você" + NotWordAfter, Ci);

        // "Então manda você." → "Então manda tu.": a clause-initial command with "você" after it is the subject, not the object.
        private static readonly Regex CommandYou = new(
            @"(?<pre>(?:[,;]\s*|" + NotWordBefore + @"(?:então|e|mas|agora|ok)\s+))(?<v>\p{L}+(?<!ia)[ae])\s+(?<you>você)(?=\s*(?:[.!,…]|$))", Ci);

        // "Liga você para ela." → "Liga-lhe." (the pronoun rules attach "lhe"), "Liga você para ela?" → "Ligas tu para ela?".
        private static readonly Regex DativeYou = new(
            NotWordBefore + @"(?<v>" + PronounRules.DativeVerbForms + @")\s+(?<you>você)\s+(?=para\s+(?:ele|ela|eles|elas|mim|nós)" + NotWordAfter + ")", Ci);

        private static string ReplaceDativeYou(Match m)
        {
            var verb = m.Groups["v"].Value;
            var end = Regex.Match(m.Result("$'"), @"[.!?…\n]");
            if (end.Success && end.Value == "?")
            {
                return PortugueseVerbs.TryThirdToSecond(verb, out var second)
                    ? CaseHelper.MatchCase(verb, second) + " " + CaseHelper.MatchCase(m.Groups["you"].Value, "tu") + " "
                    : m.Value;
            }

            return verb + " ";
        }

        // "Manda você." / "- Decide você!" → "Manda tu.": a short command at the start of a sentence, "você" is its subject.
        private static readonly Regex StartCommandYou = new(
            @"(?<pre>(?:^|[.!?…]\s+|\n|(?<![\p{L}\p{M}])[-–—]\s*))(?<v>manda|decide|escolhe|começa|faz|liga|conduz|avisa|paga|abre|entra|pergunta|explica|conta|tenta|experimenta|atende|joga|fala|diz|vai|vem|sai|fica|vê|pega|leva|escreve|toca|canta|dança|primeiro|segue|chama|responde)\s+(?<you>você)(?=\s*(?:[.!…]|$))", Ci);

        // "Olha só você fazendo uma entrada" → "Olha só para ti a fazer uma entrada", "Olha você!" → "Olha para ti!".
        private static readonly Regex LookAtYou = new(
            NotWordBefore + @"(?<v>olha|olhe|olhem)(?<adv>\s+só)?\s+(?<you>você)" +
            @"(?:\s+(?:(?<a>a)\s+(?<inf>\p{Ll}+[aeiô]r)|(?:(?<cl>me|nos)\s+)?(?<g>\p{Ll}+(?:ando|endo|indo)))" + NotWordAfter + @"|(?=\s*(?:[.!?…]|$)))", Ci);

        private static string ReplaceLookAtYou(Match m)
        {
            var upper = CaseHelper.IsAllUpper(m.Groups["you"].Value);
            var rest = m.Groups["inf"].Success ? " a " + m.Groups["inf"].Value
                : m.Groups["g"].Success ? " a " + (m.Groups["cl"].Success
                    ? PortugueseVerbs.AttachEnclitic(ProgressiveRules.ToInfinitive(m.Groups["g"].Value), m.Groups["cl"].Value.ToLowerInvariant())
                    : ProgressiveRules.ToInfinitive(m.Groups["g"].Value))
                : string.Empty;
            var result = m.Groups["v"].Value + m.Groups["adv"].Value + " " + (upper ? ("PARA TI" + rest).ToUpperInvariant() : "para ti" + rest);
            return result;
        }

        private static readonly Regex ObjectYou = new(
            @"(?<trig>" + NotWordBefore + @"(?:" + ProclisisTriggers + @")\s+)?" + NotWordBefore + @"(?<v>\p{L}+)\s+você(?!\s+mesm)" + NotWordAfter, Ci);

        private static readonly Regex SubjectYou = new(
            @"(?<sub>" + NotWordBefore + @"(?:que|se|quando|porque|onde|como|quem|enquanto|embora|caso|talvez|quanto|qual|até\s+que|antes\s+que|depois\s+que)\s+)?" +
            NotWordBefore + @"(?<you>você|tu)(?<adv>(?:\s+(?:" + Adverbs + @")" + NotWordAfter + @"){0,2})\s+(?:(?<cl>se|me|nos|o|a|os|as|lhe|te)\s+)?(?<v>\p{L}+)" + NotWordAfter + @"(?!-)", Ci);

        // "Se você pedir" / "que maravilha você dizer" → "Se pedires" / "que maravilha dizeres" (personal infinitive).
        private static readonly Regex YouInfinitive = new(
            @"(?<pre>" + NotWordBefore + @"(?:se|de|para|por|sem|até|ao|após|antes\s+de|depois\s+de|maravilha|alegria|bom|ótimo|melhor|pior|importante|preciso|tempo|hora|caso|vez)\s+)você\s+(?<inf>\p{Ll}+(?:ar|er|ir|ôr))" + NotWordAfter + @"(?!\s+se" + NotWordAfter + ")", Ci);

        // "enfraquecer você" → "enfraquecer-te".
        private static readonly Regex InfinitiveYou = new(NotWordBefore + @"(?<inf>\p{Ll}+(?:ar|er|ir))\s+você(?!\s+(?:mesm|e\s+(?:o|a|os|as|eu|ele|ela|\p{Lu}))" + NotWordAfter + ")" + NotWordAfter, Ci);

        // "Você e o Rui" / "e você também" → "Tu e o Rui" / "e tu também".
        private static readonly Regex YouAnd = new(NotWordBefore + @"(?<w>você)(?=\s+(?:e\s+(?:o|a|os|as|eu|ele|ela|\p{Lu})|também)" + NotWordAfter + ")", Ci);

        private static readonly Regex StandaloneYou = new(NotWordBefore + @"você(?=\s*(?:mesm[oa]\s*)?[.!?,…;:]|\s*$)", Ci);

        private static readonly Regex WaitingForYou = new(
            NotWordBefore + @"(?<aux>estou|estás|está|estamos|estão|estava|estavas|estávamos|estavam|estarei|estarás|estará|estaremos|estarão)(?<place>\s+(?:aqui|ali|lá))?(?<sp>\s+)esperando\s+você" + NotWordAfter, Ci);

        private static readonly HashSet<string> NotObjectVerbs = new(StringComparer.OrdinalIgnoreCase)
        {
            "que", "quê", "quem", "onde", "como", "quando", "qual", "porque", "porquê", "e", "ou", "mas", "se", "nem", "só",
        };

        /// <summary>
        /// Speaking to someone by title ("Detetive", "Senhor", "Doutora"...) is formal in Portugal: "você" then
        /// becomes the polite 3rd person (consigo, para si, "Você está bem?" → "Está bem?") instead of "tu".
        /// </summary>
        internal const string FormalTitles =
            @"senhor|senhora|senhores|senhoras|sr|sra|srta|dona|doutor|doutora|dr|dra|detetive|detective|agente|(?<!o )(?<!a )(?<!um )(?<!uma )oficial|" +
            @"inspetor|inspetora|inspector|capitão|tenente|sargento|cabo|coronel|general|comandante|major|almirante|xerife|juiz|juíza|" +
            @"meritíssimo|meritíssima|padre|reverendo|madre|majestade|alteza|excelência|presidente|ministro|ministra|embaixador|" +
            @"embaixadora|diretor|diretora|professor|professora|patrão|patroa|senador|senadora|governador|governadora|prefeito|" +
            @"prefeita|madame|mademoiselle|mister|monsieur|sir|lady|lorde|milorde|vossa|(?:vice-)?cônsul";

        private static readonly Regex FormalAddress = new(NotWordBefore + "(?:" + FormalTitles + ")" + NotWordAfter, Ci);
        private static readonly Regex CopularTitlePredicate = new(
            NotWordBefore + @"(?:é|és|são|somos|era|eras|eram|foi|foste|seria|serias)\s+" +
            @"(?:o|a|um|uma)\s+(?:" + FormalTitles + ")" + NotWordAfter, Ci);

        private static readonly Regex FormalSubjectYou = new(
            @"(?<sub>" + NotWordBefore + @"(?:que|se|quando|porque|onde|como|quem|enquanto|embora|caso|talvez|quanto|qual|até\s+que|antes\s+que|depois\s+que)\s+)?" +
            NotWordBefore + @"(?<you>você)(?<adv>(?:\s+(?:" + Adverbs + @")" + NotWordAfter + @"){0,2})\s+(?:(?<cl>se|me|nos|o|a|os|as|lhe)\s+)?(?<v>\p{L}+)" + NotWordAfter + @"(?!-)", Ci);

        public static bool IsFormal(string s) => FormalAddress.IsMatch(
            ExplicitAddressee.IsMatch(s) || Regex.IsMatch(s, NotWordBefore + @"(?:és|eras|foste|serias)" + NotWordAfter, Ci)
                ? CopularTitlePredicate.Replace(s, "") : s);

        public static string Apply(string s, string? previousText = null, ReferenceHint? reference = null,
            bool conversationalObjectConditional = false)
        {
            var explicitAddressee = ExplicitAddressee.IsMatch(s);
            // "Está bem?" next to "Is he okay?": the dropped subject is not "você".
            var droppedYou = reference is not { ExcludesDroppedYou: true };
            if (reference is not { ForceInformal: true } && (IsFormal(s) || reference is { FormalAddress: true }))
            {
                return ApplyFormal(s);
            }

            s = ConversionConstructions.InformalPredicates(s, reference);
            s = LookAtYou.Replace(s, ReplaceLookAtYou);
            s = YouInfinitive.Replace(s, m =>
            {
                var inf = m.Groups["inf"].Value;
                if (!PortugueseVerbs.IsInfinitive(inf) || PortugueseVerbs.IsFutureOrConditional(inf))
                {
                    return m.Value;
                }

                var personal = PortugueseVerbs.PersonalInfinitive(inf, "es");
                return m.Groups["pre"].Value + personal;
            });
            s = Contigo.Replace(s, m => CaseHelper.MatchCase(m.Value, "contigo"));
            s = Preposition.Replace(s, m =>
            {
                var p = m.Groups["p"].Value;
                if (p.Equals("pra", StringComparison.OrdinalIgnoreCase))
                {
                    p = CaseHelper.MatchCase(p, "para");
                }

                return p + " " + (CaseHelper.IsAllUpper(m.Value) ? "TI" : "ti");
            });
            s = ItIsYouWho.Replace(s, ReplaceItIsYouWho);
            s = BeYou.Replace(s, m =>
            {
                var v = m.Groups["v"].Value;
                if (!v.Equals("ser", StringComparison.OrdinalIgnoreCase))
                {
                    PortugueseVerbs.TryThirdToSecond(v, out v);
                }

                return v + " " + (CaseHelper.IsAllUpper(m.Value) ? "TU" : "tu");
            });
            s = InfinitiveYou.Replace(s, m =>
            {
                var inf = m.Groups["inf"].Value;
                if (!PortugueseVerbs.IsInfinitive(inf) || NotObjectVerbs.Contains(inf) ||
                    inf.ToLowerInvariant() is "ser" or "estar" or "ficar" or "parecer" or "ir" or "vir" or "acontecer" or "chegar" or "sair" or "voltar" or "falar" or "gostar" or "precisar" or "pensar" or "morar" or "viver")
                {
                    return m.Value;
                }

                return PortugueseVerbs.AttachEnclitic(inf, CaseHelper.IsAllUpper(inf) ? "TE" : "te");
            });
            s = YouAnd.Replace(s, m => CaseHelper.MatchCase(m.Groups["w"].Value, "tu"));
            s = EmphaticYou.Replace(s, ReplaceEmphaticYou);
            s = DativeYou.Replace(s, ReplaceDativeYou);
            s = CommandYou.Replace(s, m => m.Groups["pre"].Value + m.Groups["v"].Value + " " + CaseHelper.MatchCase(m.Groups["you"].Value, "tu"));
            s = StartCommandYou.Replace(s, m => m.Groups["pre"].Value + m.Groups["v"].Value + " " + CaseHelper.MatchCase(m.Groups["you"].Value, "tu"));
            s = ObjectYou.Replace(s, ReplaceObject);
            s = SubjectYou.Replace(s, m => ReplaceSubject(m, conversationalObjectConditional));
            s = WaitingForYou.Replace(s, m =>
                m.Groups["aux"].Value + m.Groups["place"].Value + m.Groups["sp"].Value +
                (CaseHelper.IsAllUpper(m.Value) ? "À TUA ESPERA" : "à tua espera"));
            s = StandaloneYou.Replace(s, m => CaseHelper.MatchCase(m.Value, "tu"));
            if (droppedYou)
            {
                s = RememberWhen.Replace(s, ReplaceRememberWhen);
                s = QuestionVerb.Replace(s, ReplaceQuestionVerb);
                s = BareKnowHow.Replace(s, ReplaceQuestionVerb);
                s = WhatQuestion.Replace(s, m => PortugueseVerbs.TryThirdToSecond(m.Groups["v"].Value, out var second)
                    ? m.Groups["pre"].Value + CaseHelper.MatchCase(m.Groups["v"].Value, second)
                    : m.Value);
                s = NegativeImperative.Replace(s, ReplaceNegativeImperative);
                s = BareNeed.Replace(s, m => m.Groups["pre"].Value + CaseHelper.MatchCase(m.Groups["v"].Value, PortugueseVerbs.ThirdToSecond[m.Groups["v"].Value.ToLowerInvariant()]));
            }
            if (reference is { SecondPerson: true })
            {
                if (reference.StartsWithSecondPersonSubject)
                {
                    s = ReferenceSubjectVerb.Replace(s, ReplaceReferenceSecondPersonVerb);
                }

                if (reference.SecondPersonSubject && !reference.ThirdPerson)
                {
                    s = ReferenceSubordinateVerb.Replace(s, m => ReplaceReferenceSubordinateVerb(m, reference));
                    s = ReferenceCausalVerb.Replace(s, ReplaceReferenceSecondPersonVerb);
                }
                if (reference.ConditionalYouSubject)
                    s = ReferenceConditionalVerb.Replace(s, ReplaceReferenceSecondPersonVerb);
                if (reference.RepeatedYouSubject)
                    s = ConversionConstructions.ReportedPastIntention(s, reference);
            }
            s = FamilyCongratulations.Replace(s, m => m.Groups["pre"].Value +
                (CaseHelper.IsAllUpper(m.Value) ? "ÀS" : "às") + m.Groups["sp"].Value +
                CaseHelper.MatchCase(m.Groups["poss"].Value, "vossas"));
            s = YourToTeu(s, previousText, explicitAddressee, reference);
            return s;
        }

        private static readonly Regex ReferenceSubjectVerb = new(
            @"(?<pre>^[\s""“«'\uE000-\uF8FF]*(?:(?:não|sim)\s*[.!]\s*)?" +
            @"(?:(?:então|bem|ora|bom|pois|mas|e|agora)[,;]?\s+)?" +
            @"(?:(?:talvez|provavelmente|certamente|possivelmente|não|nunca|já|ainda|só|apenas|também)\s+){0,3})" +
            @"(?<v>\p{L}+)" + NotWordAfter + @"(?=\s|[.!?…]|$|[\uE000-\uF8FF])(?!-)", Ci);

        private static readonly Regex ReferenceConditionalVerb = new(
            @"(?<pre>^[\s""“«'\uE000-\uF8FF]*(?:(?:e|mas|ou|então|bem)[,;]?\s+)?" +
            @"(?:se|quando|sempre\s+que|a\s+menos\s+que|assim\s+que)\s+" +
            @"(?:(?:não|nunca|já|ainda|sempre|só|alguma\s+vez|também)\s+){0,3})" +
            @"(?<v>\p{L}+)" + NotWordAfter + @"(?!-)", Ci);

        private static readonly Regex ReferenceSubordinateVerb = new(
            @"(?<pre>(?<![\p{L}\p{M}])que\s+(?:(?:não|nunca|já|ainda|sempre|também|só|apenas)\s+){0,3})" +
            @"(?<v>\p{L}+)(?![\p{L}\p{M}-])", Ci);

        private static readonly Regex ImpersonalWhatVerb = new(
            @"^(?:acontece|aconteceu|acontecia|acontecerá|aconteceria|aconteça|acontecesse)$", Ci);

        private static readonly Regex CopularWhatVerb = new(
            @"^(?:é|era|foi|será|seria|seja|fosse)$", Ci);

        private static string ReplaceReferenceSubordinateVerb(Match m, ReferenceHint reference)
        {
            var prefix = m.Result("$`");
            if (Regex.IsMatch(prefix, @"\b(?:quer|queres|queria|querias|querem|quiser)\s*$", Ci) &&
                Regex.IsMatch(reference.EnglishText ?? "", @"\bwant(?:ed|s)?\s+me\s+to\b", Ci))
                return m.Value;
            if (Regex.IsMatch(m.Groups["pre"].Value, @"\b(?:não|nunca|já|ainda|sempre|também|só|apenas)\b", Ci) &&
                !Regex.IsMatch(prefix, @"^[\s""“«'\uE000-\uF8FF-]*$|\b" +
                    @"(?:sei|sabes|sabe|sabemos|disse|dizes|diz|digo|dizer|acho|achas|acha|penso|pensas|pensa|" +
                    @"creio|acredito|certeza|claro|óbvio)\s*$", Ci))
                return m.Value;
            // In "quem achas que vê", "quem" is the embedded subject, not the addressee.
            if (ConversionConstructions.KeepsExtractedQuestionSubject(m.Result("$`") + m.Groups["pre"].Value, reference))
                return m.Value;
            if (Regex.IsMatch(m.Result("$`"), @"\bo\s+$", Ci))
            {
                var verb = m.Groups["v"].Value;
                var after = m.Result("$'");
                var impersonal = ImpersonalWhatVerb.IsMatch(verb) ||
                    verb.Equals("está", StringComparison.OrdinalIgnoreCase) &&
                    Regex.IsMatch(after, @"^\s+(?:a\s+(?:acontecer|haver)|acontecendo|havendo)\b", Ci);
                var copular = CopularWhatVerb.IsMatch(verb) &&
                    (!reference.SecondPersonWhatSubject || Regex.IsMatch(after, @"^\s+(?:isso|isto|aquilo)\b", Ci));
                if (impersonal || copular) return m.Value;
            }
            return ReplaceReferenceSecondPersonVerb(m);
        }

        private static readonly Regex ReferenceCausalVerb = new(
            @"(?<pre>^[\s""“«'\uE000-\uF8FF]*(?:(?<![\p{L}\p{M}])[-–—]\s*)?" +
            @"(?:(?:é|foi|era|será)\s+)?por\s+(?:isso|isto|aquilo)\s+(?:é\s+)?que\s+" +
            @"(?:(?:não|nunca|já|ainda|sempre|também|só|realmente|jamais|me|te|se|nos|lhe|lhes)\s+){0,3})" +
            @"(?<v>\p{L}+)" + NotWordAfter, Ci);

        private static string ReplaceReferenceSecondPersonVerb(Match m)
        {
            var verb = m.Groups["v"].Value;
            if (PortugueseVerbs.IsSecondPersonForm(verb) || Regex.IsMatch(verb, @"(?:aste|este|iste)$", Ci))
            {
                return m.Value;
            }

            if (!PortugueseVerbs.TryThirdToSecond(verb, out var second) &&
                !PortugueseVerbs.TryGuessThirdToSecond(verb, out second))
            {
                return m.Value;
            }

            return m.Groups["pre"].Value + CaseHelper.MatchCase(verb, second);
        }

        // A question that starts with a bare verb is addressed to "você": "Acha que...?" → "Achas que...?", "Lembra?" → "Lembras-te?".
        private static readonly Regex QuestionVerb = new(
            @"(?<pre>(?:^|[.!?…:;]\s+|\n|(?<![\p{L}\p{M}])[-–—]\s*|[\uE000-\uF8FF]\s*)[""“«']?(?:(?:ei|olha|então|e|mas)\s*,?\s+)?(?<voc>(?-i:\p{Lu})[\p{L}\p{M}]+,\s+)?(?:(?:o\s+quê|o\s+que),\s*|(?:por\s+que|porque|como)\s+)?)(?<v>acha|quer|sabe|precisa|gosta|conhece|lembra|entende|percebe|viu|ouviu|pensa|prefere|consegue|curte|topa|pode|está|lembrou|esqueceu|entendeu|percebeu|ouviu|reparou|gostou|quis|imagina)" +
            NotWordAfter + @"(?!-|\s+(?:ele|ela|eles|elas|o\s+senhor|a\s+senhora)" + NotWordAfter + @")" +
            // Impersonal "Pode ser essa?" (can it be this one?) has no "você".
            @"(?<!pode(?=\s+(?:ser|haver|acontecer)" + NotWordAfter + @"))" +
            // "Está tudo bem?" / "Está frio?" are impersonal.
            @"(?<!está(?=\s+(?:tudo|certo|combinado|feito|resolvido|decidido|frio|calor|escuro|a\s+chover|a\s+nevar|na\s+hora|marcado|aberto|fechado)" + NotWordAfter + @"))(?=[^.!?\n]*\?)", Cs | RegexOptions.IgnoreCase);

        // "Noah, o que faz aqui?" → "o que fazes aqui?" ("O que quer dizer isso?" and "O que fez a polícia?" keep the 3rd person).
        private static readonly Regex WhatQuestion = new(
            @"(?<pre>(?:^|[.!?…:;]\s+|\n|(?<![\p{L}\p{M}])[-–—]\s*|[\uE000-\uF8FF]\s*)[""“«']?(?:(?:e|mas|então)\s*,?\s+)?(?:(?-i:\p{Lu})[\p{L}\p{M}]+,\s+)?o\s+que\s+)" +
            @"(?<v>faz|fez|acha|achou|quer|quis|queria|sabe|pensa|precisa|viu|ouviu|disse|sente|prefere|fazia|vai(?=\s+fazer" + NotWordAfter + @"))" + NotWordAfter +
            @"(?!-|\s+(?:dizer|ele|ela|eles|elas|o|a|os|as|isso\s+de)" + NotWordAfter + @")(?=[^.!?\n]*\?)", Ci);

        private static readonly Regex BareKnowHow = new(
            @"(?<pre>(?:^|[.!?…:;]\s+|\n|(?<![\p{L}\p{M}])[-–—]\s*|[\uE000-\uF8FF]\s*)[""“«']?)(?<v>sabe)(?=\s+como" + NotWordAfter + ")", Ci);

        private static readonly Regex RememberWhen = new(
            @"(?<pre>(?:^|[.!?…:;]\s+|\n|(?<![\p{L}\p{M}])[-–—]\s*|[\uE000-\uF8FF]\s*)[""“«']?)(?<v>lembra|lembras-te)(?<when>\s+quando\s+)(?<cl>me|nos)(?<sp>\s+)(?<past>\p{L}+)" + NotWordAfter +
            @"(?!-|\s+(?:ele|ela|eles|elas|o\s+senhor|a\s+senhora)" + NotWordAfter + ")", Ci);

        private static string ReplaceRememberWhen(Match m)
        {
            var verb = m.Groups["past"].Value;
            if (PortugueseVerbs.IsSecondPersonForm(verb) || Regex.IsMatch(verb, @"(?:aste|este|iste)$", Ci))
            {
                return m.Groups["pre"].Value + CaseHelper.MatchCase(m.Groups["v"].Value, "lembras-te") +
                    m.Groups["when"].Value + m.Groups["cl"].Value + m.Groups["sp"].Value + verb;
            }
            if (!PortugueseVerbs.TryThirdToSecond(verb, out var second) &&
                !PortugueseVerbs.TryGuessThirdToSecond(verb, out second))
            {
                return m.Value;
            }

            return m.Groups["pre"].Value + CaseHelper.MatchCase(m.Groups["v"].Value, "lembras-te") +
                m.Groups["when"].Value + m.Groups["cl"].Value + m.Groups["sp"].Value + second;
        }

        private static readonly Regex FamilyCongratulations = new(
            @"(?<pre>" + NotWordBefore + @"parabéns\s+)(?:a|as|à|às)(?<sp>\s+)(?<poss>suas)(?=\s+famílias" + NotWordAfter + ")", Ci);

        // "Não fala." / "Não leva a sério" → "Não fales." / "Não leves a sério" (negative commands use the subjunctive in Portugal).
        private static readonly Dictionary<string, string> NegativeCommand = new(StringComparer.OrdinalIgnoreCase)
        {
            ["fala"] = "fales", ["faz"] = "faças", ["diz"] = "digas", ["fica"] = "fiques", ["olha"] = "olhes", ["liga"] = "ligues",
            ["mexe"] = "mexas", ["toca"] = "toques", ["chora"] = "chores", ["esquece"] = "esqueças", ["encosta"] = "encostes", ["leva"] = "leves",
            ["ignora"] = "ignores", ["sai"] = "saias", ["conta"] = "contes", ["desiste"] = "desistas", ["grita"] = "grites", ["mente"] = "mintas",
            ["começa"] = "comeces", ["volta"] = "voltes", ["pensa"] = "penses", ["julga"] = "julgues", ["deixa"] = "deixes", ["perde"] = "percas",
            ["abre"] = "abras", ["fecha"] = "feches", ["corre"] = "corras", ["come"] = "comas", ["bebe"] = "bebas", ["compra"] = "compres",
            ["chama"] = "chames", ["espera"] = "esperes", ["preocupa"] = "preocupes", ["estraga"] = "estragues", ["exagera"] = "exageres",
            ["brinca"] = "brinques", ["inventa"] = "inventes", ["surta"] = "surtes", ["vem"] = "venhas", ["põe"] = "ponhas", ["traz"] = "tragas",
            ["pede"] = "peças", ["ouve"] = "ouças", ["dorme"] = "durmas", ["segue"] = "sigas", ["foge"] = "fujas", ["responde"] = "respondas",
            ["mata"] = "mates", ["machuca"] = "magoes", ["magoa"] = "magoes", ["complica"] = "compliques", ["atrapalha"] = "atrapalhes",
            ["interrompe"] = "interrompas", ["mexe"] = "mexas", ["fuma"] = "fumes", ["usa"] = "uses", ["seja"] = "sejas",
            ["olhe"] = "olhes", ["finge"] = "finjas", ["acredita"] = "acredites", ["tenta"] = "tentes", ["toma"] = "tomes", ["xinga"] = "insultes",
        };

        private static readonly Regex NegativeImperative = new(
            @"(?<pre>(?:^|[.!?…:;]\s+|\n|(?<![\p{L}\p{M}])[-–—]\s*|[\uE000-\uF8FF]\s*)[""“«']?(?:(?:mas|e|então|por\s+favor|agora|ei|olha)\s*,?\s+)?(?:\p{Lu}\p{Ll}+,\s+)?)" +
            @"(?<no>não)\s+(?:(?<cl>me|se|te|nos|o|a|os|as|lhe)\s+)?(?<v>\p{L}+)" + NotWordAfter + @"(?!-)(?<rest>[^.!?\n]*)(?<end>[.!…]|$)", Ci);

        private static string ReplaceNegativeImperative(Match m)
        {
            var verb = m.Groups["v"].Value;
            if (!NegativeCommand.TryGetValue(verb, out var subjunctive))
            {
                return m.Value;
            }

            var rest = m.Groups["rest"].Value;
            // A subject or "que" after the verb means it is a statement ("Não fala comigo há anos" is still ambiguous, so require
            // a short command: up to 5 words).
            if (Regex.IsMatch(rest, @"^\s*(?:que|ele|ela|eles|elas|nada\s+de|bem|mal|sentido|diferença|ideia|mais\s+sentido|muito|português|inglês|francês|espanhol|alemão)" + NotWordAfter, RegexOptions.IgnoreCase) ||
                Regex.Matches(rest, @"\p{L}+").Count > 6)
            {
                return m.Value;
            }

            var clitic = m.Groups["cl"].Value;
            if (clitic.Equals("se", StringComparison.OrdinalIgnoreCase) || clitic.Equals("lhe", StringComparison.OrdinalIgnoreCase))
            {
                clitic = "te";
            }

            return m.Groups["pre"].Value + m.Groups["no"].Value + " " + (clitic.Length > 0 ? clitic + " " : string.Empty) + CaseHelper.MatchCase(verb, subjunctive) + rest + m.Groups["end"].Value;
        }

        // "Precisa de foco." / "Só precisa de saber" (subject "você" dropped) → "Precisas de...".
        private static readonly Regex BareNeed = new(
            @"(?<pre>(?:^|[.!?…:;]\s+|\n|(?<![\p{L}\p{M}])[-–—]\s*|[\uE000-\uF8FF]\s*)[""“«']?(?:(?:ei|olha|então|mas|e)\s*,?\s+)?(?:(?:só|não|também|ainda|já|nem|realmente|mesmo)\s+)?)" +
            @"(?<v>precisa|consegue|merece)(?=\s+(?:de|que|ser|ir|ficar|ter|fazer|saber|parar|ouvir|ver|vir)" + NotWordAfter + @"|\s*[.!]|\s*$)", Ci);

        private static string ReplaceQuestionVerb(Match m)
        {
            // "King, acha que sim?" speaks to a friend; "Comissário, acha...?" stays formal.
            if (m.Groups["voc"].Success && PronominalVerbs.Titles.Contains(m.Groups["voc"].Value.TrimEnd(',', ' ', '\t')))
            {
                return m.Value;
            }

            var verb = m.Groups["v"].Value;
            string second;
            var lower = verb.ToLowerInvariant();
            if (lower is "lembra" or "lembrou" or "esqueceu")
            {
                second = (lower switch { "lembra" => "lembras", "lembrou" => "lembraste", _ => "esqueceste" }) + "-te";
            }
            else if (!PortugueseVerbs.TryThirdToSecond(lower, out second) && !PortugueseVerbs.TryGuessThirdToSecond(lower, out second))
            {
                return m.Value;
            }

            return m.Groups["pre"].Value + CaseHelper.MatchCase(verb, second);
        }

        private static readonly Regex Sentence = new(@"[^.!?…\n]+[.!?…]*", Cs);

        private static readonly Regex ThirdPersonContext = new(
            NotWordBefore + @"(?:ele|ela|eles|elas|dele|dela|deles|delas|nele|nela|vocês|o\s+senhor|a\s+senhora|cada|alguém|ninguém|todos|todas|toda\s+a\s+gente|quem|pessoa|pessoas)" + NotWordAfter, Ci);

        private static readonly Regex Your = new(
            NotWordBefore + @"(?<p>seu|sua|seus|suas)(?=\s+(?<n>\p{L}+)|\s*[.!?,…]|\s*$)" + NotWordAfter, Ci);

        internal static readonly HashSet<string> Insults = new(StringComparer.OrdinalIgnoreCase)
        {
            "idiota", "idiotas", "imbecil", "burro", "burra", "estúpido", "estúpida", "babaca", "cretino", "cretina", "otário", "otária",
            "palhaço", "nojento", "nojenta", "vaca", "cabra", "safado", "safada", "desgraçado", "desgraçada", "maluco", "maluca", "louco",
            "louca", "mentiroso", "mentirosa", "covarde", "cobarde", "traidor", "traidora", "ingrato", "ingrata", "panaca", "parvo", "parva",
            "merda", "porco", "porca", "monstro", "bastardo", "filho", "filha", "doido", "doida", "puto", "puta", "canalha", "tonto", "tonta",
            "anormal", "chato", "chata", "lixo", "verme", "animal", "pervertido", "pervertida", "otários", "imbecis", "malandro", "sacana",
            "bosta", "tarado", "tarada", "escroto", "escrota", "inútil", "inúteis", "fracassado", "fracassada", "perdedor", "pateta",
            "corno", "vagabundo", "vagabunda", "egoísta", "hipócrita", "sortudo", "sortuda", "pirralho", "pirralha", "cuzão", "babacas",
            "arrombado", "viciado", "viciada", "drogado", "drogada", "bêbado", "bêbada", "bêbedo", "bêbeda", "falhado", "falhada", "lerdo", "lerda", "cabrão", "cabrões", "nerd", "metido", "metida", "convencido", "convencida", "doente",
        };

        /// <summary>
        /// "seu/sua" is almost always "your" in Brazilian subtitles; with "você" turned into "tu" it becomes "teu/tua".
        /// Sentences that also talk about a third person (ele, ela, dele...) are left alone.
        /// </summary>
        private static readonly Regex ExplicitAddressee = new(NotWordBefore + @"(?:você|tu|teu|tua|teus|tuas)" + NotWordAfter, Ci);
        // "Disse-lhe para trazer o seu carro" - an enclitic "-lhe" also names the third person.
        private static readonly Regex ThirdPersonRecipient = new(@"(?<![\p{L}\p{M}\d_])(?:lhe|lhes)" + NotWordAfter, Ci);
        private static readonly Regex SendingPossessions = new(
            NotWordBefore + @"(?:mandei|enviei|entreguei|devolvi|remeti|mandou|enviou|entregou|devolveu)\s+(?:as?\s+|os?\s+)?(?:seu|sua|seus|suas)" + NotWordAfter, Ci);
        private static readonly Regex SeparationAntecedent = new(
            NotWordBefore + @"(?:divorciando|divorciar(?:-me|-te|-se)?|separando|separar(?:-me|-te|-se)?)\s+(?:do|da|dele|dela)" + NotWordAfter, Ci);
        private static readonly Regex AlreadySentBelongings = new(
            NotWordBefore + @"(?<already>já)(?<sp>\s+)até\s+(?<v>mandei|enviei|entreguei|devolvi)(?<space>\s+)(?<poss>suas)\s+coisas" + NotWordAfter, Ci);

        private static string YourToTeu(string s, string? previousText, bool explicitAddressee, ReferenceHint? reference = null)
        {
            if (!Your.IsMatch(s))
            {
                return s;
            }

            var referenceThird = reference is { KeepsThirdPersonPossessive: true };
            var previousAntecedent = (referenceThird || previousText != null && SeparationAntecedent.IsMatch(previousText)) &&
                SendingPossessions.IsMatch(s) && !explicitAddressee;
            if (referenceThird && !previousAntecedent) return s;
            // English "your" cannot tell "tu" from formal "o seu marido", so the reference only ever prevents "teu".
            var cueThirdPerson = !explicitAddressee &&
                (ThirdPersonContext.IsMatch(s) || ThirdPersonRecipient.IsMatch(s));
            if (previousAntecedent)
            {
                return AlreadySentBelongings.Replace(s, m =>
                {
                    var result = "até," + m.Groups["sp"].Value + "já lhe " + m.Groups["v"].Value +
                        m.Groups["space"].Value + "as suas coisas";
                    return CaseHelper.MatchCase(m.Groups["already"].Value, result);
                });
            }
            if (cueThirdPerson) return s;

            return Sentence.Replace(s, sm => ThirdPersonContext.IsMatch(sm.Value) || ThirdPersonRecipient.IsMatch(sm.Value) ? sm.Value : Your.Replace(sm.Value, m =>
            {
                if (Insults.Contains(m.Groups["n"].Value) || IsVocativeInsult(sm.Value, m))
                {
                    return m.Value;
                }

                var p = m.Groups["p"].Value;
                var to = p.ToLowerInvariant() switch { "seu" => "teu", "sua" => "tua", "seus" => "teus", _ => "tuas" };
                return CaseHelper.MatchCase(p, to);
            }));
        }

        // ", seu tarado." / "- Sua ingrata!" - a "seu/sua" + one word closing the sentence after a comma is name-calling.
        private static bool IsVocativeInsult(string sentence, Match m)
        {
            var before = sentence[..m.Index].TrimEnd();
            var after = sentence[(m.Index + m.Length)..];
            return before.EndsWith(',') && Regex.IsMatch(after, @"^\s+\p{Ll}+\s*[.!?…:;]*\s*$");
        }

        private static string ApplyFormal(string s)
        {
            s = Contigo.Replace(s, m => CaseHelper.MatchCase(m.Value, "consigo"));
            s = Preposition.Replace(s, m =>
            {
                var p = m.Groups["p"].Value;
                if (p.Equals("pra", StringComparison.OrdinalIgnoreCase))
                {
                    p = CaseHelper.MatchCase(p, "para");
                }

                return p + " " + (CaseHelper.IsAllUpper(m.Value) ? "SI" : "si");
            });
            return FormalSubjectYou.Replace(s, ReplaceFormalSubject);
        }

        /// <summary>"Você está bem?" → "Está bem?", "Você se lembra?" → "Lembra-se?" - the verb stays in the 3rd person.</summary>
        private static string ReplaceFormalSubject(Match m)
        {
            var verb = m.Groups["v"].Value;
            if (!PortugueseVerbs.TryThirdToSecond(verb, out _) &&
                !(char.IsLower(verb[0]) && PortugueseVerbs.TryGuessThirdToSecond(verb, out _)))
            {
                return m.Value;
            }

            var you = m.Groups["you"].Value;
            var adv = m.Groups["adv"].Value.TrimStart();
            var clitic = m.Groups["cl"].Value;
            var advPart = adv.Length > 0 ? adv + " " : string.Empty;
            string result;
            if (clitic.Length == 0)
            {
                result = advPart + verb;
            }
            else
            {
                var proclisis = m.Groups["sub"].Success || Regex.IsMatch(adv, @"(?:^|\s)(?:" + ProclisisTriggers + @")(?:$|\s)", Ci) || PortugueseVerbs.IsFutureOrConditional(verb);
                result = proclisis ? advPart + clitic + " " + verb : advPart + PortugueseVerbs.AttachEnclitic(verb, clitic);
            }

            if (CaseHelper.IsAllUpper(you))
            {
                result = result.ToUpperInvariant();
            }
            else if (char.IsUpper(you[0]))
            {
                result = CaseHelper.Capitalize(result);
            }

            return m.Groups["sub"].Value + result;
        }

        private static string ReplaceObject(Match m)
        {
            var verb = m.Groups["v"].Value;
            var trig = m.Groups["trig"].Value;
            var te = CaseHelper.IsAllUpper(verb) ? "TE" : "te";

            // Future/conditional: "Manterei você informado" → "Manter-te-ei informado" (mesoclisis), "Não manterei você" → "Não te manterei".
            if (!NotObjectVerbs.Contains(verb) && PortugueseVerbs.TrySplitFuture(verb, out _, out _, requireTransitive: true))
            {
                return trig.Length > 0 ? trig + te + " " + verb : PortugueseVerbs.AttachMesoclitic(verb, te)!;
            }

            if (NotObjectVerbs.Contains(verb) || !PortugueseVerbs.IsTransitiveForm(verb) || PortugueseVerbs.IsFutureOrConditional(verb))
            {
                return m.Value;
            }

            return trig.Length > 0 || PronounRules.AfterSubordinateSubject(m.Result("$`"))
                ? trig + te + " " + verb
                : PortugueseVerbs.AttachEnclitic(verb, te);
        }

        /// <summary>
        /// "você tem" → "tens": European Portuguese normally drops the subject pronoun, the verb ending
        /// already says who it is ("Você não se lembra?" → "Não te lembras?").
        /// </summary>
        private static string ReplaceSubject(Match m, bool conversationalObjectConditional)
        {
            var verb = m.Groups["v"].Value;
            var explicitTu = m.Groups["you"].Value.Equals("tu", StringComparison.OrdinalIgnoreCase);
            var wordBefore = Regex.Match(m.Result("$`"), @"(?<w>\p{L}+)\s+$").Groups["w"].Value;
            if (explicitTu && (PortugueseVerbs.IsSecondPersonForm(verb) ||
                Regex.IsMatch(verb, @"(?:aste|este|iste|ando|endo|indo|ondo)$", Ci) ||
                // "Ligas tu para ela?" - "para" here is the preposition, not the verb parar.
                (!m.Groups["sub"].Success && verb.Equals("para", StringComparison.OrdinalIgnoreCase) && wordBefore.Length > 0 && PortugueseVerbs.IsSecondPersonForm(wordBefore))))
            {
                return m.Value;
            }

            // "Olha só você a fazer isso" - here "a" is the preposition before an infinitive, not "ela".
            if (!m.Groups["sub"].Success && m.Groups["cl"].Value == "a" && m.Groups["adv"].Value.Trim().Length == 0 &&
                verb.EndsWith('r') && !Regex.IsMatch(verb, @"^(?:re)?quer$", Ci))
            {
                return m.Value;
            }

            if (!PortugueseVerbs.TryThirdToSecond(verb, out var second) &&
                !(char.IsLower(verb[0]) && PortugueseVerbs.TryGuessThirdToSecond(verb, out second)))
            {
                return m.Value;
            }

            var sub = m.Groups["sub"].Value;
            var you = m.Groups["you"].Value;
            var adv = m.Groups["adv"].Value.TrimStart();
            var clitic = m.Groups["cl"].Value;
            string result;
            if (clitic.Length == 0)
            {
                result = (adv.Length > 0 ? adv + " " : string.Empty) + second;
            }
            else
            {
                var cl = clitic.Equals("se", StringComparison.OrdinalIgnoreCase) || clitic.Equals("lhe", StringComparison.OrdinalIgnoreCase)
                    ? CaseHelper.MatchCase(clitic, "te")
                    : clitic;
                var proclisis = m.Groups["sub"].Success || Regex.IsMatch(adv, @"(?:^|\s)(?:" + ProclisisTriggers + @")(?:$|\s)", Ci) || PortugueseVerbs.IsFutureOrConditional(verb);
                var conditionalObject = conversationalObjectConditional && !m.Groups["sub"].Success &&
                    adv.Length == 0 && cl.ToLowerInvariant() is "o" or "a" or "os" or "as" &&
                    PortugueseVerbs.TrySplitFuture(second, out _, out var ending, true) &&
                    ending.Equals("ias", StringComparison.OrdinalIgnoreCase);
                result = conditionalObject
                    ? "irias " + PortugueseVerbs.AttachEnclitic(
                        second[..^3].ToLowerInvariant() switch
                        {
                            "dir" => "dizer", "far" => "fazer", "trar" => "trazer",
                            var infinitive => infinitive,
                        }, cl)
                    : proclisis
                    ? (adv.Length > 0 ? adv + " " : string.Empty) + cl + " " + second
                    : (adv.Length > 0 ? adv + " " : string.Empty) + PortugueseVerbs.AttachEnclitic(second, cl);
            }

            if (CaseHelper.IsAllUpper(you))
            {
                result = result.ToUpperInvariant();
            }
            else if (char.IsUpper(you[0]) && !explicitTu)
            {
                result = CaseHelper.Capitalize(result);
            }

            return sub + (explicitTu ? m.Groups["you"].Value + " " : string.Empty) + result;
        }
    }

    // ================================================================== estar + gerúndio → estar a + infinitivo

    internal static class ProgressiveRules
    {
        private const string Auxiliaries =
            "estou|estás|está|estamos|estão|estava|estavas|estávamos|estavam|estive|estiveste|esteve|estivemos|estiveram|" +
            "estarei|estarás|estará|estaremos|estarão|estaria|estarias|estaríamos|estariam|esteja|estejas|estejamos|estejam|" +
            "estivesse|estivesses|estivéssemos|estivessem|estiver|estiveres|estivermos|estiverem|estar|estares|estarmos|estarem|estado|estando|" +
            "fico|ficas|fica|ficamos|ficam|ficava|ficavas|ficávamos|ficavam|fiquei|ficaste|ficou|ficámos|ficaram|ficar|fique|fiques|fiquem|ficarei|ficará|ficaria|" +
            "continuo|continuas|continua|continuamos|continuam|continuava|continuavam|continuei|continuou|continuaram|continuar|continue|continuem|" +
            "continuarei|continuarás|continuará|continuaremos|continuarão|continuaria|continuarias|continuaríamos|continuariam|" +
            "ando|andas|anda|andamos|andam|andava|andavas|andavam|andei|andou|andaram|" +
            "vivo|vives|vive|vivemos|vivem|vivia|viviam";

        private const string Adverbs = "só|apenas|ainda|sempre|já|quase|mesmo|realmente|simplesmente|bem|mal|lá|aqui|aí|ali|agora|não|nunca|também|todo\\s+o\\s+tempo|literalmente|seriamente|meio|tipo|tudo|mais|meio\\s+que";

        private static readonly Regex Progressive = new(
            NotWordBefore + @"(?<aux>" + Auxiliaries + @")(?<adv>(?:\s+(?:" + Adverbs + ")" + NotWordAfter + @"){0,2})\s+" +
            @"(?:(?<cl>me|te|se|nos|vos|lhe|lhes)\s+)?" +
            @"(?<g>\p{L}*(?:a|e|i|o)ndo)(?:-(?<enc>me|te|se|nos|vos|lhe|lhes|o|a|os|as|lo|la|los|las))?" + NotWordAfter, Ci);

        private static readonly HashSet<string> NotGerunds = new(StringComparer.OrdinalIgnoreCase)
        {
            "lindo", "quando", "comando", "bando", "brando", "contrabando", "nefando", "adendo", "dividendo", "reverendo",
            "horrendo", "estupendo", "remendo", "redondo", "hediondo", "segundo", "fundo", "mundo", "profundo", "rotundo",
            "oriundo", "fecundo", "imundo", "vagabundo", "moribundo", "orlando", "fernando", "rolando", "orlando",
            "rondo", "condo", "vendo", "arrependo", "suspendo", "desprendo", "empreendo", "repreendo", "despendo", "entendo", "aprendo", "compreendo", "ofendo", "defendo", "dependo", "pretendo", "estendo",
            "acendo", "prendo", "rendo", "surpreendo", "recomendo", "mando", "ando", "desando", "demando", "emendo", "comprendo",
            "atendo", "desprendo", "suspendo", "sondo",
        };

        // "Vi ele correndo" / "viu uns miúdos pegarem" → "vi-o a correr" / "viu uns miúdos a pegar".
        private static readonly Regex Perception = new(
            NotWordBefore + @"(?<v>(?:vi|viu|viste|vimos|viram|vejo|vês|vê|vemos|veem|via|vias|víamos|viam|ver|vendo|ouvi|ouviu|ouviste|ouvimos|ouviram|ouço|ouves|ouve|ouvem|ouvia|ouviam|ouvir|senti|sentiu|sentimos|sentiram|sinto|sente|sentir|notei|notou|notaram|reparei|reparou|encontrei|encontrou|encontraram|encontrámos|encontramos|apanhei|apanhou|apanharam|peguei|pegou|pegaram|imagino|imagina|imaginei|imaginou|imaginaste|imaginar|imaginava|deixei|deixou|deixa|deixe)(?:-(?:o|a|os|as|me|te|nos|lo|la|los|las))?)" +
            @"(?<obj>(?:\s+(?!(?:que|se|quando|como|onde|porque|e|mas|ou|não|já)" + NotWordAfter + @")[\p{L}-]+){0,3}?)\s+(?<g>\p{Ll}+(?:ando|endo|indo|ondo|arem|erem|irem|írem|orem))" + NotWordAfter, Ci);

        private static readonly HashSet<string> NotPersonalInfinitives = new(StringComparer.OrdinalIgnoreCase)
        {
            "querem", "preferem", "ferem", "sugerem", "requerem", "interferem", "aderem", "transferem", "conferem", "inferem",
            "digerem", "gerem", "tirem", "virem", "partirem", "derem", "puderem", "quiserem", "souberem", "tiverem", "estiverem",
            "fizerem", "disserem", "trouxerem", "vierem", "forem", "puserem", "houverem", "couberem", "morem", "decorem", "piorem",
            "melhorem", "ignorem", "adorem", "chorem", "demorem", "devorem",
        };

        // "Saiu correndo" → "Saiu a correr", "passou o dia trabalhando" → "passou o dia a trabalhar".
        private static readonly Regex Manner = new(
            NotWordBefore + @"(?<v>(?:saiu|saí|saíste|saímos|saíram|sai|sais|saem|saía|saíam|sair|entrou|entrei|entraram|entra|entram|entrar|veio|vim|vieste|viemos|vieram|vem|vêm|vinha|vinham|vir|desceu|desci|desceram|subiu|subi|subiram|fugiu|fugi|fugiram|" +
            @"passou|passei|passaste|passámos|passamos|passaram|passa|passo|passam|passava|passavam|passar|passaria|passarás|passará|levou|levei|levaram|leva|levo|levam|levar|gastei|gastou|gastaram|" +
            @"morrer|morreu|morri|morro|morre|morreram|morrerei|morrerá|morreria)" +
            @"(?:\s+(?:o\s+dia|a\s+noite|a\s+tarde|a\s+manhã|a\s+vida|o\s+tempo|horas|anos|meses|dias|semanas|minutos|a\s+semana|o\s+fim\s+de\s+semana|o\s+ano|o\s+mês|tanto\s+tempo|muito\s+tempo|todo\s+o\s+dia|a\s+noite\s+toda|o\s+dia\s+todo|a\s+manhã\s+toda|a\s+tarde\s+toda|a\s+vida\s+toda|\p{N}+\s+\p{L}+|uma\s+hora|duas\s+horas|uma\s+semana|um\s+ano|um\s+mês|um\s+tempo))?)" +
            @"\s+(?:(?<cl>me|te|se|nos|lhe|lhes)\s+)?(?<g>\p{Ll}+(?:ando|endo|indo|ondo))" + NotWordAfter, Ci);

        private static readonly HashSet<string> TimeVerbs = new(StringComparer.OrdinalIgnoreCase)
        {
            "passou", "passei", "passaste", "passámos", "passamos", "passaram", "passa", "passo", "passam", "passava", "passavam", "passar",
            "passaria", "passarás", "passará", "levou", "levei", "levaram", "leva", "levo", "levam", "levar", "gastei", "gastou", "gastaram",
        };

        // "Acabou ficando" → "Acabou por ficar".
        private static readonly Regex EndUp = new(
            NotWordBefore + @"(?<v>acabo|acabas|acaba|acabamos|acabam|acabei|acabaste|acabou|acabámos|acabaram|acabava|acabavam|acabar|acabará|acabaria|acabe|acabem)\s+(?:(?<cl>me|te|se|nos|lhe|lhes)\s+)?(?<g>\p{Ll}+(?:ando|endo|indo|ondo))" + NotWordAfter, Ci);

        // "Está procurando novidades" → "Está à procura de novidades", "estou procurando o livro" → "estou à procura do livro".
        private static readonly Regex LookingFor = new(
            NotWordBefore + @"(?<aux>" + Auxiliaries + @")(?<adv>(?:\s+(?:" + Adverbs + ")" + NotWordAfter + @"){0,2})\s+" +
            @"(?<g>procurando)(?:\s+(?<por>por))?\s+(?<next>[\p{L}\p{N}]+)" + NotWordAfter, Ci);

        private static readonly Dictionary<string, string> OfContractions = new(StringComparer.OrdinalIgnoreCase)
        {
            ["o"] = "do", ["a"] = "da", ["os"] = "dos", ["as"] = "das", ["este"] = "deste", ["esta"] = "desta", ["estes"] = "destes",
            ["estas"] = "destas", ["esse"] = "desse", ["essa"] = "dessa", ["esses"] = "desses", ["essas"] = "dessas",
            ["aquele"] = "daquele", ["aquela"] = "daquela", ["aqueles"] = "daqueles", ["aquelas"] = "daquelas", ["isto"] = "disto",
            ["isso"] = "disso", ["aquilo"] = "daquilo", ["ele"] = "dele", ["ela"] = "dela", ["eles"] = "deles", ["elas"] = "delas",
            ["meu"] = "do meu", ["minha"] = "da minha", ["meus"] = "dos meus", ["minhas"] = "das minhas", ["teu"] = "do teu",
            ["tua"] = "da tua", ["teus"] = "dos teus", ["tuas"] = "das tuas", ["seu"] = "do seu", ["sua"] = "da sua",
            ["seus"] = "dos seus", ["suas"] = "das suas", ["nosso"] = "do nosso", ["nossa"] = "da nossa", ["nossos"] = "dos nossos",
            ["nossas"] = "das nossas", ["outro"] = "doutro", ["outra"] = "doutra",
        };

        // Words after "procurando" that are not its object ("Estou procurando, mas...", "procurando saber", "procurando que...").
        private static readonly HashSet<string> NotObjects = new(StringComparer.OrdinalIgnoreCase)
        {
            "que", "se", "como", "onde", "quando", "aqui", "ali", "lá", "aí", "agora", "ainda", "há", "faz", "e", "mas", "ou", "em",
            "no", "na", "nos", "nas", "pela", "pelo", "por", "para", "pra", "com", "sem", "até", "desde", "sobre", "entre", "também",
            "muito", "bem", "mal", "nada", "tudo", "sempre", "nunca", "já", "mais", "menos", "assim", "lo", "la", "me", "te",
        };

        private static readonly HashSet<string> NounsLikeInfinitives = new(StringComparer.OrdinalIgnoreCase)
        {
            "lugar", "mulher", "jantar", "almoçar", "ar", "lar", "mar", "bar", "par", "poder", "dever", "prazer", "colher", "olhar",
            "sítio", "luar", "altar", "colar", "radar", "polegar", "talher", "açúcar", "elixir", "faquir", "emir", "vizir",
        };

        private static string ReplaceLookingFor(Match m)
        {
            var next = m.Groups["next"].Value;
            if (NotObjects.Contains(next) ||
                (Regex.IsMatch(next, @"^\p{Ll}+(?:ar|er|ir|ôr|or)$") && !NounsLikeInfinitives.Contains(next) && !next.EndsWith("dor", StringComparison.Ordinal)))
            {
                return m.Value;
            }

            var upper = CaseHelper.IsAllUpper(m.Groups["g"].Value);
            string obj;
            if (OfContractions.TryGetValue(next, out var contracted))
            {
                obj = char.IsUpper(next[0]) && !CaseHelper.IsAllUpper(next) ? CaseHelper.Capitalize(contracted) : contracted;
            }
            else
            {
                obj = "de " + next;
            }

            var result = m.Groups["aux"].Value + m.Groups["adv"].Value + " à procura " + obj;
            return upper ? result.ToUpperInvariant() : result;
        }

        private static string WithClitic(string infinitive, Group clitic) =>
            clitic.Success ? PortugueseVerbs.AttachEnclitic(infinitive, clitic.Value) : infinitive;

        private const string Clitic = @"(?:(?<cl>me|te|se|nos|lhe|lhes)\s+)?";
        private const string Gerund = @"(?<g>\p{Ll}+(?:ando|endo|indo|ondo))" + NotWordAfter + "(?:-(?<enc>me|te|se|nos|lhe|lhes|o|a|os|as|lo|la|los|las)" + NotWordAfter + ")?";

        // "Estou a abrir-me e tentando ser otimista" → "... e a tentar ser otimista".
        private static readonly Regex Coordinated = new(
            @"(?<head>" + NotWordBefore + @"(?:" + Auxiliaries + @")" + NotWordAfter + @"(?:\s+(?:" + Adverbs + @")" + NotWordAfter + @")*\s+a\s+\p{Ll}+(?:ar|er|ir|ôr|or)(?:-\p{Ll}+)?" +
            @"(?:[^.!?…\n,]{0,40}?))(?<conj>\s+(?:e|ou)|,)\s+" + Clitic + Gerund, Ci);

        // "Estava ocupada criando" / "Sou boa vivendo" → "ocupada a criar" / "boa a viver".
        private static readonly Regex AdjectiveGerund = new(
            NotWordBefore + @"(?<adj>ocupad[oa]s?|bom|boa|bons|boas|melhor|melhores|pior|piores|ótim[oa]s?|óptim[oa]s?|péssim[oa]s?|horrível|horríveis|cansad[oa]s?|fart[oa]s?|sentad[oa]s?|parad[oa]s?|deitad[oa]s?|escondid[oa]s?|acordad[oa]s?|ajoelhad[oa]s?|apanhad[oa]s?|pres[oa]s?|feliz|felizes|ansios[oa]s?|nervos[oa]s?|perdid[oa]s?|entretid[oa]s?|lá\s+fora|lá\s+dentro)\s+" + Clitic + Gerund, Ci);

        // "Tem gente vivendo aqui" / "os comboios chegando" / "mensagem dizendo" → "gente a viver" / "comboios a chegar" / "mensagem a dizer".
        private static readonly Regex NounGerund = new(
            NotWordBefore + @"(?<n>gente|alguém|ninguém|todos|todas|pessoas|pessoal|tipo|tipos|homem|homens|mulher|mulheres|miúdo|miúdos|miúda|miúdas|rapaz|rapazes|rapariga|raparigas|criança|crianças|" +
            @"casal|casais|carro|carros|comboio|comboios|polícia|polícias|agente|agentes|soldados|guardas|ele|ela|eles|elas|nós|vocês|mensagem|mensagens|recado|bilhete|" +
            @"carta|e-mail|email|nota|postal|cartaz|placa|aviso|vídeo|foto|fotografia|filho|filhos|filha|filhas|pai|mãe|pais|amigos|amigas|vizinhos|clientes|equipa|outros|outras|algo|alguma\s+coisa|coisa|coisas|nada|vigia|vigias|" +
            @"tu|ti|mim|eu|pessoa|cidadão|cidadãos|cara|caras|gajo|gajos|gaja|gajas|oficial|oficiais|grupo|unidade|modelos|loiras|loiros)\s+" +
            Clitic + Gerund, Ci);

        // "Há um elevador saindo do chão" → "Há um elevador a sair do chão".
        private static readonly Regex ThereIsGerund = new(
            @"(?<n>" + NotWordBefore + @"(?:há|havia|houve|tem|tinha)\s+(?:(?:um|uma|uns|umas|algum|alguma|alguns|algumas|muitos|muitas|vários|várias|dois|duas|três|\d+)\s+\p{L}+(?:\s+\p{L}+)?|(?:algo|alguém|gente)(?:\s+(?:muito|tão|mais|bem)\s+\p{L}+|\s+\p{L}+)?))\s+" +
            Clitic + Gerund, Ci);

        // "Estamos há dois anos trabalhando nisso" / "Virei a noite puxando" → "há dois anos a trabalhar" / "a noite a puxar".
        private static readonly Regex TimeSpanGerund = new(
            @"(?<n>" + NotWordBefore + @"(?:(?:há|faz|durante|por)\s+(?:(?:quase|mais\s+de|uns|umas)\s+)?(?:\p{L}+|\d+)\s+(?:anos|meses|dias|semanas|horas|minutos)|(?:a|uma)\s+noite(?:\s+toda)?|o\s+dia(?:\s+todo)?|todo\s+o\s+dia|uma\s+hora|horas)" + NotWordAfter + @")\s+" + Clitic + Gerund, Ci);

        // "Uma vigia me mandando sair" → "Uma vigia a mandar-me sair": a pronoun before a gerund is never Portuguese.
        private static readonly Regex CliticGerund = new(
            @"(?<n>" + NotWordBefore + @"(?!(?:vai|vou|vais|vamos|vão|ia|ias|íamos|iam|foi|fui|foram|vem|venho|vens|vêm|vinha|vinham|e|ou|não|já|que|se)" + NotWordAfter + @")\p{L}+)\s+(?<cl>me|te|nos|lhe|lhes)\s+" + Gerund, Ci);

        // "- Recarregando!" / "Só conferindo." (answers) → "- A recarregar!" / "Só a conferir."
        private static readonly Regex BareGerund = new(
            @"(?<pre>(?:^|[.!?…]\s+|\n|(?<![\p{L}\p{M}])[-–—]\s*|[\uE000-\uF8FF]\s*)[""“«']?(?:(?:e|ou)\s+)?)(?<adv>(?:só|apenas|ainda|sempre)\s+)?" + Clitic + Gerund + @"(?=(?:\s+[\p{L}\p{N}'-]+){0,3}\s*(?:[.!?…]|$))", Ci);

        private static readonly Regex LookAtPronoun = new(
            NotWordBefore + @"(?<pre>a\s+)(?<v>olhar)-(?<cl>me|te|se|nos|vos)(?=\s|[.!?,…]|$)", Ci);

        private static readonly HashSet<string> NotBareGerunds = new(StringComparer.OrdinalIgnoreCase)
        {
            "tendo", "sendo", "estando", "havendo", "considerando", "falando", "pensando", "resumindo", "voltando", "mudando", "brincando",
            "aproveitando", "começando", "seguindo", "querendo", "partindo", "contando", "tirando", "olhando", "supondo", "admitindo",
        };

        private static string ReplaceGerund(Match m, string head)
        {
            var g = m.Groups["g"].Value;
            if (!IsGerund(g))
            {
                return m.Value;
            }

            var clitic = m.Groups["cl"].Success ? m.Groups["cl"] : m.Groups["enc"];
            var allUpper = CaseHelper.IsAllUpper(g);
            var a = allUpper ? "A" : char.IsUpper(g[0]) ? "A" : "a";
            var infinitive = WithClitic(ToInfinitive(g), clitic);
            if (!allUpper)
            {
                infinitive = infinitive.ToLowerInvariant();
            }

            return head + a + " " + infinitive;
        }

        public static string Apply(string s)
        {
            s = LookingFor.Replace(s, ReplaceLookingFor);
            s = Progressive.Replace(s, Replace);
            s = EndUp.Replace(s, m => IsGerund(m.Groups["g"].Value)
                ? m.Groups["v"].Value + (CaseHelper.IsAllUpper(m.Value) ? " POR " : " por ") + (m.Groups["cl"].Success ? m.Groups["cl"].Value + " " : string.Empty) + ToInfinitive(m.Groups["g"].Value)
                : m.Value);
            s = Manner.Replace(s, m =>
            {
                var g = m.Groups["g"].Value;
                var v = m.Groups["v"].Value;
                var verb = v.Split(' ')[0];
                if (!IsGerund(g) || g.Equals("indo", StringComparison.OrdinalIgnoreCase) || g.Equals("vindo", StringComparison.OrdinalIgnoreCase) ||
                    verb.StartsWith("morr", StringComparison.OrdinalIgnoreCase) && g.ToLowerInvariant() is "sendo" or "tendo" or "estando" ||
                    (TimeVerbs.Contains(verb) && !verb.StartsWith("pass", StringComparison.OrdinalIgnoreCase) && !v.Contains(' ')))
                {
                    return m.Value;
                }

                return v + (CaseHelper.IsAllUpper(g) ? " A " : " a ") + WithClitic(ToInfinitive(g), m.Groups["cl"]);
            });
            s = Perception.Replace(s, m =>
            {
                var g = m.Groups["g"].Value;
                string infinitive;
                if (g.EndsWith("ndo", StringComparison.OrdinalIgnoreCase))
                {
                    if (!IsGerund(g))
                    {
                        return m.Value;
                    }

                    infinitive = ToInfinitive(g);
                }
                else
                {
                    if (NotPersonalInfinitives.Contains(g) || m.Groups["obj"].Length == 0)
                    {
                        return m.Value;
                    }

                    infinitive = g[..^2].Replace("ír", "ir");
                }

                return m.Groups["v"].Value + m.Groups["obj"].Value + " a " + infinitive;
            });
            for (var i = 0; i < 2; i++)
            {
                s = Coordinated.Replace(s, m => ReplaceGerund(m, m.Groups["head"].Value + m.Groups["conj"].Value + " "));
            }

            s = AdjectiveGerund.Replace(s, m => ReplaceGerund(m, m.Groups["adj"].Value + " "));
            s = NounGerund.Replace(s, m => ReplaceGerund(m, m.Groups["n"].Value + " "));
            s = ThereIsGerund.Replace(s, m => ReplaceGerund(m, m.Groups["n"].Value + " "));
            s = TimeSpanGerund.Replace(s, m => ReplaceGerund(m, m.Groups["n"].Value + " "));
            s = CliticGerund.Replace(s, m => ReplaceGerund(m, m.Groups["n"].Value + " "));
            s = BareGerund.Replace(s, m => NotBareGerunds.Contains(m.Groups["g"].Value)
                ? m.Value
                : ReplaceGerund(m, m.Groups["pre"].Value + m.Groups["adv"].Value));
            s = LookAtPronoun.Replace(s, m =>
            {
                var pronoun = m.Groups["cl"].Value.ToLowerInvariant() switch
                {
                    "me" => "mim", "te" => "ti", "se" => "si", "nos" => "nós", _ => "vós",
                };
                var result = m.Groups["v"].Value + " para " + pronoun;
                return m.Groups["pre"].Value + (CaseHelper.IsAllUpper(m.Groups["v"].Value) ? result.ToUpperInvariant() : result);
            });
            return s;
        }

        private static bool IsBlockedGerund(string word) => NotGerunds.Contains(word) || word is "Armando" or "ARMANDO";

        private static bool IsGerund(string word) => word.Length > 4 && !IsBlockedGerund(word);

        private static string Replace(Match m)
        {
            var gerund = m.Groups["g"].Value;
            if (IsBlockedGerund(gerund) && !gerund.Equals("vendo", StringComparison.OrdinalIgnoreCase))
            {
                return m.Value;
            }

            var infinitive = ToInfinitive(gerund);
            var clitic = m.Groups["cl"].Success ? m.Groups["cl"].Value : m.Groups["enc"].Value;
            if (clitic.Length > 0)
            {
                infinitive = PortugueseVerbs.AttachEnclitic(infinitive, clitic);
            }

            var a = CaseHelper.IsAllUpper(gerund) ? "A" : "a";
            return m.Groups["aux"].Value + m.Groups["adv"].Value + " " + a + " " + infinitive;
        }

        internal static string ToInfinitive(string gerund)
        {
            var upper = CaseHelper.IsAllUpper(gerund);
            var lower = gerund.ToLowerInvariant();
            var result = lower == "pondo" ? "pôr" : lower[..^3] + "r";
            return upper ? result.ToUpperInvariant() : CaseHelper.MatchCase(gerund, result);
        }
    }

    // ================================================================== pronoun placement

    internal static class PronounRules
    {
        private const string Start = @"(?<pre>(?:^|[.!?…:;]\s+|(?<![\p{L}\p{N},][ \t]*)\n)(?:(?<![\p{L}\p{M}])[-–—]\s*)?(?:[\uE000-\uF8FF]\s*)*(?:(?:E|Mas|Então|Ah|Oh|Ok|Bem|Agora|Por favor),?\s+)?)";
        private const string AfterComma = @"(?<pre>,\s+(?:[\uE000-\uF8FF]\s*)*)";

        private static readonly HashSet<string> NotVerbs = new(StringComparer.OrdinalIgnoreCase)
        {
            "não", "nunca", "também", "já", "mais", "muito", "muita", "só", "ainda", "sempre", "a", "o", "as", "os", "de",
            "que", "e", "em", "por", "para", "pra", "com", "um", "uma", "mesmo", "mesma", "eu", "tu", "ele", "ela", "você",
            "nós", "eles", "elas", "vocês", "isso", "isto", "aquilo", "aqui", "ali", "lá", "agora", "bem", "mal", "tão",
            "quando", "se", "me", "te", "nos", "lhe", "lhes", "vos", "do", "da", "no", "na", "ao", "à", "tanto", "quanto",
            "jamais", "logo", "depois", "antes", "como", "onde", "quem", "qual", "mas", "ou", "nem", "seu", "sua", "meu",
            "minha", "teu", "tua", "dele", "dela", "deles", "delas", "todo", "toda", "todos", "todas",
        };

        // "Me diga" / "Te amo" at the start of a sentence → "Diga-me" / "Amo-te"
        private static readonly Regex StartClitic = new(Start + @"(?<cl>Me|Te|Lhe|Lhes|Nos|ME|TE|LHE|LHES|NOS)\s+(?<v>\p{L}+)" + NotWordAfter + @"(?!-)", Cs);

        // Brazilian "Apenas me mate" (English "just kill me") → "Mate-me apenas": in Portugal the command comes first and the
        // pronoun goes after it. Only imperatives move - "Apenas me deixou" is a statement and stays.
        private static readonly Regex JustCommand = new(
            Start + @"(?<adv>[Aa]penas|APENAS|[Ss]implesmente|SIMPLESMENTE)\s+(?<cl>me|te|nos|o|a|os|as|lhe|lhes|se|ME|TE|NOS|LHE|SE)\s+" +
            @"(?<v>\p{L}+)" + NotWordAfter + @"(?!-)(?<paz>\s+em\s+paz" + NotWordAfter + ")?", Cs | RegexOptions.Multiline);

        private static string ReplaceJustCommand(Match m)
        {
            var v = m.Groups["v"].Value;
            if (TuImperative.ToTu(v) == null)
            {
                return m.Value;
            }

            var adv = m.Groups["adv"].Value;
            var verb = PortugueseVerbs.AttachEnclitic(v.ToLowerInvariant(), m.Groups["cl"].Value.ToLowerInvariant());
            if (adv.StartsWith('S') || adv.StartsWith('s'))
            {
                // "Simplesmente se acalme" → "Simplesmente acalme-se": the adverb stays in front, only the pronoun moves.
                return m.Groups["pre"].Value + adv + " " + (CaseHelper.IsAllUpper(adv) ? verb.ToUpperInvariant() : verb) + m.Groups["paz"].Value;
            }

            verb = CaseHelper.IsAllUpper(adv) ? verb.ToUpperInvariant() : char.IsUpper(adv[0]) ? CaseHelper.Capitalize(verb) : verb;
            // "Apenas me deixe em paz" → "Deixe-me em paz": "apenas" after it would read "leave me only in peace".
            return m.Groups["paz"].Success
                ? m.Groups["pre"].Value + verb + m.Groups["paz"].Value
                : m.Groups["pre"].Value + verb + " " + adv.ToLowerInvariant();
        }

        private static readonly Regex ReflexiveCommand = new(
            Start + @"(?<cl>se)\s+(?<v>afaste|afastem|aproxime|aproximem|sente|sentem|levante|levantem|deite|deitem|" +
            @"cale|calem|prepare|preparem|proteja|protejam|esconda|escondam|apresse|apressem|despache|despachem|" +
            @"vire|virem|mexa|mexam|acalme|acalmem)" + NotWordAfter + @"(?!-)", Ci);

        private static string ReplaceReflexiveCommand(Match m)
        {
            var after = m.Result("$'");
            var clause = Regex.Match(after, @"^[^.!?…\n]*").Value;
            if (m.Groups["v"].Value.Equals("sente", StringComparison.OrdinalIgnoreCase) &&
                Regex.IsMatch(clause, @"^\s+(?:dor|fome|sede|medo|frio|calor|falta|saudade|saudades)" + NotWordAfter, Ci) ||
                Regex.IsMatch(clause, @"^\s+(?:ele|ela|eles|elas|você|vocês|o\s+senhor|a\s+senhora)" + NotWordAfter, Ci) ||
                clause.Contains(',') || Regex.IsMatch(clause, @"\b(?:então|caso|e\s+(?:eu|tu|ele|ela|nós|você))\b", Ci))
            {
                return m.Value;
            }
            return ReplaceLeading(m, capitalize: true);
        }

        // "Nos vemos" → "Vemo-nos"
        private static readonly Regex NosVemos = new(Start + @"(?<cl>Nos|NOS)\s+(?<v>\p{L}+mos)" + NotWordAfter, Cs);

        // ", me liga" → ", liga-me"
        private static readonly Regex CommaClitic = new(AfterComma + @"(?<cl>me|te|lhe|lhes|nos)\s+(?<v>\p{Ll}+)" + NotWordAfter + @"(?!-)", Cs);

        // "Eu te amo" / "Ele se chama" / "Ela o viu" at the start of a clause → "Eu amo-te" / "Ele chama-se" / "Ela viu-o"
        private static readonly Regex SubjectClitic = new(
            @"(?<pre>(?:^|[.!?…:;,]\s+|\n)(?:(?<![\p{L}\p{M}])[-–—]\s*)?(?:[\uE000-\uF8FF]\s*)*(?:(?:[Ee]|[Mm]as|[Ee]ntão)\s+)?)" +
            @"(?<subj>(?:[Ee]u|[Ee]le|[Ee]la|[Nn]ós|[Ee]les|[Ee]las|[Tt]u|[Vv]ocê|[Vv]ocês|EU|ELE|ELA|NÓS|ELES|ELAS|TU|VOCÊ|VOCÊS))\s+" +
            @"(?<cl>me|te|se|nos|vos|lhe|lhes|o|a|os|as|ME|TE|SE|NOS|VOS|LHE|LHES|O|A|OS|AS)\s+(?<v>\p{L}+)" + NotWordAfter + @"(?!-)", Cs);

        // "Vou te matar" / "Posso te ajudar" → "Vou matar-te" / "Posso ajudar-te"
        private static readonly Regex ModalClitic = new(
            NotWordBefore + @"(?<m>\p{L}+)(?<held>-(?:me|te|nos|lhe))?\s+(?<cl>me|te|se|nos|vos|lhe|lhes)\s+(?<v>\p{L}+(?:ar|er|ir|or|ôr))" + NotWordAfter + @"(?!-)", Ci);

        // "obriga-os a te verem" / "dopar-te e depois te afogar" → "a verem-te" / "depois afogar-te".
        private static readonly Regex LinkedInfinitiveClitic = new(
            NotWordBefore + @"(?<l>a|e\s+depois|depois|e|ou|mas)\s+(?<cl>me|te|se|nos|vos|lhe|lhes)\s+" +
            @"(?<v>(?:[\p{L}\p{M}]+(?:ar|er|ir|or|ôr)|pôr)(?:em|es|mos)?)" + NotWordAfter + @"(?!-)", Ci);

        private static string ReplaceLinkedInfinitive(Match m)
        {
            var verb = m.Groups["v"].Value;
            var infinitive = Regex.Replace(verb.ToLowerInvariant(), @"(?:em|es|mos)$", "");
            // At a coordinated clause start, "se" can be a conditional conjunction, not a clitic.
            if (m.Groups["cl"].Value.Equals("se", StringComparison.OrdinalIgnoreCase) &&
                m.Groups["l"].Value.ToLowerInvariant() is "e" or "ou" or "mas")
                return m.Value;
            if (!PortugueseVerbs.IsInfinitive(infinitive) ||
                (!PortugueseVerbs.IsKnownInfinitive(infinitive) && !NaturaDictionary.Load(true).Check(infinitive)))
                return m.Value;
            return m.Groups["l"].Value + " " + PortugueseVerbs.AttachEnclitic(verb, m.Groups["cl"].Value);
        }

        // "Eu vi ele" / "Mata ela!" → "Eu vi-o" / "Mata-a!"  ("não vi ele" → "não o vi")
        private static readonly Regex ObjectPronoun = new(
            @"(?<trig>" + NotWordBefore + @"(?:não|nunca|jamais|também|já|ainda|só|nem|sempre|que|quem|porque|onde|quando|como|talvez)\s+)?" + NotWordBefore +
            @"(?<v>\p{L}+)\s+(?<p>ele|ela|eles|elas)" + NotWordAfter + @"(?!\s+(?:mesm[oa]s?|própri[oa]s?|também|é|são|foi|era|está|estão|disse|tem|vai|não|que)" + NotWordAfter + ")", Ci);

        private static readonly HashSet<string> Modals = new(StringComparer.OrdinalIgnoreCase)
        {
            "vou", "vais", "vai", "vamos", "vão", "ia", "ias", "íamos", "iam", "iria", "irias", "iríamos", "iriam",
            "fui", "foi", "quero", "queres", "quer",
            "queremos", "querem", "queria", "querias", "queríamos", "queriam", "posso", "podes", "pode", "podemos",
            "podem", "podia", "podias", "podíamos", "podiam", "poderia", "poderias", "poderíamos", "poderiam",
            "preciso", "precisas", "precisa", "precisamos", "precisam", "precisava", "devo", "deves", "deve", "devemos",
            "devem", "devia", "deveria", "deverias", "consigo", "consegues", "consegue", "conseguimos", "conseguem",
            "tento", "tentas", "tenta", "tentamos", "tentam", "venho", "vens", "vem", "vim", "veio", "deixa", "deixe",
            "tentei", "tentaste", "tentou", "tentámos", "tentaram", "tentava", "tentavas", "tentávamos", "tentavam",
            "tentarei", "tentarás", "tentará", "tentaremos", "tentarão", "tentaria", "tentarias", "tentaríamos", "tentariam",
            "deixo", "sei", "sabe", "sabes", "gostaria", "gostava", "costumo", "costuma", "acabei", "acabou", "comecei",
            "começou", "tenho", "tens", "tem", "temos", "têm", "tinha", "vem", "tentar", "querer", "poder", "dever", "conseguir",
            "começar", "costumar", "ousar", "precisar", "deixar", "saber", "continuar",
        };

        // Infinitives that do not take a direct object ("Pode ser ele", "até chegar ele").
        private static readonly HashSet<string> IntransitiveInfinitives = new(StringComparer.OrdinalIgnoreCase)
        {
            "ser", "estar", "ir", "vir", "ficar", "sair", "chegar", "morrer", "nascer", "voltar", "viver", "parecer", "acontecer",
            "existir", "dormir", "ter", "haver", "partir", "entrar", "aparecer", "falar", "acordar", "continuar", "desaparecer",
            "trabalhar", "ganhar", "perder", "mentir", "gostar", "precisar", "concordar", "casar", "correr", "andar", "subir", "descer",
            "cair", "telefonar", "responder", "dizer", "perguntar", "agradecer", "obedecer", "pertencer", "sorrir", "rir", "chorar",
            "gritar", "jantar", "almoçar", "votar", "lutar", "sonhar", "pensar", "esperar", "fugir", "nadar", "viajar", "brincar",
        };

        // "Quero estrangular ele" / "deixarem ela em paz" → "estrangulá-lo" / "deixarem-na": any dictionary infinitive after another word.
        private static bool IsObjectInfinitive(Match m, string verb)
        {
            var lower = verb.ToLowerInvariant();
            var infinitive = Regex.Replace(lower, @"(?<=[aei]r)(?:em|es)$", string.Empty);
            if (m.Groups["trig"].Length > 0 || !PortugueseVerbs.IsInfinitive(infinitive) || IntransitiveInfinitives.Contains(infinitive) ||
                !Regex.IsMatch(m.Result("$`"), @"\p{L}[\p{L}\p{M}]*\s+$"))
            {
                return false;
            }

            var before = Regex.Match(m.Result("$`"), @"(?<w>\p{L}+)\s+$").Groups["w"].Value.ToLowerInvariant();
            if (before is "o" or "a" or "os" or "as" or "um" or "uma" or "do" or "da" or "no" or "na" or "ao" or "à" or "seu" or "sua" or "meu" or "minha" or "teu" or "tua")
            {
                return false;
            }

            return PortugueseVerbs.IsKnownInfinitive(infinitive) || NaturaDictionary.Load(true).Check(infinitive);
        }

        // "Acho ela divertida" → "Acho-a divertida": "achar" + person + adjective.
        private static bool IsThinkOfAs(Match m, string verb) =>
            Regex.IsMatch(verb, @"^(?:acho|achas|acha|achei|achaste|achou|achava|achavas|achamos)$", RegexOptions.IgnoreCase) &&
            Regex.IsMatch(m.Result("$'"), @"^\s+(?:(?:muito|tão|bem|meio|mais|bastante)\s+)?\p{L}{3,}(?:o|a|os|as|l|z|e)(?=\s*(?:[.,!?…;:]|$))", RegexOptions.IgnoreCase) &&
            !Regex.IsMatch(m.Result("$'"), @"^\s+(?:(?:muito|tão|bem|meio|mais|bastante)\s+)?(?:sabe|pode|quer|deve|vive|morre|mente|faz|diz|existe|bebe|come)\b", RegexOptions.IgnoreCase);

        private static readonly HashSet<string> ObjectVerbStopWords = new(StringComparer.OrdinalIgnoreCase)
        {
            "que", "quê", "quem", "onde", "como", "quando", "qual", "porque", "porquê", "e", "ou", "mas", "se", "nem",
            "disse", "diz", "perguntou", "respondeu", "gritou", "falou", "explicou", "contou", "para", "com", "de",
        };

        // "Meu pai disse" → "O meu pai disse" (European Portuguese puts the article before a possessive).
        private static readonly Regex BarePossessive = new(
            @"(?<pre>(?:^|[.!?…:;]\s+|\n)(?:(?<![\p{L}\p{M}])[-–—]\s*)?(?:[\uE000-\uF8FF]\s*)*(?:(?:E|Mas|Então|Porque|Acho\s+que|Sei\s+que)\s+)?)" +
            @"(?<poss>(?i:meu|minha|meus|minhas|teu|tua|teus|tuas|seu|sua|seus|suas|nosso|nossa|nossos|nossas))" +
            @"(?=\s+(?<n>\p{L}+)\s+(?<w1>\p{L}+)(?:\s+(?<w2>\p{L}+))?)", Cs);

        private static readonly HashSet<string> CommonVerbs = new(StringComparer.OrdinalIgnoreCase)
        {
            "é", "está", "foi", "era", "tem", "vai", "não", "já", "estava", "ficou", "disse", "são", "estão", "foram", "eram",
            "têm", "vão", "morreu", "chegou", "ligou", "quer", "pode", "precisa", "sabe", "gosta", "mora", "trabalha", "ganhou",
            "perdeu", "acabou", "fez", "deu", "viu", "nunca", "também", "sempre", "ainda", "acha", "diz", "estará", "será",
            "tinha", "teve", "vem", "veio", "saiu", "morre", "anda", "parece", "pareceu", "chama", "chamava", "faz", "ama",
        };

        private static string AddArticle(Match m)
        {
            var noun = m.Groups["n"].Value;
            if (noun is "Deus" or "deus" || !(IsVerbish(m.Groups["w1"].Value) || (m.Groups["w2"].Success && IsVerbish(m.Groups["w2"].Value) && !IsVerbish(noun))))
            {
                return m.Value;
            }

            var poss = m.Groups["poss"].Value;
            var lower = poss.ToLowerInvariant();
            var article = lower is "minha" or "tua" or "sua" or "nossa" ? "a"
                : lower is "minhas" or "tuas" or "suas" or "nossas" ? "as"
                : lower.EndsWith('s') ? "os" : "o";
            var pre = m.Groups["pre"].Value;
            var startsSentence = pre.Length == 0 || !Regex.IsMatch(pre, @"\p{L}\s+$");
            if (CaseHelper.IsAllUpper(poss))
            {
                return pre + article.ToUpperInvariant() + " " + poss;
            }

            return pre + (startsSentence && char.IsUpper(poss[0]) ? CaseHelper.Capitalize(article) : article) + " " + poss.ToLowerInvariant();
        }

        // "Meu bebé!" → "O meu bebé!" - an exclamation about someone/something (not a vocative like "Meu Deus!", "Meu amor!").
        private static readonly Regex ExclaimedPossessive = new(
            @"(?<pre>(?:^|[.!?…:;]\s+|\n)(?:(?<![\p{L}\p{M}])[-–—]\s*)?(?:[\uE000-\uF8FF]\s*)*)(?<poss>(?i:meu|minha|meus|minhas|nosso|nossa|nossos|nossas))\s+(?<n>\p{L}+)(?=\s*!)", Cs);

        private static readonly HashSet<string> Vocatives = new(StringComparer.OrdinalIgnoreCase)
        {
            "deus", "amor", "querido", "querida", "queridos", "queridas", "caro", "cara", "caros", "senhor", "senhora", "senhores",
            "filho", "filha", "filhos", "rapaz", "amigo", "amiga", "amigos", "irmão", "irmã", "bem", "velho", "velha", "jesus",
            "menino", "menina", "anjo", "lindo", "linda", "querer", "pai", "mãe", "chapa", "camarada", "capitão", "general", "rei",
            "rainha", "príncipe", "princesa", "lorde", "deusa", "docinho", "fofo", "fofa", "tesouro", "doce", "mano", "mana",
            "caramba", "céus", "povo", "gente", "irmãos", "senhoras",
        };

        private static string AddExclaimedArticle(Match m)
        {
            var noun = m.Groups["n"].Value;
            if (Vocatives.Contains(noun) || char.IsUpper(noun[0]) && !CaseHelper.IsAllUpper(noun))
            {
                return m.Value;
            }

            var poss = m.Groups["poss"].Value;
            var lower = poss.ToLowerInvariant();
            var article = lower is "minha" or "nossa" ? "a" : lower is "minhas" or "nossas" ? "as" : lower.EndsWith('s') ? "os" : "o";
            if (CaseHelper.IsAllUpper(poss))
            {
                return m.Groups["pre"].Value + article.ToUpperInvariant() + " " + poss + " " + noun;
            }

            return m.Groups["pre"].Value + (char.IsUpper(poss[0]) ? CaseHelper.Capitalize(article) : article) + " " + lower + " " + noun;
        }

        // "Tua amiga loira que..." → "A tua amiga loira que..." (sentence-initial possessive + noun, not a vocative).
        private static readonly Regex StartPossessive = new(
            @"(?<pre>(?:^|[.!?…:;]\s+|\n)(?:(?<![\p{L}\p{M}])[-–—]\s*)?(?:[\uE000-\uF8FF]\s*)*[""“«']?" +
            @"(?:(?i:desculp[ae]|ouve|ouça|escuta|escute|olha|olhe|sim|não|bem|ok|ei|então|aliás|enfim),\s+(?=(?i:teu|tua|teus|tuas|nosso|nossos|nossas)\s))?)(?<poss>(?i:teu|tua|teus|tuas|seu|sua|seus|suas|nosso|nossa|nossos|nossas))\s+(?<n>\p{Ll}+)(?!\s*[,!?])" + NotWordAfter, Cs);

        // "Nossa" opens exclamations ("Nossa, que susto"), so at the start of a cue it is only a possessive before one of these nouns.
        private static readonly HashSet<string> NossaNouns = new(StringComparer.OrdinalIgnoreCase)
        {
            "foto", "fotos", "casa", "família", "filha", "equipa", "equipe", "vida", "cadeira", "conta", "empresa", "cidade", "escola", "rua",
            "cama", "mesa", "cozinha", "sala", "festa", "relação", "conversa", "missão", "chance", "vez", "parte", "história", "culpa",
            "responsabilidade", "gente", "unidade", "esquadra", "patrulha", "testemunha", "ajuda", "decisão", "escolha", "opinião",
            "amizade", "viagem", "reunião", "investigação", "operação", "prova", "provas", "tarefa", "linha", "esperança", "força", "voz",
        };

        private static string AddStartArticle(Match m)
        {
            // "Desculpa, tua mãe ligou": after an opening word, a family noun followed by a verb is the subject, not an address.
            var subjectAfterOpening = m.Groups["pre"].Value.TrimEnd().EndsWith(',') &&
                                      Regex.IsMatch(m.Result("$'"), @"^\s+(?:\p{Ll}+\s+)?(?:ligou|está|esteve|estava|quer|disse|chegou|veio|vem|vai|foi|é|era|tem|teve|precisa|pediu|perguntou|mandou|telefonou|morreu|saiu|ligou)" + NotWordAfter, Cs);
            if (Vocatives.Contains(m.Groups["n"].Value) && !subjectAfterOpening ||
                m.Groups["poss"].Value.Equals("nossa", StringComparison.OrdinalIgnoreCase) && !NossaNouns.Contains(m.Groups["n"].Value))
            {
                return m.Value;
            }

            // "Foi pessoal para nós,\nseu parvo." - name-calling closing the sentence takes no article.
            if (YouToTuRules.Insults.Contains(m.Groups["n"].Value) && Regex.IsMatch(m.Result("$'"), @"^\s*(?:[.!?…]|$)"))
            {
                return m.Value;
            }

            // "Além de aumentar o\nseu colesterol": the article is at the end of the previous line.
            if (char.IsLower(m.Groups["poss"].Value[0]) &&
                Regex.Match(m.Result("$`") + m.Groups["pre"].Value, @"(\p{L}+)\s*$").Groups[1].Value is { Length: > 0 } last && ArticleContractions.Contains(last))
            {
                return m.Value;
            }

            var poss = m.Groups["poss"].Value;
            var article = ArticleFor(poss);
            return CaseHelper.IsAllUpper(poss)
                ? m.Groups["pre"].Value + article.ToUpperInvariant() + " " + poss + " " + m.Groups["n"].Value
                : m.Groups["pre"].Value + (m.Groups["pre"].Value.TrimEnd().EndsWith(',') || char.IsLower(poss[0]) ? article : CaseHelper.Capitalize(article)) + " " + poss.ToLowerInvariant() + " " + m.Groups["n"].Value;
        }

        private static string ArticleFor(string possessive)
        {
            var lower = possessive.ToLowerInvariant();
            return lower is "minha" or "tua" or "sua" or "nossa" ? "a"
                : lower is "minhas" or "tuas" or "suas" or "nossas" ? "as"
                : lower.EndsWith('s') ? "os" : "o";
        }

        private static readonly Regex IntroductoryPossessive = new(
            @"(?<pre>(?:^|[.!?…:;]\s+|\n)(?:(?<![\p{L}\p{M}])[-–—]\s*)?(?:[\uE000-\uF8FF]\s*)*(?:agora|por\s+outro\s+lado|entretanto|no\s+entanto|quanto\s+a\s+isso)\s*,\s*)(?<poss>meu|minha|meus|minhas|teu|tua|teus|tuas|seu|sua|seus|suas|nosso|nossa|nossos|nossas)(?<sp>\s+)(?<n>\p{L}+)" + NotWordAfter, Ci);

        private static string AddIntroductoryArticle(Match m)
        {
            var poss = m.Groups["poss"].Value;
            var noun = m.Groups["n"].Value;
            var addressedPossessive = Regex.IsMatch(poss, @"^(?:teu|tua|teus|tuas|seu|sua|seus|suas)$", Ci);
            if (Vocatives.Contains(noun) && !addressedPossessive)
            {
                return m.Value;
            }

            var article = ArticleFor(poss);
            if (CaseHelper.IsAllUpper(m.Value)) article = article.ToUpperInvariant();
            return m.Groups["pre"].Value + article + " " + poss + m.Groups["sp"].Value + noun;
        }

        // "aceitamos tua opinião" → "aceitamos a tua opinião", "com tua mãe" → "com a tua mãe", "de seu pai" → "do seu pai".
        private static readonly Regex InnerPossessive = new(
            NotWordBefore + @"(?<prev>\p{L}+)(?<between>\s+)(?<poss>meu|minha|meus|minhas|teu|tua|teus|tuas|seu|sua|seus|suas|nosso|nossa|nossos|nossas)" + NotWordAfter + @"(?<after>\s+)(?=(?<n>\p{L}+))", Ci);

        private static readonly HashSet<string> ArticleContractions = new(StringComparer.OrdinalIgnoreCase)
        {
            "o", "a", "os", "as", "no", "na", "nos", "nas", "do", "da", "dos", "das", "ao", "à", "aos", "às", "pelo", "pela", "pelos", "pelas",
        };

        // "Diga a sua mãe" / "Conta a teu pai" → "Diga à sua mãe" / "Conta ao teu pai": "a" is the preposition before a person.
        private static readonly Regex DativePossessive = new(
            NotWordBefore + @"(?<v>diz|diga|digas|digam|dizer|disse|disseste|contar|conta|conte|contes|contou|perguntar|pergunta|pergunte|perguntes|perguntou|" +
            @"pedir|pede|peça|peças|pediu|explicar|explica|explique|expliques|explicou|telefonar|telefona|telefone|telefones|dar|dá|dê|dês|deu|" +
            @"mandar|manda|mande|mandes|mandou|entregar|entrega|entregue|entregues|entregou|mostrar|mostra|mostre|mostres|mostrou)" +
            @"(?<cl>-(?:lhe|lhes|o|a|os|as|lo|la|los|las|no|na))?(?<sp1>\s+)(?<a>a)(?<sp2>\s+)(?<poss>meu|minha|meus|minhas|teu|tua|teus|tuas|seu|sua|seus|suas|nosso|nossa|nossos|nossas)" +
            @"(?<sp3>\s+)(?<n>\p{L}+)" + NotWordAfter, Ci);

        private static readonly HashSet<string> FemalePeople = new(StringComparer.OrdinalIgnoreCase)
        {
            "mãe", "mulher", "esposa", "irmã", "filha", "namorada", "avó", "tia", "prima", "amiga", "chefe", "família", "parceira",
            "colega", "mana", "sogra", "noiva", "vizinha", "advogada", "médica", "professora", "mamã", "mamãe", "equipa", "senhora",
            "mães", "mulheres", "irmãs", "filhas", "amigas", "tias", "primas", "famílias", "parceiras", "colegas", "avós", "vizinhas",
        };

        private static string ReplaceDativePossessive(Match m)
        {
            var poss = m.Groups["poss"].Value.ToLowerInvariant();
            var feminine = poss is "minha" or "minhas" or "tua" or "tuas" or "sua" or "suas" or "nossa" or "nossas";
            if (feminine && !FemalePeople.Contains(m.Groups["n"].Value))
            {
                return m.Value; // "Mostre a sua arma" is the article
            }

            var plural = poss.EndsWith('s');
            var to = feminine ? plural ? "às" : "à" : plural ? "aos" : "ao";
            var a = m.Groups["a"].Value;
            return m.Groups["v"].Value + m.Groups["cl"].Value + m.Groups["sp1"].Value + (char.IsUpper(a[0]) ? to.ToUpperInvariant() : to) +
                   m.Groups["sp2"].Value + m.Groups["poss"].Value + m.Groups["sp3"].Value + m.Groups["n"].Value;
        }

        // "Agradeça o teu chefe" → "Agradece ao teu chefe": a person thanked takes "a" in Portugal ("agradecer o favor" is kept).
        private static readonly Regex ThankPerson = new(
            NotWordBefore + @"(?<v>agradec\p{L}*|agradeç\p{L}*)(?<sp1>\s+)(?<art>o|a|os|as)(?<sp2>\s+)" +
            @"(?<poss>(?:meu|minha|meus|minhas|teu|tua|teus|tuas|seu|sua|seus|suas|nosso|nossa|nossos|nossas|vosso|vossa|vossos|vossas)\s+)?" +
            @"(?<n>\p{L}+)" + NotWordAfter, Ci);

        private static readonly HashSet<string> MalePeople = new(StringComparer.OrdinalIgnoreCase)
        {
            "pai", "marido", "irmão", "filho", "namorado", "avô", "tio", "primo", "amigo", "chefe", "parceiro", "colega", "mano", "sogro",
            "noivo", "vizinho", "advogado", "médico", "professor", "papá", "papai", "senhor", "capitão", "tenente", "sargento", "detetive",
            "comissário", "agente", "presidente", "juiz", "procurador", "padre", "rapaz", "homem", "povo", "pessoal", "público",
            "pais", "maridos", "irmãos", "filhos", "amigos", "tios", "primos", "avós", "parceiros", "colegas", "vizinhos", "rapazes",
            "homens", "senhores", "polícias", "bombeiros", "médicos", "agentes", "jurados", "eleitores",
        };

        private static string ReplaceThankPerson(Match m)
        {
            var n = m.Groups["n"].Value;
            var art = m.Groups["art"].Value;
            var possessive = m.Groups["poss"].Success;
            // "Agradeça o Paulo" - a name after the article is a person.
            var person = MalePeople.Contains(n) || FemalePeople.Contains(n) || !possessive && char.IsUpper(n[0]) && art is "o" or "os" && !n.Equals("Deus", StringComparison.Ordinal);
            if (!person)
            {
                return m.Value;
            }

            var to = art.ToLowerInvariant() switch { "o" => "ao", "a" => "à", "os" => "aos", _ => "às" };
            return m.Groups["v"].Value + m.Groups["sp1"].Value + (char.IsUpper(art[0]) ? CaseHelper.Capitalize(to) : to) +
                   m.Groups["sp2"].Value + m.Groups["poss"].Value + n;
        }

        private static readonly HashSet<string> PlainPrepositions = new(StringComparer.OrdinalIgnoreCase)
        {
            "com", "para", "sem", "sobre", "contra", "entre", "até", "após", "perante", "sob", "conforme", "segundo",
        };

        private static readonly HashSet<string> Conjunctions = new(StringComparer.OrdinalIgnoreCase)
        {
            "que", "e", "mas", "ou", "porque", "quando", "se", "como", "onde", "nem", "enquanto", "embora", "nas", "também", "só",
        };

        private static readonly HashSet<string> NotPastVerbs = new(StringComparer.OrdinalIgnoreCase)
        {
            "lei", "rei", "sei", "meu", "seu", "teu", "céu", "chapéu", "museu", "adeus", "deus", "judeu", "europeu", "ateu", "pneu",
            "véu", "réu", "eu", "sou", "vou", "dou", "estou", "pouco", "louco", "outro", "outros", "muito", "muitos",
        };

        // Past tense and infinitive endings ("Pesquisei", "aproveitar") of verbs not in the built-in list.
        private static bool LooksLikeVerbForm(string word) =>
            word.Length >= 5 && !CaseHelper.IsAllUpper(word) && !NotPastVerbs.Contains(word) &&
            (PortugueseVerbs.IsInfinitive(word) || Regex.IsMatch(word, @"(?:ei|ou|eu|iu|aste|este|iste|ámos|aram|eram|iram)$", Cs));

        private static readonly HashSet<string> Copulas = new(StringComparer.OrdinalIgnoreCase)
        {
            "é", "são", "era", "eram", "foi", "foram", "ser", "sou", "és", "somos", "seja", "sejam", "fosse", "será", "seria", "serão",
            "parece", "pareces", "parecem", "fica", "ficou", "ficas", "como", "casa", "conta", "mas", "fora", "olá", "obrigado", "obrigada",
        };

        private static readonly HashSet<string> SerForms = new(StringComparer.OrdinalIgnoreCase)
        {
            "é", "são", "era", "eram", "foi", "foram", "ser", "sou", "és", "somos", "seja", "sejam", "fosse", "fossem", "será", "seria", "serão",
        };

        // After "ser", Portugal says "É o meu primeiro dia" / "Qual é o teu nome?", but often leaves people and a few abstract
        // nouns bare ("Ele é meu amigo", "Não é tua culpa"), as real PT-PT subtitles do.
        private static readonly HashSet<string> BareAfterSer = new(StringComparer.OrdinalIgnoreCase)
        {
            "amigo", "amiga", "amigos", "amigas", "filho", "filha", "filhos", "filhas", "irmão", "irmãos", "primo", "prima", "primos", "primas",
            "chefe", "chefes", "namorado", "namorada", "marido", "parceiro", "parceira", "parceiros", "advogado", "advogada", "cliente", "clientes",
            "testemunha", "tio", "tia", "gerente", "patrão", "patroa", "fã", "madrasta", "padrasto", "colega", "colegas", "vizinho", "vizinha",
            "cunhado", "cunhada", "afilhado", "afilhada", "padrinho", "madrinha", "noivo", "noiva", "menino", "carro", "escritório",
            "culpa", "ideia", "intenção", "opinião", "responsabilidade", "escolha", "hábito", "pecados", "conta", "assunto",
        };

        private static readonly HashSet<string> NotNounsAfterPossessive = new(StringComparer.OrdinalIgnoreCase)
        {
            "e", "ou", "também", "agora", "por", "para", "de", "que", "mesmo", "só", "sim", "não", "hoje", "aqui", "ali", "lá", "então",
            "é", "se", "como", "mas", "em", "com", "a", "o", "os", "as", "um", "uma", "nem", "já", "ainda", "sempre", "nunca", "todo", "toda",
        };

        private static string AddInnerArticle(Match m)
        {
            var prev = m.Groups["prev"].Value;
            var poss = m.Groups["poss"].Value;
            var noun = m.Groups["n"].Value;
            if (ArticleContractions.Contains(prev))
            {
                return m.Value; // "pega nas tuas coisas" already has the article
            }
            // "que sua filha desapareceu" is a subject, not an address; "mas, meu filho, ..." / "e meu amor!" still are.
            var subjectAfterConjunction = Conjunctions.Contains(prev) &&
                                          Regex.IsMatch(m.Result("$'"), @"^\p{L}+\s+\p{L}", Cs);
            // "Tenho de encontrar teu irmão": a vocative needs a comma, so a noun straight after an infinitive or a past-tense verb is an object.
            var objectOfVerb = PortugueseVerbs.IsInfinitive(prev) || Regex.IsMatch(prev, @"(?:ei|ou|eu|iu|aste|este|iste|ámos|aram|eram|iram)$", Ci) ||
                               prev.ToLowerInvariant() is "tenho" or "preciso" or "quero" or "conheço" or "amo" or "vejo" ||
                               prev.Length > 5 && Regex.IsMatch(prev, @"(?:ando|endo|indo)$", Ci) &&
                               !Regex.IsMatch(prev, @"^(?:quando|comando|brando|lindo|findo|contrabando|segundo)$", Ci);
            if (Vocatives.Contains(noun) && !PlainPrepositions.Contains(prev) && !subjectAfterConjunction && !objectOfVerb ||
                noun.Equals("Deus", StringComparison.Ordinal) ||
                (poss.Equals("nossa", StringComparison.OrdinalIgnoreCase) && noun.Equals("senhora", StringComparison.OrdinalIgnoreCase)))
            {
                return m.Value;
            }

            var article = ArticleFor(poss);
            if (CaseHelper.IsAllUpper(poss))
            {
                article = article.ToUpperInvariant();
            }

            var lowerPrev = prev.ToLowerInvariant();
            string? head = null;
            if (lowerPrev == "de")
            {
                head = "d" + article;
            }
            else if (lowerPrev == "em")
            {
                head = "n" + article;
            }
            else if (lowerPrev == "por")
            {
                head = "pel" + article;
            }

            if (head != null)
            {
                return CaseHelper.MatchCase(prev, head) + m.Groups["between"].Value + poss + m.Groups["after"].Value;
            }

            if (SerForms.Contains(prev))
            {
                return char.IsLower(noun[0]) && noun.Length > 1 && !BareAfterSer.Contains(noun) && !NotNounsAfterPossessive.Contains(noun)
                    ? prev + m.Groups["between"].Value + article + " " + poss + m.Groups["after"].Value
                    : m.Value;
            }

            if (PlainPrepositions.Contains(prev) || Conjunctions.Contains(prev) || LooksLikeVerbForm(prev) || TuImperative.IsCommand(prev) ||
                (objectOfVerb && !NotNounsAfterPossessive.Contains(noun)) ||
                (!Copulas.Contains(prev) && (PortugueseVerbs.IsKnownVerbForm(prev) || PortugueseVerbs.ThirdToSecond.ContainsKey(lowerPrev) ||
                                         PortugueseVerbs.ThirdToSecond.Values.Contains(lowerPrev))))
            {
                return prev + m.Groups["between"].Value + article + " " + poss + m.Groups["after"].Value;
            }

            return m.Value;
        }

        private static bool IsVerbish(string word) => CommonVerbs.Contains(word) || PortugueseVerbs.IsKnownVerbForm(word);

        // "mandá-los fazerem" → "mandá-los fazer": after an object pronoun the causative takes the plain infinitive.
        private static readonly Regex CausativeInfinitive = new(
            NotWordBefore + @"(?<pre>(?:mand|deix|fa[zç]|fiz|vi|v[eê])\p{L}*-(?:l?[oa]s?|nos|vos)\s+)(?<inf>\p{Ll}+(?:a|e|i)r)em" + NotWordAfter, Ci);

        public static string Apply(string s)
        {
            s = BarePossessive.Replace(s, AddArticle);
            s = ExclaimedPossessive.Replace(s, AddExclaimedArticle);
            s = InnerPossessive.Replace(s, AddInnerArticle);
            s = DativePossessive.Replace(s, ReplaceDativePossessive);
            s = ThankPerson.Replace(s, ReplaceThankPerson);
            s = StartPossessive.Replace(s, AddStartArticle);
            s = IntroductoryPossessive.Replace(s, AddIntroductoryArticle);
            s = NosVemos.Replace(s, m => PortugueseVerbs.IsKnownVerbForm(m.Groups["v"].Value)
                ? m.Groups["pre"].Value + CaseHelper.Capitalize(PortugueseVerbs.AttachEnclitic(m.Groups["v"].Value, "nos"))
                : m.Value);
            s = JustCommand.Replace(s, ReplaceJustCommand);
            s = StartClitic.Replace(s, m => ReplaceLeading(m, capitalize: true));
            s = ReflexiveCommand.Replace(s, ReplaceReflexiveCommand);
            s = CommaClitic.Replace(s, m => ReplaceLeading(m, capitalize: false));
            s = SubjectClitic.Replace(s, ReplaceSubject);
            s = ModalClitic.Replace(s, ReplaceModal);
            s = LinkedInfinitiveClitic.Replace(s, ReplaceLinkedInfinitive);
            s = NounSubjectClitic.Replace(s, ReplaceNounSubject);
            s = AdverbClitic.Replace(s, m =>
            {
                var v = m.Groups["v"].Value;
                if (!LooksLikeVerb(v) || PortugueseVerbs.IsInfinitive(v))
                {
                    return m.Value;
                }

                var attached = PortugueseVerbs.AttachMesoclitic(v, m.Groups["cl"].Value.ToLowerInvariant(), requireTransitive: false)
                    ?? PortugueseVerbs.AttachEnclitic(v, m.Groups["cl"].Value.ToLowerInvariant());
                return m.Groups["pre"].Value + attached;
            });
            s = ObjectPronoun.Replace(s, ReplaceObject);
            s = CausativeInfinitive.Replace(s, m => m.Groups["pre"].Value + m.Groups["inf"].Value);
            s = DativeObjectPara.Replace(s, m =>
            {
                var converted = ReplaceDativePara(m);
                return converted == m.Value ? m.Value : converted + " " + m.Groups["np"].Value;
            });
            s = DativePara.Replace(s, ReplaceDativePara);
            s = InfinitiveProclisis.Replace(s, ReplaceInfinitiveProclisis);
            return s;
        }

        // "Agora me arrependo" / "Hoje me sinto bem" → "Agora arrependo-me" / "Hoje sinto-me bem": these adverbs do not attract the clitic.
        private static readonly Regex AdverbClitic = new(
            @"(?<pre>(?:^|[.!?…]\s+|\n|(?<![\p{L}\p{M}])[-–—]\s*)(?:agora|hoje|aqui|ali|lá|ontem|depois|amanhã)\s+)(?<cl>me|te|se|nos)\s+(?<v>\p{L}+)" + NotWordAfter + @"(?!-)", Ci);

        private static bool LooksLikeVerb(string word) =>
            !NotVerbs.Contains(word) && !PortugueseVerbs.IsFutureOrConditional(word) && word.Length > 1;

        private static string ReplaceLeading(Match m, bool capitalize)
        {
            var verb = m.Groups["v"].Value;
            var clitic = m.Groups["cl"].Value;
            var pronoun = CaseHelper.IsAllUpper(clitic) && CaseHelper.IsAllUpper(verb) ? clitic : clitic.ToLowerInvariant();
            // "nos" is also "in the": only a finite verb form that cannot be a plural noun counts ("Nos diga", not "Nos casos").
            if (pronoun.Equals("nos", StringComparison.OrdinalIgnoreCase) &&
                !(Regex.IsMatch(verb, @"(?:[ea]|em|am)$", Ci) && PortugueseVerbs.IsKnownVerbForm(verb)))
            {
                return m.Value;
            }

            var attached = PortugueseVerbs.AttachMesoclitic(verb, pronoun, requireTransitive: false);
            if (attached == null)
            {
                if (!LooksLikeVerb(verb))
                {
                    return m.Value;
                }

                attached = PortugueseVerbs.AttachEnclitic(verb, pronoun);
            }

            var pre = m.Groups["pre"].Value;
            if (capitalize && !Regex.IsMatch(pre, @"(?:E|Mas|Então|Ah|Oh|Ok|Bem|Agora|Por favor),?\s+$"))
            {
                attached = CaseHelper.Capitalize(attached);
            }

            return pre + attached;
        }

        private static string ReplaceSubject(Match m)
        {
            var verb = m.Groups["v"].Value;
            var clitic = m.Groups["cl"].Value;
            var mesoclitic = PortugueseVerbs.AttachMesoclitic(verb, CaseHelper.IsAllUpper(verb) ? clitic : clitic.ToLowerInvariant(), requireTransitive: false);
            if (mesoclitic != null)
            {
                return m.Groups["pre"].Value + m.Groups["subj"].Value + " " + mesoclitic;
            }

            var isDirectObject = clitic.ToLowerInvariant() is "o" or "a" or "os" or "as";
            if (!LooksLikeVerb(verb) || (isDirectObject && !PortugueseVerbs.IsKnownVerbForm(verb)))
            {
                return m.Value;
            }

            return m.Groups["pre"].Value + m.Groups["subj"].Value + " " +
                   PortugueseVerbs.AttachEnclitic(verb, CaseHelper.IsAllUpper(verb) ? clitic : clitic.ToLowerInvariant());
        }

        // "Um tipo me seguiu" → "Um tipo seguiu-me" (no word that attracts the pronoun before it).
        private static readonly Regex NounSubjectClitic = new(
            NotWordBefore + @"(?<w>\p{L}+)\s+(?<cl>me|te|lhe|lhes|nos)\s+(?<v>\p{L}+)" + NotWordAfter + @"(?!-)", Cs);

        private static readonly HashSet<string> ProclisisAttractors = new(StringComparer.OrdinalIgnoreCase)
        {
            "que", "não", "nunca", "jamais", "já", "também", "só", "sempre", "ainda", "quem", "onde", "aonde", "como", "quando",
            "porque", "porquê", "se", "nem", "talvez", "tudo", "todos", "todas", "ninguém", "alguém", "algo", "nada", "bem",
            "mal", "até", "para", "de", "a", "por", "sem", "qual", "quais", "quanto", "quanta", "quantos", "quantas", "ou",
            "apenas", "mesmo", "assim", "logo", "acaso", "embora", "caso", "enquanto", "eu", "tu", "ele", "ela", "nós", "eles",
            "elas", "você", "vocês", "o", "os", "cada", "ambos", "outro", "outros", "pouco", "muito", "muitos", "aqui", "lá",
            "ali", "cá", "aí", "agora", "depois", "antes", "ontem", "hoje", "amanhã", "pois", "lhe",
            "me", "te", "nos", "deus",
        };

        // "Foi bom te ver" / "Que bom te ver!" → "Foi bom ver-te": the infinitive after an adjective takes the pronoun after it.
        private static readonly HashSet<string> InfinitiveAdjectives = new(StringComparer.OrdinalIgnoreCase)
        {
            "bom", "boa", "ótimo", "óptimo", "maravilhoso", "fantástico", "incrível", "bonito", "lindo", "agradável", "fixe",
            "difícil", "fácil", "importante", "impossível", "possível", "melhor", "pior", "horrível", "terrível", "triste",
            "estranho", "esquisito", "necessário", "preciso", "giro", "engraçado", "ótima", "óptima", "maravilhosa", "excelente",
        };

        private static string ReplaceNounSubject(Match m)
        {
            var verb = m.Groups["v"].Value;
            if (InfinitiveAdjectives.Contains(m.Groups["w"].Value) && PortugueseVerbs.IsInfinitive(verb) && !PortugueseVerbs.IsFutureOrConditional(verb))
            {
                return m.Groups["w"].Value + " " + PortugueseVerbs.AttachEnclitic(verb, m.Groups["cl"].Value);
            }

            if (ProclisisAttractors.Contains(m.Groups["w"].Value) || PortugueseVerbs.IsInfinitive(verb) ||
                AfterSubordinateSubject(m.Result("$`") + m.Groups["w"].Value) ||
                PortugueseVerbs.IsFutureOrConditional(verb) || !(PortugueseVerbs.IsKnownVerbForm(verb) || PortugueseVerbs.ThirdToSecond.ContainsKey(verb.ToLowerInvariant())))
            {
                return m.Value;
            }

            return m.Groups["w"].Value + " " + PortugueseVerbs.AttachEnclitic(verb, m.Groups["cl"].Value);
        }

        private static string ReplaceModal(Match m)
        {
            var modal = m.Groups["m"].Value;
            var verb = m.Groups["v"].Value;
            if (!Modals.Contains(modal) || !PortugueseVerbs.IsInfinitive(verb))
            {
                return m.Value;
            }

            var clitic = m.Groups["cl"].Value;
            return modal + m.Groups["held"].Value + " " + PortugueseVerbs.AttachEnclitic(verb, CaseHelper.IsAllUpper(verb) ? clitic : clitic.ToLowerInvariant());
        }

        private static string ReplaceObject(Match m)
        {
            var verb = m.Groups["v"].Value;
            var periphrasticSee = verb.Equals("veremos", StringComparison.OrdinalIgnoreCase);
            // "Se afaste ele": a verb that already has a clitic before it does not take "ele" as a second object.
            if (!periphrasticSee && (Regex.IsMatch(m.Result("$'"), @"^\s+já" + NotWordAfter, Ci) ||
                m.Groups["trig"].Length == 0 && Regex.IsMatch(m.Result("$`"), @"(?<![\p{L}\p{M}])(?:se|me|te|lhe|lhes|vos)\s+$", Ci)))
            {
                return m.Value;
            }

            if (!periphrasticSee && (ObjectVerbStopWords.Contains(verb) || PortugueseVerbs.IsFutureOrConditional(verb) ||
                !PortugueseVerbs.IsTransitiveForm(verb) && !IsObjectInfinitive(m, verb) && !IsThinkOfAs(m, verb)))
            {
                return m.Value;
            }

            var p = m.Groups["p"].Value.ToLowerInvariant() switch
            {
                "ele" => "o",
                "ela" => "a",
                "eles" => "os",
                _ => "as",
            };

            if (CaseHelper.IsAllUpper(verb))
            {
                p = p.ToUpperInvariant();
            }

            var trig = m.Groups["trig"].Value;
            if (periphrasticSee)
            {
                return trig.Length > 0
                    ? trig + p + " " + CaseHelper.MatchCase(verb, "vamos ver")
                    : CaseHelper.MatchCase(verb, "vamos") + " " + PortugueseVerbs.AttachEnclitic(CaseHelper.IsAllUpper(verb) ? "VER" : "ver", p);
            }

            if (trig.Length == 0 && AfterSubordinateSubject(m.Result("$`")))
            {
                return p + " " + verb;
            }

            return trig.Length > 0 ? trig + p + " " + verb : PortugueseVerbs.AttachEnclitic(verb, p);
        }

        private const string ArEndings =
            "o|as|a|amos|am|e|es|em|emos|ei|aste|ou|ámos|aram|ava|avas|ávamos|avam|ar|ares|armos|arem|ará|arás|arei|aremos|arão|aria|arias|aríamos|ariam|asse|asses|assem|ando";

        private const string ErEndings =
            "o|es|e|emos|em|a|as|am|i|este|eu|eram|ia|ias|íamos|iam|er|eres|ermos|erem|erá|erás|erei|eremos|erão|eria|erias|eríamos|eriam|esse|essem|endo";

        // Verbs whose Brazilian "para + pronoun" is an indirect object in Portugal: "Liga para ela" → "Liga-lhe".
        internal const string DativeVerbForms =
            "(?:(?:lig|ligu|telefon|pergunt|cont|expli(?:c|qu)|mostr|mand|envi|ensin|pag|pagu|facilit)(?:" + ArEndings + ")|" +
            "(?:escrev|respond)(?:" + ErEndings + ")|" +
            "digo|dizes|diz|dizemos|dizem|diga|digas|digam|disse|disseste|dissemos|disseram|dizia|dizias|diziam|dizer|dizeres|dizerem|direi|dirá|dirás|diremos|dirão|diria|dirias|diriam|dissesse|dissessem|dizendo|" +
            "peço|pedes|pede|pedimos|pedem|peça|peças|peçam|pedi|pediste|pediu|pediram|pedia|pedias|pediam|pedir|pedires|pedirem|pedirei|pedirá|pedirás|pediremos|pedirão|pediria|pedirias|pediriam|pedisse|pedissem|pedindo|" +
            "minto|mentes|mente|mentimos|mentem|minta|mintas|mintam|menti|mentiste|mentiu|mentiram|mentia|mentias|mentiam|mentir|mentires|mentirem|mentirei|mentirá|mentirás|mentiremos|mentirão|mentiria|mentiriam|mentisse|mentindo)";

        private static readonly Regex DativePara = new(
            @"(?<trig>" + NotWordBefore + @"(?:não|nunca|jamais|também|já|ainda|só|nem|sempre|que|quem|porque|onde|quando|como|talvez)\s+)?" +
            NotWordBefore + @"(?<v>" + DativeVerbForms + @")\s+para\s+(?<p>ele|ela|eles|elas|mim|ti|nós|você|vocês)" + NotWordAfter +
            @"(?!\s+(?:mesm[oa]s?|própri[oa]s?|só|sozinh[oa]s?|e\s+(?:para|a|ao|à|o)|ou\s+(?:para|a|ao|à|o))" + NotWordAfter + ")", Ci);

        // "Vou mostrar a casa para ele" → "Vou mostrar-lhe a casa": a direct object between a giving/telling verb and "para + pronoun".
        private const string DitransitiveVerbForms =
            "(?:(?:mostr|mand|envi|ensin|expli(?:c|qu)|cont|entreg|emprest)(?:" + ArEndings + ")|" +
            "(?:escrev|vend|oferec|devolv)(?:" + ErEndings + ")|" +
            "dou|dás|dá|damos|dão|dê|dês|dêem|deu|deste|demos|deram|dava|davas|davam|dar|darei|dará|daria|dando|" +
            "digo|diz|dizem|disse|disseram|dizia|diga|dizer|direi|dirá|diria|dizendo)";

        private static readonly Regex DativeObjectPara = new(
            @"(?<trig>" + NotWordBefore + @"(?:não|nunca|jamais|também|já|ainda|só|nem|sempre|que|quem|porque|onde|quando|como|talvez)\s+)?" +
            NotWordBefore + @"(?<v>" + DitransitiveVerbForms + @")\s+" +
            @"(?<np>(?:o|a|os|as|um|uma|uns|umas|meu|minha|meus|minhas|seu|sua|seus|suas|teu|tua|teus|tuas|nosso|nossa|este|esta|esse|essa|aquele|aquela)" +
            @"(?:\s+(?!(?:para|a|ao|à|de|e|ou|que|com|em|no|na)" + NotWordAfter + @")\p{L}+){1,2}(?:\s+d[eoa]s?\s+\p{L}+)?)" +
            @"\s+para\s+(?<p>ele|ela|eles|elas|mim|ti|nós|você|vocês)" + NotWordAfter +
            @"(?=\s*(?:[,.;:!?…]|$)|\s+(?:hoje|ontem|amanhã|agora|depois|também|já)" + NotWordAfter + ")", Ci);

        // "Pedi para ele sair" → "Pedi-lhe para sair": with these the pronoun is the one asked/told/paid to do something.
        private static readonly Regex RequestVerb = new(@"^(?:pe[dç]|dig|diz|diss|dir|pag)", RegexOptions.IgnoreCase);

        private static readonly Regex NounBefore = new(
            @"(?<![\p{L}\p{M}])(?:um|uma|o|a|os|as|uns|umas|essa|esse|esta|este|essas|estas|outra|outro|outras|minha|meu|tua|teu|sua|seu|nossa|nosso|alguma|algum|algumas|alguns|nenhuma|última|primeira|mais|cuja|cujo|duas|três|várias|muitas|poucas|" +
            @"tenho|tens|tem|temos|têm|tiver|tiveres|tiverem|tinha|tinhas|tinham|há|havia|houver)\s+$",
            RegexOptions.IgnoreCase);

        private static readonly Regex InfinitiveProclisis = new(
            @"(?<pre>(?<![\p{L}\p{M}])que\s+)" +
            @"(?:(?<subject>eu|tu|ele|ela|nós|vocês|eles|elas)(?<subjectSpace>\s+))?" +
            @"(?<aux>vou|vais|vai|vamos|vão|ia|ias|íamos|iam|iria|irias|iríamos|iriam|irei|irás|irá|iremos|irão|estou|estás|está|estamos|estão|estava|estavas|estávamos|estavam|" +
            @"posso|podes|pode|podemos|podem|podia|podias|podíamos|podiam|quero|queres|quer|queremos|querem|queria|querias|queríamos|queriam|" +
            @"devo|deves|deve|devemos|devem|devia|devias|devíamos|deviam)(?<auxSpace>\s+)(?<verb>\p{L}+)-(?<cl>me|te|se|nos|vos|lhe|lhes)(?![\p{L}\p{M}-])", Ci);

        private static string ReplaceInfinitiveProclisis(Match m) =>
            m.Groups["pre"].Value + m.Groups["subject"].Value + m.Groups["subjectSpace"].Value +
            m.Groups["cl"].Value + " " + m.Groups["aux"].Value + m.Groups["auxSpace"].Value + m.Groups["verb"].Value;

        private static string ReplaceDativePara(Match m)
        {
            var verb = m.Groups["v"].Value;
            var trig = m.Groups["trig"].Value;
            var before = m.Result("$`") + trig;
            var after = m.Result("$'");
            var infinitive = PortugueseVerbs.IsInfinitive(verb);
            // "Está a mentir para mim": "a" before an infinitive is the preposition, not an article.
            if (trig.Length == 0 && ((NounBefore.IsMatch(before) && !(infinitive && Regex.IsMatch(before, @"(?<![\p{L}\p{M}])(?:a|o)\s+$", RegexOptions.IgnoreCase))) ||
                Regex.IsMatch(before, infinitive ? @"(?<![\p{L}\p{M}])(?:me|te|se|lhe|lhes|nos|vos)\s+$" : @"(?<![\p{L}\p{M}])(?:me|te|se|lhe|lhes|nos|vos|o|a|os|as)\s+$", RegexOptions.IgnoreCase)))
            {
                return m.Value;
            }

            // "Isso não conta para ele" means "it does not count for him".
            if (Regex.IsMatch(verb, @"^cont(?:a|am)$", RegexOptions.IgnoreCase) &&
                Regex.IsMatch(before, @"(?<![\p{L}\p{M}])(?:isso|isto|aquilo|tudo|nada|que)\s+(?:(?:não|só|também|já|ainda)\s+)?$", RegexOptions.IgnoreCase))
            {
                return m.Value;
            }

            // "Liguei para ele saber" (so that he knows) is not "liguei-lhe"; only request verbs take an infinitive after the pronoun.
            var next = Regex.Match(after, @"^\s+(?:(?:me|te|se|nos|vos|lhe|lhes|o|os|as)\s+)?(?<w>\p{L}+)", RegexOptions.IgnoreCase).Groups["w"].Value;
            var infinitiveNext = next.Length > 0 &&
                (PortugueseVerbs.IsInfinitive(next) || Regex.IsMatch(next, @"(?:arem|erem|irem)$", RegexOptions.IgnoreCase));
            if (infinitiveNext && !RequestVerb.IsMatch(verb))
            {
                return m.Value;
            }

            var para = infinitiveNext ? (CaseHelper.IsAllUpper(verb) && verb.Length > 1 ? " PARA" : " para") : string.Empty;

            var clitic = m.Groups["p"].Value.ToLowerInvariant() switch
            {
                "ele" or "ela" or "você" => "lhe",
                "eles" or "elas" => "lhes",
                "mim" => "me",
                "ti" => "te",
                "nós" => "nos",
                _ => "vos",
            };
            if (CaseHelper.IsAllUpper(verb) && verb.Length > 1)
            {
                clitic = clitic.ToUpperInvariant();
            }

            if (trig.Length > 0 || (!infinitive && AfterSubordinateSubject(m.Result("$`"))))
            {
                return trig + clitic + " " + verb + para;
            }

            if (!infinitive && PortugueseVerbs.IsFutureOrConditional(verb))
            {
                return (PortugueseVerbs.AttachMesoclitic(verb, clitic, requireTransitive: false) ?? clitic + " " + verb) + para;
            }

            return PortugueseVerbs.AttachEnclitic(verb, clitic) + para;
        }

        // "que a minha mãe", "se o Jack", "quando a polícia": the subordinating word still attracts the pronoun
        // across a short subject ("que a minha mãe o traiu", not "traiu-o").
        private static readonly Regex SubordinateSubject = new(
            @"(?<![\p{L}\p{M}])(?:que|se|quando|porque|onde|aonde|embora|enquanto|caso|até\s+que|antes\s+que|depois\s+que)\s+" +
            @"(?<det>(?:o|a|os|as|um|uma|este|esta|esse|essa|aquele|aquela)\s+)?(?<pos>(?:meu|minha|meus|minhas|teu|tua|teus|tuas|seu|sua|seus|suas|nosso|nossa|nossos|nossas)\s+)?" +
            @"(?<n>[\p{L}\p{M}-]+)\s*$",
            RegexOptions.IgnoreCase);

        internal static bool AfterSubordinateSubject(string before)
        {
            var m = SubordinateSubject.Match(before);
            if (!m.Success)
            {
                return false;
            }

            var n = m.Groups["n"].Value;
            if (!m.Groups["det"].Success && !m.Groups["pos"].Success && n.ToLowerInvariant() is "eu" or "tu" or "ele" or "ela" or "nós" or "eles" or "elas" or "isso" or "isto" or "aquilo")
            {
                return true;
            }

            // Without an article the subject must be a name ("que Jack"); in "que conheci me ligou" the word is a verb.
            return m.Groups["det"].Success || m.Groups["pos"].Success
                ? !PortugueseVerbs.IsKnownVerbForm(n)
                : char.IsUpper(n[0]) && !PortugueseVerbs.IsKnownVerbForm(n.ToLowerInvariant());
        }
    }

    // ================================================================== spelling patterns

    // A sentence already addressed with "tu" (teu, tua, te, -te, contigo) gets the "tu" imperative too:
    // "Aproveite o teu indulto" → "Aproveita o teu indulto", "Pegue nas tuas coisas e saia" → "Pega nas tuas coisas e sai".
    internal static class TuImperative
    {
        // A sentence may wrap onto the next line; a new line starting with a dash is another speaker.
        private static readonly Regex Sentence = new(@"(?:[^.!?…\n]|\n(?![-–—]))+[.!?…]*", Cs);

        private static readonly Regex TuMarker = new(
            NotWordBefore + @"(?:teu|tua|teus|tuas|te|ti|contigo)" + NotWordAfter + @"|-te" + NotWordAfter, Ci);

        // Sentence-initial verb (after a dash, "e/mas/então" or a name + comma), and verbs joined with "e".
        private const string CommandStart = @"^[\s""“«'\uE000-\uF8FF]*(?:(?<![\p{L}\p{M}])[-–—]\s*)?(?:(?:e|mas|então|agora|por\s+favor|ok|bem|está\s+bem|pronto|olha|anda)\s*,?\s+)?";
        private const string CommandVerb = @"(?<v>\p{L}+)(?<cl>-(?:me|nos|lhe|lhes|o|a|os|as|lo|la|los|las|no|na|se|te))?" + NotWordAfter;

        private static readonly Regex Command = new(@"(?<pre>" + CommandStart + @"|\s+e\s+|,\s+)" + CommandVerb, Ci);

        private static readonly Dictionary<string, string> Irregular = new(StringComparer.OrdinalIgnoreCase)
        {
            ["dê"] = "dá", ["diga"] = "diz", ["faça"] = "faz", ["traga"] = "traz", ["ponha"] = "põe", ["venha"] = "vem", ["seja"] = "sê",
            ["vá"] = "vai", ["tenha"] = "tem", ["veja"] = "vê", ["saia"] = "sai", ["ouça"] = "ouve", ["peça"] = "pede", ["esteja"] = "está",
            ["siga"] = "segue", ["durma"] = "dorme", ["sirva"] = "serve", ["sinta"] = "sente", ["minta"] = "mente", ["suba"] = "sobe",
            ["fuja"] = "foge", ["leia"] = "lê", ["caia"] = "cai", ["perca"] = "perde", ["mantenha"] = "mantém", ["detenha"] = "detém",
            ["obtenha"] = "obtém", ["contenha"] = "contém", ["desfaça"] = "desfaz", ["refaça"] = "refaz", ["vista"] = "veste", ["despeça"] = "despede",
            ["consiga"] = "consegue", ["prossiga"] = "prossegue", ["descubra"] = "descobre", ["cubra"] = "cobre", ["tussa"] = "tosse", ["pare"] = "para", ["fale"] = "fala",
        };

        // Imperative forms that are more often other words at the start of a sentence.
        private static readonly HashSet<string> NotCommands = new(StringComparer.OrdinalIgnoreCase)
        {
            "sente", "parte", "parece", "acontece", "pode", "deve", "existe", "importa", "basta", "falta", "consta", "sobre", "entre",
            "contra", "cada", "nada", "toda", "tudo", "ela", "essa", "esta", "aquela", "uma", "nossa", "minha", "sua", "tua", "boa", "ainda", "agora",
            "este", "esse", "aquele", "estes", "esses", "aqueles", "sabe", "come", "bebe", "corre", "vive", "esconde", "fixe", "ataque", "livre", "leve", "doce", "triste",
            "golfe", "teste", "telefone", "monte", "enfarte", "fosse", "perda", "gripe", "torre", "cave", "detalhe", "presente", "embarque",
            "limite", "acidente", "descarte", "conforme", "tarde", "viva", "choque", "calibre", "alce", "firme", "disfarce", "alegre", "sede", "trouxa", "vista", "xarope", "tina",
            "esteja", "corte", "toque", "transporte", "suporte", "desfile", "debate", "combate", "resgate",
        };

        // "Seja como for" / "Venha o que vier" are not commands.
        private static readonly Regex NotCommandPhrase = new(
            CommandStart + @"(?:seja|venha|aconteça|custe|doa|queira)\s+(?:como|o\s+que|quem|qual|quando|onde|lá|o\s+que\s+for)" + NotWordAfter, Ci);

        // "Só me ligue." / "Só lhe peça para sair." → "Só me liga." / "Só lhe pede para sair.".
        private static readonly Regex OnlyCommand = new(
            @"(?<pre>" + CommandStart + @"(?:só|apenas)\s+)(?:(?<cl>me|se|nos|o|a|os|as|lhe|lhes)\s+)?(?<v>\p{L}+)" + NotWordAfter + @"(?!-)", Ci);

        // "Não fale com ela" / "Não se preocupe" → "Não fales com ela" / "Não te preocupes".
        private static readonly Regex NegativeCommand = new(
            @"(?<pre>" + CommandStart + @")(?<no>não)\s+(?:(?<cl>me|se|nos|o|a|os|as|lhe|lhes)\s+)?(?<v>\p{L}+)" + NotWordAfter + @"(?!-)", Ci);

        /// <summary>
        /// Brazilian commands use the "você" form ("Coloque", "Escute"); Portugal mostly says them in the "tu" form, the same as
        /// every "você" the converter turns into "tu". Formal lines (and lines after a formal one) keep the "você" form.
        /// </summary>
        public static string Apply(string s, string? previousText = null, bool formalContext = false, bool forceInformal = false) =>
            Sentence.Replace(s, sentence =>
            {
                var text = sentence.Value;
                if (formalContext)
                {
                    return text;
                }

                if (text.TrimEnd().EndsWith('?'))
                {
                    return text;
                }

                var marked = TuMarker.IsMatch(text);
                if (!marked)
                {
                    if (!forceInformal && (YouToTuRules.IsFormal(text) || previousText != null && YouToTuRules.IsFormal(previousText) ||
                        FormalMarker.IsMatch(text) || PoliteRequest.IsMatch(text) || TitleMarker.IsMatch(s)))
                    {
                        return text;
                    }
                }

                var negative = NegativeCommand.Match(text);
                if (negative.Success)
                {
                    if (!forceInformal && (YouToTuRules.IsFormal(text) || previousText != null && YouToTuRules.IsFormal(previousText) ||
                        FormalMarker.IsMatch(text) || PoliteRequest.IsMatch(text) || TitleMarker.IsMatch(s)))
                    {
                        return text;
                    }

                    return ConvertNegative(text, negative);
                }

                if (!marked && NotCommandPhrase.IsMatch(text))
                {
                    return text;
                }

                var only = OnlyCommand.Match(text);
                if (only.Success && char.IsUpper(only.Groups["pre"].Value.TrimStart(' ', '-', '–', '—', '"', '“', '«', '\'')[0]) &&
                    ToTu(only.Groups["v"].Value) is { } onlyTu && !NotCommands.Contains(only.Groups["v"].Value))
                {
                    var cl = only.Groups["cl"];
                    var clitic = cl.Success ? (cl.Value.Equals("se", StringComparison.OrdinalIgnoreCase) ? CaseHelper.MatchCase(cl.Value, "te") : cl.Value) + " " : string.Empty;
                    return only.Groups["pre"].Value + clitic + CaseHelper.MatchCase(only.Groups["v"].Value, onlyTu) + text[(only.Index + only.Length)..];
                }

                var converted = false;
                return Command.Replace(text, m =>
                {
                    // "e" and "," only continue a command or follow an opening word: "Pega nas tuas coisas e sai", "Desculpa, vê o teu...",
                    // "Joe, pega no teu casaco".
                    var pre = m.Groups["pre"].Value.TrimStart();
                    var continuation = pre.StartsWith("e ", StringComparison.OrdinalIgnoreCase) || pre.StartsWith(',');
                    if (continuation && !converted && !(pre.StartsWith(',') && IsOpening(text[..m.Index])) ||
                        !continuation && !char.IsUpper(m.Groups["v"].Value[0]) && !Regex.IsMatch(pre, @"\p{L}"))
                    {
                        return m.Value;
                    }

                    if (m.Groups["cl"].Value.Equals("-te", StringComparison.OrdinalIgnoreCase))
                    {
                        converted = true; // "Cala-te e continua a trabalhar" - already a "tu" command
                        return m.Value;
                    }

                    if (Regex.IsMatch(s[..(sentence.Index + m.Groups["v"].Index)], @"(?<![\p{L}\p{M}])(?:Sr|Sra|Srª|Dr|Dra|Drª|Mr|Mrs|Ms|Mna)\.\s*$"))
                    {
                        return m.Value; // "Sra. Chore", "Sr. Cole"
                    }

                    var tu = ToTu(m.Groups["v"].Value);
                    // "Anda, vá!" - "vá" is also an interjection in Portugal.
                    if (tu == null || !marked && (m.Groups["v"].Value.Equals("vá", StringComparison.OrdinalIgnoreCase) ||
                        m.Groups["cl"].Length == 0 && (NotCommands.Contains(m.Groups["v"].Value) || MaybeName(m.Groups["v"].Value, text[(m.Index + m.Length)..]))))
                    {
                        return m.Value;
                    }

                    converted = true;
                    var cl = m.Groups["cl"].Value;
                    if (cl.Equals("-se", StringComparison.OrdinalIgnoreCase))
                    {
                        cl = CaseHelper.IsAllUpper(cl) ? "-TE" : "-te"; // "Sente-se e come o teu jantar" → "Senta-te"
                    }

                    return m.Groups["pre"].Value + CaseHelper.MatchCase(m.Groups["v"].Value, tu) + cl;
                });
            });

        // "Por favor, mate-me apenas" - a polite request keeps the "você" form.
        private static readonly Regex PoliteRequest = new(@"^[\s""“«'\uE000-\uF8FF]*(?:(?<![\p{L}\p{M}])[-–—]\s*)?por\s+favor" + NotWordAfter, Ci);

        // A sentence that still talks about "si", "consigo", "o senhor" or "lhe" is formal.
        // "Sr. Cole, desculpe entrar" - a line addressing someone by title is formal.
        private static readonly Regex TitleMarker = new(@"(?<![\p{L}\p{M}])(?:Sr|Sra|Srª|Srta|Mna|Dr|Dra|Drª)\.", RegexOptions.CultureInvariant);

        private static readonly Regex FormalMarker = new(
            NotWordBefore + @"(?:si|consigo|o\s+senhor|a\s+senhora|vocês|você)" + NotWordAfter, Ci);

        private static string ConvertNegative(string text, Match m)
        {
            var verb = m.Groups["v"].Value;
            if (!char.IsUpper(m.Groups["no"].Value[0]) || ToTu(verb) == null || verb.EndsWith('s'))
            {
                return text;
            }

            var cl = m.Groups["cl"];
            var clitic = cl.Success ? (cl.Value.Equals("se", StringComparison.OrdinalIgnoreCase) ? CaseHelper.MatchCase(cl.Value, "te") : cl.Value) + " " : string.Empty;
            var result = m.Groups["pre"].Value + m.Groups["no"].Value + " " + clitic + verb + (CaseHelper.IsAllUpper(verb) ? "S" : "s");
            return result + text[(m.Index + m.Length)..];
        }

        // "Tina. Ebraheim." / "Gabe!" - a short lone capitalised word or one before another capitalised word may be a name; only
        // verbs from the built-in list are trusted there.
        private static bool MaybeName(string verb, string after)
        {
            if (!Regex.IsMatch(after, @"^\s*(?:[.!…,;:]|$|(?-i:\p{Lu}))")) return false;
            if (Irregular.ContainsKey(verb) || verb.Length > 5) return false;
            var w = verb.ToLowerInvariant();
            var stem = w[..^1];
            return w.EndsWith('e')
                ? !PortugueseVerbs.IsKnownInfinitive((stem.EndsWith("qu") ? stem[..^2] + "c" : stem.EndsWith("gu") ? stem[..^1] : stem.EndsWith('c') ? stem[..^1] + "ç" : stem) + "ar")
                : !PortugueseVerbs.IsKnownInfinitive((stem.EndsWith('ç') ? stem[..^1] + "c" : stem.EndsWith('j') ? stem[..^1] + "g" : stem) + "er") &&
                  !PortugueseVerbs.IsKnownInfinitive((stem.EndsWith('ç') ? stem[..^1] + "c" : stem.EndsWith('j') ? stem[..^1] + "g" : stem) + "ir");
        }

        public static bool IsCommand(string word) => char.IsUpper(word[0]) && ToTu(word) != null;

        // Text before the comma is just a name or a converted command: "Joe", "- Desculpa".
        private static bool IsOpening(string before) =>
            Regex.IsMatch(before, @"^[\s""“«'\uE000-\uF8FF]*(?:(?<![\p{L}\p{M}])[-–—]\s*)?\p{Lu}\p{Ll}+$", Cs);

        internal static string? ToTu(string verb)
        {
            if (Irregular.TryGetValue(verb, out var irregular))
            {
                return irregular;
            }

            if (NotCommands.Contains(verb) || verb.Length < 4)
            {
                return null;
            }

            var w = verb.ToLowerInvariant();
            if (w.EndsWith('e'))
            {
                var stem = w[..^1];
                var arStem = stem.EndsWith("qu") ? stem[..^2] + "c" : stem.EndsWith("gu") ? stem[..^1] : stem.EndsWith('c') ? stem[..^1] + "ç" : stem;
                // "Sente" is also sentir and "Corre" is already the "tu" form of correr.
                return IsVerb(arStem + "ar", arStem + "a", w) && !IsVerb(stem + "er", w, w) && !IsVerb(stem + "ir", w, w) ? arStem + "a" : null;
            }

            if (w.EndsWith('a'))
            {
                var stem = w[..^1];
                var erStem = stem.EndsWith('ç') ? stem[..^1] + "c" : stem.EndsWith('j') ? stem[..^1] + "g" : stem;
                // "Senta" / "Para" are already the "tu" form of sentar / parar.
                return (IsVerb(erStem + "er", erStem + "e", w) || IsVerb(erStem + "ir", erStem + "e", w)) && !IsVerb(stem + "ar", w, w) ? erStem + "e" : null;
            }

            return null;
        }

        // Built-in verb lists first, then the Natura dictionary (aproveitar, aproveita and aproveite must all be words).
        internal static bool IsInfinitive(string infinitive) =>
            PortugueseVerbs.IsKnownInfinitive(infinitive) || NaturaDictionary.Load(true).Check(infinitive);

        private static bool IsVerb(string infinitive, string tu, string you)
        {
            if (PortugueseVerbs.IsKnownInfinitive(infinitive))
            {
                return true;
            }

            var natura = NaturaDictionary.Load(true);
            return natura.Check(infinitive) && natura.Check(tu) && natura.Check(you);
        }
    }

    /// <summary>
    /// "Ele sai da cadeia e já viola a ordem" → "Ele saiu da prisão e já está a violar a ordem": Portugal describes an
    /// action already under way after "e já" with "estar a" + infinitive, and the event that set it off in the past.
    /// </summary>
    /// <summary>
    /// Brazilian drops the pronoun of "lembrar-se de" / "esquecer-se de": "Lembra de mim?" → "Lembras-te de mim?",
    /// "Eu não lembro" → "Eu não me lembro", "Ele esqueceu da reunião" → "Ele esqueceu-se da reunião".
    /// The pronoun follows the verb's person and goes before it after "não", "nunca", "que"... (proclisis).
    /// "Lembra o pai" (resembles) and "Esqueci o nome" (direct object) are kept.
    /// </summary>
    internal static class PronominalVerbs
    {
        private enum Person { First, Second, Third, FirstPlural, Ambiguous }

        private static readonly Dictionary<string, Person> Forms = BuildForms();

        private static Dictionary<string, Person> BuildForms()
        {
            var map = new Dictionary<string, Person>(StringComparer.OrdinalIgnoreCase);
            void Add(Person p, params string[] forms)
            {
                foreach (var f in forms) map[f] = p;
            }

            foreach (var infinitive in new[] { "lembrar", "esquecer" })
            {
                if (!VerbConjugator.TryConjugate(infinitive, out var paradigm))
                    throw new InvalidOperationException($"Cannot conjugate {infinitive}.");
                for (var i = 0; i < paradigm.Length; i++)
                {
                    if (i is 35 or 36 or 37 or 38) continue;
                    map[paradigm[i]] = i switch
                    {
                        1 or 6 or 14 => Person.First,
                        2 or 7 or 11 or 15 or 20 or 24 or 28 or 31 or 40 => Person.Second,
                        4 or 12 or 17 or 21 or 25 or 29 or 32 or 41 or 42 => Person.FirstPlural,
                        0 or 10 or 19 or 23 or 27 or 34 or 39 => Person.Ambiguous,
                        _ => Person.Third,
                    };
                }
            }

            Add(Person.First, "lembro", "lembrei", "esqueço", "esqueci");
            Add(Person.Second, "lembras", "lembraste", "lembravas", "lembres", "esqueces", "esqueceste", "esquecias", "esqueças");
            Add(Person.Third, "lembra", "lembrou", "lembram", "lembraram", "lembravam", "lembrem",
                "esquece", "esqueceu", "esquecem", "esqueceram", "esqueciam", "esqueçam");
            Add(Person.FirstPlural, "lembramos", "lembrámos", "lembrávamos", "lembremos", "esquecemos", "esquecíamos", "esqueçamos");
            Add(Person.Ambiguous, "lembrava", "lembre", "esquecia", "esqueça");
            return map;
        }

        private static readonly HashSet<string> Preterite = new(StringComparer.OrdinalIgnoreCase)
        {
            "lembrei", "lembraste", "lembrou", "lembramos", "lembrámos", "lembraram",
            "esqueci", "esqueceste", "esqueceu", "esquecemos", "esqueceram",
        };

        private static readonly HashSet<string> Triggers = new(StringComparer.OrdinalIgnoreCase)
        {
            "não", "nunca", "jamais", "já", "também", "ainda", "sempre", "só", "nem", "apenas", "talvez", "que", "quem", "ninguém",
            "alguém", "tudo", "nada", "bem", "quando", "onde", "como", "porque", "enquanto", "embora", "caso", "se", "quanto", "mal",
            "todos", "todas", "ambos", "ambas",
        };

        private static readonly HashSet<string> Clitics = new(StringComparer.OrdinalIgnoreCase)
        {
            "me", "te", "se", "nos", "vos", "lhe", "lhes", "o", "a", "os", "as", "lo", "la", "no", "na",
        };

        internal static readonly HashSet<string> Titles = new(StringComparer.OrdinalIgnoreCase)
        {
            "senhor", "senhora", "doutor", "doutora", "detetive", "agente", "oficial", "inspetor", "inspetora", "capitão", "tenente",
            "sargento", "cabo", "coronel", "general", "comandante", "major", "juiz", "juíza", "meritíssimo", "meritíssima", "padre",
            "comissário", "comissária", "presidente", "ministro", "ministra", "diretor", "diretora", "professor", "professora",
            "chefe", "procurador", "procuradora", "vereador", "vereadora", "governador", "governadora", "senador", "senadora", "dona",
        };

        // A verb form with the following "de/da/disso..." or the end of the clause; an optional Brazilian "Se" in front at the start.
        private static readonly Regex Verb = new(
            NotWordBefore + @"(?<se>se\s+)?(?<v>" + string.Join("|", BuildForms().Keys.OrderByDescending(k => k.Length)) + @")" + NotWordAfter + @"(?!-)" +
            @"(?=(?<de>\s+(?:de|do|da|dos|das|disso|disto|daquilo|daquele|daquela|daqueles|daquelas|dele|dela|deles|delas|desse|dessa|" +
            @"desses|dessas|deste|desta|destes|destas|dum|duma|dumas|duns|ti|mim|nós)" + NotWordAfter + @")|(?<end>\s*(?:[.!?…,]|$))|(?<relative>\s+(?:é|era|são|eram)\b))",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Multiline);

        private static readonly Regex ClauseTail = new(@"(?:[^.!?…;\n]|\n(?![-–—]))*$", RegexOptions.CultureInvariant);

        private static readonly Regex SegmentStart = new(
            @"^\s*(?:(?<![\p{L}\p{M}])[-–—]\s*)?[""“«']?(?:(?:e|mas|então|ah|oh|olha|ei|bem|agora)\s+)?(?<neg>(?:não|nunca|já|ainda|também)\s+)?$",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        private static readonly Regex Question = new(@"^(?:[^.!?…\n]|\n(?![-–—]))*\?", RegexOptions.CultureInvariant);

        private static List<string> Words(string text) =>
            Regex.Matches(text, @"[\p{L}\p{M}]+(?:-[\p{L}\p{M}]+)*").Select(w => w.Value).ToList();

        public static string Apply(string s, bool tu)
        {
            if (!Regex.IsMatch(s, @"lembr|esque[cç]", RegexOptions.IgnoreCase)) return s;
            var formal = !tu || YouToTuRules.IsFormal(s);
            return Verb.Replace(s, m => Replace(s, m, formal));
        }

        private static string Replace(string s, Match m, bool formal)
        {
            var verb = m.Groups["v"].Value;
            var lower = verb.ToLowerInvariant();
            var esquecer = lower.StartsWith("esque", StringComparison.Ordinal);
            // "ti/mim/nós" only after "de" ("lembra de mim"), never directly ("esqueceu mim").
            if (m.Groups["de"].Success && Regex.IsMatch(m.Groups["de"].Value, @"^\s+(?:ti|mim|nós)$", RegexOptions.IgnoreCase)) return m.Value;

            var before = s[..m.Index];
            // A clause ends at . ! ? ; or a new speaker's dash; a plain line break continues it ("nem me\nlembro").
            var clause = ClauseTail.Match(before).Value;
            var words = Words(clause);
            var previous = words.Count > 0 ? words[^1] : null;
            if (previous != null && Clitics.Contains(previous)) return m.Value;

            // The part after the last comma: "Certo, Tiago, lembra..." / "Estávamos no hospital, lembra?".
            var comma = clause.LastIndexOf(',');
            var segment = comma < 0 ? clause : clause[(comma + 1)..];
            var segmentWords = Words(segment);
            var at = SegmentStart.Match(segment);
            var start = at.Success;
            var bareStart = start && !at.Groups["neg"].Success;
            if (comma >= 0 && Words(clause[..comma]) is { Count: > 0 } head && Titles.Contains(head[^1])) formal = true;
            if (comma < 0 && Titles.Contains(previous ?? string.Empty)) formal = true;
            // "não se lembra" already has it; only a Brazilian "Se lembra..." at the start is moved.
            if (m.Groups["se"].Success && !bareStart) return m.Value;
            if (m.Groups["relative"].Success && (esquecer || !Regex.IsMatch(clause,
                @"\b(?:tudo\s+o\s+que|(?:a\s+)?(?:única\s+)?coisa\s+que)\s*$", Ci))) return m.Value;

            if (m.Groups["end"].Success)
            {
                // At the end of the clause the object may be before it ("o que esqueci", "tudo o que lembro").
                var relativeRecall = !esquecer && Regex.IsMatch(clause,
                    @"\b(?:tudo\s+o\s+que|(?:a\s+)?(?:única\s+)?coisa\s+que)\s*$", Ci);
                if (!relativeRecall && words.Any(w => w.Equals("que", StringComparison.OrdinalIgnoreCase) || w.Equals("quê", StringComparison.OrdinalIgnoreCase) ||
                                   w.Equals("qual", StringComparison.OrdinalIgnoreCase))) return m.Value;
                // "Esquece!" / "Esquece isso" (forget it) is fine in Portugal; only past and negative forms take the pronoun at the end.
                if (esquecer && !Preterite.Contains(lower) && !(previous?.Equals("não", StringComparison.OrdinalIgnoreCase) ?? false)) return m.Value;
            }

            var person = Forms[lower];
            string? form = null;
            if (start && lower is "lembra" or "esquece" or "lembrou" or "esqueceu")
            {
                // No subject: the line speaks to someone. Questions ask "do you...?"; "Lembra de trancar a porta" is a command.
                var question = Question.IsMatch(s[m.Index..]);
                if (!formal && question)
                {
                    form = lower switch { "lembra" => "lembras", "esquece" => "esqueces", "lembrou" => "lembraste", _ => "esqueceste" };
                    person = Person.Second;
                }
                else if (!formal && bareStart && m.Groups["de"].Success && lower is "lembra" or "esquece")
                {
                    person = Person.Second;
                }
            }
            else if (person == Person.Ambiguous)
            {
                var subject = words.LastOrDefault(w => w.ToLowerInvariant() is "eu" or "tu" or "você" or "nós" or "ele" or "ela" or "eles" or "elas");
                person = subject?.ToLowerInvariant() switch
                {
                    "eu" => Person.First, "tu" => Person.Second, "você" when !formal => Person.Second, "nós" => Person.FirstPlural,
                    null when previous?.ToLowerInvariant() is "vou" or "posso" or "quero" => Person.First,
                    null when previous?.ToLowerInvariant() is "vamos" or "podemos" or "queremos" => Person.FirstPlural,
                    null when !formal && previous?.ToLowerInvariant() is "vais" or "podes" or "queres" => Person.Second,
                    _ => Person.Third,
                };
            }

            var clitic = person switch { Person.First => "me", Person.Second => "te", Person.FirstPlural => "nos", _ => "se" };
            form = form == null ? verb : CaseHelper.MatchCase(verb, form);
            if (CaseHelper.IsAllUpper(verb) && verb.Length > 1) clitic = clitic.ToUpperInvariant();

            if (m.Groups["se"].Success)
            {
                form = char.IsUpper(m.Groups["se"].Value[0]) ? CaseHelper.Capitalize(form) : form;
            }

            if (segmentWords.Count > 0 && Triggers.Contains(segmentWords[^1]))
            {
                return clitic + " " + form;
            }

            return PortugueseVerbs.AttachMesoclitic(form, clitic, false) ?? PortugueseVerbs.AttachEnclitic(form, clitic);
        }
    }

    internal static class ImmediateProgressive
    {
        private static readonly Regex AlreadyDoing = new(
            NotWordBefore + @"(?<pre>(?:e|mas)\s+já\s+)(?<v>\p{Ll}+)" + NotWordAfter + @"(?!-)(?!\s+(?:que|se)" + NotWordAfter + ")", Ci);

        // States, modals and verbs where "já" means "right away" ("ele já vem", "já volta").
        private static readonly HashSet<string> NotProgressive = new(StringComparer.OrdinalIgnoreCase)
        {
            "é", "são", "está", "estão", "tem", "têm", "há", "vai", "vão", "vem", "vêm", "sabe", "sabem", "quer", "querem", "pode", "podem",
            "deve", "devem", "conhece", "conhecem", "parece", "parecem", "precisa", "precisam", "gosta", "gostam", "existe", "existem",
            "chega", "chegam", "volta", "voltam", "sai", "saem", "passa", "passam", "basta", "falta", "faltam", "acaba", "acabam",
            "começa", "começam", "custa", "importa", "serve", "dá", "fica", "ficam", "entra", "entram", "percebe", "percebem",
            "entende", "entendem", "lembra", "lembram", "acredita", "acreditam", "sente", "sentem", "vale", "chama", "chamam",
            "nada", "toda", "cada", "uma", "essa", "esta", "ela", "aquela", "agora", "outra", "alguma", "nenhuma", "boa", "tarde", "ontem",
            "sempre", "antes", "hoje", "noite", "este", "esse", "ele", "eles", "elas", "nem", "bem", "mesma", "mesmo", "pronta", "pronto",
            "era", "eram", "foi", "foram", "tinha", "tinham", "estava", "estavam", "ia", "iam", "morre", "morrem", "nasce", "termina",
        };

        private static readonly Dictionary<string, string> Irregular = new(StringComparer.OrdinalIgnoreCase)
        {
            ["faz"] = "fazer", ["fazem"] = "fazer", ["diz"] = "dizer", ["dizem"] = "dizer", ["traz"] = "trazer", ["trazem"] = "trazer",
            ["põe"] = "pôr", ["põem"] = "pôr", ["lê"] = "ler", ["leem"] = "ler", ["pede"] = "pedir", ["pedem"] = "pedir",
            ["mente"] = "mentir", ["mentem"] = "mentir", ["segue"] = "seguir", ["seguem"] = "seguir", ["foge"] = "fugir", ["fogem"] = "fugir",
            ["ouve"] = "ouvir", ["ouvem"] = "ouvir", ["dorme"] = "dormir", ["dormem"] = "dormir", ["sobe"] = "subir", ["sobem"] = "subir",
            ["veste"] = "vestir", ["vestem"] = "vestir", ["consegue"] = "conseguir", ["conseguem"] = "conseguir",
        };

        // The event that started it moves to the past: "sai da prisão e já está a" → "saiu da prisão e já está a".
        private static readonly Dictionary<string, string> TriggerPast = new(StringComparer.OrdinalIgnoreCase)
        {
            ["sai"] = "saiu", ["saem"] = "saíram", ["chega"] = "chegou", ["chegam"] = "chegaram", ["entra"] = "entrou", ["entram"] = "entraram",
            ["acorda"] = "acordou", ["acordam"] = "acordaram", ["aparece"] = "apareceu", ["aparecem"] = "apareceram", ["volta"] = "voltou",
            ["voltam"] = "voltaram", ["sai-se"] = "saiu-se", ["foge"] = "fugiu", ["fogem"] = "fugiram",
        };

        private static readonly Regex Trigger = new(
            @"(?<=(?:^|[.!?…]\s+|\n(?<![\p{L}\p{M}])[-–—]\s*|(?<![\p{L}\p{M}])[-–—]\s*)[^.!?…\-–—]*?" + NotWordBefore + @"(?:ele|ela|eles|elas|você|vocês|\p{Lu}\p{Ll}+)\s+(?:mal\s+)?)" +
            @"(?<v>sai|saem|chega|chegam|entra|entram|acorda|acordam|aparece|aparecem|volta|voltam|foge|fogem)" + NotWordAfter +
            @"(?=[^.!?…]*?" + NotWordBefore + @"(?:e|mas)\s+já\s+(?:está|estão)\s+a\s)", Ci);

        public static string Apply(string s)
        {
            var changed = false;
            s = AlreadyDoing.Replace(s, m =>
            {
                var v = m.Groups["v"].Value;
                var plural = v.EndsWith("am", StringComparison.OrdinalIgnoreCase) || v.EndsWith("em", StringComparison.OrdinalIgnoreCase);
                var inf = NotProgressive.Contains(v) ? null : ToInfinitive(v.ToLowerInvariant());
                if (inf == null)
                {
                    return m.Value;
                }

                changed = true;
                return m.Groups["pre"].Value + (plural ? "estão a " : "está a ") + inf;
            });

            return changed ? VocabularyRules.ApplyArrive(Trigger.Replace(s, m => CaseHelper.MatchCase(m.Groups["v"].Value, TriggerPast[m.Groups["v"].Value]))) : s;
        }

        private static string? ToInfinitive(string v)
        {
            if (Irregular.TryGetValue(v, out var irregular))
            {
                return irregular;
            }

            if (v.Length < 4)
            {
                return null;
            }

            var natura = NaturaDictionary.Load(true);
            if (!natura.Check(v) && !PortugueseVerbs.IsKnownVerbForm(v))
            {
                return null;
            }

            string? Pick(params string[] candidates)
            {
                var found = candidates.Where(TuImperative.IsInfinitive).ToList();
                return found.Count == 1 ? found[0] : null; // "sente" could be sentar or sentir
            }

            if (v.EndsWith("am")) return Pick(v[..^2] + "ar");
            if (v.EndsWith("em")) return Pick(v[..^2] + "er", v[..^2] + "ir");
            if (v.EndsWith('a')) return Pick(v[..^1] + "ar");
            if (v.EndsWith('e')) return Pick(v[..^1] + "er", v[..^1] + "ir");
            return null;
        }
    }

    internal static class SpellingRules
    {
        // Brazilian ô/ê before m/n correspond to European ó/é (econômico → económico, gênero → género).
        private static readonly Regex OBeforeNasal = new(@"(?<=" + W + @")ô(?=(?:mic|nic|met|nim|nom|mod|men|mat|nio|nia|nus|mit|nei|mul|meg|mio|mia)" + W + "*" + NotWordAfter + ")", Cs);
        private static readonly Regex OBeforeNasalUpper = new(@"(?<=" + W + @")Ô(?=(?:MIC|NIC|MET|NIM|NOM|MOD|MEN|MAT|NIO|NIA|NUS|MIT|NEI|MUL|MEG|MIO|MIA)" + W + "*" + NotWordAfter + ")", Cs);
        private static readonly Regex EBeforeNasal = new(@"(?<=" + W + @")ê(?=(?:mic|nic|ner|nit|nio|nia|nue|nuo|nua|nis|nes|meo|mea|mio|mia|men|mur|nus|nix|mul)" + W + "*" + NotWordAfter + ")", Cs);
        private static readonly Regex EBeforeNasalUpper = new(@"(?<=" + W + @")Ê(?=(?:MIC|NIC|NER|NIT|NIO|NIA|NUE|NUO|NUA|NIS|NES|MEO|MEA|MIO|MIA|MEN|MUR|NUS|NIX|MUL)" + W + "*" + NotWordAfter + ")", Cs);

        // Spellings abolished by the 1990 agreement that still appear in older Brazilian subtitles.
        private static readonly Regex Trema = new(@"(?<=[gqGQ])[üÜ](?=[eiéíêEIÉÍÊ])", Cs);
        private static readonly Regex Eia = new(@"(?<=" + W + @")éi(?=as?" + NotWordAfter + ")", Cs);
        private static readonly Regex Oia = new(@"(?<=" + W + @")ói(?=(?:a|o|as|os|co|ca|cos|cas)" + NotWordAfter + ")", Cs);
        private static readonly Regex Oo = new(@"(?<=" + W + @")ôo(?=s?" + NotWordAfter + ")", Cs);
        // European Portuguese contracts "em" with indefinite articles and pronouns: em um → num, em outra → noutra, em algum → nalgum.
        private static readonly Regex EmIndefinite = new(
            @"(?<pre>[ \t]*)" + NotWordBefore + @"(?<em>em)(?<sp>\s+)(?<w>um|uma|uns|umas|outro|outra|outros|outras|algum|alguma|alguns|algumas)" + NotWordAfter, Ci);

        private static string ReplaceEmIndefinite(Match m)
        {
            var em = m.Groups["em"].Value;
            var word = m.Groups["w"].Value;
            var contracted = "n" + word;
            if (CaseHelper.IsAllUpper(em + word))
            {
                contracted = contracted.ToUpperInvariant();
            }
            else if (char.IsUpper(em[0]))
            {
                contracted = "N" + contracted.Substring(1);
            }

            var sp = m.Groups["sp"].Value;
            var lineBreak = sp.Contains('\n') ? sp.Substring(sp.IndexOfAny(['\r', '\n'])) : null;
            // "...em\num barril" keeps the line break: "...\nnum barril".
            return lineBreak == null
                ? m.Groups["pre"].Value + contracted
                : lineBreak.TrimEnd(' ', '\t') + contracted;
        }

        private static readonly Regex Eem = new(NotWordBefore + @"(?<v>cr|d|l|v|rel|desd|prev|rev|descr|ant)êem" + NotWordAfter, Ci);

        // Plural agreement for ter/vir: "Polícias de rua tem" → "têm", "Eles vem" → "vêm".
        private const string TemAfter = @"(?![\p{L}\p{M}\d_]|-(?!\p{Ll}))";
        private const string TemVerb = @"(?<v>tem|vem|mantém|contém|obtém|detém|retém)";
        private const string TemGap = @"(?:\s+(?:não|já|também|ainda|só|sempre|nunca|lá|cá|todos|todas))*(?:\s+(?:se|me|te|nos|vos|lhe|lhes|o|a|os|as))?\s+";
        private const string DePhrase = @"(?:\s+(?:dele|dela|deles|delas|(?:de|da|do|dos|das|em|na|no|nas|nos)(?:\s+" + W + "+){1,2}))?";
        private const string PluralAdjective = @"(?:\s+(?!(?:não|já|também|ainda|só|sempre|nunca|lá|cá|todos|todas|os|as|nos|lhes)\b)" + W + "+s)?";
        private const string TimeNouns = @"(?:vezes|dias|anos|meses|semanas|horas|minutos|segundos|noites|manhãs|tardes|noites|vez)";
        private const string SentenceStartTem = @"(?<pre>(?:^|[.!?…:]\s+|\n)[ \t]*(?:-[ \t]*)?(?:<i>[ \t]*)?)";

        private static readonly Regex PronounTem = new(
            @"(?<![Pp]ara\s|[Cc]om\s|[Dd]e\s|[Ss]em\s|[Pp]or\s|[Cc]ontra\s|[Ee]ntre\s|[Ss]obre\s|[Aa]\s)" + NotWordBefore +
            @"(?<s>eles|elas|vocês|ambos|ambas)(?<g>" + TemGap + ")" + TemVerb + TemAfter, Ci);

        private static readonly Regex DeterminerTem = new(
            SentenceStartTem +
            @"(?<s>(?<d>(?:(?:todos\s+os|todas\s+as|(?:os|as)\s+(?:meus|minhas|teus|tuas|seus|suas|nossos|nossas|dois|duas|três|outros|outras|outras)|" +
            @"os|as|estes|estas|esses|essas|aqueles|aquelas|uns|umas|muitos|muitas|alguns|algumas|vários|várias|poucos|poucas|outros|outras|" +
            @"meus|minhas|teus|tuas|seus|suas|nossos|nossas|ambos|ambas|dois|duas|três|quatro|cinco|seis|sete|oito|nove|dez|\d+)\s+))?" +
            @"(?<n>(?!" + TimeNouns + NotWordAfter + ")" + W + "+s)" + PluralAdjective + DePhrase + ")" +
            @"(?<g>" + TemGap + ")" + TemVerb + TemAfter, Ci | RegexOptions.Multiline);

        private static readonly HashSet<string> SingularEndingInS = new(StringComparer.OrdinalIgnoreCase)
        {
            "depois", "antes", "pois", "mais", "menos", "talvez", "apenas", "através", "atrás", "jamais", "às", "nós", "vós",
            "deus", "jesus", "lápis", "vírus", "ônibus", "autocarros", "pires", "tênis", "ténis", "mês", "país", "português",
            "inglês", "francês", "japonês", "chinês", "freguês", "gás", "ás", "rês", "simples", "status", "bónus", "bônus", "campus",
            "corpus", "atlas", "férias", "parabéns", "óculos", "calças", "costas",
        };

        private static string ReplaceTem(Match m)
        {
            var v = m.Groups["v"].Value;
            var plural = v.ToLowerInvariant() switch
            {
                "tem" => "têm",
                "vem" => "vêm",
                var x => x.Substring(0, x.Length - 3) + (x.EndsWith("tém") ? "têm" : "vêm"),
            };
            plural = CaseHelper.MatchCase(v, plural);
            return m.Value.Substring(0, m.Groups["v"].Index - m.Index) + plural;
        }

        private static string ReplaceDeterminerTem(Match m)
        {
            var words = m.Groups["s"].Value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            var head = m.Groups["n"].Value;
            if (!m.Groups["d"].Success)
            {
                // A bare plural noun ("Polícias de rua tem") needs a capital letter and a following "de" phrase;
                // "Depois tem", "Lucas tem" and adverbs ending in -s are left alone.
                if (SingularEndingInS.Contains(head) || head.Length < 4 || !char.IsUpper(head[0]) ||
                    words.Length < 3 || !Regex.IsMatch(words[1], "^(?:de|da|do|dos|das)$") || !char.IsLower(words[2][0]))
                {
                    return m.Value;
                }
            }
            else if (words.Any(w => SingularEndingInS.Contains(w) && !w.Equals("ambos", StringComparison.OrdinalIgnoreCase)))
            {
                return m.Value;
            }

            return ReplaceTem(m);
        }

        public static string Apply(string s)
        {
            s = OBeforeNasal.Replace(s, "ó");
            s = OBeforeNasalUpper.Replace(s, "Ó");
            s = EBeforeNasal.Replace(s, "é");
            s = EBeforeNasalUpper.Replace(s, "É");
            s = Trema.Replace(s, m => m.Value == "ü" ? "u" : "U");
            s = Eia.Replace(s, "ei");
            s = Oia.Replace(s, "oi");
            s = Oo.Replace(s, "oo");
            s = Eem.Replace(s, m => m.Groups["v"].Value + (CaseHelper.IsAllUpper(m.Value) ? "EEM" : "eem"));
            s = EmIndefinite.Replace(s, ReplaceEmIndefinite);
            s = PronounTem.Replace(s, ReplaceTem);
            s = DeterminerTem.Replace(s, ReplaceDeterminerTem);
            return s;
        }
    }
}
