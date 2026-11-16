using System.Text.RegularExpressions;
using SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;
using SubtitleEdit.Plugins.Shared;

namespace BrazilianToEuropeanPortuguese.Tests;

public sealed class SeeingObjectTests
{
    public static TheoryData<string, string, string> Examples => new()
    {
        { "Aparentemente,\nela não vê assim.", "Apparently, she doesn't see it that way.",
            "Aparentemente,\nela não o vê assim." },
        { "Como ela vê?", "In what way does she see it?", "Como ela o vê?" },
        { "Como vê ela?", "How does she see it?", "Como o vê ela?" },
        { "Como vê?", "How does she see it?", "Como o vê?" },
        { "Eu não vejo assim.", "I don't see it that way.", "Eu não o vejo assim." },
        { "Nós não vemos assim.", "We don't see it that way.", "Nós não o vemos assim." },
        { "Eles não veem assim.", "They don't see it that way.", "Eles não o veem assim." },
        { "Ela nunca viu assim.", "She never saw it that way.", "Ela nunca o viu assim." },
        { "Ela não via assim.", "She didn't see it that way.", "Ela não o via assim." },
        { "Ela não verá assim.", "She won't see it that way.", "Ela não o verá assim." },
        { "Ela não veria assim.", "She wouldn't see it that way.", "Ela não o veria assim." },
        { "Ela não vê dessa forma.", "She doesn't see it that way.", "Ela não o vê dessa forma." },
        { "Ela não vê desse modo.", "She doesn't see it that way.", "Ela não o vê desse modo." },
        { "Ela vê assim.", "She sees it that way.", "Ela vê-o assim." },
        { "Ela verá assim.", "She will see it that way.", "Ela vê-lo-á assim." },
        { "Ela já vê assim.", "She already sees it that way.", "Ela já o vê assim." },
        { "Como nós vimos?", "How did we see it?", "Como nós o vimos?" },
        { "Como viram eles?", "How did they see it?", "Como o viram eles?" },
        { "Como verá ela?", "How will she see it?", "Como o verá ela?" },
        { "Como veria ela?", "How would she see it?", "Como o veria ela?" },
        { "Como o senhor vê?", "How do you see it?", "Como o senhor o vê?" },
        { "<i>Como ela vê?</i>", "How does she see it?", "<i>Como ela o vê?</i>" },
        { "APARENTEMENTE, ELA NÃO VÊ ASSIM.", "Apparently, she doesn't see it that way.",
            "APARENTEMENTE, ELA NÃO O VÊ ASSIM." },
        { "-Como ela vê?\n-Não sei.", "How does she see it? I don't know.",
            "-Como ela o vê?\n-Não sei." },
    };

    [Theory]
    [MemberData(nameof(Examples))]
    public void ReferenceConfirmedObjectsUseTheRequiredPlacement(string source, string english, string expected)
    {
        var converter = new PortugueseConverter(new ConversionOptions(YouToTu: false));
        var reference = ReferenceHint.FromText(english);
        var result = converter.Convert(source, reference: reference);
        Assert.Equal(expected, result);
        Assert.Equal(result, converter.Convert(result, reference: reference));
    }

    [Theory]
    [InlineData("Como ela vê?", null)]
    [InlineData("Como ela vê?", "How well can she see?")]
    [InlineData("Ela não vê assim.", "She can't see well like this.")]
    [InlineData("Ela não vê assim tão bem.", "She doesn't see it that way.")]
    [InlineData("Ela não o vê assim.", "She doesn't see it that way.")]
    [InlineData("Como ela o vê?", "How does she see it?")]
    [InlineData("Como o vê ela?", "How does she see it?")]
    [InlineData("Ela não me vê assim.", "She doesn't see me that way.")]
    [InlineData("Ela não vê as coisas assim.", "She doesn't see it that way.")]
    [InlineData("Ela não vê isso assim.", "She doesn't see it that way.")]
    [InlineData("Ela não vê assim.", "She doesn't see him that way.")]
    [InlineData("Ela não vê assim.", "She doesn't see them that way.")]
    [InlineData("Ela não vê assim.", "She doesn't see it that way, but he sees it that way.")]
    [InlineData("Como ela vê? Como ele vê?", "How does she see it?")]
    public void MissingReferencesOtherObjectsAndEyesightAreProtected(string source, string? english) =>
        Assert.Equal(source, new PortugueseConverter(new ConversionOptions(YouToTu: false))
            .Convert(source, reference: ReferenceHint.FromText(english)));

