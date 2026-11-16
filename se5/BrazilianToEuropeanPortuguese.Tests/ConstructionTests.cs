using System.Text.Json;
using SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;
using SubtitleEdit.Plugins.Shared;

namespace BrazilianToEuropeanPortuguese.Tests;

public sealed class ConstructionTests
{
    private static readonly PortugueseConverter Converter = new();

    [Theory]
    [InlineData("-Como sabe?\n-Só sei. Dê-me.", "Más notícias.", "How do you know? I just know. Gimme.",
        "-Como sabe?\n-Simplesmente sei. Diga-me.")]
    [InlineData("Dê-me.", "Tenho novidades.", "Give it to me.", "Diga-me.")]
    [InlineData("Dá-me!", "Os detalhes?", "Give it to me straight!", "Diz-me!")]
    [InlineData("Deem-me.", "A verdade.", "Give it to me.", "Digam-me.")]
    [InlineData("Dai-me.", "Uma explicação.", "Gimme.", "Dizei-me.")]
    [InlineData("Dê-me.", null, "Give me the bad news.", "Diga-me.")]
    [InlineData("Dê-me.", null, "Give me the details.", "Diga-me.")]
    [InlineData("DÊ-ME.", "MÁS NOTÍCIAS.", "Gimme.", "DIGA-ME.")]
    [InlineData("<i>Só sei. Dê-me.</i>", "Más notícias.", "I just know. Gimme.",
        "<i>Simplesmente sei. Diga-me.</i>")]
    public void FigurativeGiveMeMeansTellMeInInformationContext(string source, string? previous, string english, string expected)
    {
        var reference = ReferenceHint.FromText(english)! with { FormalAddress = true };
        var trace = new List<RuleChange>();
        Assert.Equal(expected, Converter.Convert(source, previous, reference, trace));
        Assert.Equal(expected, Converter.Convert(expected, previous, reference));
        Assert.Contains(trace, r => r.Id == "spoken-information");
        foreach (var options in new[] { new ConversionOptions(Colloquial: false),
            new ConversionOptions(Vocabulary: false), new ConversionOptions(DisabledRules: "spoken-information") })
        {
            var disabledTrace = new List<RuleChange>();
            new PortugueseConverter(options).Convert(source, previous, reference, disabledTrace);
            Assert.DoesNotContain(disabledTrace, r => r.Id == "spoken-information");
        }
    }

    [Fact]
    public void SpokenInformationFollowsExplicitInformalAddress()
    {
        var reference = ReferenceHint.FromText("I just know. Gimme.")! with { ForceInformal = true };
        Assert.Equal("Simplesmente sei. Diz-me.", Converter.Convert("Só sei. Dê-me.", "Más notícias.", reference));
    }

    [Theory]
    [InlineData("Dê-me.", null, "Gimme.")]
    [InlineData("Dê-me.", "Más notícias.", null)]
    [InlineData("Dê-me.", "A arma.", "Gimme.")]
    [InlineData("Dê-me.", "Más notícias.", "Give me the gun.")]
    [InlineData("Dê-me.", "Más notícias.", "Don't give it to me.")]
    [InlineData("Dê-me.", "Más notícias.", "Did you give it to me?")]
    [InlineData("Dê-me.", "Más notícias.", "He said give it to me.")]
    [InlineData("Dê-me as más notícias.", "Más notícias.", "Give me the bad news.")]
    [InlineData("Dê-me o jornal.", "Más notícias.", "Gimme.")]
    [InlineData("Não dê-me.", "Más notícias.", "Gimme.")]
    [InlineData("Se ele disser dê-me.", "Más notícias.", "Gimme.")]
    [InlineData("Dê-me. Dê-me.", "Más notícias.", "Gimme.")]
    [InlineData("Dê-me uma explicação.", "A verdade.", "Give me the explanation.")]
    [InlineData("Só sei.", "Más notícias.", "I just know.")]
    [InlineData("Dê-me.", "O jornal tem más notícias.", "Gimme.")]
    public void SpokenInformationPreservesLiteralCompleteAndUnconfirmedRequests(string source, string? previous, string? english)
    {
        var reference = ReferenceHint.FromText(english);
        Assert.Equal(source, ConversionConstructions.SpokenInformation(source, previous, reference));
        var trace = new List<RuleChange>();
        var converted = Converter.Convert(source, previous, reference, trace);
        Assert.Equal(new PortugueseConverter(new ConversionOptions(DisabledRules: "spoken-information"))
            .Convert(source, previous, reference), converted);
        Assert.DoesNotContain(trace, r => r.Id == "spoken-information");
    }

