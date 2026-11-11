using System.Text.RegularExpressions;

namespace SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;

internal static class ShakingVocabulary
{
    private const RegexOptions Ci = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;
    private static readonly string[] Source = Paradigm("chacoalhar");
    private static readonly Dictionary<string, string[]> Targets = new(StringComparer.Ordinal)
    {
        ["abanar"] = Paradigm("abanar"),
        ["agitar"] = Paradigm("agitar"),
        ["chocalhar"] = Paradigm("chocalhar"),
        ["sacudir"] = Sacudir(),
        ["insultar"] = Paradigm("insultar"),
        ["repreender"] = Paradigm("repreender"),
        ["incomodar"] = Paradigm("incomodar"),
        ["abalar"] = Paradigm("abalar"),
    };
    private static readonly Regex Verb = new(
        @"(?<![\p{L}\p{M}\d_-])(?<verb>" +
        string.Join("|", Source.Append("chacoalhá").Distinct().OrderByDescending(s => s.Length).Select(Regex.Escape)) +
        @")(?<clitic>-(?:me|te|se|nos|vos|lhe|lhes|lo|la|los|las|o|a|os|as))?" +
        @"(?<tail>-(?:ei|ás|á|emos|eis|ão|ia|ias|íamos|íeis|iam))?(?![\p{L}\p{M}\d_-])", Ci);
    private static readonly Regex Complement = new(
        @"^\s+(?:(?:bem|muito|levemente|ligeiramente|vigorosamente|violentamente|sempre|ainda)\s+){0,2}" +
        @"(?:(?:o|a|os|as|um|uma|uns|umas|este|esta|estes|estas|esse|essa|esses|essas|" +
        @"meu|minha|meus|minhas|teu|tua|teus|tuas|seu|sua|seus|suas)\s+){0,2}(?<noun>[\p{L}\p{M}]+)", Ci);
    private static readonly Regex Liquid = new(
        @"^(?:garrafas?|frascos?|recipientes?|líquidos?|remédios?|medicamentos?|xaropes?|sumos?|sucos?|leite|misturas?|soluções?)$", Ci);
    private static readonly Regex DustOrClothes = new(
        @"^(?:poeira|pó|casacos?|tapetes?|roupas?|mantas?|cobertores?|lençóis|lençol|toalhas?|braços?|pernas?|mãos?|corpo)$", Ci);
    private static readonly Regex RattlingObjects = new(
        @"^(?:chaves?|moedas?|guizos?|sinos?|chocalhos?|rolamentos?)$", Ci);
    private static readonly Regex PassiveSubject = new(
        @"(?<noun>[\p{L}\p{M}]+)\s+(?:foi|foram|era|eram|está|estão|estava|estavam|ficou|ficaram|tem\s+sido|tinha\s+sido)" +
        @"\s+(?:(?:bem|muito|demasiado|vigorosamente)\s+)?$", Ci);

    private static string[] Paradigm(string infinitive)
    {
        if (!VerbConjugator.TryConjugate(infinitive, out var forms))
            throw new InvalidOperationException($"Cannot conjugate reviewed shaking vocabulary: {infinitive}.");
        var stem = infinitive[..^2];
        var extra = infinitive.EndsWith("ar", StringComparison.Ordinal)
            ? new[] { "ais", "astes", "áveis", "areis", "aríeis", "eis", "ásseis", "ardes", "áreis", "ai" }
            : infinitive.EndsWith("er", StringComparison.Ordinal)
                ? new[] { "eis", "estes", "íeis", "ereis", "eríeis", "ais", "êsseis", "erdes", "êreis", "ei" }
                : new[] { "is", "istes", "íeis", "ireis", "iríeis", "ais", "ísseis", "irdes", "íreis", "i" };
        return forms.Concat(extra.Select(ending => stem + ending)).ToArray();
    }

    private static string[] Sacudir()
    {
        var forms = Paradigm("sacudir");
        forms[2] = "sacodes";
        forms[3] = "sacode";
        forms[5] = "sacodem";
        return forms;
    }

