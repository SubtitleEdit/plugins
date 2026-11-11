using SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;
using SubtitleEdit.Plugins.Shared;

namespace BrazilianToEuropeanPortuguese.Tests;

public sealed class CollectiveAndGreetingTests
{
    [Theory]
    [InlineData("Toda equipe\nprecisa de uma mascote.", "Every team needs a mascot.", "Toda a equipa\nprecisa de uma mascote.")]
    [InlineData("Toda a equipa\nprecisa de várias mascotes.", null, "Toda a equipa\nprecisa de várias mascotes.")]
    [InlineData("A equipa\nprecisa de ajuda.", "You need help.", "A equipa\nprecisa de ajuda.")]
    [InlineData("Cada equipa\nconsegue fazer isso.", null, "Cada equipa\nconsegue fazer isso.")]
    [InlineData("Todo grupo\nmerece ter ajuda.", null, "Todo o grupo\nmerece ter ajuda.")]
    [InlineData("A família\nsabe como ajudar.", "You know how to help.", "A família\nsabe como ajudar.")]
    [InlineData("A equipa\nquer sair?", "Do you want to leave?", "A equipa\nquer sair?")]
    [InlineData("A equipa\nnão fala.", null, "A equipa\nnão fala.")]
    [InlineData("As equipas\nprecisam de ajuda.", null, "As equipas\nprecisam de ajuda.")]
    [InlineData("<i>Toda equipe\nprecisa de uma mascote.</i>", "Every team needs a mascot.",
        "<i>Toda a equipa\nprecisa de uma mascote.</i>")]
    [InlineData("TODA EQUIPE\nPRECISA DE UMA MASCOTE.", "Every team needs a mascot.",
        "TODA A EQUIPA\nPRECISA DE UMA MASCOTE.")]
    public void SubjectSurvivesSubtitleLineBreak(string source, string? english, string expected) =>
        Check(source, english, expected);

    [Theory]
    [InlineData("Precisa de ajuda.", "You need help.", "Precisas de ajuda.")]
    [InlineData("A equipa saiu.\nPrecisa de ajuda.", "You need help.", "A equipa saiu.\nPrecisas de ajuda.")]
    [InlineData("-A equipa chegou.\n-Precisa de ajuda?", "Do you need help?",
        "-A equipa chegou.\n-Precisas de ajuda?")]
    [InlineData("Tu\nprecisa de ajuda.", "You need help.", "Tu\nprecisas de ajuda.")]
    public void RealClauseAndDialogueBoundariesStillPermitTu(string source, string english, string expected) =>
        Check(source, english, expected);

    [Theory]
    [InlineData("-Danny, ei.\n-Ei, Shelly, como vai?", "Danny, hey. Hey, Shelly, how you doing?",
        "-Danny, ei.\n-Ei, Shelly, como vais?")]
    [InlineData("Ei, Ana, como está?", "Hey, Ana, how are you?", "Ei, Ana, como estás?")]
    [InlineData("Olá, João, como vai?", "Hello, João, how are you?", "Olá, João, como vais?")]
    [InlineData("Oi, Ana, como tem estado?", "Hi, Ana, how have you been?", "Olá, Ana, como tens estado?")]
    [InlineData("Ei, Ana, como anda?", "Hey, Ana, how are you doing?", "Ei, Ana, como andas?")]
    [InlineData("Ei, pessoal.", "Hey, guys. Hello. Hi.", "Ei, pessoal.")]
    [InlineData("Ei, rapazes!", "Hey, boys!", "Ei, rapazes!")]
    [InlineData("Ei, pare!", "Hey, stop!", "Ei, para!")]
    [InlineData("Ei, pessoal.", null, "Ei, pessoal.")]
    [InlineData("Olha, pessoal.", "Look, guys.", "Olha, pessoal.")]
    [InlineData("<i>Ei, Ana, como vai?</i>", "Hey, Ana, how are you?", "<i>Ei, Ana, como vais?</i>")]
    [InlineData("EI, ANA, COMO VAI?", "Hey, Ana, how are you?", "EI, ANA, COMO VAIS?")]
    public void GreetingsAreNotVisualCommands(string source, string? english, string expected) =>
        Check(source, english, expected);

