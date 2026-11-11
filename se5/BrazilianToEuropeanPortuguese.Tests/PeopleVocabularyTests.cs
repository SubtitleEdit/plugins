using SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;
using SubtitleEdit.Plugins.Shared;

namespace BrazilianToEuropeanPortuguese.Tests;

public sealed class PeopleVocabularyTests
{
    [Theory]
    [InlineData("Os meninos estão crescendo bem.", "The boys are growing into the role.", "Os rapazes estão a crescer bem.")]
    [InlineData("As meninas chegaram.", "The girls arrived.", "As raparigas chegaram.")]
    [InlineData("O menino chegou.", "The boy arrived.", "O rapaz chegou.")]
    [InlineData("A menina chegou.", "The girl arrived.", "A rapariga chegou.")]
    [InlineData("Os garotos chegaram.", "The boys arrived.", "Os rapazes chegaram.")]
    [InlineData("As garotas chegaram.", "The girls arrived.", "As raparigas chegaram.")]
    [InlineData("Os miúdos chegaram.", "The boys arrived.", "Os rapazes chegaram.")]
    [InlineData("As miúdas chegaram.", "The girls arrived.", "As raparigas chegaram.")]
    [InlineData("Os moleques chegaram.", "The boys arrived.", "Os rapazes chegaram.")]
    [InlineData("As molecas chegaram.", "The girls arrived.", "As raparigas chegaram.")]
    [InlineData("Os moços chegaram.", "The boys arrived.", "Os rapazes chegaram.")]
    [InlineData("As moças chegaram.", "The girls arrived.", "As raparigas chegaram.")]
    [InlineData("Os guris chegaram.", "The boys arrived.", "Os rapazes chegaram.")]
    [InlineData("As gurias chegaram.", "The girls arrived.", "As raparigas chegaram.")]
    [InlineData("Os meninos e as meninas chegaram.", "The boys and girls arrived.", "Os rapazes e as raparigas chegaram.")]
    [InlineData("Os meninos pequenos chegaram.", "The little boys arrived.", "Os rapazes pequenos chegaram.")]
    [InlineData("As meninas pequenas chegaram.", "The little girls arrived.", "As raparigas pequenas chegaram.")]
    [InlineData("Os meninos chegaram com as crianças.", "The boys arrived with the children.", "Os rapazes chegaram com as crianças.")]
    [InlineData("OS MENINOS CHEGARAM.", "The boys arrived.", "OS RAPAZES CHEGARAM.")]
    [InlineData("<i>As meninas chegaram.</i>", "The girls arrived.", "<i>As raparigas chegaram.</i>")]
    [InlineData("Por favor, minha garota.\nEla precisa de ajuda.", "Please, my girlfriend-- she needs help.",
        "Por favor, minha namorada.\nEla precisa de ajuda.")]
    [InlineData("A menina chegou.", "My girlfriend arrived.", "A namorada chegou.")]
    [InlineData("O menino chegou.", "My boyfriend arrived.", "O namorado chegou.")]
    [InlineData("A minha rapariga chegou.", "My girlfriend arrived.", "A minha namorada chegou.")]
    [InlineData("O meu rapaz chegou.", "My boyfriend arrived.", "O meu namorado chegou.")]
    [InlineData("A moça chegou.", "His girlfriend arrived.", "A namorada chegou.")]
    [InlineData("A guria chegou.", "His girlfriend arrived.", "A namorada chegou.")]
    [InlineData("O guri chegou.", "Her boyfriend arrived.", "O namorado chegou.")]
    [InlineData("As garotas chegaram.", "Their girlfriends arrived.", "As namoradas chegaram.")]
    [InlineData("Os garotos chegaram.", "Their boyfriends arrived.", "Os namorados chegaram.")]
    [InlineData("A ex-garota chegou.", "His ex-girlfriend arrived.", "A ex-namorada chegou.")]
    [InlineData("A GAROTA CHEGOU.", "His girlfriend arrived.", "A NAMORADA CHEGOU.")]
    [InlineData("<i>A minha miúda chegou.</i>", "My girlfriend arrived.", "<i>A minha namorada chegou.</i>")]
    public void ExplicitEnglishGenderedNounsCoverSourceVariants(string source, string english, string expected)
    {
        var converter = new PortugueseConverter();
        var reference = ReferenceHint.FromText(english);
        Assert.Equal(expected, converter.Convert(source, reference: reference));
        Assert.Equal(expected, converter.Convert(expected, reference: reference));
    }

