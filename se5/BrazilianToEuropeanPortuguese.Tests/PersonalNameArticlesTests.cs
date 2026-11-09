using SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;
using SubtitleEdit.Plugins.Shared;
using System.Text.Json;

namespace BrazilianToEuropeanPortuguese.Tests;

public sealed class PersonalNameArticlesTests
{
    [Fact]
    public void UnresolvedNamesAreCollectedOnlyAtSupportedArticleDecisions()
    {
        var unresolved = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var converter = new PortugueseConverter(unresolvedNameArticles: unresolved);
        converter.Convert("Jamie chegou.");
        converter.Convert("pensar em João e em Maria.");
        converter.Convert("pensar em soluções.");
        converter.Convert("pensar em Jamie e em Jamie.");
        Assert.Equal(new[] { "Jamie" }, unresolved);
        unresolved.Clear();
        converter.Convert("Ligue para Alex?", reference: ReferenceHint.FromText("Should I call Alex?"));
        Assert.Equal(new[] { "Alex" }, unresolved);
        unresolved.Clear();
        converter.Convert("Sobre o que você e Xanthe discutiram?",
            reference: ReferenceHint.FromText("What did you and Xanthe fight about?"));
        Assert.Equal(new[] { "Xanthe" }, unresolved);
    }

    [Fact]
    public void SavedChoicesAndDisabledStagesDoNotReportNames()
    {
        var unresolved = new HashSet<string>();
        var options = new ConversionOptions(NameArticles: "Jamie = none\nAlex = a");
        new PortugueseConverter(options, unresolvedNameArticles: unresolved).Convert("pensar em Jamie e em Alex.");
        Assert.Empty(unresolved);
        options = new ConversionOptions(DisabledRules: "personal-name-complements,action-questions,natural-word-order");
        var converter = new PortugueseConverter(options, unresolvedNameArticles: unresolved);
        converter.Convert("pensar em Jamie.");
        converter.Convert("Ligue para Alex?", reference: ReferenceHint.FromText("Should I call Alex?"));
        converter.Convert("Sobre o que você e Xanthe discutiram?",
            reference: ReferenceHint.FromText("What did you and Xanthe fight about?"));
        Assert.Empty(unresolved);
        new PortugueseConverter(new ConversionOptions(Pronouns: false), unresolvedNameArticles: unresolved)
            .Convert("pensar em Jamie.");
        Assert.Empty(unresolved);
    }

    [Fact]
    public void SelectionLimitsNameQuestionsAndSettingsRememberOptOut()
    {
        var unresolved = new HashSet<string>();
        var blocks = new List<SrtBlock>
        {
            new() { StartMs = 1000, EndMs = 2000, Text = "pensar em Jamie." },
            new() { StartMs = 3000, EndMs = 4000, Text = "pensar em Alex." },
        };
        SubtitleConversion.Convert(blocks, ConversionOptions.Default, selected: new HashSet<int> { 1 },
            unresolvedNameArticles: unresolved);
        Assert.Equal(new[] { "Alex" }, unresolved);
        Assert.True(ConversionOptions.Load(new PluginRequest()).AskNameArticles);
        var options = new ConversionOptions(AskNameArticles: false);
        Assert.False(ConversionOptions.Load(new PluginRequest
            { SettingsVersion = ConversionOptions.Version, Settings = options.ToJson() }).AskNameArticles);
    }

    [Theory]
    [InlineData("Jamie = o", "pensar em Jamie.", "pensar no Jamie.", "Ligo para o Jamie?")]
    [InlineData("jamie = a", "pensar em Jamie.", "pensar na Jamie.", "Ligo para a Jamie?")]
    [InlineData("Maria = o", "pensar em Maria.", "pensar no Maria.", "Ligo para o Maria?")]
    [InlineData("Maria = none", "pensar em Maria.", "pensar em Maria.", "Ligo para Maria?")]
    public void ChoicesApplyToSupportedRules(string list, string source, string expected, string question)
    {
        var name = source.Split(' ').Last().TrimEnd('.');
        var converter = new PortugueseConverter(new ConversionOptions(NameArticles: list));
        Assert.Equal(expected, converter.Convert(source));
        Assert.Equal(question, converter.Convert($"Ligue para {name}?", reference: ReferenceHint.FromText("Should I call them?")));
        Assert.Equal(expected, converter.Convert(expected));
    }

    [Fact]
    public void ChoicesRespectRuleOptionsAndDoNotTurnLowercaseWordsIntoNames()
    {
        var options = new ConversionOptions(NameArticles: "rosa = o", Pronouns: false);
        Assert.Equal("pensar em Rosa.", new PortugueseConverter(options).Convert("pensar em Rosa."));
        Assert.Equal("pensar em rosa.", new PortugueseConverter(options with { Pronouns = true }).Convert("pensar em rosa."));
        Assert.Equal("pensar em Rosa.", new PortugueseConverter(options with
            { Pronouns = true, DisabledRules = "personal-name-complements" }).Convert("pensar em Rosa."));
    }

    [Theory]
    [InlineData("Jamie = a", "Tu e a Jamie discutiram sobre o quê?")]
    [InlineData("Jamie = o", "Tu e o Jamie discutiram sobre o quê?")]
    [InlineData("Jamie = none", "Você e Jamie discutiram sobre o quê?")]
    public void ChoicesAlsoApplyToNaturalWordOrder(string list, string expected)
    {
        var converter = new PortugueseConverter(new ConversionOptions(NameArticles: list));
        Assert.Equal(expected, converter.Convert("Sobre o que você e Jamie discutiram?",
            reference: ReferenceHint.FromText("What did you and Jamie fight about?")));
    }

    [Fact]
    public void SettingsRoundTripAndApplyToSeparateBatchFiles()
    {
        var configured = BatchMode.BuildConfigureResponse(new ConversionOptions(NameArticles: "Jamie = a\nAlex = o"));
        Assert.Equal(PluginStatus.Ok, configured.Status);
        var request = new PluginRequest
        {
            RequestType = PluginRequestType.Batch,
            SettingsVersion = configured.SettingsVersion,
            Settings = configured.Settings,
        };
        Assert.Equal("Jamie = a\nAlex = o", ConversionOptions.Load(request).NameArticles);
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "words.xml");
        foreach (var (source, expected) in new[] { ("pensar em Jamie.", "pensar na Jamie."), ("pensar em Alex.", "pensar no Alex.") })
        {
            request.Subtitle = new PluginSubtitle { Format = "SubRip", SubRip = $"1\n00:00:01,000 --> 00:00:02,000\n{source}\n\n" };
            var response = BatchMode.BuildBatchResponse(request, path, new EmbeddingSettings(CheckMeaning: false));
            Assert.Equal(PluginStatus.Ok, response.Status);
            Assert.Equal(expected, Assert.Single(SubRipParser.Parse(response.Subtitle!.Native)).Text);
        }
        Assert.Equal("", ConversionOptions.Load(new PluginRequest()).NameArticles);
    }

    [Theory]
    [InlineData("Jamie")]
    [InlineData("Jamie = x")]
    [InlineData("Jamie = o\nJAMIE = a")]
    [InlineData("Jamie Smith = o")]
    public void InvalidListsFailExplicitly(string list)
    {
        Assert.Throws<InvalidDataException>(() => new PortugueseConverter(new ConversionOptions(NameArticles: list)));
        var request = new PluginRequest { SettingsVersion = ConversionOptions.Version,
            Settings = JsonSerializer.SerializeToElement(new { nameArticles = list }) };
        Assert.Throws<InvalidDataException>(() => ConversionOptions.Load(request));
    }
}
