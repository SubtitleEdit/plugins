using SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;
using SubtitleEdit.Plugins.Shared;

namespace BrazilianToEuropeanPortuguese.Tests;

public sealed class PersonalObjectTests
{
    [Theory]
    [InlineData("Seu pai bateu você.", "O teu pai bateu-te.")]
    [InlineData("Ele bate você.", "Ele bate-te.")]
    [InlineData("Ele batia você.", "Ele batia-te.")]
    [InlineData("Ele vai bater você.", "Ele vai bater-te.")]
    [InlineData("Ele não bateu você.", "Ele não te bateu.")]
    [InlineData("O homem que bateu você.", "O homem que te bateu.")]
    [InlineData("Sei que o teu pai bateu você.", "Sei que o teu pai te bateu.")]
    [InlineData("Ele bateu ele.", "Ele bateu-o.")]
    [InlineData("Ele bateu ela.", "Ele bateu-a.")]
    [InlineData("Não bateu eles.", "Não os bateu.")]
    [InlineData("Ele bateu em você e na sua mãe.", "Ele bateu em ti e na sua mãe.")]
    [InlineData("Bateu em ti.", "Bateu em ti.")]
    [InlineData("Ele bateu à porta.", "Ele bateu à porta.")]
    [InlineData("Ele bateu o recorde.", "Ele bateu o recorde.")]
    [InlineData("Você bateu à porta.", "Bateste à porta.")]
    [InlineData("Bateu você mesmo?", "Bateu tu mesmo?")]
    [InlineData("<i>Seu pai bateu você.</i>", "<i>O teu pai bateu-te.</i>")]
    public void PersonalObjectsPreserveGovernmentAndOtherSenses(string source, string expected)
    {
        var converter = new PortugueseConverter();
        Assert.Equal(expected, converter.Convert(source));
        Assert.Equal(expected, converter.Convert(expected));
    }

    [Fact]
    public void ReportedCuePreservesOriginalAndTiming()
    {
        var blocks = SubRipParser.Parse("1\n00:18:30,310 --> 00:18:31,452\nSeu pai bateu você.\n\n" +
            "2\n00:18:31,464 --> 00:18:33,139\nBateu em você\ne na sua mãe,\n\n");
        var results = SubtitleConversion.Convert(blocks, new ConversionOptions(),
            english: new string?[] { "Your dad beat you.", "He beat you and your mother," });
        Assert.Equal("O teu pai bateu-te.", results[0].Text);
        Assert.Equal("Bateu em ti\ne na tua mãe,", results[1].Text);
        Assert.Equal("Seu pai bateu você.", blocks[0].Text);
        Assert.Equal(1110310L, blocks[0].StartMs);
        Assert.Equal(1113139L, blocks[1].EndMs);
    }

    [Theory]
    [InlineData("Ele atingiu você.", "Ele atingiu-te.")]
    [InlineData("Ele surrou você.", "Ele surrou-te.")]
    [InlineData("Ele livrou você do perigo.", "Ele livrou-te do perigo.")]
    [InlineData("Ele liberou você.", "Ele liberou-te.")]
    [InlineData("Ele fichou você.", "Ele fichou-te.")]
    [InlineData("Ele não recusou você.", "Ele não te recusou.")]
    [InlineData("Ele considerou você inocente.", "Ele considerou-te inocente.")]
    [InlineData("Ele honrou você.", "Ele honrou-te.")]
    [InlineData("Ele contactou você.", "Ele contactou-te.")]
    [InlineData("Eu aguento você.", "Eu aguento-te.")]
    [InlineData("Ele espancou você.", "Ele espancou-te.")]
    [InlineData("Ele socou você.", "Ele socou-te.")]
    [InlineData("Ele feriu você.", "Ele feriu-te.")]
    [InlineData("Ele bateu em você.", "Ele bateu em ti.")]
    [InlineData("Ele levou você para casa.", "Ele levou-te para casa.")]
    [InlineData("Ele surrou você como vingança.", "Ele surrou-te como vingança.")]
    [InlineData("Ele lida com você.", "Ele lida contigo.")]
    [InlineData("Ele suspeita de você.", "Ele suspeita de ti.")]
    public void DictionaryReviewedPersonalVerbsUseExistingCliticPlacement(string source, string expected)
    {
        var converter = new PortugueseConverter();
        Assert.Equal(expected, converter.Convert(source));
        Assert.Equal(expected, converter.Convert(expected));
    }

    [Fact]
    public void OptionsAndIgnoredFormsKeepPriority()
    {
        const string source = "Seu pai bateu você.";
        Assert.Equal("O seu pai bateu você.", new PortugueseConverter(new ConversionOptions(YouToTu: false)).Convert(source));
        Assert.Equal("O seu pai bateu-o.", new PortugueseConverter().Convert(source,
            reference: ReferenceHint.FromText("Your dad beat you.")! with { FormalAddress = true }));
        var local = new PortugueseLocalWordList();
        local.Ignored.Add("bateu você");
        Assert.Equal("Teu pai bateu você.", new PortugueseConverter(localWordList: local).Convert(source));
    }
}
