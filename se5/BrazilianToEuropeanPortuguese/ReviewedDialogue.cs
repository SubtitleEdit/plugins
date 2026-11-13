using System.Text.RegularExpressions;

namespace SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;

internal static class ReviewedDialogue
{
    private const RegexOptions Ci = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;
    private const string Before = @"(?<![\p{L}\p{M}-])";
    private const string After = @"(?![\p{L}\p{M}-])";
    private static readonly Regex Reminder = new(
        Before + @"(?<verb>(?:re)?lembrar|(?:re)?lembrá-l[oa])(?=\s+que\b)", Ci);

    // "e contou sobre isso" next to "and told her about it": the dropped recipient of a telling verb returns as a dative clitic.
    private static readonly (string Infinitive, string English)[] TellingVerbs =
    {
        ("contar", @"(?:tell|tells|told|telling)"),
        ("dizer", @"(?:tell|tells|told|telling)"),
        ("falar", @"(?:tell|tells|told|telling|(?:speak|speaks|spoke|talk|talks|talked)\s+to)"),
        ("explicar", @"(?:explain|explains|explained)(?:\s+(?:it|this|that|everything))?\s+to"),
        ("mostrar", @"(?:show|shows|showed|shown)"),
        ("escrever", @"(?:write|writes|wrote)\s+to"),
        ("mentir", @"(?:lie|lies|lied)\s+to"),
        ("perguntar", @"(?:ask|asks|asked)"),
        ("prometer", @"(?:promise|promises|promised)"),
        ("avisar", @"(?:warn|warns|warned)"),
    };

    private static readonly string[] IrregularDizer =
    {
        "dizer", "digo", "dizes", "diz", "dizemos", "dizem", "disse", "disseste", "dissemos", "disseram",
        "dizia", "dizias", "dizíamos", "diziam", "direi", "dirás", "dirá", "diremos", "dirão",
        "diria", "dirias", "diríamos", "diriam", "diga", "digas", "digamos", "digam",
        "dissesse", "dissesses", "disséssemos", "dissessem", "disser", "disseres", "dissermos", "disserem",
    };

    private static readonly Dictionary<string, (string Infinitive, Regex English)> TellingForms = BuildTellingForms();

    private static Dictionary<string, (string, Regex)> BuildTellingForms()
    {
        var map = new Dictionary<string, (string, Regex)>(StringComparer.OrdinalIgnoreCase);
        foreach (var (infinitive, english) in TellingVerbs)
        {
            // Possessive "her" ("told her parents") is not the recipient: the pronoun must end the phrase or
            // be followed by the content of the telling, not by a bare noun phrase.
            var regex = new Regex(@"\b" + english + @"\s+(?<obj>me|you|him|us|them|her(?=\s*(?:[.!?,;:…]|$)|\s+(?:about|of|that|this|these|those|everything|nothing|something|anything|all|the|a|an|it|he|she|they|we|I|you|what|where|when|why|how|who|to|not|never|no|so|then|and|or|but|if|because|right|again|later|now|today|tonight|yesterday|tomorrow|once|twice|first|already|just|exactly|too)\b))\b", Ci);
            var forms = infinitive == "dizer" ? IrregularDizer
                : VerbConjugator.TryConjugate(infinitive, out var conjugated)
                    ? conjugated.Where((_, index) => index is not (>= 34 and <= 38)).ToArray()
                    : throw new InvalidOperationException($"Cannot conjugate {infinitive}.");
            foreach (var form in forms)
                if (form != "mento") // "mentir" is stem-changing: the real first person is "minto"
                    map[form] = (infinitive, regex);
            if (infinitive == "mentir") map["minto"] = (infinitive, regex);
        }
        return map;
    }

    private static readonly Regex TellingRecipient = new(
        Before + @"(?<!(?<![\p{L}\p{M}])(?:me|te|se|nos|vos|lhe|lhes)\s+)(?<v>" +
        string.Join("|", TellingForms.Keys.OrderByDescending(form => form.Length).Select(Regex.Escape)) +
        @")" + After + @"(?!-)" +
        @"(?=(?<sp>\s+)(?:sobre|disso|disto|daquilo|que|tudo|isso|isto|a\s+verdade|o\s+que)" + After + @")", Ci);

