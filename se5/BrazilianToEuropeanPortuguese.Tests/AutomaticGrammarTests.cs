using System.Text.Json;
using SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;
using SubtitleEdit.Plugins.Shared;

namespace BrazilianToEuropeanPortuguese.Tests;

public sealed class AutomaticGrammarTests
{
    private static readonly PortugueseConverter Converter = new();

    [Theory]
    [InlineData("Já é o segunda de estupro\nque temos no campus hoje.", "Já é a segunda violação\nque temos no campus hoje.")]
    [InlineData("Já é o segunda de violação.", "Já é a segunda violação.")]
    [InlineData("Foi a terceiro de agressão.", "Foi a terceira agressão.")]
    [InlineData("É a segunda crime.", "É o segundo crime.")]
    [InlineData("São os primeiras de violações.", "São as primeiras violações.")]
    [InlineData("Este é a quinto de homicídio.", "Este é o quinto homicídio.")]
    [InlineData("É uma segundo tentativa.", "É uma segunda tentativa.")]
    [InlineData("Foi a primeira visita.", "Foi a primeira visita.")]
    [InlineData("JA É O SEGUNDA DE VIOLAÇÃO.", "JA É A SEGUNDA VIOLAÇÃO.")]
    public void OrdinalEventAgreementAndStrayDe(string source, string expected) => Converts(source, expected);

    [Theory]
    [InlineData("Nós está prontos.", "Nós estamos prontos.")]
    [InlineData("Eles não pode sair.", "Eles não podem sair.")]
    [InlineData("Elas sempre trabalha aqui.", "Elas trabalham sempre aqui.")]
    [InlineData("Nós chegou cedo.", "Nós chegámos cedo.")]
    [InlineData("Eles já aprendeu isso.", "Eles já aprenderam isso.")]
    [InlineData("Nós falaria com ela.", "Nós falaríamos com ela.")]
    [InlineData("Eles beberá água.", "Eles beberão água.")]
    [InlineData("Gosto o filme.", "Gosto do filme.")]
    [InlineData("Preciso uma equipa.", "Preciso de uma equipa.")]
    [InlineData("Gosto um pouco.", "Gosto um pouco.")]
    public void ReviewedGrammarWarningsBecomeCorrections(string source, string expected)
    {
        Converts(source, expected);
        Assert.Null(GrammarReview.Note(expected));
    }

    [Theory]
    [InlineData("Temos o retrato falado.", "Temos o retrato-robô.")]
    [InlineData("São retratos falados.", "São retratos-robô.")]
    [InlineData("Trabalho de meio período?", "Trabalho a tempo parcial?")]
    [InlineData("Um emprego de tempo integral.", "Um emprego a tempo inteiro.")]
    [InlineData("Obrigado, é tudo por hora.", "Obrigado, é tudo por agora.")]
    [InlineData("O comissário está a frente.", "O comissário está à frente.")]
    [InlineData("Estava na frente da casa.", "Estava à frente da casa.")]
    [InlineData("Qualquer coisa que venha a mente.", "Qualquer coisa que venha à mente.")]
    [InlineData("É difícil dizer que foi pelo o que aconteceu.", "É difícil dizer que foi pelo que aconteceu.")]
    [InlineData("Tenho um ideia melhor.", "Tenho uma ideia melhor.")]
    [InlineData("Faz todo sentido.", "Faz todo o sentido.")]
    [InlineData("Dei a ela todo dinheiro.", "Dei-lhe todo o dinheiro.")]
    public void RepeatedMissedEpisodeFamiliesBecomeSelectableChanges(string source, string expected) => Converts(source, expected);

    [Theory]
    [InlineData("É o segunda de manhã.")]
    [InlineData("É a segunda de várias visitas.")]
    [InlineData("É a segunda da lista.")]
    [InlineData("Na segunda de manhã.")]
    [InlineData("Ele refere-se a crimes.")]
    [InlineData("Vou à reunião.")]
    [InlineData("A polícia chegou.")]
    [InlineData("Eles podem sair.")]
    [InlineData("Nós trabalhamos aqui.")]
    [InlineData("Nós, que chegámos cedo, estamos prontos.")]
    [InlineData("Nós: está pronto.")]
    [InlineData("Tenho fazer isso.")]
    [InlineData("Cobra dez euros por hora.")]
    [InlineData("É um pagamento por hora.")]
    [InlineData("Ele perdeu meio período.")]
    [InlineData("Todo homem sabe.")]
    [InlineData("Toda mulher sabe.")]
    [InlineData("Todos nós sabemos.")]
    public void AmbiguousConstructionsAreNotGuessed(string source) => Assert.Equal(source, Converter.Convert(source));

    [Fact]
    public void SettingsDefaultToNoLayoutNoiseAndCanOptIn()
    {
        var text = "Esta linha comprida não é um problema de tradução.";
        var cue = new SrtBlock { Text = text, StartMs = 0, EndMs = 1000 };
        Assert.Null(SubtitleLayout.Review(text, cue, ConversionOptions.Default));
        var options = new ConversionOptions(LayoutWarnings: true);
        Assert.Contains("characters/second", SubtitleLayout.Review(text, cue, options));
        Assert.Equal(options, ConversionOptions.Load(new PluginRequest
        { SettingsVersion = ConversionOptions.Version, Settings = options.ToJson() }));
        Assert.Throws<InvalidDataException>(() => ConversionOptions.Load(new PluginRequest
        { SettingsVersion = ConversionOptions.Version, Settings = JsonSerializer.SerializeToElement(new { layoutWarnings = "yes" }) }));
        Assert.NotNull(GrammarReview.Note("Tenho fazer isso."));
    }

    [Fact]
    public void FixesRespectStageAndGroupSuppressionAndKeepMarkup()
    {
        var source = "Já é o segunda de violação.";
        Assert.Equal(source, new PortugueseConverter(new ConversionOptions(DisabledRules: "final-grammar")).Convert(source));
        Assert.Equal(source, new PortugueseConverter(new ConversionOptions(Vocabulary: false)).Convert(source));
        Converts("<i>Já é o segunda de estupro.</i>", "<i>Já é a segunda violação.</i>");
    }

    private static void Converts(string source, string expected)
    {
        Assert.Equal(expected, Converter.Convert(source));
        Assert.Equal(expected, Converter.Convert(expected));
    }
}
