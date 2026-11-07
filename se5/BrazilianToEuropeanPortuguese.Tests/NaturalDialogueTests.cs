using SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;
using SubtitleEdit.Plugins.Shared;

namespace BrazilianToEuropeanPortuguese.Tests;

public sealed class NaturalDialogueTests
{
    [Theory]
    [InlineData("Tudo o que lembro\né alguém", "All I remember is someone.", "Tudo o que me lembro\né alguém")]
    [InlineData("Tudo o que lembro é alguém.", "All I remember is someone.", "Tudo o que me lembro é alguém.")]
    [InlineData("A única coisa que lembramos é o ruído.", "The only thing we remember is the noise.",
        "A única coisa que nos lembramos é o ruído.")]
    [InlineData("mandando-se daqui\no mais rápido possível.", "by getting the hell out of here as quick as you can.",
        "saindo daqui\no mais rápido possível.")]
    [InlineData("Ele mandou-se daqui.", "He got out of here.", "Ele saiu daqui.")]
    [InlineData("Ela estava a mandar-se dali.", "She was going away.", "Ela estava a sair dali.")]
    [InlineData("Sobre o que você\ne João discutiram?", "What did you and John fight about?",
        "Tu\ne o João discutiram sobre o quê?")]
    [InlineData("Sobre o que você e a Ana falaram?", "What did you and Anne talk about?",
        "Tu e a Ana falaram sobre o quê?")]
    [InlineData("Vá embora!", "Walk away!", "Vá-se!")]
    [InlineData("VÁ EMBORA!", "Walk away!", "VÁ-SE!")]
    [InlineData("Vai embora!", "Go away!", "Vai-te!")]
    [InlineData("Vão embora!", "Go away!", "Vão-se!")]
    [InlineData("João! João.\nVá embora!", "John! John.", "João! João.\nVá-se!")]
    [InlineData("Está certo que não quer\nque eu desembale aquilo?",
        "Are you sure you don't want me to unpack that?",
        "Tem a certeza de que não quer\nque desembale aquilo?")]
    public void NaturalDialogueKeepsMeaningAndRegister(string source, string english, string expected)
    {
        var converter = new PortugueseConverter();
        var hint = ReferenceHint.FromText(english);
        Assert.Equal(expected, converter.Convert(source, reference: hint));
        Assert.Equal(expected, converter.Convert(expected, reference: hint));
    }

    [Theory]
    [InlineData("Ele mandou a carta.", "He sent the letter.", "Ele mandou a carta.")]
    [InlineData("Ele mandou-se daqui.", "He sent himself from here.", "Ele mandou-se daqui.")]
    [InlineData("Sobre o que ele e João discutiram?", "What did he and John argue about?",
        "Sobre o que ele e João discutiram?")]
    [InlineData("Está certo que ele vem.", "It's certain he is coming.", "Está certo que ele vem.")]
    [InlineData("Ele vai embora.", "He leaves.", "Ele vai embora.")]
    [InlineData("Vamos embora!", "Let's leave!", "Vamos embora!")]
    [InlineData("O que lembro ao João é importante.", "What I remind John of is important.",
        "O que lembro ao João é importante.")]
    public void OtherSensesArePreserved(string source, string english, string expected) =>
        Assert.Equal(expected, new PortugueseConverter().Convert(source, reference: ReferenceHint.FromText(english)));

    [Fact]
    public void EllipsisVocativeCarriesFormalAddressAcrossDialogueLines()
    {
        var blocks = SubRipParser.Parse(
            "1\n00:00:01,000 --> 00:00:03,000\n-Detetive...\n-Estou só dizendo.\n\n" +
            "2\n00:00:03,010 --> 00:00:05,000\n-Isso é necessário?\n-Bem, não queria\n\n" +
            "3\n00:00:05,010 --> 00:00:07,000\nconstrangê-lo na frente\nda sua paciente.\n\n");
        var result = SubtitleConversion.Convert(blocks, new ConversionOptions(), english: new string?[] {
            "Detective... I'm just saying.", "Is this necessary? Well, I didn't want",
            "to embarrass you in front of your patient." });
        Assert.True(result[2].Hint!.FormalAddress);
        Assert.Contains("da sua paciente.", result[2].Text);
        Assert.DoesNotContain("tua", result[2].Text);
    }

    [Fact]
    public void InformalCertaintyAndOptionsAreRespected()
    {
        var hint = ReferenceHint.FromText("Are you sure you don't want me to unpack that?")!;
        Assert.Equal("Tens a certeza de que não queres que desembale aquilo?",
            new PortugueseConverter().Convert("Estás certo que não queres que eu desembale aquilo?", reference: hint));
        Assert.Equal("Vá embora!", new PortugueseConverter(new ConversionOptions(Colloquial: false))
            .Convert("Vá embora!"));
        Assert.Equal("Vá embora!", new PortugueseConverter(new ConversionOptions(DisabledRules: "natural-dialogue"))
            .Convert("Vá embora!"));
        Assert.Equal("mandando-se daqui.", new PortugueseConverter(new ConversionOptions(Vocabulary: false))
            .Convert("mandando-se daqui.", reference: ReferenceHint.FromText("getting out of here.")));
        var words = new PortugueseLocalWordList();
        words.Ignored.Add("Vá embora");
        Assert.Equal("Vá embora!", new PortugueseConverter(localWordList: words).Convert("Vá embora!"));
    }

    [Fact]
    public void FormalTopicQuestionPreservesCoordinatedSubject()
    {
        var hint = ReferenceHint.FromText("What did you and John fight about?")! with { FormalAddress = true };
        Assert.Equal("Você e o João discutiram sobre o quê?",
            new PortugueseConverter().Convert("Sobre o que você e João discutiram?", reference: hint));
        Assert.Equal("Você e o Jack discutiram sobre o quê?",
            new PortugueseConverter().Convert("Sobre o que você e Jack discutiram?",
                reference: ReferenceHint.FromText("What did you and Jack fight about?")! with { FormalAddress = true }));
        Assert.Equal("Você e a Ana discutiram sobre o quê?",
            new PortugueseConverter().Convert("Sobre o que você e Ana discutiram?",
                reference: ReferenceHint.FromText("What did you and Anne fight about?")! with { FormalAddress = true }));
        Assert.Equal("Tem a certeza de que não quer que desembale aquilo?",
            new PortugueseConverter().Convert("Está certo que não quer que eu desembale aquilo?",
                reference: ReferenceHint.FromText("Are you sure you don't want me to unpack that?")! with { FormalAddress = true }));
    }
}
