using SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;
using SubtitleEdit.Plugins.Shared;

namespace BrazilianToEuropeanPortuguese.Tests;

public sealed class ReviewedDialogueTests
{
    [Theory]
    [InlineData("Os médicos disseram\nque ficará bem.", "Doc says you're gonna be okay.", "Os médicos disseram\nque ficarás bem.")]
    [InlineData("Acho que está se enganando.", "I think you're fooling yourself.", "Acho que estás a enganar-te.")]
    [InlineData("Acho que está a divertir-se.", "I think you're having fun.", "Acho que estás a divertir-te.")]
    [InlineData("Estavas a divertir-se.", "You were having fun.", "Estavas a divertir-te.")]
    [InlineData("Ela disse que vai melhorar.", "She said you'll get better.", "Ela disse que vais melhorar.")]
    [InlineData("Disseram que estava certo.", "They said you'd be right.", "Disseram que estavas certo.")]
    [InlineData("Se importa responder\nalgumas perguntas?", "Would you mind answering a few questions for me?",
        "Importas-te de responder\na algumas perguntas?")]
    [InlineData("Se importa abrir a porta?", "Do you mind opening the door?", "Importas-te de abrir a porta?")]
    [InlineData("Importa-se de esperar?", "Would you mind waiting?", "Importas-te de esperar?")]
    [InlineData("Se importa responder a estas perguntas?", "Would you mind answering these questions?", "Importas-te de responder a estas perguntas?")]
    [InlineData("Não se importa esperar?", "Would you mind waiting?", "Não te importas de esperar?")]
    [InlineData("Se importaria esperar?", "Would you mind waiting?", "Importar-te-ias de esperar?")]
    [InlineData("<i>Se importa esperar?</i>", "Would you mind waiting?", "<i>Importas-te de esperar?</i>")]
    [InlineData("SE IMPORTA ESPERAR?", "Would you mind waiting?", "IMPORTAS-TE DE ESPERAR?")]
    public void EmbeddedContractionsAndMindRequests(string source, string english, string expected)
    {
        var converter = new PortugueseConverter();
        var hint = ReferenceHint.FromText(english);
        Assert.Equal(expected, converter.Convert(source, reference: hint));
        Assert.Equal(expected, converter.Convert(expected, reference: hint));
    }

    [Theory]
    [InlineData("Os médicos disseram que ele ficará bem.", "The doctors said he'll be okay.", "ficarás")]
    [InlineData("Os médicos disseram que ficará bem.", null, "ficarás")]
    [InlineData("A pessoa que ela disse que ficará bem.", "The person she said would be okay.", "ficarás")]
    [InlineData("Ela disse que ficará bem.", "She said she'll be fine after you leave.", "ficarás")]
    [InlineData("Os médicos disseram que é necessário descansar.", "The doctors said you're supposed to rest.", "és necessário")]
    [InlineData("Alguém que pensa que você é esperto.", "Someone who thinks you're smart.", "pensas")]
    [InlineData("Se importa, responda.", "If it matters, answer.", "Importas-te")]
    [InlineData("Não importa responder.", "Would you mind answering?", "importas-te")]
    [InlineData("Ele se importa de responder?", "Would he mind answering?", "Importas-te")]
    [InlineData("Se importa responder?", null, "Importas-te")]
    public void EmbeddedAndRequestSafeguards(string source, string? english, string absent) =>
        Assert.DoesNotContain(absent, new PortugueseConverter().Convert(source, reference: ReferenceHint.FromText(english)));

