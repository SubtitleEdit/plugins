using SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;
using SubtitleEdit.Plugins.Shared;

namespace BrazilianToEuropeanPortuguese.Tests;

public sealed class DoorwayLocationTests
{
    [Theory]
    [InlineData("Por que há policiais\nna minha porta?", "Why are there cops at my door?", "Porque há polícias\nà minha porta?")]
    [InlineData("Há policiais na tua porta.", null, "Há polícias à tua porta.")]
    [InlineData("Há alguém na nossa porta.", null, "Há alguém à nossa porta.")]
    [InlineData("Estão na minha porta.", "They are at my door.", "Estão à minha porta.")]
    [InlineData("Eles estão na minha porta.", null, "Eles estão à minha porta.")]
    [InlineData("Há dois agentes nas nossas portas.", null, "Há dois agentes às nossas portas.")]
    [InlineData("Encontraram-se na minha porta.", "They met at my door.", "Encontraram-se à minha porta.")]
    [InlineData("HÁ POLICIAIS NA MINHA PORTA.", "There are cops at my door.", "HÁ POLÍCIAS À MINHA PORTA.")]
    [InlineData("<i>Há policiais na minha porta.</i>", "There are cops at my door.", "<i>Há polícias à minha porta.</i>")]
    public void PeopleAtTheDoorUseEuropeanLocation(string source, string? english, string expected)
    {
        var converter = new PortugueseConverter();
        var hint = ReferenceHint.FromText(english);
        Assert.Equal(expected, converter.Convert(source, reference: hint));
        Assert.Equal(expected, converter.Convert(expected, reference: hint));
    }

    [Theory]
    [InlineData("Há sangue na minha porta.")]
    [InlineData("Há marcas na minha porta.")]
    [InlineData("Está escrito na minha porta.")]
    [InlineData("O aviso está na minha porta.")]
    [InlineData("Há um buraco na tua porta.")]
    [InlineData("A chave está na minha porta.")]
    [InlineData("O desenho na minha porta.")]
    [InlineData("As dobradiças estão na minha porta.")]
    public void SurfaceAndUnresolvedUsesArePreserved(string source) =>
        Assert.Equal(source, new PortugueseConverter().Convert(source));

    [Fact]
    public void LiteralDoorSurfaceWinsOverOtherReferenceLocation()
    {
        Assert.Equal("Ele está à porta. Há sangue na minha porta.", new PortugueseConverter()
            .Convert("Ele está à porta. Há sangue na minha porta.",
                reference: ReferenceHint.FromText("He is at my door. There is blood on my door.")));
        var words = new PortugueseLocalWordList();
        words.Ignored.Add("na minha porta");
        Assert.Equal("Há polícias na minha porta.", new PortugueseConverter(localWordList: words)
            .Convert("Há policiais na minha porta.", reference: ReferenceHint.FromText("There are cops at my door.")));
    }

    [Fact]
    public void ReportedCueKeepsTimingAndRespectsOptions()
    {
        var cue = new SrtBlock { StartMs = 904687, EndMs = 906446, Text = "Por que há policiais\nna minha porta?" };
        var result = Assert.Single(SubtitleConversion.Convert(new[] { cue }, ConversionOptions.Default,
            english: new string?[] { "Why are there cops at my door?" }));
        Assert.Equal("Porque há polícias\nà minha porta?", result.Text);
        Assert.Equal(904687, cue.StartMs);
        Assert.Equal(906446, cue.EndMs);
        Assert.Equal("Há policiais à minha porta.", new PortugueseConverter(new ConversionOptions(Vocabulary: false))
            .Convert("Há policiais na minha porta.", reference: ReferenceHint.FromText("There are cops at my door.")));
        Assert.Equal("Há polícias na minha porta.", new PortugueseConverter(new ConversionOptions(Colloquial: false))
            .Convert("Há policiais na minha porta."));
        Assert.Equal("Há polícias na minha porta.", new PortugueseConverter(new ConversionOptions(DisabledRules: "construction-vocabulary"))
            .Convert("Há policiais na minha porta."));
    }
}
