using SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;
using SubtitleEdit.Plugins.Shared;

namespace BrazilianToEuropeanPortuguese.Tests;

public sealed class FirstPluralCliticTests
{
    [Theory]
    [InlineData("-Ryan Young.\n-Nós o viramos.", "Ryan Young.\nWe flipped him.", "-Ryan Young.\n-Nós virámo-lo.")]
    [InlineData("Nós o viramos.", "We flipped him.", "Nós virámo-lo.")]
    [InlineData("Nós a compramos.", "We bought it.", "Nós comprámo-la.")]
    [InlineData("Nós os chamamos.", "We called them.", "Nós chamámo-los.")]
    [InlineData("Nós as encontramos.", "We found them.", "Nós encontrámo-las.")]
    [InlineData("Nós o viramos.", "We turn him.", "Nós viramo-lo.")]
    [InlineData("Nós o viramos.", null, "Nós viramo-lo.")]
    [InlineData("Nós o virámos.", null, "Nós virámo-lo.")]
    [InlineData("Nós o vimos.", "We saw him.", "Nós vimo-lo.")]
    [InlineData("Nós o encontramos.", "We will find him.", "Nós encontramo-lo.")]
    [InlineData("NÓS O VIRAMOS.", "We flipped him.", "NÓS VIRÁMO-LO.")]
    [InlineData("Nós os abraçamos.", "We hugged them yesterday.", "Nós abraçámo-los.")]
    [InlineData("Nós o viramos.", "We wanted to flip him.", "Nós viramo-lo.")]
    [InlineData("Nós o viramos.", "We were told to flip him.", "Nós viramo-lo.")]
    [InlineData("Nós o viramos.", "We turn him. They found it.", "Nós viramo-lo.")]
    [InlineData("Nós o viramos.", "We helped him. We turned him.", "Nós viramo-lo.")]
    [InlineData("Nós não o viramos.", "We didn't turn him.", "Nós não o viramos.")]
    [InlineData("Ela disse que nós o viramos.", "She said that we turned him.", "Ela disse que nós o viramos.")]
    public void FirstPluralObjectCliticsRespectTenseAndAttraction(string source, string? english, string expected)
    {
        var converter = new PortugueseConverter();
        var reference = ReferenceHint.FromText(english);
        Assert.Equal(expected, converter.Convert(source, reference: reference));
        Assert.Equal(expected, converter.Convert(expected, reference: reference));
    }

    [Fact]
    public void CliticConversionCanBeDisabled()
    {
        var converter = new PortugueseConverter(new ConversionOptions(Pronouns: false));
        Assert.Equal("Nós o viramos.", converter.Convert("Nós o viramos.",
            reference: ReferenceHint.FromText("We flipped him.")));
    }

    [Fact]
    public void CliticStageCanBeDisabled()
    {
        var converter = new PortugueseConverter(new ConversionOptions(DisabledRules: "pronouns"));
        Assert.Equal("Nós o viramos.", converter.Convert("Nós o viramos.",
            reference: ReferenceHint.FromText("We flipped him.")));
    }

    [Fact]
    public void TimingAlignedReferencesResolveBothReportedCases()
    {
        var source = new[]
        {
            new SrtBlock { StartMs = 1000, EndMs = 2500, Text = "-Ryan Young.\n-Nós o viramos." },
            new SrtBlock { StartMs = 4000, EndMs = 6000, Text = "Não preste atenção nestes." },
        };
        var english = new[]
        {
            new SrtBlock { StartMs = 950, EndMs = 1600, Text = "Ryan Young." },
            new SrtBlock { StartMs = 1601, EndMs = 2600, Text = "We flipped him." },
            new SrtBlock { StartMs = 4000, EndMs = 6000, Text = "Oh, don't pay any attention\nto those." },
        };
        var alignment = ReferenceAligner.AlignByTiming(source, english);
        var result = SubtitleConversion.Convert(source, new ConversionOptions(YouToTu: false), english: alignment.Texts);
        Assert.Equal("-Ryan Young.\n-Nós virámo-lo.", result[0].Text);
        Assert.Equal("Não preste atenção a esses.", result[1].Text);
        Assert.Equal("Não preste atenção nestes.", source[1].Text);
    }
}