    [Theory]
    [InlineData("Ei, Shelly, como vai?", "Hey, Shelly, how is he doing?")]
    [InlineData("Ei, Shelly, como vai?", null)]
    [InlineData("Ei, Shelly, como vai?", "Hey, Maria, how are you?")]
    [InlineData("Ei, Shelly, como vai o trabalho?", "Hey, Shelly, how is work?")]
    [InlineData("Ei, Shelly, como vão?", "Hey, Shelly, how are you all?")]
    [InlineData("Ei, Shelly, como vai?", "Hey, Shelly, how are you all?")]
    [InlineData("Olá, doutor, como vai?", "Hello, Doctor, how are you?")]
    public void UncertainAndFormalGreetingsRemainProtected(string source, string? english) =>
        Check(source, english, source);

    [Fact]
    public void GreetingDoesNotOverrideExplicitFormalAddress()
    {
        const string source = "Ei, Shelly, como vai?";
        var hint = ReferenceHint.FromText("Hey, Shelly, how are you?")!;
        Assert.Equal(source, new PortugueseConverter().Convert(source, reference: hint with { FormalAddress = true }));
        foreach (var options in new[] { new ConversionOptions(YouToTu: false),
            new ConversionOptions(DisabledRules: "address") })
            Assert.Equal(source, new PortugueseConverter(options).Convert(source, reference: hint));
    }

    [Fact]
    public void ArticleOptionAndIgnoredPhrasesRemainAuthoritative()
    {
        const string source = "Toda equipa precisa de ajuda.";
        Assert.Equal(source, new PortugueseConverter(new ConversionOptions(DisabledRules: "collective-articles"))
            .Convert(source));
        Assert.Equal(source, new PortugueseConverter(new ConversionOptions(Colloquial: false)).Convert(source));
        var words = new PortugueseLocalWordList();
        words.Ignored.Add("Ei, Shelly, como vai");
        Assert.Equal("Ei, Shelly, como vai?", new PortugueseConverter(localWordList: words)
            .Convert("Ei, Shelly, como vai?", reference: ReferenceHint.FromText("Hey, Shelly, how are you?")));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ActualEpisodeCuesPreserveOriginalsAndTimes(bool sentenceContext)
    {
        var blocks = new[]
        {
            new SrtBlock { StartMs = 52886, EndMs = 54321, Text = "Toda equipe\nprecisa de uma mascote." },
            new SrtBlock { StartMs = 102570, EndMs = 105740, Text = "-Danny, ei.\n-Ei, Shelly, como vai?" },
            new SrtBlock { StartMs = 106674, EndMs = 107808, Text = "Ei, pessoal." },
        };
        var originals = blocks.Select(b => (b.Text, b.StartMs, b.EndMs)).ToArray();
        var results = SubtitleConversion.Convert(blocks, new ConversionOptions(SentenceContext: sentenceContext),
            english: new string?[] { "Every team needs a mascot.",
                "WOMAN: Danny, hey. Hey, Shelly, how you doing?", "Hey, guys. Hello. Hi." });
        Assert.Equal("Toda a equipa\nprecisa de uma mascote.", results[0].Text);
        Assert.Equal("-Danny, ei.\n-Ei, Shelly, como vais?", results[1].Text);
        Assert.Equal("Ei, pessoal.", results[2].Text);
        Assert.Equal(originals, blocks.Select(b => (b.Text, b.StartMs, b.EndMs)).ToArray());
    }

    private static void Check(string source, string? english, string expected)
    {
        var converter = new PortugueseConverter();
        var reference = ReferenceHint.FromText(english);
        Assert.Equal(expected, converter.Convert(source, reference: reference));
        Assert.Equal(expected, converter.Convert(expected, reference: reference));
    }
}