    [Theory]
    [InlineData("Você é meu vizinho de andar.", "Hey, we live on the same floor.", "Vivemos no mesmo andar.")]
    [InlineData("És meu vizinho de andar.", "We live on the same floor.", "Vivemos no mesmo andar.")]
    [InlineData("Tu és o meu vizinho do mesmo andar.", "We live on the same floor.", "Vivemos no mesmo andar.")]
    [InlineData("Sou tua vizinha de andar.", "We live on the same floor.", "Vivemos no mesmo andar.")]
    [InlineData("Somos vizinhos do mesmo prédio.", "We live in the same building.", "Vivemos no mesmo edifício.")]
    [InlineData("Você é meu vizinho da mesma rua.", "We live on the same street.", "Vivemos na mesma rua.")]
    [InlineData("Sou teu vizinho do mesmo bairro.", "We live in the same neighborhood.", "Vivemos no mesmo bairro.")]
    [InlineData("Nós somos vizinhos do mesmo quarteirão.", "We live on the same block.", "Vivemos no mesmo quarteirão.")]
    [InlineData("Éramos vizinhos do mesmo edifício.", "We lived in the same building.", "Vivíamos no mesmo edifício.")]
    [InlineData("Eras meu vizinho de andar.", "We lived on the same floor.", "Vivíamos no mesmo andar.")]
    [InlineData("VOCÊ É MEU VIZINHO DE ANDAR.", "We live on the same floor.", "VIVEMOS NO MESMO ANDAR.")]
    [InlineData("<i>Você é meu vizinho de andar.</i>", "Hey, we live on the same floor.", "<i>Vivemos no mesmo andar.</i>")]
    [InlineData("Ei, somos vizinhos da mesma rua!", "Hey, we live on the same street!", "Ei, vivemos na mesma rua!")]
    public void SharedResidenceRequiresMatchingEnglishMeaning(string source, string english, string expected)
    {
        var reference = ReferenceHint.FromText(english);
        var trace = new List<RuleChange>();
        Assert.Equal(expected, Converter.Convert(source, null, reference, trace));
        Assert.Contains(trace, r => r.Id == "shared-residence");
        Assert.Equal(expected, Converter.Convert(expected, null, reference));
        foreach (var options in new[] { new ConversionOptions(Colloquial: false),
            new ConversionOptions(Vocabulary: false), new ConversionOptions(DisabledRules: "shared-residence") })
        {
            var disabledTrace = new List<RuleChange>();
            new PortugueseConverter(options).Convert(source, null, reference, disabledTrace);
            Assert.DoesNotContain(disabledTrace, r => r.Id == "shared-residence");
            Assert.Equal(source, ConversionConstructions.SharedResidence(source, null));
        }
    }

    [Theory]
    [InlineData("Somos vizinhos do mesmo prédio.", null)]
    [InlineData("Somos vizinhos do mesmo prédio.", "We don't live in the same building.")]
    [InlineData("Somos vizinhos do mesmo prédio.", "They live in the same building.")]
    [InlineData("Somos vizinhos do mesmo prédio.", "We live on the same street.")]
    [InlineData("Somos vizinhos do mesmo prédio.", "We lived in the same building.")]
    [InlineData("Somos vizinhos do mesmo prédio.", "Do we live in the same building?")]
    [InlineData("Somos vizinhos do mesmo prédio.", "We live in the same building, but not anymore.")]
    [InlineData("Somos vizinhos do mesmo prédio?", "We live in the same building.")]
    [InlineData("Não somos vizinhos do mesmo prédio.", "We live in the same building.")]
    [InlineData("Somos vizinhos do mesmo prédio, mas não nos conhecemos.", "We live in the same building.")]
    public void SharedResidencePreservesMissingConflictingAndComplexEvidence(string source, string? english)
    {
        var reference = ReferenceHint.FromText(english);
        Assert.Equal(source, ConversionConstructions.SharedResidence(source, reference));
        var trace = new List<RuleChange>();
        var result = Converter.Convert(source, null, reference, trace);
        Assert.Equal(new PortugueseConverter(new ConversionOptions(DisabledRules: "shared-residence"))
            .Convert(source, null, reference), result);
        Assert.DoesNotContain(trace, r => r.Id == "shared-residence");
    }

