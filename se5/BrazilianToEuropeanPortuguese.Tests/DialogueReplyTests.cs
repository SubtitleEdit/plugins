using SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;
using SubtitleEdit.Plugins.Shared;

namespace BrazilianToEuropeanPortuguese.Tests;

public sealed class DialogueReplyTests
{
    [Theory]
    [InlineData("Não. Disse que\niria me deixar.", "No. You said you were gonna drop me off.",
        "Não. Disseste que\nme ias deixar.")]
    [InlineData("Não. Disse que iria me ajudar.", "No. You said you were going to help me.",
        "Não. Disseste que me ias ajudar.")]
    [InlineData("Sim. Disse que iria voltar.", "Yes. You said you were going to return.",
        "Sim. Disseste que ias voltar.")]
    [InlineData("Não. Disse que poderia ajudar.", "No. You said you could help.",
        "Não. Disseste que poderias ajudar.")]
    [InlineData("Não. Disse que iria me ajudar.", "No. You said you would help me.",
        "Não. Disseste que me irias ajudar.")]
    [InlineData("Disse que iria me proteger.", "You said you would protect me.",
        "Disseste que me irias proteger.")]
    [InlineData("Não. Disse que ela iria voltar.", "No. You said she would return.",
        "Não. Disseste que ela iria voltar.")]
    public void ReplyAndReportedClauseUseConfirmedSubjects(string source, string english, string expected)
    {
        var converter = new PortugueseConverter();
        var reference = ReferenceHint.FromText(english);
        Assert.Equal(expected, converter.Convert(source, reference: reference));
        Assert.Equal(expected, converter.Convert(expected, reference: reference));
    }

    [Theory]
    [InlineData("Ligue para João?", "Call John?", "Ligo para o João?")]
    [InlineData("Ligue para Jamie?", "Call Jamie?", "Ligo para o Jamie?")]
    [InlineData("Telefone para Maria?", "Phone Maria?", "Telefono para a Maria?")]
    [InlineData("Pergunte ao João?", "Ask John?", "Pergunto ao João?")]
    [InlineData("Espere aqui?", "Wait here?", "Espero aqui?")]
    [InlineData("Faça isso?", "Do that?", "Faço isso?")]
    [InlineData("Vá para casa?", "Go home?", "Vou para casa?")]
    [InlineData("Ligue para o João?", "Call John?", "Ligo para o João?")]
    [InlineData("Liga para João?", "Call John?", "Ligo para o João?")]
    [InlineData("Ligue para Alex?", "Call Alex?", "Ligo para Alex?")]
    public void ConditionalReplyQuestionAsksAboutOwnAction(string source, string english, string expected)
    {
        var blocks = SubRipParser.Parse("1\n00:00:01,000 --> 00:00:03,000\nE se precisar de ajuda...\n\n" +
            "2\n00:00:03,010 --> 00:00:05,000\n" + source + "\n\n");
        var result = SubtitleConversion.Convert(blocks, new ConversionOptions(),
            english: new string?[] { "And if you need help...", english });
        Assert.Equal(expected, result[1].Text);
    }

    [Theory]
    [InlineData("Ligue para João?", "Should I call John?", "Ligo para o João?")]
    [InlineData("Telefone para Maria?", "Shall I phone Maria?", "Telefono para a Maria?")]
    public void ExplicitSelfQuestionDoesNotNeedPreviousCue(string source, string english, string expected) =>
        Assert.Equal(expected, new PortugueseConverter().Convert(source, reference: ReferenceHint.FromText(english)));

    [Theory]
    [InlineData("É uma pequena parada.", "It's one little stop.", "É uma pequena paragem.")]
    [InlineData("Faremos uma breve parada.", "We'll make a brief stop.", "Faremos uma breve paragem.")]
    [InlineData("É a próxima parada.", "It's the next stop.", "É a próxima paragem.")]
    [InlineData("São umas pequenas paradas.", "They're small stops.", "São umas pequenas paragens.")]
    [InlineData("Uma parada militar.", "A military parade.", "Uma parada militar.")]
    [InlineData("A mulher está parada.", "The woman is standing still.", "A mulher está parada.")]
    [InlineData("É uma pequena parada.", null, "É uma pequena parada.")]
    [InlineData("Vou contar uma parada.", "I'll tell you something.", "Vou contar uma coisa.")]
    [InlineData("Ela contou uma parada.", "She counted one stop.", "Ela contou uma paragem.")]
    public void StopSenseIsNotABlanketWordSwap(string source, string? english, string expected)
    {
        var converter = new PortugueseConverter();
        var reference = ReferenceHint.FromText(english);
        Assert.Equal(expected, converter.Convert(source, reference: reference));
        Assert.Equal(expected, converter.Convert(expected, reference: reference));
    }

    [Fact]
    public void FormalAndUncertainSubjectsStayUnchanged()
    {
        var converter = new PortugueseConverter();
        Assert.Equal("Não. Disse que iria voltar.", converter.Convert("Não. Disse que iria voltar.",
            reference: ReferenceHint.FromText("No. She said she would return.")));
        Assert.Equal("Ligue para João?", converter.Convert("Ligue para João?",
            reference: ReferenceHint.FromText("Call John?")));
        Assert.Equal("Não. Disse que iria voltar.", converter.Convert("Não. Disse que iria voltar.",
            reference: ReferenceHint.FromText("No. You said you were going to return.")! with { FormalAddress = true }));
    }

