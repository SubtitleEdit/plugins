using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;

/// <summary>
/// "Talvez" before the verb takes the subjunctive, and the subjunctive attracts the clitic:
/// "talvez ele esqueceu-se" → "talvez ele se tenha esquecido", "talvez é" → "talvez seja",
/// "talvez ela estava" → "talvez ela estivesse". A verb before "talvez" ("Esqueceu-se, talvez") is left alone.
/// </summary>
internal static class MaybeSubjunctive
{
    private const RegexOptions Ci = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;

    private const string ComplementVerb =
        @"(?:(?<subj>eu|tu|ele|ela|nós|vós|eles|elas|você|vocês|" +
        @"(?:o|a|os|as)\s+(?:(?:meu|minha|meus|minhas|teu|tua|teus|tuas|seu|sua|seus|suas|nosso|nossa|nossos|nossas)\s+)?[\p{L}\p{M}]+|" +
        @"(?-i:\p{Lu}[\p{Ll}\p{M}]+))\s+)?" +
        @"(?<adv>(?:(?:não|nunca|já|ainda|também|só|mesmo|realmente)\s+){0,2}))" +
        @"(?:(?<cl>me|te|se|nos|vos|lhe|lhes)\s+)?" +
        @"(?<v>[\p{L}\p{M}]+)(?:-(?<ecl>me|te|se|nos|vos|lhe|lhes|o|a|os|as))?(?![\p{L}\p{M}-])" +
        @"(?=(?<next>\s+[\p{L}\p{M}]+)?)";

    private static readonly Regex Maybe = new(
        @"(?<pre>(?<![\p{L}\p{M}-])talvez\s+" + ComplementVerb, Ci);

    private static readonly Regex Complements = new(
        @"(?<pre>(?<![\p{L}\p{M}-])" +
        @"(?:receio|receias|receia|receamos|receais|receiam|temo|temes|teme|tememos|temeis|temem|" +
        @"desejo|desejas|deseja|desejamos|desejais|desejam|duvido|duvidas|duvida|duvidamos|duvidais|duvidam)" +
        @"\s+que\s+" + ComplementVerb, Ci);

