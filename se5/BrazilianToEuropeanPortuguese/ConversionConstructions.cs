using System.Text.RegularExpressions;

namespace SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;

/// <summary>Complement-aware constructions; ambiguous senses are not blanket word replacements.</summary>
internal static class ConversionConstructions
{
    private const RegexOptions Ci = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;
    private const string Before = @"(?<![\p{L}\p{M}-])";
    private const string After = @"(?![\p{L}\p{M}-])";
    private const string Adverbs = @"(?:(?:não|nunca|já|ainda|sempre|também|só|realmente|talvez)\s+){0,3}";
    private static string Alternatives(IEnumerable<string> words) =>
        string.Join("|", words.Distinct(StringComparer.OrdinalIgnoreCase).OrderByDescending(w => w.Length).Select(Regex.Escape));

    private static string[] Forms(string infinitive)
    {
        if (!VerbConjugator.TryConjugate(infinitive, out var forms))
            throw new InvalidOperationException($"Unsupported regular construction verb: {infinitive}");
        return forms;
    }

    private static string FinitePattern(string infinitive) =>
        Alternatives(Forms(infinitive).Where((_, index) => index is not (34 or 35 or 36 or 37 or 38)));

    private static readonly Regex HabitualAlways = new(
        @"(?<head>(?:^|[.!?;:]\s+|\n|(?<![\p{L}\p{M}-])(?:porque|que|quando|enquanto|se|mas|e)\s+)" +
        @"(?:(?:eu|tu|ele|ela|nós|vocês|eles|elas)\s+)?)" +
        @"(?<adv>sempre)(?<space>\s+)(?<v>\p{L}+)" + After + @"(?!-)", Ci);
    private static readonly HashSet<string> HabitualVerbs = BuildHabitualVerbs();

    private static HashSet<string> BuildHabitualVerbs()
    {
        var dictionary = NaturaDictionary.Load(true);
        return new(new[]
        {
            "saber", "perceber", "reconhecer", "descobrir", "notar", "encontrar", "chegar", "voltar",
            "falar", "dizer", "responder", "perguntar", "contar", "pensar", "achar", "acreditar",
            "trabalhar", "estudar", "comprar", "pagar", "comer", "beber", "ajudar", "esquecer",
            "ganhar", "perder", "mentir", "ouvir", "ver", "ler", "fazer",
        }.SelectMany(Forms).Where(form => (PortugueseVerbs.IsKnownVerbForm(form) || dictionary.Check(form)) &&
            !PortugueseVerbs.IsInfinitive(form) && !Regex.IsMatch(form, @"(?:ndo|[ai]d[oa]s?)$", Ci))
            .Concat(new[] { "sei", "sabes", "sabe", "sabemos", "sabem", "soube", "soubeste", "soubemos", "souberam",
                "saiba", "saibas", "saibamos", "saibam", "souber", "souberes", "soubermos", "souberem",
                "digo", "dizes", "diz", "dizemos", "dizem", "disse", "disseste", "dissemos", "disseram",
                "vejo", "vês", "vê", "vemos", "veem", "vi", "viste", "viu", "vimos", "viram",
                "faço", "fazes", "faz", "fazemos", "fazem", "fiz", "fizeste", "fez", "fizemos", "fizeram",
                "leio", "lês", "lê", "lemos", "leem" }), StringComparer.OrdinalIgnoreCase);
    }

    private static readonly Regex RecoverableProgressiveSubject = new(
        Before + @"(?<link>quando|enquanto|porque|se)(?<space>\s+)eu\s+(?<verb>estou)(?=\s+a\s+\p{L}+(?:ar|er|ir)\b)" +
        @"(?!\s+a\s+\p{L}+(?:ar|er|ir)\s+(?:e\s+(?:tu|ele|ela|nós|vocês|eles|elas)|mas)\b)", Ci);

    public static string NaturalWordOrder(string text)
    {
        text = HabitualAlways.Replace(text, m => HabitualVerbs.Contains(m.Groups["v"].Value)
            ? m.Groups["head"].Value + CaseHelper.MatchCase(m.Groups["adv"].Value, m.Groups["v"].Value) +
              m.Groups["space"].Value + (CaseHelper.IsAllUpper(m.Groups["adv"].Value) ? "SEMPRE" : "sempre")
            : m.Value);
        return RecoverableProgressiveSubject.Replace(text, m =>
            m.Groups["link"].Value + m.Groups["space"].Value + m.Groups["verb"].Value);
    }

    private const string MeasurementNumber = @"(?:\d+(?:[,.]\d+)?|(?:um|uma|dois|duas|três|quatro|cinco|seis|sete|oito|nove|" +
        @"dez|onze|doze|treze|catorze|quinze|dezasseis|dezassete|dezoito|dezanove|vinte|trinta|quarenta|cinquenta|" +
        @"sessenta|setenta|oitenta|noventa|cem|cento)(?:\s+e\s+(?:um|uma|dois|duas|três|quatro|cinco|seis|sete|oito|nove|" +
        @"dez|vinte|trinta|quarenta|cinquenta|sessenta|setenta|oitenta|noventa)){0,2})";
    private static readonly Regex PersonMeasurement = new(
        Before + @"(?:suspeit[oa]s?|indivíduos?|homem|homens|mulher(?:es)?|rapa(?:z|zes|rigas?)|crianças?|" +
        @"adolescentes?|adult[oa]s?|idos[oa]s?|menin[oa]s?|vítimas?|pessoas?|" +
        @"sujeit[oa]s?)(?:\s+(?:branc[oa]s?|negr[oa]s?|moren[oa]s?|lour[oa]s?|alt[oa]s?|baix[oa]s?|magr[oa]s?|gord[oa]s?)){0,2}" +
        @"\s+(?<pre>de)(?=\s+(?:(?:cerca\s+de|por\s+volta\s+de|mais\s+de|menos\s+de|aproximadamente|" +
        @"sensivelmente|uns|umas)\s+)?" + MeasurementNumber + @"\s*(?:metros?|centímetros?|cm|m|quilogramas?|quilos?|kg)" +
        @"(?![\p{L}\p{M}\d-])(?:\s+de\s+(?:altura|peso))?(?:\s*(?:$|[.,;:!?…]|\r?\n)|\s+(?:e|com)\b))", Ci);

    public static string PhysicalDescriptions(string text) => PersonMeasurement.Replace(text, m =>
        m.Value[..^m.Groups["pre"].Length] + CaseHelper.MatchCase(m.Groups["pre"].Value, "com"));

    private static readonly Regex InformationTopic = new(
        @"^(?:(?:tenho|temos|há|são|e|quais\s+são)\s+)?(?:(?:o|a|os|as|um|uma|uns|umas|ess[ea]s?|est[ea]s?)\s+)?" +
        @"(?:(?:boas?|más?|últimas?|novas?)\s+)?(?:notícias|novidades|informações|detalhes|verdade|explicação)" +
        @"\s*[.!?…]?$", Ci);
    private static readonly Regex GiveInformationReference = new(
        @"(?:^|[.!?]\s+)(?<request>gimme|give\s+(?:it\s+to\s+me|me))" +
        @"(?<explicit>\s+(?:the\s+)?(?:(?:good|bad)\s+)?(?:news|details|truth|information|explanation|update))?" +
        @"(?:\s+straight)?[.!…]?$", Ci);
    private static readonly Regex BareGiveMe = new(
        @"(?<head>(?:^|[.!?…]\s*|\n)[\uE000-\uF8FF \t]*(?:[-–—]\s*)?)" +
        @"(?<verb>dê|dá|deem|dai)-me(?=[.!…](?:\s|[\uE000-\uF8FF]|$)|\s*[\uE000-\uF8FF]*$)", Ci);
    private static readonly Regex JustKnow = new(
        @"(?<![\p{L}\p{M}])(?<just>só)(?<gap>\s+)sei(?=\s*[.!…]\s*(?:diga|diz|digam|dizei)-me\b)", Ci);