    internal static string Apply(string text, ReferenceHint? reference, Func<string, bool> isBlocked) =>
        Verb.Replace(text, m =>
        {
            var source = m.Groups["verb"].Value;
            if (isBlocked("chacoalhar") || isBlocked(source)) return m.Value;
            var cliticInfinitive = source.Equals("chacoalhá", StringComparison.OrdinalIgnoreCase);
            if (m.Groups["tail"].Success && !m.Groups["clitic"].Success ||
                cliticInfinitive && !Regex.IsMatch(m.Groups["clitic"].Value, @"^-l[oa]s?$", Ci)) return m.Value;
            var before = m.Result("$`");
            if (PortugueseConverter.HasNominalDeterminerBefore(before) ||
                Regex.IsMatch(before, @"\b(?:duas|três|quatro|cinco|seis|algumas|várias|muitas|\d+)\s+$", Ci))
            {
                if (source.Equals("chacoalho", StringComparison.OrdinalIgnoreCase)) return m.Value;
                if (Regex.IsMatch(source, @"^chacoalhadas?$", Ci))
                    return CaseHelper.MatchCase(source, source.EndsWith("s", StringComparison.OrdinalIgnoreCase)
                        ? "sacudidelas" : "sacudidela");
            }
            var english = reference?.EnglishText ?? "";
            var after = m.Result("$'");
            var noun = Complement.Match(after).Groups["noun"].Value;
            if (Regex.IsMatch(source, @"^chacoalhad[oa]s?$", Ci))
            {
                var subject = PassiveSubject.Match(before);
                if (subject.Success) noun = subject.Groups["noun"].Value;
            }
            var target = Liquid.IsMatch(noun) ? "agitar" :
                DustOrClothes.IsMatch(noun) ? "sacudir" :
                RattlingObjects.IsMatch(noun) ? "chocalhar" :
                Regex.IsMatch(english, @"\b(?:insult(?:s|ed|ing)?|abus(?:e|es|ed|ing))\b", Ci) ? "insultar" :
                Regex.IsMatch(english, @"\b(?:scold(?:s|ed|ing)?|tell(?:ing)?\s+\w+\s+off|told\s+\w+\s+off)\b", Ci) ? "repreender" :
                Regex.IsMatch(english, @"\b(?:bother(?:s|ed|ing)?|annoy(?:s|ed|ing)?|pester(?:s|ed|ing)?)\b", Ci) ? "incomodar" :
                Regex.IsMatch(english, @"\b(?:(?:shaken|rocked)\s+(?:by\s+(?:(?:the|his|her|their|our|my)\s+)?" +
                    @"(?:news|events?|ordeal|experience|death|what)|beyond|more)|" +
                    @"(?:shaken|shook)\s+up|rattled)\b", Ci) ? "abalar" :
                "abanar";
            var key = cliticInfinitive ? "chacoalhar" : source;
            var candidates = Source.Select((form, index) => (form, index))
                .Where(item => item.form.Equals(key, StringComparison.OrdinalIgnoreCase)).Select(item => item.index).ToArray();
            if (candidates.Length == 0) return m.Value;
            if (source.Equals("chacoalhamos", StringComparison.OrdinalIgnoreCase) &&
                (reference is { Past: true, Present: false } || Regex.IsMatch(english, @"\bshook\b", Ci)))
                candidates = [42];
            var outputs = candidates.Select(index => Targets[target][index]).Distinct().ToArray();
            string converted;
            if (outputs.Length == 1) converted = outputs[0];
            else if (reference is { Past: true, Present: false }) converted = Targets[target][candidates[^1]];
            else if (reference is { Present: true, Past: false }) converted = Targets[target][candidates[0]];
            else return m.Value;
            if (m.Groups["clitic"].Success)
            {
                var clitic = m.Groups["clitic"].Value[1..].ToLowerInvariant();
                if (cliticInfinitive) clitic = clitic[1..];
                converted = PortugueseVerbs.AttachEnclitic(converted, clitic);
            }
            return CaseHelper.MatchCase(source, converted + m.Groups["tail"].Value);
        });
}
