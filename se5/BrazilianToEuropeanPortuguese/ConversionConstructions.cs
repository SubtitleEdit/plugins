using System.Text.RegularExpressions;

namespace SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;

/// <summary>Complement-aware constructions; ambiguous senses are not blanket word replacements.</summary>
internal static class ConversionConstructions
{
    private const RegexOptions Ci = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;
    private const string Before = @"(?<![\p{L}\p{M}-])";
    private const string After = @"(?![\p{L}\p{M}-])";
    // Like Before, but a dialogue dash ("-Deu errado.") may precede the word; a hyphenated compound may not.
    private const string DialogueBefore = @"(?<![\p{L}\p{M}])(?<![\p{L}\p{M}]-)";
    private const string Adverbs = @"(?:(?:não|nunca|já|ainda|sempre|também|só|realmente|talvez)\s+){0,3}";
    private static string Alternatives(IEnumerable<string> words) =>
        string.Join("|", words.Distinct(StringComparer.OrdinalIgnoreCase).OrderByDescending(w => w.Length).Select(Regex.Escape));

    private static string[] Forms(string infinitive)
    {
        if (!VerbConjugator.TryConjugate(infinitive, out var forms))
            throw new InvalidOperationException($"Unsupported regular construction verb: {infinitive}");
        return forms;
    }

    private static readonly (string Infinitive, string[] Forms, string English)[] ComparisonVerbs =
    {
        ("ser", new[] { "fosse", "fosses", "fôssemos", "fossem" }, @"(?:am|is|are|was|were|['’]m|['’]s|['’]re)"),
        ("estar", new[] { "estivesse", "estivesses", "estivéssemos", "estivessem" }, @"(?:am|is|are|was|were|['’]m|['’]s|['’]re)"),
        ("ver", new[] { "visse", "visses", "víssemos", "vissem" }, @"(?:see|sees|saw|seen)"),
        ("saber", new[] { "soubesse", "soubesses", "soubéssemos", "soubessem" }, @"(?:know|knows|knew|known)"),
        ("fazer", new[] { "fizesse", "fizesses", "fizéssemos", "fizessem" }, @"(?:do|does|did|done)"),
        ("ter", new[] { "tivesse", "tivesses", "tivéssemos", "tivessem" }, @"(?:have|has|had)"),
        ("querer", new[] { "quisesse", "quisesses", "quiséssemos", "quisessem" }, @"(?:want|wants|wanted)"),
        ("divertir", Forms("divertir")[27..31], @"(?:amuse|amuses|amused|(?:am|is|are|was|were|['’]m|['’]s|['’]re)\s+amusing)"),
        ("conhecer", Forms("conhecer")[27..31], @"(?:know|knows|knew|known)"),
        ("ajudar", Forms("ajudar")[27..31], @"(?:help|helps|helped)"),
        ("enganar", Forms("enganar")[27..31], @"(?:deceive|deceives|deceived|fool|fools|fooled)"),
        ("ignorar", Forms("ignorar")[27..31], @"(?:ignore|ignores|ignored)"),
        ("acreditar", Forms("acreditar")[27..31], @"(?:believe|believes|believed)"),
        ("lembrar", Forms("lembrar")[27..31], @"(?:remember|remembers|remembered)"),
        ("pensar", Forms("pensar")[27..31], @"(?:think|thinks|thought)"),
        ("dizer", new[] { "dissesse", "dissesses", "disséssemos", "dissessem" }, @"(?:say|says|said)"),
        ("ouvir", new[] { "ouvisse", "ouvisses", "ouvíssemos", "ouvissem" }, @"(?:hear|hears|heard)"),
        ("sentir", Forms("sentir")[27..31], @"(?:feel|feels|felt)"),
        ("gostar", Forms("gostar")[27..31], @"(?:like|likes|liked)"),
    };
    private static readonly Regex ComparisonSource = new(
        @"^\s*(?:[-–—]\s*)?(?<como>como)(?<gap>\s+)" +
        @"(?:(?<subject>eu|tu|você|ele|ela|nós|vocês|eles|elas)(?<subjectGap>\s+))?" +
        @"(?:(?<negative>não|nunca)(?<negativeGap>\s+))?(?<verb>[\p{L}\p{M}]+)" + After + @"(?!-)", Ci);

    public static string HypotheticalComparison(string text, ReferenceHint? reference, Func<string, bool> blocked)
    {
        var source = ComparisonSource.Match(text);
        if (!source.Success || Regex.IsMatch(source.Value, @"\r?\n[ \t]*\r?\n") ||
            blocked(source.Value) || blocked(source.Groups["verb"].Value)) return text;
        var remaining = text[(source.Index + source.Length)..];
        if (Regex.IsMatch(remaining, @"^\s*(?:\r?\n[ \t]*\r?\n|\r?\n[ \t]*[-–—])")) return text;
        var verb = source.Groups["verb"].Value;
        var english = reference?.EnglishText ?? "";
        var candidates = new List<string>();
        foreach (var entry in ComparisonVerbs)
        {
            var slot = Array.FindIndex(entry.Forms, form => form.Equals(verb, StringComparison.OrdinalIgnoreCase));
            if (slot < 0 || blocked(entry.Infinitive)) continue;
            var match = Regex.Match(english,
                @"^(?:like|as\s+if|as\s+though)\s+(?<subject>i|you|he|she|we|they)" +
                @"(?:\s+|(?=['’]))(?:(?<negative>(?:do|does|did)\s+not|(?:don|doesn|didn)['’]t|never)\s+)?" +
                @"(?<verb>" + entry.English + @")\b(?<tail>[^.!?;:]*)(?:[.!?…])?$", Ci);
            if (!match.Success || match.Groups["negative"].Success != source.Groups["negative"].Success) continue;
            if (source.Groups["negative"].Success &&
                source.Groups["negative"].Value.Equals("nunca", StringComparison.OrdinalIgnoreCase) !=
                match.Groups["negative"].Value.Equals("never", StringComparison.OrdinalIgnoreCase)) continue;
            var subject = match.Groups["subject"].Value.ToLowerInvariant() switch
            {
                "i" => "eu", "he" => "ele", "she" => "ela", "we" => "nós", "they" => "eles",
                "you" => source.Groups["subject"].Value.ToLowerInvariant() switch
                {
                    "tu" => "tu", "você" => "você", "vocês" => "vocês", _ => "",
                },
                _ => "",
            };
            var writtenSubject = source.Groups["subject"].Value.ToLowerInvariant();
            if (writtenSubject == "elas" && subject == "eles") subject = "elas";
            if (subject.Length == 0 || writtenSubject.Length > 0 && writtenSubject != subject) continue;
            var expectedSlot = subject switch { "tu" => 1, "nós" => 2, "eles" or "elas" or "vocês" => 3, _ => 0 };
            if (slot != expectedSlot) continue;
            if (Regex.IsMatch(remaining, @"^\s+você\b", Ci) &&
                (!Regex.IsMatch(match.Groups["tail"].Value, @"^\s+you\b(?!\s+(?:and|or|all|both)\b)", Ci) ||
                 subject is "tu" or "você" or "vocês")) continue;
            if (!source.Groups["subject"].Success && Regex.IsMatch(remaining,
                @"^\s+(?:eu|ele|ela|nós|eles|elas)\b", Ci)) continue;
            candidates.Add(subject);
        }

        if (candidates.Distinct(StringComparer.Ordinal).Count() != 1) return text;
        var inferred = source.Groups["subject"].Success ? "" :
            (CaseHelper.IsAllUpper(source.Groups["como"].Value) ? candidates[0].ToUpperInvariant() : candidates[0]) + " ";
        var conjunction = CaseHelper.IsAllUpper(source.Groups["como"].Value) ? "SE" : "se";
        var insertAt = source.Groups["gap"].Index + source.Groups["gap"].Length;
        return text.Insert(insertAt, conjunction + " " + inferred);
    }

    // ============================================= inalienable possession =============================================
    // Portuguese expresses body-part and personal-effect possession with the dative clitic or the bare definite article:
    // "Ele pisou no meu rosto" → "Ele pisou-me o rosto", "Feri o meu braço" → "Feri o braço", "com a sua bota" → "com a bota".

    [Flags]
    private enum Person { None = 0, FirstSingular = 1, SecondSingular = 2, ThirdSingular = 4, FirstPlural = 8, ThirdPlural = 16 }

    // Grammatical persons each conjugation slot can express (see VerbConjugator's slot order); slot 3 includes the "tu" imperative.
    private static readonly Dictionary<int, Person> SlotPersons = new()
    {
        [1] = Person.FirstSingular, [2] = Person.SecondSingular, [3] = Person.ThirdSingular | Person.SecondSingular,
        [4] = Person.FirstPlural, [5] = Person.ThirdPlural,
        [6] = Person.FirstSingular, [7] = Person.SecondSingular, [8] = Person.ThirdSingular, [9] = Person.ThirdPlural,
        [10] = Person.FirstSingular | Person.ThirdSingular, [11] = Person.SecondSingular, [12] = Person.FirstPlural,
        [13] = Person.ThirdPlural,
        [14] = Person.FirstSingular, [15] = Person.SecondSingular, [16] = Person.ThirdSingular, [17] = Person.FirstPlural,
        [18] = Person.ThirdPlural,
        [19] = Person.FirstSingular | Person.ThirdSingular, [20] = Person.SecondSingular, [21] = Person.FirstPlural,
        [22] = Person.ThirdPlural,
        [23] = Person.FirstSingular | Person.ThirdSingular, [24] = Person.SecondSingular, [25] = Person.FirstPlural,
        [26] = Person.ThirdPlural,
        [27] = Person.FirstSingular | Person.ThirdSingular, [28] = Person.SecondSingular, [29] = Person.FirstPlural,
        [30] = Person.ThirdPlural,
        [39] = Person.FirstSingular | Person.ThirdSingular, [40] = Person.SecondSingular, [41] = Person.FirstPlural,
        [42] = Person.FirstPlural,
    };

    // "cuspir" is stem-changing in the present (cospes/cospe/cospem); the regular generation of those slots is wrong.
    private static readonly HashSet<string> MisconjugatedContactForms = new(StringComparer.OrdinalIgnoreCase)
    {
        "cuspes", "cuspe", "cuspem",
    };

    private static readonly Dictionary<string, (string Infinitive, bool Direct, Person Persons)> ContactForms = BuildContactForms();

    private static Dictionary<string, (string, bool, Person)> BuildContactForms()
    {
        var map = new Dictionary<string, (string, bool, Person)>(StringComparer.OrdinalIgnoreCase);
        var direct = new[]
        {
            "pisar", "agarrar", "apertar", "esmagar", "arranhar", "morder", "partir", "cortar", "queimar",
            "torcer", "puxar", "lamber", "beijar", "segurar", "esfregar", "lavar", "limpar", "acariciar",
        };
        var prepositional = new[] { "bater", "tocar", "mexer", "acertar", "cuspir" };
        foreach (var (infinitives, isDirect) in new[] { (direct, true), (prepositional, false) })
            foreach (var infinitive in infinitives)
            {
                var forms = Forms(infinitive);
                foreach (var slot in SlotPersons.Keys.Append(0))
                {
                    var form = forms[slot];
                    if (MisconjugatedContactForms.Contains(form)) continue;
                    var persons = slot == 0 ? Person.None : SlotPersons[slot];
                    map[form] = map.TryGetValue(form, out var known) && known.Item1 == infinitive
                        ? (infinitive, isDirect, known.Item3 | persons)
                        : (infinitive, isDirect, persons);
                }
            }
        return map;
    }

    private static readonly HashSet<string> BodyParts = new(StringComparer.OrdinalIgnoreCase)
    {
        "rosto", "cara", "cabeça", "testa", "nuca", "pescoço", "garganta", "ombro", "ombros", "braço", "braços",
        "cotovelo", "cotovelos", "pulso", "pulsos", "mão", "mãos", "dedo", "dedos", "unha", "unhas", "peito", "costas",
        "barriga", "estômago", "cintura", "perna", "pernas", "coxa", "coxas", "joelho", "joelhos", "canela", "canelas",
        "tornozelo", "tornozelos", "pé", "pés", "calcanhar", "calcanhares", "olho", "olhos", "nariz", "boca", "língua",
        "lábio", "lábios", "dente", "dentes", "orelha", "orelhas", "ouvido", "ouvidos", "queixo", "bochecha", "bochechas",
        "costela", "costelas", "cabelo", "cabelos", "pele",
    };

    private static readonly HashSet<string> PersonalEffects = new(StringComparer.OrdinalIgnoreCase)
    {
        "bota", "botas", "sapato", "sapatos", "luva", "luvas", "chapéu", "casaco", "camisa", "calças", "cinto",
        "capacete", "óculos", "relógio", "anel", "pulseira", "colar", "gravata", "lenço", "cachecol", "mochila",
        "mala", "carteira", "bolso", "bolsos",
    };

    private const string PossessivePattern = @"meu|minha|meus|minhas|teu|tua|teus|tuas|seu|sua|seus|suas|nosso|nossa|nossos|nossas";

    private static readonly Regex InalienableDative = new(
        DialogueBefore + @"(?:(?<trig>(?:" + PortugueseConverter.YouToTuRules.ProclisisTriggers + @")\s+))?" +
        @"(?<v>" + Alternatives(ContactForms.Keys) + @")\s+(?:(?<prep>em|no|na|nos|nas)\s+)?(?:(?<art>o|a|os|as)\s+)?" +
        @"(?<poss>" + PossessivePattern + @")\s+(?<n>" + Alternatives(BodyParts.Concat(PersonalEffects)) + @")" +
        After + @"(?!-)", Ci);

    private static readonly Regex RedundantPossessive = new(
        Before + @"(?<prep>com|em|no|na|nos|nas)\s+(?:(?<art>o|a|os|as)\s+)?" +
        @"(?<poss>" + PossessivePattern + @")\s+(?<n>" + Alternatives(BodyParts.Concat(PersonalEffects)) + @")" +
        After + @"(?!-)", Ci);

    private static readonly Regex ClauseSubjectPronoun = new(
        @"(?<![\p{L}\p{M}])(?:eu|tu|você|vocês|ele|ela|nós|eles|elas)(?![\p{L}\p{M}])", Ci);

    private static string ClauseSubject(string before)
    {
        var clause = Regex.Match(before, @"[^.!?;…]*$").Value;
        var cut = Regex.Matches(clause, @"\r?\n[ \t]*\r?\n|\r?\n[ \t]*[-–—]").Cast<Match>().LastOrDefault();
        if (cut != null) clause = clause[(cut.Index + cut.Length)..];
        var subjects = ClauseSubjectPronoun.Matches(clause);
        return subjects.Count == 0 ? "" : subjects[^1].Value.ToLowerInvariant();
    }

    private static Person SubjectPersonOf(string pronoun) => pronoun switch
    {
        "eu" => Person.FirstSingular, "tu" => Person.SecondSingular, "nós" => Person.FirstPlural,
        "você" or "ele" or "ela" => Person.ThirdSingular, "vocês" or "eles" or "elas" => Person.ThirdPlural,
        _ => Person.None,
    };

    private static Person EnglishSubjectPerson(string english)
    {
        var persons = Person.None;
        foreach (Match m in Regex.Matches(english, @"(?<![\p{L}\p{M}])(?<s>I|you|he|she|we|they)(?=\s|['’])", Ci))
            persons |= m.Groups["s"].Value.ToLowerInvariant() switch
            {
                "i" => Person.FirstSingular, "we" => Person.FirstPlural, "they" => Person.ThirdPlural,
                "you" => Person.SecondSingular | Person.ThirdSingular, _ => Person.ThirdSingular,
            };
        return persons;
    }

    /// <summary>The single English possessor category covering "seu" (your/his/her/their); null when absent or ambiguous.</summary>
    private static string? EnglishSeuCategory(string english)
    {
        var categories = new[] { "your", "his", "her", "their" }
            .Where(word => Regex.IsMatch(english, @"(?<![\p{L}\p{M}])" + word + @"(?![\p{L}\p{M}])", Ci)).ToArray();
        return categories.Length == 1 ? categories[0] : null;
    }

    public static string InalienablePossession(string text, ReferenceHint? reference, bool informal, Func<string, bool> blocked)
    {
        var english = ReferenceAligner.Clean(reference?.EnglishText ?? "");

        // true: the possessor is the clause subject (drop the possessive); false: another person (dative clitic); null: unknown.
        bool? PossessorIsSubject(string before, string poss, Person verbPersons)
        {
            var subject = SubjectPersonOf(ClauseSubject(before));
            var persons = subject != Person.None ? subject : verbPersons;
            var possessor = poss.ToLowerInvariant() switch
            {
                "meu" or "minha" or "meus" or "minhas" => Person.FirstSingular,
                "teu" or "tua" or "teus" or "tuas" => Person.SecondSingular,
                "nosso" or "nossa" or "nossos" or "nossas" => Person.FirstPlural,
                _ => Person.None, // seu: resolved against the English reference below
            };
            if (possessor == Person.None)
            {
                // "seu/sua" needs the reference to tell your/his/her/their and whether that person is the subject.
                var category = EnglishSeuCategory(english);
                if (category == null) return null;
                var owner = ClauseSubject(before);
                if (owner.Length > 0)
                    return (category, owner) switch
                    {
                        ("your", "você" or "vocês") => true,
                        ("his", "ele") or ("her", "ela") => true,
                        ("their", "eles" or "elas") => true,
                        _ => false,
                    };
                var subjects = EnglishSubjectPerson(english);
                return category switch
                {
                    "your" when subjects == (Person.SecondSingular | Person.ThirdSingular) => true,
                    "his" or "her" when subjects == Person.ThirdSingular => (bool?)null, // he + his: own or another man's
                    "their" when subjects == Person.ThirdPlural => null,
                    "your" or "his" or "her" when subjects != Person.None &&
                        (subjects & (Person.SecondSingular | Person.ThirdSingular)) == Person.None => false,
                    "their" when subjects != Person.None && (subjects & Person.ThirdPlural) == Person.None => false,
                    _ => null,
                };
            }

            if (subject != Person.None) return subject == possessor;
            var englishSubjects = EnglishSubjectPerson(english);
            if (persons != Person.None && englishSubjects != Person.None &&
                (persons & englishSubjects) != Person.None)
                persons &= englishSubjects;
            if (persons == Person.None)
            {
                if (englishSubjects == Person.None) return null;
                persons = englishSubjects;
            }

            if (persons == possessor) return true;
            return (persons & possessor) == Person.None ? false : null;
        }

        string? CliticFor(string poss) => poss.ToLowerInvariant() switch
        {
            "meu" or "minha" or "meus" or "minhas" => "me",
            "teu" or "tua" or "teus" or "tuas" => "te",
            "nosso" or "nossa" or "nossos" or "nossas" => "nos",
            _ => EnglishSeuCategory(english) switch
            {
                "your" => informal ? "te" : "lhe",
                "his" or "her" => "lhe",
                "their" => "lhes",
                _ => null,
            },
        };

        string ArticleFor(string poss)
        {
            var feminine = poss.EndsWith("a", StringComparison.OrdinalIgnoreCase) ||
                           poss.EndsWith("as", StringComparison.OrdinalIgnoreCase);
            var plural = poss.EndsWith("s", StringComparison.OrdinalIgnoreCase);
            return feminine ? plural ? "as" : "a" : plural ? "os" : "o";
        }

        text = InalienableDative.Replace(text, m =>
        {
            if (Regex.IsMatch(m.Value, @"\r?\n[ \t]*\r?\n|\r?\n[ \t]*[-–—]")) return m.Value;
            var (infinitive, direct, persons) = ContactForms[m.Groups["v"].Value];
            var noun = m.Groups["n"].Value;
            if (blocked(m.Value) || blocked(infinitive) || blocked(m.Groups["v"].Value) || blocked(noun)) return m.Value;
            var poss = m.Groups["poss"].Value;
            var ownNoun = PossessorIsSubject(m.Result("$`") + m.Groups["trig"].Value, poss, persons);
            if (ownNoun == null) return m.Value;
            var article = CaseHelper.MatchCase(poss, ArticleFor(poss));
            var verb = m.Groups["v"].Value;
            if (ownNoun.Value)
            {
                // "Feri o meu braço" → "Feri o braço", "Toquei na minha cara" → "Toquei na cara".
                var head = m.Groups["prep"].Success
                    ? m.Groups["prep"].Value.Equals("em", StringComparison.OrdinalIgnoreCase)
                        ? CaseHelper.MatchCase(m.Groups["prep"].Value, "n" + ArticleFor(poss))
                        : m.Groups["prep"].Value
                    : m.Groups["art"].Success ? m.Groups["art"].Value : article;
                return m.Groups["trig"].Value + verb + " " + head + " " + noun;
            }

            if (CliticFor(poss) is not { } clitic) return m.Value;
            clitic = CaseHelper.IsAllUpper(verb) ? clitic.ToUpperInvariant() : clitic;
            var complement = direct
                ? article + " " + noun
                : CaseHelper.MatchCase(poss, "n" + ArticleFor(poss)) + " " + noun;
            var trig = m.Groups["trig"].Value;
            return trig.Length > 0 || PortugueseConverter.PronounRules.HasObjectProclisisContext(m.Result("$`"))
                ? trig + clitic + " " + verb + " " + complement
                : PortugueseVerbs.AttachEnclitic(verb, clitic) + " " + complement;
        });

        return RedundantPossessive.Replace(text, m =>
        {
            if (Regex.IsMatch(m.Value, @"\r?\n[ \t]*\r?\n|\r?\n[ \t]*[-–—]")) return m.Value;
            var noun = m.Groups["n"].Value;
            if (blocked(m.Value) || blocked(noun)) return m.Value;
            var poss = m.Groups["poss"].Value;
            if (PossessorIsSubject(m.Result("$`"), poss, Person.None) != true) return m.Value;
            var prep = m.Groups["prep"].Value;
            var head = prep.Equals("em", StringComparison.OrdinalIgnoreCase)
                ? CaseHelper.MatchCase(prep, "n" + ArticleFor(poss))
                : prep.Equals("com", StringComparison.OrdinalIgnoreCase)
                    ? prep + " " + (m.Groups["art"].Success ? m.Groups["art"].Value : CaseHelper.MatchCase(poss, ArticleFor(poss)))
                    : prep;
            return head + " " + noun;
        });
    }

