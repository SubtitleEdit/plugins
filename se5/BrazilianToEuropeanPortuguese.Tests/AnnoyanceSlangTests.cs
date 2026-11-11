using SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;
using SubtitleEdit.Plugins.Shared;

namespace BrazilianToEuropeanPortuguese.Tests;

public sealed class AnnoyanceSlangTests
{
    [Theory]
    [InlineData("quando um pé no saco da polícia\ncomeçou a fazer perguntas.",
        "when some NYPD dick started asking questions.",
        "quando um chato da polícia\ncomeçou a fazer perguntas.")]
    [InlineData("Ele é um pé no saco.", null, "Ele é um chato.")]
    [InlineData("Ela é uma pé no saco.", null, "Ela é uma chata.")]
    [InlineData("Eles são uns pés no saco.", null, "Eles são uns chatos.")]
    [InlineData("Elas são umas pés no saco.", null, "Elas são umas chatas.")]
    [InlineData("Ele era um pé-no-saco.", null, "Ele era um chato.")]
    [InlineData("Isso é um pé no saco.", null, "Isso é uma chatice.")]
    [InlineData("O trabalho era um pé no saco.", null, "O trabalho era uma chatice.")]
    [InlineData("É um pé no saco.", null, "É um incómodo.")]
    [InlineData("ELE É UM PÉ NO SACO.", null, "ELE É UM CHATO.")]
    [InlineData("<i>Ele é um pé no saco.</i>", null, "<i>Ele é um chato.</i>")]
    [InlineData("Ele me enche o saco.", "He annoys me.", "Ele chateia-me.")]
    [InlineData("Ele me encheu o saco.", null, "Ele chateou-me.")]
    [InlineData("Não me enche o saco.", "He doesn't annoy me.", "Não me chateia.")]
    [InlineData("Ele enche o meu saco.", null, "Ele chateia-me.")]
    [InlineData("Ele enche meu saco.", null, "Ele chateia-me.")]
    [InlineData("Ela torra o nosso saco.", null, "Ela chateia-nos.")]
    [InlineData("Eles me torravam o saco.", null, "Eles chateavam-me.")]
    [InlineData("Para de me encher o saco.", "Stop bothering me.", "Para de me chatear.")]
    [InlineData("Para de encher o saco.", null, "Para de chatear.")]
    [InlineData("Para de torrar meu saco.", null, "Para de me chatear.")]
    [InlineData("Ele te torrou o saco.", null, "Ele chateou-te.")]
    [InlineData("Ele nos enchia o saco.", null, "Ele chateava-nos.")]
    [InlineData("Ele enche-me o saco.", null, "Ele chateia-me.")]
    [InlineData("Ele torrou-me o saco.", null, "Ele chateou-me.")]
    [InlineData("Encher o saco.", "To annoy.", "Chatear.")]
    [InlineData("Torrar o saco.", "To pester.", "Chatear.")]
    [InlineData("ELE ME ENCHE O SACO.", null, "ELE CHATEIA-ME.")]
    [InlineData("<i>Ele me torrou o saco.</i>", null, "<i>Ele chateou-me.</i>")]
    public void ReviewedAnnoyanceExpressionsUseEuropeanEquivalents(string source, string? english, string expected)
    {
        var converter = new PortugueseConverter();
        var hint = ReferenceHint.FromText(english);
        Assert.Equal(expected, converter.Convert(source, reference: hint));
        Assert.Equal(expected, converter.Convert(expected, reference: hint));
    }

    [Theory]
    [InlineData("Pôs um pé no saco.", null)]
    [InlineData("Ele pôs um pé no saco de dormir.", null)]
    [InlineData("Ele está com um pé no saco.", null)]
    [InlineData("Encheu o saco de batatas.", null)]
    [InlineData("Encheu o saco com dinheiro.", null)]
    [InlineData("Ele encheu o saco.", "He made a lot of money.")]
    [InlineData("Ele torrou o saco.", null)]
    [InlineData("Ele torrou o saco de café.", null)]
    [InlineData("Ele encheu o meu saco com arroz.", null)]
    [InlineData("Ele encheu-me o saco de moedas.", null)]
    [InlineData("Ele encheu o meu saco até cima.", null)]
    [InlineData("Ele encheu o meu saco.", "He filled my bag.")]
    [InlineData("Ele encherá o meu saco.", null)]
    [InlineData("O saco está cheio.", null)]
    public void LiteralAndFinancialSensesArePreserved(string source, string? english) =>
        Assert.Equal(source, new PortugueseConverter().Convert(source, reference: ReferenceHint.FromText(english)));

    [Fact]
    public void OptionsIgnoredPhrasesAndCustomWordsWin()
    {
        const string source = "Ele é um pé no saco.";
        foreach (var options in new[]
        {
            new ConversionOptions(Vocabulary: false),
            new ConversionOptions(Colloquial: false),
            new ConversionOptions(DisabledRules: "annoyance-slang"),
        })
            Assert.Equal(source, new PortugueseConverter(options).Convert(source));
        var words = new PortugueseLocalWordList();
        words.Ignored.Add("pé no saco");
        words.Ignored.Add("encher o saco");
        var converter = new PortugueseConverter(localWordList: words);
        Assert.Equal(source, converter.Convert(source));
        Assert.Equal("Ele enche-me o saco.", converter.Convert("Ele me enche o saco."));
        Assert.Equal("Ele me enche o saco.", new PortugueseConverter(new ConversionOptions(Pronouns: false))
            .Convert("Ele me enche o saco."));
        Assert.Equal("Ele é um chato.", new PortugueseConverter(new ConversionOptions(Pronouns: false)).Convert(source));
        var custom = new PortugueseLocalWordList();
        custom.Words.Add(("pé no saco", "melga"));
        Assert.Equal("Ele é um melga.", new PortugueseConverter(localWordList: custom).Convert(source));
    }

    [Fact]
    public void ExactCuePreservesBothLinesAndTiming()
    {
        var cue = new SrtBlock { StartMs = 807371, EndMs = 810759,
            Text = "quando um pé no saco da polícia\ncomeçou a fazer perguntas." };
        var result = Assert.Single(SubtitleConversion.Convert(new[] { cue }, ConversionOptions.Default,
            english: new string?[] { "when some NYPD dick started asking questions." }));
        Assert.Equal("quando um chato da polícia\ncomeçou a fazer perguntas.", result.Text);
        Assert.Equal(807371, cue.StartMs);
        Assert.Equal(810759, cue.EndMs);
    }
}
