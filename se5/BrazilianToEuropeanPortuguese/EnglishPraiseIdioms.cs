using System.Text.RegularExpressions;

namespace SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;

internal static class EnglishPraiseIdioms
{
    private const RegexOptions Ci = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;
    private static readonly Regex EnglishAuthenticity = new(
        @"\b(?<subject>he|she|they)(?:['’](?<contract>s|re)|\s+(?<verb>is|are|was|were))\s+" +
        @"(?:(?:really|truly|definitely)\s+)?the\s+(?<idiom>real\s+deal|genuine\s+article|real\s+McCoy)\b", Ci);
    private static readonly Regex PersonalLiteral = new(
        @"(?<![\p{L}\p{M}\d_-])(?:(?<subject>ele|ela|eles|elas)(?<subjectGap>\s+)(?<verb>é|são|era|eram)(?<verbGap>\s+))?" +
        @"(?<literal>(?:o\s+)?negócio\s+(?:real|verdadeiro)|a\s+coisa\s+(?:real|verdadeira)|" +
        @"o\s+artigo\s+(?:genuíno|verdadeiro)|verdadeir[oa]s?)" +
        @"(?![\p{L}\p{M}\d_-])(?=\s*(?:[,!.?…;:]|$|[\uE000-\uF8FF]))", Ci);
    private static readonly Regex EnglishExpertise = new(
        @"\b(?<subject>I|you|he|she|we|they|(?:this|that|these|those)\s+(?:guys?|men|women|girls?|boys?))\s+" +
        @"(?:(?:really|certainly|definitely|always|never|sure)\s+){0,2}(?:know|knows|knew)\s+" +
        @"(?:(?:really|certainly|definitely)\s+)?(?<own>my|your|his|her|our|their)\s+stuff\b" +
        @"(?!\s+(?:is|was|are|were|belongs|belonged)\b)", Ci);
    private static readonly Regex LiteralExpertise = new(
        @"(?<![\p{L}\p{M}\d_-])(?<verb>conheço|conheces|conhece|conhecemos|conhecem|" +
        @"conhecia|conhecias|conhecíamos|conheciam|sei|sabes|sabe|sabemos|sabem|sabia|sabias|sabíamos|sabiam)" +
        @"(?<gap>\s+)(?<adv>(?:(?:mesmo|realmente|bem)\s+){0,2})(?:as\s+)?" +
        @"(?:minhas|tuas|suas|nossas|vossas)\s+coisas(?![\p{L}\p{M}\d_-])" +
        @"(?=\s*(?:[,!.?…;:]|$|[\uE000-\uF8FF]))", Ci);
    private static readonly Dictionary<string, (string Know, string Do)> ExpertiseForms = new(StringComparer.OrdinalIgnoreCase)
    {
        ["conheço"] = ("sei", "faço"), ["sei"] = ("sei", "faço"),
        ["conheces"] = ("sabes", "fazes"), ["sabes"] = ("sabes", "fazes"),
        ["conhece"] = ("sabe", "faz"), ["sabe"] = ("sabe", "faz"),
        ["conhecemos"] = ("sabemos", "fazemos"), ["sabemos"] = ("sabemos", "fazemos"),
        ["conhecem"] = ("sabem", "fazem"), ["sabem"] = ("sabem", "fazem"),
        ["conhecia"] = ("sabia", "fazia"), ["sabia"] = ("sabia", "fazia"),
        ["conhecias"] = ("sabias", "fazias"), ["sabias"] = ("sabias", "fazias"),
        ["conhecíamos"] = ("sabíamos", "fazíamos"), ["sabíamos"] = ("sabíamos", "fazíamos"),
        ["conheciam"] = ("sabiam", "faziam"), ["sabiam"] = ("sabiam", "faziam"),
    };

