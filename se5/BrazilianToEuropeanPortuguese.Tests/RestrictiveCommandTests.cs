using SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;

namespace BrazilianToEuropeanPortuguese.Tests;

public sealed class RestrictiveCommandTests
{
    [Theory]
    [InlineData("Só lembre-se quem\nvocê enfrenta.", "Lembra-te só quem\nenfrentas.")]
    [InlineData("Só lembra-te quem enfrentas.", "Lembra-te só quem enfrentas.")]
    [InlineData("Só lembra-te de mim.", "Lembra-te só de mim.")]
    [InlineData("só lembra-te de mim.", "lembra-te só de mim.")]
    [InlineData("SÓ LEMBRA-TE DE MIM.", "LEMBRA-TE SÓ DE MIM.")]
    [InlineData("Só prepara-te para sair.", "Prepara-te só para sair.")]
    [InlineData("Só concentra-te no trabalho.", "Concentra-te só no trabalho.")]
    [InlineData("Só senta-te aqui.", "Senta-te só aqui.")]
    [InlineData("Só veste-te depressa.", "Veste-te só depressa.")]
    [InlineData("Só lava-te primeiro.", "Lava-te só primeiro.")]
    [InlineData("Apenas lembra-te de mim.", "Lembra-te apenas de mim.")]
    [InlineData("Somente lembra-te de mim.", "Lembra-te somente de mim.")]
    [InlineData("-Só cala-te um minuto.", "-Cala-te só um minuto.")]
    [InlineData("Joe, só lembra-te de mim.", "Joe, lembra-te só de mim.")]
    [InlineData("Só\nlembra-te de mim.", "Lembra-te\nsó de mim.")]
    public void RestrictiveAdverbsFollowKnownReflexiveCommands(string source, string expected)
    {
        var converter = new PortugueseConverter();
        Assert.Equal(expected, converter.Convert(source));
        Assert.Equal(expected, converter.Convert(expected));
    }

    [Theory]
    [InlineData("Ele só lembrou-se de mim.", "Ele só se lembrou de mim.")]
    [InlineData("Só lembras-te de mim.", "Só te lembras de mim.")]
    [InlineData("Só lembrava-te disso.", "Só te lembrava disso.")]
    [InlineData("Apenas lembraste-te disso.", "Apenas te lembraste disso.")]
    [InlineData("Somente lembraste-te disso.", "Somente te lembraste disso.")]
    [InlineData("Não te lembres disso.", "Não te lembres disso.")]
    [InlineData("Disse que só lembra-te de mim.", "Disse que só lembra-te de mim.")]
    [InlineData("Se só lembra-te de mim.", "Se só lembra-te de mim.")]
    [InlineData("Só, lembra-te de mim.", "Só, lembra-te de mim.")]
    [InlineData("Lembra-te só de mim.", "Lembra-te só de mim.")]
    [InlineData("Eu só lembra-te de mim.", "Eu só te lembra de mim.")]
    public void IndicativeNegationSubordinationAndScopeArePreserved(string source, string expected) =>
        Assert.Equal(expected, AttractedEnclisis.Apply(source));

    [Fact]
    public void PronounSettingStillControlsTheCorrection()
    {
        var converter = new PortugueseConverter(new ConversionOptions(Pronouns: false, YouToTu: false));
        Assert.Equal("Só lembra-te de mim.", converter.Convert("Só lembra-te de mim."));
    }
}