    [Theory]
    [InlineData("Suspeito de cerca de 1,80m, porte médio.", "Suspeito com cerca de 1,80m, porte médio.")]
    [InlineData("Uma mulher de aproximadamente 1,65 m.", "Uma mulher com aproximadamente 1,65 m.")]
    [InlineData("Um homem de 1,80 metros de altura.", "Um homem com 1,80 metros de altura.")]
    [InlineData("Um homem alto de dois metros.", "Um homem alto com dois metros.")]
    [InlineData("O indivíduo de mais de 180 cm.", "O indivíduo com mais de 180 cm.")]
    [InlineData("Uma pessoa de menos de 70 kg.", "Uma pessoa com menos de 70 kg.")]
    [InlineData("Um rapaz de cerca de oitenta quilos.", "Um rapaz com cerca de oitenta quilos.")]
    [InlineData("Uma criança de vinte e dois quilogramas de peso.", "Uma criança com vinte e dois quilogramas de peso.")]
    [InlineData("Uma rapariga de 170 centímetros.", "Uma rapariga com 170 centímetros.")]
    [InlineData("Os suspeitos de cerca de 1,80m.", "Os suspeitos com cerca de 1,80m.")]
    [InlineData("SUSPEITA DE CERCA DE 1,70M.", "SUSPEITA COM CERCA DE 1,70M.")]
    [InlineData("<i>Suspeito de cerca de\n1,80m, porte médio.</i>", "<i>Suspeito com cerca de\n1,80m, porte médio.</i>")]
    [InlineData("Homem de 1,80m e 80 kg.", "Homem com 1,80m e 80 kg.")]
    [InlineData("Mulher de cerca de 60 kg, cabelo castanho.", "Mulher com cerca de 60 kg, cabelo castanho.")]
    public void PhysicalDescriptionsUseComForMeasurements(string source, string expected)
    {
        Assert.Equal(expected, Converter.Convert(source));
        Assert.Equal(expected, Converter.Convert(expected));
        Assert.Equal(source, new PortugueseConverter(new ConversionOptions(Colloquial: false)).Convert(source));
        Assert.Equal(source, new PortugueseConverter(new ConversionOptions(DisabledRules: "physical-descriptions")).Convert(source));
        var trace = new List<RuleChange>();
        Assert.Equal(expected, Converter.Convert(source, trace: trace));
        Assert.Contains(trace, r => r.Id == "physical-descriptions");
    }

    [Theory]
    [InlineData("Um muro de cerca de dois metros.")]
    [InlineData("Uma mesa de 1,80m.")]
    [InlineData("Um suspeito de cerca de 30 anos.")]
    [InlineData("Uma mulher de cabelos castanhos.")]
    [InlineData("Um homem de palavra.")]
    [InlineData("Suspeito de tráfico de droga.")]
    [InlineData("O homem de cerca de dois milhões.")]
    [InlineData("O homem de 1,80 milhões de euros.")]
    [InlineData("Uma pessoa de 70 kg de carga.")]
    [InlineData("Um homem com cerca de 1,80m.")]
    [InlineData("Um homem de uma família de quatro pessoas.")]
    [InlineData("Uma mulher de cerca de 1,70 milhas de distância.")]
    [InlineData("Uma dose de 80 mg.")]
    public void PhysicalDescriptionsPreserveOtherDeComplements(string source) =>
        Assert.Equal(source, Converter.Convert(source));