    private static readonly Regex OwedObject = new(
        Before + @"(?<verb>[\p{L}\p{M}]+)(?<gap>\s+)(?<object>isto|isso|aquilo|uma\s+explicação|um\s+favor)" +
        @"\s+a\s+(?<person>ele|ela|eles|elas)(?=\s*(?:[.!?…]|[\uE000-\uF8FF]|$))", Ci);
    private static readonly HashSet<string> OweForms = new(
        VerbConjugator.TryConjugate("dever", out var forms) ? forms : throw new InvalidOperationException("Cannot conjugate dever."),
        StringComparer.OrdinalIgnoreCase);
    private const string SeeingForms =
        @"vejo|vês|vê|vemos|vedes|veem|vi|viste|viu|vimos|vistes|viram|via|vias|víamos|víeis|viam|" +
        @"verei|verás|verá|veremos|vereis|verão|veria|verias|veríamos|veríeis|veriam|" +
        @"veja|vejas|vejamos|vejais|vejam|visse|visses|víssemos|vísseis|vissem";
    private const string SeeingSubject = @"(?:eu|tu|ele|ela|nós|vós|eles|elas|você|vocês|o\s+senhor|a\s+senhora)";
    private static readonly Regex SeeingManner = new(
        Before + @"(?<verb>" + SeeingForms + @")(?<gap>\s+)(?<manner>assim|dessa\s+forma|desse\s+modo)" +
        After + @"(?=\s*(?:[.!?…]|[\uE000-\uF8FF]|$))", Ci);
    private static readonly Regex SeeingQuestion = new(
        @"(?:(?<![\p{L}\p{M}-])|(?<=^-)|(?<=\n-))" +
        @"(?<how>como)(?<gap>\s+)(?<subject>" + SeeingSubject + @"\s+)?(?<verb>" + SeeingForms + @")" +
        @"(?<inverted>\s+" + SeeingSubject + @")?(?=\s*(?:[\uE000-\uF8FF]\s*)*\?)", Ci);
    private static readonly Regex EnglishSeeingObject = new(
        @"\b(?:see|sees|saw|seen)\s+it\b", Ci);
    private static readonly Regex PressNoun = new(
        Before + @"(?<head>(?:a|as|uma|umas|numa|numas|esta|estas|essa|essas|minha|minhas|sua|suas|tua|tuas|nossa|nossas|de)\s+)" +
        @"(?<noun>colec?tivas?)" + After + @"(?!\s+de\s+imprensa\b)", Ci);
    private static readonly Regex Broad = new(
        Before + @"(?<head>(?:esta|essa|aquela|estas|essas|aquelas|uma|umas)\s+)(?<noun>donas?)" +
        After + @"(?!\s+(?:de|do|da|dos|das)\b|\s+(?-i:\p{Lu}))", Ci);
    private static readonly Regex Learned = new(
        Before + @"duramente\s+(?<part>aprendid[oa]s?)" + After, Ci);
    private static readonly Regex Business = new(
        Before + @"(?<verb>vai|vamos)\s+fazer\s+(?<noun>negócios?)(?<place>\s+aqui)?\s+ou\s+(?:o\s+)?quê(?=\s*\?)", Ci);
    private static readonly Regex Quote = new(
        Before + @"(?<subject>(?:um|uma|dois|duas|três|quatro|cinco|seis|sete|oito|nove|dez|isto|isso|\d+)\s+)" +
        @"(?<verb>custará|custarão)(?=\s+(?:[$€£]\s*)?\d[\d., ]*(?:\s*(?:dólares?|euros?|libras?)\b|[$€£]|[.!?…]|$))", Ci);
    private static readonly Regex FutureContext = new(
        @"\b(?:amanhã|depois|futuro|futura|próxim[oa]s?|quando|se|daqui\s+a|previsão|previsto|" +
        @"tomorrow|later|future|next|when|if|forecast|predicted|eventually)\b", Ci);
    private static readonly Regex MindRequest = new(
        @"(?<head>^[\s\uE000-\uF8FF]*(?:[-–—]\s*)?)(?:(?<negative>não)\s+)?" +
        @"(?:(?<subject>você|tu)\s+)?(?:(?<clitic>se|te)\s+)?(?<verb>importa|importas|importaria|importarias)" +
        @"(?:-(?<enclitic>se|te))?(?<gap>\s+)(?:de\s+)?(?<inf>[\p{L}\p{M}]+)" + After, Ci);
    private const string ClauseStart = @"(?<head>^|[.!?;]\s+|\n[ \t]*)(?<speaker>[-–—]?[ \t]*[\uE000-\uF8FF]*)";
    private static readonly Regex BigMouth = new(
        ClauseStart + @"(?:(?:você|tu)\s+)?(?<verb>tem|tens)\s+um\s+bocão" + After +
        @"(?<tail>,?\s+e\s+não\s+(?:se\s+cala|te\s+calas))?", Ci);
    private static readonly Regex KeepCommand = new(
        ClauseStart + @"(?<verb>fica|fique)\s+(?<gerund>[\p{L}\p{M}]+(?:ando|endo|indo))" + After, Ci);
    private static readonly Regex OnlyCommand = new(
        ClauseStart + @"(?<only>só|apenas|somente)(?<gap>\s+)(?<verb>fica|fique|vai|vá|faz|faça|olha|olhe|ouve|ouça|espera|espere)" + After, Ci);
    private const string EmphaticHow = @"como\s+(?:diabos|diabo|raio|raios|caraças)";
    private const string QuestionSubjects = @"eu|tu|ele|ela|nós|vós|eles|elas";
    private static readonly Regex EmphaticQuestion = new(
        @"(?<head>^|[.!?;]\s+|\n(?=[ \t]*[-–—]))(?<speaker>[-–—]?[ \t]*[\uE000-\uF8FF]*)" +
        @"(?<how>" + EmphaticHow + @")(?<gap>\s+)(?<subject>" + QuestionSubjects +
        @")(?<space>\s+)(?<clitic>(?:me|te|se|nos|vos|lhe|lhes|o|a|os|as)\s+)?(?<verb>[\p{L}\p{M}]+)" +
        @"(?=\s*(?:[\uE000-\uF8FF]\s*)*\?)", Ci);
    private static readonly Regex EmphaticInversionBefore = new(
        @"(?:^|[.!?;]\s+|\n[ \t]*)[-–—]?[ \t]*[\uE000-\uF8FF]*" + EmphaticHow + @"\s+$", Ci);
    private static readonly Regex EmphaticAttractor = new(
        Before + EmphaticHow + @"(?:\s+(?:" + QuestionSubjects + @"))?\s*$", Ci);
    internal static bool HasEmphaticAttractor(string before) => EmphaticAttractor.IsMatch(before);
    private static readonly HashSet<string> QuestionCognition = new(StringComparer.OrdinalIgnoreCase)
    {
        "sei", "sabes", "sabe", "sabemos", "sabeis", "sabem",
        "sabia", "sabias", "sabíamos", "sabíeis", "sabiam",
        "soube", "soubeste", "soubemos", "soubestes", "souberam",
        "posso", "podes", "pode", "podemos", "podeis", "podem",
        "podia", "podias", "podíamos", "podíeis", "podiam", "pôde",
        "consigo", "consegues", "consegue", "conseguimos", "conseguis", "conseguem",
    };
    private static bool IsEmphaticQuestionVerb(string verb) => QuestionCognition.Contains(verb) ||
        PortugueseVerbs.TrySplitFuture(verb, out _, out _, requireTransitive: false);

