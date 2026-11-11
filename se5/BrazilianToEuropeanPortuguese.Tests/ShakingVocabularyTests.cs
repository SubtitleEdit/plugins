using SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;

namespace BrazilianToEuropeanPortuguese.Tests;

public sealed class ShakingVocabularyTests
{
    [Theory]
    [InlineData("Chacoalhar a garrafa antes de beber.", null, "Agitar a garrafa antes de beber.")]
    [InlineData("Ele chacoalhou o casaco para tirar a poeira.", null, "Ele sacudiu o casaco para tirar a poeira.")]
    [InlineData("Chacoalhe bem o remédio.", null, "Agita bem o medicamento.")]
    [InlineData("Chacoalhou o suco.", null, "Agitou o sumo.")]
    [InlineData("Chacoalha a cabeça.", null, "Abana a cabeça.")]
    [InlineData("Chacoalha os braços.", null, "Sacode os braços.")]
    [InlineData("Chacoalhas as mãos.", null, "Sacodes as mãos.")]
    [InlineData("Chacoalham o tapete.", null, "Sacodem o tapete.")]
    [InlineData("Chacoalhe o lençol.", null, "Sacuda o lençol.")]
    [InlineData("Chacoalhando o casaco.", null, "A sacudir o casaco.")]
    [InlineData("O casaco foi chacoalhado.", null, "O casaco foi sacudido.")]
    [InlineData("As garrafas foram chacoalhadas.", null, "As garrafas foram agitadas.")]
    [InlineData("Não me chacoalhe!", "Don't shake me!", "Não me abanes!")]
    [InlineData("Chacoalhou-me.", "He shook me.", "Abanou-me.")]
    [InlineData("Vou chacoalhá-lo.", "I'm going to shake him.", "Vou abaná-lo.")]
    [InlineData("Chacoalhá-lo-ei.", "I will shake him.", "Abaná-lo-ei.")]
    [InlineData("Chacoalhar-me-ia.", "He would shake me.", "Abanar-me-ia.")]
    [InlineData("Não chacoalhe o frasco.", null, "Não agites o frasco.")]
    [InlineData("CHACOALHOU O CASACO.", null, "SACUDIU O CASACO.")]
    [InlineData("<i>Chacoalhe a garrafa.</i>", null, "<i>Agita a garrafa.</i>")]
    [InlineData("Chacoalhou\no casaco.", null, "Sacudiu\no casaco.")]
    [InlineData("Levei um chacoalhão.", null, "Levei um abanão.")]
    [InlineData("Dois chacoalhões.", null, "Dois abanões.")]
    [InlineData("Deu uma chacoalhada.", null, "Deu uma sacudidela.")]
    [InlineData("Duas chacoalhadas.", null, "Duas sacudidelas.")]
    [InlineData("CHACOALHÃO!", null, "ABANÃO!")]
    [InlineData("Ele chacoalhou o funcionário.", "He insulted the employee.", "Ele insultou o funcionário.")]
    [InlineData("Ele chacoalhou o funcionário.", "He scolded the employee.", "Ele repreendeu o funcionário.")]
    [InlineData("Pare de chacoalhar.", "Stop pestering.", "Para de incomodar.")]
    [InlineData("Ficou chacoalhado.", "He was shaken by the news.", "Ficou abalado.")]
    [InlineData("Foi chacoalhado.", "He was shaken by his kidnapper.", "Foi abanado.")]
    [InlineData("Chacoalhamos a garrafa.", "We shook the bottle.", "Agitámos a garrafa.")]
    [InlineData("Chacoalhamos a garrafa.", "We shake the bottle.", "Agitamos a garrafa.")]
    [InlineData("Chacoalhamos a garrafa.", null, "Agitamos a garrafa.")]
    [InlineData("Mas chacoalhada\nalém do normal,", "But shaken up more than usual.", "Mas abalada\nalém do normal,")]
    [InlineData("se eu estiver chacoalhando\nrolamentos de esferas", "if I'm shaking ball bearings", "se eu estiver a chocalhar\nrolamentos de esferas")]
    [InlineData("Chacoalhou as chaves.", null, "Chocalhou as chaves.")]
    [InlineData("Chacoalha as moedas.", null, "Chocalha as moedas.")]
    [InlineData("Não chacoalhem os guizos.", null, "Não chocalhem os guizos.")]
    [InlineData("Os chocalhos foram chacoalhados.", null, "Os chocalhos foram chocalhados.")]
    [InlineData("<i>Chacoalhar as chaves.</i>", null, "<i>Chocalhar as chaves.</i>")]
    [InlineData("CHACOALHOU OS ROLAMENTOS.", null, "CHOCALHOU OS ROLAMENTOS.")]
    public void PhysicalAndFigurativeContexts(string source, string? english, string expected)
    {
        var converter = new PortugueseConverter();
        var reference = ReferenceHint.FromText(english);
        Assert.Equal(expected, converter.Convert(source, reference: reference));
        Assert.Equal(expected, converter.Convert(expected, reference: reference));
    }

