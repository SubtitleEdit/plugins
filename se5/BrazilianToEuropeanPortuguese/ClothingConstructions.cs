using System.Text.RegularExpressions;

namespace SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;

internal static class ClothingConstructions
{
    private const RegexOptions Ci = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;
    private const string Before = @"(?<![\p{L}\p{M}-])";
    private const string After = @"(?![\p{L}\p{M}-])";
    private const string Clothes = @"(?:jeans|calças|cuecas|cueca|calcinha|calcinhas|camisa|camisola|casaco|saia|vestido|sapatos|botas|meias|soutien|sutiã)";
    private static readonly (Regex English, Regex Portuguese, string Singular, string Plural, char Gender)[] SportswearSenses =
    {
        (new(@"\bleotards?\b", Ci),
            PortugueseConverter.CreateWordRegex(new[] { "fantasia", "fantasias", "collant", "collants", "colant", "colants" }),
            "maillot", "maillots", 'm'),
        (new(@"\btracksuits?\b", Ci), PortugueseConverter.CreateWordRegex(new[] { "agasalho", "agasalhos" }),
            "fato de treino", "fatos de treino", 'm'),
        (new(@"\bwet\s?suits?\b", Ci), PortugueseConverter.CreateWordRegex(new[] { "macacão", "macacões" }),
            "fato de mergulho", "fatos de mergulho", 'm'),
        (new(@"\b(?:golf|polo)\s+shirts?\b", Ci), PortugueseConverter.CreateWordRegex(new[] { "camisa", "camisas", "camiseta", "camisetas", "t-shirt", "t-shirts" }),
            "polo", "polos", 'm'),
        (new(@"\b(?:cycling|bicycle)\s+jerseys?\b", Ci),
            PortugueseConverter.CreateWordRegex(new[] { "camisa", "camisas", "camiseta", "camisetas", "t-shirt", "t-shirts" }),
            "camisola de ciclismo", "camisolas de ciclismo", 'f'),
        (new(@"\b(?:riding|equestrian)\s+breeches\b|\bjodhpurs\b", Ci),
            PortugueseConverter.CreateWordRegex(new[] { "culotes" }), "calças de equitação", "calças de equitação", 'f'),
    };
    private static readonly Regex EnglishGarments = new(
        @"\b(?:leotards?|tracksuits?|wet\s?suits?|shirts?|jerseys?|breeches|jodhpurs|costumes?|dresses|" +
        @"tights|trousers|pants|shorts|skirts?|coats?|jackets?|sweaters?|uniforms?|swimsuits?|bikinis?|bras?|boots?|shoes?)\b", Ci);

    internal static string Sportswear(string text, ReferenceHint? reference, Func<string, bool> allow)
    {
        var english = reference?.EnglishText;
        if (english == null) return text;
        var senses = SportswearSenses.Where(s => s.English.IsMatch(english)).ToArray();
        if (senses.Length != 1 || senses[0].English.Matches(english).Count != 1 ||
            EnglishGarments.Matches(english).Count != 1) return text;
        var sense = senses[0];
        var matches = sense.Portuguese.Matches(text);
        if (matches.Count != 1) return text;
        return sense.Portuguese.Replace(text, m =>
        {
            var source = m.Groups["key"].Value;
            if (!allow(source)) return m.Value;
            var plural = source.EndsWith('s') || source.EndsWith('S');
            if (plural != sense.English.Match(english).Value.EndsWith("s", StringComparison.OrdinalIgnoreCase)) return m.Value;
            var target = plural ? sense.Plural : sense.Singular;
            return PortugueseConverter.ReplaceWordEntry(m,
                new PortugueseConverter.WordEntry(source, target, false, sense.Gender, null, null));
        });
    }

