using SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;
using SubtitleEdit.Plugins.Shared;

namespace BrazilianToEuropeanPortuguese.Tests;

public sealed class HabitualFamilyTests
{
    [Theory]
    [InlineData("Sempre me apoiam.", "Apoiam-me sempre.")]
    [InlineData("Sempre me dizem isso.", "Dizem-me sempre isso.")]
    [InlineData("Eles sempre me apoiam.", "Eles apoiam-me sempre.")]
    [InlineData("Ela sempre me apoia.", "Ela apoia-me sempre.")]
    [InlineData("Ele sempre nos apoiou.", "Ele apoiou-nos sempre.")]
    [InlineData("Eles sempre me apoiavam.", "Eles apoiavam-me sempre.")]
    [InlineData("Eles sempre te ajudam.", "Eles ajudam-te sempre.")]
    [InlineData("Sempre nos ajudam.", "Ajudam-nos sempre.")]
    [InlineData("Eles sempre se ajudam.", "Eles ajudam-se sempre.")]
    [InlineData("Eles sempre lhe respondem.", "Eles respondem-lhe sempre.")]
    [InlineData("Eles sempre os ajudam.", "Eles ajudam-nos sempre.")]
    [InlineData("Nós sempre nos ajudamos.", "Nós ajudamo-nos sempre.")]
    [InlineData("Sempre me defendem.", "Defendem-me sempre.")]
    [InlineData("Sempre me protegem.", "Protegem-me sempre.")]
    [InlineData("Sempre me acompanham.", "Acompanham-me sempre.")]
    [InlineData("Porque sempre me apoiam.", "Porque me apoiam sempre.")]
    [InlineData("Porque eles sempre me apoiam.", "Porque eles me apoiam sempre.")]
    [InlineData("Quando sempre me ajudam.", "Quando me ajudam sempre.")]
    [InlineData("Disse que eles sempre me ajudam.", "Disse que eles me ajudam sempre.")]
    [InlineData("SEMPRE ME APOIAM.", "APOIAM-ME SEMPRE.")]
    [InlineData("<i>Sempre me apoiam.</i>", "<i>Apoiam-me sempre.</i>")]
    [InlineData("Sempre me\napoiam.", "Apoiam-me\nsempre.")]
    [InlineData("Sempre, me apoiam.", "Sempre, apoiam-me.")]
    public void HabitualOrderAlsoPlacesCliticsCorrectly(string source, string expected) =>
        AssertConversion(source, null, expected);

    [Theory]
    [InlineData("Não me apoiam sempre.")]
    [InlineData("Nunca me apoiam.")]
    [InlineData("Sempre me apoiarão.")]
    [InlineData("Sempre me apoiariam.")]
    [InlineData("Eles sempre me podem ajudar.")]
    [InlineData("Sempre te achei muito inteligente.")]
    public void OtherAttractorsPausesAndUnreviewedVerbChainsAreNotReordered(string source) =>
        Assert.Equal(source, new PortugueseConverter().Convert(source));

    [Theory]
    [InlineData("É como Reagan são criados.", "They're Reagans. It's how we're raised.", "É como os Reagan são criados.")]
    [InlineData("É como Reagan são criados.", null, "É como os Reagan são criados.")]
    [InlineData("Reagan são criados assim.", null, "Os Reagan são criados assim.")]
    [InlineData("Reagan foram educadas assim.", null, "As Reagan foram educadas assim.")]
    [InlineData("Silva são unidos.", "The Silvas are close.", "Os Silva são unidos.")]
    [InlineData("Smith chegaram.", "The Smiths arrived.", "Os Smith chegaram.")]
    [InlineData("Smith estavam em casa.", "The Smiths were at home.", "Os Smith estavam em casa.")]
    [InlineData("Smith têm razão.", "The Smith family is right.", "Os Smith têm razão.")]
    [InlineData("Smith têm filhas.", "The Smiths have daughters.", "Os Smith têm filhas.")]
    [InlineData("Smith nunca chegaram.", "The Smiths never arrived.", "Os Smith nunca chegaram.")]
    [InlineData("Smith foram mortas.", "The Smiths were killed.", "As Smith foram mortas.")]
    [InlineData("Reagan sempre são criados assim.", null, "Os Reagan sempre são criados assim.")]
    [InlineData("Smith são irmãs.", "The Smiths are sisters.", "As Smith são irmãs.")]
    [InlineData("Disse que Reagan são criados assim.", null, "Disse que os Reagan são criados assim.")]
    [InlineData("É como Smith serão educados.", "It's how the Smiths will be raised.", "É como os Smith serão educados.")]
    [InlineData("Cerqueira da Mota chegaram.", "The Cerqueira da Motas arrived.", "Os Cerqueira da Mota chegaram.")]
    [InlineData("REAGAN SÃO CRIADOS ASSIM.", "The Reagans are raised this way.", "OS REAGAN SÃO CRIADOS ASSIM.")]
    [InlineData("<i>É como Reagan são criados.</i>", null, "<i>É como os Reagan são criados.</i>")]
    [InlineData("É como Reagan\nsão criados.", null, "É como os Reagan\nsão criados.")]
    public void PluralFamilySubjectsReceiveArticlesWithoutChangingSurnameSpelling(string source, string? english, string expected) =>
        AssertConversion(source, english, expected);

