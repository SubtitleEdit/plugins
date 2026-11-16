using SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;
using SubtitleEdit.Plugins.Shared;

namespace BrazilianToEuropeanPortuguese.Tests;

public sealed class InterrogationAgreementTests
{
    [Theory]
    [InlineData("Já morou na Pensilvânia?", "Did you ever live in Pennsylvania?", "Já viveste na Pensilvânia?")]
    [InlineData("Já viveu lá?", "Have you ever lived there?", "Já viveste lá?")]
    [InlineData("Trabalhou aqui?", "Did you work here?", "Trabalhaste aqui?")]
    [InlineData("Já recebeu a carta?", "Have you received the letter?", "Já recebeste a carta?")]
    [InlineData("Já partiu?", "Did you leave already?", "Já partiste?")]
    [InlineData("Já esteve aqui?", "Have you been here?", "Já estiveste aqui?")]
    [InlineData("Já se envolveu", "You ever involved", "Já te envolveste")]
    [InlineData("Já se envolveu\nem roubo de carros lá?", "Were you ever involved in car theft there?",
        "Já te envolveste\nem roubo de carros lá?")]
    [InlineData("Se envolveu nisso?", "Were you involved in that?", "Envolveste-te nisso?")]
    [InlineData("Já se arrependeu?", "Have you ever regretted it?", "Já te arrependeste?")]
    [InlineData("Já se comprometeu?", "Have you committed yourself?", "Já te comprometeste?")]
    [InlineData("<i>Já viveu lá?</i>", "Did you ever live there?", "<i>Já viveste lá?</i>")]
    [InlineData("JÁ VIVEU LÁ?", "Did you ever live there?", "JÁ VIVESTE LÁ?")]
    [InlineData("Já vendeu o carro que estava na garagem?", "Did you sell the car that was in the garage?",
        "Já vendeste o carro que estava na garagem?")]
    [InlineData("Conhece a mulher que viu?", "Do you know the woman you saw?", "Conheces a mulher que viste?")]
    [InlineData("Você é um desses caras\nque gosta de entregar", "Are you one of these guys",
        "És um desses tipos\nque gosta de entregar")]
    [InlineData("Você é casado,\ntenho certeza que entende.", "Yeah. See, you're married; I'm sure you understand.",
        "És casado,\ntenho a certeza que percebes.")]
    public void ReferenceConfirmedInformalQuestions(string source, string english, string expected)
    {
        var converter = new PortugueseConverter();
        var hint = ReferenceHint.FromText(english);
        Assert.Equal(expected, converter.Convert(source, reference: hint));
        Assert.Equal(expected, converter.Convert(expected, reference: hint));
    }

    [Theory]
    [InlineData("Já viveu lá?", "Did he ever live there?")]
    [InlineData("Já viveu lá?", "Did you and Maria live there?")]
    [InlineData("Já viveu lá?", "Did you both live there?")]
    [InlineData("Já viveu lá?", null)]
    [InlineData("Já ele viveu lá?", "Did you ever live there?")]
    [InlineData("O João viveu lá?", "Did you ever live there?")]
    [InlineData("Já se vende a casa?", "Are you selling the house?")]
    public void OtherAndUncertainSubjectsStayProtected(string source, string? english) =>
        Assert.Equal(source, new PortugueseConverter().Convert(source, reference: ReferenceHint.FromText(english)));

    [Theory]
    [InlineData("Receio que\nvocê está preso.", "I'm afraid you're under arrest.", "Receio que\nestejas preso.")]
    [InlineData("Receio que estás preso.", null, "Receio que estejas preso.")]
    [InlineData("Receio que ele está preso.", "I'm afraid he is under arrest.", "Receio que ele esteja preso.")]
    [InlineData("Temo que tu tens razão.", null, "Temo que tu tenhas razão.")]
    [InlineData("Receamos que eles estão presos.", null, "Receamos que eles estejam presos.")]
    [InlineData("Receio que ela precisa de ajuda.", null, "Receio que ela precise de ajuda.")]
    [InlineData("Desejo que tu ficas bem.", null, "Desejo que tu fiques bem.")]
    [InlineData("Duvido que ele sabe.", null, "Duvido que ele saiba.")]
    [InlineData("Receio que ele engana-se.", null, "Receio que ele se engane.")]
    [InlineData("Receio que ele não pode vir.", null, "Receio que ele não possa vir.")]
    [InlineData("RECEIO QUE ESTÁS PRESO.", null, "RECEIO QUE ESTEJAS PRESO.")]
    [InlineData("<i>Receio que estás preso.</i>", null, "<i>Receio que estejas preso.</i>")]
    [InlineData("Espero que ambos precisam\nse conhecer de novo.", null, "Espero que ambos precisem\nde se conhecer de novo.")]
    [InlineData("Espero que ambas precisam de ajuda.", null, "Espero que ambas precisem de ajuda.")]
    [InlineData("Esperamos que eles podem vir.", null, "Esperamos que eles possam vir.")]
    [InlineData("Espero que tu sabes.", null, "Espero que tu saibas.")]
    [InlineData("Espero que a equipa chega cedo.", null, "Espero que a equipa chegue cedo.")]
    [InlineData("Esperemos que todos estão bem.", null, "Esperemos que todos estejam bem.")]
    [InlineData("Desejo que ambos ficam bem.", null, "Desejo que ambos fiquem bem.")]
    [InlineData("Temo que ambas enganam-se.", null, "Temo que ambas se enganem.")]
    [InlineData("ESPERO QUE AMBOS PRECISAM DE AJUDA.", null, "ESPERO QUE AMBOS PRECISEM DE AJUDA.")]
    public void ComplementMoodPreservesSubject(string source, string? english, string expected)
    {
        var converter = new PortugueseConverter();
        var hint = ReferenceHint.FromText(english);
        Assert.Equal(expected, converter.Convert(source, reference: hint));
        Assert.Equal(expected, converter.Convert(expected, reference: hint));
    }

