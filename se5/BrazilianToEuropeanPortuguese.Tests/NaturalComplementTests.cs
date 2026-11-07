using SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;
using SubtitleEdit.Plugins.Shared;

namespace BrazilianToEuropeanPortuguese.Tests;

public sealed class NaturalComplementTests
{
    [Theory]
    [InlineData("Parece um dia devagar\nno escritório.", "It looks like it's a slow day in the office.",
        "Parece um dia lento\nno escritório.")]
    [InlineData("Foi uma manhã devagar.", "It was a slow morning.", "Foi uma manhã lenta.")]
    [InlineData("Foram dias devagar.", "They were slow days.", "Foram dias lentos.")]
    [InlineData("Foram semanas devagar.", "They were slow weeks.", "Foram semanas lentas.")]
    [InlineData("UM DIA DEVAGAR.", "A slow day.", "UM DIA LENTO.")]
    [InlineData("O que está havendo?", null, "O que se está a passar?")]
    [InlineData("O que estava havendo?", null, "O que se estava a passar?")]
    [InlineData("O que está acontecendo?", "What's going on?", "O que se está a passar?")]
    [InlineData("O que está a haver?", null, "O que se está a passar?")]
    [InlineData("<i>O que está havendo?</i>", null, "<i>O que se está a passar?</i>")]
    [InlineData("O QUE ESTÁ HAVENDO?", null, "O QUE SE ESTÁ A PASSAR?")]
    [InlineData("-O que está havendo?", null, "-O que se está a passar?")]
    [InlineData("Espere até ver a sala de jogos.", "Wait till you see the game room.",
        "Espera até veres a sala de jogos.")]
    [InlineData("-Espere até ver a sala de jogos.\n-O quê?", "Wait till you see the game room. The what?",
        "-Espera até veres a sala de jogos.\n-O quê?")]
    [InlineData("- Espere até ver a sala.", "Wait until you see the room.", "- Espera até veres a sala.")]
    [InlineData("<i>-Espere até ver a sala.</i>", "Wait until you see the room.",
        "<i>-Espera até veres a sala.</i>")]
    [InlineData("-Sim.\n-Espere até ver a sala.", "Yes. Wait until you see the room.",
        "-Sim.\n-Espera até veres a sala.")]
    [InlineData("Espere até ouvir isso.", "Wait until you hear that.", "Espera até ouvires isso.")]
    [InlineData("Espere até conseguir entrar.", "Wait until you manage to enter.", "Espera até conseguires entrar.")]
    [InlineData("Espera até receber a carta.", "Wait till you receive the letter.", "Espera até receberes a carta.")]
    [InlineData("Espera até pôr tudo no lugar.", "Wait until you put everything in place.",
        "Espera até pores tudo no lugar.")]
    [InlineData("Espera até\nver a sala.", "Wait until you see the room.", "Espera até\nveres a sala.")]
    [InlineData("Alugamos por você.", "We rented it for you.", "Alugamos para ti.")]
    [InlineData("Compramos uma casa por você.", "We bought a house for you.", "Compramos uma casa para ti.")]
    [InlineData("Reservamos por você.", "We reserved it for you.", "Reservamos para ti.")]
    [InlineData("Preparámos tudo por ti.", "We prepared everything for you.", "Preparámos tudo para ti.")]
    [InlineData("Alugou por mim.", "He rented it for me.", "Alugou para mim.")]
    [InlineData("Este departamento sempre pode\nusar uma detetive condecorada.",
        "This department can always use a decorated detective.",
        "Este departamento pode sempre\nusar uma detetive condecorada.")]
    [InlineData("A equipa sempre pode contar contigo.", "The team can always count on you.",
        "A equipa pode sempre contar contigo.")]
    [InlineData("Eles sempre podem ajudar.", "They can always help.", "Eles podem sempre ajudar.")]
    [InlineData("A equipa sempre poderia melhorar.", "The team could always improve.",
        "A equipa poderia sempre melhorar.")]
    [InlineData("Eu sempre posso ajudar.", "I can always help.", "Eu posso sempre ajudar.")]
    [InlineData("O negócio é", "Thing is...", "A questão é")]
    [InlineData("O negócio é que não tenho tempo.", "The thing is that I don't have time.",
        "A questão é que não tenho tempo.")]
    [InlineData("A coisa é, ele não queria vir.", "The thing is, he didn't want to come.",
        "A questão é que ele não queria vir.")]
    [InlineData("O negócio era que não tinha tempo.", "The thing was that I didn't have time.",
        "A questão era que não tinha tempo.")]
    [InlineData("Bem, o lance é que não tenho tempo.", "Well, the thing is that I don't have time.",
        "Bem, a questão é que não tenho tempo.")]
    [InlineData("que não tem mais fogo\nem você?", "you got no more fight left in you?",
        "que não tens mais fogo\nem ti?")]
    [InlineData("que ainda pode ajudar.", "You can still help.", "que ainda podes ajudar.")]
    [InlineData("que nunca sabe.", "You never know.", "que nunca sabes.")]
    [InlineData("Certo, você é adulto agora,\ne é o Comissário",
        "All right, you're your age now, and you're the PC",
        "Certo, és adulto agora,\ne és o Comissário")]
    [InlineData("Certo, você é médico agora e é o Diretor.",
        "All right, you're a doctor now and you're the Director.",
        "Certo, és médico agora e és o Diretor.")]
    public void NaturalConstructionsAreGeneralizedAndStable(string source, string? english, string expected)
    {
        var converter = new PortugueseConverter();
        var reference = ReferenceHint.FromText(english);
        Assert.Equal(expected, converter.Convert(source, reference: reference));
        Assert.Equal(expected, converter.Convert(expected, reference: reference));
    }