    [Theory]
    [InlineData("Você lembra o que aconteceu?", "Lembras-te do que aconteceu?")]
    [InlineData("Eu lembro o nome.", "Eu lembro-me do nome.")]
    [InlineData("Ela lembra aquilo que disse.", "Ela lembra-se daquilo que disse.")]
    [InlineData("Não lembro isso.", "Não me lembro disso.")]
    [InlineData("Nós lembramos a data.", "Nós lembramo-nos da data.")]
    [InlineData("Esqueci de te ligar.", "Esqueci-me de te ligar.")]
    [InlineData("Ela está namorando ele.", "Ela está a namorar com ele.")]
    [InlineData("Eu namorei ela.", "Eu namorei com ela.")]
    [InlineData("Você namora ela?", "Namoras com ela?")]
    [InlineData("Ela namorava um médico.", "Ela namorava um médico.")]
    [InlineData("Ele namora a minha colega.", "Ele namora a minha colega.")]
    [InlineData("Eu lembrarei do nome.", "Eu lembrar-me-ei do nome.")]
    [InlineData("Não esqueceremos da reunião.", "Não nos esqueceremos da reunião.")]
    [InlineData("Vamos lembrar de ligar.", "Vamos lembrar-nos de ligar.")]
    [InlineData("Para lembrar o que aconteceu.", "Para lembrar-se do que aconteceu.")]
    public void GovernmentComplements(string source, string expected) => Converts(source, expected);

    [Theory]
    [InlineData("Você acha que pode resolver isso?", "Achas que podes resolver isso?")]
    [InlineData("Você pensa que consegue ajudar?", "Pensas que consegues ajudar?")]
    [InlineData("Você acha que não pode entrar?", "Achas que não podes entrar?")]
    [InlineData("Se você soubesse, teria contado.", "Se soubesses, terias contado.")]
    [InlineData("Se você pudesse, não faria isso.", "Se pudesses, não farias isso.")]
    public void SubjectContinuity(string source, string expected) => Converts(source, expected);

    [Theory]
    [InlineData("Você disse que estava cansado.", "You said you were tired.", "Disseste que estavas cansado.")]
    [InlineData("Você disse que não queria sair.", "You said you didn't want to leave.", "Disseste que não querias sair.")]
    [InlineData("Você acha que pode resolver isso?", "You think you can fix it?", "Achas que podes resolver isso?")]
    [InlineData("Você disse que queria isso.", "You said you wanted it.", "Disseste que querias isso.")]
    public void RepeatedEnglishSubjectConfirmsEmbeddedSubject(string source, string english, string expected)
    {
        var reference = ReferenceHint.FromText(english);
        Assert.Equal(expected, Converter.Convert(source, null, reference));
        Assert.Equal(expected, Converter.Convert(expected, null, reference));
    }