    private static readonly Dictionary<string, string> ThrowForms = BuildThrowForms();
    private static readonly Regex ThrowPerson = new(
        Before + @"(?:(?<precl>o|a|os|as)\s+)?(?<v>" + string.Join("|", ThrowForms.Keys.OrderByDescending(w => w.Length)) + @")" +
        @"(?:-(?<enc>o|a|os|as))?(?<space>\s+)(?:(?<person>ele|ela|eles|elas|você|vocês)\s+)?" +
        @"(?:no|ao|para\s+o)\s+chão" + After, Ci);
    private static readonly Regex RemoveClothes = new(
        Before + @"(?<v>tirou|tiraram|tirei|tiraste|tirámos|tirava|tiravam|arrancou|arrancaram|arranquei|arrancaste|arrancámos|arrancava|arrancavam)" +
        @"\s+(?:(?<det>o|a|os|as)\s+)?(?:(?<poss>seu|sua|seus|suas|meu|minha|meus|minhas|teu|tua|teus|tuas|nosso|nossa|nossos|nossas)\s+)?" +
        @"(?<noun>" + Clothes + @")(?<owner>\s+(?:dele|dela|deles|delas))?" + After, Ci);
    private static readonly Regex Underwear = new(
        Before + @"(?<prefix>(?:(?:a|uma|da|na|à|pela|essa|esta|aquela|minha|tua|sua|nossa|vossa|nova|velha|outra|própria)\s+){0,4})" +
        @"(?<noun>cueca|calcinha|meia-calça)(?!\s+boxer\b)" + After +
        @"(?<copula>\s+(?:está|estava|é|era|ficou))?(?<adj>(?:\s+(?:nova|velha|limpa|suja|branca|preta|azul|vermelha|amarela|verde|cinzenta|bege|roxa|transparente|curta|comprida|fina|grossa|rasgada|molhada|seca|pequena|grande)){0,2})", Ci);
    private static readonly Dictionary<string, string> Plurals = new(StringComparer.OrdinalIgnoreCase)
    {
        ["a"] = "as", ["uma"] = "umas", ["da"] = "das", ["na"] = "nas", ["à"] = "às", ["pela"] = "pelas",
        ["essa"] = "essas", ["esta"] = "estas", ["aquela"] = "aquelas", ["minha"] = "minhas", ["tua"] = "tuas",
        ["sua"] = "suas", ["nossa"] = "nossas", ["vossa"] = "vossas", ["nova"] = "novas", ["velha"] = "velhas",
        ["outra"] = "outras", ["própria"] = "próprias",
    };

    private static Dictionary<string, string> BuildThrowForms()
    {
        if (!VerbConjugator.TryConjugate("jogar", out var source) || !VerbConjugator.TryConjugate("mandar", out var target))
            throw new InvalidOperationException("Cannot conjugate person-to-ground construction.");
        return source.Select((word, i) => (word, target: target[i])).GroupBy(pair => pair.word)
            .ToDictionary(g => g.Key, g => g.First().target, StringComparer.OrdinalIgnoreCase);
    }