    [Theory]
    [InlineData("Os meninos chegaram.", null, "Os meninos chegaram.")]
    [InlineData("As meninas chegaram.", "The children arrived.", "As meninas chegaram.")]
    [InlineData("Os garotos chegaram.", "The kids arrived.", "Os miúdos chegaram.")]
    [InlineData("A minha garota chegou.", null, "A minha miúda chegou.")]
    [InlineData("A minha garota chegou.", "My daughter arrived.", "A minha miúda chegou.")]
    [InlineData("A menina chegou.", "My friend arrived.", "A menina chegou.")]
    [InlineData("A menina e a namorada chegaram.", "The girl and his girlfriend arrived.", "A rapariga e a namorada chegaram.")]
    [InlineData("A menina e a moça chegaram.", "His girlfriend and her sister arrived.", "A menina e a rapariga chegaram.")]
    [InlineData("O menino chegou.", "His girlfriend arrived.", "O menino chegou.")]
    [InlineData("As meninas chegaram.", "His girlfriend arrived.", "As meninas chegaram.")]
    [InlineData("A menina chegou.", "His girlfriends arrived.", "A menina chegou.")]
    [InlineData("A minha namorada chegou.", "My girlfriend arrived.", "A minha namorada chegou.")]
    [InlineData("Os meninos chegaram.", "They arrived.", "Os meninos chegaram.")]
    public void MissingOrDifferentEnglishDoesNotInventBoyGirlEvidence(string source, string? english, string expected) =>
        Assert.Equal(expected, new PortugueseConverter().Convert(source, reference: ReferenceHint.FromText(english)));

    [Theory]
    [InlineData("Tento me lembrar\nque sou rico de parceiros,", "Keep trying to remind myself I'm rich with partners, but", "Tento lembrar-me\nque tenho sorte com parceiros,")]
    [InlineData("Sou rica de amigas.", "I'm rich in friends.", "Tenho sorte com amigas.")]
    [InlineData("Somos ricos em parceiros.", "We're rich with partners.", "Temos sorte com parceiros.")]
    [InlineData("Ela é rica de amigos.", "She is rich in friends.", "Ela tem sorte com amigos.")]
    [InlineData("Eles são ricos em amigos.", "They are rich in friends.", "Eles têm sorte com amigos.")]
    [InlineData("Eu era rico de amigos.", "I was rich in friends.", "Eu tinha sorte com amigos.")]
    [InlineData("Serei rico de aliados.", "I'll be rich in allies.", "Terei sorte com aliados.")]
    [InlineData("Seria rica em companheiras.", "I'd be rich in companions.", "Teria sorte com companheiras.")]
    [InlineData("Não sou rico de amigos.", "I'm not rich in friends.", "Não tenho sorte com amigos.")]
    [InlineData("Não fui rico de amigos.", "I was not rich in friends.", "Não tive sorte com amigos.")]
    [InlineData("Que sejamos ricos em amigos.", "May we be rich in friends.", "Que tenhamos sorte com amigos.")]
    [InlineData("Se fossem ricos de amigos.", "If they were rich in friends.", "Se tivessem sorte com amigos.")]
    [InlineData("Sou rico em bons amigos.", "I'm rich in good friends.", "Tenho sorte com bons amigos.")]
    [InlineData("Somos ricos de colegas.", "We're rich with colleagues.", "Temos sorte com colegas.")]
    [InlineData("É bom ser rico de amigos.", "It's good to be rich in friends.", "É bom ter sorte com amigos.")]
    [InlineData("SOU RICO DE PARCEIROS.", "I'm rich with partners.", "TENHO SORTE COM PARCEIROS.")]
    [InlineData("<i>Sou rico de amigos.</i>", "I'm rich in friends.", "<i>Tenho sorte com amigos.</i>")]
    public void FigurativeHumanWealthUsesNaturalFortuneConstruction(string source, string english, string expected)
    {
        var converter = new PortugueseConverter();
        var reference = ReferenceHint.FromText(english);
        Assert.Equal(expected, converter.Convert(source, reference: reference));
        Assert.Equal(expected, converter.Convert(expected, reference: reference));
    }

