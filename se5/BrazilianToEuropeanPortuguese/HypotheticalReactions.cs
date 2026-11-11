using System.Text.RegularExpressions;

namespace SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;

internal static class HypotheticalReactions
{
    private const RegexOptions Ci = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;
    private const string Boundary = @"(?<![\p{L}\p{M}-])";
    private const string Reaction =
        @"(?<reaction>(?:(?<subject>eu|tu|ele|ela|nós|vocês|eles|elas)\s+)?" +
        @"(?:(?:também|não|nunca|certamente|provavelmente)\s+){0,2}" +
        @"(?<verb>ficaria|ficarias|ficaríamos|ficariam|estaria|estarias|estaríamos|estariam)" +
        @"\s+(?:(?:também|muito|tão|mesmo|bastante|igualmente|completamente)\s+){0,3}" +
        @"(?:zangad[oa]s?|furios[oa]s?|irritad[oa]s?|tristes?|abalad[oa]s?|contentes?|feliz|felizes|" +
        @"assustad[oa]s?|preocupad[oa]s?|ofendid[oa]s?|indignad[oa]s?|magoad[oa]s?|chatead[oa]s?|" +
        @"surpreendid[oa]s?|revoltad[oa]s?|brav[oa]s?))";
    private const string Condition =
        @"(?<condition>se\s+(?:(?<inner>eu|tu|ele|ela|nós|vocês|eles|elas)\s+)?" +
        @"(?<copula>fosse|fosses|fôssemos|fossem)\s+(?<other>ele|ela|eles|elas|tu|você|vocês))";
    private static readonly Regex Trailing = new(Boundary + Reaction + @"\s+" + Condition +
        @"(?=\s*[\uE000-\uF8FF]*(?:[.!?…;]|$))", Ci);
    private static readonly Regex Leading = new(Boundary + Condition + @"(?<comma>,\s+)" + Reaction +
        @"(?=\s*[\uE000-\uF8FF]*(?:[.!?…;]|$))", Ci);

    internal static string Apply(string text, ReferenceHint? reference)
    {
        string Replace(Match m)
        {
            if (Regex.IsMatch(m.Result("$`"), @"\b(?:como|mesmo|ainda|só)\s+$", Ci)) return m.Value;
            var subject = m.Groups["subject"].Value.ToLowerInvariant();
            var inner = m.Groups["inner"].Value.ToLowerInvariant();
            var verb = m.Groups["verb"].Value.ToLowerInvariant();
            if (subject.Length == 0)
                subject = inner.Length > 0 ? inner :
                    verb.EndsWith("ríamos", StringComparison.Ordinal) ? "nós" :
                    verb.EndsWith("rias", StringComparison.Ordinal) ? "tu" :
                    Regex.IsMatch(reference?.EnglishText ?? "", @"\bI(?:['’]d|\s+would)\s+(?:also\s+)?(?:be|go|get|feel)\b", Ci) ? "eu" : "";
            var expected = subject switch
            {
                "eu" or "ele" or "ela" => "fosse", "tu" => "fosses", "nós" => "fôssemos",
                "vocês" or "eles" or "elas" => "fossem", _ => "",
            };
            if (expected.Length == 0 || !m.Groups["copula"].Value.Equals(expected, StringComparison.OrdinalIgnoreCase) ||
                inner.Length > 0 && inner != subject || subject == m.Groups["other"].Value.ToLowerInvariant()) return m.Value;
            var expectedVerb = subject switch
            {
                "eu" or "ele" or "ela" => @"^(?:ficaria|estaria)$",
                "tu" => @"^(?:ficarias|estarias)$", "nós" => @"^(?:ficaríamos|estaríamos)$",
                _ => @"^(?:ficariam|estariam)$",
            };
            if (!Regex.IsMatch(verb, expectedVerb, Ci)) return m.Value;
            var complement = subject switch
            {
                "eu" => "comigo", "tu" => "contigo", "nós" => "connosco", _ => "com " + subject,
            };
            var condition = CaseHelper.MatchCase(m.Groups["condition"].Value, "se fosse " + complement);
            return m.Groups["comma"].Success
                ? condition + m.Groups["comma"].Value + m.Groups["reaction"].Value
                : m.Groups["reaction"].Value + " " + condition;
        }
        return Leading.Replace(Trailing.Replace(text, Replace), Replace);
    }
}