    [Theory]
    [InlineData("O que está havendo no laboratório?", "What's there in the lab?",
        "O que está a haver no laboratório?")]
    [InlineData("Está havendo uma reunião.", "A meeting is taking place.", "Está a haver uma reunião.")]
    [InlineData("Sabe o que está havendo?", "You know what's happening?", "Sabes o que está a haver?")]
    [InlineData("O que está acontecendo?", "What are you doing?", "O que está a acontecer?")]
    [InlineData("O que houve?", null, "O que se passou?")]
    [InlineData("Espera até ver a sala.", null, "Espera até ver a sala.")]
    [InlineData("Espera até ele ver a sala.", "Wait until he sees the room.", "Espera até ele ver a sala.")]
    [InlineData("Espera até veres a sala.", "Wait until you see the room.", "Espera até veres a sala.")]
    [InlineData("Alugamos por ti.", "We rented it on your behalf.", "Alugamos por ti.")]
    [InlineData("Alugamos por você.", null, "Alugamos por ti.")]
    [InlineData("Alugamos por você.", "We rented it instead of you.", "Alugamos por ti.")]
    [InlineData("Lutamos por ti.", "We fight for you.", "Lutamos por ti.")]
    [InlineData("Este departamento sempre pode usar uma sala.", null,
        "Este departamento sempre pode usar uma sala.")]
    [InlineData("O negócio é lucrativo.", "The business is profitable.", "O negócio é lucrativo.")]
    [InlineData("A coisa é azul.", "The thing is blue.", "A coisa é azul.")]
    [InlineData("O negócio é que não tenho tempo.", null, "O negócio é que não tenho tempo.")]
    [InlineData("que não tem mais fogo em você?", "He has no more fight left in him?",
        "que não tem mais fogo em ti?")]
    [InlineData("Certo, você é adulto agora e ele é o Comissário.",
        "All right, you're an adult now and he is the Commissioner.",
        "Certo, és adulto agora e ele é o Comissário.")]
    [InlineData("O tipo de cansaço que não passa a dormir.", "The kind you don't sleep off.",
        "O tipo de cansaço que não passa a dormir.")]
    public void OtherMeaningsAndUnknownSubjectsStayProtected(string source, string? english, string expected) =>
        Assert.Equal(expected, new PortugueseConverter().Convert(source, reference: ReferenceHint.FromText(english)));

    [Theory]
    [InlineData("O dia passou devagar.", "It was a slow day.")]
    [InlineData("Parece um dia devagar.", null)]
    [InlineData("Parece um dia devagar.", "It was a busy day.")]
    [InlineData("Foi uma semana devagar.", "It was a slow day.")]
    public void SlowPeriodCorrectionPreservesAdverbsAndRequiresMatchingReference(string source, string? english) =>
        Assert.Equal(source, new PortugueseConverter().Convert(source, reference: ReferenceHint.FromText(english)));