    // Present and imperfect indicative of irregular verbs → subjunctive.
    private static readonly Dictionary<string, string> Irregular = new(StringComparer.OrdinalIgnoreCase)
    {
        ["é"] = "seja", ["sou"] = "seja", ["és"] = "sejas", ["somos"] = "sejamos", ["são"] = "sejam",
        ["era"] = "fosse", ["eras"] = "fosses", ["éramos"] = "fôssemos", ["eram"] = "fossem",
        ["está"] = "esteja", ["estou"] = "esteja", ["estás"] = "estejas", ["estamos"] = "estejamos", ["estão"] = "estejam",
        ["estava"] = "estivesse", ["estavas"] = "estivesses", ["estávamos"] = "estivéssemos", ["estavam"] = "estivessem",
        ["tem"] = "tenha", ["tenho"] = "tenha", ["tens"] = "tenhas", ["temos"] = "tenhamos", ["têm"] = "tenham",
        ["tinha"] = "tivesse", ["tinhas"] = "tivesses", ["tínhamos"] = "tivéssemos", ["tinham"] = "tivessem",
        ["vai"] = "vá", ["vou"] = "vá", ["vais"] = "vás",
        ["ia"] = "fosse", ["ias"] = "fosses", ["íamos"] = "fôssemos", ["iam"] = "fossem",
        ["há"] = "haja", ["havia"] = "houvesse",
        ["pode"] = "possa", ["posso"] = "possa", ["podes"] = "possas", ["podemos"] = "possamos", ["podem"] = "possam",
        ["podia"] = "pudesse", ["podias"] = "pudesses", ["podíamos"] = "pudéssemos", ["podiam"] = "pudessem",
        ["sabe"] = "saiba", ["sei"] = "saiba", ["sabes"] = "saibas", ["sabemos"] = "saibamos", ["sabem"] = "saibam",
        ["sabia"] = "soubesse", ["sabias"] = "soubesses", ["sabíamos"] = "soubéssemos", ["sabiam"] = "soubessem",
        ["quer"] = "queira", ["quero"] = "queira", ["queres"] = "queiras", ["queremos"] = "queiramos", ["querem"] = "queiram",
        ["queria"] = "quisesse", ["querias"] = "quisesses", ["queríamos"] = "quiséssemos", ["queriam"] = "quisessem",
        ["faz"] = "faça", ["faço"] = "faça", ["fazes"] = "faças", ["fazemos"] = "façamos", ["fazem"] = "façam",
        ["fazia"] = "fizesse", ["fazias"] = "fizesses", ["fazíamos"] = "fizéssemos", ["faziam"] = "fizessem",
        ["diz"] = "diga", ["digo"] = "diga", ["dizes"] = "digas", ["dizemos"] = "digamos", ["dizem"] = "digam",
        ["dizia"] = "dissesse", ["dizias"] = "dissesses", ["dizíamos"] = "disséssemos", ["diziam"] = "dissessem",
        ["vê"] = "veja", ["vejo"] = "veja", ["vês"] = "vejas", ["vemos"] = "vejamos", ["veem"] = "vejam", ["vêem"] = "vejam",
        ["vem"] = "venha", ["venho"] = "venha", ["vens"] = "venhas", ["vêm"] = "venham",
        ["dá"] = "dê", ["dou"] = "dê", ["dás"] = "dês", ["damos"] = "demos", ["dão"] = "deem",
        ["põe"] = "ponha", ["ponho"] = "ponha", ["pões"] = "ponhas", ["pomos"] = "ponhamos", ["põem"] = "ponham",
        ["punha"] = "pusesse", ["punham"] = "pusessem",
        ["traz"] = "traga", ["trago"] = "traga", ["trazes"] = "tragas", ["trazem"] = "tragam",
        ["trazia"] = "trouxesse", ["traziam"] = "trouxessem",
        ["sai"] = "saia", ["saio"] = "saia", ["sais"] = "saias", ["saem"] = "saiam",
        ["ouve"] = "ouça", ["ouço"] = "ouça", ["ouves"] = "ouças", ["ouvem"] = "ouçam",
        ["pede"] = "peça", ["peço"] = "peça", ["pedes"] = "peças", ["pedem"] = "peçam",
        ["perde"] = "perca", ["perco"] = "perca", ["perdes"] = "percas", ["perdem"] = "percam",
        ["consegue"] = "consiga", ["consegues"] = "consigas", ["conseguem"] = "consigam",
        ["sente"] = "sinta", ["sinto"] = "sinta", ["sentes"] = "sintas", ["sentem"] = "sintam",
        ["segue"] = "siga", ["sigo"] = "siga", ["segues"] = "sigas", ["seguem"] = "sigam",
        ["prefere"] = "prefira", ["prefiro"] = "prefira", ["preferes"] = "prefiras", ["preferem"] = "prefiram",
        ["lê"] = "leia", ["leio"] = "leia", ["lês"] = "leias", ["leem"] = "leiam", ["lêem"] = "leiam",
        ["cabe"] = "caiba", ["vale"] = "valha", ["valem"] = "valham", ["dorme"] = "durma", ["dormem"] = "durmam",
        ["mente"] = "minta", ["mentem"] = "mintam", ["foge"] = "fuja", ["fogem"] = "fujam", ["sobe"] = "suba", ["sobem"] = "subam",
        ["acredita"] = "acredite", ["acredito"] = "acredite",
    };