    [Theory]
    [InlineData("Ele virou médico.", "Ele tornou-se médico.")]
    [InlineData("Eu virei advogado.", "Eu tornei-me advogado.")]
    [InlineData("Nós viramos amigos.", "Nós tornamo-nos amigos.")]
    [InlineData("Ela nunca virou professora.", "Ela nunca se tornou professora.")]
    [InlineData("Ele está virando um problema.", "Ele está a tornar-se um problema.")]
    [InlineData("Vou pegar você na estação.", "Vou buscar-te à estação.")]
    [InlineData("Vou pegar ela no aeroporto.", "Vou buscá-la ao aeroporto.")]
    [InlineData("Ela está encarando você.", "Ela está a olhar fixamente para ti.")]
    [InlineData("Precisamos encarar o problema.", "Precisamos de enfrentar o problema.")]
    [InlineData("Ele encarou a verdade.", "Ele enfrentou a verdade.")]
    [InlineData("Não, é para você acertar a cabeça do outro cara.", "Não, é para acertares na cabeça do outro tipo.")]
    [InlineData("A bala acertou a cabeça dele.", "A bala acertou na cabeça dele.")]
    [InlineData("Atingiram os olhos do suspeito.", "Atingiram os olhos do suspeito.")]
    [InlineData("Eu curti o filme.", "Eu gostei do filme.")]
    [InlineData("Ela curte esta música.", "Ela gosta desta música.")]
    [InlineData("Ele reclama do trabalho.", "Ele queixa-se do trabalho.")]
    [InlineData("Eu não reclamo disso.", "Eu não me queixo disso.")]
    [InlineData("Esta sopa está gostosa.", "Esta sopa está saborosa.")]
    [InlineData("O bolo ficou muito gostoso.", "O bolo ficou muito saboroso.")]
    [InlineData("Você virou médico.", "Tornaste-te médico.")]
    [InlineData("Eu virarei médico.", "Eu tornar-me-ei médico.")]
    [InlineData("Eu reclamaria do trabalho.", "Eu queixar-me-ia do trabalho.")]
    [InlineData("Nós reclamamos do trabalho.", "Nós queixamo-nos do trabalho.")]
    [InlineData("Ela chega em casa e já reclama de tudo.", "Ela chegou a casa e já está a queixar-se de tudo.")]
    [InlineData("Vou virar médico.", "Vou tornar-me médico.")]
    [InlineData("Vamos reclamar do trabalho.", "Vamos queixar-nos do trabalho.")]
    [InlineData("Você reclama do trabalho.", "Queixas-te do trabalho.")]
    public void ComplementAwareVocabulary(string source, string expected) => Converts(source, expected);

    [Theory]
    [InlineData("Isso é para você fazer amanhã.", "Isso é para fazeres amanhã.")]
    [InlineData("Antes de você sair, me avise.", "Antes de saíres, avisa-me.")]
    [InlineData("É melhor você não contar.", "É melhor não contares.")]
    [InlineData("Depois de você se vestir, pode sair.", "Depois de te vestires, podes sair.")]
    [InlineData("Sem vocês perceber, ele saiu.", "Sem perceberem, ele saiu.")]
    [InlineData("Para nós fazer isso, precisamos de tempo.", "Para fazermos isso, precisamos de tempo.")]
    [InlineData("Antes de eles sair, fala comigo.", "Antes de eles saírem, fala comigo.")]
    [InlineData("É importante você não me esquecer.", "É importante não me esqueceres.")]
    [InlineData("É melhor você pôr isto aqui.", "É melhor pores isto aqui.")]
    [InlineData("Sem você cair.", "Sem caíres.")]
    [InlineData("Para vocês construir a casa.", "Para construírem a casa.")]
    [InlineData("Para nós sair.", "Para sairmos.")]
    [InlineData("Antes de você sair, não me avise.", "Antes de saíres, não me avises.")]
    public void PersonalInfinitiveSubjectsAndInterveningWords(string source, string expected) => Converts(source, expected);

    [Theory]
    [InlineData("Manter-te-ei informado.", "Vou manter-te informado.")]
    [InlineData("Dar-te-emos notícias.", "Vamos dar-te notícias.")]
    [InlineData("Fá-lo-ei amanhã.", "Vou fazê-lo amanhã.")]
    [InlineData("Trá-lo-ão amanhã.", "Vão trazê-lo amanhã.")]
    [InlineData("Dir-te-á tudo.", "Vai dizer-te tudo.")]
    [InlineData("Eu estou pronto.", "Estou pronto.")]
    [InlineData("Nós vamos sair.", "Vamos sair.")]
    [InlineData("Eu não quero sair.", "Não quero sair.")]
    [InlineData("Eu amo-te.", "Amo-te.")]
    public void OptionalConversationalStyle(string source, string expected)
    {
        var converter = new PortugueseConverter(new ConversionOptions(Conversational: true));
        Assert.Equal(expected, converter.Convert(source));
        Assert.Equal(expected, converter.Convert(expected));
    }

    [Theory]
    [InlineData("Manter-te-ei informado.")]
    [InlineData("Eu estou pronto.")]
    [InlineData("Nós vamos sair.")]
    public void ConversationalStyleIsOffByDefault(string source) => Assert.Equal(source, Converter.Convert(source));