    [Theory]
    [InlineData("O chacoalho está aqui.")]
    [InlineData("microchacoalhar")]
    [InlineData("chacoalhar_uma_coisa")]
    [InlineData("Chocalhar as chaves.")]
    [InlineData("Chocalhou as moedas.")]
    [InlineData("Vou chocalhar a garrafa.")]
    public void NounsAmbiguousTensesAndOtherWordsAreProtected(string source) =>
        Assert.Equal(source, new PortugueseConverter().Convert(source));

    [Theory]
    [InlineData("chacoalhar", "sacudir")]
    [InlineData("chacoalho", "sacudo")]
    [InlineData("chacoalhas", "sacodes")]
    [InlineData("chacoalha", "sacode")]
    [InlineData("chacoalhamos", "sacudimos")]
    [InlineData("chacoalham", "sacodem")]
    [InlineData("chacoalhei", "sacudi")]
    [InlineData("chacoalhaste", "sacudiste")]
    [InlineData("chacoalhou", "sacudiu")]
    [InlineData("chacoalharam", "sacudiram")]
    [InlineData("chacoalhava", "sacudia")]
    [InlineData("chacoalhavas", "sacudias")]
    [InlineData("chacoalhávamos", "sacudíamos")]
    [InlineData("chacoalhavam", "sacudiam")]
    [InlineData("chacoalharei", "sacudirei")]
    [InlineData("chacoalharás", "sacudirás")]
    [InlineData("chacoalhará", "sacudirá")]
    [InlineData("chacoalharemos", "sacudiremos")]
    [InlineData("chacoalharão", "sacudirão")]
    [InlineData("chacoalharia", "sacudiria")]
    [InlineData("chacoalharias", "sacudirias")]
    [InlineData("chacoalharíamos", "sacudiríamos")]
    [InlineData("chacoalhariam", "sacudiriam")]
    [InlineData("chacoalhe", "sacuda")]
    [InlineData("chacoalhes", "sacudas")]
    [InlineData("chacoalhemos", "sacudamos")]
    [InlineData("chacoalhem", "sacudam")]
    [InlineData("chacoalhasse", "sacudisse")]
    [InlineData("chacoalhasses", "sacudisses")]
    [InlineData("chacoalhássemos", "sacudíssemos")]
    [InlineData("chacoalhassem", "sacudissem")]
    [InlineData("chacoalhares", "sacudires")]
    [InlineData("chacoalharmos", "sacudirmos")]
    [InlineData("chacoalharem", "sacudirem")]
    [InlineData("chacoalhando", "sacudindo")]
    [InlineData("chacoalhado", "sacudido")]
    [InlineData("chacoalhada", "sacudida")]
    [InlineData("chacoalhados", "sacudidos")]
    [InlineData("chacoalhadas", "sacudidas")]
    [InlineData("chacoalhara", "sacudira")]
    [InlineData("chacoalharas", "sacudiras")]
    [InlineData("chacoalháramos", "sacudíramos")]
    [InlineData("chacoalhámos", "sacudimos")]
    [InlineData("chacoalhais", "sacudis")]
    [InlineData("chacoalhastes", "sacudistes")]
    [InlineData("chacoalháveis", "sacudíeis")]
    [InlineData("chacoalhareis", "sacudireis")]
    [InlineData("chacoalharíeis", "sacudiríeis")]
    [InlineData("chacoalheis", "sacudais")]
    [InlineData("chacoalhásseis", "sacudísseis")]
    [InlineData("chacoalhardes", "sacudirdes")]
    [InlineData("chacoalháreis", "sacudíreis")]
    [InlineData("chacoalhai", "sacudi")]
    public void ReviewedSacudirParadigm(string source, string expected) =>
        Assert.Equal(expected + " o casaco.",
            new PortugueseConverter(new ConversionOptions(Progressive: false, YouToTu: false)).Convert(source + " o casaco."));