    [Theory]
    [InlineData("Não duvido que ele sabe.")]
    [InlineData("O receio que ele tem.")]
    [InlineData("Receio que sim.")]
    [InlineData("Receio que não.")]
    [InlineData("Receio que ele esteve preso.")]
    [InlineData("Disse que ele está preso.")]
    [InlineData("Acho que ele está preso.")]
    [InlineData("Receio que estejas preso.")]
    [InlineData("Espero que ambos precisem de ajuda.")]
    [InlineData("Espero que ele precisava de ajuda.")]
    [InlineData("Espero que sim.")]
    [InlineData("Espero que não.")]
    [InlineData("A espera que ele tem.")]
    [InlineData("Ele espera o comboio.")]
    public void NonTargetsStayUnchanged(string source) =>
        Assert.Equal(source, new PortugueseConverter().Convert(source));

    [Theory]
    [InlineData("Já viveu lá?", "Did you ever live there?")]
    [InlineData("Já se envolveu?", "Were you ever involved?")]
    public void FormalAndDisabledAddressKeepThirdPerson(string source, string english)
    {
        var hint = ReferenceHint.FromText(english)!;
        Assert.Equal(source, new PortugueseConverter().Convert(source, reference: hint with { FormalAddress = true }));
        Assert.Equal(source, new PortugueseConverter(new ConversionOptions(YouToTu: false)).Convert(source, reference: hint));
        Assert.Equal(source, new PortugueseConverter(new ConversionOptions(DisabledRules: "address")).Convert(source, reference: hint));
    }

    [Fact]
    public void FormalInvertedQuestionRemainsFormal() =>
        Assert.Equal("Tem a certeza de que não quer\nque desembale aquilo?", new PortugueseConverter()
            .Convert("Está certo que não quer\nque eu desembale aquilo?",
                reference: ReferenceHint.FromText("Are you sure you don't want me to unpack that?")! with { FormalAddress = true }));

    [Fact]
    public void FormalMoodCorrectionDoesNotImplyInformalAddress() =>
        Assert.Equal("Receio que esteja preso.", new PortugueseConverter(new ConversionOptions(YouToTu: false))
            .Convert("Receio que está preso."));

    [Fact]
    public void ComplementRuleCanBeDisabled() =>
        Assert.Equal("Receio que estás preso.", new PortugueseConverter(
            new ConversionOptions(DisabledRules: "complement-subjunctive")).Convert("Receio que estás preso."));

    [Fact]
    public void HopeComplementUsesTheSameRuleSwitch() =>
        Assert.Equal("Espero que ambos precisam de ajuda.", new PortugueseConverter(
            new ConversionOptions(DisabledRules: "complement-subjunctive")).Convert("Espero que ambos precisam de ajuda."));

    [Fact]
    public void IgnoredPhraseRemainsAuthoritative()
    {
        var words = new PortugueseLocalWordList();
        words.Ignored.Add("Receio que estás preso");
        Assert.Equal("Receio que estás preso.", new PortugueseConverter(localWordList: words)
            .Convert("Receio que estás preso."));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SplitScenePreservesCueBoundariesAndTimes(bool sentenceContext)
    {
        const string source = "140\n00:07:09,530 --> 00:07:11,987\nParker Mack.\nÉ seu nome completo?\n\n" +
            "141\n00:07:11,990 --> 00:07:13,118\nSim, certo.\n\n" +
            "142\n00:07:13,141 --> 00:07:14,934\nJá morou na Pensilvânia?\n\n" +
            "143\n00:07:14,936 --> 00:07:17,627\nFui à faculdade lá.\nPor quê? O que há?\n\n" +
            "144\n00:07:17,628 --> 00:07:18,989\nJá se envolveu\n\n" +
            "145\n00:07:19,007 --> 00:07:21,365\nem roubo de carro lá?\n\n" +
            "149\n00:07:29,178 --> 00:07:31,968\nReceio que\nvocê está preso.\n\n";
        var blocks = SubRipParser.Parse(source);
        var result = SubtitleConversion.Convert(blocks, new ConversionOptions(SentenceContext: sentenceContext),
            english: new string?[] { "Parker Mack--is that your full name?", "Yeah, that's right.",
                "Did you ever live in Pennsylvania?", "I went to college there. Why? What's up?",
                "You ever involved", "in a grand larceny auto situation down there?",
                "I'm afraid you're under arrest." });
        Assert.Equal("Já viveste na Pensilvânia?", result[2].Text);
        Assert.Equal("Já te envolveste", result[4].Text);
        Assert.Equal("Receio que\nestejas preso.", result[6].Text);
        Assert.Equal(blocks.Count, result.Count);
        Assert.Equal(449178L, blocks[6].StartMs);
        Assert.Equal(451968L, blocks[6].EndMs);
        Assert.Equal("Já morou na Pensilvânia?", blocks[2].Text);
        Assert.Equal("Já se envolveu", blocks[4].Text);
        Assert.Equal("Receio que\nvocê está preso.", blocks[6].Text);
    }
}
