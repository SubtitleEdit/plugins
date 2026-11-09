using SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;
using SubtitleEdit.Plugins.Shared;

namespace BrazilianToEuropeanPortuguese.Tests;

public sealed class InformalPredicateTests
{
    [Theory]
    [InlineData("Desculpa.", "You took my seat. I'm sorry.", "Desculpa.")]
    [InlineData("Certeza?", "You sure?", "Certeza?")]
    [InlineData("Não faz sentido.", "You know, it just doesn't make sense.", "Não faz sentido.")]
    [InlineData("Não está certo, cara.", "You know, it's not right, bro.", "Não está certo, meu.")]
    [InlineData("Não é assustador.", "You're starting to scare me. That's not scary.", "Não é assustador.")]
    [InlineData("Não é catastrófico?", "You consider that catastrophic?", "Não é catastrófico?")]
    [InlineData("Não é egoísta.", "You're not selfish.", "Não és egoísta.")]
    [InlineData("Não abriu.", "You never opened it.", "Não abriste.")]
    [InlineData("Sabe.", "You know.", "Sabes.")]
    [InlineData("Será adulto.", "You'll be an adult.", "Serás adulto.")]
    [InlineData("Seria adulto.", "You'd be an adult.", "Serias adulto.")]
    [InlineData("Não faz sentido.", "You know, that doesn't make sense.", "Não faz sentido.")]
    [InlineData("Anda dizendo que sou corrupto.", "You're telling people I'm corrupt. I'm not your friend.",
        "Andas a dizer que sou corrupto.")]
    [InlineData("Roubou a minha frase.", "You stole my line. I'm sorry.", "Roubaste a minha frase.")]
    [InlineData("É adulto.", "You are an adult. I agree.", "És adulto.")]
    [InlineData("É policial?", "You a cop?", "És polícia?")]
    [InlineData("Acho que precisa de ajuda.", "You know, John needs help.", "Acho que precisa de ajuda.")]
    [InlineData("Sei que precisa de ajuda.", "You know, you need help.", "Sei que precisas de ajuda.")]
    public void ReferenceHeadsDoNotInventNominalOrImpersonalSubjects(string source, string english, string expected)
    {
        var converter = new PortugueseConverter();
        var reference = ReferenceHint.FromText(english);
        Assert.Equal(expected, converter.Convert(source, reference: reference));
        Assert.Equal(expected, converter.Convert(expected, reference: reference));
    }

    [Theory]
    [InlineData("no que Maria diria se\ncontasse que iria parar.",
        "what Maria would say if you told her you were gonna quit.",
        "no que Maria diria se\ncontasses que irias parar.")]
    [InlineData("se dissesse que iria sair.", "if you said you were going to leave.",
        "se dissesses que irias sair.")]
    [InlineData("se soubesse que poderia ajudar.", "if you knew you could help.",
        "se soubesses que poderias ajudar.")]
    [InlineData("se explicasse que teria tempo.", "if you explained you would have time.",
        "se explicasses que terias tempo.")]
    [InlineData("se contasse que ia voltar.", "if you said you were going to return.",
        "se contasses que ias voltar.")]
    [InlineData("se pensasse que conseguiria resolver.", "if you thought you could solve it.",
        "se pensasses que conseguirias resolver.")]
    [InlineData("se dissesse que odiava golfe.", "if you said you hated golf.",
        "se dissesses que odiavas golfe.")]
    [InlineData("se dissesse que sabia.", "if you said you knew.",
        "se dissesses que sabias.")]
    [InlineData("se explicasse que precisava sair.", "if you explained you needed to leave.",
        "se explicasses que precisavas de sair.")]
    [InlineData("Você é péssimo pescando,\nodeia golfe.", "You suck at fishing, you hate golf.",
        "És péssimo a pescar,\nodeias golfe.")]
    [InlineData("Você é calmo, trabalha bem, ajuda todos.", "You are calm, you work well, you help everyone.",
        "És calmo, trabalhas bem, ajudas todos.")]
    [InlineData("Você gosta de nadar, prefere correr.", "You like swimming, you prefer running.",
        "Gostas de nadar, preferes correr.")]
    [InlineData("Você trabalha aqui e mora perto.", "You work here and you live nearby.",
        "Trabalhas aqui e vives perto.")]
    [InlineData("Você sabe nadar, mas tem medo.", "You can swim, but you are afraid.",
        "Sabes nadar, mas tens medo.")]
    public void ReferenceConfirmedPredicatesAgree(string source, string english, string expected)
    {
        var converter = new PortugueseConverter();
        var reference = ReferenceHint.FromText(english);
        Assert.Equal(expected, converter.Convert(source, reference: reference));
        Assert.Equal(expected, converter.Convert(expected, reference: reference));
    }

    [Theory]
    [InlineData("Você é péssimo pescando,\nodeia golfe.", "És péssimo a pescar,\nodeias golfe.")]
    [InlineData("Você é calmo e trabalha bem.", "És calmo e trabalhas bem.")]
    [InlineData("Você é calmo e trabalha bem, ajuda todos.", "És calmo e trabalhas bem, ajuda todos.")]
    public void UnambiguousSharedSubjectsDoNotRequireAReference(string source, string expected) =>
        Assert.Equal(expected, new PortugueseConverter().Convert(source));