    [Theory]
    [InlineData("Sou rico de parceiros.", null)]
    [InlineData("Sou rico de parceiros.", "My partners made me rich.")]
    [InlineData("Sou rico de parceiros.", "I'm rich with partners' money.")]
    [InlineData("Sou rico em dinheiro.", "I'm rich in money.")]
    [InlineData("O leite é rico em cálcio.", "Milk is rich in calcium.")]
    [InlineData("O país é rico em recursos.", "The country is rich in resources.")]
    public void LiteralAndUnconfirmedWealthIsNotReinterpreted(string source, string? english) =>
        Assert.Equal(source, new PortugueseConverter().Convert(source, reference: ReferenceHint.FromText(english)));

    [Fact]
    public void SettingsAndLocalOverridesAreRespected()
    {
        var hint = ReferenceHint.FromText("The boys and girls arrived.");
        var words = new PortugueseLocalWordList();
        words.Words.Add(("meninos", "jovens"));
        words.Ignored.Add("meninas");
        Assert.Equal("Os jovens e as meninas chegaram.",
            new PortugueseConverter(localWordList: words).Convert("Os meninos e as meninas chegaram.", reference: hint));
        Assert.Equal("Os meninos chegaram.", new PortugueseConverter(new ConversionOptions(Vocabulary: false))
            .Convert("Os meninos chegaram.", reference: hint));
        Assert.Equal("Os meninos chegaram.", new PortugueseConverter(new ConversionOptions(DisabledRules: "verb-vocabulary"))
            .Convert("Os meninos chegaram.", reference: hint));
        var rich = ReferenceHint.FromText("I'm rich with partners.");
        foreach (var options in new[]
        {
            new ConversionOptions(Vocabulary: false),
            new ConversionOptions(Colloquial: false),
            new ConversionOptions(DisabledRules: "reference-idioms"),
        })
            Assert.Equal("Sou rico de parceiros.", new PortugueseConverter(options).Convert("Sou rico de parceiros.", reference: rich));
    }

    [Fact]
    public void PartnerVocabularyRespectsSettingsAndLocalOverrides()
    {
        var hint = ReferenceHint.FromText("My girlfriend arrived.");
        var words = new PortugueseLocalWordList();
        words.Words.Add(("menina", "jovem"));
        words.Ignored.Add("garota");
        var converter = new PortugueseConverter(localWordList: words);
        Assert.Equal("A jovem chegou.", converter.Convert("A menina chegou.", reference: hint));
        Assert.Equal("A garota chegou.", converter.Convert("A garota chegou.", reference: hint));
        foreach (var options in new[] { new ConversionOptions(Vocabulary: false),
            new ConversionOptions(DisabledRules: "verb-vocabulary") })
            Assert.Equal("A menina chegou.", new PortugueseConverter(options).Convert("A menina chegou.", reference: hint));
    }

    [Fact]
    public void ScreenshotPartnerUsesAlignedEnglishAndCannotBeChangedBackByJudge()
    {
        const string source = "Por favor, minha garota.\nEla precisa de ajuda.";
        var blocks = new[] { new SrtBlock { StartMs = 1000, EndMs = 4000, Text = source } };
        var english = new[] { "Please, my girlfriend--\nshe needs help." };
        var result = Assert.Single(SubtitleConversion.Convert(blocks, new ConversionOptions(), english: english));
        Assert.Equal("Por favor, minha namorada.\nEla precisa de ajuda.", result.Text);
        Assert.Empty(JudgeAlternatives.Generate(source, result.Text));
        Assert.Equal(1000, blocks[0].StartMs);
        Assert.Equal(4000, blocks[0].EndMs);
    }
}
