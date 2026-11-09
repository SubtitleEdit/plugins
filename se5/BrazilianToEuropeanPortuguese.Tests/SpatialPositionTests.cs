using SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;
using SubtitleEdit.Plugins.Shared;

namespace BrazilianToEuropeanPortuguese.Tests;

public sealed class SpatialPositionTests
{
    [Theory]
    [InlineData("Logo atrás de você.\nPolícia.", "Right behind you. Police.", "Estou mesmo atrás de ti.\nPolícia.")]
    [InlineData("Estou logo atrás de você.", "I'm right behind you.", "Estou mesmo atrás de ti.")]
    [InlineData("Estou logo atrás de ti.", "I'm just behind you.", "Estou mesmo atrás de ti.")]
    [InlineData("Estamos logo atrás de você.", "We're right behind you.", "Estamos mesmo atrás de ti.")]
    [InlineData("Ela está logo atrás de mim.", "She's right behind me.", "Ela está mesmo atrás de mim.")]
    [InlineData("Estava logo atrás dele.", "I was right behind him.", "Estava mesmo atrás dele.")]
    [InlineData("Logo à frente de você.", "Right in front of you.", "Estou mesmo à frente de ti.")]
    [InlineData("Estou logo ao lado de você.", "I'm right next to you.", "Estou mesmo ao lado de ti.")]
    [InlineData("LOGO ATRÁS DE VOCÊ.", "Right behind you.", "ESTOU MESMO ATRÁS DE TI.")]
    [InlineData("<i>Logo atrás de você.</i>", "Right behind you.", "<i>Estou mesmo atrás de ti.</i>")]
    public void CloseSpatialPositionsAreReferenceConfirmed(string source, string english, string expected)
    {
        var reference = ReferenceHint.FromText(english);
        var converter = new PortugueseConverter();
        Assert.Equal(expected, converter.Convert(source, reference: reference));
        Assert.Equal(expected, converter.Convert(expected, reference: reference));
    }

    [Theory]
    [InlineData("Vou logo atrás dele.", "I'll go after him soon.")]
    [InlineData("Logo atrás dele.", "Right behind him.")]
    [InlineData("Logo atrás de ti.", "I'll be right behind you.")]
    [InlineData("Logo atrás de ti.", "Right in front of you.")]
    [InlineData("Logo atrás de ti.", "Right behind me.")]
    [InlineData("Logo atrás de ti.", null)]
    [InlineData("Logo atrás de ti.", "Right behind you and right behind me.")]
    [InlineData("Chego logo.", "I'll be there soon.")]
    public void TemporalOrUncertainPositionsArePreserved(string source, string? english) =>
        Assert.Equal(source, new PortugueseConverter().Convert(source, reference: ReferenceHint.FromText(english)));

    [Fact]
    public void FormalAddressAndOptionsAreRespected()
    {
        const string source = "Logo atrás de você.";
        var reference = ReferenceHint.FromText("Right behind you.")!;
        Assert.Equal("Estou mesmo atrás de si.", new PortugueseConverter().Convert(source,
            reference: reference with { FormalAddress = true }));
        Assert.Equal("Estou mesmo atrás de você.", new PortugueseConverter(new ConversionOptions(YouToTu: false))
            .Convert(source, reference: reference));
        Assert.Equal("Logo atrás de ti.", new PortugueseConverter(new ConversionOptions(Colloquial: false))
            .Convert(source, reference: reference));
        Assert.Equal("Logo atrás de ti.", new PortugueseConverter(new ConversionOptions(DisabledRules: "spatial-position"))
            .Convert(source, reference: reference));
        var list = new PortugueseLocalWordList();
        list.Ignored.Add("Logo atrás de você");
        Assert.Equal(source, new PortugueseConverter(localWordList: list).Convert(source, reference: reference));
    }

    [Fact]
    public void SubtitlePipelinePreservesPoliceCueAndTimings()
    {
        var source = SubRipParser.Parse("70\n00:03:10,000 --> 00:03:12,000\nLogo atrás de você.\nPolícia.\n\n");
        var result = SubtitleConversion.Convert(source, new ConversionOptions(),
            english: new string?[] { "Right behind you. Police." });
        Assert.Equal("Estou mesmo atrás de ti.\nPolícia.", result[0].Text);
        Assert.Equal("Logo atrás de você.\nPolícia.", source[0].Text);
        Assert.Equal(190000L, source[0].StartMs);
        Assert.Equal(192000L, source[0].EndMs);
    }
}