    // Irregular past forms → person index (0 eu/ele, 1 tu, 2 nós, 3 eles) and participle used with "tenha".
    private static readonly Dictionary<string, (int Person, string Participle)> IrregularPast = new(StringComparer.OrdinalIgnoreCase)
    {
        ["fui"] = (0, "?"), ["foste"] = (1, "?"), ["foi"] = (0, "?"), ["fomos"] = (2, "?"), ["foram"] = (3, "?"),
        ["estive"] = (0, "estado"), ["estiveste"] = (1, "estado"), ["esteve"] = (0, "estado"), ["estivemos"] = (2, "estado"), ["estiveram"] = (3, "estado"),
        ["tive"] = (0, "tido"), ["tiveste"] = (1, "tido"), ["teve"] = (0, "tido"), ["tivemos"] = (2, "tido"), ["tiveram"] = (3, "tido"),
        ["fiz"] = (0, "feito"), ["fizeste"] = (1, "feito"), ["fez"] = (0, "feito"), ["fizemos"] = (2, "feito"), ["fizeram"] = (3, "feito"),
        ["disse"] = (0, "dito"), ["disseste"] = (1, "dito"), ["dissemos"] = (2, "dito"), ["disseram"] = (3, "dito"),
        ["vi"] = (0, "visto"), ["viste"] = (1, "visto"), ["viu"] = (0, "visto"), ["vimos"] = (2, "visto"), ["viram"] = (3, "visto"),
        ["vim"] = (0, "vindo"), ["vieste"] = (1, "vindo"), ["veio"] = (0, "vindo"), ["viemos"] = (2, "vindo"), ["vieram"] = (3, "vindo"),
        ["pus"] = (0, "posto"), ["puseste"] = (1, "posto"), ["pôs"] = (0, "posto"), ["pusemos"] = (2, "posto"), ["puseram"] = (3, "posto"),
        ["dei"] = (0, "dado"), ["deste"] = (1, "dado"), ["deu"] = (0, "dado"), ["demos"] = (2, "dado"), ["deram"] = (3, "dado"),
        ["soube"] = (0, "sabido"), ["soubeste"] = (1, "sabido"), ["soubemos"] = (2, "sabido"), ["souberam"] = (3, "sabido"),
        ["pude"] = (0, "podido"), ["pôde"] = (0, "podido"), ["puderam"] = (3, "podido"),
        ["quis"] = (0, "querido"), ["quiseste"] = (1, "querido"), ["quiseram"] = (3, "querido"),
        ["trouxe"] = (0, "trazido"), ["trouxeste"] = (1, "trazido"), ["trouxeram"] = (3, "trazido"),
        ["houve"] = (0, "havido"),
    };

    private static readonly Dictionary<string, string> IrregularParticiples = new(StringComparer.OrdinalIgnoreCase)
    {
        ["abrir"] = "aberto", ["escrever"] = "escrito", ["cobrir"] = "coberto", ["descobrir"] = "descoberto",
        ["pagar"] = "pago", ["ganhar"] = "ganho", ["dizer"] = "dito", ["fazer"] = "feito", ["ver"] = "visto",
        ["vir"] = "vindo", ["pôr"] = "posto",
    };

    // After "foi" these complements mean movement ("tenha ido embora"); otherwise "foi" is "ser" ("tenha sido").
    private static readonly Regex Movement = new(@"^\s+(?:embora|lá|ali|aí|para|pra|até|ao|à|aos|às|daqui|dali)$", Ci);

    private static readonly string[] Have = { "tenha", "tenhas", "tenhamos", "tenham" };

    private const string SubjunctiveAuxiliary =
        "tenha|tenhas|tenhamos|tenham|tivesse|tivesses|tivéssemos|tivessem|seja|sejas|sejamos|sejam|esteja|estejas|estejamos|estejam";

    // Inside a clause the subjunctive is never enclitic: "talvez ele tenha-lhe ligado" → "talvez ele lhe tenha ligado".
    private static readonly Regex EncliticSubjunctive = new(
        @"(?<pre>[\p{L}\p{M}]+[ \t]+)(?<v>" + SubjunctiveAuxiliary + @")-(?<cl>me|te|se|nos|vos|lhe|lhes|o|a|os|as)(?![\p{L}\p{M}-])", Ci);