    [Fact]
    public void SlowPeriodCorrectionRespectsOptionsAndIgnoredText()
    {
        const string source = "Parece um dia devagar.";
        var hint = ReferenceHint.FromText("It looks like a slow day.");
        foreach (var options in new[] { new ConversionOptions(Vocabulary: false),
                     new ConversionOptions(Colloquial: false), new ConversionOptions(DisabledRules: "reference-idioms") })
            Assert.Equal(source, new PortugueseConverter(options).Convert(source, reference: hint));
        var words = new PortugueseLocalWordList();
        words.Ignored.Add("dia devagar");
        Assert.Equal(source, new PortugueseConverter(localWordList: words).Convert(source, reference: hint));
    }

    [Fact]
    public void SplitIntroductionsAndDashedRepliesWorkThroughTheSubtitlePipeline()
    {
        var blocks = SubRipParser.Parse(
            "1\n00:00:01,000 --> 00:00:03,000\nO negócio é\n\n" +
            "2\n00:00:03,010 --> 00:00:05,000\nque meu novo chefe quer que eu vá.\n\n" +
            "3\n00:01:01,000 --> 00:01:03,000\nO que está havendo?\n\n" +
            "4\n00:01:03,010 --> 00:01:05,000\n-Espera até ver a sala de jogos.\n-O quê?\n\n" +
            "5\n00:02:01,000 --> 00:02:03,000\nCerto, você é adulto agora,\ne é o Diretor.\n\n");
        var result = SubtitleConversion.Convert(blocks, new ConversionOptions(),
            english: new string?[] { "Thing is...", "My new boss wants me gone.", "What's going on?",
                "Wait till you see the game room. The what?", "All right, you're an adult now, and you're the Director." });
        Assert.Equal("A questão é", result[0].Text);
        Assert.Equal("que o meu novo chefe quer que eu vá.", result[1].Text);
        Assert.Equal("O que se está a passar?", result[2].Text);
        Assert.Equal("-Espera até veres a sala de jogos.\n-O quê?", result[3].Text);
        Assert.Equal("Certo, és adulto agora,\ne és o Diretor.", result[4].Text);
    }

    [Fact]
    public void OptionsFormalityAndLocalListsAreRespected()
    {
        var reference = ReferenceHint.FromText("Wait until you see the room.")!;
        Assert.Equal("Espere até ver a sala.", new PortugueseConverter().Convert("Espere até ver a sala.",
            reference: reference with { FormalAddress = true }));
        Assert.Equal("Espere até ver a sala.", new PortugueseConverter(new ConversionOptions(YouToTu: false))
            .Convert("Espere até ver a sala.", reference: reference));
        Assert.Equal("Espera até ver a sala.", new PortugueseConverter(new ConversionOptions(DisabledRules: "wait-until"))
            .Convert("Espere até ver a sala.", reference: reference));
        Assert.Equal("Alugamos por ti.", new PortugueseConverter(new ConversionOptions(Vocabulary: false))
            .Convert("Alugamos por você.", reference: ReferenceHint.FromText("We rented it for you.")));
        Assert.Equal("Alugamos para si.", new PortugueseConverter().Convert("Alugamos por você.",
            reference: ReferenceHint.FromText("We rented it for you.")! with { FormalAddress = true }));
        var list = new PortugueseLocalWordList();
        list.Ignored.Add("Alugamos por você");
        list.Ignored.Add("O que está havendo");
        list.Ignored.Add("O negócio é");
        var converter = new PortugueseConverter(localWordList: list);
        Assert.Equal("Alugamos por você.", converter.Convert("Alugamos por você.",
            reference: ReferenceHint.FromText("We rented it for you.")));
        Assert.Equal("O que está havendo?", converter.Convert("O que está havendo?"));
        Assert.Equal("O negócio é", converter.Convert("O negócio é", reference: ReferenceHint.FromText("Thing is...")));
        foreach (var options in new[] { new ConversionOptions(Vocabulary: false),
                     new ConversionOptions(Colloquial: false), new ConversionOptions(DisabledRules: "explanatory-introductions") })
            Assert.Equal("O negócio é", new PortugueseConverter(options).Convert("O negócio é",
                reference: ReferenceHint.FromText("Thing is...")));
        Assert.Equal("Certo, é médico agora e é o Diretor.", new PortugueseConverter().Convert(
            "Certo, você é médico agora e é o Diretor.",
            reference: ReferenceHint.FromText("All right, you're a doctor now and you're the Director.")! with { FormalAddress = true }));
        Assert.Equal("Senhor, é o Diretor.", new PortugueseConverter().Convert("Senhor, você é o Diretor.",
            reference: ReferenceHint.FromText("Sir, you're the Director.")));
    }
}