    internal static bool KeepsEmphaticQuestionSubject(string before, string verb, string subject) =>
        EmphaticInversionBefore.IsMatch(before) && IsEmphaticQuestionVerb(verb) &&
        (subject.EndsWith("s", StringComparison.OrdinalIgnoreCase)
            ? Regex.IsMatch(verb, @"(?:am|ão)$", Ci)
            : Regex.IsMatch(verb, @"(?:ria|rá)$|^(?:sabe|sabia|soube|pode|podia|pôde|consegue)$", Ci));

    private const string Occupations =
        @"burocratas?|médic[oa]s?|advogad[oa]s?|políci[ao]s?|professor(?:a|es|as)?|" +
        @"detetives?|engenheir[oa]s?|enfermeir[oa]s?|presidentes?|ministr[oa]s?|diretor(?:a|es|as)?";
    private static readonly Regex RecentOccupation = new(
        Before + @"(?<verb>foi|foram)(?<gap>\s+)(?<role>(?:(?:um|uma|uns|umas)\s+)?(?<noun>" +
        Occupations + @"))(?<time>\s+até\s+(?:há\s+)?(?:\d+|um|uma|dois|duas|três|quatro|cinco|dez|quinze|vinte|alguns|algumas)" +
        @"\s+(?:segundos?|minutos?|horas?|dias?)(?:\s+atrás)?)" +
        @"(?=\s*(?:[.!?…]|[\uE000-\uF8FF]|$))", Ci);
    private static readonly Regex EnglishRecentState = new(
        @"\b(?:was|were)\s+(?:(?:a|an|the)\s+)?(?:[\p{L}'-]+\s+){1,5}(?:until|till)\s+" +
        @"(?:\d+|a|an|one|two|three|four|five|ten|fifteen|twenty|some|a\s+few)\s+" +
        @"(?:seconds?|minutes?|hours?|days?)\s+ago\b", Ci);

