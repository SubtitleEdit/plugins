using SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;
using SubtitleEdit.Plugins.Shared;

namespace BrazilianToEuropeanPortuguese.Tests;

public sealed class ReferenceInitialAdverbTests
{
    [Theory]
    [InlineData("Realmente gosta\ndesse cara, hein?", "You really dig this guy, huh?", "Realmente gostas\ndesse tipo, não?")]
    [InlineData("Realmente gosta dele.", "You really like him.", "Realmente gostas dele.")]
    [InlineData("Simplesmente gosta dele.", "You just like him.", "Simplesmente gostas dele.")]
    [InlineData("Sempre gosta dele.", "You always like him.", "Sempre gostas dele.")]
    [InlineData("Jamais gosta disso.", "You never like that.", "Jamais gostas disso.")]
    [InlineData("Quase entende tudo.", "You almost understand everything.", "Quase percebes tudo.")]
    [InlineData("Realmente não gosta dele.", "You really don't like him.", "Realmente não gostas dele.")]
    [InlineData("Realmente já sabe disso.", "You really already know that.", "Realmente já sabes disso.")]
    [InlineData("REALMENTE GOSTA DELE.", "You really like him.", "REALMENTE GOSTAS DELE.")]
    [InlineData("<i>Realmente gosta dele.</i>", "You really like him.", "<i>Realmente gostas dele.</i>")]
    public void InitialAdverbsDoNotHideTheReferenceConfirmedAddressee(string source, string english, string expected)
    {
        var hint = ReferenceHint.FromText(english)!;
        Assert.True(hint.StartsWithSecondPersonSubject);
        var converter = new PortugueseConverter();
        Assert.Equal(expected, converter.Convert(source, reference: hint));
        Assert.Equal(expected, converter.Convert(expected, reference: hint));
    }

    [Theory]
    [InlineData("Realmente gosta dele.", null)]
    [InlineData("Realmente gosta dele.", "He really likes him.")]
    [InlineData("Realmente gosta dele.", "What happened to you?")]
    [InlineData("Realmente ele gosta de ti.", "You really think he likes you.")]
    [InlineData("Realmente Maria gosta de ti.", "You really think Maria likes you.")]
    [InlineData("Realmente a ajuda é necessária.", "You really need help.")]
    [InlineData("Realmente é impossível esperar.", "You really cannot wait.")]
    public void OtherSubjectsAndNominalOrImpersonalUsesStayUnchanged(string source, string? english) =>
        Assert.Equal(source, new PortugueseConverter().Convert(source, reference: ReferenceHint.FromText(english)));

    [Fact]
    public void AddressOptionAndDisabledStageAreRespected()
    {
        var source = "Realmente gosta dele.";
        var hint = ReferenceHint.FromText("You really like him.")!;
        foreach (var options in new[]
        {
            new ConversionOptions(YouToTu: false),
            new ConversionOptions(Address: AddressMode.Formal),
            new ConversionOptions(DisabledRules: "address"),
        })
        {
            var blocks = new[] { new SrtBlock { StartMs = 1000, EndMs = 2000, Text = source } };
            Assert.Equal(source, Assert.Single(SubtitleConversion.Convert(blocks, options,
                english: new string?[] { hint.EnglishText })).Text);
        }
    }

    [Fact]
    public void TimingAlignedCueUsesItsOwnEnglishSubject()
    {
        var blocks = new[]
        {
            new SrtBlock { StartMs = 23469, EndMs = 24974, Text = "Realmente gosta\ndesse cara, hein?" },
            new SrtBlock { StartMs = 25126, EndMs = 28555, Text = "Sim, verdadeiro, sim.\nPode parar de falar agora?" },
        };
        var english = new[]
        {
            new SrtBlock { StartMs = 23291, EndMs = 25159, Text = "You really dig\nthis guy, huh?" },
            new SrtBlock { StartMs = 25193, EndMs = 26327, Text = "Yeah, he's\nthe real deal, yeah." },
            new SrtBlock { StartMs = 26360, EndMs = 28662, Text = "Will you just...\njust stop talking now?" },
        };
        var aligned = ReferenceAligner.AlignByTiming(blocks, english);
        var conversions = SubtitleConversion.Convert(blocks, ConversionOptions.Default, english: aligned.Texts);
        Assert.Equal("Realmente gostas\ndesse tipo, não?", conversions[0].Text);
        Assert.Contains(conversions[0].Rules, rule => rule.Id == "address");
        Assert.StartsWith("Sim, verdadeiro, sim.", conversions[1].Text);
        var response = BatchMode.BuildOkResponse(blocks.ToList(), conversions.Select((cue, i) => (cue, i))
            .ToDictionary(item => item.i, item => item.cue.Text), ConversionOptions.Default);
        var output = SubRipParser.Parse(response.Subtitle!.Native);
        Assert.Equal(blocks.Select(b => (b.StartMs, b.EndMs)), output.Select(b => (b.StartMs, b.EndMs)));
    }
}