    public static string SpokenInformation(string text, string? previous, ReferenceHint? reference)
    {
        var english = reference?.EnglishText;
        if (english == null) return text;
        var request = GiveInformationReference.Match(english);
        if (!request.Success || !request.Groups["explicit"].Success &&
            !InformationTopic.IsMatch(ReferenceAligner.Clean(previous ?? ""))) return text;
        if (BareGiveMe.Matches(text).Count != 1) return text;
        text = BareGiveMe.Replace(text, m => m.Groups["head"].Value + CaseHelper.MatchCase(m.Groups["verb"].Value,
            m.Groups["verb"].Value.ToLowerInvariant() switch
            {
                "dê" => "diga", "dá" => "diz", "deem" => "digam", _ => "dizei",
            }) + (CaseHelper.IsAllUpper(m.Groups["verb"].Value) ? "-ME" : "-me"));
        if (Regex.IsMatch(english, @"(?:^|[.!?]\s+)i\s+just\s+know[.!…]\s+", Ci))
            text = JustKnow.Replace(text, m =>
                CaseHelper.MatchCase(m.Groups["just"].Value, "simplesmente") + m.Groups["gap"].Value +
                (CaseHelper.IsAllUpper(m.Value) ? "SEI" : "sei"));
        return text;
    }

    private static readonly Regex SharedResidenceReference = new(
        @"^(?:(?:hey|hi|hello|well)[,!]?\s+)?we\s+(?<verb>live|lived)\s+" +
        @"(?:on|in)\s+the\s+same\s+(?<place>floor|building|street|block|neighbou?rhood)[.!…]?$", Ci);
    private static readonly Regex NeighbourResidence = new(
        @"^(?<head>[\uE000-\uF8FF\s]*(?:(?:olá|ei|olha|bem)[,!]?\s+)?)" +
        @"(?<clause>(?:(?:você|tu)\s+)?(?<present>é|és)\s+(?:[oa]\s+)?(?:meu|minha|noss[oa])\s+vizinh[oa]|" +
        @"(?:eu\s+)?(?<present>sou)\s+(?:[oa]\s+)?(?:teu|tua|seu|sua|voss[oa])\s+vizinh[oa]|" +
        @"(?:(?:nós)\s+)?(?<present>somos)\s+vizinh[oa]s|" +
        @"(?:(?:você|tu)\s+)?(?<past>era|eras)\s+(?:[oa]\s+)?(?:meu|minha|noss[oa])\s+vizinh[oa]|" +
        @"(?:eu\s+)?(?<past>era)\s+(?:[oa]\s+)?(?:teu|tua|seu|sua|voss[oa])\s+vizinh[oa]|" +
        @"(?:(?:nós)\s+)?(?<past>éramos)\s+vizinh[oa]s)" +
        @"\s+(?<place>de\s+andar|d[oa]\s+mesm[oa]\s+(?:andar|prédio|edifício|rua|quarteirão|bairro))" +
        @"(?<end>[.!…]?[\uE000-\uF8FF\s]*)$", Ci);

    public static string SharedResidence(string text, ReferenceHint? reference)
    {
        var english = SharedResidenceReference.Match(reference?.EnglishText ?? "");
        if (!english.Success) return text;
        var place = english.Groups["place"].Value.ToLowerInvariant() switch
        {
            "floor" => ("andar", "no mesmo andar"),
            "building" => ("prédio|edifício", "no mesmo edifício"),
            "street" => ("rua", "na mesma rua"),
            "block" => ("quarteirão", "no mesmo quarteirão"),
            _ => ("bairro", "no mesmo bairro"),
        };
        var past = english.Groups["verb"].Value.Equals("lived", StringComparison.OrdinalIgnoreCase);
        return NeighbourResidence.Replace(text, m =>
        {
            if (m.Groups["past"].Success != past ||
                !Regex.IsMatch(m.Groups["place"].Value, @"\b(?:" + place.Item1 + @")$", Ci)) return m.Value;
            var wording = (past ? "vivíamos " : "vivemos ") + place.Item2;
            return m.Groups["head"].Value + CaseHelper.MatchCase(m.Groups["clause"].Value, wording) +
                m.Groups["end"].Value;
        });
    }

    private static readonly Regex RememberClause = new(
        Before + @"(?<v>" + FinitePattern("lembrar") + @")(?:-(?<cl>me|te|se|nos|vos))?(?<space>\s+)" +
        @"(?<what>o\s+que|a\s+que|os\s+que|as\s+que|aquilo\s+que|isso|isto|aquilo)" + After, Ci);
    private static readonly Regex DatePerson = new(
        Before + @"(?<v>" + Alternatives(Forms("namorar").Where((_, i) => i is not (35 or 36 or 37 or 38))) + @")" +
        @"(?<space>\s+)(?<p>ele|ela|eles|elas|você|vocês|tu|mim|ti)" + After, Ci);
    private static readonly Regex DirectRememberName = new(
        Before + @"(?<v>" + FinitePattern("lembrar") + @")\s+(?<p>o|a)\s+(?=(?:nome|endereço|morada|número|data|hora|senha|código)\b)", Ci);

    public static string Government(string text)
    {
        text = RememberClause.Replace(text, m =>
            m.Groups["v"].Value + (m.Groups["cl"].Success ? "-" + m.Groups["cl"].Value : "") +
            m.Groups["space"].Value + ContractDe(m.Groups["what"].Value));
        text = DirectRememberName.Replace(text, m => m.Groups["v"].Value + " " +
            CaseHelper.MatchCase(m.Groups["p"].Value, m.Groups["p"].Value.Equals("o", StringComparison.OrdinalIgnoreCase) ? "do" : "da") + " ");
        return DatePerson.Replace(text, m =>
        {
            var p = m.Groups["p"].Value;
            var target = p.ToLowerInvariant() switch { "mim" => "comigo", "ti" or "tu" => "contigo", _ => "com " + p };
            return m.Groups["v"].Value + m.Groups["space"].Value + target;
        });
    }

    private static string ContractDe(string phrase) => phrase.ToLowerInvariant() switch
    {
        "o que" => CaseHelper.MatchCase(phrase, "do que"),
        "a que" => CaseHelper.MatchCase(phrase, "da que"),
        "os que" => CaseHelper.MatchCase(phrase, "dos que"),
        "as que" => CaseHelper.MatchCase(phrase, "das que"),
        "isso" => CaseHelper.MatchCase(phrase, "disso"),
        "isto" => CaseHelper.MatchCase(phrase, "disto"),
        "aquilo" => CaseHelper.MatchCase(phrase, "daquilo"),
        _ => "d" + phrase,
    };