    internal static string NaturalStyle(string text, ReferenceHint? reference, bool pronouns, Func<string, bool> blocked)
    {
        if (pronouns)
            text = EmphaticQuestion.Replace(text, m =>
            {
                var verb = m.Groups["verb"].Value;
                var subject = m.Groups["subject"].Value;
                if (!IsEmphaticQuestionVerb(verb) || blocked(verb) ||
                    new[] { "ele", "ela", "eles", "elas" }.Contains(subject, StringComparer.OrdinalIgnoreCase) &&
                    !KeepsEmphaticQuestionSubject(m.Groups["how"].Value + " ", verb, subject))
                    return m.Value;
                return m.Groups["head"].Value + m.Groups["speaker"].Value + m.Groups["how"].Value +
                    m.Groups["gap"].Value + m.Groups["clitic"].Value + verb + m.Groups["space"].Value + subject;
            });
        if (!blocked("ser") && EnglishRecentState.IsMatch(reference?.EnglishText ?? ""))
            text = RecentOccupation.Replace(text, m =>
            {
                var verb = m.Groups["verb"].Value;
                var noun = m.Groups["noun"].Value;
                var plural = verb.Equals("foram", StringComparison.OrdinalIgnoreCase);
                var before = Regex.Match(m.Result("$`"), @"[^.!?;]*$").Value;
                if (blocked(verb) || blocked(noun) || noun.EndsWith("s", StringComparison.OrdinalIgnoreCase) != plural ||
                    Regex.IsMatch(before, @"\b(?:não|nunca|nem|jamais)\b", Ci) ||
                    !Regex.IsMatch(m.Groups["time"].Value, @"\bhá\b|\batrás\b", Ci)) return m.Value;
                return CaseHelper.MatchCase(verb, plural ? "eram" : "era") + m.Groups["gap"].Value +
                    m.Groups["role"].Value + m.Groups["time"].Value;
            });
        return text;
    }

    private static readonly Dictionary<string, string> PriorityForms = new(StringComparer.OrdinalIgnoreCase)
    {
        ["priorizar"] = "dar", ["priorizo"] = "dou", ["priorizas"] = "dás", ["prioriza"] = "dá",
        ["priorizamos"] = "damos", ["priorizam"] = "dão",
        ["priorizava"] = "dava", ["priorizavas"] = "davas", ["priorizávamos"] = "dávamos", ["priorizavam"] = "davam",
        ["priorizei"] = "dei", ["priorizaste"] = "deste", ["priorizou"] = "deu", ["priorizaram"] = "deram",
        ["priorizarei"] = "darei", ["priorizarás"] = "darás", ["priorizará"] = "dará",
        ["priorizaremos"] = "daremos", ["priorizarão"] = "darão",
        ["priorizaria"] = "daria", ["priorizarias"] = "darias", ["priorizaríamos"] = "daríamos", ["priorizariam"] = "dariam",
        ["priorize"] = "dê", ["priorizes"] = "dês", ["priorizemos"] = "demos", ["priorizem"] = "deem",
    };
    private static readonly Regex TargetingStreetCrime = new(
        Before + @"(?<verb>" + string.Join("|", PriorityForms.Keys.OrderByDescending(s => s.Length)) +
        @")\s+visar\s+(?:ao|aos|o|os|à|a)\s+(?<noun>crimes?|criminalidade)\s+de\s+rua" +
        @"(?=\s*(?:[,.;:!?…]|[\uE000-\uF8FF]|$))", Ci);
    private static string CrimePolicy(string text, string english, Func<string, bool> blocked)
    {
        if (!Regex.IsMatch(english, @"\bprioriti[sz](?:e|es|ed|ing)\s+targeting\s+street\s+crime" +
            @"(?=\s*(?:[.,;:!?…]|$)|\s+(?:in|on|by|and|rather|over|before|after)\b)", Ci) ||
            new[] { "priorizar", "visar", "crime", "crimes", "criminalidade", "combate" }.Any(blocked))
            return text;
        return TargetingStreetCrime.Replace(text, m => blocked(m.Groups["verb"].Value) ? m.Value :
            CaseHelper.MatchCase(m.Groups["verb"].Value, PriorityForms[m.Groups["verb"].Value]) +
            (CaseHelper.IsAllUpper(m.Value) ? " PRIORIDADE AO COMBATE AOS CRIMES DE RUA" : " prioridade ao combate aos crimes de rua"));
    }

    internal static bool HasBigMouthPossession(ReferenceHint? reference) =>
        Regex.IsMatch(reference?.EnglishText ?? "", @"\byou(?:'ve\s+got|\s+have|\s+got)\s+a\s+big\s+mouth\b", Ci) &&
        !Regex.IsMatch(reference?.EnglishText ?? "", @"\b(?:he|she|they)\b", Ci);

