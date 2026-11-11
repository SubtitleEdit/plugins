using SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;
using SubtitleEdit.Plugins.Shared;

namespace BrazilianToEuropeanPortuguese.Tests;

public sealed class PerfectScoreTests
{
    [Theory]
    [InlineData("Na verdade, gabaritei.", "Actually, I nailed it.", "Na verdade, acertei em tudo.")]
    [InlineData("Eu gabaritei a prova.", "I aced the test.", "Eu acertei em tudo na prova.")]
    [InlineData("Ela gabaritou o exame.", null, "Ela acertou em tudo no exame.")]
    [InlineData("Gabaritamos as provas.", null, "Acertamos em tudo nas provas.")]
    [InlineData("Gabaritámos os testes.", null, "Acertámos em tudo nos testes.")]
    [InlineData("Gabaritar este exame.", null, "Acertar em tudo neste exame.")]
    [InlineData("Ele gabaritará esse teste.", null, "Ele acertará em tudo nesse teste.")]
    [InlineData("Ela gabaritaria uma prova.", null, "Ela acertaria em tudo numa prova.")]
    [InlineData("Eu gabaritei\nas questões.", null, "Eu acertei\ntodas as questões.")]
    [InlineData("Gabaritou todas as perguntas.", null, "Acertou todas as perguntas.")]
    [InlineData("Eu não gabaritei.", null, "Eu não acertei em tudo.")]
    [InlineData("Eu gabarito.", null, "Eu acerto em tudo.")]
    [InlineData("Tu gabaritas.", null, "Tu acertas em tudo.")]
    [InlineData("Ela gabaritava.", null, "Ela acertava em tudo.")]
    [InlineData("Eles gabaritavam.", null, "Eles acertavam em tudo.")]
    [InlineData("Eu gabaritarei.", null, "Eu acertarei em tudo.")]
    [InlineData("Nós gabaritaríamos.", null, "Nós acertaríamos em tudo.")]
    [InlineData("Se eu gabaritasse.", null, "Se eu acertasse em tudo.")]
    [InlineData("Se nós gabaritássemos.", null, "Se nós acertássemos em tudo.")]
    [InlineData("Se eles gabaritarem.", null, "Se eles acertarem em tudo.")]
    [InlineData("Vós gabaritais.", null, "Vós acertais em tudo.")]
    [InlineData("Para gabaritarem.", null, "Para acertarem em tudo.")]
    [InlineData("Tenho gabaritado.", null, "Tenho acertado em tudo.")]
    [InlineData("O exame foi gabaritado.", null, "O exame foi resolvido sem erros.")]
    [InlineData("As provas foram gabaritadas.", null, "As provas foram resolvidas sem erros.")]
    [InlineData("Gabaritando.", null, "A acertar em tudo.")]
    [InlineData("GABARITEI O TESTE.", null, "ACERTEI EM TUDO NO TESTE.")]
    [InlineData("<i>Gabaritei.</i>", null, "<i>Acertei em tudo.</i>")]
    [InlineData("O gabarito da prova.", null, "As soluções da prova.")]
    [InlineData("Os gabaritos dos testes.", null, "As soluções dos testes.")]
    public void ExamAchievementAndAnswerKeys(string source, string? english, string expected)
    {
        var converter = new PortugueseConverter();
        var hint = ReferenceHint.FromText(english);
        Assert.Equal(expected, converter.Convert(source, reference: hint));
        Assert.Equal(expected, converter.Convert(expected, reference: hint));
    }

    [Theory]
    [InlineData("O gabarito de madeira.")]
    [InlineData("O gabarito está aqui.")]
    [InlineData("Ele tem gabarito.")]
    [InlineData("Não tenho gabarito.")]
    [InlineData("Gabarito de madeira.")]
    [InlineData("Profissional de gabarito.")]
    [InlineData("Ele gabaritou o candidato.")]
    [InlineData("Ele gabaritou para o cargo.")]
    [InlineData("O técnico está gabaritado para o cargo.")]
    [InlineData("O técnico está gabaritado.")]
    [InlineData("O meu gabarito da prova.")]
    [InlineData("Pelo gabarito da prova.")]
    [InlineData("A gabaritada.")]
    [InlineData("gabaritei_ontem")]
    [InlineData("microgabaritei")]
    [InlineData("Eu acertei as perguntas.")]
    [InlineData("Gabaritar-me.")]
    public void OtherSensesAndUnrelatedWordsArePreserved(string source) =>
        Assert.Equal(source, new PortugueseConverter().Convert(source));

    [Fact]
    public void OptionsAndLocalExceptionsRemainAuthoritative()
    {
        foreach (var options in new[] { new ConversionOptions(Vocabulary: false),
            new ConversionOptions(DisabledRules: "reviewed-verb-senses") })
            Assert.Equal("Gabaritei.", new PortugueseConverter(options).Convert("Gabaritei."));
        var words = new PortugueseLocalWordList();
        words.Ignored.Add("gabaritar");
        words.Ignored.Add("gabarito");
        Assert.Equal("Gabaritei.", new PortugueseConverter(localWordList: words).Convert("Gabaritei."));
        Assert.Equal("O gabarito da prova.", new PortugueseConverter(localWordList: words).Convert("O gabarito da prova."));
        words.Words.Add(("gabaritei", "brilhei"));
        Assert.Equal("Brilhei.", new PortugueseConverter(localWordList: words).Convert("Gabaritei."));
        Assert.Equal("Gabaritei.", new PortugueseConverter().Convert("Gabaritei.",
            reference: ReferenceHint.FromText("I qualified for the job.")));
    }

    [Fact]
    public void EpisodeCueRetainsTimingAndOriginal()
    {
        var blocks = new[] { new SrtBlock { StartMs = 2489711, EndMs = 2492103, Text = "Na verdade, gabaritei." } };
        var result = SubtitleConversion.Convert(blocks, new ConversionOptions(), english: ["Actually, I nailed it."]);
        Assert.Equal("Na verdade, acertei em tudo.", Assert.Single(result).Text);
        Assert.Equal("Na verdade, gabaritei.", blocks[0].Text);
        Assert.Equal(2489711, blocks[0].StartMs);
        Assert.Equal(2492103, blocks[0].EndMs);
    }
}
