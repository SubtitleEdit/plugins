using System.Text.RegularExpressions;

namespace SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;

internal static class DiscourseTags
{
    private const RegexOptions Ci = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;
    private static readonly Regex Tag = new(
        @"(?<pre>,\s*(?:não\s+)?)(?<verb>sabe|entende|percebe|compreende|viu|ouviu|entendeu|percebeu)" +
        @"(?=\s*\?+[.!?]*[\s""”»'\uE000-\uF8FF]*$)", Ci);
    private static readonly Dictionary<string, string> Informal = new(StringComparer.OrdinalIgnoreCase)
    {
        ["sabe"] = "sabes", ["entende"] = "entendes", ["percebe"] = "percebes", ["compreende"] = "compreendes",
        ["viu"] = "viste", ["ouviu"] = "ouviste", ["entendeu"] = "entendeste", ["percebeu"] = "percebeste",
    };

    internal static bool HasInformalEvidence(string text)
    {
        if (PortugueseConverter.YouToTuRules.IsFormal(text) ||
            Regex.IsMatch(text, @"(?m)^\s*[-–—]|\b(?:você|vocês|vós)\b|[""«»“”]", Ci)) return false;
        if (Regex.IsMatch(text, @"(?<![\p{L}\p{M}-])(?:tu|teus?|tuas?|ti|contigo)\b|-te\b", Ci) ||
            Regex.IsMatch(text, @"^[\s\uE000-\uF8FF]*(?:ouve|olha|escuta|vê|repara|espera|desculpa)\s*,", Ci))
            return true;
        var analysis = new ClauseAgreementAnalysis(text);
        return Regex.Matches(text, @"\p{L}+").Any(m =>
            PortugueseVerbs.IsSecondPersonForm(m.Value) && analysis.Decide(m.Index, null).Allowed);
    }

    internal static string Apply(string text, ReferenceHint? hint)
    {
        if (hint is { FormalAddress: true } && hint is not { ForceInformal: true } ||
            PortugueseConverter.YouToTuRules.IsFormal(text) ||
            Regex.IsMatch(text, @"(?m)^\s*[-–—]|\b(?:vocês|vós)\b|[""«»“”]", Ci)) return text;
        var tag = Tag.Match(text);
        if (!tag.Success || tag.Index == 0) return text;
        var english = hint?.EnglishText;
        if (!string.IsNullOrWhiteSpace(english))
        {
            var pattern = tag.Groups["verb"].Value.ToLowerInvariant() switch
            {
                "sabe" => @"you\s+know",
                "viu" => @"(?:you\s+see|see)",
                "ouviu" => @"(?:you\s+hear(?:\s+me)?|hear\s+me)",
                _ => @"(?:you\s+(?:understand|get\s+it)|(?:do|did)\s+you\s+(?:understand|get\s+it))",
            };
            if (!Regex.IsMatch(english, @",\s*(?:" + pattern + @")\s*\?+[.!?]*\s*$", Ci))
                return text;
        }
        if (hint is not { ForceInformal: true } && hint is not { InformalTagContinuation: true } &&
            !HasInformalEvidence(text[..tag.Index])) return text;
        var verb = tag.Groups["verb"];
        return text[..verb.Index] + CaseHelper.MatchCase(verb.Value, Informal[verb.Value]) +
            text[(verb.Index + verb.Length)..];
    }
}
