using SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;

namespace BrazilianToEuropeanPortuguese.Tests;

public sealed class NounSubjectDirectCliticTests
{
    [Theory]
    [InlineData("A família a registrou\nno acampamento,", "A família registou-a\nno acampamento,")]
    [InlineData("A família a registou no acampamento.", "A família registou-a no acampamento.")]
    [InlineData("A família o registou.", "A família registou-o.")]
    [InlineData("A família os registou.", "A família registou-os.")]
    [InlineData("A família as registou.", "A família registou-as.")]
    [InlineData("A minha família a registou.", "A minha família registou-a.")]
    [InlineData("Maria a viu.", "Maria viu-a.")]
    [InlineData("O professor o ouviu.", "O professor ouviu-o.")]
    [InlineData("O professor a vê.", "O professor vê-a.")]
    [InlineData("Os pais a encontraram.", "Os pais encontraram-na.")]
    [InlineData("A FAMÍLIA A REGISTOU.", "A FAMÍLIA REGISTOU-A.")]
    [InlineData("<i>A família a registou.</i>", "<i>A família registou-a.</i>")]
    public void NounSubjectsPlaceDirectObjectsAfterValidatedFiniteVerbs(string source, string expected)
    {
        var converter = new PortugueseConverter();
        Assert.Equal(expected, converter.Convert(source));
        Assert.Equal(expected, converter.Convert(expected));
    }

    [Theory]
    [InlineData("A família não a registou.")]
    [InlineData("A família já a registou.")]
    [InlineData("Se a família a registou.")]
    [InlineData("Disse que a família a registou.")]
    [InlineData("Quem a registou?")]
    [InlineData("A família a registará.")]
    [InlineData("A família a registaria.")]
    [InlineData("A família a casa.")]
    [InlineData("Maria a ajuda financeira.")]
    [InlineData("É bom o ver.")]
    [InlineData("Ele aparafusou a peça.")]
    [InlineData("Esse é o jogo errado.")]
    [InlineData("Sinto o cheiro.")]
    [InlineData("venceu o teste de ADN?")]
    [InlineData("venceram o teste de ADN?")]
    public void ArticlesAttractorsAndNonFiniteOrFutureUsesStayUnchanged(string source) =>
        Assert.Equal(source, new PortugueseConverter().Convert(source));

    [Fact]
    public void PronounOptionAndStageControlTheRule()
    {
        const string source = "A família a registou.";
        Assert.Equal(source, new PortugueseConverter(new ConversionOptions(Pronouns: false)).Convert(source));
        Assert.Equal(source, new PortugueseConverter(new ConversionOptions(DisabledRules: "pronouns")).Convert(source));
    }
}
