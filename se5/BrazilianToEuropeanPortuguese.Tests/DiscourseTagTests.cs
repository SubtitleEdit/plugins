using SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;
using SubtitleEdit.Plugins.Shared;

namespace BrazilianToEuropeanPortuguese.Tests;

public sealed class DiscourseTagTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ActualEpisodeContinuationUsesConvertedInformalCommand(bool context)
    {
        var blocks = new[]
        {
            new SrtBlock { StartMs = 85686, EndMs = 88720, Text = "Ouça, isto nunca vai ficar\nmais fácil," },
            new SrtBlock { StartMs = 88922, EndMs = 92707, Text = "mas temos que começar\na caminhar, sabe?" },
        };
        var original = blocks.Select(b => (b.Text, b.StartMs, b.EndMs)).ToArray();
        var converted = SubtitleConversion.Convert(blocks, new ConversionOptions(SentenceContext: context),
            english: new string?[] { "Look, this is... never gonna get easier,",
                "but we do kind of have to start putting one foot in front of the other, you know?" });
        Assert.Equal("Ouve, isto nunca vai ficar\nmais fácil,", converted[0].Text);
        Assert.Equal("mas temos de começar\na caminhar, sabes?", converted[1].Text);
        Assert.Contains(converted[1].Rules, r => r.Id == "discourse-tags");
        Assert.Equal(original, blocks.Select(b => (b.Text, b.StartMs, b.EndMs)).ToArray());
    }

    [Theory]
    [InlineData("Tu podes ajudar, sabe?", "You can help, you know?", "Tu podes ajudar, sabes?")]
    [InlineData("Ouve, temos de ir, sabe?", "Listen, we have to go, you know?", "Ouve, temos de ir, sabes?")]
    [InlineData("É a tua vez, entende?", "It's your turn, you understand?", "É a tua vez, percebes?")]
    [InlineData("É a tua vez, percebe?", "It's your turn, you get it?", "É a tua vez, percebes?")]
    [InlineData("É a tua vez, compreende?", null, "É a tua vez, compreendes?")]
    [InlineData("Tu podes ir, viu?", "You can go, you see?", "Tu podes ir, viste?")]
    [InlineData("Tu podes ir, ouviu?", "You can go, you hear me?", "Tu podes ir, ouviste?")]
    [InlineData("Tu podes ir, entendeu?", "You can go, did you understand?", "Tu podes ir, percebeste?")]
    [InlineData("<i>Tu podes ir, sabe?</i>", "You can go, you know?", "<i>Tu podes ir, sabes?</i>")]
    [InlineData("TU PODES IR, SABE?", "You can go, you know?", "TU PODES IR, SABES?")]
    public void DetachedTagsAgreeWithLocalInformalEvidence(string source, string? english, string expected)
    {
        var converter = new PortugueseConverter();
        var hint = ReferenceHint.FromText(english);
        Assert.Equal(expected, converter.Convert(source, reference: hint));
        Assert.Equal(expected, converter.Convert(expected, reference: hint));
    }

    [Theory]
    [InlineData("Temos de ir, sabe?", "We have to go, you know?")]
    [InlineData("Ele sabe?", "Does he know?")]
    [InlineData("É a tua vez, ele sabe?", "It's your turn, does he know?")]
    [InlineData("É a tua vez, sabe como fazer?", "It's your turn, do you know how?")]
    [InlineData("É a tua vez, sabe?", "It's your turn, he knows?")]
    [InlineData("É a tua vez, sabe?", "It's your turn, you all know?")]
    [InlineData("É a tua vez, sabe? Outra coisa.", "It's your turn, you know? Another thing.")]
    [InlineData("-É a tua vez.\n-Sabe?", "It's your turn. You know?")]
    [InlineData("Ela tem uma irmã, sabe?", "She has a sister, you know?")]
    public void AmbiguousLiteralPluralAndDialogueCasesRemainUnchanged(string text, string english) =>
        Assert.Equal(text, DiscourseTags.Apply(text, ReferenceHint.FromText(english)));

    [Theory]
    [InlineData("Ouve, isto é difícil,", "mas temos de ir, sabe?")]
    public void FormalAndDisabledSettingsWin(string previous, string text)
    {
        foreach (var options in new[] { new ConversionOptions(YouToTu: false),
            new ConversionOptions(DisabledRules: "address"), new ConversionOptions(DisabledRules: "discourse-tags") })
            Assert.DoesNotContain("sabes?", new PortugueseConverter(options).Convert(text, previous,
                ReferenceHint.FromText("We have to go, you know?")));
        var hint = ReferenceHint.FromText("We have to go, you know?")!;
        Assert.Equal(text, DiscourseTags.Apply(text, hint with { FormalAddress = true, InformalTagContinuation = true }));
    }

    [Theory]
    [InlineData(true, false, false, false)]
    [InlineData(false, true, false, false)]
    [InlineData(false, false, true, false)]
    [InlineData(false, false, false, true)]
    public void PreviousCueEvidenceDoesNotCrossUnsafeBoundaries(bool gap, bool speaker, bool selected, bool sentenceEnd)
    {
        var blocks = new[]
        {
            new SrtBlock { StartMs = 0, EndMs = 1000, Text = "Ouve, isto é difícil" + (sentenceEnd ? "." : ",") },
            new SrtBlock { StartMs = gap ? 5000 : 1100, EndMs = 6000, Text = "mas temos de ir, sabe?" },
        };
        var converted = SubtitleConversion.Convert(blocks, new ConversionOptions(),
            english: new string?[] { "Listen, this is hard,", "but we have to go, you know?" },
            speakers: speaker ? new string?[] { "A", "B" } : null,
            selected: selected ? new HashSet<int> { 1 } : null);
        Assert.Equal(blocks[1].Text, converted[1].Text);
    }

    [Fact]
    public void IgnoredTagRemainsUntouched()
    {
        var words = new PortugueseLocalWordList();
        words.Ignored.Add("sabe");
        Assert.Equal("É a tua vez, sabe?", new PortugueseConverter(localWordList: words)
            .Convert("É a tua vez, sabe?", reference: ReferenceHint.FromText("It's your turn, you know?")));
    }

    [Theory]
    [InlineData(null, "mas temos de ir, sabes?")]
    [InlineData("ouve", "mas temos de ir, sabe?")]
    public void ConvertedPreviousEvidenceWorksWithoutEnglishAndHonorsIgnoredWords(string? ignored, string expected)
    {
        var words = new PortugueseLocalWordList();
        if (ignored != null) words.Ignored.Add(ignored);
        var result = SubtitleConversion.Convert(new[]
        {
            new SrtBlock { StartMs = 0, EndMs = 1000, Text = "Ouve, isto é difícil," },
            new SrtBlock { StartMs = 1100, EndMs = 3000, Text = "mas temos de ir, sabe?" },
        }, new ConversionOptions(), words);
        Assert.Equal(expected, result[1].Text);
    }

    [Fact]
    public void DisablingTagRulePreventsPreviousCueInferenceFromChangingTheTag()
    {
        foreach (var options in new[] { new ConversionOptions(YouToTu: false),
            new ConversionOptions(DisabledRules: "address"), new ConversionOptions(DisabledRules: "discourse-tags") })
        {
            var result = SubtitleConversion.Convert(new[]
            {
                new SrtBlock { StartMs = 0, EndMs = 1000, Text = "Ouve, isto é difícil," },
                new SrtBlock { StartMs = 1100, EndMs = 3000, Text = "mas temos de ir, sabe?" },
            }, options);
            Assert.Equal("mas temos de ir, sabe?", result[1].Text);
        }
    }
}