    private static readonly Regex Personal = new(
        Before + @"(?<pre>antes\s+de|depois\s+de|apesar\s+de|em\s+vez\s+de|para|por|sem|até|após|ao|" +
        @"é\s+(?:melhor|pior|importante|preciso|necessário|bom|difícil|fácil))\s+" +
        @"(?<subject>você|tu|vocês|nós|eles|elas)\s+(?<adv>" + Adverbs + @")(?:(?<cl>me|te|se|nos|vos|lhe|lhes)\s+)?" +
        @"(?<inf>\p{L}+(?:ar|er|ir)|pôr)" + After, Ci);
    private static readonly Regex PersonalContinuation = new(
        Before + @"(?<head>(?:antes\s+de|depois\s+de|sem|para)\s+(?:você|tu)\s+" + Adverbs +
        @"(?:(?:me|te|se|nos|vos)\s+)?(?<inf>\p{L}+(?:ar|er|ir)|pôr)" + After + @",\s+)" +
        @"(?<neg>não\s+)?(?:(?<cl>me|nos|lhe|lhes|se)\s+)?(?<v>\p{L}+)" + After, Ci);

    public static string PersonalInfinitives(string text, bool informal)
    {
        if (informal)
            text = PersonalContinuation.Replace(text, m =>
            {
                if (!PortugueseVerbs.IsKnownInfinitive(m.Groups["inf"].Value)) return m.Value;
                var verb = m.Groups["v"].Value;
                string? target;
                if (ControlledVerbs.Contains(verb))
                {
                    if (m.Groups["cl"].Value.Equals("se", StringComparison.OrdinalIgnoreCase)) return m.Value;
                    var following = Regex.Match(text[(m.Index + m.Length)..], @"^\s+(?<inf>\p{L}+)").Groups["inf"].Value;
                    target = ControlledActions.Contains(following) && Second(verb, out var second) ? second : null;
                }
                else
                {
                    target = PortugueseConverter.TuImperative.ToTu(verb);
                    if (target != null && m.Groups["neg"].Success)
                        target = Second(verb, out var second) ? second : null;
                }
                if (target == null) return m.Value;
                return m.Groups["head"].Value + m.Groups["neg"].Value +
                       (m.Groups["cl"].Success ? m.Groups["cl"].Value + " " : "") + CaseHelper.MatchCase(verb, target);
            });
        return Personal.Replace(text, m =>
        {
            var infinitive = m.Groups["inf"].Value;
            if (!PortugueseVerbs.IsKnownInfinitive(infinitive) &&
                !(PortugueseVerbs.IsInfinitive(infinitive) && NaturaDictionary.Load(true).Check(infinitive))) return m.Value;
            var subject = m.Groups["subject"].Value.ToLowerInvariant();
            if (!informal && subject is ("você" or "tu")) return m.Value;
            var ending = subject switch { "você" or "tu" => "es", "nós" => "mos", _ => "em" };
            var personal = PortugueseVerbs.PersonalInfinitive(infinitive, ending);
            var clitic = m.Groups["cl"].Value;
            if (clitic.Equals("se", StringComparison.OrdinalIgnoreCase))
                clitic = subject switch { "você" or "tu" => "te", "nós" => "nos", _ => "se" };
            var keepSubject = subject is "eles" or "elas" ? subject + " " : "";
            return m.Groups["pre"].Value + " " + keepSubject + m.Groups["adv"].Value +
                   (clitic.Length > 0 ? clitic + " " : "") + CaseHelper.MatchCase(infinitive, personal);
        });
    }

    private static readonly Regex ControlledClause = new(
        Before + @"(?<subject>você|tu)\s+(?<headAdv>" + Adverbs + @")(?<head>\p{L}+)\s+que\s+" +
        @"(?<tail>" + Adverbs + @")(?:(?<cl>se|me|te|nos|lhe|lhes)\s+)?(?<v>\p{L}+)" + After, Ci);
    private static readonly HashSet<string> ControlHeads = new(
        new[] { "achar", "pensar", "acreditar", "saber" }.SelectMany(Forms), StringComparer.OrdinalIgnoreCase);
    private static readonly HashSet<string> ControlledVerbs = new(
        new[] { "pode", "podia", "poderia", "puder", "pudesse", "consegue", "conseguia", "conseguiria",
            "deve", "devia", "deveria", "precisa", "precisava", "precisaria", "quer", "queria", "quereria",
            "vai", "iria", "teria", "tinha" }, StringComparer.OrdinalIgnoreCase);
    private static readonly HashSet<string> ControlledActions = new(new[]
    {
        "resolver", "ajudar", "fazer", "conseguir", "terminar", "acabar", "encontrar", "salvar", "ganhar", "vencer",
        "entrar", "sair", "voltar", "vir", "ir", "trabalhar", "estudar", "conduzir", "dirigir", "pagar", "comprar",
        "vender", "explicar", "contar", "dizer", "falar", "esconder", "fugir", "escapar", "lutar", "mudar",
    }, StringComparer.OrdinalIgnoreCase);
    private static readonly Regex ConditionalTail = new(
        Before + @"(?<pre>se\s+(?:você|tu)\s+" + Adverbs + @")(?:se\s+)?(?<v>\p{L}+)(?<gap>[^.!?;:\n]{0,100},\s*)" +
        @"(?<adv>" + Adverbs + @")(?:(?<cl>me|te|se|nos|lhe|lhes)\s+)?(?<tail>\p{L}+)" + After, Ci);
    private static readonly Regex ExplicitReferenceClause = new(
        Before + @"(?<pre>(?:você|tu)\s+" + Adverbs + @")(?:disse|sabia|percebeu|achou|pensou|descobriu)\s+que\s+" +
        @"(?<adv>" + Adverbs + @")(?:(?<cl>se|me|te|nos|lhe|lhes)\s+)?(?<v>\p{L}+)" + After, Ci);

    private static readonly Regex ExtractedQuestionBridge = new(
        Before + @"(?:quem|o\s+que|(?:qual|quais)(?:\s+\p{L}+){0,2})\s+(?:(?:você|tu)\s+)?" + Adverbs +
        @"(?:ach(?:a|as|ava|avas|ou|aste|aria|arias)|pens(?:a|as|ava|avas|ou|aste|aria|arias)|" +
        @"acredit(?:a|as|ava|avas|ou|aste|aria|arias)|imagin(?:a|as|ava|avas|ou|aste|aria|arias)|" +
        @"supõe|supões|supunha|supunhas|diz|dizes|disse|disseste|diria|dirias|sabe|sabes|sabia|sabias)" +
        @"\s+que\s+" + Adverbs + @"$", Ci);
    private static readonly Regex EnglishRepeatedQuestionSubject = new(
        @"\b(?:who|(?:what|which)(?:\s+\p{L}+){0,2})\s+" +
        @"(?:(?:do|did|would|could|can|will)\s+)?you\s+" +
        @"(?:(?:really|actually|honestly|still|just|not|don't|didn't)\s+){0,3}" +
        @"(?:think|thought|believe|believed|imagine|imagined|suppose|supposed|say|said|know|knew)\s+" +
        @"(?:that\s+)?you\b", Ci);

    internal static bool KeepsExtractedQuestionSubject(string beforeVerb, ReferenceHint? reference) =>
        ExtractedQuestionBridge.IsMatch(beforeVerb) &&
        !EnglishRepeatedQuestionSubject.IsMatch(reference?.EnglishText ?? "");

