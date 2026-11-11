using SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;
using SubtitleEdit.Plugins.Shared;

namespace BrazilianToEuropeanPortuguese.Tests;

public sealed class ClauseAgreementAnalysisTests
{
    [Theory]
    [InlineData("Certeza?", "Certeza", "NominalUse")]
    [InlineData("Ajuda financeira.", "Ajuda", "NominalUse")]
    [InlineData("Ela precisa de ajuda.", "precisa", "OtherSubject")]
    [InlineData("Toda a equipa\nprecisa de uma mascote.", "precisa", "OtherSubject")]
    [InlineData("Toda equipa\nprecisa de mascotes.", "precisa", "OtherSubject")]
    [InlineData("Cada equipa\nconsegue sair.", "consegue", "OtherSubject")]
    [InlineData("Maria trabalha aqui.", "trabalha", "OtherSubject")]
    [InlineData("É necessário sair.", "É", "Impersonal")]
    [InlineData("A pessoa que tu achas que precisa de ajuda.", "precisa", "RelativeSubject")]
    [InlineData("A médica que tu pensas que trabalha aqui.", "trabalha", "RelativeSubject")]
    [InlineData("Não tens o que fazer.", "fazer", "NonFiniteUse")]
    [InlineData("Que miúda esperta tens.", "miúda", "NominalUse")]
    public void StructuralReasonsBlockUnsafeAgreement(string text, string verb, string reason)
    {
        var decision = new ClauseAgreementAnalysis(text).Decide(text.IndexOf(verb, StringComparison.Ordinal),
            ReferenceHint.FromText("You think this works."));
        Assert.False(decision.Allowed);
        Assert.Equal(reason, decision.Block.ToString());
    }

    [Theory]
    [InlineData("Precisa de ajuda.", "Precisa")]
    [InlineData("O quê, está a controlar-me?", "está")]
    [InlineData("Se não precisar de ajuda.", "precisar")]
    [InlineData("Tu sabes que precisa de ajuda.", "precisa")]
    [InlineData("Ela saiu. Precisa de ajuda.", "Precisa")]
    [InlineData("Ela saiu… Precisa de ajuda.", "Precisa")]
    [InlineData("Ela saiu;\nprecisa de ajuda.", "precisa")]
    [InlineData("-Ela saiu.\n-Precisa de ajuda.", "Precisa")]
    [InlineData("<i>Precisa</i> de ajuda.", "Precisa")]
    [InlineData("E está certo.", "está")]
    [InlineData("Eu gostaria que confirmasse comigo.", "confirmasse")]
    [InlineData("Disseste que sairia.", "sairia")]
    [InlineData("Se mentir, é um crime.", "mentir")]
    [InlineData("A pessoa que tu achas que você precisa de ajuda.", "precisa")]
    public void BoundariesDoNotLeakAnUnrelatedSubject(string text, string verb)
    {
        var decision = new ClauseAgreementAnalysis(text).Decide(text.IndexOf(verb, StringComparison.Ordinal),
            ReferenceHint.FromText("You need help."));
        Assert.True(decision.Allowed);
    }

    [Theory]
    [InlineData("Ajuda financeira.", "You help financially.", "Ajuda financeira.")]
    [InlineData("Conta própria.", "You count on yourself.", "Conta própria.")]
    [InlineData("É necessário sair.", "You need to leave.", "É necessário sair.")]
    [InlineData("Era possível sair.", "You could leave.", "Era possível sair.")]
    [InlineData("É possível.", "You can do it.", "É possível.")]
    [InlineData("A médica que você pensa que precisa de ajuda.", "The doctor you think needs help.",
        "A médica que pensas que precisa de ajuda.")]
    [InlineData("Está falando com um cara que costumava pesar 65kg.",
        "You're talking to a guy that used to weigh a buck 45 soaking wet.",
        "Estás a falar com um tipo que costumava pesar 65kg.")]
    [InlineData("Estamos atrás do cara que comanda o jogo de pôquer.",
        "That'll get you killed, all right. The guy we're looking at runs the poker game.",
        "Estamos atrás do tipo que comanda o jogo de póquer.")]
    [InlineData("Que garota esperta você tem.", "Smart kid you got there.", "Que miúda esperta tens.")]
    [InlineData("Não tem o que fazer.", "You have nothing to do.", "Não tens o que fazer.")]
    [InlineData("O que significa que Reid terá acabado com dois policiais.",
        "And you'll go to jail. Which means Reid takes out two cops.",
        "O que significa que Reid terá acabado com dois polícias.")]
    public void ReviewedNegativeCasesDoNotChangeMeaning(string source, string english, string expected) =>
        Assert.Equal(expected, new PortugueseConverter().Convert(source, reference: ReferenceHint.FromText(english)));

