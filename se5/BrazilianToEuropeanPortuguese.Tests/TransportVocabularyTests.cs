using System.Text.RegularExpressions;
using SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;

namespace BrazilianToEuropeanPortuguese.Tests;

public sealed class TransportVocabularyTests
{
    [Theory]
    [InlineData("Deu uma chupeta no meu carro no inverno passado.", "Deu arranque com cabos ao meu carro no inverno passado.")]
    [InlineData("Deu uma chucha no meu carro no inverno passado.", "Deu arranque com cabos ao meu carro no inverno passado.")]
    [InlineData("Dei chupeta no carro.", "Dei arranque com cabos ao carro.")]
    [InlineData("Ela dava uma chupeta na moto.", "Ela dava arranque com cabos à moto.")]
    [InlineData("Vamos dar uma chupeta no barco.", "Vamos dar arranque com cabos ao barco.")]
    [InlineData("Ele dará uma chupeta ao carro.", "Ele dará arranque com cabos ao carro.")]
    [InlineData("Fiz uma chupeta no carro.", "Fiz um arranque com cabos ao carro.")]
    [InlineData("Ela fez uma chupeta na lancha.", "Ela fez um arranque com cabos à lancha.")]
    [InlineData("Ele tinha dado uma chupeta no carro.", "Ele tinha dado arranque com cabos ao carro.")]
    [InlineData("DEU UMA CHUPETA NO CARRO.", "DEU ARRANQUE COM CABOS AO CARRO.")]
    [InlineData("<i>Deu uma chupeta no carro.</i>", "<i>Deu arranque com cabos ao carro.</i>")]
    public void JumpStartingUsesMechanicalSenseWithoutNeedingEnglish(string source, string expected)
    {
        var converter = new PortugueseConverter(new ConversionOptions(YouToTu: false));
        Assert.Equal(expected, converter.Convert(source));
        Assert.Equal(expected, converter.Convert(expected));
        for (var i = 0; i < source.Length; i++)
        {
            if (source[i] != ' ') continue;
            foreach (var newline in new[] { "\n", "\r\n" })
            {
                var result = converter.Convert(source[..i] + newline + source[(i + 1)..]);
                Assert.Equal(expected, Regex.Replace(result, @"\s+", " "));
                Assert.Equal(1, result.Count(c => c == '\n'));
            }
        }
    }

    public static IEnumerable<object[]> TransportTerms() => PortugueseConverter.BuiltInWords
        .SkipWhile(w => w.Br != "cabos de chupeta")
        .TakeWhile(w => w.Br != "retrato falado")
        .Select(w => new object[] { w.Br, w.Pt });

    [Theory]
    [MemberData(nameof(TransportTerms))]
    public void ReviewedTableSurvivesTheWholePipeline(string source, string expected)
    {
        var converter = new PortugueseConverter(new ConversionOptions(YouToTu: false));
        Assert.Equal(expected, converter.Convert(source));
        Assert.Equal(expected, converter.Convert(expected));
        Assert.Equal(expected.ToUpperInvariant(), converter.Convert(source.ToUpperInvariant()));
        for (var i = 0; i < source.Length; i++)
        {
            if (source[i] != ' ') continue;
            var result = converter.Convert("Tenho " + source[..i] + "\n" + source[(i + 1)..] + " aqui.");
            Assert.Equal("Tenho " + expected + " aqui.", Regex.Replace(result, @"\s+", " "));
            Assert.Equal(1, result.Count(c => c == '\n'));
        }
    }

    [Theory]
    [InlineData("O virabrequim novo.", "A cambota nova.")]
    [InlineData("Os virabrequins novos.", "As cambotas novas.")]
    [InlineData("O câmbio automático novo.", "A caixa de velocidades automática nova.")]
    [InlineData("O conjunto de pedivela novo.", "A pedaleira nova.")]
    [InlineData("A calota da roda nova.", "O tampão da roda novo.")]
    [InlineData("A lona de freio nova.", "O ferodo de travão novo.")]
    [InlineData("A chupeta do bebé.", "A chucha do bebé.")]
    [InlineData("Deu uma chupeta ao bebé no carro.", "Deu uma chucha ao bebé no carro.")]
    [InlineData("O câmbio do euro subiu.", "O câmbio do euro subiu.")]
    [InlineData("A roda está partida.", "A roda está partida.")]
    [InlineData("O sistema de ignição.", "O sistema de ignição.")]
    [InlineData("A correia dentada da máquina.", "A correia dentada da máquina.")]
    [InlineData("O tanque de guerra.", "O tanque de guerra.")]
    [InlineData("A corrente e a vela.", "A corrente e a vela.")]
    [InlineData("A bateria da banda.", "A bateria da banda.")]
    [InlineData("Uma bicicleta sem câmbio.", "Uma bicicleta sem câmbio.")]
    public void GenderAndUnqualifiedFalseFriendsAreProtected(string source, string expected) =>
        Assert.Equal(expected, new PortugueseConverter(new ConversionOptions(YouToTu: false)).Convert(source));

    [Fact]
    public void OptionsIgnoredPhrasesAndUserMappingsRemainAuthoritative()
    {
        const string source = "Deu uma chupeta no carro.";
        Assert.Equal(source, new PortugueseConverter(new ConversionOptions(Vocabulary: false, YouToTu: false)).Convert(source));
        Assert.Equal("Deu uma chucha no carro.", new PortugueseConverter(
            new ConversionOptions(DisabledRules: "transport-vocabulary", YouToTu: false)).Convert(source));
        var words = new PortugueseLocalWordList();
        words.Ignored.Add("chupeta");
        Assert.Equal(source, new PortugueseConverter(localWordList: words).Convert(source));
        words.Ignored.Clear();
        words.Words.Add(("chupeta", "ajuda"));
        Assert.Equal("Deu uma ajuda no carro.", new PortugueseConverter(localWordList: words).Convert(source));
        words.Words.Clear();
        words.Words.Add(("motor de partida", "motor de arranque personalizado"));
        Assert.Equal("motor de arranque personalizado", new PortugueseConverter(localWordList: words).Convert("motor de partida"));
        words.Words.Clear();
        words.Ignored.Add("caixa de câmbio");
        Assert.Equal("caixa de câmbio", new PortugueseConverter(localWordList: words).Convert("caixa de câmbio"));
    }

    [Theory]
    [InlineData("chupeta no carro", "ajuda de arranque", "Deu uma ajuda de arranque.")]
    [InlineData("Deu uma chupeta no carro", "Ajudou no arranque", "Ajudou no arranque.")]
    public void MultiwordLocalOverridesWinBeforeContextualRewriting(string key, string value, string expected)
    {
        var words = new PortugueseLocalWordList();
        words.Words.Add((key, value));
        var converter = new PortugueseConverter(new ConversionOptions(YouToTu: false), words);
        Assert.Equal(expected, converter.Convert("Deu uma chupeta no carro."));
        Assert.Equal(expected, Regex.Replace(converter.Convert("Deu uma chupeta\nno carro."), @"\s+", " "));
    }
}
