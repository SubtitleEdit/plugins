using System.Text.RegularExpressions;

namespace SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;

internal static class GreetingConstructions
{
    private const RegexOptions Ci = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;
    private static readonly Regex NamedQuestion = new(
        @"(?<pre>(?:^|[.!?]\s+|\n)[\s""“«'\uE000-\uF8FF-]*" +
        @"(?:ei|olá|oi)\s*,\s*(?<name>(?-i:\p{Lu}[\p{L}\p{M}'’-]+))\s*[,!?]\s*como\s+)" +
        @"(?<verb>vai|está|anda|tem)(?=(?:\s+estado)?\s*\?)", Ci);
    private static readonly Regex CollectiveArticle = new(
        @"(?<![\p{L}\p{M}\d_-])(?<quant>toda|todo)(?<gap>\s+)" +
        @"(?<noun>equipa|equipe|família|turma|tripulação|comunidade|população|comissão|" +
        @"grupo|povo|pelotão|batalhão|departamento)(?![\p{L}\p{M}\d_-])", Ci);

    public static string CollectiveArticles(string text) => CollectiveArticle.Replace(text, m =>
    {
        var feminine = m.Groups["noun"].Value.ToLowerInvariant() is
            "equipa" or "equipe" or "família" or "turma" or "tripulação" or "comunidade" or "população" or "comissão";
        if (m.Groups["quant"].Value.EndsWith("a", StringComparison.OrdinalIgnoreCase) != feminine) return m.Value;
        var article = feminine ? "a" : "o";
        if (CaseHelper.IsAllUpper(m.Value)) article = article.ToUpperInvariant();
        return m.Groups["quant"].Value + " " + article + m.Groups["gap"].Value + m.Groups["noun"].Value;
    });

    public static string InformalQuestions(string text, string? english)
    {
        if (string.IsNullOrWhiteSpace(english)) return text;
        return NamedQuestion.Replace(text, m =>
        {
            var name = m.Groups["name"].Value;
            if (PortugueseConverter.YouToTuRules.IsFormal(name) ||
                !Regex.IsMatch(english, @"\b(?:hey|hi|hello)\s*,?\s+" + Regex.Escape(name) +
                    @"\s*[,!.?]?\s*how\s+(?:(?:are|have)\s+)?you\b(?!\s+(?:all|both|guys|two|and|or)\b)" +
                    @"(?:['’]re)?(?:\s+(?:doing|been|going))?", Ci))
                return m.Value;
            var verb = m.Groups["verb"].Value;
            return m.Groups["pre"].Value + CaseHelper.MatchCase(verb, verb.ToLowerInvariant() switch
            {
                "vai" => "vais", "está" => "estás", "anda" => "andas", _ => "tens",
            });
        });
    }
}
