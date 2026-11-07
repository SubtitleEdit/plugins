using SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;

namespace BrazilianToEuropeanPortuguese.Tests;

public sealed class LinkedInfinitiveTests
{
    [Theory]
    [InlineData("mas me derrubar", "mas derrubar-me")]
    [InlineData("mas me ajudar", "mas ajudar-me")]
    [InlineData("mas te proteger", "mas proteger-te")]
    [InlineData("a se vestir", "a vestir-se")]
    [InlineData("mas nos ouvir", "mas ouvir-nos")]
    [InlineData("mas vos dizer", "mas dizer-vos")]
    [InlineData("mas lhe oferecer", "mas oferecer-lhe")]
    [InlineData("mas lhes permitir", "mas permitir-lhes")]
    [InlineData("mas me pôr", "mas pôr-me")]
    [InlineData("depois se compor", "depois compor-se")]
    [InlineData("mas nos vermos", "mas vermo-nos")]
    [InlineData("mas me verem", "mas verem-me")]
    [InlineData("mas te protegeres", "mas protegeres-te")]
    [InlineData("mas me fotografar", "mas fotografar-me")]
    [InlineData("MAS ME DERRUBAR", "MAS DERRUBAR-ME")]
    [InlineData("mas\nme derrubar", "mas\nderrubar-me")]
    [InlineData("<i>mas me derrubar</i>", "<i>mas derrubar-me</i>")]
    public void LinkedInfinitivesUseEnclisis(string source, string expected)
    {
        var converter = new PortugueseConverter();
        Assert.Equal(expected, converter.Convert(source));
        Assert.Equal(expected, converter.Convert(expected));
    }

    [Theory]
    [InlineData("mas não me derrubar")]
    [InlineData("mas nunca me ajudar")]
    [InlineData("mas que me ajudem")]
    [InlineData("E se deixarmos isso só entre nós?")]
    [InlineData("mas se protegerem a entrada")]
    [InlineData("mas se vestir")]
    [InlineData("mas se compor")]
    [InlineData("E se precisar de um ombro para chorar...")]
    [InlineData("e se comprar uma casa")]
    [InlineData("mas se conseguir entrar")]
    [InlineData("ou se encontrar trabalho")]
    [InlineData("mas nos lugares")]
    [InlineData("mas nos bares")]
    [InlineData("mas me inventarverbo")]
    public void NonInfinitivesAndAttractorsArePreserved(string source) =>
        Assert.Equal(source, new PortugueseConverter().Convert(source));

    [Fact]
    public void PronounOptionControlsTheCorrection() =>
        Assert.Equal("mas me derrubar",
            new PortugueseConverter(new ConversionOptions(Pronouns: false)).Convert("mas me derrubar"));

    [Fact]
    public void FiniteVerbKeepsExistingEnclisisBehaviour() =>
        Assert.Equal("mas parece-me", new PortugueseConverter().Convert("mas me parece"));
}
