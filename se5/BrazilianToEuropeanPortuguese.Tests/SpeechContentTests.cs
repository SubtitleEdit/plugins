using SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;
using SubtitleEdit.Plugins.Shared;

namespace BrazilianToEuropeanPortuguese.Tests;

public sealed class SpeechContentTests
{
    [Theory]
    [InlineData("Eu falei algo.", null, "Eu disse alguma coisa.")]
    [InlineData("Tu falaste algo?", null, "Tu disseste alguma coisa?")]
    [InlineData("Ele falou alguma coisa?", null, "Ele disse alguma coisa?")]
    [InlineData("Eles falaram qualquer coisa.", null, "Eles disseram qualquer coisa.")]
    [InlineData("Não falei nada.", null, "Não disse nada.")]
    [InlineData("Eu falo a verdade.", null, "Eu digo a verdade.")]
    [InlineData("Tu falas mentiras.", null, "Tu dizes mentiras.")]
    [InlineData("Ela fala uma mentira.", null, "Ela diz uma mentira.")]
    [InlineData("Eles falam muitas coisas.", null, "Eles dizem muitas coisas.")]
    [InlineData("Eu falava isto.", null, "Eu dizia isto.")]
    [InlineData("Tu falavas aquilo.", null, "Tu dizias aquilo.")]
    [InlineData("Nós falávamos a verdade.", null, "Nós dizíamos a verdade.")]
    [InlineData("Eu falarei tudo.", null, "Eu direi tudo.")]
    [InlineData("Ele falará a verdade.", null, "Ele dirá a verdade.")]
    [InlineData("Ela falaria isso.", null, "Ela diria isso.")]
    [InlineData("Nós falaremos algumas palavras.", null, "Nós diremos algumas palavras.")]
    [InlineData("Se falassem algo.", null, "Se dissessem alguma coisa.")]
    [InlineData("Se falar algo, avisa.", null, "Se disser alguma coisa, avisa.")]
    [InlineData("Quando falarem algo.", null, "Quando disserem alguma coisa.")]
    [InlineData("É melhor falar algo.", null, "É melhor dizer alguma coisa.")]
    [InlineData("Não quero falar nada.", null, "Não quero dizer nada.")]
    [InlineData("Ela está falando algo.", null, "Ela está a dizer alguma coisa.")]
    [InlineData("Ele tinha falado algo.", null, "Ele tinha dito alguma coisa.")]
    [InlineData("A verdade foi falada.", null, "A verdade foi falada.")]
    [InlineData("Nós falamos algo.", "We say something.", "Nós dizemos alguma coisa.")]
    [InlineData("Nós falamos algo.", "We said something.", "Nós dissemos alguma coisa.")]
    [InlineData("Nós falámos algo.", null, "Nós dissemos alguma coisa.")]
    [InlineData("Fala-me alguma coisa.", null, "Diz-me alguma coisa.")]
    [InlineData("Falar-lhe algo.", null, "Dizer-lhe alguma coisa.")]
    [InlineData("Ele falou-me algo.", null, "Ele disse-me alguma coisa.")]
    [InlineData("-Falei algo.", null, "-Disse alguma coisa.")]
    [InlineData("ELE FALOU ALGO?", null, "ELE DISSE ALGUMA COISA?")]
    [InlineData("<i>Ele falou algo?</i>", null, "<i>Ele disse alguma coisa?</i>")]
    [InlineData("Ele falou\nalgo.", null, "Ele disse\nalguma coisa.")]
    [InlineData("O que você falou?", "What did you say?", "O que disseste?")]
    [InlineData("O que ele falava?", "What was he saying?", "O que ele dizia?")]
    [InlineData("Ele falava que era inocente.", "He said he was innocent.", "Ele dizia que era inocente.")]
    [InlineData("Se falar que eu trabalho aqui, morre.", "If he says I work here, he dies.",
        "Se disser que eu trabalho aqui, morre.")]
    [InlineData("Ela falou: \"Não\".", "She said no.", "Ela disse: \"Não\".")]
    [InlineData("O que está falando?", "What are you saying?", "O que estás a dizer?")]
    [InlineData("É o que está falando?", "Is that what you're saying?", "É o que estás a dizer?")]
    [InlineData("O que me falou?", "What did he tell me?", "O que me disse?")]
    [InlineData("Ele falou para mim algo.", null, "Ele disse-me alguma coisa.")]
    [InlineData("Ela falava para ele a verdade.", null, "Ela dizia-lhe a verdade.")]
    [InlineData("Depois que falei para ela.", "After I told her.", "Depois que lhe disse.")]
    [InlineData("Eu falarei-lhe tudo.", null, "Eu dir-lhe-ei tudo.")]
    [InlineData("Se o agente falar algo.", null, "Se o agente disser alguma coisa.")]
    [InlineData("Quando a Maria falar algo.", null, "Quando a Maria disser alguma coisa.")]
    [InlineData("Se alguém falar algo.", null, "Se alguém disser alguma coisa.")]
    [InlineData("Se quiser falar algo.", null, "Se quiser dizer alguma coisa.")]
    [InlineData("Se me falar algo.", null, "Se me disser alguma coisa.")]
    [InlineData("Se eles falarem para mim algo.", null, "Se eles me disserem alguma coisa.")]
    [InlineData("Não há como falar\nisso para ela", "But one that there's no way you can bring up to her",
        "Não há como\nlhe dizer isso")]
    [InlineData("- Johnny, o que falou para ele?\n- Nada.", "Johnny, what did you tell him? Nothing.",
        "- Johnny, o que lhe disseste?\n- Nada.")]
    [InlineData("Ele tinha falado alguma coisa.", null, "Ele tinha dito alguma coisa.")]
    [InlineData("Vós falais a verdade.", null, "Vós dizeis a verdade.")]
    [InlineData("Vós falastes a verdade.", null, "Vós dissestes a verdade.")]
    [InlineData("Vós falaríeis a verdade.", null, "Vós diríeis a verdade.")]
    [InlineData("Se vós falardes a verdade.", null, "Se vós disserdes a verdade.")]
    public void ContentUsesDizerWithMatchingTenseAndPerson(string source, string? english, string expected)
    {
        var converter = new PortugueseConverter();
        var hint = ReferenceHint.FromText(english);
        Assert.Equal(expected, converter.Convert(source, reference: hint));
        Assert.Equal(expected, converter.Convert(expected, reference: hint));
    }

