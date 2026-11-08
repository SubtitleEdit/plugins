using System.Text.RegularExpressions;

namespace SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;

/// <summary>
/// A relative "onde" giving the purpose of a command takes the subjunctive in European Portuguese:
/// "Mãos onde podemos vê-las!" → "Mãos onde possamos vê-las!", "Põe as mãos onde eu posso ver" → "onde eu possa ver".
/// Indirect questions and factual places ("Sabes onde posso…", "uma cama onde posso dormir") keep the indicative.
/// </summary>
internal static class PurposeClauses
{
    private const RegexOptions Ci = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;

    private static readonly Regex Where = new(
        @"(?<pre>(?<![\p{L}\p{M}-])(?:onde|aonde)\s+(?:(?:eu|nós|ele|ela|eles|elas|todos|toda\s+a\s+gente|a\s+gente)\s+)?(?:(?:não|sempre|bem)\s+)?" +
        @"(?:(?:me|te|nos|vos|lhe|lhes|o|a|os|as)\s+)?)" +
        @"(?<v>posso|podes|pode|podemos|podem|consigo|consegues|consegue|conseguimos|conseguem|vejo|vês|vê|vemos|veem|vêem)(?![\p{L}\p{M}-])", Ci);

    // Placement and staying commands (not "diz/mostra/sabe", which introduce indirect questions).
    private const string Commands =
        @"põe|ponha|ponham|ponde|pões|coloca|coloque|coloquem|mete|meta|metam|mantém|mantenha|mantenham|mantende|deixa|deixe|deixem|" +
        @"levanta|levante|levantem|fica|fique|fiquem|encosta|encoste|encostem|estaciona|estacione|estacionem|guarda|guarde|guardem|" +
        @"senta|sente|sentem|vem|venha|venham|anda|ande|andem|vira|vire|virem|estende|estenda|estendam|ergue|erga|ergam|" +
        @"mexe|mexa|mexam|tira|tire|tirem|sai|saia|saiam|volta|volte|voltem|aparece|apareça|apareçam";
    private static readonly Regex CommandStart = new(
        @"^[^\p{L}\p{M}]*(?:(?:agora|já|só|e|então|ok|vá),?\s+)?(?:" + Commands + @")(?:-(?:me|te|se|nos|vos|o|a|os|as|lo|la|los|las|no|na|nos|nas))?(?![\p{L}\p{M}])", Ci);
    // Verbless police commands: "Mãos onde…", "As mãos no ar, onde…".
    private static readonly Regex VerblessStart = new(
        @"^[^\p{L}\p{M}]*(?:(?:as|os|essas|esses|tuas|teus|vossas|vossos|suas|seus|ambas\s+as)\s+)?(?:mãos|mão|braços|armas|arma)" +
        @"(?:\s+(?:no\s+ar|para\s+cima|ao\s+alto|à\s+vista|para\s+fora|fora))?\s*,?\s*$", Ci);
    private static readonly Regex EnglishCommandWhere = new(
        @"\b(?:hands|put|keep|stay|stand|place|leave|hold|move|step|get\s+(?:out|back|down|over))\b[^.!?]*\bwhere\s+(?:I|we|they|everyone|everybody|he|she|you|people)\s+can\b", Ci);
    private static readonly Regex Interrupting = new(@"[,;:]|(?<![\p{L}\p{M}])(?:que|se|quando|porque|mas|onde)(?![\p{L}\p{M}])", Ci);

    // Source typo: "Coloque-as mãos" (article hyphenated as a pronoun) becomes "Põe-nas mãos" later on.
    private static readonly Regex HyphenatedArticle = new(
        @"(?<v>(?<![\p{L}\p{M}-])[\p{L}\p{M}]{3,}(?:e|a|em|am|ui|ei|ai))-(?<art>as|os|a|o)\s+(?<n>[\p{L}\p{M}]+)(?![\p{L}\p{M}-])", Ci);
    private static readonly Dictionary<string, string[]> ArticleNouns = new(StringComparer.OrdinalIgnoreCase)
    {
        ["as"] = ["mãos", "armas", "chaves", "coisas", "malas", "mochilas", "luvas", "calças", "botas", "pernas", "costas", "janelas", "portas", "algemas", "fotos", "fotografias"],
        ["os"] = ["braços", "olhos", "pés", "dedos", "bolsos", "documentos", "papéis", "óculos", "sapatos", "joelhos", "ombros"],
        ["a"] = ["mão", "arma", "porta", "janela", "boca", "cabeça", "carteira", "chave", "mala", "bolsa", "faca", "mochila", "pistola"],
        ["o"] = ["carro", "telefone", "telemóvel", "celular", "dinheiro", "braço", "pé", "casaco", "capuz", "boné", "volante", "motor"],
    };

    public static string FixHyphenatedArticles(string s) =>
        s.IndexOf('-') < 0 ? s : HyphenatedArticle.Replace(s, m =>
            ArticleNouns.TryGetValue(m.Groups["art"].Value, out var nouns) &&
            nouns.Contains(m.Groups["n"].Value, StringComparer.OrdinalIgnoreCase)
                ? m.Groups["v"].Value + " " + m.Groups["art"].Value + " " + m.Groups["n"].Value
                : m.Value);

    public static string Apply(string s, string? english)
    {
        if (s.IndexOf("onde", StringComparison.OrdinalIgnoreCase) < 0) return s;
        var englishCommand = EnglishCommandWhere.IsMatch(english ?? string.Empty);
        return Where.Replace(s, m =>
        {
            if (!MaybeSubjunctive.TryPresentSubjunctive(m.Groups["v"].Value, out var subjunctive)) return m.Value;
            var prefix = SentencePrefix(s, m.Index);
            var commanded = (CommandStart.IsMatch(prefix) && !Interrupting.IsMatch(prefix)) || VerblessStart.IsMatch(prefix);
            if (!commanded && !(englishCommand && !Interrupting.IsMatch(prefix) && prefix.Length > 0)) return m.Value;
            return m.Groups["pre"].Value + subjunctive;
        });
    }

    private static string SentencePrefix(string s, int index)
    {
        var start = s.LastIndexOfAny(['.', '!', '?', '…'], Math.Max(0, index - 1));
        var prefix = s.Substring(start + 1, index - start - 1);
        prefix = Regex.Replace(prefix, @"<[^>]*>|\{[^}]*\}", string.Empty);
        return Regex.Replace(prefix, @"\s+", " ").Trim();
    }
}