    internal static string Address(string text, ReferenceHint? reference, bool informal, bool progressive,
        bool vocabulary, Func<string, bool> blocked)
    {
        var english = reference?.EnglishText ?? "";
        if (vocabulary && HasBigMouthPossession(reference) && !blocked("bocão") && !blocked("boca"))
            text = BigMouth.Replace(text, m => m.Groups["head"].Value + m.Groups["speaker"].Value +
                CaseHelper.MatchCase(m.Groups["verb"].Value, informal ? "tens uma boca grande" : "tem uma boca grande") +
                (m.Groups["tail"].Success ? informal ? " e não te calas" : " e não se cala" : ""));
        if (progressive && Regex.IsMatch(english, @"(?:^|[.!?]\s+)(?:keep|keep\s+on)\s+\p{L}+ing\b", Ci) &&
            !blocked("ficar") && !blocked("continuar"))
        {
            text = KeepCommand.Replace(text, m =>
            {
                var inf = PortugueseConverter.ProgressiveRules.ToInfinitive(m.Groups["gerund"].Value);
                if (!PortugueseVerbs.IsKnownInfinitive(inf) || blocked(inf)) return m.Value;
                return m.Groups["head"].Value + m.Groups["speaker"].Value +
                    CaseHelper.MatchCase(m.Groups["verb"].Value, informal ? "continua" : "continue") + " a " + inf;
            });
            if (informal && Regex.IsMatch(english, @"\byou(?:'re|\s+are)\s+(?:gonna|going\s+to)\b", Ci) &&
                !Regex.IsMatch(english, @"\b(?:he|she|they)\b", Ci))
                text = Regex.Replace(text, @"(?<head>\bcontinua\s+a\s+[^.!?;–—-]+,\s+)vai(?=\s+\p{L}+[aeiô]r\b)",
                    "${head}vais", Ci);
        }
        if (Regex.IsMatch(english, @"\bjust\s+(?:stay|go|do|look|listen|wait)\b", Ci) &&
            !Regex.IsMatch(english, @"\b(?:i|he|she|we|they)\s+just\b", Ci))
            text = OnlyCommand.Replace(text, m => blocked(m.Groups["verb"].Value) ? m.Value :
                m.Groups["head"].Value + m.Groups["speaker"].Value +
                CaseHelper.MatchCase(m.Groups["only"].Value, "simplesmente") + m.Groups["gap"].Value +
                (informal ? PortugueseConverter.TuImperative.ToTu(m.Groups["verb"].Value) ?? m.Groups["verb"].Value : m.Groups["verb"].Value));
        return text;
    }