    [Theory]
    [InlineData("Ele fala português.", "He speaks Portuguese.")]
    [InlineData("Ela falou com ele.", "She talked to him.")]
    [InlineData("Ele falou sobre isso.", "He spoke about that.")]
    [InlineData("Ela falou de alguma coisa.", "She talked about something.")]
    [InlineData("Ele fala muito alto.", "He speaks very loudly.")]
    [InlineData("Ele não consegue falar.", "He cannot speak.")]
    [InlineData("Ele falou durante horas.", "He talked for hours.")]
    [InlineData("Nós falamos algo.", null)]
    [InlineData("O que ele falou?", null)]
    [InlineData("Sobre o que ele falou?", "What did he talk about?")]
    [InlineData("Ele falava que era inocente.", null)]
    [InlineData("Ele falava que era inocente.", "He spoke as if he was innocent.")]
    [InlineData("Ele falou com ela. Ela falava que era inocente.", "He spoke to her. She said she was innocent.")]
    [InlineData("falei_algo", "I said something.")]
    [InlineData("falou2 algo", "He said something.")]
    [InlineData("refalou algo", "He said something.")]
    [InlineData("bem-falado algo", "He said something.")]
    [InlineData("A fala \"não\".", "He said no.")]
    [InlineData("Sem falar que custa muito.", "Not to mention that it costs a lot, he said.")]
    [InlineData("O que ele falou?", "Tell me what he was talking about.")]
    [InlineData("Se o agente mais velho falar alguma coisa.", null)]
    public void OtherSensesAndUnresolvedReferencesAreProtected(string source, string? english) =>
        Assert.Equal(source, new PortugueseConverter().Convert(source, reference: ReferenceHint.FromText(english)));

    [Theory]
    [InlineData(AddressMode.Formal, "- Disse alguma coisa?\n- Nada.")]
    [InlineData(AddressMode.Informal, "- Disseste alguma coisa?\n- Nada.")]
    public void EpisodeCueRespectsRegisterAndPreservesOriginal(AddressMode address, string expected)
    {
        const string source = "- Você falou algo?\n- Nada.";
        var blocks = new[] { new SrtBlock { StartMs = 1262422, EndMs = 1265703, Text = source } };
        var result = Assert.Single(SubtitleConversion.Convert(blocks, new ConversionOptions(Address: address),
            english: ["Did you say anything? Not a peep."]));
        Assert.Equal(expected, result.Text);
        Assert.Equal(source, blocks[0].Text);
        Assert.Equal(1262422, blocks[0].StartMs);
        Assert.Equal(1265703, blocks[0].EndMs);
    }

    [Fact]
    public void OptionsAndLocalVocabularyAreRespected()
    {
        foreach (var options in new[] { new ConversionOptions(Vocabulary: false),
            new ConversionOptions(DisabledRules: "reviewed-verb-senses") })
            Assert.Equal("Ele falou algo.", new PortugueseConverter(options).Convert("Ele falou algo."));
        Assert.Equal("Ele disse alguma coisa.", new PortugueseConverter(new ConversionOptions(Colloquial: false))
            .Convert("Ele falou algo."));
        foreach (var word in new[] { "falar", "falou", "algo" })
        {
            var local = new PortugueseLocalWordList();
            local.Words.Add((word, word));
            Assert.Equal("Ele falou algo.", new PortugueseConverter(localWordList: local).Convert("Ele falou algo."));
        }
        var ignored = new PortugueseLocalWordList();
        ignored.Ignored.Add("falou algo");
        Assert.Equal("Ele falou algo.", new PortugueseConverter(localWordList: ignored).Convert("Ele falou algo."));
        Assert.Equal("O que estás a falar?", new PortugueseConverter().Convert("O que está falando?",
            reference: ReferenceHint.FromText("What are you talking about?")));
    }
}