    public static string Apply(string text, string? english, Func<string, bool> blocked)
    {
        if (string.IsNullOrWhiteSpace(english)) return text;
        var idioms = EnglishAuthenticity.Matches(english);
        var candidates = PersonalLiteral.Matches(text);
        if (idioms.Count == 1 && candidates.Count == 1)
        {
            var idiom = idioms[0];
            var candidate = candidates[0];
            var subject = candidate.Groups["subject"].Value.ToLowerInvariant();
            var enSubject = idiom.Groups["subject"].Value.ToLowerInvariant();
            var expected = enSubject switch { "he" => "ele", "she" => "ela", _ => subject };
            var literal = candidate.Groups["literal"].Value;
            var words = Regex.Matches(candidate.Value, @"[\p{L}\p{M}]+").Select(m => m.Value);
            var before = text[..candidate.Index];
            var omitted = subject.Length == 0;
            var allowedOmission = enSubject != "they" &&
                Regex.IsMatch(before, @"(?:^|[.!?;]\s*)[\s""“«'\uE000-\uF8FF-]*(?:sim\s*,\s*)?$", Ci);
            var past = idiom.Groups["verb"].Value.ToLowerInvariant() is "was" or "were";
            var ptPast = candidate.Groups["verb"].Value.ToLowerInvariant() is "era" or "eram";
            var authenticity = !idiom.Groups["idiom"].Value.StartsWith("real", StringComparison.OrdinalIgnoreCase) ||
                Regex.IsMatch(idiom.Groups["idiom"].Value, @"McCoy$", Ci);
            // Authenticity-only contexts must not turn a genuine person into praise of their skill.
            var authenticContext = Regex.IsMatch(english, @"\b(?:fake|impost[eo]r|counterfeit|fraud|pretend|genuine|authentic)\b", Ci);
            if ((subject == expected || omitted && allowedOmission) && (omitted || past == ptPast) &&
                (enSubject != "they" || subject is "eles" or "elas") &&
                !words.Any(blocked) && !blocked(literal) && !blocked("ser") &&
                !Regex.IsMatch(before, @"\b(?:não|nunca|nem)\s*$", Ci))
            {
                if (omitted) subject = expected;
                var plural = subject is "eles" or "elas";
                var feminine = subject is "ela" or "elas";
                var copula = past ? plural ? "eram" : "era" : plural ? "são" : "é";
                var adjective = authenticity || authenticContext
                    ? feminine ? plural ? "autênticas" : "autêntica" : plural ? "autênticos" : "autêntico"
                    : "mesmo " + (feminine ? plural ? "boas" : "boa" : plural ? "bons" : "bom");
                var replacement = omitted ? subject + " " + copula + " " + adjective
                    : candidate.Groups["subject"].Value + candidate.Groups["subjectGap"].Value + copula +
                      candidate.Groups["verbGap"].Value + adjective;
                if (CaseHelper.IsAllUpper(candidate.Value)) replacement = replacement.ToUpperInvariant();
                else if (char.IsUpper(candidate.Value[0])) replacement = CaseHelper.Capitalize(replacement);
                text = text[..candidate.Index] + replacement + text[(candidate.Index + candidate.Length)..];
            }
        }

        var expertise = EnglishExpertise.Matches(english);
        if (expertise.Count != 1 || LiteralExpertise.Matches(text).Count != 1) return text;
        var enOwner = expertise[0].Groups["subject"].Value.ToLowerInvariant() switch
        {
            "i" => "my", "you" => "your", "he" => "his", "she" => "her", "we" => "our", "they" => "their",
            _ => expertise[0].Groups["own"].Value.ToLowerInvariant(),
        };
        if (!expertise[0].Groups["own"].Value.Equals(enOwner, StringComparison.OrdinalIgnoreCase)) return text;
        return LiteralExpertise.Replace(text, m =>
        {
            if (blocked("conhecer") || blocked("saber") || blocked("coisas") ||
                blocked(m.Value) || Regex.Matches(m.Value, @"[\p{L}\p{M}]+").Any(w => blocked(w.Value)))
                return m.Value;
            var forms = ExpertiseForms[m.Groups["verb"].Value];
            var replacement = forms.Know + m.Groups["gap"].Value + m.Groups["adv"].Value + "o que " + forms.Do;
            return CaseHelper.IsAllUpper(m.Value) ? replacement.ToUpperInvariant()
                : char.IsUpper(m.Value[0]) ? CaseHelper.Capitalize(replacement) : replacement;
        });
    }
}