    internal static string Objects(string text, ReferenceHint? reference, bool informal, Func<string, bool> blocked)
    {
        var english = reference?.EnglishText ?? "";
        if (!blocked("ver") && EnglishSeeingObject.Matches(english).Count == 1)
        {
            if (Regex.IsMatch(english, @"\b(?:see|sees|saw|seen)\s+it\s+(?:that|this|the\s+same|a\s+different)\s+way\b", Ci) &&
                SeeingManner.Matches(text).Count == 1)
                text = SeeingManner.Replace(text, m =>
                {
                    var verb = m.Groups["verb"].Value;
                    var before = Regex.Match(m.Result("$`"), @"[^.!?;:]*$").Value;
                    if (blocked(verb) || Regex.IsMatch(before,
                        @"(?:\b(?:me|te|se|nos|vos|lhe|lhes|o|a|os|as)|[\uE000-\uF8FF])\s*$", Ci))
                        return m.Value;
                    var proclisis = Regex.IsMatch(before,
                        @"\b(?:não|nunca|jamais|também|já|só|apenas|sempre|ainda|talvez|que|quando|como|porque)\b", Ci) ||
                        PortugueseConverter.PronounRules.AfterSubordinateSubject(before);
                    var target = proclisis ? (CaseHelper.IsAllUpper(verb) ? "O " : "o ") + verb :
                        PortugueseVerbs.AttachMesoclitic(verb, "o", requireTransitive: false) ??
                        PortugueseVerbs.AttachEnclitic(verb, "o");
                    return target + m.Groups["gap"].Value + m.Groups["manner"].Value;
                });
            if (Regex.IsMatch(english, @"\b(?:how|in\s+what\s+way)\b[^.!?;]*\b(?:see|sees|saw|seen)\s+it\b", Ci) &&
                SeeingQuestion.Matches(text).Count == 1)
                text = SeeingQuestion.Replace(text, m => blocked(m.Groups["verb"].Value) ? m.Value :
                    m.Groups["how"].Value + m.Groups["gap"].Value + m.Groups["subject"].Value +
                    (CaseHelper.IsAllUpper(m.Groups["verb"].Value) ? "O " : "o ") +
                    m.Groups["verb"].Value + m.Groups["inverted"].Value);
        }
        if (Regex.IsMatch(english, @"^(?:(?:would|do|could|will)\s+you\s+mind|you\s+mind)\s+\p{L}+ing\b", Ci) &&
            !blocked("importar"))
            text = MindRequest.Replace(text, m =>
            {
                var verb = m.Groups["verb"].Value;
                var inf = m.Groups["inf"].Value;
                if (!PortugueseVerbs.IsKnownInfinitive(inf) || blocked(verb) ||
                    !m.Groups["clitic"].Success && !m.Groups["enclitic"].Success) return m.Value;
                var conditional = verb.StartsWith("importari", StringComparison.OrdinalIgnoreCase);
                var target = informal ? conditional ? "importarias" : "importas" : conditional ? "importaria" : "importa";
                var clitic = informal ? "te" : "se";
                var negative = m.Groups["negative"].Success;
                var result = negative ? "não " + clitic + " " + target
                    : conditional ? PortugueseVerbs.AttachMesoclitic(target, clitic, requireTransitive: false)
                    : PortugueseVerbs.AttachEnclitic(target, clitic);
                if (result == null) return m.Value;
                var gap = m.Groups["gap"].Value;
                var rest = m.Result("$'");
                var complement = inf.Equals("responder", StringComparison.OrdinalIgnoreCase) &&
                    Regex.IsMatch(rest, @"^\s+(?:(?:algumas|umas|várias|estas|essas|duas|três|poucas)\s+)?(?:perguntas|questões)\b", Ci)
                    ? " a" : "";
                var first = new[] { "negative", "subject", "clitic", "verb" }
                    .Select(name => m.Groups[name]).First(group => group.Success).Value;
                var replacement = CaseHelper.MatchCase(first, result) + " de" + gap + inf + complement;
                if (CaseHelper.IsAllUpper(verb) && CaseHelper.IsAllUpper(inf))
                    replacement = replacement.ToUpperInvariant();
                return m.Groups["head"].Value + replacement;
            });
        if (informal && Regex.IsMatch(english, @"\b(?:push|throw|kick|put|turn|force|shove)\s+you\s+out\b", Ci) &&
            !Regex.IsMatch(english, @"\b(?:him|her|them)\b", Ci) && !blocked("pôr"))
            text = Regex.Replace(text, Before + @"(?<verb>pô-l[oa])(?=\s+para\s+fora\b)", m =>
                blocked(m.Value) ? m.Value : CaseHelper.MatchCase(m.Value, "pôr-te"), Ci);
        if (Regex.IsMatch(english, @"\bremind\s+you\s+that\b", Ci) &&
            !Regex.IsMatch(text, @"\b(?:vocês|vós)\b", Ci))
            text = Reminder.Replace(text, m =>
            {
                var verb = m.Groups["verb"].Value;
                var infinitive = verb.StartsWith("re", StringComparison.OrdinalIgnoreCase) ? "relembrar" : "lembrar";
                return blocked(infinitive) || blocked(verb) ? m.Value :
                    CaseHelper.MatchCase(verb, infinitive + (informal ? "-te" : "-lhe"));
            });
        if (Regex.IsMatch(english, @"\bow(?:e|es|ed|ing)\b", Ci) && !blocked("dever"))
            text = OwedObject.Replace(text, m =>
            {
                var verb = m.Groups["verb"].Value;
                if (!OweForms.Contains(verb) || PortugueseVerbs.IsInfinitive(verb) ||
                    Regex.IsMatch(verb, @"id[oa]s?$", Ci) || blocked(verb)) return m.Value;
                var clitic = m.Groups["person"].Value.EndsWith("s", StringComparison.OrdinalIgnoreCase) ? "lhes" : "lhe";
                if (CaseHelper.IsAllUpper(verb)) clitic = clitic.ToUpperInvariant();
                var attached = PortugueseVerbs.IsFutureOrConditional(verb)
                    ? PortugueseVerbs.AttachMesoclitic(verb, clitic, requireTransitive: false)
                    : PortugueseVerbs.AttachEnclitic(verb, clitic);
                if (attached == null) return m.Value;
                var before = m.Result("$`");
                if (Regex.IsMatch(before, @"\b(?:não|nunca|jamais|que|se|quando)\s*$", Ci))
                    attached = CaseHelper.MatchCase(verb, clitic) + " " + verb.ToLowerInvariant();
                return attached + m.Groups["gap"].Value + m.Groups["object"].Value;
            });
        if (TellingRecipient.Matches(text).Count == 1)
            text = TellingRecipient.Replace(text, m =>
            {
                var verb = m.Groups["v"].Value;
                var (infinitive, englishTelling) = TellingForms[verb];
                if (blocked(verb) || blocked(infinitive) ||
                    PortugueseConverter.HasNominalDeterminerBefore(m.Result("$`")) ||
                    Regex.IsMatch(m.Result("$'"), @"^[ \t]*(?:\r?\n[ \t]*\r?\n|\r?\n[ \t]*[-–—])"))
                    return m.Value;
                var recipients = englishTelling.Matches(english)
                    .Select(telling => telling.Groups["obj"].Value.ToLowerInvariant())
                    .Distinct(StringComparer.Ordinal).ToArray();
                if (recipients.Length != 1) return m.Value;
                // "Ele falou com ela. Ela falava que era inocente.": the recipient is already expressed elsewhere.
                if (Regex.IsMatch(text, @"(?<![\p{L}\p{M}])(?:me|te|lhe|lhes|vos)(?![\p{L}\p{M}])|-(?:me|te|lhe|lhes|nos|vos)\b", Ci) ||
                    Regex.IsMatch(text, @"\b(?:com|a|para|pra)\s+(?:ela|ele|eles|elas|mim|ti|si|você|vocês|o\s+senhor|a\s+senhora)\b", Ci))
                    return m.Value;
                var before = Regex.Match(m.Result("$`"), @"[^.!?;:]*$").Value;
                if (Regex.IsMatch(before, @"\b(?:não|nunca|jamais)\b", Ci) !=
                    Regex.IsMatch(english, @"\b(?:not|never)\b|n['’]t\b", Ci))
                    return m.Value;
                var clitic = recipients[0] switch
                {
                    "me" => "me", "him" or "her" => "lhe", "us" => "nos", "them" => "lhes",
                    _ => informal ? "te" : "lhe",
                };
                if (CaseHelper.IsAllUpper(verb)) clitic = clitic.ToUpperInvariant();
                if (Regex.IsMatch(before, Before +
                        @"(?:não|nunca|jamais|já|ainda|sempre|também|só|apenas|talvez|que|quem|porque|se|quando|como|onde)\s+$", Ci) ||
                    PortugueseConverter.PronounRules.AfterSubordinateSubject(before))
                    return CaseHelper.MatchCase(verb, clitic) + " " + verb;
                return (PortugueseVerbs.IsFutureOrConditional(verb)
                    ? PortugueseVerbs.AttachMesoclitic(verb, clitic, requireTransitive: false)
                    : PortugueseVerbs.AttachEnclitic(verb, clitic)) ?? m.Value;
            });
        return text;
    }