    [Fact]
    public void HospitalDialogueUsesInformalAgreementWithoutChangingOtherSubjects()
    {
        var source = new[]
        {
            Cue(1046265, 1048391, "Que engraçado."),
            Cue(1048393, 1050087, "Os médicos disseram\nque ficará bem."),
            Cue(1050098, 1051493, "Então por que\nestá aqui?"),
            Cue(1051603, 1053230, "Porque\npeguei o caso."),
            Cue(1054507, 1056400, "Se importa responder\nalgumas perguntas?"),
            Cue(1056453, 1057848, "Sim, me importo."),
        };
        string?[] english = ["That's funny.", "Doc says you're gonna be okay.", "Yeah",
            "'Cause I caught the case.", "Would you mind answering a few questions for me?", "Yeah, I do mind."];
        foreach (var context in new[] { false, true })
        {
            var result = SubtitleConversion.Convert(source, new ConversionOptions(SentenceContext: context), english: english);
            Assert.Equal("Os médicos disseram\nque ficarás bem.", result[1].Text);
            Assert.Equal("Então porque\nestás aqui?", result[2].Text);
            Assert.Equal("Importas-te de responder\na algumas perguntas?", result[4].Text);
            Assert.Equal("Sim, importo-me.", result[5].Text);
        }
    }

    [Fact]
    public void HospitalFixesRespectFormalAddressAndDisabledOptions()
    {
        var reported = ReferenceHint.FromText("Doc says you're gonna be okay.")!;
        foreach (var options in new[] { new ConversionOptions(YouToTu: false),
            new ConversionOptions(DisabledRules: "address") })
            Assert.Equal("Disseram que ficará bem.", new PortugueseConverter(options)
                .Convert("Disseram que ficará bem.", reference: reported));
        Assert.Equal("Disseram que ficará bem.", new PortugueseConverter()
            .Convert("Disseram que ficará bem.", reference: reported with { FormalAddress = true }));
        var request = ReferenceHint.FromText("Would you mind answering a few questions?")!;
        Assert.Equal("Importa-se de responder a algumas perguntas.", new PortugueseConverter()
            .Convert("Se importa responder algumas perguntas.", reference: request with { FormalAddress = true }));
        Assert.Equal("Importa-se de responder a algumas perguntas?", new PortugueseConverter(new ConversionOptions(YouToTu: false))
            .Convert("Se importa responder algumas perguntas?", reference: request));
        Assert.Equal("Importa-se de responder a algumas perguntas?", new PortugueseConverter(new ConversionOptions(DisabledRules: "address"))
            .Convert("Se importa responder algumas perguntas?", reference: request));
        Assert.DoesNotContain(" de ", new PortugueseConverter(new ConversionOptions(DisabledRules: "dialogue-objects"))
            .Convert("Se importa responder algumas perguntas?", reference: request));
        Assert.DoesNotContain(" de ", new PortugueseConverter(new ConversionOptions(Pronouns: false))
            .Convert("Se importa responder algumas perguntas?", reference: request));
        var local = new PortugueseLocalWordList();
        local.Ignored.Add("importa");
        Assert.DoesNotContain(" de ", new PortugueseConverter(localWordList: local)
            .Convert("Se importa responder algumas perguntas?", reference: request));
    }