    internal static string PeopleAndClothes(string text, ReferenceHint? reference, bool vocabulary, bool pronouns)
    {
        var english = reference?.EnglishText ?? "";
        if (vocabulary)
            text = ThrowPerson.Replace(text, m =>
            {
                var person = m.Groups["person"].Value;
                var clitic = m.Groups["precl"].Success ? m.Groups["precl"].Value : m.Groups["enc"].Value;
                if (person.Length == 0 && clitic.Length == 0) return m.Value;
                if (person.Length == 0 && !Regex.IsMatch(english, @"\b(?:him|her|them|you)\b[^.!?]{0,45}\b(?:ground|floor)\b", Ci))
                    return m.Value;
                var verb = CaseHelper.MatchCase(m.Groups["v"].Value, ThrowForms[m.Groups["v"].Value]);
                return clitic.Length > 0
                    ? (m.Groups["precl"].Success ? clitic + " " + verb : PortugueseVerbs.AttachEnclitic(verb, clitic)) + " para o chão"
                    : verb + m.Groups["space"].Value + person + " para o chão";
            });
        if (!pronouns) return text;
        var englishOwner = Regex.IsMatch(english, @"\b(?:her|his)\s+(?:jeans|pants|trousers|underwear|panties|shirt|skirt|dress|clothes)\b", Ci);
        text = RemoveClothes.Replace(text, m =>
        {
            var poss = m.Groups["poss"].Value.ToLowerInvariant();
            var owner = m.Groups["owner"].Value.Trim().ToLowerInvariant();
            var clitic = owner switch
            {
                "dele" or "dela" => "lhe", "deles" or "delas" => "lhes",
                _ => poss switch
                {
                    "meu" or "minha" or "meus" or "minhas" => "me",
                    "teu" or "tua" or "teus" or "tuas" => "te",
                    "nosso" or "nossa" or "nossos" or "nossas" => "nos",
                    "seu" or "sua" or "seus" or "suas" when englishOwner => "lhe",
                    "" when englishOwner && Regex.IsMatch(text[..m.Index],
                        @"\b(?:tirou|arrancou)\s+(?:(?:o|a|os|as)\s+)?(?:seu|sua|seus|suas)\s+" + Clothes +
                        @"\s+e\s*$", Ci) => "lhe",
                    _ => "",
                },
            };
            if (clitic.Length == 0) return m.Value;
            var noun = m.Groups["noun"].Value;
            var feminine = Regex.IsMatch(noun, @"^(?:calças|cuecas?|calcinhas?|camisa|camisola|saia|botas|meias)$", Ci);
            var article = (feminine ? "a" : "o") + (noun.EndsWith('s') ? "s" : "");
            var verb = m.Groups["v"].Value;
            var before = text[..m.Index];
            var attracted = Regex.IsMatch(before, @"\b(?:não|nunca|já|que|sempre)\s*$", Ci);
            var result = attracted ? clitic + " " + verb : PortugueseVerbs.AttachEnclitic(verb, clitic);
            return CaseHelper.MatchCase(verb, result) + " " + article + " " + noun;
        });
        return Regex.Replace(text, @"(?<ground>\bpara\s+o\s+chão)\s+(?=tirou-lhe\b|arrancou-lhe\b)", "${ground}, ", Ci);
    }

    internal static string PluralUnderwear(string text) => Underwear.Replace(text, m =>
    {
        var tights = m.Groups["noun"].Value.Equals("meia-calça", StringComparison.OrdinalIgnoreCase);
        var prefix = Regex.Replace(m.Groups["prefix"].Value, @"\p{L}+", word =>
        {
            if (!Plurals.TryGetValue(word.Value, out var plural)) return word.Value;
            if (tights)
                plural = plural switch
                {
                    "as" => "os", "umas" => "uns", "das" => "dos", "nas" => "nos", "às" => "aos", "pelas" => "pelos",
                    "essas" => "esses", "estas" => "estes", "aquelas" => "aqueles", "minhas" => "meus", "tuas" => "teus",
                    "suas" => "seus", "nossas" => "nossos", "vossas" => "vossos",
                    _ => plural[..^2] + "os",
                };
            return CaseHelper.MatchCase(word.Value, plural);
        }, Ci);
        var adjectives = Regex.Replace(m.Groups["adj"].Value, @"\p{L}+", word =>
        {
            var adjective = word.Value.ToLowerInvariant();
            var plural = adjective == "azul" ? "azuis" : tights && adjective.EndsWith('a') ? adjective[..^1] + "os" : adjective + "s";
            return CaseHelper.MatchCase(word.Value, plural);
        }, Ci);
        var copula = m.Groups["copula"].Value;
        if (copula.Length > 0)
            copula = Regex.Replace(copula, @"\p{L}+", word => CaseHelper.MatchCase(word.Value,
                word.Value.ToLowerInvariant() switch { "está" => "estão", "estava" => "estavam", "é" => "são", "era" => "eram", _ => "ficaram" }), Ci);
        return prefix + CaseHelper.MatchCase(m.Groups["noun"].Value, tights ? "collants" : "cuecas") + copula + adjectives;
    });
}