    private static readonly Dictionary<string, string> RetortForms = MapForms("retrucar", "responder")
        .Concat(MapForms("revidar", "responder"))
        .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.OrdinalIgnoreCase);
    private static readonly Regex RetortVerb = new(
        DialogueBefore + @"(?<v>" + Alternatives(RetortForms.Keys) + @")" + After + @"(?!-)", Ci);
    private static readonly Regex EnglishRetort = new(
        @"\b(?:retort(?:s|ed|ing)?|repl(?:y|ies|ied|ying)|answer(?:s|ed|ing)?\s+back)\b", Ci);
    private static readonly Regex EnglishRevenge = new(
        @"\b(?:get|gets|got|getting)\s+(?:back\s+at|even\s+with)\s+" +
        @"(?:him|her|them|me|you|us|(?:the|his|her|their|my|our)\s+\p{L}+)\b|" +
        @"\b(?:revenge|vengeance|aveng(?:e|es|ed|ing))\b", Ci);
    private static readonly string[] RetaliationSource = Forms("revidar");
    private static readonly string[] RetaliationTarget = Forms("vingar");
    private const string RevengeAuxiliary =
        @"(?:decidi|decidiste|decidiu|decidimos|decidiram|quero|queres|quer|queremos|querem|" +
        @"vou|vais|vai|vamos|vão|posso|podes|pode|podemos|podem|" +
        @"(?:estou|estás|está|estamos|estão|estava|estavas|estávamos|estavam)\s+a)";
    private static readonly int[] RetaliationFiniteSlots =
        Enumerable.Range(1, 30).Concat(Enumerable.Range(39, 4)).ToArray();
    private static readonly Regex RevengeVerb = new(
        DialogueBefore + @"(?:(?<subject>eu|tu|você|ele|ela|nós|vocês|eles|elas)(?<gap>\s+))?" +
        @"(?:(?<neg>não)(?<negGap>\s+))?(?<v>" +
        Alternatives(RetaliationFiniteSlots.Select(i => RetaliationSource[i]).Append("revidar")) +
        @")" + After + @"(?!-)(?=\s*(?:[,!.?…;:]|$|[\uE000-\uF8FF]))", Ci);

    public static string RetaliationVocabulary(string text, ReferenceHint? reference, bool pronouns, Func<string, bool> blocked)
    {
        var english = ReferenceAligner.Clean(reference?.EnglishText ?? "");
        if (EnglishRetort.IsMatch(english) && EnglishRevenge.IsMatch(english)) return text;
        if (EnglishRetort.IsMatch(english))
            text = RetortVerb.Replace(text, m =>
                blocked(m.Value.StartsWith("retruc", StringComparison.OrdinalIgnoreCase) ? "retrucar" : "revidar") ||
                blocked(m.Value) || PortugueseConverter.HasNominalDeterminerBefore(m.Result("$`"))
                    ? m.Value : CaseHelper.MatchCase(m.Value, RetortForms[m.Value]));

        if (!pronouns || !EnglishRevenge.IsMatch(english) || blocked("revidar")) return text;
        return RevengeVerb.Replace(text, m =>
        {
            if (Regex.IsMatch(m.Value, @"\r?\n[ \t]*\r?\n") || blocked(m.Value) || blocked(m.Groups["v"].Value))
                return m.Value;
            var source = m.Groups["v"].Value;
            var before = m.Result("$`");
            if (!m.Groups["subject"].Success &&
                Regex.IsMatch(before, @"(?:\r?\n[ \t]*\r?\n[ \t]*|\r?\n[ \t]*-[ \t]*)$"))
                return m.Value;
            var clause = Regex.Match(before, @"[^.!?;:]*$").Value + m.Value;
            if (Regex.IsMatch(clause, @"\b(?:não|nunca)\b", Ci) !=
                Regex.IsMatch(english, @"\b(?:not|never)\b|n't\b", Ci))
                return m.Value;
            string? clitic;
            string target;
            if (source.Equals("revidar", StringComparison.OrdinalIgnoreCase))
            {
                var auxiliary = Regex.Match(before, @"\b(?<aux>" + RevengeAuxiliary + @")\s+$", Ci)
                    .Groups["aux"].Value.ToLowerInvariant();
                auxiliary = Regex.Replace(auxiliary, @"\s+", " ");
                var auxSubject = Regex.Match(before,
                    @"\b(?<s>eu|tu|você|ele|ela|nós|vocês|eles|elas)\s+" + RevengeAuxiliary + @"\s+$", Ci)
                    .Groups["s"].Value.ToLowerInvariant();
                if (auxSubject.Length > 0 && !EnglishHasSubject(english, auxSubject)) return m.Value;
                clitic = auxiliary switch
                {
                    "decidi" or "quero" or "vou" or "posso" or "estou a" => "me",
                    "decidiste" or "queres" or "vais" or "podes" or "estás a" or "estavas a" => "te",
                    "decidimos" or "queremos" or "vamos" or "podemos" or "estamos a" or "estávamos a" => "nos",
                    "decidiu" or "decidiram" or "quer" or "querem" or "vai" or "vão" or "pode" or "podem" or
                        "está a" or "estão a" or "estavam a" => "se",
                    "estava a" when auxSubject == "eu" => "me",
                    "estava a" when auxSubject is "ele" or "ela" or "você" => "se",
                    _ => null,
                };
                target = "vingar";
            }
            else
            {
                var subject = m.Groups["subject"].Value.ToLowerInvariant();
                var expected = subject switch
                {
                    "eu" => "me", "tu" => "te", "nós" => "nos",
                    "você" or "ele" or "ela" or "vocês" or "eles" or "elas" => "se",
                    _ => null,
                };
                var candidates = RetaliationFiniteSlots.Where(i =>
                        RetaliationSource[i].Equals(source, StringComparison.OrdinalIgnoreCase))
                    .SelectMany(i => i is 10 or 19 or 23 or 27 or 39
                        ? new[] { (Target: RetaliationTarget[i], Clitic: "me"), (Target: RetaliationTarget[i], Clitic: "se") }
                        : new[] { (Target: RetaliationTarget[i], Clitic: RetaliationClitic(i)) })
                    .Where(pair => expected == null || pair.Clitic == expected).Distinct().ToArray();
                if (candidates.Length != 1) return m.Value;
                (target, clitic) = candidates[0];
                if (subject.Length > 0 && !EnglishHasSubject(english, subject)) return m.Value;
            }
            if (clitic == null) return m.Value;
            target = CaseHelper.MatchCase(source, target);
            if (CaseHelper.IsAllUpper(source)) clitic = clitic.ToUpperInvariant();
            var proclisis = m.Groups["neg"].Success ||
                Regex.IsMatch(before, @"\b(?:que|se|quando|enquanto|porque|quem|talvez|nunca|não)\s+$", Ci);
            var converted = proclisis ? clitic + " " + target :
                PortugueseVerbs.AttachMesoclitic(target, clitic, false) ?? PortugueseVerbs.AttachEnclitic(target, clitic);
            return m.Groups["subject"].Value + m.Groups["gap"].Value +
                (m.Groups["neg"].Success ? m.Groups["neg"].Value + m.Groups["negGap"].Value : "") + converted;
        });
    }

    private static bool EnglishHasSubject(string english, string subject)
    {
        var englishSubject = subject switch
        {
            "eu" => "i", "tu" or "você" or "vocês" => "you",
            "ele" => "he", "ela" => "she", "nós" => "we", _ => "they",
        };
        return Regex.IsMatch(english, @"\b" + englishSubject + @"\b", Ci);
    }

    private static string RetaliationClitic(int slot) => slot switch
    {
        1 or 6 or 14 or 19 => "me",
        2 or 7 or 11 or 15 or 20 or 24 or 28 or 40 => "te",
        4 or 12 or 17 or 21 or 25 or 29 or 41 or 42 => "nos",
        _ => "se",
    };

    private static string FinitePattern(string infinitive) =>
        Alternatives(Forms(infinitive).Where((_, index) => index is not (34 or 35 or 36 or 37 or 38)));

    private static readonly Regex PrankReference = new(
        @"\b(?:pranks?|practical\s+jokes?|jokes?|hoax(?:es)?)\b", Ci);
    private static readonly Regex TroteOtherSense = new(
        @"\b(?:cavalos?|éguas?|potros?|galope|hipismo|andadura|trotar|rasgado|peludo|" +
        @"horses?|ponies|gallop|trotting|hazing|freshmen|initiation)\b", Ci);
    private static readonly Regex PassingTroteBefore = new(
        @"\b(?:" + FinitePattern("passar") +
        @"|dar|dá|dão|deu|deram|dei|demos|dava|davam|fazer|faz|fez)(?:\s+(?:o|os|um|uns))?\s+$", Ci);
    private static readonly Dictionary<string, string> PassPrankForms = MapForms("passar", "pregar");
    private static readonly Regex PassPrank = new(
        DialogueBefore + @"(?<v>" + FinitePattern("passar") + @")(?<gap>\s+)" +
        @"(?<article>um|uns|o|os)(?<space>\s+)(?<noun>trote|trotes)" + After +
        @"(?=\s*(?:[,!.?…;:]|$|[\uE000-\uF8FF]))", Ci);

    public static bool IsPrankTrote(Match m, string text, ReferenceHint? reference)
    {
        var english = ReferenceAligner.Clean(reference?.EnglishText ?? "");
        var before = m.Result("$`");
        var key = m.Groups["key"];
        var after = text[(key.Index + key.Length)..];
        return PrankReference.IsMatch(english) && !TroteOtherSense.IsMatch(english) &&
            !TroteOtherSense.IsMatch(text) && !Regex.IsMatch(m.Value, @"\r?\n[ \t]*\r?\n") &&
            !m.Groups["det"].Value.Trim().Equals("a", StringComparison.OrdinalIgnoreCase) &&
            !Regex.IsMatch(after, @"^\s+(?:estudantil|universitári[oa]s?|de\s+(?:peludo|cavalo))\b", Ci) &&
            !PassingTroteBefore.IsMatch(before + m.Groups["det"].Value);
    }

    public static string PrankActions(string text, ReferenceHint? reference, Func<string, bool> blocked)
    {
        var english = ReferenceAligner.Clean(reference?.EnglishText ?? "");
        if (TroteOtherSense.IsMatch(text) || TroteOtherSense.IsMatch(english)) return text;
        return PassPrank.Replace(text, m =>
        {
            var noun = m.Groups["noun"].Value;
            var article = m.Groups["article"].Value;
            var plural = noun.EndsWith("s", StringComparison.OrdinalIgnoreCase);
            if (Regex.IsMatch(m.Value, @"\r?\n[ \t]*\r?\n") ||
                blocked("passar") || blocked("trote") || blocked(noun) || blocked(m.Value) ||
                plural != article.EndsWith("s", StringComparison.OrdinalIgnoreCase))
                return m.Value;
            var female = article.ToLowerInvariant() switch
            {
                "um" => "uma", "uns" => "umas", "o" => "a", _ => "as",
            };
            return CaseHelper.MatchCase(m.Groups["v"].Value, PassPrankForms[m.Groups["v"].Value]) +
                m.Groups["gap"].Value + CaseHelper.MatchCase(article, female) + m.Groups["space"].Value +
                CaseHelper.MatchCase(noun, plural ? "partidas" : "partida");
        });
    }

    private static readonly Regex FerrarSlang = new(
        DialogueBefore + @"(?<verb>" + Alternatives(Forms("ferrar").Append("ferrá")) + @")" +
        @"(?<clitic>-(?:me|te|se|nos|vos|lhe|lhes|o|a|os|as|lo|la|los|las))?" + After +
        @"(?<com>\s+com\b)?", Ci);
    private static readonly Regex EnglishScrewSlang = new(
        @"\bscrew(?:ed|ing|s)?(?:\s+\p{L}+){0,5}\s+(?:up|over)\b|" +
        @"\b(?:am|is|are|was|were|been|be|get|got|getting|you're|we're|they're|I'm|he's|she's)\s+(?:really\s+|totally\s+)?screwed\b", Ci);
    private static readonly Regex FerrarLiteral = new(
        @"\b(?:cavalos?|éguas?|potros?|gado|ferraduras?|ferro|âncoras?|velas?|toldos?|unhas?|dentes?|mordidas?|murros?|pontapés?|reprimendas?|sono|dormir|braços?|dedos?)\b", Ci);
    private static readonly Regex FerrarSlangObject = new(
        @"^\s+(?:tudo|isso|isto|aquilo|" +
        @"(?:(?:o|a|os|as|meu|minha|teu|tua|seu|sua|nosso|nossa)\s+){0,2}" +
        @"(?:planos?|negócios?|vidas?|carreiras?|relaç(?:ão|ões)|oportunidades?|hipóteses?|chances?|futuro|reputaç(?:ão|ões)|" +
        @"casos?|investigaç(?:ão|ões)|operaç(?:ão|ões)|situaç(?:ão|ões)|coisas?))\b", Ci);

    public static string FerrarSlangVocabulary(string text, ReferenceHint? reference) => FerrarSlang.Replace(text, m =>
    {
        var before = m.Result("$`");
        var after = m.Result("$'");
        var local = Regex.Match(before, @"[^.!?;]*$").Value + m.Value + Regex.Match(after, @"^[^.!?;]*").Value;
        if (FerrarLiteral.IsMatch(local) ||
            Regex.IsMatch(after, @"^\s+(?:a\s+(?:\p{L}+(?:ar|er|ir)|uma\s+(?:ideia|emoção))|ao\s+sentimento)\b", Ci))
            return m.Value;
        var personalClitic = Regex.IsMatch(m.Groups["clitic"].Value, @"^-(?:me|te|se|nos|vos)$", Ci) ||
            Regex.IsMatch(before, @"\b(?:me|te|se|nos|vos)\s+$", Ci) ||
            Regex.IsMatch(m.Groups["verb"].Value, @"^ferrad[oa]s?$", Ci) &&
            Regex.IsMatch(before, @"\b(?:estou|estás|está|estamos|estão|estava|estavas|estávamos|estavam|ficou|fiquei|ficaste|ficámos|ficaram)\s+(?:(?:mesmo|completamente|totalmente|bem)\s+)?$", Ci);
        var knownObject = FerrarSlangObject.IsMatch(after);
        if (m.Groups["com"].Success && !knownObject) return m.Value;
        if (!personalClitic && !knownObject &&
            !EnglishScrewSlang.IsMatch(reference?.EnglishText ?? "")) return m.Value;
        return CaseHelper.MatchCase(m.Groups["verb"].Value, "lix" + m.Groups["verb"].Value[4..].ToLowerInvariant()) +
            m.Groups["clitic"].Value;
    });

    private static readonly Regex HabitualAlways = new(
        @"(?<head>(?:^|[.!?;:]\s+|\n|[\uE000-\uF8FF]\s*|(?<![\p{L}\p{M}-])(?:porque|que|quando|enquanto|se|mas|e)\s+)" +
        @"(?:(?:eu|tu|ele|ela|nós|vocês|eles|elas)\s+)?)" +
        @"(?<adv>sempre)(?<space>\s+)(?<clitic>(?:me|te|se|nos|vos|lhe|lhes|o|a|os|as)\s+)?" +
        @"(?<v>\p{L}+)" + After + @"(?!-)", Ci);

    private static readonly Dictionary<string, string> PolitePluralCommands = new[]
    {
        "avisar", "informar", "comunicar", "telefonar", "ligar", "chamar", "contactar", "contatar",
        "notificar", "alertar", "aguardar", "esperar", "comparecer", "procurar", "verificar", "confirmar",
        "enviar", "responder", "entrar", "sair", "parar", "regressar", "voltar", "continuar",
        "seguir", "entregar", "ajudar", "apoiar", "proteger", "trazer", "fazer", "dizer", "dar",
        "ir", "ser", "estar", "ter", "vir", "ver", "pedir", "manter",
    }.ToDictionary(verb => verb, verb => verb switch
    {
        "seguir" => "sigam", "trazer" => "tragam", "fazer" => "façam", "dizer" => "digam",
        "dar" => "deem", "ir" => "vão", "ser" => "sejam", "estar" => "estejam", "ter" => "tenham",
        "vir" => "venham", "ver" => "vejam", "pedir" => "peçam", "manter" => "mantenham", "sair" => "saiam",
        "contatar" => "contactem", _ => Forms(verb)[26],
    }, StringComparer.OrdinalIgnoreCase);
    private static readonly Regex PoliteInfinitive = new(
        Before + @"(?<please>por\s+favor)(?<gap>\s*,?\s+)(?<neg>não\s+)?" +
        @"(?<verb>" + Alternatives(PolitePluralCommands.Keys) + @")" +
        @"(?<clitic>-(?:me|te|se|nos|vos|lhe|lhes|o|a|os|as))?" + After, Ci);
    private static readonly Regex PluralConditionalAddressee = new(
        @"^\s+(?:(?:o|a|os|as|ao|à|aos|às)\s+(?:central|polícia|comando|equipa|autoridades|reforços|documentos|relatório)\s+)?" +
        @"(?:se|quando|assim\s+que|logo\s+que)(?:\s+vocês)?\s+(?:(?:não|já|ainda|também)\s+){0,2}" +
        @"(?<verb>[\p{L}\p{M}]+)\b", Ci);

    public static string PoliteInfinitiveCommands(string text) => PoliteInfinitive.Replace(text, m =>
    {
        var after = m.Result("$'");
        var conditional = PluralConditionalAddressee.Match(after);
        if (!conditional.Success) return m.Value;
        var conditionalVerb = conditional.Groups["verb"].Value.ToLowerInvariant();
        if (Regex.IsMatch(conditionalVerb, @"^(?:houverem|choverem|nevarem|acontecerem|existirem|ocorrerem)$", Ci))
            return m.Value;
        var plural = Regex.IsMatch(conditionalVerb,
            @"^(?:virem|forem|tiverem|estiverem|fizerem|disserem|derem|souberem|puderem|quiserem|trouxerem|mantiverem)$", Ci);
        if (!plural && Regex.IsMatch(conditionalVerb, @"(?:arem|erem|irem)$", Ci))
        {
            var infinitive = conditionalVerb[..^2];
            plural = PortugueseVerbs.IsKnownInfinitive(infinitive) || NaturaDictionary.Load(true).Check(infinitive);
        }
        if (!plural) return m.Value;
        var verb = m.Groups["verb"].Value;
        var target = CaseHelper.MatchCase(verb, PolitePluralCommands[verb]);
        var clitic = m.Groups["clitic"].Value;
        if (clitic.Length > 0)
            target = m.Groups["neg"].Success
                ? (CaseHelper.IsAllUpper(verb) ? clitic[1..].ToUpperInvariant() : clitic[1..].ToLowerInvariant()) + " " + target
                : PortugueseVerbs.AttachEnclitic(target, clitic[1..]);
        return m.Groups["please"].Value + m.Groups["gap"].Value + m.Groups["neg"].Value + target;
    });
    private static readonly HashSet<string> HabitualVerbs = BuildHabitualVerbs();

    private static HashSet<string> BuildHabitualVerbs()
    {
        var dictionary = NaturaDictionary.Load(true);
        return new(new[]
        {
            "saber", "perceber", "reconhecer", "descobrir", "notar", "encontrar", "chegar", "voltar",
            "falar", "dizer", "responder", "perguntar", "contar", "pensar", "achar", "acreditar",
            "trabalhar", "estudar", "comprar", "pagar", "comer", "beber", "ajudar", "esquecer",
            "ganhar", "perder", "mentir", "ouvir", "ver", "ler", "fazer", "apoiar", "defender", "proteger", "acompanhar",
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
    private static readonly Regex ModalAlways = new(
        Before + @"(?<always>sempre)(?<gap>\s+)(?<modal>posso|podes|pode|podemos|podem|podia|podias|podíamos|podiam|" +
        @"poderia|poderias|poderíamos|poderiam|devo|deves|deve|devemos|devem|devia|devias|devíamos|deviam)" +
        @"(?=\s+\p{L}+(?:ar|er|ir|ôr)" + After + @")", Ci);

    private static readonly Dictionary<string, string> SayForms = BuildSayForms();
    private static Dictionary<string, string> BuildSayForms()
    {
        var dizer = new[]
        {
            "dizer", "digo", "dizes", "diz", "dizemos", "dizem",
            "disse", "disseste", "disse", "disseram",
            "dizia", "dizias", "dizíamos", "diziam",
            "direi", "dirás", "dirá", "diremos", "dirão",
            "diria", "dirias", "diríamos", "diriam",
            "diga", "digas", "digamos", "digam",
            "dissesse", "dissesses", "disséssemos", "dissessem",
            "dizeres", "dizermos", "dizerem",
            "dizendo", "dito", "dita", "ditos", "ditas",
            "dissera", "disseras", "disséramos", "dissemos",
        };
        var result = Forms("falar").Select((form, index) => (form, target: dizer[index]))
            .GroupBy(pair => pair.form, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First().target, StringComparer.OrdinalIgnoreCase);
        foreach (var (source, target) in new[]
        {
            ("falámos", "dissemos"), ("falais", "dizeis"), ("falastes", "dissestes"),
            ("faláveis", "dizíeis"), ("falareis", "direis"), ("falaríeis", "diríeis"),
            ("faleis", "digais"), ("falásseis", "dissésseis"), ("falardes", "dizerdes"),
            ("faláreis", "disséreis"), ("falai", "dizei"),
        })
            result[source] = target;
        return result;
    }

    private static string? SayForm(string verb, string before, string english)
    {
        var target = SayForms[verb];
        if (verb.Equals("falamos", StringComparison.OrdinalIgnoreCase))
        {
            if (Regex.IsMatch(english, @"\bwe\s+(?:(?:always|just|only)\s+)?(?:said|talked|spoke)\b", Ci))
                target = "dissemos";
            else if (!Regex.IsMatch(english, @"\bwe\s+(?:(?:always|just|only)\s+)?(?:say|talk|speak)\b", Ci))
                return null;
        }
        if (Regex.IsMatch(verb, @"^falar(?:es|mos|des|em)?$", Ci))
        {
            var conditional = Regex.IsMatch(before, @"\b(?:se|quando|assim\s+que|logo\s+que|sempre\s+que|toda\s+(?:a\s+)?vez\s+que)" +
                @"\s+(?:(?:eu|tu|ele|ela|nós|vós|vocês|eles|elas|alguém|ninguém|todos|todas|" +
                @"(?:(?:o|a|os|as|um|uma|uns|umas|meu|minha|teu|tua|seu|sua)\s+){1,2}[\p{L}\p{M}]+|" +
                @"(?-i:\p{Lu}[\p{L}\p{M}]+(?:\s+\p{Lu}[\p{L}\p{M}]+){0,2}))\s+)?" +
                Adverbs + @"(?:(?:me|te|lhe|lhes|nos|vos)\s+)?$", Ci);
            if (conditional)
                target = "disser" + verb[5..].ToLowerInvariant();
            else if (Regex.IsMatch(before, @"\b(?:se|quando|assim\s+que|logo\s+que)\s+[^,;.!?]*$", Ci) &&
                !Regex.IsMatch(before, @"\b(?:a|de|para|quiser(?:es|mos|em)?|puder(?:es|mos|em)?|vou|vais|vai|vamos|vão)\s+$", Ci))
                return null;
        }
        return CaseHelper.MatchCase(verb, target);
    }

    private static readonly Regex ContentSpeechVerb = new(
        @"(?<![\p{L}\p{M}\p{N}_])(?<![\p{L}\p{M}]-)(?<verb>" + Alternatives(SayForms.Keys) + @")" +
        @"(?<clitic>-(?:me|te|lhe|lhes|nos|vos))?(?![\p{L}\p{M}\p{N}_-])" +
        @"(?<recipient>\s+(?:a|para)\s+(?:mim|ti|si|ele|ela|eles|elas|nós|vós|você|vocês)" +
        @"(?![\p{L}\p{M}\p{N}_-]))?" +
        @"(?:(?<gap>\s+)(?<algo>algo)(?![\p{L}\p{M}\p{N}_-]))?", Ci);
    private static readonly Regex SpeechContent = new(
        @"^\s+(?<content>alguma\s+coisa|coisa\s+alguma|qualquer\s+coisa|nada|algo|tudo|isto|isso|aquilo|" +
        @"(?:a\s+)?verdade|mentiras|(?:uma|a|as|muitas)\s+mentiras?|" +
        @"(?:uma|duas|três|algumas|muitas|estas|essas)\s+(?:coisas?|palavras?))" +
        @"(?![\p{L}\p{M}\p{N}_-])", Ci);
    private static readonly Regex EnglishSay = new(@"\b(?:say|says|said|saying|tell|tells|told|telling)\b", Ci);
    private static readonly Regex EnglishSpeechActivity = new(@"\b(?:talk(?:s|ed|ing)?|speak(?:s|ing)?|spoke|spoken)\b", Ci);
    private static readonly Regex SpeechQuestionBefore = new(
        @"(?<![\p{L}\p{M}])o\s+que\s+(?:é\s+que\s+)?(?:(?:eu|tu|você|ele|ela|nós|vós|vocês|eles|elas)\s+)?" +
        @"(?:(?:me|te|lhe|nos|vos|lhes)\s+)?" +
        @"(?:(?:estou|estás|está|estamos|estão|estava|estavas|estávamos|estavam)\s+(?:a\s+)?)?" +
        Adverbs + "$", Ci);

    private static string SpeechContentVocabulary(string text, string english, Func<string, bool>? isBlocked)
    {
        var matches = ContentSpeechVerb.Matches(text);
        return ContentSpeechVerb.Replace(text, m =>
        {
            var verb = m.Groups["verb"].Value;
            if (isBlocked?.Invoke("falar") == true || isBlocked?.Invoke(verb) == true) return m.Value;
            var before = Regex.Match(m.Result("$`"), @"[^.!?;:]*$").Value;
            if (Regex.IsMatch(verb, @"^falas?$", Ci) && PortugueseConverter.HasNominalDeterminerBefore(before))
                return m.Value;
            var after = m.Groups["gap"].Value + m.Groups["algo"].Value + m.Result("$'");
            var content = SpeechContent.Match(after);
            var question = SpeechQuestionBefore.Match(before);
            var confirmed = matches.Count == 1 && EnglishSay.IsMatch(english) && !EnglishSpeechActivity.IsMatch(english);
            var contentQuestion = question.Success &&
                !Regex.IsMatch(before[..question.Index], @"\b(?:de|do|da|sobre|com|por|em|no|na)\s+$", Ci);
            var reported = !Regex.IsMatch(before, @"\bsem\s+$", Ci) &&
                Regex.IsMatch(after, @"^\s+(?:que|o\s+que)\b|^\s*[:,]?\s*[""“«]", Ci);
            var addressed = m.Groups["recipient"].Success && Regex.IsMatch(after, @"^\s*[,.;!?]|^\s*$", Ci);
            if (!content.Success && !(confirmed && (contentQuestion || reported || addressed))) return m.Value;
            if (content.Success && isBlocked?.Invoke(content.Groups["content"].Value) == true) return m.Value;
            var target = SayForm(verb, before, english);
            if (target is null) return m.Value;
            var clitic = m.Groups["clitic"].Value;
            if (clitic.Length > 0)
                target = PortugueseVerbs.AttachMesoclitic(target, clitic[1..]) ??
                    PortugueseVerbs.AttachEnclitic(target, clitic[1..]);
            return target + m.Groups["recipient"].Value + (m.Groups["algo"].Success
                ? m.Groups["gap"].Value + CaseHelper.MatchCase(m.Groups["algo"].Value, "alguma coisa") : "");
        });
    }

    private const string NonsenseModifiers = @"(?:(?:sempre|só|apenas|muit[oa]s|tant[oa]s|algum[ao]s|ess[ae]s|est[ae]s)\s+){0,2}";
    private static readonly Regex SpokenNonsenseBefore = new(
        DialogueBefore + @"(?:" + Alternatives(SayForms.Keys) + "|" +
        PortugueseConverter.PronounRules.DizerVerbForms + @")\s+" + NonsenseModifiers + "$", Ci);
    internal static bool KeepSpokenTolices(string before) => SpokenNonsenseBefore.IsMatch(before);
    private static readonly Regex SayNonsense = new(
        DialogueBefore + @"(?<verb>" + Alternatives(SayForms.Keys) + @")" +
        @"(?<gap>\s+" + NonsenseModifiers + @")" +
        @"(?<noun>bobagens|tolices|disparates|besteiras|asneiras)" + After, Ci);
    private static readonly Regex EveryTime = new(
        DialogueBefore + @"(?<phrase>toda\s+(?:a\s+)?vez\s+que)" + After, Ci);
    private static readonly Regex IntoxicatedSlang = new(
        DialogueBefore + @"(?<state>estou|estás|está|estamos|estão|estava|estavas|estávamos|estavam|estar|estiver|estiverem|" +
        Alternatives(Forms("ficar").Concat(Forms("andar"))) + @")" +
        @"(?<gap>\s+(?:(?:muito|mesmo|completamente|totalmente|bem|todo|toda|todos|todas)\s+){0,2})" +
        @"(?<adj>doid[ãa]o|doidona|doidões|doidonas)" + After, Ci);
    private static readonly Regex EnglishIntoxication = new(
        @"\b(?:stoned|drugged)\b|\b(?:get|gets|got|getting|be|been|being|am|is|are|was|were|feel|feels|felt|" +
        @"I'm|he's|she's|you're|we're|they're)\s+(?:(?:really|very|so|too|completely)\s+){0,2}high\b" +
        @"(?!\s+(?:scores?|grades?|marks?|hopes?|expectations?|praise|school|heels?|up|above|blood|temperature|priority|risk|in)\b)" +
        @"(?!\s+on\s+(?:life|love|adrenaline|the\s+list)\b)", Ci);
    private static readonly Regex DrugIntoxicationComplement = new(
        @"^\s+(?:de|com)\s+(?:(?:a|as|o|os|muita|muitas|muito|muitos)\s+)?" +
        @"(?:drogas?|cocaína|heroína|canábis|cannabis|maconha|marijuana|erva|ganza|haxixe|ecstasy|mdma|" +
        @"metanfetamina|cetamina|ketamina)\b", Ci);

    public static string SpokenIdioms(string text, ReferenceHint? reference, bool vocabulary, Func<string, bool> isBlocked)
    {
        if (vocabulary)
        {
            text = SayNonsense.Replace(text, m =>
            {
                var verb = m.Groups["verb"].Value;
                if (isBlocked("falar") || isBlocked(verb) ||
                    new[] { "bobagens", "tolices", "disparates", "besteiras", "asneiras" }.Any(isBlocked)) return m.Value;
                var before = Regex.Match(m.Result("$`"), @"[^.!?;]*$").Value;
                var target = SayForm(verb, before, reference?.EnglishText ?? "");
                return target is null ? m.Value : target + m.Groups["gap"].Value + m.Groups["noun"].Value;
            });
            text = IntoxicatedSlang.Replace(text, m =>
            {
                if (isBlocked(m.Groups["adj"].Value)) return m.Value;
                var after = Regex.Match(m.Result("$'"), @"^[^.!?;]*").Value;
                if (Regex.IsMatch(after, @"^\s+(?:por|de\s+(?:alegria|felicidade|amor|entusiasmo))\b", Ci) ||
                    !EnglishIntoxication.IsMatch(reference?.EnglishText?.Replace('’', '\'') ?? "") &&
                    !DrugIntoxicationComplement.IsMatch(after)) return m.Value;
                var target = m.Groups["adj"].Value.ToLowerInvariant() switch
                {
                    "doidona" => "pedrada", "doidonas" => "pedradas", "doidões" => "pedrados", _ => "pedrado",
                };
                return m.Groups["state"].Value + m.Groups["gap"].Value +
                    CaseHelper.MatchCase(m.Groups["adj"].Value, target);
            });
        }
        return EveryTime.Replace(text, m =>
        {
            var before = Regex.Match(m.Result("$`"), @"[^.!?;]*$").Value;
            var alreadyHabitual = Regex.IsMatch(before,
                @"\bsempre\b(?!.*\b(?:que|se|quando|porque|enquanto|mas|e)\b)", Ci | RegexOptions.Singleline);
            var negative = Regex.IsMatch(before, @"\b(?:não|nem|nunca|raramente)\b", Ci);
            return CaseHelper.MatchCase(m.Value,
                alreadyHabitual ? negative ? "de cada vez que" : "quando" : "sempre que");
        });
    }

    private static readonly Dictionary<string, string> DurationNouns = new(StringComparer.OrdinalIgnoreCase)
    {
        ["noite"] = "night", ["noites"] = "nights", ["dia"] = "day", ["dias"] = "days",
        ["hora"] = "hour", ["horas"] = "hours", ["semana"] = "week", ["semanas"] = "weeks",
        ["mês"] = "month", ["meses"] = "months", ["ano"] = "year", ["anos"] = "years",
        ["manhã"] = "morning", ["manhãs"] = "mornings", ["tarde"] = "afternoon", ["tardes"] = "afternoons",
        ["minuto"] = "minute", ["minutos"] = "minutes", ["segundo"] = "second", ["segundos"] = "seconds",
    };
    private static readonly Dictionary<string, string> DurationAdjectives = new(StringComparer.OrdinalIgnoreCase)
    {
        ["long"] = "long", ["curt"] = "short", ["dur"] = "hard", ["difícil"] = "difficult",
        ["difíceis"] = "difficult", ["cansativ"] = "tiring", ["tranquil"] = "quiet", ["fri"] = "cold",
    };
    private static readonly Regex QuantifiedDuration = new(
        Before + @"(?<quantity>muito|muita|muitos|muitas)(?<gap>\s+)" +
        @"(?<adjective>(?:long|curt|dur|cansativ|tranquil|fri)[oa]s?|difícil|difíceis)\s+" +
        @"(?<noun>" + Alternatives(DurationNouns.Keys) + @")" + After, Ci);

    private static string DurationWordOrder(string text, ReferenceHint? reference) =>
        QuantifiedDuration.Replace(text, m =>
        {
            var noun = m.Groups["noun"].Value;
            var adjective = m.Groups["adjective"].Value;
            var plural = noun.EndsWith("s", StringComparison.OrdinalIgnoreCase);
            var feminine = Regex.IsMatch(noun, @"^(?:noites?|horas?|semanas?|manhãs?|tardes?)$", Ci);
            var invariant = adjective.StartsWith("dif", StringComparison.OrdinalIgnoreCase);
            var ending = (feminine ? "a" : "o") + (plural ? "s" : "");
            if (invariant ? !adjective.Equals(plural ? "difíceis" : "difícil", StringComparison.OrdinalIgnoreCase)
                : !adjective.EndsWith(ending, StringComparison.OrdinalIgnoreCase))
                return m.Value;
            var quantity = m.Groups["quantity"].Value;
            var stem = invariant ? adjective : adjective[..^ending.Length];
            var englishPhrase = DurationAdjectives[stem] + @"\s+" + DurationNouns[noun];
            var english = reference?.EnglishText ?? "";
            var degree = quantity.Equals("muito", StringComparison.OrdinalIgnoreCase) ||
                Regex.IsMatch(english, @"\bvery\s+" + englishPhrase + @"\b", Ci) &&
                !Regex.IsMatch(english, @"\b(?:(?:many|a\s+lot\s+of|lots\s+of)\s+(?:very\s+)?|(?:not|never|hardly)\s+very\s+)" +
                    englishPhrase + @"\b", Ci);
            if (!degree && !quantity.Equals("muit" + ending, StringComparison.OrdinalIgnoreCase))
                return m.Value;
            var reordered = degree ? noun + " muito " + adjective : quantity + " " + noun + " " + adjective;
            return CaseHelper.MatchCase(m.Value, reordered);
        });

    public static string NaturalWordOrder(string text, ReferenceHint? reference, PersonalNameArticles names, bool pronouns)
    {
        text = DurationWordOrder(text, reference);
        text = EchoTopicQuestion.Replace(text, m =>
        {
            if (!Regex.IsMatch(reference?.EnglishText ?? "", @"\bwhat\s+did\s+you\s+and\b[^.!?]*\b(?:fight|argue|talk|discuss)\b", Ci))
                return m.Value;
            var subject = m.Groups["subject"].Value;
            if (!Regex.IsMatch(subject, @"\b(?:você|tu)\s+e\s+", Ci)) return m.Value;
            subject = Regex.Replace(subject, @"(?<=\be\s)(?<name>(?-i:\p{Lu}\p{Ll}+))\b", name =>
            {
                var value = name.Groups["name"].Value;
                var article = PersonalNameArticle(value, names);
                return article == null ? value : article + " " + value;
            }, Ci);
            return m.Groups["head"].Value + CaseHelper.Capitalize(subject) + " " +
                m.Groups["verb"].Value + " sobre o quê";
        });
        if (Regex.IsMatch(reference?.EnglishText ?? "", @"\b(?:can|could|may|might|should|must)\s+always\b", Ci))
            text = ModalAlways.Replace(text, m => CaseHelper.MatchCase(m.Groups["always"].Value,
                m.Groups["modal"].Value) + m.Groups["gap"].Value +
                (CaseHelper.IsAllUpper(m.Groups["always"].Value) ? "SEMPRE" : "sempre"));
        text = HabitualAlways.Replace(text, m =>
        {
            var verb = m.Groups["v"].Value;
            if (!HabitualVerbs.Contains(verb)) return m.Value;
            var head = m.Groups["head"].Value;
            var clitic = m.Groups["clitic"].Value.Trim();
            var target = CaseHelper.MatchCase(m.Groups["adv"].Value, verb);
            if (clitic.Length > 0)
            {
                if (!pronouns || PortugueseVerbs.IsFutureOrConditional(verb)) return m.Value;
                if (Forms("achar").Contains(verb, StringComparer.OrdinalIgnoreCase))
                    return m.Value;
                var before = m.Result("$`") + head;
                var attracts = PortugueseConverter.PronounRules.AfterSubordinateSubject(before) ||
                    Regex.IsMatch(before, @"\b(?:que|quando|porque|enquanto|se)\s+$", Ci);
                target = attracts
                    ? CaseHelper.MatchCase(m.Groups["adv"].Value, clitic) + " " + verb.ToLowerInvariant()
                    : PortugueseVerbs.AttachEnclitic(target, clitic);
                if (CaseHelper.IsAllUpper(m.Value)) target = target.ToUpperInvariant();
            }
            return head + target + m.Groups["space"].Value +
                (CaseHelper.IsAllUpper(m.Groups["adv"].Value) ? "SEMPRE" : "sempre");
        });
        return RecoverableProgressiveSubject.Replace(text, m =>
            m.Groups["link"].Value + m.Groups["space"].Value + m.Groups["verb"].Value);
    }

    private static readonly Regex HappeningQuestion = new(
        @"(?<head>(?:^|(?<=[.!?;\n]))[\s\p{Co}-]*)(?<question>o\s+que)(?<gap>\s+)(?<aux>está|estava)" +
        @"\s+(?:havendo|acontecendo|a\s+(?:haver|acontecer))(?=\s*\?)", Ci);

    public static string HappeningQuestions(string text, ReferenceHint? reference) =>
        reference is not null && !Regex.IsMatch(reference.EnglishText ?? "",
            @"\bwhat(?:['’]s|\s+(?:is|was))\s+(?:going\s+on|happening)\b", Ci)
            ? text
            : HappeningQuestion.Replace(text, m => m.Groups["head"].Value +
                m.Groups["question"].Value + m.Groups["gap"].Value +
                (CaseHelper.IsAllUpper(m.Value) ? "SE " : "se ") + m.Groups["aux"].Value +
                (CaseHelper.IsAllUpper(m.Value) ? " A PASSAR" : " a passar"));

    private static readonly Regex WaitUntilPersonal = new(
        @"(?<![\p{L}\p{M}])(?<![\p{L}\p{M}]-)(?<wait>espere|espera)(?<gap>\s+)(?<until>até)(?<space>\s+)" +
        @"(?<inf>\p{L}+(?:ar|er|ir)|pôr)" + After + @"(?!-|\s+(?:eu|ele|ela|eles|elas|nós|vocês)\b)", Ci);
    private static readonly Regex EnglishWaitUntilYou = new(
        @"\bwait\s+(?:until|till)\s+you\b", Ci);

    public static string WaitUntil(string text, ReferenceHint? reference) =>
        EnglishWaitUntilYou.IsMatch(reference?.EnglishText ?? "")
            ? WaitUntilPersonal.Replace(text, m =>
            {
                var infinitive = m.Groups["inf"].Value;
                if (!PortugueseVerbs.IsInfinitive(infinitive) ||
                    !PortugueseVerbs.IsKnownInfinitive(infinitive) && !NaturaDictionary.Load(true).Check(infinitive)) return m.Value;
                return CaseHelper.MatchCase(m.Groups["wait"].Value, "espera") + m.Groups["gap"].Value +
                    m.Groups["until"].Value + m.Groups["space"].Value + PortugueseVerbs.PersonalInfinitive(infinitive, "es");
            }) : text;

    private static readonly Regex AcquisitionBeneficiary = new(
        Before + @"(?<verb>" + Alternatives(new[] { "alugar", "comprar", "reservar", "preparar", "arrendar" }.SelectMany(Forms)) +
        @")(?<object>(?:\s+(?!por\b)[\p{L}\p{M}]+){0,4})(?<gap>\s+)(?<prep>por)(?<space>\s+)" +
        @"(?<person>você|ti|mim|nós|ele|ela|eles|elas)" + After, Ci);
    private static readonly Regex EnglishAcquisitionBeneficiary = new(
        @"\b(?:rent(?:ed|s|ing)?|bought|buy|buys|buying|reserv(?:e|ed|es|ing)|prepar(?:e|ed|es|ing))\b" +
        @"[^.!?;]{0,80}\bfor\s+(?:you|me|us|him|her|them)\b", Ci);

    public static string AcquisitionBeneficiaries(string text, ReferenceHint? reference) =>
        EnglishAcquisitionBeneficiary.IsMatch(reference?.EnglishText ?? "") &&
        !Regex.IsMatch(reference?.EnglishText ?? "", @"\b(?:instead\s+of|on\s+(?:your|his|her|their|my|our)\s+behalf|by\s+you)\b", Ci)
            ? AcquisitionBeneficiary.Replace(text, m => m.Groups["verb"].Value + m.Groups["object"].Value +
                m.Groups["gap"].Value + CaseHelper.MatchCase(m.Groups["prep"].Value, "para") +
                m.Groups["space"].Value + m.Groups["person"].Value) : text;

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

    private static readonly HashSet<string> FemininePersonalNames = new(StringComparer.Ordinal)
    {
        "Ana", "Maria", "Joana", "Mariana", "Marta", "Sofia", "Inês", "Beatriz", "Carolina",
        "Catarina", "Leonor", "Teresa", "Isabel", "Helena", "Patrícia", "Diana", "Sara",
        "Sandra", "Paula", "Luísa", "Susana", "Cristina", "Alice", "Laura", "Linda",
        "Clara", "Rosa", "Eva", "Emma", "Emily", "Sarah", "Mary", "Elizabeth", "Jennifer",
        "Jessica", "Rachel", "Susan", "Margaret", "Karen", "Nancy", "Abigail",
    };
    private static readonly HashSet<string> MasculinePersonalNames = new(StringComparer.Ordinal)
    {
        "João", "José", "Manuel", "António", "Pedro", "Miguel", "Tiago", "Diogo", "Francisco",
        "Rui", "Luís", "Carlos", "Paulo", "Jorge", "Ricardo", "André", "Tomás", "Gonçalo",
        "Nuno", "Daniel", "David", "Gabriel", "Rafael", "Bruno", "Alexandre", "Eduardo",
        "John", "Jack", "James", "Robert", "Michael", "William", "Richard", "Thomas", "Charles",
        "Henry", "George", "Kevin", "Frank", "Peter", "Paul", "Mark", "Steven", "Anthony",
    };
    private static readonly Regex ThinkPersonalName = new(
        Before + @"(?<verb>" + Alternatives(Forms("pensar")) + @")(?:-(?:me|te|se|nos|vos|lhe|lhes))?" +
        @"(?<gap>\s+(?:(?:sempre|também|ainda|só|apenas|muito|constantemente)\s+){0,2})" +
        @"(?<complement>em\s+[\p{L}\p{M}]+" + After +
        @"(?:\s+(?:e|ou)\s+em\s+[\p{L}\p{M}]+" + After + @")*)", Ci);
    private static readonly Regex PersonalNamePreposition = new(
        Before + @"(?<prep>em)(?<space>\s+)(?<name>[\p{L}\p{M}]+)" + After, Ci);

    private static string? PersonalNameArticle(string name, PersonalNameArticles names)
    {
        if (names.TryGet(name, out var selected)) return selected == "none" ? null : selected;
        if (FemininePersonalNames.Contains(name)) return "a";
        if (MasculinePersonalNames.Contains(name)) return "o";
        names.ReportUnknown(name);
        return null;
    }

    public static string PersonalNameComplements(string text, PersonalNameArticles names) => ThinkPersonalName.Replace(text, m =>
        m.Value[..(m.Groups["complement"].Index - m.Index)] +
        PersonalNamePreposition.Replace(m.Groups["complement"].Value, match => ContractPersonalName(match, names)));

    private static string ContractPersonalName(Match m, PersonalNameArticles names)
    {
        var name = m.Groups["name"].Value;
        var selected = PersonalNameArticle(name, names);
        var article = selected == null ? null : "n" + selected;
        if (article == null) return m.Value;
        return CaseHelper.MatchCase(m.Groups["prep"].Value, article) + m.Groups["space"].Value + name;
    }

    private static readonly Dictionary<string, string> QuestionActions = new(StringComparer.OrdinalIgnoreCase)
    {
        ["ligue"] = "ligo", ["telefone"] = "telefono", ["pergunte"] = "pergunto",
        ["diga"] = "digo", ["espere"] = "espero", ["deixe"] = "deixo",
        ["vá"] = "vou", ["venha"] = "venho", ["ajude"] = "ajudo", ["leve"] = "levo", ["faça"] = "faço",
        ["liga"] = "ligo", ["telefona"] = "telefono", ["pergunta"] = "pergunto",
        ["diz"] = "digo", ["espera"] = "espero", ["deixa"] = "deixo",
        ["vai"] = "vou", ["vem"] = "venho", ["ajuda"] = "ajudo", ["leva"] = "levo", ["faz"] = "faço",
    };
    private static readonly Regex ActionQuestion = new(
        @"^(?<head>[\s\uE000-\uF8FF]*(?:[-–—]\s*)?)(?<verb>" + Alternatives(QuestionActions.Keys) + @")" +
        @"(?<rest>[^.!;]*\?[\s\uE000-\uF8FF]*)$", Ci);
    private static readonly Regex TelephoneName = new(
        Before + @"(?<verb>" + Alternatives(new[] { "ligar", "telefonar" }.SelectMany(Forms)) + @")" +
        @"(?<gap>\s+)(?<prep>para)(?<space>\s+)(?<name>[\p{L}\p{M}]+)" + After, Ci);

    public static string ActionQuestions(string text, string? previous, ReferenceHint? reference, bool nameArticles, PersonalNameArticles names)
    {
        var english = reference?.EnglishText ?? "";
        var explicitSelf = Regex.IsMatch(english, @"^(?:shall|should|do|can)\s+i\s+" +
            @"(?:call|phone|ring|ask|tell|wait|leave|go|come|help|take|do)\b[^.!;]*\?$", Ci);
        if (!explicitSelf && reference is not { SelfActionQuestion: true } ||
            PortugueseConverter.YouToTuRules.IsFormal(text) ||
            previous != null && PortugueseConverter.YouToTuRules.IsFormal(previous)) return text;
        text = ActionQuestion.Replace(text, m => m.Groups["head"].Value +
            CaseHelper.MatchCase(m.Groups["verb"].Value, QuestionActions[m.Groups["verb"].Value]) + m.Groups["rest"].Value);
        if (!nameArticles) return text;
        return TelephoneName.Replace(text, m =>
        {
            var name = m.Groups["name"].Value;
            var article = PersonalNameArticle(name, names);
            if (article == null) return m.Value;
            return m.Groups["verb"].Value + m.Groups["gap"].Value + m.Groups["prep"].Value +
                m.Groups["space"].Value + article + " " + name;
        });
    }

    private static readonly Regex ReportedIntention = new(
        Before + @"(?<head>disseste|sabias|percebeste|achaste|pensaste|descobriste)(?<bridge>\s+que\s+)" +
        @"(?<aux>irias)(?=\s+(?:(?:me|te|se|nos|vos|lhe|lhes)\s+)?\p{L}+(?:ar|er|ir|ôr)" + After + @")", Ci);

    public static string ReportedPastIntention(string text, ReferenceHint reference) =>
        Regex.IsMatch(reference.EnglishText ?? "", @"\byou\s+were\s+(?:gonna|going\s+to)\b", Ci)
            ? ReportedIntention.Replace(text, m => m.Groups["head"].Value + m.Groups["bridge"].Value +
                CaseHelper.MatchCase(m.Groups["aux"].Value, "ias")) : text;

    private static readonly Regex StopNoun = new(
        Before + @"(?<det>uma|umas|a|as|esta|estas|essa|essas)(?<gap>\s+)" +
        @"(?<adjective>(?:(?:pequenas?|breves?|rápidas?|curtas?|últimas?|próximas?|outras?)\s+)?)" +
        @"(?<noun>paradas?)" + After +
        @"(?!\s+(?:cardíacas?|cardiorrespiratórias?|militar(?:es)?|de\s+(?:sucesso|sucessos))\b)", Ci);

    private static readonly Dictionary<string, string> RespondToIncidentForms = MapForms("atender", "responder");
    private static readonly Regex IncidentResponse = new(
        Before + @"(?<verb>" + Alternatives(RespondToIncidentForms.Keys) + @")(?<gap>\s+)" +
        @"(?<prep>no|na|nos|nas)(?=\s+(?:(?:um|uma|o|a|os|as)\s+)?\p{L})", Ci);
    private static readonly Regex EnglishIncidentResponse = new(
        @"\brespond(?:ed|ing|s)?\s+to\s+(?:the|a|an|that|this|our)\s+" +
        @"(?:(?:night|local|nearby)\s+){0,2}(?:club|nightclub|scene|incident|emergency|accident|" +
        @"fire|collision|crash|hospital|house|home|school|building|hotel|bar|restaurant|address)\b", Ci);
    private static readonly Regex EnglishMedicalArrest = new(
        @"\b(?:(?:went|go|goes|going|gone)\s+into\s+(?:(?:cardiac|cardiorespiratory)\s+)?arrest|" +
        @"(?:had|has|have|having|suffered|suffers|suffering)\s+(?:a\s+)?(?:cardiac|cardiorespiratory)\s+arrest)\b", Ci);
    private static readonly Regex MedicalArrestNoun = new(
        Before + @"(?<verb>teve|tiveram|tinha|tinham|tem|têm|ter|terá|terão|sofreu|sofreram|sofre|sofrem|sofrer)" +
        @"(?<gap>\s+)(?<article>uma|a)(?<space>\s+)(?<noun>parada|paragem)" + After +
        @"(?!\s+(?:cardíaca|cardiorrespiratória|respiratória|militar|de\s+\p{L})\b)", Ci);

    public static string EmergencyConstructions(string text, ReferenceHint? reference)
    {
        var english = reference?.EnglishText ?? "";
        if (EnglishIncidentResponse.IsMatch(english) &&
            !Regex.IsMatch(english, @"\b(?:phone|telephone|doorbell|answered|reply|replied)\b", Ci))
            text = IncidentResponse.Replace(text, m => CaseHelper.MatchCase(m.Groups["verb"].Value,
                RespondToIncidentForms[m.Groups["verb"].Value]) + m.Groups["gap"].Value + m.Groups["prep"].Value);
        if (EnglishMedicalArrest.IsMatch(english))
            text = MedicalArrestNoun.Replace(text, m => m.Groups["verb"].Value + m.Groups["gap"].Value +
                m.Groups["article"].Value + m.Groups["space"].Value +
                CaseHelper.MatchCase(m.Groups["noun"].Value,
                    Regex.IsMatch(english, @"\bcardiorespiratory\s+arrest\b", Ci)
                        ? "paragem cardiorrespiratória" : "paragem cardíaca"));
        return text;
    }

    private static readonly Regex PoliceSquadRoom = new(
        DialogueBefore + @"(?<room>sala)(?<gap>\s+)(?<article>do)(?<space>\s+)(?<noun>esquadrão)" + After +
        @"(?!\s+(?:de|dos|das|militar|tático)\b)", Ci);
    private static readonly Regex PoliceSquadPlace = new(
        DialogueBefore + @"(?<article>ao|no|pelo|do|para\s+o|em\s+um)(?<gap>\s+)(?<noun>esquadrão|distrito\s+policial)" +
        After + @"(?!\s+(?:de|dos|das|militar|tático)\b)", Ci);
    private static readonly Regex EnglishPolicePlace = new(
        @"\b(?:squad\s+room|(?:at|in|to|by|from|into|outside|inside|near|left|leave|leaving)\s+(?:the|a|his|her|our|their)\s+" +
        @"(?:squad(?:\s+room)?|precinct|police\s+station|station\s+house))\b", Ci);
    private static readonly Regex MilitaryContext = new(
        @"\b(?:military|army|soldiers?|barracks|aircraft|air\s+force|naval|marines?|troops?)\b", Ci);

    public static string PoliceStationVocabulary(string text, ReferenceHint? reference, Func<string, bool> blocked)
    {
        var english = ReferenceAligner.Clean(reference?.EnglishText ?? "");
        if (!EnglishPolicePlace.IsMatch(english) || MilitaryContext.IsMatch(english)) return text;
        bool Protected(Match m) => Regex.IsMatch(m.Value, @"\r?\n[ \t]*\r?\n") ||
            blocked(Regex.Replace(m.Groups["noun"].Value, @"\s+", " ")) || blocked(m.Value) ||
            Regex.Matches(m.Value, @"[\p{L}\p{M}]+").Any(w => blocked(w.Value));
        text = PoliceSquadRoom.Replace(text, m => Protected(m) ? m.Value :
            m.Groups["room"].Value + m.Groups["gap"].Value +
            CaseHelper.MatchCase(m.Groups["article"].Value, "da") + m.Groups["space"].Value +
            CaseHelper.MatchCase(m.Groups["noun"].Value, "esquadra"));
        return PoliceSquadPlace.Replace(text, m =>
        {
            if (Protected(m)) return m.Value;
            var article = m.Groups["article"].Value;
            if (article.Equals("do", StringComparison.OrdinalIgnoreCase) &&
                !Regex.IsMatch(m.Result("$`"), @"\b(?:fora|dentro|perto|longe|saiu|saí|saíram|sair|voltou|veio|vieram|regressou)\s+$", Ci))
                return m.Value;
            var target = Regex.Replace(article.ToLowerInvariant(), @"\s+", " ") switch
            {
                "ao" => "à", "no" => "na", "pelo" => "pela", "do" => "da",
                "para o" => "para a", _ => "em uma",
            };
            if (Regex.IsMatch(article, @"\r?\n"))
                target = Regex.Replace(target, @"\s+", m => article.Contains("\r\n") ? "\r\n" : "\n");
            return CaseHelper.MatchCase(article, target) + m.Groups["gap"].Value +
                CaseHelper.MatchCase(m.Groups["noun"].Value, "esquadra");
        });
    }

    private sealed record ProgressiveVerb(string Infinitive, string English, string[] Present, string[] Past);
    private static readonly ProgressiveVerb[] ReferenceProgressiveVerbs =
    {
        new("mentir", "lying", ["minto", "mentes", "mente", "mentimos", "mentem"], ["mentia", "mentias", "mentia", "mentíamos", "mentiam"]),
        new("falar", "talking|speaking", ["falo", "falas", "fala", "falamos", "falam"], ["falava", "falavas", "falava", "falávamos", "falavam"]),
        new("trabalhar", "working", ["trabalho", "trabalhas", "trabalha", "trabalhamos", "trabalham"], ["trabalhava", "trabalhavas", "trabalhava", "trabalhávamos", "trabalhavam"]),
        new("esperar", "waiting", ["espero", "esperas", "espera", "esperamos", "esperam"], ["esperava", "esperavas", "esperava", "esperávamos", "esperavam"]),
        new("chorar", "crying", ["choro", "choras", "chora", "choramos", "choram"], ["chorava", "choravas", "chorava", "chorávamos", "choravam"]),
        new("correr", "running", ["corro", "corres", "corre", "corremos", "correm"], ["corria", "corrias", "corria", "corríamos", "corriam"]),
        new("dormir", "sleeping", ["durmo", "dormes", "dorme", "dormimos", "dormem"], ["dormia", "dormias", "dormia", "dormíamos", "dormiam"]),
        new("comer", "eating", ["como", "comes", "come", "comemos", "comem"], ["comia", "comias", "comia", "comíamos", "comiam"]),
        new("beber", "drinking", ["bebo", "bebes", "bebe", "bebemos", "bebem"], ["bebia", "bebias", "bebia", "bebíamos", "bebiam"]),
    };
    private static readonly Regex ExplicitProgressiveSubject = new(
        DialogueBefore + @"(?<subject>eu|tu|você|ele|ela|nós|vocês|eles|elas)(?<gap>\s+)" +
        @"(?:(?<neg>não)(?<negGap>\s+))?(?<verb>" +
        Alternatives(ReferenceProgressiveVerbs.SelectMany(v => v.Present.Concat(v.Past))) + @")" +
        After + @"(?!-)", Ci);

    public static string ReferenceProgressive(string text, ReferenceHint? reference, Func<string, bool> blocked)
    {
        var english = ReferenceAligner.Clean(reference?.EnglishText ?? "").Replace('’', '\'');
        if (english.Length == 0) return text;
        return ExplicitProgressiveSubject.Replace(text, m =>
        {
            if (Regex.IsMatch(m.Value, @"\r?\n[ \t]*\r?\n") || blocked(m.Value)) return m.Value;
            var subject = m.Groups["subject"].Value.ToLowerInvariant();
            var (index, enSubject, presentAux, pastAux) = subject switch
            {
                "eu" => (0, "i", "(?:am|'m)", "was"),
                "tu" => (1, "you", "(?:are|'re)", "were"),
                "você" => (2, "you", "(?:are|'re)", "were"),
                "ele" => (2, "he", "(?:is|'s)", "was"),
                "ela" => (2, "she", "(?:is|'s)", "was"),
                "nós" => (3, "we", "(?:are|'re)", "were"),
                "vocês" => (4, "you", "(?:are|'re)", "were"),
                _ => (4, "they", "(?:are|'re)", "were"),
            };
            var verb = m.Groups["verb"].Value;
            var matches = ReferenceProgressiveVerbs.Where(v =>
                v.Present[index].Equals(verb, StringComparison.OrdinalIgnoreCase) ||
                v.Past[index].Equals(verb, StringComparison.OrdinalIgnoreCase)).ToArray();
            if (matches.Length != 1 || blocked(matches[0].Infinitive) || blocked(verb)) return m.Value;
            var entry = matches[0];
            var past = entry.Past[index].Equals(verb, StringComparison.OrdinalIgnoreCase);
            var aux = past ? pastAux : presentAux;
            var negative = m.Groups["neg"].Success ? @"(?:\s+not|n't)" : @"(?!\s+not\b|n't)";
            var opposite = m.Groups["neg"].Success ? @"(?!\s+not\b|n't)" : @"(?:\s+not|n't)";
            var pattern = @"\b" + enSubject + @"(?:\s+" + aux + @"|" + aux + @")" +
                negative + @"\s+(?:" + entry.English + @")\b";
            var contradictory = @"\b" + enSubject + @"(?:\s+" + aux + @"|" + aux + @")" +
                opposite + @"\s+(?:" + entry.English + @")\b";
            if (!Regex.IsMatch(english, pattern, Ci) ||
                Regex.IsMatch(english, contradictory, Ci) ||
                ExplicitProgressiveSubject.Matches(text).Count(candidate =>
                    candidate.Groups["subject"].Value.Equals(m.Groups["subject"].Value, StringComparison.OrdinalIgnoreCase) &&
                    candidate.Groups["verb"].Value.Equals(verb, StringComparison.OrdinalIgnoreCase) &&
                    candidate.Groups["neg"].Success == m.Groups["neg"].Success) != 1 ||
                Regex.IsMatch(english, @"\b(?:always|usually|normally|habitually)\b", Ci) ||
                entry.Infinitive == "mentir" && Regex.IsMatch(english, @"\blying\s+(?:down|in|on|under|beside|next\s+to)\b", Ci))
                return m.Value;
            var ptAux = (past, index) switch
            {
                (false, 0) => "estou", (false, 1) => "estás", (false, 2) => "está",
                (false, 3) => "estamos", (false, _) => "estão",
                (true, 0 or 2) => "estava", (true, 1) => "estavas",
                (true, 3) => "estávamos", _ => "estavam",
            };
            return m.Groups["subject"].Value + m.Groups["gap"].Value +
                (m.Groups["neg"].Success ? m.Groups["neg"].Value + m.Groups["negGap"].Value : "") +
                CaseHelper.MatchCase(verb, ptAux + " a " + entry.Infinitive);
        });
    }

    private static readonly Regex EchoTopicQuestion = new(
        @"(?<head>(?:^|[.!?;]\s*|\n)[\s\uE000-\uF8FF-]*)sobre\s+o\s+que\s+" +
        @"(?<subject>(?:você|tu)\s+e\s+(?:(?:o|a)\s+)?(?-i:\p{Lu}\p{Ll}+)(?:\s+(?-i:\p{Lu}\p{Ll}+)){0,2})\s+" +
        @"(?<verb>discutiram|brigaram|falaram|conversaram)(?=\s*\?)", Ci);
    private static readonly Dictionary<string, string> SlangDepartureForms = MapForms("mandar", "sair");
    private static readonly Regex SlangDeparture = new(
        Before + @"(?:(?<cl>me|te|se|nos|vos)\s+)?(?<verb>" + Alternatives(SlangDepartureForms.Keys) +
        @")(?:-(?<attached>me|te|se|nos|vos))?(?<gap>\s+)(?<place>daqui|daí|dali)" + After, Ci);
    private static readonly Regex DepartureCommand = new(
        @"(?<head>(?:^|[.!?;]\s*|\n)[\s\uE000-\uF8FF-]*(?:por\s+favor,\s*)?)" +
        @"(?<verb>vá|vai|vão)(?<gap>\s+)embora(?=\s*[.!…]|\s*$)", Ci);
    private static readonly Regex CertaintyQuestion = new(
        @"(?<head>(?:^|[.!?;]\s*|\n)[\s\uE000-\uF8FF-]*)(?<verb>está|estás|estão|estamos)" +
        @"(?<gap>\s+)cert[oa]s?\s+que\s+(?<body>[^.!?]*)(?=\?)", Ci);

    public static string NaturalDialogue(string text, ReferenceHint? reference, bool vocabulary)
    {
        if (vocabulary && Regex.IsMatch(reference?.EnglishText ?? "", @"\b(?:get(?:ting)?|got|go(?:ing)?|went|leave|leaving)" +
            @"\s+(?:(?:the\s+hell\s+)?out\s+of\s+here|away|here)\b", Ci))
            text = SlangDeparture.Replace(text, m =>
                m.Groups["cl"].Success || m.Groups["attached"].Success
                    ? CaseHelper.MatchCase(m.Groups["verb"].Value, SlangDepartureForms[m.Groups["verb"].Value]) +
                        m.Groups["gap"].Value + m.Groups["place"].Value : m.Value);
        text = DepartureCommand.Replace(text, m => m.Groups["head"].Value + PortugueseVerbs.AttachEnclitic(m.Groups["verb"].Value,
            CaseHelper.IsAllUpper(m.Groups["verb"].Value)
                ? (m.Groups["verb"].Value.Equals("vai", StringComparison.OrdinalIgnoreCase) ? "TE" : "SE")
                : (m.Groups["verb"].Value.Equals("vai", StringComparison.OrdinalIgnoreCase) ? "te" : "se")));
        if (Regex.IsMatch(reference?.EnglishText ?? "", @"\bare\s+you\s+sure\b", Ci))
            text = CertaintyQuestion.Replace(text, m =>
            {
                var verb = m.Groups["verb"].Value.ToLowerInvariant() switch
                { "estás" => "tens", "estão" => "têm", "estamos" => "temos", _ => "tem" };
                var body = m.Groups["body"].Value;
                if (Regex.IsMatch(reference?.EnglishText ?? "", @"\bwant(?:ed|s)?\s+me\s+to\b", Ci))
                    body = Regex.Replace(body, @"\bque\s+eu\s+(?=\p{L})", "que ", Ci);
                return m.Groups["head"].Value + CaseHelper.MatchCase(m.Groups["verb"].Value, verb) +
                    m.Groups["gap"].Value + "a certeza de que " + body;
            });
        return text;
    }

    private const string FamilyNoun = @"mãe|pai|mamã|papá|avó|avô|tia|tio|irmã|irmão";
    private static readonly Regex FamilySubject = new(
        @"(?<head>(?:^|[.!?;]\s*|\n)[\s\uE000-\uF8FF-]*)(?<noun>" + FamilyNoun + @")" + After +
        @"(?=\s+(?:e\s+(?:(?:o|a)\s+)?(?:" + FamilyNoun + @")\b|" +
        @"(?<verb>\p{L}+)\b))", Ci);
    private static readonly Regex CoordinatedFamily = new(
        Before + @"(?<first>(?:a|o)\s+(?:" + FamilyNoun + @")" +
        @"(?:\s+(?-i:\p{Lu}\p{Ll}+)){0,2})(?<link>\s+e\s+)(?<noun>" + FamilyNoun + @")" +
        After + @"(?=\s+(?-i:\p{Lu}[\p{L}\p{M}]+)\b|\s+(?<verb>\p{L}+)\b|[\s\uE000-\uF8FF]*$)", Ci);
    private static readonly Regex EnglishFamilySubject = new(
        @"(?:^|[.!?]\s*)(?:mom|mum|mother|dad|father|grandma|grandpa|grandmother|grandfather|" +
        @"aunt|uncle|sister|brother)\b(?!\s*[,!?:])", Ci);

    public static string FamilySubjects(string text, ReferenceHint? reference)
    {
        if (!EnglishFamilySubject.IsMatch(reference?.EnglishText ?? "")) return text;
        text = FamilySubject.Replace(text, m =>
        {
            if (m.Groups["verb"].Success && (!PortugueseVerbs.IsKnownVerbForm(m.Groups["verb"].Value) ||
                PortugueseVerbs.IsInfinitive(m.Groups["verb"].Value))) return m.Value;
            return m.Groups["head"].Value + FamilyArticle(m.Groups["noun"].Value, true);
        });
        return CoordinatedFamily.Replace(text, m =>
        {
            if (m.Groups["verb"].Success && (!PortugueseVerbs.IsKnownVerbForm(m.Groups["verb"].Value) ||
                PortugueseVerbs.IsInfinitive(m.Groups["verb"].Value))) return m.Value;
            return m.Groups["first"].Value + m.Groups["link"].Value + FamilyArticle(m.Groups["noun"].Value, false);
        });
    }

    private const string SurnameToken = @"(?-i:\p{Lu}[\p{L}\p{M}'’-]+)";
    private static readonly Regex FamilySurnameSubject = new(
        @"(?<head>(?:^|[.!?;:]\s*|\n|[\uE000-\uF8FF]\s*|" +
        Before + @"(?:que|como|quando|porque|se|embora|enquanto)\s+)[\s\uE000-\uF8FF-]*)" +
        @"(?<name>" + SurnameToken + @"(?:\s+(?:(?:de|da|do|dos|das)\s+)?" + SurnameToken + @"){0,2}?)" +
        @"(?<gap>\s+(?:(?:não|nunca|já|ainda|sempre|também|só|todos|todas)\s+){0,3})" +
        @"(?<verb>têm|vêm|\p{L}+(?:am|em|ão))" + After, Ci);
    private static readonly Regex PluralFamilyAuxiliary = new(
        @"^(?:são|eram|foram|serão|seriam|sejam|fossem|estão|estavam|estiveram|estarão|estariam|estejam|estivessem|" +
        @"têm|tinham|tiveram|terão|teriam|tenham|tivessem|vêm)$", Ci);
    private static readonly Regex RaisedFamilyPredicate = new(
        @"^\s+(?:(?:sempre|assim|bem|mal|todos|todas|muito)\s+){0,3}(?<part>criad[oa]s|educad[oa]s)" + After, Ci);

    public static string FamilySurnameSubjects(string text, ReferenceHint? reference, PersonalNameArticles names) =>
        FamilySurnameSubject.Replace(text, m =>
        {
            var name = m.Groups["name"].Value;
            var first = name.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)[0];
            if (Regex.IsMatch(first, @"^(?:o|a|os|as|um|uma|uns|umas|eu|tu|ele|ela|nós|vós|vocês|eles|elas|" +
                @"todos|todas|estes|estas|esses|essas|aqueles|aquelas|quem|quais|quantos|quantas)$", Ci) ||
                names.TryGet(first, out var selected) && selected == "none") return m.Value;
            var verb = m.Groups["verb"].Value;
            var pluralVerb = PluralFamilyAuxiliary.IsMatch(verb) ||
                Regex.IsMatch(verb, @"(?:am|em|ão)$", Ci) && PortugueseVerbs.IsKnownVerbForm(verb) &&
                !PortugueseVerbs.IsInfinitive(verb);
            if (!pluralVerb) return m.Value;
            var english = reference?.EnglishText ?? "";
            var escaped = Regex.Escape(Regex.Replace(name, @"\s+", " "));
            var confirmed = Regex.IsMatch(english, @"(?<![\p{L}\p{M}])" + escaped +
                @"(?:s|es)\b|(?<![\p{L}\p{M}])(?:the\s+" + escaped + @"\s+family|" +
                escaped + @"\s+family)\b", Ci);
            var predicate = m.Result("$'");
            var raised = RaisedFamilyPredicate.Match(predicate);
            if (!confirmed)
            {
                if (english.Length > 0 || !raised.Success ||
                    !Regex.IsMatch(verb, @"^(?:são|eram|foram|serão|seriam|sejam|fossem)$", Ci) ||
                    name.Contains(' ') || NaturaDictionary.Load(true).Check(name.ToLowerInvariant()))
                    return m.Value;
            }
            var copula = Regex.IsMatch(verb, @"^(?:são|eram|foram|serão|seriam|sejam|fossem|" +
                @"estão|estavam|estiveram|estarão|estariam|estejam|estivessem)$", Ci);
            var feminine = copula && Regex.IsMatch(predicate,
                @"^\s+(?:(?:sempre|assim|bem|mal|todas|muito|já|ainda)\s+){0,3}" +
                @"(?:\p{L}+(?:adas|idas)|mortas|feitas|vistas|ditas|postas|abertas|mulheres|irmãs|filhas|meninas|raparigas)\b", Ci);
            var article = feminine ? "as" : "os";
            var initial = Regex.IsMatch(m.Groups["head"].Value, @"^(?:[\s\uE000-\uF8FF-]*|[.!?;:]\s*)$");
            if (CaseHelper.IsAllUpper(name)) article = article.ToUpperInvariant();
            else if (initial) article = CaseHelper.Capitalize(article);
            return m.Groups["head"].Value + article + " " + name + m.Groups["gap"].Value + verb;
        });

    private static string FamilyArticle(string noun, bool initial)
    {
        var feminine = Regex.IsMatch(noun, @"^(?:mãe|mamã|avó|tia|irmã)$", Ci);
        var article = feminine ? "a" : "o";
        if (CaseHelper.IsAllUpper(noun)) return article.ToUpperInvariant() + " " + noun;
        return (initial && char.IsUpper(noun[0]) ? article.ToUpperInvariant() : article) + " " + noun;
    }

    private static readonly Dictionary<string, string[]> ReferenceObjectActions = new(StringComparer.OrdinalIgnoreCase)
    {
        ["found"] = new[] { "encontrou", "encontraram", "encontrei", "encontrámos", "encontramos" },
        ["find"] = new[] { "encontrou", "encontraram", "encontrei", "encontrámos", "encontramos" },
        ["saw"] = new[] { "viu", "viram", "vi", "vimos" },
        ["helped"] = new[] { "ajudou", "ajudaram", "ajudei", "ajudámos", "ajudamos" },
        ["called"] = new[] { "chamou", "chamaram", "chamei", "chamámos", "chamamos" },
        ["took"] = new[] { "levou", "levaram", "levei", "levámos", "levamos" },
        ["got"] = new[] { "levou", "levaram", "levei", "levámos", "levamos" },
        ["brought"] = new[] { "trouxe", "trouxeram", "trouxemos", "levou", "levaram", "levei", "levámos", "levamos" },
        ["rescued"] = new[] { "resgatou", "resgataram", "resgatei", "resgatámos", "resgatamos" },
        ["saved"] = new[] { "salvou", "salvaram", "salvei", "salvámos", "salvamos" },
    };
    private static readonly Regex EnglishObjectAction = new(
        @"\b(?<verb>" + Alternatives(ReferenceObjectActions.Keys) + @")\s+you\b" +
        @"(?!\s+(?:(?:and|or)\s+(?:him|her|them|me|us|you)|all|both|guys|two)\b)", Ci);
    private static readonly Regex PastObjectClitic = new(
        Before + @"(?<clitic>o|a)(?<gap>\s+)(?<verb>" +
        Alternatives(ReferenceObjectActions.Values.SelectMany(forms => forms).Distinct(StringComparer.OrdinalIgnoreCase)) +
        @")" + After + @"(?!-)", Ci);
    private static readonly Dictionary<string, string> BringPastForms = new(StringComparer.OrdinalIgnoreCase)
    {
        ["levou"] = "trouxe", ["levaram"] = "trouxeram", ["levei"] = "trouxe",
        ["levámos"] = "trouxemos", ["levamos"] = "trouxemos",
    };

    public static string ReferenceObjectClitics(string text, ReferenceHint? reference, bool vocabulary)
    {
        var english = reference?.EnglishText ?? "";
        return PastObjectClitic.Replace(text, m =>
        {
            var verb = m.Groups["verb"].Value;
            var actions = EnglishObjectAction.Matches(english).Cast<Match>().Where(action =>
                ReferenceObjectActions[action.Groups["verb"].Value].Contains(verb, StringComparer.OrdinalIgnoreCase)).ToArray();
            if (actions.Length != 1) return m.Value;
            var action = actions[0];
            if (action.Groups["verb"].Value.ToLowerInvariant() is "found" or "find" &&
                Regex.IsMatch(english[(action.Index + action.Length)..], @"^\s+(?:a|an|the)\s+\p{L}", Ci))
                return m.Value;
            if (action.Groups["verb"].Value.Equals("got", StringComparison.OrdinalIgnoreCase) &&
                !Regex.IsMatch(english[(action.Index + action.Length)..], @"^\s+to\s+(?:the|a|an|this|that)\s+\p{L}", Ci))
                return m.Value;
            if (Regex.IsMatch(english, @"\b(?:found|find|saw|helped|called|took|got|brought|rescued|saved)\s+" +
                @"(?:him|her|them|it)\b", Ci)) return m.Value;
            if (vocabulary && action.Groups["verb"].Value.Equals("brought", StringComparison.OrdinalIgnoreCase) &&
                BringPastForms.TryGetValue(verb, out var brought))
                verb = CaseHelper.MatchCase(verb, brought);
            var prefix = Regex.Match(text[..m.Index], @"[^.!?;:,]*$").Value;
            if (Regex.IsMatch(prefix, Before + @"(?:" + PortugueseConverter.YouToTuRules.ProclisisTriggers +
                @"|se|quando|onde|como|porque|embora|caso|enquanto)\b", Ci))
                return CaseHelper.MatchCase(m.Groups["clitic"].Value, "te") + m.Groups["gap"].Value + verb;
            if (char.IsUpper(m.Groups["clitic"].Value[0]))
                verb = CaseHelper.MatchCase(m.Groups["clitic"].Value, verb);
            return PortugueseVerbs.AttachEnclitic(verb, CaseHelper.IsAllUpper(verb) ? "TE" : "te");
        });
    }

    private static readonly Regex PlateNoun = new(DialogueBefore + @"placas?" + After, Ci);
    private static readonly Regex VehicleContext = new(
        @"\b(?:carros?|automóveis|automóvel|veículos?|caminh(?:ão|ões)|cami(?:ão|ões)|carrinhas?|carr(?:os?|inhas?)-patrulha|" +
        @"motos?|motocicletas?|motociclos?|autocarros?|ônibus|sedans?|sedãs?|carruagens?|táxis?|reboques?|" +
        @"SUV|BMW|Nissan|Ford|Toyota|Chevrolet|Honda|Mercedes|Volkswagen|Renault|Peugeot)\b", Ci);
    private static readonly Regex PlateOtherSense = new(
        @"^\s+(?:de\s+(?:(?:a|o|uma|um)\s+)?(?:rua|sinalizaç(?:ão|ões)|trânsito|pare|stop|aviso|publicidade|homenagem|" +
        @"identificaç(?:ão|ões)\s+da\s+porta|metal|aço|alumínio|madeira|vidro|gesso|betão|concreto|rede|som|vídeo|video|" +
        @"memória|circuito|Petri|cultura|induç(?:ão|ões)|gás|cozinha|parede|matrícula)|" +
        @"tectónicas?|tectônicas?|bacterianas?|dentárias?|dentais|comemorativas?|publicitárias?|indicativas?|" +
        @"gráficas?|mãe|solares?|fotovoltaicas?|vitrocerâmicas?)\b", Ci);
    private static readonly Regex EnglishVehiclePlate = new(
        @"\b(?:licen[cs]e|number|registration|vanity|temporary)\s*(?:-\s*)?plates?\b|" +
        @"\b(?:plates?\s+(?:numbers?|came\s+back|we\s+could\s+ID)|(?:run|running|ran|check|checking|checked|" +
        @"call\s+in|called\s+in|stolen|out-of-state|Jersey|Pennsylvania)\s+(?:(?:the|these|those|a|his|her)\s+)?plates?)\b|" +
        @"\bregistration\b", Ci);
    private static readonly Regex EnglishOtherPlate = new(
        @"\b(?:(?:dinner|dining|food|ceramic|tectonic|dental|commemorative|memorial|circuit)\s+plates?|" +
        @"(?:road|street|warning)\s+signs?|motherboards?|graphics\s+cards?|school\s+registration)\b", Ci);

    public static string VehiclePlates(string text, string? previous, ReferenceHint? reference, Func<string, bool> isBlocked) =>
        PlateNoun.Replace(text, m =>
        {
            if (isBlocked(m.Value) || isBlocked("placa") || isBlocked("placas") ||
                new[] { "placa do carro", "número da placa", "sem placa" }.Any(isBlocked)) return m.Value;
            var before = Regex.Match(m.Result("$`"), @"[^.!?;]*$").Value;
            var after = Regex.Match(m.Result("$'"), @"^[^.!?;]*").Value;
            if (PlateOtherSense.IsMatch(after) ||
                Regex.IsMatch(before, @"\b(?:rua|parede|porta|prédio|edifício|dentes|dentista|computador|fogão)" +
                    @"\b[^.!?;]{0,20}$", Ci)) return m.Value;
            var english = reference?.EnglishText ?? "";
            var confirmedEnglish = EnglishVehiclePlate.IsMatch(english) ||
                Regex.IsMatch(english, @"\bplates?\b", Ci) && Regex.IsMatch(english,
                    @"\b(?:car|vehicle|truck|van|sedan|SUV|motorcycle|bus|stolen|DMV)\b", Ci);
            if (EnglishOtherPlate.IsMatch(english)) confirmedEnglish = false;
            var vehicleAfter = Regex.IsMatch(after, @"^\s+(?:d[oa]s?|de)\s+(?:(?:um|uma|uns|umas)\s+)?", Ci) &&
                VehicleContext.IsMatch(Regex.Match(after, @"^\s+(?:d[oa]s?|de)\s+(?:\p{L}+\s+){0,2}\p{L}+").Value);
            var regional = Regex.IsMatch(after, @"^\s+(?:de\s+(?:fora|outro\s+estado)|estrangeiras?|clonadas?)\b", Ci);
            var number = Regex.IsMatch(before, @"\bnúmeros?\s+d[ae]s?\s+$", Ci) &&
                !EnglishOtherPlate.IsMatch(english);
            var localVehicle = VehicleContext.IsMatch(before) || vehicleAfter;
            var previousVehicle = Regex.IsMatch(text[..m.Index],
                @"^[\s\uE000-\uF8FF-]*(?:(?:e|mas|com|sem|a|as|o|os|uma|umas|número|da|das|em|marca|modelo,?|semelhante)\s+)*$", Ci) &&
                VehicleContext.IsMatch(previous ?? "");
            if (!confirmedEnglish && !localVehicle && !regional && !number && !previousVehicle) return m.Value;
            return CaseHelper.MatchCase(m.Value,
                m.Value.EndsWith("s", StringComparison.OrdinalIgnoreCase) ? "matrículas" : "matrícula");
        });

    public static string TravelStops(string text, ReferenceHint? reference) =>
        Regex.IsMatch(reference?.EnglishText ?? "", @"\b(?:(?:a|one|two|three|some|the|this|that|another|our)\s+" +
            @"(?:(?:little|small|short|brief|quick|last|next|more|bus|train|scheduled)\s+){0,3}|" +
            @"(?:little|small|short|brief|quick|last|next|bus|train|scheduled)\s+)stops?\b", Ci) &&
        !Regex.IsMatch(reference?.EnglishText ?? "", @"\b(?:parade|cardiac|arrest|chart|standing|stopped)\b", Ci)
            ? StopNoun.Replace(text, m => m.Groups["det"].Value + m.Groups["gap"].Value +
                m.Groups["adjective"].Value + CaseHelper.MatchCase(m.Groups["noun"].Value,
                    m.Groups["noun"].Value.EndsWith('s') ? "paragens" : "paragem")) : text;

    private static readonly Regex InformationTopic = new(
        @"^(?:(?:tenho|temos|há|são|e|quais\s+são)\s+)?(?:(?:o|a|os|as|um|uma|uns|umas|ess[ea]s?|est[ea]s?)\s+)?" +
        @"(?:(?:boas?|más?|últimas?|novas?)\s+)?(?:notícias|novidades|informações|detalhes|verdade|explicação)" +
        @"\s*[.!?…]?$", Ci);
    private static readonly Regex GiveRequestReference = new(
        @"(?:^|[.!?]\s+)[\s]*(?:(?:boys|girls|guys|kids|folks|everyone|gentlemen|ladies|sir|ma['’]am|" +
        @"(?-i:[A-Z][a-z]+))\s*,\s*)?(?:please\s*,?\s*)?(?:give|grant)\s+(?<recipient>me|us)\s+" +
        @"(?<object>(?:(?:a|one|two|three|a\s+few|some|another|your)\s+)" +
        @"(?:(?:little|brief|short|quick|more|undivided|full)\s+){0,2}" +
        @"(?:seconds?|minutes?|moments?|time|break|favou?r|chance|opportunity|hand|help|attention))" +
        @"(?=\s*[,!.?…]|$)", Ci);
    private static readonly Regex MissingGiveRecipient = new(
        @"(?<head>(?:^|[.!?…]\s+|\n)[\s\uE000-\uF8FF–—-]*" +
        @"(?:(?:rapazes|raparigas|miúdos|miúdas|pessoal|meninos|meninas|senhores|senhoras|" +
        @"senhor|senhora|detetive|agente|doutor|doutora|(?-i:\p{Lu}[\p{Ll}\p{M}]+))" +
        @"(?:\s+(?-i:\p{Lu}[\p{Ll}\p{M}]+))?\s*,\s*)?(?:por\s+favor\s*,?\s*)?)" +
        @"(?<verb>dêem|deem|dai|dê|dá)(?<space>\s+)" +
        @"(?<object>(?:um|uma|dois|duas|três|alguns|algumas|mais|o|a)\s+" +
        @"(?:(?:pouco|bocado)\s+de\s+)?(?:(?:breve|pequeno|pequena|toda|total|vossa|sua)\s+){0,2}" +
        @"(?:segundos?|minutos?|momentos?|tempo|pausa|descanso|folga|favor|hipótese|chance|oportunidade|mão|ajuda|atenção))" +
        @"(?=\s*[,!.?…\uE000-\uF8FF]|$)", Ci);

    public static string GiveRequestRecipients(string text, string? english)
    {
        if (string.IsNullOrWhiteSpace(english)) return text;
        var requests = GiveRequestReference.Matches(english);
        var candidates = MissingGiveRecipient.Matches(text);
        if (requests.Count != 1 || candidates.Count != 1) return text;
        var request = requests[0];
        var candidate = candidates[0];
        var category = Regex.Match(request.Groups["object"].Value,
            @"(?:seconds?|minutes?|moments?|time|break|favou?r|chance|opportunity|hand|help|attention)$", Ci).Value.ToLowerInvariant();
        var objectPattern = category switch
        {
            "second" or "seconds" => @"segundos?$",
            "minute" or "minutes" => @"minutos?$",
            "moment" or "moments" => @"momentos?$",
            "time" => @"tempo$",
            "break" => @"(?:pausa|descanso|folga)$",
            "favor" or "favour" => @"favor$",
            "chance" or "opportunity" => @"(?:hipótese|chance|oportunidade)$",
            "hand" or "help" => @"(?:mão|ajuda)$",
            _ => @"atenção$",
        };
        if (!Regex.IsMatch(candidate.Groups["object"].Value, objectPattern, Ci)) return text;
        var verb = candidate.Groups["verb"];
        var clitic = request.Groups["recipient"].Value.Equals("me", StringComparison.OrdinalIgnoreCase) ? "me" : "nos";
        return text[..verb.Index] + PortugueseVerbs.AttachEnclitic(verb.Value, clitic) +
            text[(verb.Index + verb.Length)..];
    }

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

    // "prestar/dar atenção em" is Brazilian; European Portuguese uses "a": "Não preste atenção nestes" → "a estes".
    private static readonly string AttentionHead =
        Before + @"(?<head>(?:" + Alternatives(Forms("prestar")) + @"|dar|dá|dás|damos|dão|dou|dei|deste|deu|demos|deram|dava|davas|dávamos|davam|" +
        @"dê|dês|dêem|deem|dando|dado|daria|darias|daríamos|dariam|darei|darás|dará|daremos|darão|desse|desses|déssemos|dessem|der|deres|dermos|derem)" +
        @"\s+(?:(?:mais|muita|pouca|nenhuma|alguma|qualquer|a\s+mínima|a\s+menor|toda\s+a|total|bastante|demasiada)\s+)?atenção\s+)";
    private static readonly Regex AttentionIn = new(
        AttentionHead +
        @"(?<prep>em|no|na|nos|nas|num|numa|nuns|numas|nisso|nisto|naquilo|nele|nela|neles|nelas|neste|nesta|nestes|nestas|nesse|nessa|nesses|nessas|naquele|naquela|naqueles|naquelas)" + After +
        @"(?:(?=\s+(?<poss>meu|teu|seu|nosso|vosso|minha|tua|sua|nossa|vossa|meus|teus|seus|nossos|vossos|minhas|tuas|suas|nossas|vossas)" + After + @"))?" +
        @"(?<tail>[^.!?;]*?(?<![\p{L}\p{M}])(?:tanto\s+(?:quanto|como)|mais\s+do\s+que|menos\s+do\s+que|e\s+também|como)\s+)?" +
        @"(?(tail)(?<prep2>no|na|nos|nas|em)" + After + @"(?:(?=\s+(?<poss2>meu|teu|seu|nosso|vosso|minha|tua|sua|nossa|vossa|meus|teus|seus|nossos|vossos|minhas|tuas|suas|nossas|vossas)" + After + @"))?)", Ci);
    private static readonly Regex EnglishThose = new(@"\b(?:those|that)\b", Ci);
    private static readonly Regex EnglishThese = new(@"\b(?:these|this)\b", Ci);
    private static readonly Regex EnglishAttentionTarget = new(
        @"\battention\s+to\s+(?<distance>those|that|these|this)\b", Ci);
    private static readonly Regex AttentionNear = new(
        AttentionHead + @"(?<prep>a\s+)(?<dem>este|esta|estes|estas)" + After, Ci);

    private static bool AttentionIsDistal(ReferenceHint? reference)
    {
        var english = reference?.EnglishText ?? "";
        var targets = EnglishAttentionTarget.Matches(english);
        if (targets.Count == 1)
            return targets[0].Groups["distance"].Value.Equals("those", StringComparison.OrdinalIgnoreCase) ||
                   targets[0].Groups["distance"].Value.Equals("that", StringComparison.OrdinalIgnoreCase);
        return targets.Count == 0 && EnglishThose.IsMatch(english) && !EnglishThese.IsMatch(english);
    }

    private static string AttentionTarget(string prep, string possessive, ReferenceHint? reference)
    {
        var lower = prep.ToLowerInvariant();
        // Brazilian "em sua conversa" has no article; European Portuguese needs it: "à sua conversa", "ao seu carro".
        if (lower == "em" && possessive.Length > 0)
        {
            var poss = possessive.ToLowerInvariant();
            var feminine = poss.EndsWith('a') || poss.EndsWith("as", StringComparison.Ordinal);
            var plural = poss.EndsWith('s');
            lower = (feminine ? "na" : "no") + (plural ? "s" : "");
        }

        string target = lower switch
        {
            "em" => "a",
            "no" => "ao",
            "na" => "à",
            "nos" => "aos",
            "nas" => "às",
            _ when lower.StartsWith("naqu", StringComparison.Ordinal) => "à" + lower[2..],
            _ when lower.StartsWith("nu", StringComparison.Ordinal) => "a " + lower[1..],
            _ => "a " + lower[1..],
        };
        // "pay attention to those" with a Brazilian "nestes": European Portuguese keeps the distance, "a esses".
        if (target.StartsWith("a est", StringComparison.Ordinal) && AttentionIsDistal(reference))
            target = "a ess" + target[5..];
        return CaseHelper.MatchCase(prep, target);
    }

    public static string Government(string text, ReferenceHint? reference = null)
    {
        text = AttentionIn.Replace(text, m =>
        {
            var result = m.Groups["head"].Value + AttentionTarget(m.Groups["prep"].Value, m.Groups["poss"].Value, reference);
            if (m.Groups["tail"].Success)
            {
                var gap = m.Value[(m.Groups["prep"].Index + m.Groups["prep"].Length - m.Index)..(m.Groups["tail"].Index + m.Groups["tail"].Length - m.Index)];
                result += gap + AttentionTarget(m.Groups["prep2"].Value, m.Groups["poss2"].Value, reference);
            }

            return result;
        });
        if (AttentionIsDistal(reference))
            text = AttentionNear.Replace(text, m => m.Groups["head"].Value + m.Groups["prep"].Value +
                CaseHelper.MatchCase(m.Groups["dem"].Value, "ess" + m.Groups["dem"].Value[3..]));
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

    private const string SubjectBridge =
        @"(?:(?:você|tu)\s+)?" + Adverbs +
        @"(?:ach(?:a|as|ava|avas|ou|aste|aria|arias)|pens(?:a|as|ava|avas|ou|aste|aria|arias)|" +
        @"acredit(?:a|as|ava|avas|ou|aste|aria|arias)|imagin(?:a|as|ava|avas|ou|aste|aria|arias)|" +
        @"supõe|supões|supunha|supunhas|diz|dizes|disse|disseste|diria|dirias|sabe|sabes|sabia|sabias)" +
        @"\s+que\s+" + Adverbs + @"$";
    private static readonly Regex ExtractedQuestionBridge = new(
        Before + @"(?:quem|o\s+que|(?:qual|quais)(?:\s+\p{L}+){0,2})\s+" + SubjectBridge, Ci);
    private static readonly Regex ExtractedRelativeBridge = new(
        Before + @"(?:alguém|ninguém|(?:o|a|os|as|um|uma|uns|umas)\s+\p{L}+(?:\s+\p{L}+)?|aquele|aquela)\s+que\s+" +
        SubjectBridge, Ci);
    private static readonly Regex EnglishRepeatedQuestionSubject = new(
        @"\b(?:who|(?:what|which)(?:\s+\p{L}+){0,2})\s+" +
        @"(?:(?:do|did|would|could|can|will)\s+)?you\s+" +
        @"(?:(?:really|actually|honestly|still|just|not|don't|didn't)\s+){0,3}" +
        @"(?:think|thought|believe|believed|imagine|imagined|suppose|supposed|say|said|know|knew)\s+" +
        @"(?:that\s+)?you\b", Ci);

    internal static bool KeepsExtractedQuestionSubject(string beforeVerb, ReferenceHint? reference) =>
        (ExtractedQuestionBridge.IsMatch(beforeVerb) &&
         !EnglishRepeatedQuestionSubject.IsMatch(reference?.EnglishText ?? "")) ||
        (ExtractedRelativeBridge.IsMatch(beforeVerb) && reference is not { RepeatedYouSubject: true });

    private static bool Second(string word, out string second) =>
        PortugueseVerbs.TryThirdToSecond(word, out second) || PortugueseVerbs.TryGuessThirdToSecond(word, out second);

    private static readonly Regex KeptWaiting = new(
        Before + @"(?<aux>ter|tenho|tens|tem|temos|têm|tinha|tinhas|tínhamos|tinham|teria|terias|teriam|tivesse|tivesses)\s+" +
        @"deixado\s+você\s+esperando" + After, Ci);
    private static readonly Regex CloseSpatialPosition = new(
        Before + @"(?<prefix>(?:(?:eu|tu|ele|ela|nós|eles|elas|você|vocês)\s+)?" +
        @"(?:estou|estás|está|estamos|estão|estava|estavas|estávamos|estavam|fico|fica|ficamos|ficam)\s+)?" +
        @"(?<soon>logo)(?<space>\s+)(?<position>atrás|à\s+frente|ao\s+lado)(?<complement>\s+(?:de|do|da|dos|das)\s*[\p{L}-]+)" + After, Ci);
    private static readonly Regex EnglishCloseSpatialPosition = new(
        @"(?:^|[.!?]\s*)(?:(?<subject>I['’]m|I\s+am|I\s+was|we['’]re|we\s+are|we\s+were|" +
        @"he['’]s|he\s+is|she['’]s|she\s+is|they['’]re|they\s+are|you['’]re|you\s+are)\s+)?" +
        @"(?:right|just)\s+(?<position>behind|in\s+front\s+of|next\s+to|beside)\s+" +
        @"(?<object>you|me|us|him|her|them)(?![\p{L}])", Ci);

    public static string SpatialPosition(string text, ReferenceHint? reference)
    {
        var english = EnglishCloseSpatialPosition.Matches(reference?.EnglishText ?? "");
        if (english.Count != 1 || Regex.Matches(reference?.EnglishText ?? "",
                @"\b(?:right|just)\s+(?:behind|in\s+front\s+of|next\s+to|beside)\b", Ci).Count != 1)
            return text;
        var expected = english[0].Groups["position"].Value.ToLowerInvariant() switch
        {
            "behind" => "atrás", "in front of" => "à frente", _ => "ao lado",
        };
        var matches = CloseSpatialPosition.Matches(text);
        if (matches.Count != 1 || !Regex.IsMatch(matches[0].Groups["position"].Value,
                "^" + Regex.Escape(expected).Replace(@"\ ", @"\s+") + "$", Ci))
            return text;
        var objectPattern = english[0].Groups["object"].Value.ToLowerInvariant() switch
        {
            "you" => @"de\s+(?:você|vocês|ti|si|vós)", "me" => @"de\s+mim", "us" => @"de\s+nós",
            "him" => @"dele|de\s+ele", "her" => @"dela|de\s+ela", _ => @"deles|delas|de\s+(?:eles|elas)",
        };
        if (!Regex.IsMatch(matches[0].Groups["complement"].Value, @"^\s+(?:" + objectPattern + @")$", Ci))
            return text;
        return CloseSpatialPosition.Replace(text, m =>
        {
            var prefix = m.Groups["prefix"].Value;
            if (prefix.Length == 0)
            {
                if (!Regex.IsMatch(m.Result("$`"), @"^[\s""“«'\uE000-\uF8FF-]*$", Ci)) return m.Value;
                var subject = english[0].Groups["subject"].Value;
                if ((subject.Length == 0 && english[0].Groups["object"].Value.Equals("you", StringComparison.OrdinalIgnoreCase)) ||
                    Regex.IsMatch(subject, @"^I(?:['’]m|\s+am)$", Ci))
                    prefix = CaseHelper.MatchCase(m.Groups["soon"].Value, "estou") + " ";
                else return m.Value;
            }
            var marker = CaseHelper.IsAllUpper(m.Value) ? "MESMO" : "mesmo";
            return prefix + marker + m.Groups["space"].Value + m.Groups["position"].Value + m.Groups["complement"].Value;
        });
    }

    private static readonly Regex WaitForPerson = new(
        DialogueBefore + @"(?<verb>" + Alternatives(Forms("esperar")) + @")" +
        @"(?<gap>\s+(?:(?:ainda|sempre|também|só|apenas|muito)\s+){0,2})" +
        @"(?:(?<article>o|a|os|as)(?<space>\s+)" +
        @"(?<person>(?:(?:meu|minha|teu|tua|seu|sua|nosso|nossa|meus|minhas|teus|tuas|seus|suas|nossos|nossas)\s+)?" +
        @"(?:pai|mãe|pais|irmãos?|irmãs?|filhos?|filhas?|amigos?|amigas?|colegas?|polícias?|agentes?|médicos?|médicas?)|" +
        @"(?-i:\p{Lu}[\p{L}\p{M}]+))|(?<person>(?-i:\p{Lu}[\p{L}\p{M}]+)))" + After, Ci);

    public static string WaitingForPeople(string text, PersonalNameArticles names, Func<string, bool> isBlocked) => WaitForPerson.Replace(text, m =>
    {
        if (isBlocked("esperar") || isBlocked(m.Groups["verb"].Value)) return m.Value;
        var after = m.Result("$'");
        if (Regex.IsMatch(after, @"^\s+(?:\p{L}+(?:ar|er|ir|arem|erem|irem)|que)\b", Ci)) return m.Value;
        var person = m.Groups["person"].Value;
        if (!person.Contains(' ') && char.IsUpper(person[0]) &&
            NaturaDictionary.Load(true).Check(person.ToLowerInvariant()) &&
            !FemininePersonalNames.Contains(person) && !MasculinePersonalNames.Contains(person)) return m.Value;
        if (!m.Groups["article"].Success)
        {
            var article = PersonalNameArticle(person, names);
            var preposition = article == "o" ? "pelo" : article == "a" ? "pela" : "por";
            return m.Groups["verb"].Value + m.Groups["gap"].Value +
                (CaseHelper.IsAllUpper(m.Value) ? preposition.ToUpperInvariant() : preposition) + " " + person;
        }
        var prep = m.Groups["article"].Value.ToLowerInvariant() switch
        { "a" => "pela", "os" => "pelos", "as" => "pelas", _ => "pelo" };
        return m.Groups["verb"].Value + m.Groups["gap"].Value +
            (CaseHelper.IsAllUpper(m.Value) ? prep.ToUpperInvariant() : CaseHelper.MatchCase(m.Groups["article"].Value, prep)) +
            m.Groups["space"].Value + person;
    });

    public static string WaitingComplements(string text, ReferenceHint? reference)
    {
        if (!Regex.IsMatch(reference?.EnglishText ?? "", @"\b(?:keep|kept|keeping)\s+you\s+waiting\b", Ci))
            return text;
        return KeptWaiting.Replace(text, m =>
        {
            var auxiliary = m.Groups["aux"].Value;
            var before = m.Result("$`");
            var proclitic = Regex.IsMatch(before, Before + @"(?:não|nunca|jamais|que|se|quando|já|também)\s*$", Ci);
            var head = proclitic ? "te " + auxiliary.ToLowerInvariant() : PortugueseVerbs.AttachEnclitic(auxiliary, "te");
            return CaseHelper.MatchCase(auxiliary, head + " deixado à espera");
        });
    }

    private static readonly Regex ReferencedConditionalComplement = new(
        Before + @"(?<pre>se\s+" + Adverbs + @")(?<v>\p{L}+)(?<bridge>\s+que\s+" + Adverbs +
        @")(?<aux>\p{L}+)" + After + @"(?!-)", Ci);
    private static readonly Regex SharedPredicate = new(
        @"(?=" + Before + @"(?:(?<subject>você|tu)\s+)?(?<head>\p{L}+)" + After +
        @"(?<body>[^,;.!?]{0,100}?)(?<link>,\s*(?:(?:e|mas|ou)\s+)?|\s+(?:e|mas|ou)\s+)" +
        @"(?<adv>" + Adverbs + @")(?<tail>\p{L}+)" + After + @"(?!-))", Ci);
    private static readonly Regex SubjectChange = new(
        Before + @"(?:eu|ele|ela|eles|elas|nós|vocês|você|tu|que|quem|se|quando|porque|embora)\b" +
        @"|(?-i:\p{Lu}[\p{Ll}\p{M}]+)", Ci);
    private static readonly Regex SubordinateOpening = new(
        @"(?:^|[.!?;\n])[\s\uE000-\uF8FF\p{P}-]*(?:(?:e|mas|então|bem|ok)[,;]?\s+)?" +
        @"(?:se|quando|caso|enquanto|assim\s+que|sempre\s+que|logo\s+que|até\s+que|a\s+menos\s+que)\s+[^,;.!?]*$", Ci);
    private static readonly Regex EnglishSharedYouPredicate = new(
        @"^\s*(?:(?:all\s+right|okay|ok|well|right)[,;]?\s+)?(?:(?:and|but|or)\s+)?" +
        @"(?:(?:if|when|whenever|unless|as\s+soon\s+as)\s+)?" +
        @"you(?:['’](?:re|ve|ll|d))?\b[^.!?;]{1,120}?(?:,\s*|\s+(?:and|but|or)\s+)" +
        @"you(?:['’](?:re|ve|ll|d))?\b", Ci);

    public static string InformalPredicates(string text, ReferenceHint? reference)
    {
        var conditionalMatches = ReferencedConditionalComplement.Matches(text);
        if (conditionalMatches.Count == 1 && reference is { RepeatedConditionalYouSubject: true })
            text = ReferencedConditionalComplement.Replace(text, m =>
                m.Groups["v"].Value.EndsWith("sse", StringComparison.OrdinalIgnoreCase) &&
                Second(m.Groups["v"].Value, out var verb) && Second(m.Groups["aux"].Value, out var auxiliary)
                    ? m.Groups["pre"].Value + verb + m.Groups["bridge"].Value + auxiliary : m.Value);

        var changedTails = new HashSet<int>();
        ClauseAgreementAnalysis? analysis = null;
        // Overlapping heads support chained predicates; right-to-left edits preserve captured offsets.
        foreach (Match m in SharedPredicate.Matches(text).Cast<Match>().Reverse())
        {
            var head = m.Groups["head"].Value;
            if ((!m.Groups["subject"].Success || !Second(head, out _)) &&
                !PortugueseVerbs.IsSecondPersonForm(head)) continue;
            // "Quando és seguido, fica parado": a comma closing a subordinate clause starts the main clause (often a command).
            if (reference?.EnglishText == null && m.Groups["link"].Value.TrimStart().StartsWith(',') && !Regex.IsMatch(m.Groups["link"].Value, @"\b(?:e|mas|ou)\s*$", Ci) &&
                SubordinateOpening.IsMatch(text[..m.Groups["head"].Index])) continue;
            if (SubjectChange.IsMatch(m.Groups["body"].Value) ||
                reference?.EnglishText != null && !EnglishSharedYouPredicate.IsMatch(reference.EnglishText) ||
                reference?.EnglishText == null && !Regex.IsMatch(head,
                    @"^(?:é|és|era|eras|foi|foste|seria|serias|está|estás|estava|estavas|esteve|estiveste)$", Ci) ||
                PortugueseVerbs.IsSecondPersonForm(m.Groups["tail"].Value) ||
                !PortugueseVerbs.TryThirdToSecond(m.Groups["tail"].Value, out var second)) continue;
            var tail = m.Groups["tail"];
            if ((analysis ??= new ClauseAgreementAnalysis(text)).Decide(tail.Index, reference).Allowed && changedTails.Add(tail.Index))
                text = text.Remove(tail.Index, tail.Length).Insert(tail.Index, second);
        }
        return text;
    }

    private static readonly Regex ExplanatoryThing = new(
        @"(?<head>(?:^|(?<=[.!?;\n]))[\s\uE000-\uF8FF-]*(?:(?:bem|mas|olha|pois)[,;]?\s+)?)" +
        @"(?<phrase>o\s+(?:negócio|lance)|a\s+coisa)(?<space>\s+)(?<verb>é|era)" +
        @"(?:(?<que>\s+que)\b|(?<comma>\s*,\s*)|(?=\s*[\uE000-\uF8FF]*(?:$|\.\.\.|…|\r?\n)))", Ci);
    private static readonly Regex EnglishExplanatoryThing = new(
        @"(?:^|[.!?]\s*)(?:(?:well|but|look|so)[,;]?\s+)?(?:the\s+)?thing\s+(?:is|was)\b" +
        @"(?=\s*(?:that\b|[,.:…]|$))", Ci);

    public static string ExplanatoryIntroductions(string text, ReferenceHint? reference) =>
        EnglishExplanatoryThing.IsMatch(reference?.EnglishText ?? "")
            ? ExplanatoryThing.Replace(text, m => m.Groups["head"].Value +
                CaseHelper.MatchCase(m.Groups["phrase"].Value, "a questão") + m.Groups["space"].Value +
                m.Groups["verb"].Value + (m.Groups["que"].Success ? m.Groups["que"].Value :
                    m.Groups["comma"].Success ? " que " : "")) : text;

    public static string Clauses(string text, ReferenceHint? reference)
    {
        ClauseAgreementAnalysis? analysis = null;
        text = ControlledClause.Replace(text, m =>
        {
            var head = m.Groups["head"].Value;
            var verb = m.Groups["v"].Value;
            if (!ControlHeads.Contains(head) || !ControlledVerbs.Contains(verb) || !Second(verb, out var second)) return m.Value;
            if (!(analysis ??= new ClauseAgreementAnalysis(text)).Decide(m.Groups["v"].Index, reference).Allowed) return m.Value;
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
        analysis = null;
        text = ConditionalTail.Replace(text, m =>
        {
            var verb = m.Groups["v"].Value;
            var tail = m.Groups["tail"].Value;
            if (!(analysis ??= new ClauseAgreementAnalysis(text)).Decide(m.Groups["tail"].Index, reference).Allowed) return m.Value;
            if (Regex.IsMatch(m.Groups["gap"].Value, Before + @"(?:ele|ela|eles|elas|nós|eu|você|vocês)\b|(?-i:\p{Lu}\p{Ll}+)", Ci))
                return m.Value;
            if (!verb.EndsWith("sse", StringComparison.OrdinalIgnoreCase) && verb.ToLowerInvariant() is not ("fosse" or "tivesse" or "pudesse")) return m.Value;
            if (!PortugueseVerbs.TrySplitFuture(tail, out _, out var ending, false) ||
                !ending.Equals("ia", StringComparison.OrdinalIgnoreCase) || reference is { ThirdPerson: true } ||
                !Second(tail, out var second)) return m.Value;
            return m.Value[..(m.Groups["tail"].Index - m.Index)] + second;
        });
        if (reference is { RepeatedYouSubject: true })
        {
            analysis = null;
            text = ExplicitReferenceClause.Replace(text, m =>
                (analysis ??= new ClauseAgreementAnalysis(text)).Decide(m.Groups["v"].Index, reference).Allowed &&
                !KeepsExtractedQuestionSubject(text[..m.Groups["v"].Index], reference) &&
                Second(m.Groups["v"].Value, out var second)
                ? m.Value[..(m.Groups["v"].Index - m.Index)] + second : m.Value);
        }
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

    private const string ExamObjectPattern =
        @"(?<gap>\s+)(?<det>o|a|os|as|um|uma|uns|umas|este|esta|estes|estas|esse|essa|esses|essas|aquele|aquela)\s+" +
        @"(?<exam>testes?|provas?|exames?|concursos?|vestibulares?|questionários?|avaliaç(?:ão|ões))";
    private static readonly Regex PerfectScoreVerb = new(
        DialogueBefore + @"(?<![\d_])(?<v>" + Alternatives(Forms("gabaritar").Concat(new[]
        {
            "gabaritais", "gabaritastes", "gabaritáveis", "gabaritareis", "gabaritaríeis",
            "gabariteis", "gabaritásseis", "gabaritardes", "gabaritáreis", "gabaritai",
        })) + @")" + After + @"(?![\d_])(?:(?:" + ExamObjectPattern +
        @"|(?<gap>\s+)(?:todas\s+)?(?:as\s+)?(?<questions>perguntas|questões|respostas))" + After + @")?", Ci);
    private static readonly Regex ExamKey = new(
        DialogueBefore + @"(?<![\d_])(?<det>o|os|um|uns)(?<gap>\s+)(?<noun>gabaritos?)" + After +
        @"(?=\s+(?:de|do|da|dos|das)\s+(?:(?:um|uma|uns|umas|este|esta|estes|estas)\s+)?(?:testes?|provas?|exames?|questionários?|questões)\b)", Ci);

    private static string PerfectScores(string text, string english, Func<string, bool>? blocked)
    {
        text = PerfectScoreVerb.Replace(text, m =>
        {
            var verb = m.Groups["v"].Value;
            if (blocked?.Invoke("gabaritar") == true || blocked?.Invoke(verb) == true ||
                PortugueseConverter.HasNominalDeterminerBefore(m.Result("$`"))) return m.Value;
            if (verb.Equals("gabarito", StringComparison.OrdinalIgnoreCase) &&
                !Regex.IsMatch(m.Result("$`"), @"\beu\s+(?:(?:não|nunca|sempre|já|também|só)\s+){0,3}$", Ci))
                return m.Value;
            var after = m.Result("$'");
            var exam = m.Groups["exam"];
            var questions = m.Groups["questions"];
            if (!exam.Success && !questions.Success &&
                Regex.IsMatch(after, @"^\s+(?!(?:em|no|na|nos|nas|num|numa|sempre|nunca|também|hoje|ontem|amanhã|novamente|outra\s+vez)\b)\p{L}", Ci))
                return m.Value;
            if (Regex.IsMatch(english, @"\b(?:qualif(?:y|ies|ied|ying)|qualifications?|templates?|gauges?)\b", Ci))
                return m.Value;
            var ending = verb["gabarit".Length..];
            var participle = Regex.IsMatch(ending, @"^ad[oa]s?$", Ci);
            if (participle && !Regex.IsMatch(m.Result("$`"), @"\b(?:ter|tenho|tens|tem|temos|têm|tinha|tinham|tínhamos|" +
                @"tive|teve|tivemos|tiveram|tiver|tiverem|tenha|tenham|havia|haviam|tivesse|tivessem)\s+$", Ci))
                return Regex.IsMatch(m.Result("$`"), @"\b(?:testes?|provas?|exames?|questionários?|avaliaç(?:ão|ões))" +
                    @"\s+(?:(?:foi|foram|está|estão|estava|estavam|tinha\s+sido|tem\s+sido)\s+)?$", Ci)
                    ? CaseHelper.MatchCase(verb, "resolvid" + ending[2..] + " sem erros") : m.Value;
            var result = "acert" + ending;
            if (questions.Success)
                result += m.Groups["gap"].Value + "todas as " + questions.Value;
            else
            {
                result += " em tudo";
                if (exam.Success)
                {
                    var det = m.Groups["det"].Value.ToLowerInvariant();
                    result += m.Groups["gap"].Value + "n" + det + " " + exam.Value;
                }
            }
            return CaseHelper.MatchCase(verb, result);
        });
        return ExamKey.Replace(text, m =>
        {
            var noun = m.Groups["noun"].Value;
            if (blocked?.Invoke("gabarito") == true || blocked?.Invoke(noun) == true) return m.Value;
            var det = m.Groups["det"].Value;
            var target = (det.StartsWith("u", StringComparison.OrdinalIgnoreCase) ? "umas" : "as") + m.Groups["gap"].Value;
            return CaseHelper.MatchCase(m.Value, target + "soluções");
        });
    }

    private static readonly Dictionary<string, string> WiretapForms = MapForms("grampear", "colocar");
    private static readonly Dictionary<string, string> InterceptForms = MapForms("grampear", "intercetar");
    private static readonly Regex InterceptConversation = new(
        Before + @"(?<v>" + FinitePattern("grampear") + @")(?=\s+" +
        @"(?:(?:a|as|uma|umas|alguma|algumas|essa|essas|esta|estas)\s+)?" +
        @"(?:conversas?|comunicações?|chamadas?)" + After + ")", Ci);
    private static readonly Regex WiretapObject = new(
        Before + @"(?<v>" + FinitePattern("grampear") + @")(?<gap>\s+)" +
        @"(?<object>(?:(?:o|a|os|as|um|uma|uns|umas|este|esta|esse|essa|seu|sua|meu|minha|teu|tua)\s+)?" +
        @"(?<noun>telefones?|celulares?|telemóveis?|telemóvel|linha\s+telef[oóô]nica|linhas\s+telef[oóô]nicas|" +
        @"local|casa|sala|gabinete|escritório))" + After, Ci);
    private static readonly Dictionary<string, string> StallForms = MapForms("embromar", "ganhar");
    private static readonly Dictionary<string, string> ProcrastinateForms = MapForms("embromar", "protelar");
    private static readonly Regex StallVerb = new(Before + @"(?<v>" + FinitePattern("embromar") + @")" + After, Ci);
    private static readonly Dictionary<string, string> DozeForms = MapForms("cochilar", "dormitar");
    private static readonly Regex DozeVerb = new(Before + @"(?<v>" + FinitePattern("cochilar") + @")" + After, Ci);

    private static readonly Dictionary<string, string> WidenForms = MapForms("lacear", "alargar");
    private static readonly Dictionary<string, string> YieldForms = MapForms("lacear", "ceder");
    private static readonly Dictionary<string, string> SoftenForms = MapForms("lacear", "amolecer");
    private static readonly Regex LacearVerb = new(
        DialogueBefore + @"(?<v>" + Alternatives(WidenForms.Keys.Append("laceá")) + @")" +
        @"(?<cl>-(?:me|te|se|nos|vos|o|a|os|as|lo|la|los|las))?" + After, Ci);

    private static string LacearVocabulary(string text, ReferenceHint? reference, Func<string, bool>? isBlocked) =>
        LacearVerb.Replace(text, m =>
        {
            var verb = m.Groups["v"].Value;
            if (isBlocked?.Invoke("lacear") == true || isBlocked?.Invoke(verb) == true) return m.Value;
            var before = Regex.Match(m.Result("$`"), @"[^.!?;]*$").Value;
            var after = Regex.Match(m.Result("$'"), @"^[^.!?;]*").Value;
            var local = before + m.Value + after;
            if (Regex.IsMatch(local, @"\b(?:laços?|fitas?|enfeites?|ornamentos?)\b", Ci) ||
                Regex.IsMatch(reference?.EnglishText ?? "", @"\b(?:ribbons?|bows?|decorate|adorn)\b", Ci))
                return m.Value;
            var cliticInfinitive = verb.Equals("laceá", StringComparison.OrdinalIgnoreCase);
            var key = cliticInfinitive ? "lacear" : verb;
            var directObject = Regex.IsMatch(after,
                @"^\s+(?:(?:um\s+pouco|mais|bem|ligeiramente)\s+)?(?:(?:o|a|os|as|um|uma|uns|umas|este|esta|estes|estas|esse|essa|esses|essas|" +
                @"meus?|minhas?|seus?|suas?|teus?|tuas?|nossos?|nossas?)\s+\p{L}|(?:sapatos?|botas?|calças?|luvas?|cintos?|algemas?|couro|pele)\b)", Ci) ||
                Regex.IsMatch(m.Groups["cl"].Value, @"^-(?:o|a|os|as|lo|la|los|las)$", Ci);
            var material = Regex.IsMatch(local, @"\b(?:couro|pele|material)\b", Ci) &&
                !Regex.IsMatch(local, @"\b(?:sapatos?|botas?|calças?|luvas?|algemas?|cintos?)\b", Ci);
            var soften = material || Regex.IsMatch(reference?.EnglishText ?? "", @"\bsoften(?:s|ed|ing)?\b", Ci);
            var target = (soften ? SoftenForms : directObject || m.Groups["cl"].Success || Regex.IsMatch(verb, @"ad[oa]s?$", Ci)
                ? WidenForms : YieldForms)[key];
            if (cliticInfinitive)
                target = VerbConjugator.CliticInfinitive(soften ? "amolecer" : "alargar");
            return CaseHelper.MatchCase(verb, target) + m.Groups["cl"].Value;
        });

    private static readonly Dictionary<string, string> PromisePersons = BuildPromisePersons();
    private static Dictionary<string, string> BuildPromisePersons()
    {
        var forms = Forms("prometer");
        // Same slots as VerbConjugator, through personal infinitives; ambiguous finite forms need an explicit subject.
        var persons = new[]
        {
            "", "me", "te", "se", "nos", "se", "me", "te", "se", "se",
            "", "te", "nos", "se", "me", "te", "se", "nos", "se",
            "", "te", "nos", "se", "", "te", "nos", "se",
            "", "te", "nos", "se", "te", "nos", "se",
        };
        return forms.Take(persons.Length).Select((form, i) => (form, person: persons[i]))
            .GroupBy(item => item.form, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key,
                group => group.Select(item => item.person).Distinct().Count() == 1 ? group.First().person : "",
                StringComparer.OrdinalIgnoreCase);
    }
    private static readonly Regex PromiseLaugh = new(
        Before + @"(?<v>" + Alternatives(PromisePersons.Keys) + @")(?<gap>\s+)(?<neg>não\s+|nunca\s+)?" +
        @"(?:se\s+)?(?<inf>rir)(?:-se)?" + After + @"(?!-)", Ci);
    private static readonly Regex TellNobody = new(
        Before + @"(?<v>" + Alternatives(Forms("contar")) + "|" +
        PortugueseConverter.PronounRules.DizerVerbForms + ")" +
        @"(?<gap>\s+)(?<prep>para)(?=\s+ninguém\b)", Ci);
    private static readonly Regex PromiseRequest = new(
        @"(?<pre>^[\s""“«'\p{Co}-]*)(?<v>promete)(?=\s+(?:não\s+)?\p{L}+(?:ar|er|ir)\b[^.!?]*\?\s*[\p{Co}]*$)", Ci);

    public static string PromiseQuestions(string text, ReferenceHint? reference) =>
        reference != null && !reference.StartsWithSecondPersonSubject ? text :
        PromiseRequest.Replace(text, m => m.Groups["pre"].Value + CaseHelper.MatchCase(m.Groups["v"].Value, "prometes"));

    public static string PromiseComplements(string text)
    {
        text = PromiseLaugh.Replace(text, m =>
        {
            var clitic = PromisePersons[m.Groups["v"].Value];
            var subject = Regex.Match(m.Result("$`"),
                @"\b(?<subject>eu|tu|ele|ela|nós|vocês|eles|elas)\s+(?:(?:não|nunca|sempre|também|já|ainda|só|realmente)\s+){0,3}$", Ci);
            if (subject.Success)
                clitic = subject.Groups["subject"].Value.ToLowerInvariant() switch { "eu" => "me", "tu" => "te", "nós" => "nos", _ => "se" };
            if (clitic.Length == 0) return m.Value;
            if (CaseHelper.IsAllUpper(m.Groups["v"].Value)) clitic = clitic.ToUpperInvariant();
            var infinitive = m.Groups["inf"].Value;
            return m.Groups["v"].Value + m.Groups["gap"].Value +
                (m.Groups["neg"].Success ? m.Groups["neg"].Value + clitic + " " + infinitive
                    : infinitive + "-" + clitic);
        });
        return TellNobody.Replace(text, m => m.Groups["v"].Value + m.Groups["gap"].Value +
            CaseHelper.MatchCase(m.Groups["prep"].Value, "a"));
    }

    public static string ReviewedVerbSenses(string text, ReferenceHint? reference, Func<string, bool>? isBlocked = null)
    {
        bool Allowed(string infinitive, string form) =>
            isBlocked?.Invoke(infinitive) != true && isBlocked?.Invoke(form) != true;
        var english = reference?.EnglishText ?? "";
        text = SpeechContentVocabulary(text, english, isBlocked);
        text = PerfectScores(text, english, isBlocked);
        if (Regex.IsMatch(english, @"\b(?:chatter|wires|wiretap(?:ped|ping)?|intercept(?:ed|s|ing)?)\b", Ci))
            text = InterceptConversation.Replace(text, m => Allowed("grampear", m.Value)
                ? CaseHelper.MatchCase(m.Value, InterceptForms[m.Value]) : m.Value);
        text = WiretapObject.Replace(text, m =>
        {
            if (!Allowed("grampear", m.Groups["v"].Value)) return m.Value;
            var noun = m.Groups["noun"].Value;
            if (Regex.IsMatch(text, @"\b(?:vídeo|video|câmaras?|câmeras?)\b", Ci)) return m.Value;
            if (noun.ToLowerInvariant() is "local" or "casa" or "sala" or "gabinete" or "escritório" &&
                !Regex.IsMatch(english, @"\b(?:wire|wired|wiring|wiretap|wiretapped|bug|bugged|bugging)\b", Ci))
                return m.Value;
            var verb = m.Groups["v"].Value;
            return CaseHelper.MatchCase(verb, WiretapForms[verb]) +
                (CaseHelper.IsAllUpper(verb) ? " SOB ESCUTA" : " sob escuta") +
                m.Groups["gap"].Value + m.Groups["object"].Value;
        });
        if (Regex.IsMatch(english, @"\b(?:doz(?:e|es|ed|ing)|nap(?:s|ped|ping)?)\b", Ci) &&
            !Regex.IsMatch(english, @"\b(?:opportunity|chance|mistake)\b", Ci))
            text = DozeVerb.Replace(text, m => Allowed("cochilar", m.Value) &&
                !(m.Value.Equals("cochilo", StringComparison.OrdinalIgnoreCase) &&
                  PortugueseConverter.HasNominalDeterminerBefore(m.Result("$`")))
                ? CaseHelper.MatchCase(m.Value, DozeForms[m.Value]) : m.Value);
        if (Regex.IsMatch(english, @"\bstall(?:s|ed|ing)?\b", Ci) &&
            !Regex.IsMatch(english, @"\b(?:engine|motor|car|vehicle|plane|aircraft)\b", Ci))
            text = StallVerb.Replace(text, m =>
            {
                if (!Allowed("embromar", m.Value)) return m.Value;
                var after = m.Result("$'");
                if (Regex.IsMatch(after, @"^\s+(?:você|vocês|tu|ele|ela|eles|elas)\b", Ci)) return m.Value;
                if (Regex.IsMatch(after, @"^\s+(?:o|a|os|as|um|uma|uns|umas|este|esta|esse|essa)\b", Ci))
                    return CaseHelper.MatchCase(m.Value, ProcrastinateForms[m.Value]);
                return CaseHelper.MatchCase(m.Value, StallForms[m.Value]) +
                    (CaseHelper.IsAllUpper(m.Value) ? " TEMPO" : " tempo");
            });
        return LacearVocabulary(text, reference, isBlocked);
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
    private static readonly Regex PainInTheNeck = new(
        DialogueBefore + @"(?:(?<det>um|uma|uns|umas)|(?<copula>sou|és|é|somos|são|era|eras|éramos|eram|foi|fui|foram|será|serão|seria|seriam))" +
        @"(?<gap>\s+)(?<phrase>p(?:é|és)(?:\s+|-)no(?:\s+|-)saco)" + After, Ci);
    private static readonly Dictionary<string, string> AnnoyForms = MapForms("encher", "chatear")
        .Concat(MapForms("torrar", "chatear")).ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.OrdinalIgnoreCase);
    private static readonly Regex AnnoyingSack = new(
        DialogueBefore + @"(?:(?<pre>me|te|nos|vos|lhe|lhes)\s+)?(?<verb>" + Alternatives(AnnoyForms.Keys) + @")" +
        @"(?<post>-(?:me|te|nos|vos|lhe|lhes))?\s+(?:o\s+)?(?:(?<poss>meu|teu|nosso|vosso)\s+)?saco" +
        After + @"(?!\s+(?:de|com)\b)", Ci);
    private static readonly Regex AnnoyReference = new(
        @"\b(?:annoy(?:s|ed|ing)?|bother(?:s|ed|ing)?|pester(?:s|ed|ing)?|nag(?:s|ged|ging)?)\b|" +
        @"\b(?:pain\s+in\s+the\s+(?:ass|arse|neck|butt)|get(?:s|ting)?\s+on\s+\w+\s+nerves)\b", Ci);

    public static string AnnoyanceSlang(string text, ReferenceHint? reference, bool pronouns, Func<string, bool> isBlocked)
    {
        text = PainInTheNeck.Replace(text, m =>
        {
            if (isBlocked("pé no saco") || isBlocked("pés no saco")) return m.Value;
            var before = Regex.Match(m.Result("$`"), @"[^.!?;]*$").Value;
            if (m.Groups["copula"].Success) before += m.Groups["copula"].Value + " ";
            var after = Regex.Match(m.Result("$'"), @"^[^.!?;]*").Value;
            if (Regex.IsMatch(before, @"\b(?:pôs|colocou|meteu|enfiou|enfiar|meter|pôr|colocar|com|tirou|tirar)\s+$", Ci) ||
                Regex.IsMatch(after, @"^\s+(?:de\s+(?:dormir|papel|plástico|lixo|batatas)|com\s+)", Ci))
                return m.Value;
            var feminine = m.Groups["det"].Value.EndsWith("a", StringComparison.OrdinalIgnoreCase) ||
                m.Groups["det"].Value.EndsWith("as", StringComparison.OrdinalIgnoreCase);
            var plural = m.Groups["det"].Value.EndsWith("s", StringComparison.OrdinalIgnoreCase);
            if (m.Groups["copula"].Success)
            {
                feminine = Regex.IsMatch(before, @"\b(?:ela|elas)\s+\S+\s+$", Ci);
                plural = Regex.IsMatch(m.Groups["copula"].Value, @"^(?:somos|são|éramos|eram|foram|serão|seriam)$", Ci);
            }
            var thing = Regex.IsMatch(before,
                @"\b(?:isso|isto|aquilo|tudo|o\s+(?:trabalho|trânsito|processo|filme)|a\s+(?:burocracia|papelada|espera))" +
                @"\s+(?:é|era|foi|será|seria|parece|tornou-se)\s+$", Ci);
            var person = feminine || Regex.IsMatch(before, @"\b(?:eu|tu|ele|ela|nós|vocês|eles|elas)" +
                @"\s+(?:sou|és|é|somos|são|era|eras|éramos|eram|foi|fui|foram|será|serão|seria|seriam|parece|parecem)\s+$", Ci) ||
                Regex.IsMatch(after, @"^\s+(?:da\s+polícia|do\s+(?:FBI|departamento)|que\s+(?:me|te|nos)\s+)", Ci) ||
                Regex.IsMatch(reference?.EnglishText ?? "", @"\b(?:he|she|you|they|guy|woman|man|cop|NYPD|FBI)\b", Ci);
            var noun = thing ? "chatice" : person ? feminine ? "chata" : "chato" : "incómodo";
            if (plural) noun += "s";
            var det = thing ? plural ? "umas" : "uma" : person ? m.Groups["det"].Value.ToLowerInvariant() : plural ? "uns" : "um";
            if (m.Groups["copula"].Success) det = thing || feminine ? plural ? "umas" : "uma" : plural ? "uns" : "um";
            if (m.Groups["copula"].Success && CaseHelper.IsAllUpper(m.Value)) det = det.ToUpperInvariant();
            return (m.Groups["copula"].Success ? m.Groups["copula"].Value + m.Groups["gap"].Value + det + " " :
                CaseHelper.MatchCase(m.Groups["det"].Value, det) + m.Groups["gap"].Value) +
                CaseHelper.MatchCase(m.Groups["phrase"].Value, noun);
        });
        if (!pronouns) return text;
        return AnnoyingSack.Replace(text, m =>
        {
            var verb = m.Groups["verb"].Value;
            var infinitive = verb.StartsWith("torr", StringComparison.OrdinalIgnoreCase) ? "torrar" : "encher";
            if (isBlocked(infinitive) || isBlocked(verb) || isBlocked("encher o saco") || isBlocked("torrar o saco"))
                return m.Value;
            var before = Regex.Match(m.Result("$`"), @"[^.!?;]*$").Value;
            var after = m.Result("$'");
            if (PortugueseVerbs.IsFutureOrConditional(verb) ||
                Regex.IsMatch(after, @"^\s+(?:até|cheio|vazio|azul|preto|branco)\b", Ci) ||
                Regex.IsMatch(reference?.EnglishText ?? "", @"\b(?:fill(?:s|ed|ing)?|roast(?:s|ed|ing)?)\b[^.!?]*\b(?:bag|sack)\b|" +
                    @"\b(?:money|profit|profits)\b", Ci)) return m.Value;
            var hasClitic = m.Groups["pre"].Success || m.Groups["post"].Success;
            if (hasClitic && m.Groups["poss"].Success) return m.Value;
            var stopCommand = Regex.IsMatch(before, @"\b(?:para|pare|pares|parem|deixa|deixe|deixes|deixem)\s+de\s+$", Ci);
            if (!hasClitic && !m.Groups["poss"].Success &&
                !stopCommand && !AnnoyReference.IsMatch(reference?.EnglishText ?? "")) return m.Value;
            var target = CaseHelper.MatchCase(verb, AnnoyForms[verb]);
            if (m.Groups["pre"].Success) return m.Groups["pre"].Value + " " + target;
            if (m.Groups["post"].Success) return PortugueseVerbs.AttachEnclitic(target, m.Groups["post"].Value[1..]);
            if (m.Groups["poss"].Success)
            {
                var clitic = m.Groups["poss"].Value.ToLowerInvariant() switch
                { "meu" => "me", "teu" => "te", "nosso" => "nos", _ => "vos" };
                return clitic + " " + target;
            }
            return target;
        });
    }
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

    private static readonly Dictionary<string, string> SlowPeriodReferences = new(StringComparer.OrdinalIgnoreCase)
    {
        ["dia"] = "day", ["dias"] = "days", ["manhã"] = "morning", ["manhãs"] = "mornings",
        ["tarde"] = "afternoon", ["tardes"] = "afternoons", ["noite"] = "night", ["noites"] = "nights",
        ["semana"] = "week", ["semanas"] = "weeks", ["mês"] = "month", ["meses"] = "months",
    };
    private static readonly Regex QuantifiedPoliceNoun = new(
        Before + @"(?<quantity>(?:há|havia|houve|haverá|haveria|haja|houvesse|houver)(?:\s+(?:ainda|já|também|sempre|só|apenas))?|" +
        @"(?:(?:muito|muitos|muitas|bem|ainda|cada\s+vez)\s+)?(?:mais|menos)|" +
        @"(?:alguns|algumas|vários|várias|muitos|muitas|poucos|poucas|tantos|tantas|quantos|quantas)|\d+)" +
        @"(?<gap>\s+)(?<noun>policiais)" + After, Ci);

    private static readonly Regex PrepositionalPoliceNoun = new(
        Before + @"(?<prep>para|pra|com|sem|entre|perante|por|a|em)(?<gap>\s+)" +
        @"(?<noun>policiais)" + After +
        @"(?=\s*(?:[,.;:!?…\uE000-\uF8FF]|$)|\s+(?:e|ou|que|como|do|da|dos|das|" +
        @"corruptos|corruptas|honestos|honestas|armados|armadas|fardados|fardadas|" +
        @"uniformizados|uniformizadas|feridos|feridas|mortos|mortas|suficientes|" +
        @"presentes|envolvidos|envolvidas|disfarçados|disfarçadas)\b)", Ci);

    public static string PoliceNouns(string text)
    {
        text = QuantifiedPoliceNoun.Replace(text, m =>
            m.Groups["quantity"].Value + m.Groups["gap"].Value + CaseHelper.MatchCase(m.Groups["noun"].Value, "polícias"));
        return PrepositionalPoliceNoun.Replace(text, m =>
        {
            if (m.Groups["prep"].Value.Equals("a", StringComparison.OrdinalIgnoreCase) &&
                Regex.IsMatch(text[..m.Index], @"\b(?:com|sem|para|pra|por|entre|perante|em)\s+$", Ci))
                return m.Value;
            return m.Groups["prep"].Value + m.Groups["gap"].Value + CaseHelper.MatchCase(m.Groups["noun"].Value, "polícias");
        });
    }

    private static readonly Regex PossessiveDoorway = new(
        Before + @"(?<prep>na|nas)(?<gap>\s+)(?<poss>minha|tua|sua|nossa|vossa|minhas|tuas|suas|nossas|vossas)" +
        @"(?<space>\s+)(?<door>portas?)" + After, Ci);
    private static readonly Regex AtDoorReference = new(
        @"\bat\s+(?:(?:my|your|his|her|our|their|the)\s+)(?:front\s+|back\s+)?doors?\b", Ci);

    public static string DoorwayLocation(string text, ReferenceHint? reference) =>
        PossessiveDoorway.Replace(text, m =>
        {
            var before = Regex.Match(m.Result("$`"), @"[^.!?;]*$").Value;
            var after = Regex.Match(m.Result("$'"), @"^[^.!?;]*").Value;
            var literalSurface = Regex.IsMatch(before + after,
                @"\b(?:marcas?|buracos?|riscos?|sangue|tinta|cartaz(?:es)?|avisos?|placas?|números?|fechaduras?|maçanetas?|chaves?|desenhos?|pintad[oa]s?|colad[oa]s?|escrit[oa]s?)\b", Ci);
            if (literalSurface) return m.Value;
            var presence = Regex.IsMatch(before,
                @"\b(?:há|havia|houve|haverá|haveria|está|estão|estava|estavam|ficou|ficaram|espera|esperam|esperava|esperavam)" +
                @"\s+(?:(?:um|uma|uns|umas|o|a|os|as|algum|alguma|alguns|algumas|mais|menos|dois|duas|três)\s+)?" +
                @"(?:polícias?|policiais|agentes?|pessoas?|homens?|mulheres|mulher|rapazes|raparigas|visitas?|alguém|ninguém)\s+$", Ci) ||
                Regex.IsMatch(before, @"\b(?:eu|tu|ele|ela|nós|vocês|eles|elas|alguém|ninguém|polícias|agentes|pessoas|homens|mulheres)" +
                    @"\s+(?:estou|estás|está|estamos|estão|estava|estavas|estávamos|estavam|espera|esperam|esperava|esperavam)\s+$", Ci);
            if (!presence && !AtDoorReference.IsMatch(reference?.EnglishText ?? "")) return m.Value;
            return CaseHelper.MatchCase(m.Groups["prep"].Value,
                m.Groups["prep"].Value.Equals("nas", StringComparison.OrdinalIgnoreCase) ? "às" : "à") +
                m.Groups["gap"].Value + m.Groups["poss"].Value + m.Groups["space"].Value + m.Groups["door"].Value;
        });

    private static readonly Regex SlowPeriod = new(
        Before + @"(?<noun>" + Alternatives(SlowPeriodReferences.Keys) + @")(?<gap>\s+)(?<adverb>devagar)" +
        After + @"(?!\s+(?:demais|de\s+mais)\b)", Ci);

    // Brazilian slang "faturar" (to score drugs) is "comprar" in Portugal; invoicing/earning senses are kept.
    private static readonly Dictionary<string, string> ScoreForms = MapForms("faturar", "comprar");
    private const string Drugs =
        @"molly|ecstasy|êxtase|md|mdma|coca|cocaína|pó|erva|maconha|marijuana|haxixe|droga|drogas|heroína|crack|metanfetamina|meta|" +
        @"comprimidos?|pílulas?|bala|balas|ácido|lsd|ganza|cogumelos|oxi|oxicodona|fentanil|cetamina|ketamina|anfetaminas?|speed|bagulho|pedra|pedras";
    private static readonly Regex ScoreDrugs = new(
        Before + @"(?<v>" + Alternatives(Forms("faturar")) + @")(?<rest>\s+(?:(?:um\s+pouco|um\s+bocado|uma\s+grama|um\s+grama|umas\s+gramas|um\s+saco|um\s+saquinho|uma\s+trouxinha)\s+(?:de|da|do)\s+|" +
        @"(?:um|uma|uns|umas|algum|alguma|alguns|algumas|mais|o|a|os|as|essa|esse|essas|esses|aquela|aquele|aquelas|aqueles)\s+)?(?:" + Drugs + @"))" + After, Ci);
    private static readonly Regex EnglishBuyDrugs = new(
        @"\b(?:buy|buys|bought|buying|purchas\w*|score|scores|scored|scoring)\s+(?:some|a\s+(?:bag|gram|hit|dime|eight|eightball|ounce|kilo)|drugs|dope|coke|weed|crack|meth|heroin|pills|molly|smack|blow|ecstasy|x)\b", Ci);
    // Intransitive drug "score": "how much you looking to score?", "where I used to score", "trying to score from him".
    private static readonly Regex EnglishScoreAlone = new(
        @"(?<!\b(?:the|a|high|low|final|credit|test|keep|keeping|kept|settle|settling|settled|even|evening|that|this|what's|whats|know)\s+)" +
        @"\b(?:score|scores|scored|scoring)\b(?!\s+(?:of|a|an|the|one|two|points?|goals?|runs?|touchdowns?|baskets?|big|high|low|well|tickets?|seats?|deals?|dates?|with|on|against|for\s+(?:the|our|their|his|her)\s+team)\b)" +
        @"(?=\s*(?:[?.!,;:…-]|$|\b(?:from|off|again|tonight|today|yesterday|earlier|later|here|there|anymore|anywhere|somewhere|downtown|uptown|" +
        @"last\s+(?:night|week|time)|that\s+(?:day|night)|(?:on|in|at|around)\s+(?:the|this|that|my|his|her|our|their)\s+\w+|for\s+(?:me|him|her|us|them))\b))", Ci);
    private static readonly Regex EnglishSport = new(
        @"\b(?:goals?|points?|touchdowns?|runs?|baskets?|game|match|team|league|innings?|penalty|tests?|exams?|scoreboard|keeper|striker|half|period|quarter|inning|overtime|field|court|pitch)\b", Ci);
    private static readonly Regex ScoreAny = new(Before + @"(?<v>" + Alternatives(Forms("faturar")) + @")" + After + @"(?!\s+(?:com|uma\s+(?:garota|mina|gata|miúda|mulher)|ela|ele|gols?|golos?|pontos?|milhões|mil|dinheiro|uma\s+nota|uma\s+grana|alto|muito|bem)" + After + ")", Ci);

    // Sports "faturar" (to score) is "marcar" in Portugal: "faturou dois gols" → "marcou dois golos".
    private static readonly Dictionary<string, string> SportScoreForms = MapForms("faturar", "marcar");
    private static readonly Regex SportScoreNoun = new(
        Before + @"(?<v>" + Alternatives(Forms("faturar")) + @")(?=\s+(?:(?:o|os|um|uns|dois|duas|três|quatro|cinco|mais|outro|outros|o\s+primeiro|o\s+segundo|o\s+terceiro|o\s+último)\s+)?" +
        @"(?:gol|gols|golo|golos|pontos?|cestos?|cestas?|touchdowns?|ensaios?|home\s+runs?|corridas?)" + After + ")", Ci);
    private static readonly Regex EnglishSportScore = new(
        @"\b(?:score|scores|scored|scoring)\b(?=[^.!?]*?\b(?:goals?|touchdowns?|baskets?|runs?|half|period|quarter|inning|overtime|game|match|team|league|penalty|field|court|pitch)\b)|" +
        @"\b(?:goals?|touchdowns?|baskets?|game|match|team|league|penalty|striker)\b[^.!?]*?\b(?:score|scores|scored|scoring)\b", Ci);
    private static readonly Regex SportScoreAny = new(
        Before + @"(?<v>" + Alternatives(Forms("faturar")) + @")" + After +
        @"(?!\s+(?:milhões|mil|dinheiro|uma\s+nota|uma\s+grana|alto|com|uma\s+(?:garota|mina|gata|miúda|mulher))" + After + ")", Ci);

    // Match halves are "partes" in Portugal: "no segundo tempo" → "na segunda parte", "2º tempo" → "2ª parte".
    private static readonly Regex MatchHalf = new(
        @"(?<pre>(?:^|(?<=[.!?…:;—–\-""(]\s*)|(?<![\p{L}\p{M}-])(?<det>(?:todo\s+)?(?:o|no|do|ao|pelo|neste|nesse|naquele|deste|desse|daquele|este|esse|aquele|seu|teu|nosso)\s+)))" +
        @"(?<ord>primeiro|segundo|1º|2º|1\.º|2\.º)\s+(?<n>tempo)(?<adj>\s+(?:inteiro|todo|seguido|inteirinho|todinho|completo|passado))?" + After +
        @"(?!\s+(?:verbal|de\s+(?:espera|resposta|reação|reacção)|composto|simples)" + After + ")", Ci);
    private static readonly Dictionary<string, string> FeminineDeterminers = new(StringComparer.OrdinalIgnoreCase)
    {
        ["o"] = "a", ["no"] = "na", ["do"] = "da", ["ao"] = "à", ["pelo"] = "pela", ["neste"] = "nesta", ["nesse"] = "nessa",
        ["naquele"] = "naquela", ["deste"] = "desta", ["desse"] = "dessa", ["daquele"] = "daquela", ["este"] = "esta",
        ["esse"] = "essa", ["aquele"] = "aquela", ["seu"] = "sua", ["teu"] = "tua", ["nosso"] = "nossa",
    };
    // Quarters ("terceiro período/tempo" → "terceiro quarto") need English confirmation, since "período" is also a school term.
    private static readonly Regex EnglishGameQuarter = new(
        @"\b(?:first|second|third|fourth|1st|2nd|3rd|4th|final|last|next|this|that|what|which|each|every|the|two|three)\s+quarters?\b" +
        @"(?!\s+(?:of|mile|miles|horse|horses|hour|hours|inch|inches|pound|pounds|century|million|billion|percent|tank|cup|note|past|to|after|before)\b)" +
        @"(?!-)", Ci);
    private static readonly Regex GameQuarter = new(
        @"(?<pre>" + Before + @"(?:(?<ord>primeiro|segundo|terceiro|quarto|último|próximo|1º|2º|3º|4º|1\.º|2\.º|3\.º|4\.º|primeiros|últimos|dois\s+últimos|dois\s+primeiros)|" +
        @"(?:qual|que|em\s+que|neste|nesse|deste|desse|cada)(?:\s+o)?)\s+)(?<n>tempos?|períodos?)" + After, Ci);
    // Sports overtime is "prolongamento" in Portugal; contract extensions keep "prorrogação".
    private static readonly Regex EnglishSportOvertime = new(
        @"\b(?:overtime|extra\s+time|OT)\b(?=[^.!?]*\b(?:game|match|score|scored|win|won|lose|lost|tie|tied|goal|team|period|sudden)\b)|" +
        @"\b(?:game|match|score|scored|tied|goal|team)\b[^.!?]*\b(?:overtime|extra\s+time)\b|\b(?:into|to|in)\s+(?:overtime|extra\s+time)\b(?![^.!?]*\b(?:pay|paid|work|shift|hours)\b)", Ci);
    private static readonly Regex Overtime = new(
        Before + @"(?:(?<det>a|na|da|à|pela|uma|numa|duma|esta|nesta|desta|essa|nessa|dessa|as|nas|das|às|pelas)\s+)?(?<n>prorrogação|prorrogações|tempo\s+extra)" + After, Ci);
    private static readonly Dictionary<string, string> MasculineDeterminers = new(StringComparer.OrdinalIgnoreCase)
    {
        ["a"] = "o", ["na"] = "no", ["da"] = "do", ["à"] = "ao", ["pela"] = "pelo", ["uma"] = "um", ["numa"] = "num",
        ["duma"] = "dum", ["esta"] = "este", ["nesta"] = "neste", ["desta"] = "deste", ["essa"] = "esse", ["nessa"] = "nesse",
        ["dessa"] = "desse", ["as"] = "os", ["nas"] = "nos", ["das"] = "dos", ["às"] = "aos", ["pelas"] = "pelos",
    };

    private static string SportPeriodsInText(string text, ReferenceHint? reference)
    {
        var english = reference?.EnglishText ?? string.Empty;
        if (text.IndexOf("tempo", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            text = MatchHalf.Replace(text, m =>
            {
                var det = m.Groups["det"].Value;
                if (det.Length > 0)
                {
                    var words = det.TrimEnd().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    var last = words[^1];
                    if (!FeminineDeterminers.TryGetValue(last, out var feminine)) return m.Value;
                    var prefix = words.Length > 1 ? CaseHelper.MatchCase(words[0], "toda") + " " : string.Empty;
                    det = m.Groups["pre"].Value[..m.Groups["pre"].Value.IndexOf(det, StringComparison.Ordinal)] +
                          prefix + CaseHelper.MatchCase(last, feminine) + det[det.TrimEnd().Length..];
                }
                else det = m.Groups["pre"].Value;
                var ord = m.Groups["ord"].Value;
                var fem = ord.ToLowerInvariant() switch
                {
                    "primeiro" => "primeira", "segundo" => "segunda",
                    "1º" => "1ª", "2º" => "2ª", "1.º" => "1.ª", "2.º" => "2.ª", _ => ord,
                };
                var adj = m.Groups["adj"].Value;
                if (adj.Length > 0) adj = adj[..^1] + CaseHelper.MatchCase(adj[^1..], "a");
                return det + CaseHelper.MatchCase(ord, fem) + " " + CaseHelper.MatchCase(m.Groups["n"].Value, "parte") + adj;
            });
        }
        if (EnglishGameQuarter.IsMatch(english))
            text = GameQuarter.Replace(text, m => m.Groups["pre"].Value +
                CaseHelper.MatchCase(m.Groups["n"].Value, m.Groups["n"].Value.EndsWith('s') ? "quartos" : "quarto"));
        if (EnglishSportOvertime.IsMatch(english))
            text = Overtime.Replace(text, m =>
            {
                var noun = m.Groups["n"].Value;
                var det = m.Groups["det"].Success ? CaseHelper.MatchCase(m.Groups["det"].Value, MasculineDeterminers[m.Groups["det"].Value]) + " " : string.Empty;
                return det + CaseHelper.MatchCase(noun, noun.EndsWith("ões", StringComparison.OrdinalIgnoreCase) ? "prolongamentos" : "prolongamento");
            });
        return text;
    }

    private static readonly Regex SportContext = new(
        Before + @"(?:(?:primeira|segunda|1ª|2ª|1\.ª|2\.ª)\s+parte|prolongamento|golos?|gols?|penáltis?|pênaltis?|grande\s+penalidade|marcador|placar)" + After, Ci);

    private static string SportScoreInText(string text, ReferenceHint? reference)
    {
        text = SportScoreNoun.Replace(text, m => CaseHelper.MatchCase(m.Groups["v"].Value, SportScoreForms[m.Groups["v"].Value]));
        if (EnglishSportScore.IsMatch(reference?.EnglishText ?? "") || SportContext.IsMatch(text))
            text = SportScoreAny.Replace(text, m => CaseHelper.MatchCase(m.Groups["v"].Value, SportScoreForms[m.Groups["v"].Value]));
        return text;
    }

    private static bool EnglishScoresDrugs(string english) =>
        EnglishBuyDrugs.IsMatch(english) || (EnglishScoreAlone.IsMatch(english) && !EnglishSport.IsMatch(english));

    private static string ScoreDrugsInText(string text, ReferenceHint? reference)
    {
        text = SportPeriodsInText(text, reference);
        text = SportScoreInText(text, reference);
        text = ScoreDrugs.Replace(text, m => CaseHelper.MatchCase(m.Groups["v"].Value, ScoreForms[m.Groups["v"].Value]) + m.Groups["rest"].Value);
        if (EnglishScoresDrugs(reference?.EnglishText ?? ""))
            text = ScoreAny.Replace(text, m => CaseHelper.MatchCase(m.Groups["v"].Value, ScoreForms[m.Groups["v"].Value]));
        return text;
    }

    // "errado" used as a manner adverb is "mal" in Portugal: "entendeste tudo errado" → "percebeste tudo mal",
    // "lembrei errado" → "lembrei mal", "deu errado" → "correu mal". Only finite/infinitive forms of action verbs
    // (never copulas such as "estar errado") and only the invariable masculine singular.
    private static readonly string[] WrongMannerVerbs =
    {
        "entender", "perceber", "compreender", "ouvir", "ler", "lembrar", "recordar", "escrever", "fazer", "calcular",
        "interpretar", "escolher", "julgar", "pronunciar", "soletrar", "adivinhar", "medir", "avaliar", "ver", "dizer",
        "contar", "estimar", "traduzir", "copiar", "apostar", "jogar", "explicar", "perceber", "copiar", "montar", "programar",
        "configurar", "marcar", "somar", "pesar", "tocar", "cantar", "conduzir", "dirigir", "agir", "lidar", "responder",
    };
    private static readonly string[] IrregularMannerForms =
    {
        "faço", "fazes", "faz", "fazemos", "fazem", "fiz", "fizeste", "fez", "fizemos", "fizeram", "fazia", "fazias", "fazíamos",
        "faziam", "farei", "farás", "fará", "faremos", "farão", "faria", "farias", "faríamos", "fariam", "faça", "faças", "façamos",
        "façam", "fizesse", "fizesses", "fizéssemos", "fizessem", "fizer", "fizeres", "fizermos", "fizerem", "fazer", "fazeres",
        "fazermos", "fazerem",
        "digo", "dizes", "diz", "dizemos", "dizem", "disse", "disseste", "dissemos", "disseram", "dizia", "dizias", "dizíamos",
        "diziam", "direi", "dirás", "dirá", "diremos", "dirão", "diria", "dirias", "diríamos", "diriam", "diga", "digas", "digamos",
        "digam", "dissesse", "dissessem", "disser", "disserem", "dizer",
        "vejo", "vês", "vê", "vemos", "veem", "vi", "viste", "viu", "vimos", "viram", "via", "vias", "víamos", "viam", "verei",
        "verás", "verá", "veria", "verias", "veriam", "veja", "vejas", "vejamos", "vejam", "visse", "visses", "vissem", "ver",
        "leio", "lês", "lê", "leem", "li", "leste", "leu", "leram", "lia", "lias", "liam", "leia", "leias", "leiam", "lesse", "ler",
        "ouço", "ouça", "ouças", "ouçam",
    };
    private static readonly string Participles = @"(?:entendid|percebid|compreendid|ouvid|lid|lembrad|recordad|escrit|feit|" +
        @"calculad|interpretad|escolhid|julgad|pronunciad|soletrad|adivinhad|medid|avaliad|vist|dit|contad|estimad|traduzid|copiad|" +
        @"apostad|jogad|explicad|montad|programad|configurad|marcad|somad|pesad|tocad|cantad|conduzid|dirigid|agid|lidad|respondid)o";
    private static readonly Regex WrongManner = new(
        DialogueBefore + @"(?<!(?<![\p{L}\p{M}])(?:o|a|os|as|um|uma|uns|umas|no|na|nos|nas|do|da|dos|das|pelo|pela|ao|à|num|numa|dum|duma|" +
        @"meu|minha|teu|tua|seu|sua|nosso|nossa|este|esta|esse|essa|aquele|aquela|qualquer|outro|outra|mesmo|mesma|cada|primeiro|último)\s+)" +
        @"(?<v>(?:" + Alternatives(WrongMannerVerbs.SelectMany(FinitePatternForms).Concat(IrregularMannerForms)) + @")(?:-(?:me|te|o|a|nos|vos|os|as|lo|la|los|las|lhe|lhes))?|" +
        @"(?:tenho|tens|tem|temos|têm|tinha|tinhas|tínhamos|tinham|tenha|tenhas|tenhamos|tenham|tivesse|tivesses|tivéssemos|tivessem|" +
        @"teria|terias|teríamos|teriam|ter|havia|haviam)\s+" + Participles + @")" +
        @"(?<mid>\s+(?:(?:tudo|isso|isto|aquilo|sempre|completamente|totalmente)\s+)?)(?<w>errado)" + After +
        @"(?!\s+(?:de|da|do)\s)", Ci);
    private static readonly Dictionary<string, string> GoWrongForms = new(StringComparer.OrdinalIgnoreCase)
    {
        ["dá"] = "corre", ["dão"] = "correm", ["deu"] = "correu", ["deram"] = "correram", ["dava"] = "corria", ["davam"] = "corriam",
        ["der"] = "correr", ["derem"] = "correrem", ["desse"] = "corresse", ["dessem"] = "corressem", ["daria"] = "correria",
        ["dariam"] = "correriam", ["dará"] = "correrá", ["darão"] = "correrão", ["dê"] = "corra", ["deem"] = "corram",
        ["dêem"] = "corram", ["dar"] = "correr",
    };
    private static readonly Regex GoWrong = new(
        DialogueBefore + @"(?<v>" + Alternatives(GoWrongForms.Keys) + @")(?<mid>\s+(?:(?:tudo|sempre|completamente|totalmente)\s+)?)(?<w>errado)" + After +
        @"(?!\s+(?:o|a|os|as|um|uma|uns|umas|de|da|do|ao|à|para|pra)\s)", Ci);

    private static IEnumerable<string> FinitePatternForms(string infinitive) =>
        VerbConjugator.TryConjugate(infinitive, out var forms)
            ? forms.Where((_, index) => index is not (34 or 35 or 36 or 37 or 38))
            : Array.Empty<string>();

    private static string WrongAsAdverb(string text)
    {
        if (text.IndexOf("errado", StringComparison.OrdinalIgnoreCase) < 0) return text;
        text = GoWrong.Replace(text, m =>
        {
            var v = m.Groups["v"].Value;
            if (!GoWrongForms.TryGetValue(v, out var target)) return m.Value;
            return CaseHelper.MatchCase(v, target) + m.Groups["mid"].Value + CaseHelper.MatchCase(m.Groups["w"].Value, "mal");
        });
        return WrongManner.Replace(text, m =>
            m.Groups["v"].Value + m.Groups["mid"].Value + CaseHelper.MatchCase(m.Groups["w"].Value, "mal"));
    }

    // "see for yourself" is "ver por ti mesmo/próprio": "Vê tu mesmo" → "Vê por ti mesmo", "Para veres tu mesmo" →
    // "Para veres por ti mesmo"; formal "você mesmo" → "por si mesmo", plural "vocês mesmos" → "por vocês mesmos".
    private static readonly Regex SeeForYourself = new(
        DialogueBefore + @"(?<v>" + Alternatives(new[] { "verificar", "confirmar", "comprovar", "constatar", "descobrir", "julgar", "avaliar" }
            .SelectMany(FinitePatternForms).Concat(new[]
            {
                "ver", "veres", "vermos", "verem", "vê", "vês", "veja", "vejas", "vejamos", "vejam", "vi", "viste", "viu", "vimos",
                "viram", "verás", "verá", "verão", "verias", "veria",
            })) +
        @")(?<gap>\s+)(?<who>tu|você|vocês|vós)(?<rest>\s+(?:mesm[oa]s?|própri[oa]s?))" + After, Ci);

    public static string SeeForYourselfInText(string text)
    {
        if (text.IndexOf("mesm", StringComparison.OrdinalIgnoreCase) < 0 && text.IndexOf("própri", StringComparison.OrdinalIgnoreCase) < 0) return text;
        return SeeForYourself.Replace(text, m =>
        {
            var who = m.Groups["who"].Value.ToLowerInvariant() switch { "tu" => "ti", "você" => "si", var other => other };
            return m.Groups["v"].Value + m.Groups["gap"].Value + CaseHelper.MatchCase(m.Groups["who"].Value, "por") + " " +
                   (CaseHelper.IsAllUpper(m.Groups["who"].Value) ? who.ToUpperInvariant() : who) + m.Groups["rest"].Value;
        });
    }

    // "fazer vocês esperarem" → "fazer-vos esperar": the causee of fazer/deixar/mandar is an object pronoun before a plain infinitive.
    private static readonly Regex CausativeYou = new(
        DialogueBefore + @"(?<v>" + Alternatives(new[] { "deixar", "mandar" }.SelectMany(FinitePatternForms).Concat(new[]
        {
            "fazer", "faz", "fazes", "fazem", "fiz", "fez", "fizeram", "fazia", "faziam", "faça", "façam", "fizer", "fizesse", "farei",
            "fará", "faria",
        })) + @")(?<gap>\s+)vocês\s+(?<inf>[\p{L}\p{M}]*(?:arem|erem|irem|orem|ôrem))" + After, Ci);
    private static readonly HashSet<string> CausativeAttractors = new(StringComparer.OrdinalIgnoreCase)
    {
        "não", "nunca", "já", "que", "quem", "se", "quando", "porque", "também", "só", "jamais", "nem", "talvez",
    };

    // An apology to a group is plural: "Desculpe por vos fazer esperar" → "Desculpem por vos fazer esperar".
    private static readonly Regex GroupApology = new(
        @"(?<pre>(?:^|[.!?…]\s+|\n(?=[-–—])|\n(?![\p{Ll}]))[\s""“«']*(?:[-–—]\s*)?)(?<v>desculpe|desculpa|perdoe|perdoa)(?![\p{L}\p{M}-])" +
        @"(?=(?:[^.!?…\n]|\n(?![-–—]))*(?<![\p{L}\p{M}])(?:vos|vocês|vosso|vossa|vossos|vossas)(?![\p{L}\p{M}])|" +
        @"\s*,\s*(?:pessoal|malta|rapazes|raparigas|meninos|meninas|miúdos|senhores|senhoras|todos|gente|amigos|amigas)(?![\p{L}\p{M}])|" +
        @"\s+a\s+todos(?![\p{L}\p{M}]))", Ci);

    public static string GroupApologyPlural(string text) =>
        GroupApology.Replace(text, m => m.Groups["pre"].Value + CaseHelper.MatchCase(m.Groups["v"].Value,
            m.Groups["v"].Value.StartsWith("d", StringComparison.OrdinalIgnoreCase) ? "desculpem" : "perdoem"));

    private static readonly Regex ArrivalApology = new(
        @"(?<head>(?:^|\n)[\s\uE000-\uF8FF-]*(?:(?:ei|olá|oi|boa\s+noite|bom\s+dia|boa\s+tarde)[!,.]\s*)?)" +
        @"(?<verb>desculpa|desculpe|perdoa|perdoe)(?<end>[!.\uE000-\uF8FF\s]*$)", Ci);
    public static string ArrivalGroupApology(string text) => ArrivalApology.Replace(text, m =>
        m.Groups["head"].Value + CaseHelper.MatchCase(m.Groups["verb"].Value,
            m.Groups["verb"].Value.StartsWith("d", StringComparison.OrdinalIgnoreCase) ? "desculpem" : "perdoem") +
        m.Groups["end"].Value);

    public static string CausativeYouPlural(string text)
    {
        if (text.IndexOf("vocês", StringComparison.OrdinalIgnoreCase) < 0) return text;
        return CausativeYou.Replace(text, m =>
        {
            var inf = m.Groups["inf"].Value;
            var plain = inf[..^2];
            if (!PortugueseVerbs.IsKnownInfinitive(plain.ToLowerInvariant()) && !PortugueseVerbs.IsKnownVerbForm(plain) &&
                !(plain.Length > 4 && VerbConjugator.TryConjugate(plain.ToLowerInvariant(), out _)) &&
                plain.ToLowerInvariant() is not ("ir" or "vir" or "ver" or "ser" or "ter" or "dar" or "ler" or "rir" or "pôr" or "sair" or "cair"))
                return m.Value;
            var verb = m.Groups["v"].Value;
            var previous = Regex.Match(text[..m.Index], @"([\p{L}\p{M}]+)\s+$");
            var vos = CaseHelper.IsAllUpper(verb) ? "VOS" : "vos";
            var head = previous.Success && CausativeAttractors.Contains(previous.Groups[1].Value)
                ? vos + " " + verb
                : PortugueseVerbs.AttachMesoclitic(verb, vos, false) ?? verb + "-" + vos;
            return head + m.Groups["gap"].Value + plain;
        });
    }

    private static readonly Dictionary<string, string> WealthToFortune = new(StringComparer.OrdinalIgnoreCase)
    {
        ["sou"] = "tenho", ["és"] = "tens", ["é"] = "tem", ["somos"] = "temos", ["são"] = "têm",
        ["era"] = "tinha", ["eras"] = "tinhas", ["éramos"] = "tínhamos", ["eram"] = "tinham",
        ["fui"] = "tive", ["foste"] = "tiveste", ["foi"] = "teve", ["fomos"] = "tivemos", ["foram"] = "tiveram",
        ["serei"] = "terei", ["serás"] = "terás", ["será"] = "terá", ["seremos"] = "teremos", ["serão"] = "terão",
        ["seria"] = "teria", ["serias"] = "terias", ["seríamos"] = "teríamos", ["seriam"] = "teriam",
        ["seja"] = "tenha", ["sejas"] = "tenhas", ["sejamos"] = "tenhamos", ["sejam"] = "tenham",
        ["fosse"] = "tivesse", ["fosses"] = "tivesses", ["fôssemos"] = "tivéssemos", ["fossem"] = "tivessem",
        ["for"] = "tiver", ["fores"] = "tiveres", ["formos"] = "tivermos", ["forem"] = "tiverem",
        ["ser"] = "ter", ["seres"] = "teres", ["sermos"] = "termos", ["serem"] = "terem", ["sendo"] = "tendo",
    };
    private static readonly Regex RichInPeople = new(
        Before + @"(?<v>" + Alternatives(WealthToFortune.Keys) + @")(?<gap>\s+)ric[oa]s?\s+(?:de|em)\s+" +
        @"(?<people>(?:(?:os|as|meus|minhas|teus|tuas|seus|suas|nossos|nossas|bons|boas|grandes|verdadeiros|verdadeiras)\s+){0,3}" +
        @"(?:parceir[oa]s|amig[oa]s|companheir[oa]s|colegas|aliad[oa]s|família))" + After, Ci);
    private static readonly Regex RichInPeopleReference = new(
        @"\brich\s+(?:in|with)\s+(?:(?:my|your|his|her|our|their|good|great|close|true|loyal)\s+){0,3}" +
        @"(?:partners|friends|companions|colleagues|allies|family)\b" +
        @"(?!['’]|\s+(?:money|wealth|fortune|income|profits|assets)\b)", Ci);
    private static readonly Regex LifePageantReference = new(
        @"\blife['’]s\s+rich\s+(?:pageant|tapestry)\b", Ci);
    private static readonly Regex LifePageant = new(
        Before + @"(?<article>(?:o|a|do|da|no|na|ao|à|pelo|pela)\s+)?(?<phrase>ric[oa]\s+(?:trama|tapeçaria|desfile|cortejo|espetáculo)\s+da\s+vida)" +
        After, Ci);

    private static string LifePageantVocabulary(string text, ReferenceHint? reference)
    {
        if (!LifePageantReference.IsMatch(reference?.EnglishText ?? "")) return text;
        return LifePageant.Replace(text, m =>
        {
            var phrase = m.Groups["phrase"].Value;
            var before = m.Result("$`");
            if (Regex.IsMatch(before, @"\b(?:um|uma|este|esta|esse|essa|aquele|aquela|todo|toda)\s+$", Ci))
                return m.Value;
            var article = m.Groups["article"].Value.Trim().ToLowerInvariant() switch
            {
                "do" or "da" => "do",
                "no" or "na" => "no",
                "ao" or "à" => "ao",
                "pelo" or "pela" => "pelo",
                _ => "o",
            };
            var target = article + " rico espetáculo da vida";
            var original = m.Groups["article"].Success ? m.Value : phrase;
            return CaseHelper.MatchCase(original, target);
        });
    }

    private static readonly Dictionary<string, string> FishingForms = MapForms("jogar", "pescar");
    private static readonly Dictionary<string, string> BaitForms = MapForms("pegar", "morder");
    private static readonly Dictionary<string, string> AddUpForms = MapForms("somar", "bater");
    private const string IdiomEnd = @"(?=\s*(?:[,!.?…;:]|$|[\uE000-\uF8FF]))";
    private const string GreenPhrase = @"\s+(?:um\s+)?verde(?:\s+(?:para|pra)\s+colher\s+maduro)?" + After + IdiomEnd;
    private static readonly Regex ProgressiveFishing = new(
        DialogueBefore + @"(?<aux>estou|estás|está|estamos|estão|estava|estavas|estávamos|estavam|" +
        @"estive|estiveste|esteve|estivemos|estiveram|estarei|estarás|estará|estaremos|estarão|" +
        @"estaria|estarias|estaríamos|estariam|esteja|estejas|estejamos|estejam|estivesse|estivesses|" +
        @"estivéssemos|estivessem|estar)(?<gap>\s+)(?:jogando|a\s+jogar)" + GreenPhrase, Ci);
    private static readonly Regex Fishing = new(
        DialogueBefore + @"(?<v>" + Alternatives(FishingForms.Keys) + @")" + GreenPhrase, Ci);
    private static readonly Regex TakeBait = new(
        DialogueBefore + @"(?<v>" + Alternatives(BaitForms.Keys) + @")(?<gap>\s+)(?:a|na|o|no)\s+(?:isca|isco)" +
        After + IdiomEnd, Ci);
    private static readonly Regex CoffeeJoe = new(
        DialogueBefore + @"(?<cup>xícara|chávena)(?<gap>\s+)de\s+Joe" + After +
        @"(?<takeaway>(?<takeawayGap>\s+)(?:para|pra)\s+viagem)?", Ci);
    private static readonly Regex NotAddingUp = new(
        DialogueBefore + @"(?<neg>não)(?<gap>\s+)(?<v>" + Alternatives(AddUpForms.Keys) + @")" + After + IdiomEnd, Ci);
    private static readonly Regex FishermenLiteral = new(DialogueBefore + @"jogadores" + After + IdiomEnd, Ci);

    private static string FishingAndCalques(string text, ReferenceHint? reference, Func<string, bool> blocked)
    {
        var english = ReferenceAligner.Clean(reference?.EnglishText ?? "").Replace('’', '\'');
        if (english.Length == 0) return text;
        string Replace(Regex pattern, Func<Match, string> replacement, params string[] vocabulary) =>
            pattern.Replace(text, m => Regex.IsMatch(m.Value, @"\r?\n[ \t]*\r?\n") ||
                vocabulary.Any(blocked) || blocked(m.Value) ||
                Regex.Matches(m.Value, @"[\p{L}\p{M}]+").Any(w => blocked(w.Value))
                    ? m.Value : CaseHelper.MatchCase(m.Value, replacement(m)));

        if (Regex.IsMatch(english, @"\b(?:fishing|fished|fish(?:es)?)\b", Ci) &&
            !Regex.IsMatch(english, IdiomHints.LiteralFishingEnglish, Ci))
        {
            text = Replace(ProgressiveFishing, m => m.Groups["aux"].Value + m.Groups["gap"].Value + "à pesca", "jogar");
            text = Replace(Fishing, m => FishingForms[m.Groups["v"].Value], "jogar");
        }
        if (Regex.IsMatch(english, @"\b(?:take|takes|taking|took|taken)\s+(?:the|chump)\s+bait\b", Ci) &&
            !Regex.IsMatch(english, @"\b(?:fish|fishes|trout|salmon|tuna|hook|hooks|worm|worms|rod|tackle)\b", Ci))
            text = Replace(TakeBait, m => BaitForms[m.Groups["v"].Value] + m.Groups["gap"].Value + "o isco", "pegar", "isca", "isco");
        if (Regex.Matches(english, @"\bfishermen\b", Ci).Count == 1 && FishermenLiteral.Matches(text).Count == 1 &&
            !Regex.IsMatch(english, @"\bplayers\b", Ci))
            text = Replace(FishermenLiteral, _ => "pescadores", "jogadores");
        if (Regex.IsMatch(english, @"\bcup\s+of\s+joe\b", Ci))
            text = Replace(CoffeeJoe, m => m.Groups["cup"].Value + m.Groups["gap"].Value + "de café" +
                (m.Groups["takeaway"].Success && Regex.IsMatch(english, @"\bto\s+go\b", Ci)
                    ? m.Groups["takeawayGap"].Value + "para levar" : m.Groups["takeaway"].Value), "Joe");
        if (Regex.IsMatch(english, IdiomHints.NotAddingUpEnglish, Ci))
            text = Replace(NotAddingUp, m => m.Groups["neg"].Value + m.Groups["gap"].Value +
                AddUpForms[m.Groups["v"].Value] + " certo", "somar");
        return text;
    }

    public static string Idioms(string text, ReferenceHint? reference, Func<string, bool> blocked)
    {
        text = FishingAndCalques(text, reference, blocked);
        text = LifePageantVocabulary(text, reference);
        text = WrongAsAdverb(text);
        text = ScoreDrugsInText(text, reference);
        text = SlowPeriod.Replace(text, m =>
        {
            var noun = m.Groups["noun"].Value;
            if (!Regex.IsMatch(reference?.EnglishText ?? "", @"\bslow\s+" +
                SlowPeriodReferences[noun] + @"\b", Ci)) return m.Value;
            var feminine = Regex.IsMatch(noun, @"^(?:manhãs?|tardes?|noites?|semanas?)$", Ci);
            var adjective = feminine ? "lenta" : "lento";
            if (noun.EndsWith('s')) adjective += "s";
            return noun + m.Groups["gap"].Value + CaseHelper.MatchCase(m.Groups["adverb"].Value, adjective);
        });
        text = ClarifyDoubt.Replace(text, m => CaseHelper.MatchCase(m.Groups["v"].Value, ClarifyDoubtForms[m.Groups["v"].Value]) +
            m.Groups["cl"].Value + m.Groups["space"].Value + m.Groups["det"].Value + m.Groups["noun"].Value);
        text = MaybeSlang.Replace(text, m => CaseHelper.MatchCase(m.Value,
            "se" + m.Groups["space"].Value + "calhar"));
        text = LetsGoExpression.Replace(text, m => m.Groups["head"].Value +
            CaseHelper.MatchCase(m.Groups["phrase"].Value, "vamos a isso"));
        var english = reference?.EnglishText;
        if (RichInPeopleReference.IsMatch(english ?? ""))
            text = RichInPeople.Replace(text, m => CaseHelper.MatchCase(m.Groups["v"].Value,
                WealthToFortune[m.Groups["v"].Value]) + m.Groups["gap"].Value +
                (CaseHelper.IsAllUpper(m.Value) ? "SORTE COM " : "sorte com ") + m.Groups["people"].Value);
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