    [Theory]
    [InlineData("Vi a coletiva de imprensa.", "I saw the press conference.", "Vi a conferência de imprensa.")]
    [InlineData("Organizaram coletivas de imprensa.", null, "Organizaram conferências de imprensa.")]
    [InlineData("Vi a colectiva de imprensa.", null, "Vi a conferência de imprensa.")]
    [InlineData("VI AS COLETIVAS DE IMPRENSA.", null, "VI AS CONFERÊNCIAS DE IMPRENSA.")]
    [InlineData("Ela quer uma coletiva\nde imprensa.", null, "Ela quer uma conferência\nde imprensa.")]
    [InlineData("Não numa coletiva do\ndepartamento.", "Not at a department press conference.", "Não numa conferência de imprensa do\ndepartamento.")]
    [InlineData("Cancelei minha coletiva.", "I canceled my presser.", "Cancelei a minha conferência de imprensa.")]
    [InlineData("De coletivas ou exclusivas?", "Press conference or select interviews?", "De conferências de imprensa ou entrevistas exclusivas?")]
    [InlineData("Acontece que esta dona é a presidente.", "This broad happens to be the mayor.", "Acontece que esta tipa é a presidente.")]
    [InlineData("Estas donas chegaram.", "These broads arrived.", "Estas tipas chegaram.")]
    [InlineData("Vai deixar esta dona\npô-lo para fora?", "You're just gonna let this broad push you out the door?",
        "Vais deixar esta tipa\npôr-te para fora?")]
    [InlineData("Ela tentou pô-lo para fora.", "She tried to push you out.", "Ela tentou pôr-te para fora.")]
    [InlineData("<i>Esta dona chegou.</i>", "This broad arrived.", "<i>Esta tipa chegou.</i>")]
    [InlineData("E um duramente aprendido.", "And a hard-earned one.", "E um aprendido a duras penas.")]
    [InlineData("Uma lição duramente aprendida.", "A hard-learned lesson.", "Uma lição aprendida a duras penas.")]
    [InlineData("Foram factos duramente aprendidos.", "Hard-earned facts.", "Foram factos aprendidos a duras penas.")]
    [InlineData("DURAMENTE APRENDIDAS.", "Hard-earned.", "APRENDIDAS A DURAS PENAS.")]
    [InlineData("Vai fazer negócio\nou o quê?", "Doing business here or what?", "Vamos fazer negócio\nou quê?")]
    [InlineData("Vamos fazer negócios aqui ou o quê?", "Are we doing business here or what?", "Vamos fazer negócios aqui ou quê?")]
    [InlineData("Quatro custarão $ 200.", "Four will cost you... $200.", "Quatro custam 200 dólares.")]
    [InlineData("Um custará 20 euros.", "One will cost you 20 euros.", "Um custa 20 euros.")]
    [InlineData("Devo lembrar que Nicky está se formando.", "I need to remind you that Nicky is graduating.", "Devo lembrar-te que Nicky está a formar-se.")]
    [InlineData("Preciso lembrá-la que ele chegou.", "I need to remind you that he arrived.", "Preciso de lembrar-te que ele chegou.")]
    [InlineData("Deve isso a ela.", "You owe that to her.", "Deves-lhe isso.")]
    [InlineData("Eu devo isto a eles.", "I owe this to them.", "Eu devo-lhes isto.")]
    [InlineData("Não devo isso a ela.", "I don't owe that to her.", "Não lhe devo isso.")]
    [InlineData("Eu deverei isso a ela.", "I will owe that to her.", "Eu dever-lhe-ei isso.")]
    [InlineData("Eu deveria isso a ela.", "I would owe that to her.", "Eu dever-lhe-ia isso.")]
    [InlineData("Acho que eu devo isso a ela.", "I think I owe that to her.", "Acho que eu lhe devo isso.")]
    public void ReviewedSensesAndConstructions(string source, string? english, string expected)
    {
        var converter = new PortugueseConverter();
        var hint = ReferenceHint.FromText(english);
        Assert.Equal(expected, converter.Convert(source, reference: hint));
        Assert.Equal(expected, converter.Convert(expected, reference: hint));
    }

    [Theory]
    [InlineData("Esta dona de casa chegou.", "This broad arrived.", "tipa")]
    [InlineData("A Dona Maria chegou.", "This broad arrived.", "tipa")]
    [InlineData("Ela é a dona.", "She is the owner.", "tipa")]
    [InlineData("Esta dona chegou.", null, "tipa")]
    [InlineData("Uma decisão coletiva.", "A collective decision at the press conference.", "conferência")]
    [InlineData("Uma coletiva de arte.", null, "conferência")]
    [InlineData("O dinheiro duramente conquistado.", "Hard-earned money.", "penas")]
    [InlineData("Ele foi julgado duramente.", "He was judged harshly.", "penas")]
    [InlineData("Vai fazer negócio ou o quê?", "Are you doing business or what?", "Vamos")]
    [InlineData("Quatro custarão 200 euros amanhã.", "Four will cost you 200 euros tomorrow.", "custam")]
    [InlineData("Se encomendar, quatro custarão 200 euros.", "If you order, four will cost you 200 euros.", "custam")]
    [InlineData("Quatro custarão 200 euros.", null, "custam")]
    [InlineData("Quatro custariam 200 euros.", "Four would cost you 200 euros.", "custam")]
    [InlineData("Devo lembrar que ele chegou.", "I must remember that he arrived.", "lembrar-te")]
    [InlineData("Devo lembrar que ele chegou.", null, "lembrar-te")]
    [InlineData("Deve isso a ela, não a ele.", "You owe that to her, not to him.", "-lhe")]
    [InlineData("Ela tentou pô-lo para fora.", "She tried to push him out.", "pôr-te")]
    public void OtherSensesAndUnconfirmedInferencesArePreserved(string source, string? english, string absent) =>
        Assert.DoesNotContain(absent, new PortugueseConverter().Convert(source, reference: ReferenceHint.FromText(english)));