    [Theory]
    [InlineData("Ele pentelhou a professora.", "Ele chateou a professora.")]
    [InlineData("Não pentelhe.", "Não chateie.")]
    [InlineData("Pentelhar.", "Chatear.")]
    [InlineData("Pentelhando.", "A chatear.")]
    [InlineData("Vou pentelhá-lo.", "Vou chateá-lo.")]
    [InlineData("Não xerete nas minhas coisas.", "Não bisbilhotes nas minhas coisas.")]
    [InlineData("Xeretou os meus segredos.", "Bisbilhotou os meus segredos.")]
    [InlineData("Xeretando na vida.", "A bisbilhotar na vida.")]
    [InlineData("Xeretando meu caso?", "A bisbilhotar o meu caso?")]
    [InlineData("Futucou nas gavetas.", "Remexeu nas gavetas.")]
    [InlineData("Não futuque na minha bolsa.", "Não remexas na minha bolsa.")]
    [InlineData("Futucava os bolsos.", "Remexia os bolsos.")]
    public void OtherReviewedMissingVerbs(string source, string expected)
    {
        var converter = new PortugueseConverter();
        Assert.Equal(expected, converter.Convert(source));
        Assert.Equal(expected, converter.Convert(expected));
    }

    [Theory]
    [InlineData("Xeretou o chefe.")]
    [InlineData("Futucou a memória.")]
    [InlineData("Futucou a camisa.")]
    [InlineData("Futucou o braço.")]
    [InlineData("Futucou o chefe.")]
    public void PolysemousVerbsRequireReviewedComplements(string source) =>
        Assert.Equal(source, new PortugueseConverter().Convert(source));

    [Fact]
    public void SettingsExceptionsAndTraceAreRespected()
    {
        var source = "Chacoalhou o casaco.";
        foreach (var options in new[]
        {
            new ConversionOptions(Vocabulary: false),
            new ConversionOptions(DisabledRules: "shaking-vocabulary"),
        })
            Assert.Equal(source, new PortugueseConverter(options).Convert(source));
        foreach (var word in new[] { "chacoalhar", "chacoalhou" })
        {
            var ignored = new PortugueseLocalWordList();
            ignored.Ignored.Add(word);
            Assert.Equal(source, new PortugueseConverter(localWordList: ignored).Convert(source));
        }
        var custom = new PortugueseLocalWordList();
        custom.Words.Add(("chacoalhar", "mexer"));
        Assert.Equal(source, new PortugueseConverter(localWordList: custom).Convert(source));
        custom.Words.Add(("chacoalhou", "mexeu"));
        Assert.Equal("Mexeu o casaco.", new PortugueseConverter(localWordList: custom).Convert(source));
        var trace = new List<RuleChange>();
        new PortugueseConverter().Convert(source, trace: trace);
        Assert.Contains(trace, r => r.Id == "shaking-vocabulary");
        Assert.Equal("Ele pentelhou a professora.", new PortugueseConverter(new ConversionOptions(Vocabulary: false))
            .Convert("Ele pentelhou a professora."));
        Assert.Equal("Não xeretes nas minhas coisas.", new PortugueseConverter(new ConversionOptions(DisabledRules: "verb-vocabulary"))
            .Convert("Não xerete nas minhas coisas."));
        foreach (var verb in new[] { "pentelhar", "xeretar", "futucar" })
        {
            var ignored = new PortugueseLocalWordList();
            ignored.Ignored.Add(verb);
            Assert.Equal($"Vamos {verb} nas coisas.",
                new PortugueseConverter(localWordList: ignored).Convert($"Vamos {verb} nas coisas."));
        }
    }
}