    private static bool Second(string word, out string second) =>
        PortugueseVerbs.TryThirdToSecond(word, out second) || PortugueseVerbs.TryGuessThirdToSecond(word, out second);

    public static string Clauses(string text, ReferenceHint? reference)
    {
        text = ControlledClause.Replace(text, m =>
        {
            var head = m.Groups["head"].Value;
            var verb = m.Groups["v"].Value;
            if (!ControlHeads.Contains(head) || !ControlledVerbs.Contains(verb) || !Second(verb, out var second)) return m.Value;
            if (KeepsExtractedQuestionSubject(text[..m.Groups["v"].Index], reference)) return m.Value;
            // Explicitly different English subjects block even a plausible subject-continuity reading.
            if (reference is { ThirdPerson: true, RepeatedYouSubject: false }) return m.Value;
            if (m.Groups["cl"].Value.Equals("se", StringComparison.OrdinalIgnoreCase)) return m.Value;
            var following = Regex.Match(text[(m.Index + m.Length)..], @"^\s+(?<inf>\p{L}+)").Groups["inf"].Value;
            if (!ControlledActions.Contains(following) ||
                reference is not { RepeatedYouSubject: true } &&
                (!Regex.IsMatch(text[(m.Index + m.Length)..], @"^[^.!?;]*\?") ||
                 verb.ToLowerInvariant() is not ("pode" or "consegue" or "deve" or "precisa" or "quer"))) return m.Value;
            return m.Value[..(m.Groups["v"].Index - m.Index)] + second;
        });
        text = ConditionalTail.Replace(text, m =>
        {
            var verb = m.Groups["v"].Value;
            var tail = m.Groups["tail"].Value;
            if (Regex.IsMatch(m.Groups["gap"].Value, Before + @"(?:ele|ela|eles|elas|nós|eu|você|vocês)\b|(?-i:\p{Lu}\p{Ll}+)", Ci))
                return m.Value;
            if (!verb.EndsWith("sse", StringComparison.OrdinalIgnoreCase) && verb.ToLowerInvariant() is not ("fosse" or "tivesse" or "pudesse")) return m.Value;
            if (!PortugueseVerbs.TrySplitFuture(tail, out _, out var ending, false) ||
                !ending.Equals("ia", StringComparison.OrdinalIgnoreCase) || reference is { ThirdPerson: true } ||
                !Second(tail, out var second)) return m.Value;
            return m.Value[..(m.Groups["tail"].Index - m.Index)] + second;
        });
        if (reference is { RepeatedYouSubject: true })
            text = ExplicitReferenceClause.Replace(text, m =>
                !KeepsExtractedQuestionSubject(text[..m.Groups["v"].Index], reference) &&
                Second(m.Groups["v"].Value, out var second)
                ? m.Value[..(m.Groups["v"].Index - m.Index)] + second : m.Value);
        return text;
    }