    [Fact]
    public void GenuineFuturePriceInPreviousCueIsPreserved() =>
        Assert.Contains("custarão", new PortugueseConverter().Convert("Quatro custarão 200 euros.",
            "A entrega será amanhã.", ReferenceHint.FromText("Four will cost you 200 euros.")));

    [Fact]
    public void FormalAndDisabledAddressKeepReminderRecipientFormal()
    {
        var hint = ReferenceHint.FromText("I need to remind you that she is graduating.")! with { FormalAddress = true };
        Assert.Equal("Devo lembrar-lhe que ela está a formar-se.", new PortugueseConverter().Convert(
            "Devo lembrar que ela está se formando.", reference: hint));
        Assert.Equal("Ela tentou pô-lo para fora.", new PortugueseConverter().Convert("Ela tentou pô-lo para fora.",
            reference: ReferenceHint.FromText("She tried to push you out.")! with { FormalAddress = true }));
        Assert.Contains("lembrar-lhe", new PortugueseConverter(new ConversionOptions(YouToTu: false)).Convert(
            "Devo lembrar que ela chegou.", reference: ReferenceHint.FromText("I remind you that she arrived.")));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void NewNamedSummonsDoesNotMakeTheEarlierFamilyMonologueFormal(bool sentenceContext)
    {
        var blocks = FamilyScene();
        var result = SubtitleConversion.Convert(blocks, new ConversionOptions(SentenceContext: sentenceContext), english: FamilyEnglish());
        Assert.All(result.Take(10), r => Assert.False(r.Hint?.FormalAddress == true));
        Assert.True(result[10].Hint!.FormalAddress);
        Assert.Equal("Devo lembrar-te que Nicky\nestá a formar-se este ano", result[2].Text);
        Assert.Equal("e precisas de estar lá.", result[3].Text);
        Assert.Equal("é o teu trabalho levá-la\npara o altar.", result[5].Text);
        Assert.Equal("Percebeste?", result[6].Text);
        Assert.Equal("Precisas de sair desta, Jack.", result[7].Text);
        Assert.Equal("Deves-lhe isso.", result[8].Text);
        Assert.Equal("Ouviste?", result[9].Text);
        Assert.Equal("O seu marido precisa de cirurgia.", result[11].Text);
        Assert.Equal(741375, blocks[2].StartMs);
    }

    [Theory]
    [InlineData("Precisa sair desta, Jack.", "You've got to pull through, Jack.", false)]
    [InlineData("Precisa sair desta, Reagan.", "You've got to pull through, Reagan.", true)]
    [InlineData("Precisa sair desta, senhor.", "You've got to pull through, sir.", true)]
    public void NamedSummonsBoundaryRequiresADifferentUntitledRecipient(string previous, string english, bool formal)
    {
        var blocks = new[] { Cue(0, 1000, previous), Cue(1100, 2000, "Ouviu?"), Cue(2100, 3000, "Sra. Reagan?") };
        Assert.Equal(formal, DialogueRegister.IsFormal(blocks, 1, new string?[] { english, "You hear me?", "Ms. Reagan?" }));
        Assert.True(DialogueRegister.IsFormal(blocks, 2));
    }

    [Fact]
    public void LocalExceptionsAndStageSwitchesAreRespected()
    {
        var words = new PortugueseLocalWordList();
        foreach (var word in new[] { "dona", "coletiva", "duramente", "custar", "lembrar", "dever", "fazer" })
            words.Ignored.Add(word);
        var converter = new PortugueseConverter(localWordList: words);
        Assert.Contains("dona", converter.Convert("Esta dona chegou.", reference: ReferenceHint.FromText("This broad arrived.")));
        Assert.Contains("coletiva", converter.Convert("Uma coletiva.", reference: ReferenceHint.FromText("A presser.")));
        Assert.Contains("Duramente", converter.Convert("Duramente aprendido.", reference: ReferenceHint.FromText("Hard-earned.")));
        Assert.Contains("custarão", converter.Convert("Quatro custarão 200 euros.", reference: ReferenceHint.FromText("Four will cost you 200 euros.")));
        Assert.DoesNotContain("lembrar-te", converter.Convert("Devo lembrar que ela chegou.", reference: ReferenceHint.FromText("I remind you that she arrived.")));
        Assert.DoesNotContain("-lhe", converter.Convert("Eu devo isso a ela.", reference: ReferenceHint.FromText("I owe that to her.")));
        foreach (var (stage, source, english) in new[]
        {
            ("dialogue-objects", "Devo lembrar que ela chegou.", "I remind you that she arrived."),
            ("dialogue-vocabulary", "Esta dona chegou.", "This broad arrived."),
            ("dialogue-idioms", "E um duramente aprendido.", "And a hard-earned one."),
        })
        {
            var rules = new List<RuleChange>();
            new PortugueseConverter(new ConversionOptions(DisabledRules: stage)).Convert(source,
                reference: ReferenceHint.FromText(english), trace: rules);
            Assert.DoesNotContain(rules, r => r.Id == stage);
        }
    }

    [Theory]
    [InlineData("Sim, ajuda a encontrar seu carro se foi roubado.", "Yeah, it helps you find your car if it was stolen.", "foi-se")]
    [InlineData("Como está Jack?", "How is Jack?", "estás")]
    [InlineData("Daremos a volta completa depois.", "We'll give you the full tour after.", "volta-a")]
    [InlineData("Ela mudou de turno por minha causa.", "She changed her shift because of me.", "pela minha causa")]
    public void AdditionalEpisodeScreeningDoesNotIntroduceGrammarErrors(string source, string english, string absent)
    {
        var rules = new List<RuleChange>();
        var result = new PortugueseConverter().Convert(source, reference: ReferenceHint.FromText(english), trace: rules);
        Assert.True(!result.Contains(absent), result + "\n" + string.Join(", ", rules.Select(r => r.Id)));
    }

    private static SrtBlock Cue(int start, int end, string text) => new() { StartMs = start, EndMs = end, Text = text };

    private static SrtBlock[] FamilyScene() =>
    [
        Cue(722857, 726142, "Sério, Jack?"),
        Cue(726181, 728343, "Nossa foto de casamento?"),
        Cue(741375, 744730, "Devo lembrar que Nicky\nestá se formando este ano"),
        Cue(744787, 747012, "e precisa estar lá."),
        Cue(747014, 749416, "E quando ela se casar,"),
        Cue(749416, 751542, "é seu trabalho levá-la\npara o altar."),
        Cue(751544, 753406, "Entendeu?"),
        Cue(754354, 757311, "Precisa sair desta, Jack."),
        Cue(757313, 759671, "Deve isso a ela."),
        Cue(759700, 762100, "Ouviu?"),
        Cue(762150, 764200, "Sra. Reagan?"),
        Cue(764250, 767200, "Seu marido precisa de cirurgia."),
    ];

    private static string?[] FamilyEnglish() =>
    [
        "Really, Jack?", "Our wedding photo?",
        "I need to remind you that Nicky is graduating this year", "and you need to be there.",
        "And when she gets married,", "that's your job to walk her down the aisle.",
        "You got that?", "You've got to pull through, Jack.", "You owe that to her.", "You hear me?",
        "Ms. Reagan?", "Your husband needs surgery.",
    ];
}
