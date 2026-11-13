using System.Text.RegularExpressions;
using SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;
using SubtitleEdit.Plugins.Shared;

namespace BrazilianToEuropeanPortuguese.Tests;

public sealed class PorFamilyCliticTests
{
    [Theory]
    [InlineData("O prefeito interino\nse opõe ao plano.", "O presidente da câmara interino\nopõe-se ao plano.")]
    [InlineData("O presidente da câmara interino se opõe ao plano.", "O presidente da câmara interino opõe-se ao plano.")]
    [InlineData("A presidente da câmara interina se opõe ao plano.", "A presidente da câmara interina opõe-se ao plano.")]
    [InlineData("Os presidentes se opõem ao plano.", "Os presidentes opõem-se ao plano.")]
    [InlineData("O diretor se opunha ao plano.", "O diretor opunha-se ao plano.")]
    [InlineData("Os diretores se opunham ao plano.", "Os diretores opunham-se ao plano.")]
    [InlineData("O diretor se opôs ao plano.", "O diretor opôs-se ao plano.")]
    [InlineData("Os diretores se opuseram ao plano.", "Os diretores opuseram-se ao plano.")]
    [InlineData("O diretor se dispôs a ajudar.", "O diretor dispôs-se a ajudar.")]
    [InlineData("O diretor se expõe ao perigo.", "O diretor expõe-se ao perigo.")]
    [InlineData("O diretor se propõe a ajudar.", "O diretor propõe-se a ajudar.")]
    [InlineData("O diretor se impõe.", "O diretor impõe-se.")]
    [InlineData("O diretor se recompôs.", "O diretor recompôs-se.")]
    [InlineData("A equipa se compõe de três pessoas.", "A equipa compõe-se de três pessoas.")]
    [InlineData("O diretor se contrapõe ao plano.", "O diretor contrapõe-se ao plano.")]
    [InlineData("O diretor se interpõe.", "O diretor interpõe-se.")]
    [InlineData("O diretor se repôs.", "O diretor repôs-se.")]
    [InlineData("O diretor se sobrepõe.", "O diretor sobrepõe-se.")]
    [InlineData("O diretor lhe propôs um acordo.", "O diretor propôs-lhe um acordo.")]
    [InlineData("O diretor me propôs um acordo.", "O diretor propôs-me um acordo.")]
    [InlineData("O diretor o expôs.", "O diretor expô-lo.")]
    [InlineData("Eles se opõem ao plano.", "Eles opõem-se ao plano.")]
    [InlineData("Ela se opunha ao plano.", "Ela opunha-se ao plano.")]
    [InlineData("Hoje se opõem ao plano.", "Hoje opõem-se ao plano.")]
    [InlineData("E depois se recompôs.", "E depois recompôs-se.")]
    [InlineData("O PRESIDENTE DA CÂMARA INTERINO SE OPÕE AO PLANO.", "O PRESIDENTE DA CÂMARA INTERINO OPÕE-SE AO PLANO.")]
    [InlineData("<i>O presidente se opõe ao plano.</i>", "<i>O presidente opõe-se ao plano.</i>")]
    [InlineData("O presidente não opõe-se ao plano.", "O presidente não se opõe ao plano.")]
    [InlineData("O presidente nunca opôs-se ao plano.", "O presidente nunca se opôs ao plano.")]
    [InlineData("Quando ele opõe-se ao plano.", "Quando ele se opõe ao plano.")]
    [InlineData("Se ele propõe-se a ajudar.", "Se ele se propõe a ajudar.")]
    public void ReviewedPorParadigmsUseTheExistingPlacementRules(string source, string expected)
    {
        var converter = new PortugueseConverter(new ConversionOptions(YouToTu: false));
        var result = converter.Convert(source);
        Assert.Equal(expected, result);
        Assert.Equal(result, converter.Convert(result));
    }

    [Theory]
    [InlineData("O presidente não se opõe ao plano.")]
    [InlineData("O presidente já se opõe ao plano.")]
    [InlineData("Só o presidente se opõe ao plano.")]
    [InlineData("Todos se opõem ao plano.")]
    [InlineData("Que ele se oponha ao plano.")]
    [InlineData("Disse que o presidente da câmara interino se opõe ao plano.")]
    [InlineData("Quando o presidente da câmara interino se opõe ao plano.")]
    [InlineData("Se o presidente da câmara interino se opõe ao plano.")]
    [InlineData("Acredito que a minha nova equipa se dispõe a ajudar.")]
    [InlineData("Ele verifica se opõe resistência.")]
    [InlineData("Ele perguntou se expõe os quadros.")]
    [InlineData("O presidente da câmara interino opõe-se ao plano.")]
    [InlineData("Quero saber se o presidente se opôs ao plano.")]
    [InlineData("O diretor se opusesse ao plano.")]
    [InlineData("O diretor se opor ao plano.")]
    [InlineData("O diretor o posto.")]
    [InlineData("O diretor a proposta.")]
    public void AttractorsConditionalSeNonFiniteFormsAndAlreadyCorrectTextAreProtected(string source) =>
        Assert.Equal(source, new PortugueseConverter(new ConversionOptions(YouToTu: false)).Convert(source));

    [Theory]
    [InlineData("O presidente da câmara interino se opõe ao plano.")]
    [InlineData("Disse que o presidente da câmara interino se opõe ao plano.")]
    [InlineData("O presidente não opõe-se ao plano.")]
    [InlineData("O diretor lhe propôs um acordo.")]
    public void EveryWordBoundaryPreservesPlacementAndLineCount(string source)
    {
        var converter = new PortugueseConverter(new ConversionOptions(YouToTu: false));
        var expected = converter.Convert(source);
        for (var index = 0; index < source.Length; index++)
        {
            if (source[index] != ' ') continue;
            foreach (var newline in new[] { "\n", "\r\n" })
            {
                var result = converter.Convert(source[..index] + newline + source[(index + 1)..]);
                Assert.Equal(expected, Regex.Replace(result, @"\s+", " "));
                Assert.Equal(1, result.Count(c => c == '\n'));
                Assert.Equal(result, converter.Convert(result));
            }
        }
    }

    [Fact]
    public void OptionsIgnoredWordsAndCustomWordsRemainAuthoritative()
    {
        const string source = "O presidente se opõe ao plano.";
        foreach (var options in new[] { new ConversionOptions(Pronouns: false),
            new ConversionOptions(DisabledRules: "pronouns") })
            Assert.Equal(source, new PortugueseConverter(options).Convert(source));
        var words = new PortugueseLocalWordList();
        words.Ignored.Add("opõe");
        Assert.Equal(source, new PortugueseConverter(localWordList: words).Convert(source));
        words.Ignored.Clear();
        words.Words.Add(("opõe", "contrapõe"));
        Assert.Equal("O presidente contrapõe-se ao plano.",
            new PortugueseConverter(localWordList: words).Convert(source));
    }

    [Fact]
    public void ExactSharedSubtitleKeepsAccentsTimingAndInput()
    {
        var block = new SrtBlock { StartMs = 135636, EndMs = 139191, Text = "O prefeito interino\nse opõe ao plano." };
        var result = SubtitleConversion.Convert([block], ConversionOptions.Default,
            english: new string?[] { "The interim mayor opposes the plan." });
        Assert.Equal("O presidente da câmara interino\nopõe-se ao plano.", result[0].Text);
        Assert.Equal("O prefeito interino\nse opõe ao plano.", block.Text);
        Assert.Equal(135636, block.StartMs);
        Assert.Equal(139191, block.EndMs);
    }
}
