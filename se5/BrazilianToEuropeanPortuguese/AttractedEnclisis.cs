using System.Text.RegularExpressions;

namespace SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;

// Words such as "não", "já", "que" or "quando" pull the pronoun before a finite verb in European Portuguese:
// "Não disse-lhe nada" → "Não lhe disse nada", "o que Linda disse-lhe" → "o que a Linda lhe disse" (subject kept as is),
// "Quando o Frank viu-me" → "Quando o Frank me viu".
internal static class AttractedEnclisis
{
    private const RegexOptions Ci = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;

    private const string Attractor =
        "não|nunca|jamais|já|também|só|apenas|somente|sempre|ainda|nem|talvez|que|quem|onde|aonde|quando|porque|como|enquanto|embora|caso|se|" +
        "ninguém|alguém|tudo|nada";

    private const string Subject =
        @"(?:(?:eu|tu|ele|ela|nós|eles|elas|você|vocês|isso|isto|aquilo)|" +
        @"(?:(?:o|a|os|as|um|uma|este|esta|esse|essa|aquele|aquela)\s+)?(?:(?:meu|minha|meus|minhas|teu|tua|teus|tuas|seu|sua|seus|suas|nosso|nossa|nossos|nossas)\s+)?[\p{L}\p{M}]+)";

    // Only plain clitics: "-lo/-la/-no/-na" changed the verb form and are left alone.
    private static readonly Regex Enclitic = new(
        @"(?:(?<![\p{L}\p{M}-])|(?<=^-)|(?<=\n-))(?<attr>" + Attractor + @")(?<gap>[ \t]+|[ \t]*\n[ \t]*)" +
        @"(?<subj>" + Subject + @"(?:[ \t]+|[ \t]*\n[ \t]*))?" +
        @"(?<adv>(?:(?:não|nunca|já|também|só|sempre|ainda)[ \t]+)*)" +
        @"(?<v>[\p{L}\p{M}]+)-(?<cl>me|te|se|lhe|lhes|nos|vos|o|a|os|as)(?![\p{L}\p{M}-])", Ci);

    public static string Apply(string s)
    {
        if (s.IndexOf('-') < 0) return s;
        return Enclitic.Replace(s, m =>
        {
            var verb = m.Groups["v"].Value;
            var lower = verb.ToLowerInvariant();
            var attr = m.Groups["attr"].Value.ToLowerInvariant();
            var restrictive = attr is "só" or "apenas" or "somente";
            // After "se/quando/caso + subject" an infinitive-looking form is the future subjunctive: "se ele lembrar-se" → "se ele se lembrar".
            var futureSubjunctive = m.Groups["subj"].Success && PortugueseVerbs.IsInfinitive(lower) &&
                m.Groups["attr"].Value.ToLowerInvariant() is "se" or "quando" or "caso" or "enquanto";
            if (!(restrictive ? PortugueseVerbs.IsFiniteCliticForm(lower) : PortugueseVerbs.IsKnownVerbForm(lower)) && !futureSubjunctive || PortugueseVerbs.IsInfinitive(lower) && !futureSubjunctive ||
                lower.EndsWith("ndo", StringComparison.Ordinal) || PortugueseVerbs.IsFutureOrConditional(lower)) return m.Value;
            // "se" alone may itself be a clitic, but not before another clitic: "Se faz-te sentir" can only be "if" → "Se te faz sentir".
            if (attr == "se" && !m.Groups["subj"].Success &&
                (m.Groups["cl"].Value.Equals("se", StringComparison.OrdinalIgnoreCase) || m.Groups["adv"].Length > 0)) return m.Value;
            if (!m.Groups["subj"].Success && IsAffirmativeImperative(s, m.Index, attr, lower, m.Groups["cl"].Value))
            {
                if (attr is "só" or "apenas" or "somente" &&
                    m.Groups["cl"].Value.Equals("te", StringComparison.OrdinalIgnoreCase) && m.Groups["adv"].Length == 0)
                {
                    var command = verb + "-" + m.Groups["cl"].Value;
                    var adverb = m.Groups["attr"].Value;
                    if (char.IsUpper(adverb[0]))
                    {
                        command = char.ToUpperInvariant(command[0]) + command[1..];
                        if (!adverb.All(char.IsUpper)) adverb = adverb.ToLowerInvariant();
                    }
                    return command + m.Groups["gap"].Value + adverb;
                }
                return m.Value;
            }
            if (m.Groups["subj"].Success)
            {
                var subject = m.Groups["subj"].Value.TrimEnd();
                // A subject only sits between a subordinating word and its verb ("que a Linda disse"); "não ele" is not one.
                if (attr is "não" or "nunca" or "jamais" or "já" or "também" or "só" or "apenas" or "somente" or "sempre" or "ainda" or "nem" or "talvez" or
                    "ninguém" or "alguém" or "tudo" or "nada") return m.Value;
                if (!PortugueseConverter.PronounRules.AfterSubordinateSubject(attr + " " + subject)) return m.Value;
            }
            var clitic = m.Groups["cl"].Value;
            return m.Groups["attr"].Value + m.Groups["gap"].Value + m.Groups["subj"].Value + m.Groups["adv"].Value +
                   clitic + " " + verb;
        });
    }

    // Restrictive adverbs move after a recognised tu reflexive command; other adverbs retain their existing placement.
    private static bool IsAffirmativeImperative(string s, int index, string attr, string verb, string clitic)
    {
        if (attr is not ("só" or "apenas" or "somente" or "sempre" or "também" or "ainda" or "já")) return false;
        var you = clitic.Equals("se", StringComparison.OrdinalIgnoreCase);
        if (!you && !clitic.Equals("te", StringComparison.OrdinalIgnoreCase)) return false;
        var i = index - 1;
        while (i >= 0 && (s[i] == ' ' || s[i] == '\t' || s[i] == '"' || s[i] == '«')) i--;
        if (i >= 0 && ".!?…\n-—:;,".IndexOf(s[i]) < 0) return false;
        var regularImperative = verb.EndsWith('a') && PortugueseConverter.TuImperative.IsInfinitive(verb[..^1] + "ar") ||
            verb.EndsWith('e') && (PortugueseConverter.TuImperative.IsInfinitive(verb[..^1] + "er") ||
                PortugueseConverter.TuImperative.IsInfinitive(verb[..^1] + "ir"));
        if (!regularImperative && (verb.EndsWith("va", StringComparison.Ordinal) || verb.EndsWith("ia", StringComparison.Ordinal))) return false;
        // "Só lembre-se" is the formal command that later becomes "Só lembra-te".
        if (you) return PortugueseConverter.TuImperative.ToTu(verb) != null || verb == "sente";
        return regularImperative || PortugueseVerbs.TryThirdToSecond(verb, out var second) &&
            second.Equals(verb + "s", StringComparison.OrdinalIgnoreCase);
    }
}