    internal static string Vocabulary(string text, ReferenceHint? reference, Func<string, bool> blocked)
    {
        var english = reference?.EnglishText ?? "";
        text = CrimePolicy(text, english, blocked);
        if (Regex.IsMatch(english, @"\b(?:is|are|was|were|be|been|being|'s|'re)\s+(?:(?:a|an|the)\s+)?(?:prick|jerk|idiot|asshole)s?\b", Ci) &&
            !Regex.IsMatch(english, @"\b(?:sex|sexy|naughty|lewd|promiscuous)\b", Ci) && !blocked("safado"))
            text = Regex.Replace(text, Before + @"(?<det>um|uma|uns|umas)(?<gap>\s+)(?<noun>safad[oa]s?)" + After,
                m => blocked(m.Groups["noun"].Value) ? m.Value : m.Groups["det"].Value + m.Groups["gap"].Value +
                    CaseHelper.MatchCase(m.Groups["noun"].Value,
                        m.Groups["noun"].Value.EndsWith("s", StringComparison.OrdinalIgnoreCase) ? "idiotas" : "idiota"), Ci);
        if (Regex.IsMatch(english, @"\b(?:press\s+conferences?|pressers?)\b", Ci))
        {
            text = PressNoun.Replace(text, m =>
            {
                var noun = m.Groups["noun"].Value;
                return blocked(noun) ? m.Value : m.Groups["head"].Value +
                    CaseHelper.MatchCase(noun, noun.EndsWith("s", StringComparison.OrdinalIgnoreCase) ? "conferências de imprensa" : "conferência de imprensa");
            });
            if (Regex.IsMatch(english, @"\b(?:select|exclusive)\s+interviews?\b", Ci))
                text = Regex.Replace(text, @"(?<=conferências de imprensa\s+ou\s+)exclusivas\b", "entrevistas exclusivas", Ci);
        }
        if (Regex.IsMatch(english, @"\b(?:this|that|these|those|a|the)\s+broads?\b", Ci) && !blocked("dona"))
            text = Broad.Replace(text, m => blocked(m.Groups["noun"].Value) ? m.Value :
                m.Groups["head"].Value + CaseHelper.MatchCase(m.Groups["noun"].Value,
                    m.Groups["noun"].Value.EndsWith("s", StringComparison.OrdinalIgnoreCase) ? "tipas" : "tipa"));
        return text;
    }

