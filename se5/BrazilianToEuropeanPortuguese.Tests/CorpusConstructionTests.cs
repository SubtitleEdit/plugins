using SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;
using SubtitleEdit.Plugins.Shared;

namespace BrazilianToEuropeanPortuguese.Tests;

public sealed class CorpusConstructionTests
{
    [Theory]
    [InlineData("Alguém que você acha que é calmo.", "Someone you think is calm.", "Alguém que achas que é calmo.")]
    [InlineData("O homem que você pensa que trabalha aqui.", "The man you think works here.", "O homem que pensas que trabalha aqui.")]
    [InlineData("Uma pessoa que você acredita que pode ajudar.", "A person you believe can help.", "Uma pessoa que acreditas que pode ajudar.")]
    [InlineData("Alguém que pensa que você é esperto.", "Someone who thinks you're smart.", "Alguém que pensa que és esperto.")]
    [InlineData("Você acha que é calmo.", "You think you are calm.", "Achas que és calmo.")]
    [InlineData("Diz.", "But you say your experience tells you it's a gun. It does.", "Diz.")]
    [InlineData("Sabe.", "You know. He does.", "Sabe.")]
    [InlineData("Diz.", "You say.", "Dizes.")]
    [InlineData("Continue fazendo isso.", "You keep doing that.", "Continua a fazer isso.")]
    [InlineData("Continues fazendo isso.", "I hope you keep doing that.", "Continues a fazer isso.")]
    [InlineData("Começa a ganhar dinheiro.", "You start making money.", "Começas a ganhar dinheiro.")]
    public void ReferenceAgreementPreservesEmbeddedSubjectsAndCommands(string source, string english, string expected)
    {
        var converter = new PortugueseConverter();
        var reference = ReferenceHint.FromText(english);
        Assert.Equal(expected, converter.Convert(source, reference: reference));
        Assert.Equal(expected, converter.Convert(expected, reference: reference));
    }

    [Theory]
    [InlineData("Espero não ter deixado você esperando.", "I hope I didn't keep you waiting.", "Espero não te ter deixado à espera.")]
    [InlineData("Tenho deixado você esperando.", "I have kept you waiting.", "Tenho-te deixado à espera.")]
    [InlineData("Nunca tinha deixado você esperando.", "I had never kept you waiting.", "Nunca te tinha deixado à espera.")]
    [InlineData("<i>Espero não ter deixado você esperando.</i>", "I hope I didn't keep you waiting.", "<i>Espero não te ter deixado à espera.</i>")]
    public void KeptWaitingUsesTheExistingAuxiliaryAndCliticRules(string source, string english, string expected)
    {
        var converter = new PortugueseConverter();
        var reference = ReferenceHint.FromText(english);
        Assert.Equal(expected, converter.Convert(source, reference: reference));
        Assert.Equal(expected, converter.Convert(expected, reference: reference));
    }

    [Theory]
    [InlineData("e a música ainda tocando?", "e a música ainda a tocar?")]
    [InlineData("As pessoas ainda esperando.", "As pessoas ainda a esperar.")]
    [InlineData("A DCO na minha casa, tirando fotos.", "A DCO na minha casa, a tirar fotos.")]
    [InlineData("A DCO na minha casa tirando fotos.", "A DCO na minha casa a tirar fotos.")]
    [InlineData("Na rua correndo.", "Na rua a correr.")]
    [InlineData("Na casa Fernando.", "Na casa Fernando.")]
    [InlineData("A música é linda.", "A música é linda.")]
    [InlineData("A pessoa ainda linda.", "A pessoa ainda linda.")]
    [InlineData("Na sala de comando.", "Na sala de comando.")]
    [InlineData("Na minha casa, pensando nisso.", "Na minha casa, pensando nisso.")]
    [InlineData("Num caso envolvendo apostas.", "Num caso envolvendo apostas.")]
    [InlineData("Num contrato incluindo despesas.", "Num contrato incluindo despesas.")]
    [InlineData("Num documento contendo dados.", "Num documento contendo dados.")]
    public void ProgressiveComplementsPreserveNonverbalAndPunctuatedUses(string source, string expected)
    {
        var converter = new PortugueseConverter();
        Assert.Equal(expected, converter.Convert(source));
        Assert.Equal(expected, converter.Convert(expected));
    }

    [Fact]
    public void OptionsAndIgnoredTextKeepPriority()
    {
        const string source = "Espero não ter deixado você esperando.";
        var reference = ReferenceHint.FromText("I hope I didn't keep you waiting.")!;
        Assert.Equal(source, new PortugueseConverter().Convert(source, reference: reference with { FormalAddress = true }));
        Assert.Equal(source, new PortugueseConverter(new ConversionOptions(Progressive: false)).Convert(source, reference: reference));
        Assert.Equal(source, new PortugueseConverter(new ConversionOptions(Pronouns: false)).Convert(source, reference: reference));
        Assert.Equal(source, new PortugueseConverter(new ConversionOptions(DisabledRules: "waiting-complements")).Convert(source, reference: reference));
        var list = new PortugueseLocalWordList();
        list.Ignored.Add("deixado você esperando");
        Assert.Equal(source, new PortugueseConverter(localWordList: list).Convert(source, reference: reference));
        Assert.Equal("Espero não ter deixado você esperando.", new PortugueseConverter().Convert(source));
        Assert.Equal("e a música ainda tocando?", new PortugueseConverter(new ConversionOptions(Progressive: false)).Convert("e a música ainda tocando?"));
    }

    [Fact]
    public void ShortRepliesAndWaitingComplementsPreserveCueTimes()
    {
        var blocks = SubRipParser.Parse("1\n00:00:01,000 --> 00:00:03,000\nEspero não ter deixado\nvocê esperando.\n\n" +
            "2\n00:00:03,100 --> 00:00:04,100\nDiz.\n\n");
        var results = SubtitleConversion.Convert(blocks, new ConversionOptions(),
            english: new string?[] { "I hope I didn't keep you waiting.", "You say it. It does." });
        Assert.Equal("Espero não te ter deixado à espera.", string.Join(" ", results[0].Text.Split('\n')));
        Assert.Equal("Diz.", results[1].Text);
        Assert.Equal(1000L, blocks[0].StartMs);
        Assert.Equal(4100L, blocks[1].EndMs);
        Assert.Equal("Espero não ter deixado\nvocê esperando.", blocks[0].Text);
    }
}