    private static readonly Dictionary<string, string> TurnIntoForms = MapForms("virar", "tornar");
    private static readonly Regex Become = new(
        Before + @"(?<v>" + Alternatives(TurnIntoForms.Keys) + @")(?<space>\s+)" +
        @"(?=(?:(?:um|uma|uns|umas)\s+)?(?:médic[oa]s?|advogad[oa]s?|políci[ao]s?|professor(?:a|es|as)?|" +
        @"detetives?|engenheir[oa]s?|enfermeir[oa]s?|presidente|ministro|ministra|criminos[oa]s?|assassin[oa]s?|" +
        @"amig[oa]s?|inimig[oa]s?|herói|heroína|chefes?|problema|pesadelo|hábito|rotina|obsessão)\b)", Ci);
    private static readonly Dictionary<string, string> FaceForms = MapForms("encarar", "enfrentar");
    private static readonly Regex FaceProblem = new(
        Before + @"(?<v>" + Alternatives(FaceForms.Keys) + @")(?<space>\s+)" +
        @"(?=(?:(?:o|a|os|as|um|uma|este|esta|esse|essa|teu|tua|seu|sua)\s+){0,2}" +
        @"(?:problemas?|realidade|verdade|consequências|riscos?|desafios?|situação|medos?|dificuldades|responsabilidades)\b)", Ci);
    private static readonly Dictionary<string, string> StareForms = MapForms("encarar", "olhar");
    private static readonly Regex StarePerson = new(
        Before + @"(?<v>" + Alternatives(StareForms.Keys) + @")\s+(?<p>você|vocês|ele|ela|eles|elas|mim|ti)" + After, Ci);
    private static readonly Regex Tasty = new(
        Before + @"(?<food>(?:o|a|este|esta|esse|essa)\s+(?:bolo|sopa|comida|jantar|almoço|peixe|carne|pão|café|chá|molho|chocolate))" +
        @"(?<middle>\s+(?:está|estava|é|ficou|parece)\s+(?:muito\s+|mesmo\s+|tão\s+)?)(?<adj>gostoso|gostosa)" + After, Ci);
    private static readonly Dictionary<string, string> EnjoyForms = MapForms("curtir", "gostar");
    private static readonly Regex EnjoyThing = new(
        Before + @"(?<v>" + Alternatives(EnjoyForms.Keys) + @")\s+(?<det>o|a|os|as|este|esta|esse|essa)\s+" +
        @"(?=(?:filmes?|músicas?|canções|livros?|concertos?|séries?|espetáculos?|jogos?|ideia|viagem|festa)\b)", Ci);
    private static readonly Dictionary<string, string> ComplainForms = MapForms("reclamar", "queixar");
    private static readonly Regex ComplainAbout = new(
        Before + @"(?<v>" + Alternatives(ComplainForms.Keys) + @")\s+(?<p>de|do|da|dos|das|disso|disto|daquilo)" + After, Ci);
    private static readonly Dictionary<string, string> FetchForms = MapForms("pegar", "buscar");
    private static readonly Regex CollectPerson = new(
        Before + @"(?<v>" + Alternatives(FetchForms.Keys) + @")\s+(?<p>você|vocês|ele|ela|eles|elas)\s+" +
        @"(?<where>no|na)\s+(?<place>estação|aeroporto|escola|hotel|hospital|trabalho)" + After, Ci);
    private static readonly Regex HitBodyPart = new(
        Before + @"(?<v>" + Alternatives(Forms("acertar").Where((_, index) => index is not (35 or 36 or 37 or 38))) + @")\s+" +
        @"(?<det>a|o|as|os)\s+(?<part>cabeça|cara|rosto|testa|nuca|boca|nariz|queixo|olho|olhos|ouvido|ouvidos|orelha|orelhas|pescoço|garganta|peito|costas|barriga|estômago|ombro|ombros|braço|braços|perna|pernas|joelho|joelhos|mão|mãos|pé|pés)" +
        @"(?<owner>\s+d(?:e|o|a|os|as)\s+(?:ele|ela|eles|elas|mim|ti|nós|você|vocês|tu|outro(?:s|as)?(?:\s+\p{L}+){0,2}|mesm[oa]s?|rapaz|rapariga|tipo|gajo|homem|mulher|suspeit[oa]s?|agente|detetive|polícia|(?-i:\p{Lu})[\p{L}\p{M}'’-]*))?" + After, Ci);
    private static readonly Dictionary<string, string> UploadFileForms = MapForms("subir", "carregar")
        .Where(pair => pair.Key is not ("sube" or "subes" or "subem"))
        .Concat(new Dictionary<string, string> { ["sobe"] = "carrega", ["sobes"] = "carregas", ["sobem"] = "carregam" })
        .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, string> DigitalFileForms = MapForms("baixar", "descarregar")
        .Concat(MapForms("salvar", "guardar")).Concat(UploadFileForms)
        .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.OrdinalIgnoreCase);
    private static readonly Regex DigitalFile = new(
        Before + @"(?<v>" + Alternatives(DigitalFileForms.Keys) + @")(?<space>\s+)" +
        @"(?<det>(?:(?:o|os|um|uns|este|estes|esse|esses|aquele|aqueles|meu|meus|teu|teus|seu|seus|novo|novos)\s+){0,3})" +
        @"(?<noun>arquivos?|ficheiros?|documentos?)" + After, Ci);
    private static readonly Dictionary<string, string> DataEntryForms = MapForms("digitar", "introduzir")
        .ToDictionary(pair => pair.Key, pair => pair.Key == "digita" ? "introduz" : pair.Value, StringComparer.OrdinalIgnoreCase);
    private static readonly Regex DataEntry = new(
        Before + @"(?<v>" + Alternatives(DataEntryForms.Keys) + @")(?=\s+" +
        @"(?:(?:o|a|os|as|um|uma|uns|umas|este|esta|estes|estas|esse|essa|esses|essas)\s+)?" +
        @"(?:senhas?|palavras?-passe|códigos?|dados|números?)" + After + ")", Ci);

    private static Dictionary<string, string> MapForms(string source, string target)
    {
        var from = Forms(source);
        var to = Forms(target);
        return from.Select((form, i) => (form, target: to[i])).GroupBy(pair => pair.form, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First().target, StringComparer.OrdinalIgnoreCase);
    }

    private static string Reflexive(string text, Match match, string verb, bool informal)
    {
        var before = Regex.Match(text[..match.Index], @"[^.!?;:]*$").Value;
        var subjects = Regex.Matches(before, Before + @"(?<s>eu|tu|você|nós|vocês|eles|elas|ele|ela)\b", Ci);
        var subject = subjects.Count > 0 ? subjects[^1].Groups["s"].Value.ToLowerInvariant() : "";
        var lower = verb.ToLowerInvariant();
        var previous = Regex.Match(before, @"\p{L}+\s*$").Value.Trim().ToLowerInvariant();
        if (subject.Length == 0 && ReflexiveAmbiguous.Contains(lower))
        {
            subject = previous switch
            {
                "vou" or "posso" or "quero" or "estou" => "eu",
                "vais" or "podes" or "queres" or "estás" => "tu",
                "vamos" or "podemos" or "queremos" or "estamos" => "nós",
                "vai" or "pode" or "quer" or "está" or "vão" or "podem" or "querem" or "estão" => "ele",
                _ => "",
            };
            if (subject.Length == 0) return match.Groups["v"].Value;
        }
        var clitic = subject switch
        {
            "eu" => "me", "tu" => "te", "você" when informal => "te", "nós" => "nos",
            "" when ReflexiveFirst.Contains(lower) => "me",
            "" when lower.EndsWith("mos", StringComparison.OrdinalIgnoreCase) => "nos",
            "" when informal && ReflexiveSecond.Contains(lower) => "te",
            _ => "se",
        };
        if (previous is "me" or "te" or "se" or "nos" or "vos") return verb;
        if (CaseHelper.IsAllUpper(verb)) clitic = clitic.ToUpperInvariant();
        if (informal && subject is ("você" or "tu")) return clitic + " " + verb;
        if (Regex.IsMatch(before, Before + @"(?:não|nunca|já|sempre|também|que|se|talvez|quando)\b[^,]*$", Ci))
            return clitic + " " + verb;
        return PortugueseVerbs.AttachMesoclitic(verb, clitic, false) ?? PortugueseVerbs.AttachEnclitic(verb, clitic);
    }

    private static readonly HashSet<string> ReflexiveFirst = new(new[] { "tornar", "queixar" }.SelectMany(Forms)
        .Where(f => f.EndsWith('o') || f.EndsWith("ei", StringComparison.OrdinalIgnoreCase)), StringComparer.OrdinalIgnoreCase);
    private static readonly HashSet<string> ReflexiveSecond = new(new[] { "tornar", "queixar" }
        .SelectMany(infinitive => Forms(infinitive).Where((_, index) => index is 2 or 7 or 11 or 15 or 20 or 24 or 28 or 31 or 40)),
        StringComparer.OrdinalIgnoreCase);
    private static readonly HashSet<string> ReflexiveAmbiguous = new(new[] { "tornar", "queixar" }
        .SelectMany(infinitive => Forms(infinitive).Where((_, index) => index is 0 or 10 or 19 or 23 or 27 or 34 or 39)),
        StringComparer.OrdinalIgnoreCase);

    public static string Vocabulary(string text, bool pronouns, bool informal)
    {
        text = DataEntry.Replace(text, m => CaseHelper.MatchCase(m.Value, DataEntryForms[m.Value]));
        text = DigitalFile.Replace(text, m =>
        {
            var verb = m.Groups["v"].Value;
            var noun = m.Groups["noun"].Value;
            if (UploadFileForms.ContainsKey(verb) && noun.StartsWith("documento", StringComparison.OrdinalIgnoreCase) &&
                !Regex.IsMatch(m.Result("$'"), @"^\s+(?:para|em|no|na)\s+(?:(?:o|a|um|uma)\s+)?(?:servidor|plataforma|site|nuvem|portal)\b", Ci))
                return m.Value;
            return CaseHelper.MatchCase(verb, DigitalFileForms[verb]) + m.Groups["space"].Value + m.Groups["det"].Value +
                (noun.StartsWith("arquivo", StringComparison.OrdinalIgnoreCase)
                    ? CaseHelper.MatchCase(noun, noun.EndsWith("s", StringComparison.OrdinalIgnoreCase)
                        ? "ficheiros" : "ficheiro") : noun);
        });
        if (pronouns)
        {
            text = Become.Replace(text, m => Reflexive(text, m,
                CaseHelper.MatchCase(m.Groups["v"].Value, TurnIntoForms[m.Groups["v"].Value]), informal) + m.Groups["space"].Value);
        }
        text = FaceProblem.Replace(text, m => CaseHelper.MatchCase(m.Groups["v"].Value, FaceForms[m.Groups["v"].Value]) + m.Groups["space"].Value);
        text = StarePerson.Replace(text, m => CaseHelper.MatchCase(m.Groups["v"].Value, StareForms[m.Groups["v"].Value]) +
            " fixamente para " + m.Groups["p"].Value);
        text = Tasty.Replace(text, m => m.Groups["food"].Value + m.Groups["middle"].Value +
            CaseHelper.MatchCase(m.Groups["adj"].Value, m.Groups["adj"].Value.EndsWith("a", StringComparison.OrdinalIgnoreCase) ? "saborosa" : "saboroso"));
        text = EnjoyThing.Replace(text, m =>
        {
            var det = m.Groups["det"].Value.ToLowerInvariant();
            var preposition = det switch { "o" => "do", "a" => "da", "os" => "dos", "as" => "das", _ => "d" + det };
            return CaseHelper.MatchCase(m.Groups["v"].Value, EnjoyForms[m.Groups["v"].Value]) + " " + preposition + " ";
        });
        text = HitBodyPart.Replace(text, m =>
        {
            var tail = text[(m.Index + m.Length)..];
            if (m.Groups["owner"].Length == 0 && Regex.IsMatch(tail, @"^\s+d(?:e|o|a|os|as)\s+\p{L}+", Ci)) return m.Value;
            return m.Groups["v"].Value + " " +
                   CaseHelper.MatchCase(m.Groups["det"].Value, m.Groups["det"].Value.ToLowerInvariant() switch
                   { "a" => "na", "o" => "no", "as" => "nas", _ => "nos" }) + " " + m.Groups["part"].Value + m.Groups["owner"].Value;
        });
        return CollectPerson.Replace(text, m => CaseHelper.MatchCase(m.Groups["v"].Value, FetchForms[m.Groups["v"].Value]) +
            " " + m.Groups["p"].Value + " " + (m.Groups["where"].Value.Equals("na", StringComparison.OrdinalIgnoreCase) ? "à" : "ao") +
            " " + m.Groups["place"].Value);
    }

    public static string Complaints(string text, bool informal) => ComplainAbout.Replace(text, m =>
        Reflexive(text, m, CaseHelper.MatchCase(m.Groups["v"].Value, ComplainForms[m.Groups["v"].Value]), informal) +
        " " + m.Groups["p"].Value);

    private static readonly Regex FutureMesoclisis = new(
        Before + @"(?<root>\p{L}+)-(?<cl>me|te|se|nos|vos|lhe|lhes|lo|la|los|las)-(?<ending>ei|ás|á|emos|ão)" + After, Ci);
    private static readonly Regex RedundantSubject = new(
        @"(?<pre>^|[.!?]\s+)(?<subject>eu|nós)\s+(?<adv>" + Adverbs + @")(?:(?<cl>me|nos)\s+)?(?<v>\p{L}+)(?<attached>-(?:me|nos|te|o|a|os|as|lhe|lhes))?" + After, Ci);
    private static readonly HashSet<string> FirstSingular = new(StringComparer.OrdinalIgnoreCase)
    {
        "sou", "estou", "tenho", "vou", "posso", "quero", "sei", "faço", "digo", "vejo", "ouço", "peço",
        "fui", "estive", "tive", "fiz", "vi", "vim", "amo", "gosto", "lembro", "esqueço", "preciso",
        "penso", "acho", "acredito", "espero", "sinto", "durmo", "venho", "trago", "leio", "falo",
        "trabalho", "estudo", "ajudo", "encontro", "chamo",
    };
    private static readonly HashSet<string> FirstPlural = new(StringComparer.OrdinalIgnoreCase)
    {
        "somos", "estamos", "temos", "vamos", "podemos", "queremos", "sabemos", "fazemos", "dizemos", "vemos", "ouvimos",
        "fomos", "estivemos", "tivemos", "fizemos", "vimos", "dissemos",
    };

    public static string Conversational(string text, ReferenceHint? reference)
    {
        if (reference is { FormalAddress: true } || PortugueseConverter.YouToTuRules.IsFormal(text)) return text;
        text = FutureMesoclisis.Replace(text, m =>
        {
            var root = m.Groups["root"].Value.ToLowerInvariant();
            var clitic = m.Groups["cl"].Value.ToLowerInvariant();
            var infinitive = root switch { "dir" or "di" => "dizer", "far" or "fá" => "fazer", "trar" or "trá" => "trazer", _ => root };
            if (clitic.StartsWith('l') && !infinitive.EndsWith('r'))
                infinitive = root.TrimEnd('á', 'ê') + (root.EndsWith('á') ? "ar" : root.EndsWith('ê') ? "er" : "r");
            if (!PortugueseVerbs.IsKnownInfinitive(infinitive)) return m.Value;
            var auxiliary = m.Groups["ending"].Value.ToLowerInvariant() switch
            { "ei" => "vou", "ás" => "vais", "á" => "vai", "emos" => "vamos", _ => "vão" };
            return CaseHelper.MatchCase(m.Value, auxiliary + " " + PortugueseVerbs.AttachEnclitic(infinitive, clitic));
        });
        if (Regex.IsMatch(text, Before + @"(?:mas|porém|enquanto|só|apenas|também|própri[oa]s?|mesm[oa]s?)\b", Ci) ||
            Regex.IsMatch(text, Before + @"(?:ele|ela|eles|elas|tu|vocês|você)\b", Ci)) return text;
        return RedundantSubject.Replace(text, m =>
        {
            var verb = m.Groups["v"].Value;
            var plural = m.Groups["subject"].Value.Equals("nós", StringComparison.OrdinalIgnoreCase);
            var known = plural ? FirstPlural.Contains(verb) || verb.EndsWith("mos", StringComparison.OrdinalIgnoreCase) && PortugueseVerbs.IsKnownVerbForm(verb)
                : FirstSingular.Contains(verb) || verb.EndsWith('o') && PortugueseVerbs.IsKnownVerbForm(verb);
            if (!known) return m.Value;
            var replacement = m.Groups["adv"].Value + (m.Groups["cl"].Success ? m.Groups["cl"].Value + " " : "") +
                              verb + m.Groups["attached"].Value;
            return m.Groups["pre"].Value + CaseHelper.MatchCase(m.Groups["subject"].Value, replacement);
        });
    }

    private static readonly Dictionary<string, string> ChatForms = MapForms("bater", "conversar");
    private static readonly Dictionary<string, string> ClarifyDoubtForms = MapForms("tirar", "esclarecer");
    private static readonly Regex ClarifyDoubt = new(
        Before + @"(?<v>" + Alternatives(ClarifyDoubtForms.Keys) + @")(?<cl>-(?:me|te|lhe|lhes|nos))?" +
        @"(?<space>\s+)(?<det>(?:(?:a|as|uma|umas|esta|estas|essa|essas|minha|minhas|tua|tuas|sua|suas)\s+)?)" +
        @"(?<noun>dúvidas?)" + After, Ci);
    private static readonly Regex MaybeSlang = new(Before + @"se(?<space>\s+)pá" + After, Ci);
    private static readonly Regex LetsGoExpression = new(
        @"(?<head>^(?:[-–—]\s*)?|[.!?]\s+)(?<phrase>vamos\s+nessa)(?=\s*[.!?…]*\s*$)", Ci);
    private static readonly Regex Chat = new(
        Before + @"(?<v>" + Alternatives(ChatForms.Keys) + @")(?<space>\s+)(?:um\s+)?papo" + After, Ci);
    private static readonly Dictionary<string, string> ExchangeIdeaForms = MapForms("trocar", "conversar");
    private static readonly Regex ExchangeIdea = new(
        Before + @"(?<v>" + Alternatives(ExchangeIdeaForms.Keys) + @")(?<space>\s+)uma\s+ideia" + After + @"(?!\s+por\b)", Ci);
    private static readonly Dictionary<string, string> EmbarrassmentForms = MapForms("pagar", "passar");
    private static readonly Regex Embarrassment = new(
        Before + @"(?<v>" + Alternatives(EmbarrassmentForms.Keys) + @")(?<space>\s+)(?:um\s+)?mico" + After, Ci);
    private static readonly Regex Idle = new(
        Before + @"(?<v>" + Alternatives(Forms("ficar")) + @")(?<space>\s+)de\s+bobeira" + After, Ci);
    private static readonly Regex Nonsense = new(
        Before + @"(?<v>" + Alternatives(Forms("falar")) + @")(?<space>\s+)abobrinhas?" + After, Ci);
    private static readonly Dictionary<string, string> DrinkBeerForms = MapForms("tomar", "beber");
    private static readonly Regex DrinkBeer = new(
        Before + @"(?<v>" + Alternatives(DrinkBeerForms.Keys) + @")(?<space>\s+)(?=(?:(?:uma|umas|a|as)\s+)?brejas?\b)", Ci);
    private const string GiveForms = @"dar|dou|dás|dá|damos|dão|dei|deste|deu|demos|deram|dava|davas|dávamos|davam|" +
        @"darei|darás|dará|daremos|darão|daria|darias|daríamos|dariam|dê|dês|deem|desse|desses|déssemos|dessem|der|deres|dermos|derem|dado|dando";
    private static readonly Regex LeisureWalk = new(
        Before + @"(?<v>" + GiveForms + @")(?<space>\s+)(?:(?<article>um|uns)(?<gap>\s+))?(?<noun>rol[êé]s?|rol[eê]zinhos?)" + After, Ci);
    private static readonly Regex GiveHelpOrBreak = new(
        Before + @"(?<v>" + GiveForms + @")(?<cl>-(?:me|te|lhe|lhes|nos))?(?<space>\s+)(?<article>uma)(?<gap>\s+)" +
        @"(?<noun>força|escapada)" + After +
        @"(?!\s+(?:magnética|gravitacional|centrípeta|centrífuga|eletromagnética|de\s+\d+)\b)", Ci);
    private static readonly Dictionary<string, string> StartConversationForms = MapForms("puxar", "meter");
    private static readonly Regex StartConversation = new(
        Before + @"(?<v>" + Alternatives(StartConversationForms.Keys) + @")(?<space>\s+)assunto" + After, Ci);
    private static readonly Dictionary<string, string> SmallTalkForms = MapForms("jogar", "conversar");
    private static readonly Regex SmallTalk = new(
        Before + @"(?<v>" + Alternatives(SmallTalkForms.Keys) + @")\s+conversa\s+fora" + After, Ci);
    private static readonly Regex FillerSpeech = new(
        Before + @"(?<v>" + Alternatives(Forms("encher")) + @")(?<space>\s+)linguiça" + After +
        @"(?!\s+(?:com|de|caseira|recheada)\b)", Ci);
    private static readonly Regex TellThing = new(
        Before + @"(?<v>" + Alternatives(Forms("contar")) + @")(?<cl>-(?:me|te|lhe|lhes|nos))?" +
        @"(?<space>\s+)(?<article>uma|umas)(?<gap>\s+)(?<noun>paradas?)" + After +
        @"(?!\s+(?:cardíacas?|cardiorrespiratórias?|militar(?:es)?|de\s+(?:autocarro|ônibus))\b)", Ci);
    private static readonly Regex FedUp = new(
        Before + @"(?<head>(?:(?<subject>eu|tu|ele|ela|nós|vocês|eles|elas)\s+)?" + Adverbs + @")" +
        @"(?<v>estou|estás|está|estamos|estão|estava|estavas|estávamos|estavam|estar|" + Alternatives(Forms("ficar")) + @")" +
        @"(?<space>\s+)de\s+saco\s+(?<adj>chei[oa]s?)" + After, Ci);
    private static readonly Regex VerbalIntensifier = new(
        Before + @"(?<v>" + Alternatives(new[] { "falar", "trabalhar", "estudar", "correr" }.SelectMany(Forms)) + @")" +
        @"(?<space>\s+)(?:pra|para)\s+caramba" + After, Ci);
    private static readonly Dictionary<string, string> HelpOutForms = MapForms("quebrar", "desenrascar");
    private static readonly Regex HelpOut = new(
        Before + @"(?<v>" + Alternatives(HelpOutForms.Keys) + @")(?<cl>-(?:me|te|lhe|lhes|nos))?\s+(?:um|o)\s+galho" + After, Ci);
    private static readonly Regex BrokeReference = new(
        @"^i(?:'m|\s+am)\s+(?:(?:flat|completely|totally)\s+)?(?:broke|skint)[.!…]?$", Ci);
    private static readonly Regex ExhaustedReference = new(
        @"^i(?:'m|\s+am)\s+(?:(?:absolutely|completely|totally)\s+)?(?:beat|knackered)[.!…]?$", Ci);
    private static readonly Regex BrokenSlang = new(
        @"^(?<head>(?:eu\s+)?estou)(?<space>\s+)(?<adj>quebrad[oa])(?<end>[.!…]?)$", Ci);

    public static string Idioms(string text, ReferenceHint? reference)
    {
        text = ClarifyDoubt.Replace(text, m => CaseHelper.MatchCase(m.Groups["v"].Value, ClarifyDoubtForms[m.Groups["v"].Value]) +
            m.Groups["cl"].Value + m.Groups["space"].Value + m.Groups["det"].Value + m.Groups["noun"].Value);
        text = MaybeSlang.Replace(text, m => CaseHelper.MatchCase(m.Value,
            "se" + m.Groups["space"].Value + "calhar"));
        text = LetsGoExpression.Replace(text, m => m.Groups["head"].Value +
            CaseHelper.MatchCase(m.Groups["phrase"].Value, "vamos a isso"));
        var english = reference?.EnglishText;
        if (english != null)
        {
            var plain = ReferenceAligner.Clean(english).Replace('’', '\'');
            var meaning = BrokeReference.IsMatch(plain) ? "sem dinheiro"
                : ExhaustedReference.IsMatch(plain) ? "de rastos" : null;
            if (meaning != null)
                text = BrokenSlang.Replace(text, m => m.Groups["head"].Value + m.Groups["space"].Value +
                    CaseHelper.MatchCase(m.Groups["adj"].Value, meaning) + m.Groups["end"].Value);
        }
        text = StartConversation.Replace(text, m => CaseHelper.MatchCase(m.Groups["v"].Value, StartConversationForms[m.Groups["v"].Value]) +
            m.Groups["space"].Value + (CaseHelper.IsAllUpper(m.Value) ? "CONVERSA" : "conversa"));
        text = SmallTalk.Replace(text, m => CaseHelper.MatchCase(m.Groups["v"].Value, SmallTalkForms[m.Groups["v"].Value]));
        if (english == null || !Regex.IsMatch(english,
                @"\b(?:stuff(?:s|ed|ing)?|fill(?:s|ed|ing)?)\b[^.!?]{0,40}\bsausages?\b", Ci))
            text = FillerSpeech.Replace(text, m => m.Groups["v"].Value + m.Groups["space"].Value +
                (CaseHelper.IsAllUpper(m.Value) ? "CHOURIÇOS" : "chouriços"));
        text = TellThing.Replace(text, m => m.Groups["v"].Value + m.Groups["cl"].Value + m.Groups["space"].Value +
            m.Groups["article"].Value + m.Groups["gap"].Value + CaseHelper.MatchCase(m.Groups["noun"].Value,
                m.Groups["noun"].Value.EndsWith("s", StringComparison.OrdinalIgnoreCase) ? "coisas" : "coisa"));
        text = GiveHelpOrBreak.Replace(text, m => m.Groups["v"].Value + m.Groups["cl"].Value +
            m.Groups["space"].Value + m.Groups["article"].Value + m.Groups["gap"].Value +
            CaseHelper.MatchCase(m.Groups["noun"].Value, m.Groups["noun"].Value.Equals("força", StringComparison.OrdinalIgnoreCase)
                ? "ajuda" : "escapadela"));
        text = FedUp.Replace(text, m =>
        {
            var subject = m.Groups["subject"].Value.ToLowerInvariant();
            var verb = m.Groups["v"].Value.ToLowerInvariant();
            var feminine = subject is "ela" or "elas" || m.Groups["adj"].Value.Contains("a", StringComparison.OrdinalIgnoreCase);
            var plural = subject is "nós" or "vocês" or "eles" or "elas" || verb.EndsWith('m') ||
                verb.EndsWith("mos", StringComparison.Ordinal) || verb.EndsWith("ão", StringComparison.Ordinal) ||
                m.Groups["adj"].Value.EndsWith("s", StringComparison.OrdinalIgnoreCase);
            return m.Value[..(m.Groups["v"].Index - m.Index)] + m.Groups["v"].Value + m.Groups["space"].Value +
                   CaseHelper.MatchCase(m.Groups["adj"].Value, (feminine ? "farta" : "farto") + (plural ? "s" : ""));
        });
        text = VerbalIntensifier.Replace(text, m => m.Groups["v"].Value + m.Groups["space"].Value +
            (CaseHelper.IsAllUpper(m.Value) ? "IMENSO" : "imenso"));
        text = Chat.Replace(text, m => CaseHelper.MatchCase(m.Groups["v"].Value, ChatForms[m.Groups["v"].Value]));
        text = ExchangeIdea.Replace(text, m => CaseHelper.MatchCase(m.Groups["v"].Value, ExchangeIdeaForms[m.Groups["v"].Value]));
        text = Embarrassment.Replace(text, m => CaseHelper.MatchCase(m.Groups["v"].Value, EmbarrassmentForms[m.Groups["v"].Value]) +
            m.Groups["space"].Value + (CaseHelper.IsAllUpper(m.Value) ? "VERGONHA" : "vergonha"));
        text = Idle.Replace(text, m => m.Groups["v"].Value + m.Groups["space"].Value +
            (CaseHelper.IsAllUpper(m.Value) ? "SEM FAZER NADA" : "sem fazer nada"));
        text = Nonsense.Replace(text, m => m.Groups["v"].Value + m.Groups["space"].Value +
            (CaseHelper.IsAllUpper(m.Value) ? "DISPARATES" : "disparates"));
        text = DrinkBeer.Replace(text, m => CaseHelper.MatchCase(m.Groups["v"].Value, DrinkBeerForms[m.Groups["v"].Value]) +
            m.Groups["space"].Value);
        text = LeisureWalk.Replace(text, m =>
        {
            var noun = m.Groups["noun"].Value;
            var target = noun.Contains("z", StringComparison.OrdinalIgnoreCase) ? "voltinha" : "volta";
            if (noun.EndsWith("s", StringComparison.OrdinalIgnoreCase)) target += "s";
            return m.Groups["v"].Value + m.Groups["space"].Value +
                   CaseHelper.MatchCase(m.Groups["article"].Success ? m.Groups["article"].Value : noun,
                       m.Groups["article"].Value.Equals("uns", StringComparison.OrdinalIgnoreCase) ||
                       !m.Groups["article"].Success && noun.EndsWith("s", StringComparison.OrdinalIgnoreCase) ? "umas" : "uma") +
                   (m.Groups["gap"].Success ? m.Groups["gap"].Value : " ") + CaseHelper.MatchCase(noun, target);
        });
        if (string.IsNullOrWhiteSpace(english)) return text;
        if (Regex.IsMatch(english, @"\b(?:favou?r|help(?:ed|ing)?\s+(?:me|you|him|her|us|them)\s+out|lend\s+(?:(?:me|you|him|her|us|them)\s+)?a\s+hand)\b", Ci) &&
            !Regex.IsMatch(english, @"\b(?:branches|branch|tree|twig|wood)\b", Ci))
            text = HelpOut.Replace(text, m => CaseHelper.MatchCase(m.Groups["v"].Value, HelpOutForms[m.Groups["v"].Value]) +
                m.Groups["cl"].Value);
        if (Regex.IsMatch(english, @"\b(?:stingy|miserly|cheapskates?|tightwads?)\b", Ci))
            text = Regex.Replace(text, Before + @"(?<noun>pão-duro|pães-duros)" + After,
                m => CaseHelper.MatchCase(m.Value, m.Value.StartsWith("pão", StringComparison.OrdinalIgnoreCase) ? "forreta" : "forretas"), Ci);
        if (Regex.IsMatch(english, @"\b(?:cost|costs|costing|costed)\b[^.!?]{0,60}\ban\s+arm\s+and\s+a\s+leg\b", Ci))
            text = Regex.Replace(text, Before + @"(?<v>custa|custou|custava|custam|custaram|custar|custaria|custará|custando)(?<cl>-(?:me|te|nos|lhe|lhes))?\s+(?:um\s+braço\s+e\s+uma\s+perna)", "${v}${cl} os olhos da cara", Ci);
        if (Regex.IsMatch(english, @"\ball\s+ears\b", Ci))
            text = Regex.Replace(text, Before + @"(?<v>estou|estás|está|estamos|estão|estava|estavas|estávamos|estavam)\s+(?<all>tod[oa]s?)\s+ouvidos\b", m =>
                CaseHelper.MatchCase(m.Groups["v"].Value, m.Groups["v"].Value.ToLowerInvariant() switch
                { "estou" => "sou", "estás" => "és", "está" => "é", "estamos" => "somos", "estão" => "são", "estava" => "era", "estavas" => "eras", "estávamos" => "éramos", _ => "eram" }) +
                " " + m.Groups["all"].Value + " ouvidos", Ci);
        if (Regex.IsMatch(english, @"\bon\s+the\s+house\b", Ci))
            text = Regex.Replace(text, Before + @"(?<v>é|era|foi|será)\s+por\s+conta\s+da\s+casa\b", "${v} oferta da casa", Ci);
        if (Regex.IsMatch(english, @"\bonce\s+in\s+a\s+blue\s+moon\b", Ci))
            text = Regex.Replace(text, Before + @"uma\s+vez\s+(?:em|numa)\s+(?:uma\s+)?lua\s+azul\b",
                m => CaseHelper.MatchCase(m.Value, "muito de vez em quando"), Ci);
        if (Regex.IsMatch(english, @"\b(?:spill|spills|spilled|spilling)\s+the\s+beans\b", Ci) &&
            !Regex.IsMatch(english, @"\b(?:floor|plate|pot|table|kitchen|cook|cooking|cooked)\b", Ci) &&
            !Regex.IsMatch(text, Before + @"(?:chão|panela|prato|mesa|cozinha|cozinhar)\b", Ci))
            text = Regex.Replace(text, Before + @"(?<v>derramar|derramou|derramei|derramaram|derrama|derramas|derramam|derramando)\s+os\s+feijões\b", m =>
            {
                var forms = MapForms("derramar", "contar");
                return CaseHelper.MatchCase(m.Groups["v"].Value, forms[m.Groups["v"].Value]) + " tudo";
            }, Ci);
        return text;
    }
}
