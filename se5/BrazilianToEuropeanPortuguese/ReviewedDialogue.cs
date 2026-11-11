using System.Text.RegularExpressions;

namespace SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;

internal static class ReviewedDialogue
{
    private const RegexOptions Ci = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;
    private const string Before = @"(?<![\p{L}\p{M}-])";
    private const string After = @"(?![\p{L}\p{M}-])";
    private static readonly Regex Reminder = new(
        Before + @"(?<verb>(?:re)?lembrar|(?:re)?lembrá-l[oa])(?=\s+que\b)", Ci);
    private static readonly Regex OwedObject = new(
        Before + @"(?<verb>[\p{L}\p{M}]+)(?<gap>\s+)(?<object>isto|isso|aquilo|uma\s+explicação|um\s+favor)" +
        @"\s+a\s+(?<person>ele|ela|eles|elas)(?=\s*(?:[.!?…]|[\uE000-\uF8FF]|$))", Ci);
    private static readonly HashSet<string> OweForms = new(
        VerbConjugator.TryConjugate("dever", out var forms) ? forms : throw new InvalidOperationException("Cannot conjugate dever."),
        StringComparer.OrdinalIgnoreCase);
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

    internal static string Objects(string text, ReferenceHint? reference, bool informal, Func<string, bool> blocked)
    {
        var english = reference?.EnglishText ?? "";
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
        return text;
    }

    internal static string Vocabulary(string text, ReferenceHint? reference, Func<string, bool> blocked)
    {
        var english = reference?.EnglishText ?? "";
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
