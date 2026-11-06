namespace SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;

public sealed record WordPair(string Brazilian, string European);

internal static class LocalWordListCsv
{
    private static readonly string[] WordHeaders = { "pt-BR", "pt-PT" };
    private static readonly string[] IgnoreHeaders = { "pt-BR" };

    internal static void ValidateWord(WordPair pair)
    {
        if (string.IsNullOrWhiteSpace(pair.Brazilian) || string.IsNullOrWhiteSpace(pair.European))
            throw new FormatException("PT-BR and PT-PT are required for custom words.");
        if (string.Equals(pair.Brazilian.Trim(), pair.European.Trim(), StringComparison.OrdinalIgnoreCase))
            throw new FormatException("Use the Ignore word list for identical PT-BR and PT-PT terms.");
    }

    internal static IReadOnlyList<WordPair> ParseWords(string csv)
    {
        var rows = SemicolonCsv.Parse(csv, WordHeaders);
        var pairs = rows.Select(row => new WordPair(row[0].Trim(), row[1].Trim())).ToList();
        foreach (var pair in pairs) ValidateWord(pair);
        return pairs;
    }

    internal static IReadOnlyList<string> ParseIgnored(string csv)
    {
        var rows = SemicolonCsv.Parse(csv, IgnoreHeaders);
        var words = rows.Select(row => row[0].Trim()).ToList();
        if (words.Any(string.IsNullOrWhiteSpace))
            throw new FormatException("PT-BR is required for every ignored word or expression.");
        return words;
    }

    internal static string SerializeWords(IEnumerable<WordPair> words)
    {
        var pairs = words.ToList();
        foreach (var pair in pairs) ValidateWord(pair);
        return SemicolonCsv.Serialize(WordHeaders, pairs.Select(pair => new[] { pair.Brazilian, pair.European }));
    }

    internal static string SerializeIgnored(IEnumerable<string> words) =>
        SemicolonCsv.Serialize(IgnoreHeaders, words.Select(word => new[] { word }));
}
