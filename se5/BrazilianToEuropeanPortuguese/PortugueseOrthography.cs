using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;

internal static class PortugueseOrthography
{
    // Reviewed stems; the target dictionary must accept each resulting inflection.
    private static readonly (string Before, string After)[] Stems =
    {
        ("acção", "ação"), ("acções", "ações"), ("accion", "acion"),
        ("activ", "ativ"), ("actor", "ator"), ("actriz", "atriz"), ("actrizes", "atrizes"),
        ("acto", "ato"), ("actual", "atual"), ("actua", "atua"),
        ("adop", "ado"), ("afect", "afet"), ("bapt", "bat"),
        ("colect", "colet"), ("correc", "corre"), ("correct", "corret"),
        ("direcção", "direção"), ("direcções", "direções"), ("direct", "diret"),
        ("efect", "efet"), ("exact", "exat"), ("excep", "exce"), ("óptim", "ótim"),
        ("object", "objet"), ("project", "projet"), ("protecção", "proteção"),
        ("protecções", "proteções"), ("react", "reat"), ("recep", "rece"),
        ("seleccion", "selecion"), ("selecção", "seleção"), ("selecções", "seleções"),
        ("inspecção", "inspeção"), ("inspecções", "inspeções"), ("inspector", "inspetor"),
        ("detect", "detet"), ("percep", "perce"), ("concep", "conce"),
        ("decep", "dece"), ("aspect", "aspet"), ("espectador", "espetador"),
        ("espectác", "espetác"), ("espectac", "espetac"),
        ("recept", "recet"), ("percept", "percet"), ("concept", "concet"),
        ("ruptur", "rutur"), ("secção", "seção"), ("secções", "seções"),
        ("interacção", "interação"), ("interacções", "interações"), ("interactiv", "interativ"),
        ("transacção", "transação"), ("transacções", "transações"), ("tractor", "trator"),
        ("electric", "eletric"), ("eléctric", "elétric"), ("arquitect", "arquitet"),
        ("Egipto", "Egito"), ("tecto", "teto"),
        ("vôo", "voo"), ("vôos", "voos"), ("lêem", "leem"), ("vêem", "veem"),
        ("crêem", "creem"), ("dêem", "deem"),
    };
    private static readonly Regex Word = new(@"[\p{L}\p{M}]+");
    private static readonly Regex FinalInterrogativeQue = new(
        @"(?<![\p{L}\p{M}-])que(?![\p{L}\p{M}-])(?=[\s\uE000-\uF8FF]*\?)",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    internal static string Apply(string text, bool postAgreement)
    {
        text = FinalInterrogativeQue.Replace(text, match => CaseHelper.MatchCase(match.Value, "quê"));
        var dictionary = NaturaDictionary.Load(postAgreement);
        return Word.Replace(text, match =>
        {
            var word = match.Value;
            if (dictionary.Check(word)) return word;
            var candidates = Stems
                .Select(pair => (From: postAgreement ? pair.Before : pair.After, To: postAgreement ? pair.After : pair.Before))
                .Where(pair => word.StartsWith(pair.From, StringComparison.OrdinalIgnoreCase))
                .Select(pair => CaseHelper.MatchCase(word, pair.To + word[pair.From.Length..]))
                .Where(dictionary.Check)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(2).ToArray();
            return candidates.Length == 1 ? candidates[0] : word;
        });
    }
}
