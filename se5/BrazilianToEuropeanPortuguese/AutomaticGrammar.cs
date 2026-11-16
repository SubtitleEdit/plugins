using System.Text.RegularExpressions;

namespace SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;

internal static class AutomaticGrammar
{
    private const RegexOptions Ci = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;
    private const string Boundary = @"(?![\p{L}\p{M}-])";
    private const string Ordinal = @"(?:primeir|segund|terceir|quart|quint|sext|sétim|oitav|non|décim)[oa]s?";
    private static readonly Dictionary<string, bool> NounGender = new(StringComparer.OrdinalIgnoreCase)
    {
        ["violação"] = true, ["violações"] = true, ["agressão"] = true, ["agressões"] = true,
        ["morte"] = true, ["mortes"] = true, ["tentativa"] = true, ["tentativas"] = true,
        ["reunião"] = true, ["reuniões"] = true, ["visita"] = true, ["visitas"] = true,
        ["chamada"] = true, ["chamadas"] = true, ["queixa"] = true, ["queixas"] = true,
        ["equipa"] = true, ["equipas"] = true, ["carrinha"] = true, ["carrinhas"] = true,
        ["vez"] = true, ["vezes"] = true, ["ocorrência"] = true, ["ocorrências"] = true,
        ["caso"] = false, ["casos"] = false, ["crime"] = false, ["crimes"] = false,
        ["acidente"] = false, ["acidentes"] = false, ["assalto"] = false, ["assaltos"] = false,
        ["homicídio"] = false, ["homicídios"] = false, ["incidente"] = false, ["incidentes"] = false,
        ["ataque"] = false, ["ataques"] = false, ["roubo"] = false, ["roubos"] = false,
        ["frigorífico"] = false, ["frigoríficos"] = false, ["casaco"] = false, ["casacos"] = false,
        ["autocarro"] = false, ["autocarros"] = false, ["comboio"] = false, ["comboios"] = false,
        ["telemóvel"] = false, ["telemóveis"] = false,
        ["ideia"] = true, ["ideias"] = true,
        ["boca"] = true, ["bocas"] = true, ["acordo"] = false, ["acordos"] = false,
    };
    private static readonly string Nouns = string.Join("|", NounGender.Keys.OrderByDescending(n => n.Length).Select(Regex.Escape));
    private static readonly Regex OrdinalEvent = new(
        @"(?<![\p{L}\p{M}-])(?<det>o|a|os|as|um|uma|uns|umas|este|esta|estes|estas|esse|essa|esses|essas)\s+" +
        @"(?<ordinal>" + Ordinal + @")(?<space>\s+)(?<de>de\s+)?(?<noun>" + Nouns + ")" + Boundary, Ci);
    private static readonly Regex DeterminerNoun = new(
        @"(?<pre>^|[.!?;]\s+|\b(?:é|era|foi|será|são|eram|foram|serão)\s+)(?<det>o|a|os|as|um|uma|uns|umas|este|esta|estes|estas|esse|essa|esses|essas)\s+" +
        @"(?<noun>" + Nouns + ")" + Boundary, Ci);
    private static readonly Regex IndefiniteNoun = new(
        @"(?<![\p{L}\p{M}-])(?<det>um|uma|uns|umas)\s+(?<noun>" + Nouns + ")" + Boundary, Ci);
    private static readonly Regex TemporalPorHora = new(
        @"(?<![\p{L}\p{M}-])(?<head>(?:é|era|foi|será)\s+(?:tudo|só\s+isso|só\s+isto)|(?:basta|chega))\s+por\s+hora" + Boundary, Ci);
    private static readonly Regex AtFront = new(
        @"(?<![\p{L}\p{M}-])(?<v>estou|estás|está|estamos|estão|estava|estavas|estávamos|estavam|ficou|fica|ficamos|ficam)\s+a\s+frente" + Boundary, Ci);
    private static readonly Regex ComeToMind = new(
        @"(?<![\p{L}\p{M}-])(?<v>vem|vêm|veio|vieram|vinha|vinham|venha|venham|vier|vierem|vir)\s+a\s+mente" + Boundary, Ci);
    private static readonly Regex TodoMassNoun = new(
        @"(?<![\p{L}\p{M}-])(?<todo>todo)\s+(?<noun>dinheiro|sentido|tempo)" + Boundary, Ci);
    private static readonly Regex TodoArticleNoun = new(
        @"(?<![\p{L}\p{M}-])(?<todo>tod[oa]s?)(?<gap>\s+)(?<article>o|a|os|as)(?<space>\s+)(?<noun>" +
        Nouns + ")" + Boundary, Ci);
    private static readonly Regex Subject = new(
        @"(?<pre>^|[.!?;]\s+)(?<subject>nós|eles|elas)\s+(?<adv>(?:(?:não|já|sempre|nunca|também|ainda)\s+){0,2})" +
        @"(?<verb>\p{L}+)" + Boundary + @"(?!\s*[-:])", Ci);
    private static readonly Dictionary<string, string[]> PluralForms = BuildPluralForms();
    private static readonly Regex RequiredDe = new(
        @"(?<pre>^|[.!?]\s+)(?<subject>eu\s+|tu\s+|nós\s+|eles\s+|elas\s+)?(?<verb>gosto|gostas|gostamos|gostam|preciso|precisas|precisamos|precisam)" +
        @"\s+(?<det>o|a|os|as|um|uma|uns|umas)\s+(?!(?:pouco|bocado|bocadinho|que)\b)(?=\p{L})", Ci);
    private static readonly Regex InjuryState = new(
        @"(?<![\p{L}\p{M}-])(?<part>pernas?|braços?|mãos?|pés?|joelhos?|tornozelos?|ombros?|pulsos?|cotovelos?|" +
        @"dedos?|costelas?|costas|anca|ancas|cabeça|pescoço|coluna|tendões|tendão|músculos?|ligamentos?)" +
        @"(?<owner>(?:\s+(?:esquerd[oa]s?|direit[oa]s?))?" +
        @"(?:\s+d(?:e|o|a|os|as)(?:\s+(?!(?:foi|foram|que|e|ou|não|nunca|já|também)\b)[\p{L}\p{M}]+){1,6})?)" +
        @"(?<gap>\s+)(?<verb>foi|foram)(?<space>\s+)(?<degree>(?:(?:gravemente|ligeiramente|seriamente)\s+)?)" +
        @"(?<injury>lesionad[oa]s?|magoad[oa]s?|ferid[oa]s?)" + Boundary +
        @"(?!\s+(?:por|pel[oa]s?|de\s+propósito|intencionalmente|deliberadamente)\b)", Ci);