    [Theory]
    [InlineData("Aparentemente, ela não vê assim.", "Apparently, she doesn't see it that way.")]
    [InlineData("Como ela vê?", "How does she see it?")]
    [InlineData("Como vê ela?", "How does she see it?")]
    public void EveryWordBoundaryPreservesObjectAndLineCount(string source, string english)
    {
        var converter = new PortugueseConverter();
        var reference = ReferenceHint.FromText(english);
        var expected = converter.Convert(source, reference: reference);
        for (var index = 0; index < source.Length; index++)
        {
            if (source[index] != ' ') continue;
            foreach (var newline in new[] { "\n", "\r\n" })
            {
                var result = converter.Convert(source[..index] + newline + source[(index + 1)..], reference: reference);
                Assert.Equal(expected, Regex.Replace(result, @"\s+", " "));
                Assert.Equal(1, result.Count(c => c == '\n'));
                Assert.Equal(result, converter.Convert(result, reference: reference));
            }
        }
    }

    [Fact]
    public void OptionsIgnoredTextAndCustomVocabularyRemainAuthoritative()
    {
        const string source = "Como ela vê?";
        var reference = ReferenceHint.FromText("How does she see it?");
        foreach (var options in new[] { new ConversionOptions(Pronouns: false),
            new ConversionOptions(DisabledRules: "dialogue-objects") })
            Assert.Equal(source, new PortugueseConverter(options).Convert(source, reference: reference));
        Assert.Equal("Como ela o vê?", new PortugueseConverter(new ConversionOptions(
            YouToTu: false, Vocabulary: false, Colloquial: false)).Convert(source, reference: reference));
        var words = new PortugueseLocalWordList();
        words.Ignored.Add("vê");
        Assert.Equal(source, new PortugueseConverter(localWordList: words).Convert(source, reference: reference));
        words.Ignored.Clear();
        words.Words.Add(("vê", "encara"));
        Assert.Equal("Como ela encara?", new PortugueseConverter(localWordList: words).Convert(source, reference: reference));
    }

    [Theory]
    [InlineData("Como ela o vê?", "Como ela o vê?")]
    [InlineData("Como o vê ela?", "Como o vê ela?")]
    [InlineData("Como ela vê-o?", "Como ela o vê?")]
    [InlineData("Como ela\nvê-o?", "Como ela\no vê?")]
    [InlineData("Como ele disse-lhe isso?", "Como ele lhe disse isso?")]
    public void InterrogativeSubjectsRetainProclisisWithoutAReference(string source, string expected) =>
        Assert.Equal(expected, new PortugueseConverter(new ConversionOptions(YouToTu: false)).Convert(source));

    [Fact]
    public void ExactSharedDialogueUsesAlignedReferencesAndLeavesInputsUntouched()
    {
        var blocks = new[] {
            new SrtBlock { StartMs = 148253, EndMs = 149614, Text = "Aparentemente,\nela não vê assim." },
            new SrtBlock { StartMs = 149616, EndMs = 151443, Text = "Como ela vê?" },
        };
        var result = SubtitleConversion.Convert(blocks, ConversionOptions.Default, english: new string?[] {
            "Apparently, she doesn't see it that way.", "In what way does she see it?" });
        Assert.Equal("Aparentemente,\nela não o vê assim.", result[0].Text);
        Assert.Equal("Como ela o vê?", result[1].Text);
        Assert.Equal("Aparentemente,\nela não vê assim.", blocks[0].Text);
        Assert.Equal("Como ela vê?", blocks[1].Text);
        Assert.Equal(148253, blocks[0].StartMs);
        Assert.Equal(151443, blocks[1].EndMs);
    }
}