    [Theory]
    [InlineData("Eu faria isso.")]
    [InlineData("Eu estava pronto.")]
    [InlineData("Eu sou médico, mas ele é advogado.")]
    [InlineData("Só eu sei.")]
    [InlineData("Eu também quero.")]
    [InlineData("Eu e a Maria vamos sair.")]
    [InlineData("Senhor, manter-lhe-ei a porta aberta.")]
    [InlineData("Eu disse que viria.")]
    public void ConversationalStyleProtectsContrastFormalityAndConditional(string source)
    {
        var normal = Converter.Convert(source);
        Assert.Equal(normal, new PortugueseConverter(new ConversionOptions(Conversational: true)).Convert(source));
    }

    [Theory]
    [InlineData("Isso me custa um braço e uma perna.", "It costs me an arm and a leg.", "Isso custa-me os olhos da cara.")]
    [InlineData("Estou todo ouvidos.", "I'm all ears.", "Sou todo ouvidos.")]
    [InlineData("Estamos todos ouvidos.", "We're all ears.", "Somos todos ouvidos.")]
    [InlineData("É por conta da casa.", "It's on the house.", "É oferta da casa.")]
    [InlineData("Ele derramou os feijões.", "He spilled the beans.", "Ele contou tudo.")]
    [InlineData("Ela aparece uma vez numa lua azul.", "She appears once in a blue moon.", "Ela aparece muito de vez em quando.")]
    public void IdiomsNeedBothPortugueseConstructionAndEnglishEvidence(string source, string english, string expected)
    {
        var reference = ReferenceHint.FromText(english);
        Assert.Equal(expected, Converter.Convert(source, null, reference));
        Assert.Equal(expected, Converter.Convert(expected, null, reference));
        Assert.NotEqual(expected, Converter.Convert(source));
    }

    [Theory]
    [InlineData("Pode confiar em mim.")]
    [InlineData("Ele lembra o pai.")]
    [InlineData("Vira à esquerda.")]
    [InlineData("Ele virou a página.")]
    [InlineData("Ele virou o rosto.")]
    [InlineData("O couro precisa de curtir.")]
    [InlineData("Ele reclama a herança.")]
    [InlineData("Ela é gostosa.")]
    [InlineData("Isso é para eu fazer amanhã.")]
    [InlineData("É melhor ele não contar.")]
    [InlineData("Ele namora com ela.")]
    [InlineData("Ela está a namorar com um médico.")]
    [InlineData("Ela está a reclamar a herança.")]
    [InlineData("Não sei se ele é médico.")]
    [InlineData("Vamos acertar a cabeça do parafuso.")]
    [InlineData("Virava médico.")]
    [InlineData("Reclamaria do trabalho.")]
    public void ValidOrAmbiguousSensesStayUnchanged(string source) => Assert.Equal(source, Converter.Convert(source));

    [Theory]
    [InlineData("Você acha que ele pode resolver isso?", "Achas que ele pode resolver isso?")]
    [InlineData("Você acha que pode chover?", "Achas que pode chover?")]
    [InlineData("Você disse que estava cansado.", "Disseste que estava cansado.")]
    [InlineData("Se você soubesse que ele estava aqui, teria contado.", "Se soubesses que ele estava aqui, teria contado.")]
    [InlineData("Você acha que se pode trabalhar aqui?", "Achas que se pode trabalhar aqui?")]
    public void DifferentOrUnclearClauseSubjectIsProtected(string source, string expected) => Converts(source, expected);

    [Fact]
    public void ReferenceDoesNotSupplyAnAbsentPortugueseIdiom()
    {
        Assert.Equal("Há feijões na mesa.", Converter.Convert("Há feijões na mesa.", null, ReferenceHint.FromText("He spilled the beans.")));
        Assert.Equal("Ele derramou os feijões.", Converter.Convert("Ele derramou os feijões.", null, ReferenceHint.FromText("He spilled beans on the floor.")));
        Assert.Equal("Ele derramou os feijões no chão.", Converter.Convert("Ele derramou os feijões no chão.", null, ReferenceHint.FromText("He spilled the beans on the floor.")));
    }

