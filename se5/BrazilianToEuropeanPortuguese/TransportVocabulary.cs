using System.Text.RegularExpressions;

namespace SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;

internal static class TransportVocabulary
{
    private const RegexOptions Ci = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;
    private const string Vehicle = @"(?:carros?|automóveis|automóvel|veículos?|motos?|motocicletas?|" +
        @"motociclos?|camiões|camião|caminhões|caminhão|carrinhas?|camionetas?|caminhonetes?|" +
        @"barcos?|lanchas?|embarcações|embarcação|motores?)";
    private static readonly Regex JumpStart = new(
        @"(?<![\p{L}\p{M}\d_-])(?<verb>dar|dou|dás|dá|damos|dais|dão|dei|deste|deu|demos|destes|deram|" +
        @"dava|davas|dávamos|dáveis|davam|dera|deras|déramos|déreis|darei|darás|dará|daremos|dareis|darão|" +
        @"daria|darias|daríamos|daríeis|dariam|dê|dês|demos|deis|deem|desse|desses|déssemos|désseis|dessem|" +
        @"der|deres|dermos|derdes|derem|dares|darmos|dardes|darem|dando|dado|" +
        @"fazer|faço|fazes|faz|fazemos|fazeis|fazem|fiz|fizeste|fez|fizemos|fizestes|fizeram|" +
        @"fazia|fazias|fazíamos|fazíeis|faziam|farei|farás|fará|faremos|fareis|farão|faria|farias|faríamos|faríeis|fariam|" +
        @"faça|faças|façamos|façais|façam|fizesse|fizesses|fizéssemos|fizésseis|fizessem|fizer|fizeres|fizermos|fizerdes|fizerem|fazendo|feito)" +
        @"(?<gap>\s+)(?:uma\s+)?(?<noun>chupeta|chucha)(?<space>\s+)" +
        @"(?<prep>nos|nas|no|na|aos|às|ao|à|em|a)(?<gap2>\s+)" +
        @"(?<vehicle>(?:(?:o|a|os|as|um|uma|uns|umas|meu|minha|meus|minhas|teu|tua|teus|tuas|" +
        @"seu|sua|seus|suas|nosso|nossa|nossos|nossas|vosso|vossa|vossos|vossas|este|esta|esse|essa)\s+){0,2}" +
        Vehicle + @")(?![\p{L}\p{M}\d_-])", Ci);

    internal static string Apply(string text, Func<string, bool> blocked) => JumpStart.Replace(text, m =>
    {
        var verb = m.Groups["verb"].Value;
        var noun = m.Groups["noun"].Value;
        if (blocked("chupeta") || blocked("chucha") || blocked(noun) || blocked(verb) ||
            blocked(m.Groups["vehicle"].Value)) return m.Value;
        var words = Regex.Split(m.Value, @"\s+");
        for (var start = 0; start < words.Length; start++)
            for (var length = 1; start + length <= words.Length; length++)
                if (blocked(string.Join(" ", words, start, length))) return m.Value;
        var give = verb.StartsWith("d", StringComparison.OrdinalIgnoreCase);
        if (blocked(give ? "dar" : "fazer")) return m.Value;
        var prep = m.Groups["prep"].Value.ToLowerInvariant() switch
        {
            "no" => "ao", "na" => "à", "nos" => "aos", "nas" => "às", "em" => "a",
            var value => value,
        };
        var replacement = (give ? "" : "um ") + "arranque com cabos" +
            m.Groups["space"].Value + prep;
        if (CaseHelper.IsAllUpper(m.Value)) replacement = replacement.ToUpperInvariant();
        return verb + m.Groups["gap"].Value + replacement + m.Groups["gap2"].Value + m.Groups["vehicle"].Value;
    });
}
