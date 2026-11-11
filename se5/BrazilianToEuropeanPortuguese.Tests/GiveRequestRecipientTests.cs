using SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;
using SubtitleEdit.Plugins.Shared;

namespace BrazilianToEuropeanPortuguese.Tests;

public sealed class GiveRequestRecipientTests
{
    [Theory]
    [InlineData(true, "Rapazes, deem-me um segundo,\ncerto?")]
    [InlineData(false, "Rapazes, dêem-me um segundo,\ncerto?")]
    public void ActualPluralRequestKeepsPluralAndSelectedOrthography(bool postAgreement, string expected)
    {
        const string source = "Garotos, dêem um segundo,\ncerto?";
        var converter = new PortugueseConverter(new ConversionOptions(PostAgreement: postAgreement));
        var hint = ReferenceHint.FromText("Boys, give me a second, okay? See you guys later. Bye. Bye.");
        var trace = new List<RuleChange>();
        Assert.Equal(expected, converter.Convert(source, reference: hint, trace: trace));
        Assert.Equal(expected, converter.Convert(expected, reference: hint));
        Assert.Contains(trace, r => r.Id == "give-request-recipient");
    }

    [Theory]
    [InlineData("Dá um minuto.", "Give me a minute.", "Dá-me um minuto.")]
    [InlineData("Dê um momento.", "Give me a moment.", "Dê-me um momento.")]
    [InlineData("Deem dois segundos.", "Give us two seconds.", "Deem-nos dois segundos.")]
    [InlineData("Dai uma oportunidade.", "Give me a chance.", "Dai-me uma oportunidade.")]
    [InlineData("Pessoal, deem uma mão.", "Guys, give us a hand.", "Pessoal, deem-nos uma mão.")]
    [InlineData("Dê um pouco de tempo.", "Give me some time.", "Dê-me um pouco de tempo.")]
    [InlineData("Dê mais tempo.", "Give me some more time.", "Dê-me mais tempo.")]
    [InlineData("Dê uma pausa.", "Give me a break.", "Dê-me uma pausa.")]
    [InlineData("Dê uma ajuda.", "Give me some help.", "Dê-me uma ajuda.")]
    [InlineData("Dê a sua atenção.", "Give me your attention.", "Dê-me a sua atenção.")]
    [InlineData("Ana, dê um segundo.", "Ana, give me a second.", "Ana, dê-me um segundo.")]
    [InlineData("Por favor, dê um minuto.", "Please, give me a minute.", "Por favor, dê-me um minuto.")]
    [InlineData("DEEM UM SEGUNDO.", "Give us a second.", "DEEM-NOS UM SEGUNDO.")]
    public void ReferenceRecoversRecipientWithoutChangingPerson(string source, string english, string expected)
    {
        Assert.Equal(expected, ConversionConstructions.GiveRequestRecipients(source, english));
        Assert.Equal(expected, ConversionConstructions.GiveRequestRecipients(expected, english));
    }

    [Theory]
    [InlineData("Deem um segundo.", null)]
    [InlineData("Deem um segundo.", "Give him a second.")]
    [InlineData("Deem um segundo.", "Don't give me a second.")]
    [InlineData("Deem um segundo.", "He said give me a second.")]
    [InlineData("Ele dê um segundo.", "Give me a second.")]
    [InlineData("Se ele dê um segundo.", "Give me a second.")]
    [InlineData("Não dê um segundo.", "Give me a second.")]
    [InlineData("Deem um segundo a ele.", "Give me a second.")]
    [InlineData("Deem um segundo para pensar.", "Give me a second.")]
    [InlineData("Deem-me um segundo.", "Give me a second.")]
    [InlineData("Deem-lhe um segundo.", "Give me a second.")]
    [InlineData("Deem um minuto.", "Give me a second.")]
    [InlineData("Deem um relógio.", "Give me a watch.")]
    [InlineData("Deem um segundo. Deem um segundo.", "Give me a second.")]
    [InlineData("Deem um segundo.", "Give me a second. Give us a second.")]
    [InlineData("Deem um segundo.", "Can you give me a second?")]
    public void AmbiguousLiteralAndAlreadySuppliedRecipientsAreProtected(string text, string? english) =>
        Assert.Equal(text, ConversionConstructions.GiveRequestRecipients(text, english));

    [Fact]
    public void FormalAddressDoesNotRemoveTheEnglishRecipient()
    {
        var hint = ReferenceHint.FromText("Give me a second.")! with { FormalAddress = true };
        Assert.Equal("Dê-me um segundo.", new PortugueseConverter().Convert("Dê um segundo.", reference: hint));
        Assert.Equal("Deem-me um segundo.", new PortugueseConverter().Convert("Deem um segundo.", reference: hint));
    }

    [Fact]
    public void MarkupIgnoredPhrasesAndDisabledRulesAreRespected()
    {
        const string text = "Deem um segundo.";
        var hint = ReferenceHint.FromText("Give me a second.");
        Assert.Equal("<i>Deem-me um segundo.</i>", new PortugueseConverter().Convert("<i>Deem um segundo.</i>", reference: hint));
        foreach (var options in new[] { new ConversionOptions(Pronouns: false),
            new ConversionOptions(DisabledRules: "give-request-recipient") })
            Assert.Equal(text, new PortugueseConverter(options).Convert(text, reference: hint));
        var words = new PortugueseLocalWordList();
        words.Ignored.Add("deem um segundo");
        Assert.Equal(text, new PortugueseConverter(localWordList: words).Convert(text, reference: hint));
    }

    [Theory]
    [InlineData(true, "deem", "deem")]
    [InlineData(true, "dêem", "deem")]
    [InlineData(false, "deem", "dêem")]
    [InlineData(false, "dêem", "dêem")]
    public void DarPluralSpellingIsAuthoritativeEvenWhenDictionaryAcceptsBoth(bool post, string word, string expected) =>
        Assert.Equal(expected + "-me.", new PortugueseConverter(new ConversionOptions(PostAgreement: post))
            .Convert(word + "-me."));

    [Fact]
    public void SourceAndCueTimingsRemainUnchanged()
    {
        var block = new SrtBlock { StartMs = 123438, EndMs = 125591, Text = "Garotos, dêem um segundo,\ncerto?" };
        var result = SubtitleConversion.Convert(new[] { block }, new ConversionOptions(),
            english: new string?[] { "Boys, give me a second, okay? See you guys later. Bye. Bye." });
        Assert.Equal("Rapazes, deem-me um segundo,\ncerto?", result[0].Text);
        Assert.Equal("Garotos, dêem um segundo,\ncerto?", block.Text);
        Assert.Equal(123438, block.StartMs);
        Assert.Equal(125591, block.EndMs);
    }
}
