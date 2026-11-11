using SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;
using SubtitleEdit.Plugins.Shared;

namespace BrazilianToEuropeanPortuguese.Tests;

public sealed class FormalDativeTests
{
    [Theory]
    [InlineData("- Ele mentiu para você.\n- Só um pouco.", "He lied to you, dummy. Just a little bit.",
        "- Ele mentiu-lhe.\n- Só um pouco.")]
    [InlineData("Ele mentiu para si.", "He lied to you.", "Ele mentiu-lhe.")]
    [InlineData("Ele não mentiu para si.", "He didn't lie to you.", "Ele não lhe mentiu.")]
    [InlineData("Sei que ele mentiu para si.", "I know he lied to you.", "Sei que ele lhe mentiu.")]
    [InlineData("Ele mentirá para si.", "He will lie to you.", "Ele mentir-lhe-á.")]
    [InlineData("Ele mentiria para si.", "He would lie to you.", "Ele mentir-lhe-ia.")]
    [InlineData("Ele não mentiria para si.", "He would not lie to you.", "Ele não lhe mentiria.")]
    [InlineData("Ele está mentindo para você.", "He is lying to you.", "Ele está a mentir-lhe.")]
    [InlineData("Ela respondeu para si.", "She replied to you.", "Ela respondeu-lhe.")]
    [InlineData("Ela telefonou para si.", "She phoned you.", "Ela telefonou-lhe.")]
    [InlineData("Ele explicou a si.", "He explained to you.", "Ele explicou-lhe.")]
    [InlineData("Ele escreveu para si.", "He wrote to you.", "Ele escreveu-lhe.")]
    [InlineData("Ele mentiu\npara si.", "He lied to you.", "Ele\nmentiu-lhe.")]
    [InlineData("Ele não mentiu\npara si.", "He didn't lie to you.", "Ele não\nlhe mentiu.")]
    [InlineData("Ele contou para si.", "He told you.", "Ele contou-lhe.")]
    [InlineData("Ele mandou para si.", "He sent you.", "Ele mandou-lhe.")]
    [InlineData("ELE MENTIU PARA SI.", "He lied to you.", "ELE MENTIU-LHE.")]
    [InlineData("<i>Ele mentiu para si.</i>", "He lied to you.", "<i>Ele mentiu-lhe.</i>")]
    public void FormalRecipientsUseExistingDativePlacement(string source, string english, string expected)
    {
        var converter = new PortugueseConverter();
        var hint = ReferenceHint.FromText(english)! with { FormalAddress = true };
        Assert.Equal(expected, converter.Convert(source, reference: hint));
        Assert.Equal(expected, converter.Convert(expected, reference: hint));
    }

    [Theory]
    [InlineData("Ele mentiu para si.", null)]
    [InlineData("Ele mentiu para si.", "He lied to himself.")]
    [InlineData("Ele mentiu para si mesmo.", "He lied to you.")]
    [InlineData("Ele mentiu para si e para ela.", "He lied to you and her.")]
    [InlineData("Ele mentiu para si e para mim.", "He lied to you and me.")]
    [InlineData("Ele mentiu para si e para si.", "He lied to you.")]
    [InlineData("Ele mentiu só para si.", "He lied only to you.")]
    [InlineData("Ele mentiu para si própria.", "He lied to you.")]
    [InlineData("Ele mentiu para si, não para ele.", "He lied to you, not to him.")]
    [InlineData("Ele pensou para si.", "He thought to himself.")]
    [InlineData("Ele mentiu para si.", "He lied to you and himself.")]
    [InlineData("Isto é para si.", "This is for you.")]
    public void ReflexiveContrastiveAndUnconfirmedUsesArePreserved(string source, string? english) =>
        Assert.Equal(source, new PortugueseConverter().Convert(source,
            reference: ReferenceHint.FromText(english) is { } hint ? hint with { FormalAddress = true } : null));

    [Fact]
    public void InformalAddressAndDisabledPronounsAreRespected()
    {
        var hint = ReferenceHint.FromText("He lied to you.")!;
        Assert.Equal("Ele mentiu-te.", new PortugueseConverter().Convert("Ele mentiu para você.", reference: hint));
        Assert.Equal("Ele mentiu a SI.", new PortugueseConverter()
            .Convert("Ele mentiu para SI.", reference: hint with { FormalAddress = true }));
        foreach (var options in new[] { new ConversionOptions(Pronouns: false),
            new ConversionOptions(DisabledRules: "pronouns") })
            Assert.Equal("Ele mentiu para si.", new PortugueseConverter(options)
                .Convert("Ele mentiu para si.", reference: hint with { FormalAddress = true }));
    }

    [Fact]
    public void AmbiguousMultipleRecipientsAreNotAssignedByCueLevelEvidence()
    {
        const string source = "Ele mentiu para si. Ela contou para si.";
        Assert.Equal(source, new PortugueseConverter().Convert(source,
            reference: ReferenceHint.FromText("He lied to you. She told herself.")! with { FormalAddress = true }));
    }

    [Fact]
    public void EpisodeCuePreservesOriginalAndTimings()
    {
        var blocks = new[] { new SrtBlock { StartMs = 1260534, EndMs = 1262422,
            Text = "- Ele mentiu para você.\n- Só um pouco." } };
        var result = SubtitleConversion.Convert(blocks, new ConversionOptions(Address: AddressMode.Formal),
            english: ["He lied to you, dummy. Just a little bit."]);
        Assert.Equal("- Ele mentiu-lhe.\n- Só um pouco.", Assert.Single(result).Text);
        Assert.Equal("- Ele mentiu para você.\n- Só um pouco.", blocks[0].Text);
        Assert.Equal(1260534, blocks[0].StartMs);
        Assert.Equal(1262422, blocks[0].EndMs);
    }
}
