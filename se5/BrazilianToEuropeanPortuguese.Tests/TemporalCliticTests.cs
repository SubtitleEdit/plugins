using System.Text.RegularExpressions;
using SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;
using SubtitleEdit.Plugins.Shared;

namespace BrazilianToEuropeanPortuguese.Tests;

public sealed class TemporalCliticTests
{
    [Theory]
    [InlineData("e depois as vende\npara a escravidão sexual.", "e depois vende-as\npara a escravidão sexual.")]
    [InlineData("E depois o vende.", "E depois vende-o.")]
    [InlineData("Mas depois a vende.", "Mas depois vende-a.")]
    [InlineData("Depois os vende.", "Depois vende-os.")]
    [InlineData("Depois as vendem.", "Depois vendem-nas.")]
    [InlineData("Depois o fez.", "Depois fê-lo.")]
    [InlineData("Depois os vimos.", "Depois vimo-los.")]
    [InlineData("Amanhã as venderá.", "Amanhã vendê-las-á.")]
    [InlineData("Depois lhe contou tudo.", "Depois contou-lhe tudo.")]
    [InlineData("Depois lhes contou tudo.", "Depois contou-lhes tudo.")]
    [InlineData("Depois vos conto tudo.", "Depois conto-vos tudo.")]
    [InlineData("E depois me arrependo.", "E depois arrependo-me.")]
    [InlineData("E depois te encontrou.", "E depois encontrou-te.")]
    [InlineData("E depois se sentou.", "E depois sentou-se.")]
    [InlineData("E depois nos encontramos.", "E depois encontramo-nos.")]
    [InlineData("Ontem a vimos.", "Ontem vimo-la.")]
    [InlineData("Hoje o encontrei.", "Hoje encontrei-o.")]
    [InlineData("Agora as vendem.", "Agora vendem-nas.")]
    [InlineData("Ele chega e depois as vende.", "Ele chega e depois vende-as.")]
    [InlineData("Ele chega, mas depois as vende.", "Ele chega, mas depois vende-as.")]
    [InlineData("Depois, as vende.", "Depois, vende-as.")]
    [InlineData("<i>e depois as vende.</i>", "<i>e depois vende-as.</i>")]
    [InlineData("-E depois as vende.\n-Eu sei.", "-E depois vende-as.\n-Eu sei.")]
    [InlineData("E DEPOIS AS VENDE.", "E DEPOIS VENDE-AS.")]
    public void NeutralAdverbsSupportEveryPlainClitic(string source, string expected)
    {
        var converter = new PortugueseConverter();
        var result = converter.Convert(source);
        Assert.Equal(expected, result);
        Assert.Equal(result, converter.Convert(result));
    }

    [Theory]
    [InlineData("Só depois as vende.")]
    [InlineData("Apenas depois as vende.")]
    [InlineData("Também depois as vende.")]
    [InlineData("Nem depois as vende.")]
    [InlineData("Não é depois que as vende.")]
    [InlineData("Quando depois as vende.")]
    [InlineData("Disse que depois as vende.")]
    [InlineData("Só\ndepois as vende.")]
    [InlineData("Quando\ndepois as vende.")]
    [InlineData("Depois não as vende.")]
    [InlineData("Depois já as vende.")]
    [InlineData("Depois as janelas.")]
    [InlineData("Hoje as vendas aumentaram.")]
    [InlineData("Hoje os guardas chegaram.")]
    [InlineData("Depois a conta chegou.")]
    [InlineData("Depois nos casos graves.")]
    [InlineData("Depois de as vender.")]
    [InlineData("Depois de se sentar.")]
    [InlineData("-Depois\n-As vende.")]
    public void FocusSubordinationNominalUsesAndSpeakersStayProtected(string text) =>
        Assert.Equal(text, new PortugueseConverter(new ConversionOptions(YouToTu: false)).Convert(text));

    [Theory]
    [InlineData("e depois as vende para a escravidão sexual.")]
    [InlineData("E depois lhes contou tudo.")]
    [InlineData("Depois os vende.")]
    [InlineData("Depois as vendem.")]
    public void EveryOrdinaryWrapKeepsTheSameCorrection(string source)
    {
        var converter = new PortugueseConverter();
        var expected = converter.Convert(source);
        for (var index = 0; index < source.Length; index++)
        {
            if (source[index] != ' ') continue;
            foreach (var newline in new[] { "\n", "\r\n" })
            {
                var result = converter.Convert(source[..index] + newline + source[(index + 1)..]);
                Assert.Equal(expected, Regex.Replace(result, @"\s+", " "));
                Assert.Equal(1, result.Count(c => c == '\n'));
                Assert.Equal(result, converter.Convert(result));
            }
        }
    }

    [Fact]
    public void RuleSettingsAndProtectedTextWin()
    {
        const string source = "e depois as vende.";
        foreach (var options in new[] { new ConversionOptions(Pronouns: false),
            new ConversionOptions(DisabledRules: "pronouns") })
            Assert.Equal(source, new PortugueseConverter(options).Convert(source));
        Assert.Equal("e depois vende-as.", new PortugueseConverter(new ConversionOptions(
            Vocabulary: false, Colloquial: false, YouToTu: false)).Convert(source));
        var ignored = new PortugueseLocalWordList();
        ignored.Ignored.Add("as vende");
        Assert.Equal(source, new PortugueseConverter(localWordList: ignored).Convert(source));
        var custom = new PortugueseLocalWordList();
        custom.Words.Add(("as vende", "vende-as"));
        Assert.Equal("e depois vende-as.", new PortugueseConverter(localWordList: custom).Convert(source));
    }

    [Fact]
    public void ExactSharedCueDoesNotRequireReferenceOrChangeSource()
    {
        var cue = new SrtBlock { StartMs = 64199, EndMs = 66424,
            Text = "e depois as vende\npara a escravidão sexual." };
        var result = Assert.Single(SubtitleConversion.Convert(new[] { cue }, ConversionOptions.Default));
        Assert.Equal("e depois vende-as\npara a escravidão sexual.", result.Text);
        Assert.Equal("e depois as vende\npara a escravidão sexual.", cue.Text);
        Assert.Equal(64199, cue.StartMs);
        Assert.Equal(66424, cue.EndMs);
        Assert.Contains(result.Rules, rule => rule.Id == "pronouns");
    }
}