    // "esquecer-se de" / "lembrar-se de" keep the pronoun in compound subjunctives: "que ele tenha esquecido de" →
    // "que ele se tenha esquecido de"; a direct object ("tenha esquecido o nome") is kept.
    private static readonly Regex ReflexiveCompound = new(
        @"(?<pre>(?<![\p{L}\p{M}-])(?<subj>eu|tu|ele|ela|nós|eles|elas|[\p{L}\p{M}]+)\s+(?<adv>(?:(?:não|nunca|já|também|só)\s+){0,2}))" +
        @"(?<aux>tenha|tenhas|tenhamos|tenham|tivesse|tivesses|tivéssemos|tivessem)\s+(?<part>esquecid[oa]s?|lembrad[oa]s?)" +
        @"(?=\s+(?:de|do|da|dos|das|disso|disto|daquilo|dele|dela|deles|delas|dum|duma)(?![\p{L}\p{M}])|\s+(?:tudo\s+)?mal(?![\p{L}\p{M}])|\s*[.!?…,;]|\s*$)", Ci);

    // "talvez há dois anos" is "maybe two years ago", not existential "haver".
    private static readonly Regex TimeAgo = new(
        @"^\s+(?:\d+|um|uma|uns|umas|dois|duas|três|quatro|cinco|seis|sete|oito|nove|dez|vinte|trinta|cem|muito|muitos|muitas|pouco|poucos|poucas|alguns|algumas|bastante|mais|menos)" +
        @"(?:\s+[\p{L}\p{M}]+)?\s+(?:segundos?|minutos?|horas?|dias?|semanas?|meses|mês|anos?|décadas?|tempo)(?![\p{L}\p{M}])", Ci);

    // A relative "que" whose antecedent is the object: "Algo que tenhas esquecido" is not reflexive.
    private static readonly Regex ObjectAntecedent = new(
        @"(?<![\p{L}\p{M}-])(?:algo|tudo|nada|coisa|coisas|aquilo|isso|isto|o|a|os|as|aquele|aquela|aqueles|aquelas|qualquer)\s+$", Ci);

    private static readonly HashSet<string> NotClauseHosts = new(StringComparer.OrdinalIgnoreCase)
    {
        "me", "te", "se", "nos", "vos", "lhe", "lhes", "o", "a", "os", "as",
    };

    // The same verbs in indicative/conditional compound tenses: "ele tinha esquecido de" → "ele tinha-se esquecido de",
    // "que ele tinha esquecido de" → "que ele se tinha esquecido de", "teria esquecido" → "ter-se-ia esquecido".
    private static readonly Regex ReflexiveIndicative = new(
        @"(?<![\p{L}\p{M}-])(?<aux>tenho|tens|tem|temos|têm|tinha|tinhas|tínhamos|tinham|havia|havias|havíamos|haviam|" +
        @"teria|terias|teríamos|teriam|terei|terás|terá|teremos|terão)\s+(?<part>esquecid[oa]s?|lembrad[oa]s?)" +
        @"(?=\s+(?:de|do|da|dos|das|disso|disto|daquilo|dele|dela|deles|delas|dum|duma)(?![\p{L}\p{M}])|\s+(?:tudo\s+)?mal(?![\p{L}\p{M}])|\s*[.!?…,;]|\s*$)", Ci);

    private static readonly Regex IndicativeClause = new(@"(?:[^.!?…;,\n]|\n(?![-–—]))*$", RegexOptions.CultureInvariant);

    private static readonly HashSet<string> ProclisisTriggers = new(StringComparer.OrdinalIgnoreCase)
    {
        "não", "nunca", "jamais", "já", "também", "ainda", "sempre", "só", "nem", "apenas", "talvez", "que", "quem", "ninguém",
        "alguém", "tudo", "nada", "bem", "quando", "onde", "como", "porque", "enquanto", "embora", "caso", "se", "quanto",
        "todos", "todas", "ambos", "ambas", "logo", "até", "mesmo",
    };

    private static readonly HashSet<string> ThirdPersonSubjects = new(StringComparer.OrdinalIgnoreCase)
    {
        "ele", "ela", "você", "isso", "alguém", "ninguém", "quem", "pai", "mãe", "filho", "filha", "marido", "mulher",
    };

