using SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;

namespace BrazilianToEuropeanPortuguese.Tests;

public sealed class LifePageantTests
{
    [Theory]
    [InlineData("Rica trama da vida.", "Life's rich pageant.", "O rico espetáculo da vida.")]
    [InlineData("A rica trama da vida.", "Life's rich pageant.", "O rico espetáculo da vida.")]
    [InlineData("Rico desfile da vida.", "Life’s rich pageant.", "O rico espetáculo da vida.")]
    [InlineData("Faz parte da rica trama da vida.", "It's all part of life's rich pageant.", "Faz parte do rico espetáculo da vida.")]
    [InlineData("Faz parte do rico desfile da vida.", "It's all part of life's rich pageant.", "Faz parte do rico espetáculo da vida.")]
    [InlineData("É parte de uma rica trama da vida.", "It's part of life's rich pageant.", "É parte de uma rica trama da vida.")]
    [InlineData("Faz parte de toda a rica trama da vida.", "It's all part of life's rich tapestry.", "Faz parte de toda a rica trama da vida.")]
    [InlineData("Na rica trama da vida.", "In life's rich tapestry.", "No rico espetáculo da vida.")]
    [InlineData("Rica tapeçaria da vida.", "Life's rich tapestry.", "O rico espetáculo da vida.")]
    [InlineData("RICA TRAMA DA VIDA!", "Life's rich pageant!", "O RICO ESPETÁCULO DA VIDA!")]
    [InlineData("<i>Rica trama da vida.</i>", "Life's rich pageant.", "<i>O rico espetáculo da vida.</i>")]
    [InlineData("Rica trama\nda vida.", "Life's rich pageant.", "O rico espetáculo\nda vida.")]
    [InlineData("O rico espetáculo da vida.", "Life's rich pageant.", "O rico espetáculo da vida.")]
    public void ReferenceConfirmedLifeMetaphorUsesEuropeanRendering(string source, string english, string expected)
    {
        var converter = new PortugueseConverter();
        var hint = ReferenceHint.FromText(english);
        Assert.Equal(expected, converter.Convert(source, reference: hint));
        Assert.Equal(expected, converter.Convert(expected, reference: hint));
    }

    [Theory]
    [InlineData("Rica trama da vida.", null)]
    [InlineData("Rica trama da vida.", "The story of her life.")]
    [InlineData("O concurso de beleza começa.", "The beauty pageant begins.")]
    [InlineData("A tapeçaria é rica.", "The tapestry is rich.")]
    [InlineData("O desfile da vida começa.", "The pageant begins.")]
    [InlineData("A vida é rica.", "Life's rich pageant.")]
    [InlineData("São as voltas que a vida dá.", "Life's rich pageant.")]
    [InlineData("A rica comédia humana.", "Life's rich pageant.")]
    public void LiteralAndAlreadyIdiomaticTextIsPreserved(string source, string? english) =>
        Assert.Equal(source, new PortugueseConverter().Convert(source, reference: ReferenceHint.FromText(english)));

    [Fact]
    public void OptionsAndDisabledStageAreRespected()
    {
        var reference = ReferenceHint.FromText("Life's rich pageant.");
        foreach (var options in new[]
        {
            new ConversionOptions(Vocabulary: false),
            new ConversionOptions(Colloquial: false),
            new ConversionOptions(DisabledRules: "reference-idioms"),
        })
            Assert.Equal("Rica trama da vida.", new PortugueseConverter(options).Convert("Rica trama da vida.", reference: reference));
    }
}
