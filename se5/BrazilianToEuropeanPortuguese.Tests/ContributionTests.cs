using SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;
using System.Text;

namespace BrazilianToEuropeanPortuguese.Tests;

public sealed class ContributionTests
{
    [Fact]
    public void LocalWordCsvRoundTripsWordsExpressionsAndFormulaProtection()
    {
        var pairs = new[]
        {
            new WordPair("expressão; brasileira", "expressão \"portuguesa\""),
            new WordPair("=nome", "'termo"),
        };
        var csv = LocalWordListCsv.SerializeWords(pairs);
        Assert.StartsWith("pt-BR;pt-PT\r\n", csv);
        Assert.Equal(pairs, LocalWordListCsv.ParseWords("\uFEFF" + csv));
        var ignored = new[] { "Tá Bom Bar", "nome; composto", "=não alterar" };
        Assert.Equal(ignored, LocalWordListCsv.ParseIgnored(LocalWordListCsv.SerializeIgnored(ignored)));
        Assert.Empty(LocalWordListCsv.ParseWords(LocalWordListCsv.SerializeWords(Array.Empty<WordPair>())));
        Assert.Empty(LocalWordListCsv.ParseIgnored(LocalWordListCsv.SerializeIgnored(Array.Empty<string>())));
    }

    [Theory]
    [InlineData("pt-BR;pt-PT\npalavra;")]
    [InlineData("pt-BR;pt-PT\n;termo")]
    [InlineData("pt-BR;pt-PT\npalavra;PALAVRA")]
    [InlineData("pt-BR;pt-PT\nválida;correta\ninválida;")]
    [InlineData("pt-BR;pt-PT\npalavra;termo;extra")]
    [InlineData("pt-BR;pt-PT\n\"palavra;termo")]
    public void InvalidLocalWordsAreRejected(string csv) =>
        Assert.Throws<FormatException>(() => LocalWordListCsv.ParseWords(csv));

    [Theory]
    [InlineData("pt-BR\n\"\"")]
    [InlineData("pt-BR\n   ")]
    [InlineData("pt-BR\npalavra;extra")]
    [InlineData("pt-BR;pt-PT\npalavra;termo")]
    public void InvalidIgnoreCsvIsRejected(string csv) =>
        Assert.Throws<FormatException>(() => LocalWordListCsv.ParseIgnored(csv));

    [Fact]
    public void ContributionCsvCannotAccidentallyBecomeActiveRules()
    {
        var csv = VocabularyContributions.Serialize(new[] { new VocabularyContribution("teste", "", "ensaio", "", "") });
        Assert.Throws<FormatException>(() => LocalWordListCsv.ParseWords(csv));
        Assert.Throws<FormatException>(() => LocalWordListCsv.ParseIgnored(csv));
        Assert.Throws<FormatException>(() => VocabularyContributions.Parse(
            LocalWordListCsv.SerializeWords(new[] { new WordPair("teste", "ensaio") })));
    }

    [Fact]
    public void CsvCanBeSavedReopenedEditedAndSavedAgain()
    {
        var path = Path.Combine(Path.GetTempPath(), "ptpt-contributions-" + Guid.NewGuid().ToString("N") + ".csv");
        try
        {
            var entry = new VocabularyContribution("vocabulário", "uma frase; outra", "termo", "exemplo", "");
            File.WriteAllText(path, VocabularyContributions.Serialize(new[] { entry }), new UTF8Encoding(true));
            Assert.Equal(new byte[] { 0xEF, 0xBB, 0xBF }, File.ReadAllBytes(path).Take(3));
            var loaded = VocabularyContributions.Parse(File.ReadAllText(path)).ToList();
            loaded[0] = loaded[0] with { Description = "contexto revisto" };
            File.WriteAllText(path, VocabularyContributions.Serialize(loaded), new UTF8Encoding(true));
            Assert.Equal("contexto revisto", Assert.Single(VocabularyContributions.Parse(File.ReadAllText(path))).Description);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void ContributionsRoundTripAccentsQuotesSeparatorsAndMultilineExamples()
    {
        var entries = new[]
        {
            new VocabularyContribution("expressão", "Ela disse: \"olá\";\r\noutra linha", "expressão PT", "", "contexto; fonte"),
            new VocabularyContribution("expressão", "", "outro sentido", "Uma frase\noutra frase", ""),
        };
        var csv = VocabularyContributions.Serialize(entries);
        Assert.StartsWith("pt-BR;Example pt-BR;pt-PT;Example pt-PT;Description\r\n", csv);
        Assert.Contains("\"\"olá\"\"", csv);
        Assert.Equal(entries, VocabularyContributions.Parse("\uFEFF" + csv));
    }

    [Theory]
    [InlineData("=1+1")]
    [InlineData("+SUM(A1)")]
    [InlineData("-cmd")]
    [InlineData("@formula")]
    [InlineData("  =1+1")]
    [InlineData("\ttext")]
    [InlineData("\ntext")]
    [InlineData("'literal")]
    [InlineData("'=literal")]
    public void SpreadsheetFormulaProtectionIsReversible(string value)
    {
        var entry = new VocabularyContribution(value, value, value, value, value);
        var csv = VocabularyContributions.Serialize(new[] { entry });
        Assert.Contains("\"'" + value.Replace("\"", "\"\"") + "\"", csv);
        Assert.Equal(entry, Assert.Single(VocabularyContributions.Parse(csv)));
    }

    [Theory]
    [InlineData("", "word")]
    [InlineData("word", " ")]
    public void BothLanguageTermsAreRequired(string brazilian, string european)
    {
        var entry = new VocabularyContribution(brazilian, "", european, "", "");
        Assert.Throws<FormatException>(() => VocabularyContributions.Serialize(new[] { entry }));
        Assert.Throws<FormatException>(() => VocabularyContributions.Parse(
            "pt-BR;Example pt-BR;pt-PT;Example pt-PT;Description\n" + brazilian + ";;" + european + ";;"));
    }

    [Theory]
    [InlineData("wrong;columns")]
    [InlineData("pt-BR;Example pt-BR;pt-PT;Example pt-PT;Description\nword;;term;")]
    [InlineData("pt-BR;Example pt-BR;pt-PT;Example pt-PT;Description\n\"word;;term;;")]
    [InlineData("pt-BR;Example pt-BR;pt-PT;Example pt-PT;Description\n\"word\"bad;;term;;")]
    [InlineData("pt-BR;Example pt-BR;pt-PT;Example pt-PT;Description\nwo\"rd;;term;;")]
    public void InvalidCsvFailsExplicitly(string csv) =>
        Assert.Throws<FormatException>(() => VocabularyContributions.Parse(csv));

    [Fact]
    public void TemplateAndExternalCsvAllowOptionalEmptyFieldsAndIdenticalTerms()
    {
        Assert.Empty(VocabularyContributions.Parse(VocabularyContributions.Serialize(Array.Empty<VocabularyContribution>())));
        var entry = Assert.Single(VocabularyContributions.Parse(
            "pt-BR;Example pt-BR;pt-PT;Example pt-PT;Description\n\nword;;word;;\n"));
        Assert.Equal(new VocabularyContribution("word", "", "word", "", ""), entry);
    }
}