    private static readonly HashSet<string> NotNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "ontem", "hoje", "agora", "antes", "depois", "afinal", "então", "entretanto", "aqui", "ali", "lá", "assim", "claro", "sim",
        "bem", "mas", "e", "ou", "pois", "porém", "contudo", "acho", "pensei", "talvez", "se", "quando", "porque", "como", "onde",
        "tu", "nós", "eles", "elas", "vocês", "quem", "alguém", "ninguém", "isto", "aquilo", "tudo", "nada", "olha", "ok", "bom",
        "pronto", "enfim", "também", "sempre", "nunca", "já", "ainda", "só", "aí", "cá", "senão", "logo", "desta", "dessa",
    };

    // A capitalised clause opener is a name subject ("Frank tinha esquecido") unless it is a common word or a verb form.
    private static bool LooksLikeName(string word) =>
        word.Length > 1 && !NotNames.Contains(word) && !word.EndsWith("mente", StringComparison.OrdinalIgnoreCase) &&
        !PortugueseVerbs.IsKnownVerbForm(word);

    private static string ReflexiveIndicativeReplace(string s, Match m)
    {
        var aux = m.Groups["aux"].Value;
        var lower = aux.ToLowerInvariant();
        var clause = IndicativeClause.Match(s[..m.Index]).Value;
        var words = Regex.Matches(clause, @"[\p{L}\p{M}]+").Select(w => w.Value).ToList();
        var previous = words.Count > 0 ? words[^1] : null;
        if (previous != null && NotClauseHosts.Contains(previous)) return m.Value;
        // Without a following "de" the object may be fronted: "o que tinha esquecido".
        var tail = s[(m.Index + m.Length)..];
        var openEnd = !Regex.IsMatch(tail, @"^\s+(?:de|do|da|dos|das|disso|disto|daquilo|dele|dela|deles|delas|dum|duma|(?:tudo\s+)?mal)(?![\p{L}\p{M}])", Ci);
        if (openEnd && words.Any(w => w.ToLowerInvariant() is "que" or "quê" or "qual" or "quais")) return m.Value;

        string? clitic = lower switch
        {
            "tens" or "tinhas" or "havias" or "terias" or "terás" => "te",
            "temos" or "tínhamos" or "havíamos" or "teríamos" or "teremos" => "nos",
            "têm" or "tinham" or "haviam" or "teriam" or "terão" or "tem" or "terá" => "se",
            "tenho" or "terei" => "me",
            _ => null,
        };
        if (clitic == null)
        {
            // "tinha/havia/teria" are first or third person: decide from the subject right before it.
            var subjectIndex = words.Count - 1;
            while (subjectIndex >= 0 && words[subjectIndex].ToLowerInvariant() is "não" or "nunca" or "já" or "também" or "só" or "ainda" or "mesmo")
                subjectIndex--;
            if (subjectIndex < 0) return m.Value;
            var subject = words[subjectIndex];
            if (subject.Equals("eu", StringComparison.OrdinalIgnoreCase)) clitic = "me";
            else if (ThirdPersonSubjects.Contains(subject) ||
                     (char.IsUpper(subject[0]) && !CaseHelper.IsAllUpper(subject) && (subjectIndex > 0 || LooksLikeName(subject)))) clitic = "se";
            else return m.Value;
        }
        if (CaseHelper.IsAllUpper(aux)) clitic = clitic.ToUpperInvariant();
        var gap = m.Value.Substring(aux.Length, m.Value.Length - aux.Length - m.Groups["part"].Length);

        if (words.Any(ProclisisTriggers.Contains))
            return clitic + " " + (char.IsUpper(aux[0]) && !CaseHelper.IsAllUpper(aux) && clause.Trim().Length == 0
                ? aux.ToLowerInvariant() : aux) + gap + m.Groups["part"].Value;
        var attached = PortugueseVerbs.AttachMesoclitic(aux, clitic, false) ?? PortugueseVerbs.AttachEnclitic(aux, clitic);
        return attached + gap + m.Groups["part"].Value;
    }

    public static string ApplyLate(string s)
    {
        s = EncliticSubjunctive.Replace(s, m => m.Groups["pre"].Value + m.Groups["cl"].Value + " " + m.Groups["v"].Value);
        s = ReflexiveCompound.Replace(s, m =>
        {
            if (m.Groups["subj"].Value.Equals("que", StringComparison.OrdinalIgnoreCase) &&
                ObjectAntecedent.IsMatch(s.Substring(0, m.Index))) return m.Value;
            if (NotClauseHosts.Contains(m.Groups["subj"].Value) && m.Groups["adv"].Length == 0) return m.Value;
            var aux = m.Groups["aux"].Value.ToLowerInvariant();
            var subj = m.Groups["subj"].Value.ToLowerInvariant();
            var clitic = aux switch
            {
                "tenhas" or "tivesses" => "te",
                "tenhamos" or "tivéssemos" => "nos",
                "tenha" or "tivesse" when subj == "eu" => "me",
                _ => "se",
            };
            if (CaseHelper.IsAllUpper(m.Groups["aux"].Value)) clitic = clitic.ToUpperInvariant();
            return m.Groups["pre"].Value + clitic + " " + m.Groups["aux"].Value + " " + m.Groups["part"].Value;
        });
        if (s.IndexOf("esquecid", StringComparison.OrdinalIgnoreCase) < 0 && s.IndexOf("lembrad", StringComparison.OrdinalIgnoreCase) < 0) return s;
        return ReflexiveIndicative.Replace(s, m => ReflexiveIndicativeReplace(s, m));
    }

    public static string Apply(string s)
    {
        if (s.IndexOf("talvez", StringComparison.OrdinalIgnoreCase) < 0) return s;
        return Maybe.Replace(s, m => Replace(m, s.Substring(m.Index + m.Length)));
    }

    public static string ApplyComplements(string s) => Complements.Replace(s, m =>
    {
        // Negated doubt and nominal "o receio que..." do not license this inference.
        if (Regex.IsMatch(m.Result("$`"),
                @"(?<![\p{L}\p{M}])(?:não|nunca|nem|sem|o|a|os|as|um|uma|esse|essa|este|esta|meu|minha|teu|tua|seu|sua)\s+$", Ci) ||
            !TryPresentSubjunctive(m.Groups["v"].Value, out var subjunctive) ||
            m.Groups["cl"].Success && m.Groups["ecl"].Success)
            return m.Value;
        var clitic = m.Groups["cl"].Success ? m.Groups["cl"].Value : m.Groups["ecl"].Value;
        return m.Groups["pre"].Value + (clitic.Length > 0 ? clitic + " " : string.Empty) + subjunctive;
    });

    private static string Replace(Match m, string rest)
    {
        var verb = m.Groups["v"].Value;
        var lower = verb.ToLowerInvariant();
        var hasSubject = m.Groups["subj"].Success || m.Groups["cl"].Success || m.Groups["ecl"].Success;
        string? replacement = null;

        if (lower == "há" && TimeAgo.IsMatch(rest)) return m.Value;

        if (Irregular.TryGetValue(lower, out var subjunctive))
        {
            replacement = subjunctive;
        }
        else if (IrregularPast.TryGetValue(lower, out var past))
        {
            var participle = past.Participle == "?"
                ? Movement.IsMatch(m.Groups["next"].Value) ? "ido" : "sido"
                : past.Participle;
            replacement = Have[past.Person] + " " + participle;
        }
        else if (TryRegular(lower, hasSubject, m.Groups["next"].Value.Trim(), out var regular))
        {
            replacement = regular;
        }

        if (replacement == null) return m.Value;

        if (CaseHelper.IsAllUpper(verb) && verb.Length > 1) replacement = replacement.ToUpperInvariant();
        else if (char.IsUpper(verb[0])) replacement = CaseHelper.Capitalize(replacement);

        var clitic = m.Groups["cl"].Success ? m.Groups["cl"].Value.TrimEnd() : m.Groups["ecl"].Value;
        if (m.Groups["cl"].Success && m.Groups["ecl"].Success) return m.Value;
        return m.Groups["pre"].Value + (clitic.Length > 0 ? clitic + " " : string.Empty) + replacement;
    }

    // Present indicative → present subjunctive only ("podemos" → "possamos"); past and imperfect forms are rejected.
    internal static bool TryPresentSubjunctive(string form, out string subjunctive)
    {
        subjunctive = string.Empty;
        var lower = form.ToLowerInvariant();
        string? result = null;
        if (lower is "consigo") result = "consiga";
        else if (lower is "conseguimos") result = "consigamos";
        else if (Irregular.TryGetValue(lower, out var irregular))
        {
            if (irregular.EndsWith("sse", StringComparison.Ordinal) || irregular.EndsWith("sses", StringComparison.Ordinal) ||
                irregular.EndsWith("ssemos", StringComparison.Ordinal) || irregular.EndsWith("ssem", StringComparison.Ordinal)) return false;
            result = irregular;
        }
        else if (TryRegular(lower, true, string.Empty, out var regular) && !regular.Contains(' ') &&
                 !regular.EndsWith("sse", StringComparison.Ordinal) && !regular.EndsWith("ssem", StringComparison.Ordinal) &&
                 !regular.EndsWith("sses", StringComparison.Ordinal) && !regular.EndsWith("ssemos", StringComparison.Ordinal))
            result = regular;
        if (result == null) return false;
        subjunctive = CaseHelper.IsAllUpper(form) && form.Length > 1 ? result.ToUpperInvariant()
            : char.IsUpper(form[0]) ? CaseHelper.Capitalize(result) : result;
        return true;
    }

    // Slots of VerbConjugator: present 1-5, preterite 6-9, imperfect 10-13, present subjunctive 23-26,
    // imperfect subjunctive 27-30, participle 35, European preterite "nós" 42.
    private static bool TryRegular(string form, bool hasSubject, string next, out string replacement)
    {
        replacement = string.Empty;
        foreach (var infinitive in CandidateInfinitives(form))
        {
            if (!PortugueseVerbs.IsKnownInfinitive(infinitive) || !VerbConjugator.TryConjugate(infinitive, out var forms)) continue;
            var slot = Array.FindIndex(forms, f => f.Equals(form, StringComparison.OrdinalIgnoreCase));
            if (slot < 0) continue;
            // Without a subject, a bare present singular is often a noun or preposition ("talvez para", "talvez nada").
            if (!hasSubject && slot is 1 or 3) return false;
            // "talvez marcas de agulha": a bare "-as/-es" form before "de" is usually a plural noun.
            if (!hasSubject && slot == 2 && Regex.IsMatch(next, "^(?:de|do|da|dos|das)$", Ci)) return false;
            var participle = IrregularParticiples.TryGetValue(infinitive, out var p) ? p : forms[35];
            string? result = slot switch
            {
                1 or 3 => forms[23],
                2 => forms[24],
                5 => forms[26],
                6 or 8 => "tenha " + participle,
                7 => "tenhas " + participle,
                9 => "tenham " + participle,
                42 when infinitive.EndsWith("ar", StringComparison.Ordinal) => "tenhamos " + participle,
                10 => forms[27],
                11 => forms[28],
                12 => forms[29],
                13 => forms[30],
                _ => null,
            };
            if (result == null) continue;
            replacement = result;
            return true;
        }

        return false;
    }

    private static IEnumerable<string> CandidateInfinitives(string form)
    {
        for (var cut = 1; cut <= Math.Min(6, form.Length - 2); cut++)
        {
            var stem = form[..^cut];
            foreach (var fixedStem in new[]
            {
                stem,
                stem.EndsWith("qu", StringComparison.Ordinal) ? stem[..^2] + "c" : null,
                stem.EndsWith("gu", StringComparison.Ordinal) ? stem[..^2] + "g" : null,
                stem.EndsWith('ç') ? stem[..^1] + "c" : null,
                stem.EndsWith('j') ? stem[..^1] + "g" : null,
            })
            {
                if (fixedStem == null) continue;
                yield return fixedStem + "ar";
                yield return fixedStem + "er";
                yield return fixedStem + "ir";
            }
        }
    }
}
