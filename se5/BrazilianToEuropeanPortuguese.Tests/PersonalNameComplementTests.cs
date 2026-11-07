using SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;

namespace BrazilianToEuropeanPortuguese.Tests;

public sealed class PersonalNameComplementTests
{
    [Theory]
    [InlineData("até mesmo para\npensar em Linda.", "até mesmo para\npensar na Linda.")]
    [InlineData("Penso em Maria.", "Penso na Maria.")]
    [InlineData("Pensas em João.", "Pensas no João.")]
    [InlineData("Ele pensa em Ana.", "Ele pensa na Ana.")]
    [InlineData("Pensamos em Pedro.", "Pensamos no Pedro.")]
    [InlineData("Eles pensam em Sofia.", "Eles pensam na Sofia.")]
    [InlineData("Pensei em Miguel.", "Pensei no Miguel.")]
    [InlineData("Ela pensou em Teresa.", "Ela pensou na Teresa.")]
    [InlineData("Pensávamos em Carlos.", "Pensávamos no Carlos.")]
    [InlineData("Eles pensarão em Beatriz.", "Eles pensarão na Beatriz.")]
    [InlineData("Pensaria em Daniel.", "Pensaria no Daniel.")]
    [InlineData("Se pensarmos em Mariana.", "Se pensarmos na Mariana.")]
    [InlineData("Não penso em Sara.", "Não penso na Sara.")]
    [InlineData("Penso sempre em Linda.", "Penso sempre na Linda.")]
    [InlineData("Pensar em Ana e em João.", "Pensar na Ana e no João.")]
    [InlineData("pensar em Ana ou em Pedro.", "pensar na Ana ou no Pedro.")]
    [InlineData("pensar em Maria João.", "pensar na Maria João.")]
    [InlineData("pensar em Ana Silva.", "pensar na Ana Silva.")]
    [InlineData("Tenho pensado em Marta.", "Tenho pensado na Marta.")]
    [InlineData("Estou a pensar em José.", "Estou a pensar no José.")]
    [InlineData("pensar\nem\nLinda.", "pensar\nna\nLinda.")]
    [InlineData("<i>pensar em Linda.</i>", "<i>pensar na Linda.</i>")]
    public void ThinkComplementsUseArticlesWithReviewedPersonalNames(string source, string expected)
    {
        var converter = new PortugueseConverter();
        Assert.Equal(expected, converter.Convert(source));
        Assert.Equal(expected, converter.Convert(expected));
    }

    [Theory]
    [InlineData("pensar em alguém.")]
    [InlineData("pensar em pessoa alguma.")]
    [InlineData("pensar em pessoas.")]
    [InlineData("pensar em sair.")]
    [InlineData("pensar em Lisboa.")]
    [InlineData("pensar em Portugal.")]
    [InlineData("pensar em Deus.")]
    [InlineData("pensar em linda companhia.")]
    [InlineData("pensar em rosas.")]
    [InlineData("pensar em soluções.")]
    [InlineData("pensar na Linda.")]
    [InlineData("pensar em Xanthe.")]
    [InlineData("falar em Linda.")]
    public void AmbiguousAndUnrelatedComplementsStayUnchanged(string source) =>
        Assert.Equal(source, new PortugueseConverter().Convert(source));

    [Fact]
    public void PronounOptionControlsNameArticles() =>
        Assert.Equal("pensar em Linda.",
            new PortugueseConverter(new ConversionOptions(Pronouns: false)).Convert("pensar em Linda."));

    [Fact]
    public void IndividualRuleCanBeDisabled() =>
        Assert.Equal("pensar em Linda.",
            new PortugueseConverter(new ConversionOptions(DisabledRules: "personal-name-complements"))
                .Convert("pensar em Linda."));

    [Fact]
    public void LocalListsKeepPriority()
    {
        var ignored = new PortugueseLocalWordList();
        ignored.Ignored.Add("pensar em Linda");
        Assert.Equal("pensar em Linda.", new PortugueseConverter(localWordList: ignored)
            .Convert("pensar em Linda."));

        var custom = new PortugueseLocalWordList();
        custom.Words.Add(("pensar em Linda", "recordar a Linda"));
        Assert.Equal("recordar a Linda.", new PortugueseConverter(localWordList: custom)
            .Convert("pensar em Linda."));
    }
}