    [Theory]
    [InlineData("Você a deixaria louca\nem casa.", "Irias deixá-la louca\nem casa.")]
    [InlineData("Você o ajudaria.", "Irias ajudá-lo.")]
    [InlineData("Você os protegeria.", "Irias protegê-los.")]
    [InlineData("Você as veria.", "Irias vê-las.")]
    [InlineData("Você o faria.", "Irias fazê-lo.")]
    [InlineData("Você o diria.", "Irias dizê-lo.")]
    [InlineData("Você a traria.", "Irias trazê-la.")]
    [InlineData("Você o manteria.", "Irias mantê-lo.")]
    [InlineData("<i>Você a ajudaria.</i>", "<i>Irias ajudá-la.</i>")]
    [InlineData("VOCÊ A AJUDARIA.", "IRIAS AJUDÁ-LA.")]
    public void InformalObjectConditionalsUseEncliticInfinitives(string source, string expected)
    {
        var converter = new PortugueseConverter();
        Assert.Equal(expected, converter.Convert(source));
        Assert.Equal(expected, converter.Convert(expected));
    }

    [Theory]
    [InlineData("Você não a deixaria.", "Não a deixarias.")]
    [InlineData("Se você a ajudaria.", "Se a ajudarias.")]
    [InlineData("Você te ajudaria.", "Ajudar-te-ias.")]
    public void ProclisisTriggersAndOtherCliticsKeepExistingBehaviour(string source, string expected) =>
        Assert.Equal(expected, new PortugueseConverter().Convert(source));

    [Theory]
    [InlineData("se contasse que iria parar.", null, "se contasse que iria parar.")]
    [InlineData("se contasse que iria parar.", "if she told you she would quit.", "se contasse que iria parar.")]
    [InlineData("se contasse que ela iria parar.", "if you told her she would quit.", "se contasses que ela iria parar.")]
    [InlineData("Você gosta dela, odeia o irmão.", "You like her, she hates her brother.",
        "Gostas dela, odeia o irmão.")]
    [InlineData("Você gosta dela, Maria odeia o irmão.", null,
        "Gostas dela, Maria odeia o irmão.")]
    [InlineData("Você sabe que ela trabalha, mora perto.", null,
        "Sabes que ela trabalha, vive perto.")]
    [InlineData("Você procura emprego, ajuda financeira.", null,
        "Procuras emprego, ajuda financeira.")]
    [InlineData("Você tem fruta, banana madura.", null, "Tens fruta, banana madura.")]
    [InlineData("se dissesse que saberia.", "if you said she would know.", "se dissesses que saberia.")]
    public void ChangedOrUncertainSubjectsStayThirdPerson(string source, string? english, string expected) =>
        Assert.Equal(expected, new PortugueseConverter().Convert(source, reference: ReferenceHint.FromText(english)));

    [Fact]
    public void AddressAndPronounOptionsAreRespected()
    {
        const string source = "Você é péssimo pescando,\nodeia golfe.";
        Assert.Equal("É péssimo a pescar,\nodeia golfe.",
            new PortugueseConverter().Convert(source, reference:
                ReferenceHint.FromText("You suck at fishing, you hate golf.")! with { FormalAddress = true }));
        Assert.Equal("Você é péssimo a pescar,\nodeia golfe.",
            new PortugueseConverter(new ConversionOptions(YouToTu: false)).Convert(source));
        Assert.Equal("A deixarias louca.",
            new PortugueseConverter(new ConversionOptions(Pronouns: false)).Convert("Você a deixaria louca."));
        Assert.Equal("A deixarias louca.",
            new PortugueseConverter(new ConversionOptions(Colloquial: false)).Convert("Você a deixaria louca."));
        Assert.Equal("Você deixá-la-ia louca.",
            new PortugueseConverter(new ConversionOptions(DisabledRules: "address")).Convert("Você a deixaria louca."));
    }

    [Fact]
    public void LocalListsKeepPriority()
    {
        var list = new PortugueseLocalWordList();
        list.Ignored.Add("odeia golfe");
        Assert.Equal("És péssimo a pescar, odeia golfe.",
            new PortugueseConverter(localWordList: list).Convert("Você é péssimo pescando, odeia golfe."));
        list.Words.Add(("louca", "furiosa"));
        Assert.Equal("Irias deixá-la furiosa.", new PortugueseConverter(localWordList: list)
            .Convert("Você a deixaria louca."));
        list.Ignored.Add("Você a deixaria");
        Assert.Equal("Você a deixaria furiosa.", new PortugueseConverter(localWordList: list)
            .Convert("Você a deixaria louca."));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SplitConditionalSentenceKeepsCueBoundaries(bool sentenceContext)
    {
        var blocks = SubRipParser.Parse("680\n00:35:16,053 --> 00:35:17,613\nTalvez você deva pensar\n\n" +
            "681\n00:35:17,627 --> 00:35:20,616\nno que Maria diria se\ncontasse que iria parar.\n\n");
        var result = SubtitleConversion.Convert(blocks, new ConversionOptions(SentenceContext: sentenceContext),
            english: new string?[] { "Maybe you should think about",
                "what Maria would say if you told her you were gonna quit." });
        Assert.Equal("Talvez devas pensar", result[0].Text);
        Assert.Equal("no que Maria diria se\ncontasses que irias parar.", result[1].Text);
        Assert.Equal(2, blocks.Count);
        Assert.Equal(2117627L, blocks[1].StartMs);
        Assert.Equal(2120616L, blocks[1].EndMs);
    }
}