    internal static string Idioms(string text, string? previous, ReferenceHint? reference, Func<string, bool> blocked)
    {
        var english = reference?.EnglishText ?? "";
        if (Regex.IsMatch(english, @"\ba\s+lot\s+of\s+(?:damn|damned|bloody)\b|\bso\s+many\s+(?:damn|damned|bloody)\b", Ci) &&
            !blocked("maldito") && !blocked("maldita") && !blocked("malditos") && !blocked("malditas"))
            text = Regex.Replace(text, Before + @"(?<quantity>muitos|muitas)(?<gap>\s+)(?<emphasis>maldit[oa]s)\s+(?<noun>[\p{L}\p{M}]+s)" + After,
                m => m.Groups["quantity"].Value.EndsWith("as", StringComparison.OrdinalIgnoreCase) !=
                    m.Groups["emphasis"].Value.EndsWith("as", StringComparison.OrdinalIgnoreCase) ? m.Value :
                    m.Groups["quantity"].Value + m.Groups["gap"].Value + m.Groups["noun"].Value, Ci);
        if (Regex.IsMatch(english, @"\ball\s+(?:deals|agreements)\s+are\s+off\b", Ci) &&
            !blocked("acordo") && !blocked("acordos"))
            text = Regex.Replace(text, Before + @"(?<all>tod[oa]s)(?<gap>\s+)(?:os|as)(?<space>\s+)acordos(?<verbGap>\s+)não\s+valem" + After,
                m => CaseHelper.MatchCase(m.Groups["all"].Value, "todos") + m.Groups["gap"].Value +
                    "os" + m.Groups["space"].Value + "acordos" + m.Groups["verbGap"].Value + "são nulos", Ci);
        if (Regex.IsMatch(english, @"\bit(?:'s|\s+has|\s+had)\s+(?:only\s+|just\s+)?been\s+(?:only\s+|just\s+)?(?:a|an|a\s+few|some|\d+)\s+(?:seconds?|minutes?|hours?|days?|weeks?|months?|years?)\b", Ci) &&
            !blocked("fazer") && !blocked("passar"))
            text = Regex.Replace(text,
                @"(?<head>^|[,;.!?]\s+|\bmas\s+)(?<only>(?:(?:só|apenas|somente)\s+)?)(?<verb>faz|fazia|há|havia)\s+" +
                @"(?<duration>(?:um|uma|uns|umas|dois|duas|três|quatro|cinco|seis|sete|oito|nove|dez|alguns|algumas|poucos|poucas|\d+)\s+(?<unit>segundos?|minutos?|horas?|dias?|semanas?|mês|meses|anos?))" +
                After + @"(?!\s+que\b)(?=\s*(?:[.!?…;\uE000-\uF8FF]|$|\bdesde\b))", m =>
                {
                    var plural = m.Groups["unit"].Value.EndsWith("s", StringComparison.OrdinalIgnoreCase) &&
                        !m.Groups["unit"].Value.Equals("mês", StringComparison.OrdinalIgnoreCase);
                    var past = Regex.IsMatch(m.Groups["verb"].Value, @"^(?:fazia|havia)$", Ci);
                    return m.Groups["head"].Value + m.Groups["only"].Value +
                        CaseHelper.MatchCase(m.Groups["verb"].Value, past ? plural ? "tinham passado" : "tinha passado" :
                            plural ? "passaram" : "passou") + " " + m.Groups["duration"].Value;
                }, Ci);
        if (Regex.IsMatch(english, @"\bhard[- ](?:earned|learned|learnt)\b|\b(?:learned|learnt)\b[^.!?]{0,40}\bthe\s+hard\s+way\b", Ci) &&
            !blocked("duramente") && !blocked("aprender"))
            text = Learned.Replace(text, m => blocked(m.Groups["part"].Value) ? m.Value :
                CaseHelper.MatchCase(m.Value, m.Groups["part"].Value + " a duras penas"));
        if (Regex.IsMatch(english, @"^(?:are\s+we\s+)?doing\s+(?:some\s+)?business(?:\s+here)?\s+or\s+what\s*\?$", Ci) &&
            !blocked("fazer") && !blocked("negócio"))
            text = Business.Replace(text, m => CaseHelper.MatchCase(m.Groups["verb"].Value, "vamos") +
                " fazer " + m.Groups["noun"].Value + m.Groups["place"].Value + " ou quê");
        if (Regex.IsMatch(english, @"\bwill\s+cost\s+you\b", Ci) &&
            Regex.IsMatch(english, @"[$€£]\s*\d|\d\s*(?:dollars?|euros?|pounds?)\b", Ci) &&
            !FutureContext.IsMatch(english + " " + text + " " + previous) && !blocked("custar"))
            text = Quote.Replace(text, m => blocked(m.Groups["verb"].Value) ? m.Value : m.Groups["subject"].Value +
                CaseHelper.MatchCase(m.Groups["verb"].Value,
                    m.Groups["verb"].Value.Equals("custarão", StringComparison.OrdinalIgnoreCase) ? "custam" : "custa"));
        return text;
    }
}
