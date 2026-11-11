using System.Text.RegularExpressions;

namespace SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;

internal static class ExplanationConstructions
{
    private const RegexOptions Ci = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;
    private static readonly Regex Follow = new(
        @"(?<pre>(?:^|[.!?;]\s*)[\s\uE000-\uF8FF-]*(?:(?:sim|bem|certo|está\s+bem|mas|e|então)[,]?\s+){0,3})" +
        @"(?<verb>acompanhe|acompanha|acompanhem|acompanhai)(?=\s*[,;.!?])", Ci);
    private static readonly Regex ReasoningLeap = new(
        @"(?<![\p{L}\p{M}-])(?<head>(?:este|esse|isto|isso)\s+é\s+o|(?:um|o)\s+(?:grande\s+)?)(?<gap>\s*)(?<noun>pulo)" +
        @"(?![\p{L}\p{M}-])", Ci);
    private static readonly Regex Continuation = new(
        @"(?<pre>^[\s\uE000-\uF8FF]*que\s+(?:(?:não|nunca|já|também|ainda|sempre|só)\s+){0,2}" +
        @"(?:(?<clitic>me|te|se|nos|vos|lhe|lhes|o|a|os|as)\s+)?)(?<verb>[\p{L}\p{M}]+)(?![\p{L}\p{M}-])", Ci);
    private static readonly Regex GoverningSubject = new(
        @"(?:^|[.!?;]\s+)[\s\uE000-\uF8FF]*(?:e\s+se\s+|se\s+|mas\s+|e\s+)?" +
        @"(?<subject>você|tu)\s+(?:(?:não|nunca|já|também|ainda|sempre|só)\s+){0,2}" +
        @"(?<verb>[\p{L}\p{M}]+)(?<tail>(?:\s+[\p{L}\p{M}]+){0,5})\s*$", Ci);
    private static readonly HashSet<string> GoverningForms = new(
        new[] { "achar", "pensar", "acreditar", "imaginar", "considerar", "decidir", "prometer" }
            .SelectMany(Forms).Concat(new[] { "diz", "disse", "dizes", "disseste", "diria", "dirias",
                "sabe", "sabes", "sabia", "sabias", "soube", "soubeste" }), StringComparer.OrdinalIgnoreCase);

    private static IEnumerable<string> Forms(string verb)
    {
        if (!VerbConjugator.TryConjugate(verb, out var forms))
            throw new InvalidOperationException($"Cannot conjugate reviewed continuation verb: {verb}.");
        return forms;
    }

    internal static string FollowReasoning(string text, ReferenceHint? reference, Func<string, bool> isBlocked,
        bool pronouns, bool vocabulary)
    {
        var english = reference?.EnglishText ?? "";
        var follows = Regex.IsMatch(english,
            @"\b(?:stay\s+with\s+me|follow\s+(?:me|my\s+(?:reasoning|argument|logic))|bear\s+with\s+me)\b", Ci);
        var reasoning = (Regex.IsMatch(english, @"\b(?:reasoning|argument|explanation|logic)\b", Ci) ||
            follows && Regex.IsMatch(english, @"\bleap\b", Ci)) &&
            !Regex.IsMatch(english, @"\b(?:jump|jumping|cliff|roof|ledge|faith|ski|jumped|physical|literal)\b", Ci);
        if (!reasoning) return text;
        if (pronouns && !isBlocked("acompanhar") && follows)
            text = Follow.Replace(text, m => isBlocked(m.Groups["verb"].Value) ? m.Value :
                m.Groups["pre"].Value + PortugueseVerbs.AttachEnclitic(m.Groups["verb"].Value,
                    CaseHelper.IsAllUpper(m.Groups["verb"].Value) ? "ME" : "me"));
        if (vocabulary && !isBlocked("pulo") && Regex.IsMatch(english, @"\bleap\b", Ci))
            text = ReasoningLeap.Replace(text, m => m.Groups["head"].Value + m.Groups["gap"].Value +
                CaseHelper.MatchCase(m.Groups["noun"].Value, "salto"));
        return text;
    }

    internal static bool HasGoverningAddressee(string previous, ReferenceHint? reference)
    {
        var match = GoverningSubject.Match(Regex.Replace(previous, @"<[^>]*>|\{[^}]*\}", ""));
        if (!match.Success) return false;
        var verb = match.Groups["verb"].Value;
        var tail = match.Groups["tail"].Value;
        return reference is { SecondPersonSubject: true, ThirdPerson: false } && tail.Length == 0 && GoverningForms.Contains(verb) ||
            Regex.IsMatch(verb, @"^(?:mete|meteu|metia|meteria|metesse|meteste|metes|metias|meterias|metesses)$", Ci) &&
            Regex.IsMatch(tail, @"^\s+na\s+(?:(?:sua|tua)\s+)?cabeça$", Ci);
    }

    internal static string ContinueAddressee(string text) => Continuation.Replace(text, m =>
    {
        var verb = m.Groups["verb"].Value;
        if (m.Groups["clitic"].Value.Equals("se", StringComparison.OrdinalIgnoreCase) ||
            PortugueseVerbs.IsSecondPersonForm(verb) || PortugueseVerbs.IsKnownInfinitive(verb) ||
            PortugueseConverter.TuImperative.IsImpersonalConditional(verb) ||
            Regex.IsMatch(m.Result("$'"), @"^\s+(?:(?:eu|ele|ela|eles|elas|nós|vocês|o\s+senhor|a\s+senhora)\b|" +
                @"(?:o|a|os|as)\s+(?!seu\b|sua\b|seus\b|suas\b|teu\b|tua\b|teus\b|tuas\b)\p{L})", Ci))
            return m.Value;
        return PortugueseVerbs.TryThirdToSecond(verb, out var second) ||
               PortugueseVerbs.TryGuessThirdToSecond(verb, out second)
            ? m.Groups["pre"].Value + CaseHelper.MatchCase(verb, second) : m.Value;
    });
}