    [Theory]
    [InlineData("É como Reagan é criado.", "It's how Reagan is raised.")]
    [InlineData("É como os Reagan são criados.", "It's how the Reagans are raised.")]
    [InlineData("As Smith são irmãs.", "The Smiths are sisters.")]
    [InlineData("João e Maria são criados assim.", null)]
    [InlineData("Reagan, são criados assim?", null)]
    [InlineData("Crianças são criadas assim.", null)]
    [InlineData("Reagan são altos.", null)]
    [InlineData("Reagan são criados assim.", "Reagan is raised this way.")]
    [InlineData("É como eles são criados.", "It's how they are raised.")]
    [InlineData("É como todos são criados.", null)]
    public void SingularNamesCommonNounsCoordinationAndExistingArticlesAreProtected(string source, string? english) =>
        AssertConversion(source, english, source);

    [Fact]
    public void OptionsAndLocalExceptionsRemainEffective()
    {
        const string family = "É como Reagan são criados.";
        foreach (var options in new[]
        {
            new ConversionOptions(Pronouns: false),
            new ConversionOptions(DisabledRules: "family-subjects"),
            new ConversionOptions(NameArticles: "Reagan = none"),
        })
            Assert.Equal(family, new PortugueseConverter(options).Convert(family));
        foreach (var options in new[]
        {
            new ConversionOptions(Colloquial: false),
            new ConversionOptions(Pronouns: false),
            new ConversionOptions(DisabledRules: "natural-word-order"),
        })
            Assert.Equal("Sempre me apoiam.", new PortugueseConverter(options).Convert("Sempre me apoiam."));
        var words = new PortugueseLocalWordList();
        words.Ignored.Add("Sempre me apoiam");
        words.Ignored.Add("Reagan");
        var converter = new PortugueseConverter(localWordList: words);
        Assert.Equal("Sempre me apoiam.", converter.Convert("Sempre me apoiam."));
        Assert.Equal(family, converter.Convert(family));
    }

    [Fact]
    public void ExactEpisodeCueKeepsBothLinesAndTiming()
    {
        var cue = new SrtBlock { StartMs = 13362, EndMs = 16219,
            Text = "Sempre me apoiam.\nÉ como Reagan são criados." };
        var result = Assert.Single(SubtitleConversion.Convert(new[] { cue }, ConversionOptions.Default,
            english: new string?[] { "They always got my back. They're Reagans. It's how we're raised." }));
        Assert.Equal("Apoiam-me sempre.\nÉ como os Reagan são criados.", result.Text);
        Assert.Equal(13362, cue.StartMs);
        Assert.Equal(16219, cue.EndMs);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void FamilyTopicCanBeResumedByPluralMembersInFollowingCue(bool sentenceContext)
    {
        var blocks = new[]
        {
            new SrtBlock { StartMs = 11098, EndMs = 12492, Text = "A minha família..." },
            new SrtBlock { StartMs = 13362, EndMs = 16219,
                Text = "Sempre me apoiam.\nÉ como Reagan são criados." },
        };
        var result = SubtitleConversion.Convert(blocks, new ConversionOptions(SentenceContext: sentenceContext),
            english: new string?[] { "My family--", "They always got my back. They're Reagans. It's how we're raised." });
        Assert.Equal("A minha família...", result[0].Text);
        Assert.Equal("Apoiam-me sempre.\nÉ como os Reagan são criados.", result[1].Text);
        Assert.Equal("A minha família apoia-me sempre.", new PortugueseConverter()
            .Convert("A minha família me apoia sempre."));
    }

    private static void AssertConversion(string source, string? english, string expected)
    {
        var converter = new PortugueseConverter();
        var reference = ReferenceHint.FromText(english);
        Assert.Equal(expected, converter.Convert(source, reference: reference));
        Assert.Equal(expected, converter.Convert(expected, reference: reference));
    }
}