    private static Dictionary<string, string[]> BuildPluralForms()
    {
        var result = GrammarReview.SingularPlural.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.OrdinalIgnoreCase);
        foreach (var inf in new[] { "falar", "trabalhar", "estudar", "ajudar", "chegar", "voltar", "entrar", "pensar",
                     "achar", "acreditar", "contar", "comprar", "pagar", "esperar", "gostar", "precisar", "aprender",
                     "comer", "beber", "vender", "receber", "viver", "partir", "abrir" })
        {
            if (!VerbConjugator.TryConjugate(inf, out var forms))
                throw new InvalidOperationException($"Cannot conjugate {inf}.");
            foreach (var (singular, first, third) in new[]
                     { (1, 4, 5), (2, 4, 5), (3, 4, 5), (6, 42, 9), (7, 42, 9), (8, 42, 9),
                         (10, 12, 13), (11, 12, 13), (14, 17, 18), (15, 17, 18), (16, 17, 18),
                         (19, 21, 22), (20, 21, 22) })
                result[forms[singular]] = [forms[first], forms[third]];
        }
        return result;
    }

    private static string AgreeDeterminer(string word, bool feminine, bool plural)
    {
        var stem = word.ToLowerInvariant().TrimEnd('s');
        var target = stem switch
        {
            "o" or "a" => feminine ? "a" : "o",
            "um" or "uma" or "un" => feminine ? "uma" : "um",
            "este" or "esta" => feminine ? "esta" : "este",
            _ => feminine ? "essa" : "esse",
        };
        if (plural) target = target == "um" ? "uns" : target + "s";
        return CaseHelper.MatchCase(word, target);
    }

    public static string Apply(string text, ConversionOptions options)
    {
        if (options.Vocabulary)
        {
            text = TodoArticleNoun.Replace(text, m =>
            {
                var noun = m.Groups["noun"].Value;
                var feminine = NounGender[noun];
                var plural = noun.EndsWith("s", StringComparison.OrdinalIgnoreCase);
                return CaseHelper.MatchCase(m.Groups["todo"].Value, "tod" + (feminine ? "a" : "o") + (plural ? "s" : "")) +
                    m.Groups["gap"].Value + AgreeDeterminer(m.Groups["article"].Value, feminine, plural) +
                    m.Groups["space"].Value + noun;
            });
            text = OrdinalEvent.Replace(text, m =>
            {
                var noun = m.Groups["noun"].Value;
                var feminine = NounGender[noun];
                var plural = noun.EndsWith('s');
                var ordinal = m.Groups["ordinal"].Value;
                var stem = ordinal.TrimEnd('s', 'S')[..^1];
                return AgreeDeterminer(m.Groups["det"].Value, feminine, plural) + " " +
                    CaseHelper.MatchCase(ordinal, stem + (feminine ? "a" : "o") + (plural ? "s" : "")) +
                    m.Groups["space"].Value + noun;
            });
            text = DeterminerNoun.Replace(text, m =>
            {
                var noun = m.Groups["noun"].Value;
                var det = m.Groups["det"].Value;
                var plural = det.EndsWith('s');
                if (plural != noun.EndsWith('s')) return m.Value;
                return m.Groups["pre"].Value + AgreeDeterminer(det, NounGender[noun], plural) + " " + noun;
            });
            text = IndefiniteNoun.Replace(text, m =>
            {
                var noun = m.Groups["noun"].Value;
                var det = m.Groups["det"].Value;
                return det.EndsWith('s') != noun.EndsWith('s') ? m.Value :
                    AgreeDeterminer(det, NounGender[noun], noun.EndsWith('s')) + " " + noun;
            });
        }
        if (options.Colloquial)
        {
            text = InjuryState.Replace(text, m =>
            {
                var part = m.Groups["part"].Value;
                var plural = part.EndsWith("s", StringComparison.OrdinalIgnoreCase);
                var injury = m.Groups["injury"].Value;
                if (plural != m.Groups["verb"].Value.Equals("foram", StringComparison.OrdinalIgnoreCase) ||
                    plural != injury.EndsWith("s", StringComparison.OrdinalIgnoreCase)) return m.Value;
                return part + m.Groups["owner"].Value + m.Groups["gap"].Value +
                    CaseHelper.MatchCase(m.Groups["verb"].Value, plural ? "ficaram" : "ficou") +
                    m.Groups["space"].Value + m.Groups["degree"].Value + injury;
            });
            text = TemporalPorHora.Replace(text, "${head} por agora");
            text = AtFront.Replace(text, "${v} à frente");
            text = ComeToMind.Replace(text, "${v} à mente");
            text = Regex.Replace(text, @"(?<![\p{L}\p{M}-])(?<p>pelo|pela|pelos|pelas)\s+(?:o|a|os|as)\s+que\b", "${p} que", Ci);
            text = TodoMassNoun.Replace(text, "${todo} o ${noun}");
            text = Subject.Replace(text, m => PluralForms.TryGetValue(m.Groups["verb"].Value, out var forms)
                ? m.Groups["pre"].Value + m.Groups["subject"].Value + " " + m.Groups["adv"].Value +
                  CaseHelper.MatchCase(m.Groups["verb"].Value, forms[m.Groups["subject"].Value.Equals("nós", StringComparison.OrdinalIgnoreCase) ? 0 : 1])
                : m.Value);
            text = RequiredDe.Replace(text, m =>
            {
                var det = m.Groups["det"].Value;
                return m.Groups["pre"].Value + m.Groups["subject"].Value + m.Groups["verb"].Value + " " +
                    (det.ToLowerInvariant() switch { "o" => "do", "a" => "da", "os" => "dos", "as" => "das", _ => "de " + det }) + " ";
            });
        }
        return text;
    }
}