    [Fact]
    public void CliticMovementPreservesConditionalAndOtherPersons()
    {
        Assert.Equal("Ele disse que me iria ajudar.", new PortugueseConverter().Convert("Ele disse que iria me ajudar."));
        Assert.Equal("Ela disse que nos iria proteger.", new PortugueseConverter().Convert("Ela disse que iria nos proteger."));
        Assert.Equal("Disseste que iria me ajudar.", new PortugueseConverter(new ConversionOptions(Pronouns: false))
            .Convert("Disse que iria me ajudar.", reference: ReferenceHint.FromText("You said she would help me.")));
    }

    [Fact]
    public void OptionsAndIgnoredPhrasesProtectExistingBehaviour()
    {
        var reference = ReferenceHint.FromText("Should I call John.");
        Assert.Equal("Ligue para João?", new PortugueseConverter().Convert("Ligue para João?", reference: reference));
        reference = ReferenceHint.FromText("Should I call John?");
        Assert.Equal("Ligue para João?", new PortugueseConverter(new ConversionOptions(YouToTu: false))
            .Convert("Ligue para João?", reference: reference));
        Assert.Equal("Ligue para João?", new PortugueseConverter(new ConversionOptions(DisabledRules: "action-questions"))
            .Convert("Ligue para João?", reference: reference));
        Assert.Equal("Ligo para João?", new PortugueseConverter(new ConversionOptions(Pronouns: false))
            .Convert("Ligue para João?", reference: reference));
        Assert.Equal("É uma pequena parada.", new PortugueseConverter(new ConversionOptions(Vocabulary: false))
            .Convert("É uma pequena parada.", reference: ReferenceHint.FromText("It's one little stop.")));
        Assert.Equal("É uma pequena parada.", new PortugueseConverter(new ConversionOptions(DisabledRules: "travel-stops"))
            .Convert("É uma pequena parada.", reference: ReferenceHint.FromText("It's one little stop.")));
        var list = new PortugueseLocalWordList();
        list.Ignored.Add("Ligue para João");
        list.Ignored.Add("pequena parada");
        Assert.Equal("Ligue para João?", new PortugueseConverter(localWordList: list)
            .Convert("Ligue para João?", reference: reference));
        Assert.Equal("É uma pequena parada.", new PortugueseConverter(localWordList: list)
            .Convert("É uma pequena parada.", reference: ReferenceHint.FromText("It's one little stop.")));
    }

    [Theory]
    [InlineData("E se precisar de ajuda.", 3010)]
    [InlineData("E se precisar de ajuda...", 8000)]
    [InlineData("Se ele precisar de ajuda...", 3010)]
    public void BareQuestionNeedsABoundedSecondPersonConditional(string previous, long start)
    {
        var blocks = SubRipParser.Parse("1\n00:00:01,000 --> 00:00:03,000\n" + previous +
            "\n\n2\n00:00:08,000 --> 00:00:10,000\nLigue para João?\n\n");
        blocks[1].StartMs = start;
        var english = previous.StartsWith("Se ele", StringComparison.Ordinal)
            ? "If he needs help..." : "And if you need help...";
        var result = SubtitleConversion.Convert(blocks, new ConversionOptions(),
            english: new string?[] { english, "Call John?" });
        Assert.Equal("Ligue para João?", result[1].Text);
    }

    [Theory]
    [InlineData("Vamos. É amiga do Paulo, Sra. Silva, certo?",
        "Come on. It's Paul's friend, Mrs. Silva, all right?")]
    [InlineData("É a minha vizinha, Sra. Silva.", "She's my neighbour, Mrs. Silva.")]
    [InlineData("É o meu colega, Sr. Silva.", "He's my colleague, Mr. Silva.")]
    public void NamedThirdPersonAppositionDoesNotImposeFormalRegister(string mention, string englishMention)
    {
        var blocks = SubRipParser.Parse("1\n00:00:01,000 --> 00:00:03,000\nVamos, João.\n\n" +
            "2\n00:00:03,010 --> 00:00:06,000\nNão. Disse que\niria me deixar.\n\n" +
            "3\n00:00:06,010 --> 00:00:09,000\n" + mention + "\n\n");
        var result = SubtitleConversion.Convert(blocks, new ConversionOptions(),
            english: new string?[] { "Come on, John.", "No. You said you were gonna drop me off.", englishMention });
        Assert.False(result[1].Hint!.FormalAddress);
        Assert.Equal("Não. Disseste que\nme ias deixar.", result[1].Text);
    }

    [Theory]
    [InlineData("Sra. Silva, pode ajudar?", "Mrs. Silva, can you help?")]
    [InlineData("É a minha amiga, senhora.", "She's my friend, ma'am.")]
    [InlineData("É a minha amiga, Sra. Silva, pode ajudar?", "She's my friend, Mrs. Silva, can you help?")]
    public void ActualFormalVocativesKeepRegister(string address, string englishAddress)
    {
        var blocks = SubRipParser.Parse("1\n00:00:01,000 --> 00:00:03,000\nNão. Disse que iria voltar.\n\n" +
            "2\n00:00:03,010 --> 00:00:06,000\n" + address + "\n\n");
        var result = SubtitleConversion.Convert(blocks, new ConversionOptions(),
            english: new string?[] { "No. You said you were going to return.", englishAddress });
        Assert.True(result[0].Hint!.FormalAddress);
        Assert.Equal("Não. Disse que iria voltar.", result[0].Text);
    }
}
