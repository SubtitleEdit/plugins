using SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;

namespace BrazilianToEuropeanPortuguese.Tests;

public sealed class PoliceNounTests
{
    [Theory]
    [InlineData("Sem isso, haveria muito\nmais policiais", "Sem isso, haveria muito\nmais polícias")]
    [InlineData("Precisamos de mais policiais.", "Precisamos de mais polícias.")]
    [InlineData("Há menos policiais.", "Há menos polícias.")]
    [InlineData("Chegaram 20 policiais.", "Chegaram 20 polícias.")]
    [InlineData("MAIS POLICIAIS.", "MAIS POLÍCIAS.")]
    [InlineData("As operações policiais.", "As operações policiais.")]
    [InlineData("Mais operações policiais.", "Mais operações policiais.")]
    [InlineData("Há muitos carros policiais.", "Há muitos carros policiais.")]
    public void QuantifiersIdentifyPersonnelWithoutChangingAdjectives(string source, string expected)
    {
        var converter = new PortugueseConverter();
        Assert.Equal(expected, converter.Convert(source));
        Assert.Equal(expected, converter.Convert(expected));
    }

    [Fact]
    public void ReferenceAndUserOptionsRemainEffective()
    {
        const string source = "Há mais policiais.";
        Assert.Equal("Há mais polícias.", new PortugueseConverter().Convert(source,
            reference: ReferenceHint.FromText("There are more cops.")));
        Assert.Equal("Há mais agentes.", new PortugueseConverter().Convert(source,
            reference: ReferenceHint.FromText("There are more officers.")));
        Assert.Equal(source, new PortugueseConverter(new ConversionOptions(Vocabulary: false)).Convert(source));
        Assert.Equal(source, new PortugueseConverter(new ConversionOptions(DisabledRules: "police-nouns")).Convert(source));
        var words = new PortugueseLocalWordList();
        words.Ignored.Add("mais policiais");
        Assert.Equal(source, new PortugueseConverter(localWordList: words).Convert(source));
    }
}