    [Theory]
    [InlineData("Precisa de ajuda.", "You need help.", "Precisas de ajuda.")]
    [InlineData("Se precisar de ajuda.", "If you need help.", "Se precisares de ajuda.")]
    [InlineData("Você pensa que precisa de ajuda.", "You think you need help.", "Pensas que precisas de ajuda.")]
    [InlineData("Você trabalha aqui e mora perto.", "You work here and you live nearby.", "Trabalhas aqui e vives perto.")]
    [InlineData("Então soube que chegou.", "Then I learned you arrived.", "Então soube que chegaste.")]
    [InlineData("É possível que ajude.", "You might help.", "É possível que ajudes.")]
    [InlineData("E está certo.", "And you'd be right.", "E estás certo.")]
    [InlineData("Eu gostaria que confirmasse comigo.", "I wish you would check in with me.",
        "Eu gostaria que confirmasses comigo.")]
    [InlineData("Você disse que sairia daqui.", "You said you would be out of here.",
        "Disseste que sairias daqui.")]
    [InlineData("Se mentir, é um crime.", "If you lie, it is a felony.", "Se mentires, é um crime.")]
    [InlineData("O serviço que comanda?", "Why don't you tell us about the service you're running?", "O serviço que comandas?")]
    [InlineData("O livro que quer ler.", "The book you want to read.", "O livro que queres ler.")]
    public void ReviewedPositiveCasesKeepRequiredConversions(string source, string english, string expected) =>
        Assert.Equal(expected, new PortugueseConverter().Convert(source, reference: ReferenceHint.FromText(english)));

    [Fact]
    public void CandidatesRetainMorphologicalAmbiguity()
    {
        Assert.True(ClauseAgreementAnalysis.Candidates("ajuda").HasFlag(VerbCandidate.ThirdPerson));
        Assert.True(ClauseAgreementAnalysis.Candidates("ajuda").HasFlag(VerbCandidate.NominalHomograph));
        Assert.True(ClauseAgreementAnalysis.Candidates("ajudas").HasFlag(VerbCandidate.SecondPerson));
        Assert.False(ClauseAgreementAnalysis.Candidates("certeza").HasFlag(VerbCandidate.ThirdPerson));
    }

    [Theory]
    [InlineData("O que está fazendo?", "What the hell are you doing?", "O que estás a fazer?")]
    [InlineData("Pode fazer o que quiser.", "You can do whatever you want.", "Podes fazer o que quiseres.")]
    [InlineData("Tem certeza que está pronta?", "You sure you're up for this?", "Tens a certeza que estás pronta?")]
    [InlineData("Você lê o que está nos rótulos.", "You read what's on the labels.",
        "Lês o que está nos rótulos.")]
    [InlineData("É tudo que pode fazer.", "That's all you can do.", "É tudo o que podes fazer.")]
    [InlineData("Estou preocupada com o que está fazendo.",
        "Are you worried I don't know what I'm doing? I'm worried about what you are doing.",
        "Estou preocupada com o que estás a fazer.")]
    [InlineData("Dou o tempo que precisar.", "I'll give you as much time as you need.",
        "Dou o tempo que precisares.")]
    [InlineData("O que sabe da testemunha que desapareceu?",
        "What do you know about the witness that disappeared?",
        "O que sabes da testemunha que desapareceu?")]
    public void ComplementAndObjectRelativeSubjectsRemainDistinct(string source, string english, string expected) =>
        Assert.Equal(expected, new PortugueseConverter().Convert(source, reference: ReferenceHint.FromText(english)));

    [Fact]
    public void FormalAndDisabledAddressStayUnchanged()
    {
        const string source = "Precisa de ajuda.";
        var reference = ReferenceHint.FromText("You need help.")!;
        Assert.Equal(source, new PortugueseConverter().Convert(source, reference: reference with { FormalAddress = true }));
        Assert.Equal(source, new PortugueseConverter(new ConversionOptions(YouToTu: false)).Convert(source, reference: reference));
        Assert.Equal(source, new PortugueseConverter(new ConversionOptions(DisabledRules: "address")).Convert(source, reference: reference));
    }

    [Fact]
    public void SharedPipelinePreservesSelectionOriginalsAndTiming()
    {
        var blocks = SubRipParser.Parse("1\n00:00:01,000 --> 00:00:02,000\nÉ necessário sair.\n\n" +
            "2\n00:00:02,100 --> 00:00:03,000\nPrecisa de ajuda.\n\n");
        var result = SubtitleConversion.Convert(blocks, new ConversionOptions(),
            english: new string?[] { "You need to leave.", "You need help." }, selected: new HashSet<int> { 0 });
        Assert.Equal("É necessário sair.", result[0].Text);
        Assert.Equal("Precisa de ajuda.", result[1].Text);
        Assert.Equal("É necessário sair.", blocks[0].Text);
        Assert.Equal(3000L, blocks[1].EndMs);
        var selected = SubtitleConversion.Convert(blocks, new ConversionOptions(),
            english: new string?[] { "You need to leave.", "You need help." }, selected: new HashSet<int> { 1 });
        Assert.Equal("Precisas de ajuda.", selected[1].Text);
        Assert.Equal(2100L, blocks[1].StartMs);
        Assert.Equal("Precisa de ajuda.", blocks[1].Text);
    }
}