    [Fact]
    public void FormalReferenceBlocksNewSecondPersonContinuityAndStyle()
    {
        var reference = new ReferenceHint(true, false, false, false) { FormalAddress = true };
        var converter = new PortugueseConverter(new ConversionOptions(Conversational: true));
        Assert.Equal("Acha que pode resolver isso?", converter.Convert("Você acha que pode resolver isso?", null, reference));
        Assert.Equal("É melhor não contar.", converter.Convert("É melhor você não contar.", null, reference));
        Assert.Equal("Manter-lhe-ei a porta aberta.", converter.Convert("Manter-lhe-ei a porta aberta.", null, reference));
    }

    [Fact]
    public void NewStagesCanBeDisabledAndHaveTrace()
    {
        var trace = new List<RuleChange>();
        Assert.Equal("Ele tornou-se médico.", Converter.Convert("Ele virou médico.", null, null, trace));
        Assert.Contains(trace, rule => rule.Id == "construction-vocabulary");
        Assert.Equal("Ele virou médico.", new PortugueseConverter(
            new ConversionOptions(DisabledRules: "construction-vocabulary")).Convert("Ele virou médico."));
        Assert.Equal("Manter-te-ei informado.", new PortugueseConverter(
            new ConversionOptions(Conversational: true, DisabledRules: "conversational-style")).Convert("Manter-te-ei informado."));
    }

    [Fact]
    public void ConversationalSettingRoundTripsAndRejectsInvalidShapes()
    {
        var options = new ConversionOptions(Conversational: true);
        Assert.Equal(options, ConversionOptions.Load(new PluginRequest
        { SettingsVersion = ConversionOptions.Version, Settings = options.ToJson() }));
        Assert.False(ConversionOptions.Load(new PluginRequest
        { SettingsVersion = ConversionOptions.Version, Settings = JsonSerializer.SerializeToElement(new { }) }).Conversational);
        Assert.Throws<InvalidDataException>(() => ConversionOptions.Load(new PluginRequest
        { SettingsVersion = ConversionOptions.Version, Settings = JsonSerializer.SerializeToElement(new { conversational = "yes" }) }));
    }

    [Fact]
    public void TagsAndLineBreaksRemainIntact()
    {
        var converted = Converter.Convert("<i>Ele virou\nmédico.</i>");
        Assert.Equal("<i>Ele tornou-se\nmédico.</i>", converted);
        Assert.Equal(converted, Converter.Convert(converted));
    }

    [Fact]
    public void NewConstructionsRespectDisabledGroups()
    {
        Assert.Equal("Ele virou médico.", new PortugueseConverter(new ConversionOptions(Vocabulary: false)).Convert("Ele virou médico."));
        Assert.Equal("Ele reclama do trabalho.", new PortugueseConverter(new ConversionOptions(Pronouns: false)).Convert("Ele reclama do trabalho."));
        Assert.Equal("É melhor você não contar.", new PortugueseConverter(new ConversionOptions(YouToTu: false)).Convert("É melhor você não contar."));
        var reference = ReferenceHint.FromText("It's on the house.");
        Assert.Equal("É por conta da casa.", new PortugueseConverter(new ConversionOptions(Colloquial: false)).Convert("É por conta da casa.", null, reference));
    }

    [Fact]
    public void SharedCuePipelineUsesSavedConversationalSetting()
    {
        var cue = new SrtBlock { Text = "Eu estou pronto.", StartMs = 1000, EndMs = 4000 };
        Assert.Equal("Eu estou pronto.", SubtitleConversion.Convert(new[] { cue }, new ConversionOptions())[0].Text);
        Assert.Equal("Estou pronto.", SubtitleConversion.Convert(new[] { cue }, new ConversionOptions(Conversational: true))[0].Text);
    }

    private static void Converts(string source, string expected)
    {
        Assert.Equal(expected, Converter.Convert(source));
        Assert.Equal(expected, Converter.Convert(expected));
    }
}
