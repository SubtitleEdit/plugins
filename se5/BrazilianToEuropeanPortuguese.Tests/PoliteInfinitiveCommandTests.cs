using SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;
using SubtitleEdit.Plugins.Shared;

namespace BrazilianToEuropeanPortuguese.Tests;

public sealed class PoliteInfinitiveCommandTests
{
    [Theory]
    [InlineData("<i>54 anos. Por favor,\navisar se avistarem o suspeito.</i>",
        "<i>54 anos. Por favor,\navisem se avistarem o suspeito.</i>")]
    [InlineData("Por favor, avisar se virem o suspeito.", "Por favor, avisem se virem o suspeito.")]
    [InlineData("Por favor, informar se souberem.", "Por favor, informem se souberem.")]
    [InlineData("Por favor, comunicar à central se virem o suspeito.", "Por favor, comuniquem à central se virem o suspeito.")]
    [InlineData("Por favor, informar a central se virem o suspeito.", "Por favor, informem a central se virem o suspeito.")]
    [InlineData("Por favor, ligar quando chegarem.", "Por favor, liguem quando chegarem.")]
    [InlineData("Por favor, telefonar se precisarem.", "Por favor, telefonem se precisarem.")]
    [InlineData("Por favor, chamar se quiserem.", "Por favor, chamem se quiserem.")]
    [InlineData("Por favor, verificar se encontrarem algo.", "Por favor, verifiquem se encontrarem algo.")]
    [InlineData("Por favor, fazer se puderem.", "Por favor, façam se puderem.")]
    [InlineData("Por favor, dizer se tiverem dúvidas.", "Por favor, digam se tiverem dúvidas.")]
    [InlineData("Por favor, sair quando forem chamados.", "Por favor, saiam quando forem chamados.")]
    [InlineData("Por favor, seguir quando chegarem.", "Por favor, sigam quando chegarem.")]
    [InlineData("Por favor, pedir se precisarem.", "Por favor, peçam se precisarem.")]
    [InlineData("Por favor, manter se quiserem.", "Por favor, mantenham se quiserem.")]
    [InlineData("Por favor, comparecer quando puderem.", "Por favor, compareçam quando puderem.")]
    [InlineData("Por favor, trazer se tiverem.", "Por favor, tragam se tiverem.")]
    [InlineData("Por favor, não entrar se virem o suspeito.", "Por favor, não entrem se virem o suspeito.")]
    [InlineData("Por favor, avisar-me se virem o suspeito.", "Por favor, avisem-me se virem o suspeito.")]
    [InlineData("Por favor, não avisar-me se virem o suspeito.", "Por favor, não me avisem se virem o suspeito.")]
    [InlineData("Por favor avisar se vocês virem o suspeito.", "Por favor avisem se vocês virem o suspeito.")]
    [InlineData("Por favor, avisar se não virem o suspeito.", "Por favor, avisem se não virem o suspeito.")]
    [InlineData("Por favor, avisar assim que chegarem.", "Por favor, avisem assim que chegarem.")]
    [InlineData("POR FAVOR, AVISAR SE AVISTAREM O SUSPEITO.", "POR FAVOR, AVISEM SE AVISTAREM O SUSPEITO.")]
    public void SharedPluralConditionalIdentifiesRequestAddressees(string source, string expected)
    {
        var converter = new PortugueseConverter();
        Assert.Equal(expected, converter.Convert(source));
        Assert.Equal(expected, converter.Convert(expected));
    }

    [Theory]
    [InlineData("Por favor, avisar se ele chegar.")]
    [InlineData("Por favor, avisar se eles virem o suspeito.")]
    [InlineData("Por favor, avisar se os agentes virem o suspeito.")]
    [InlineData("Por favor, avisar se eles se virem.")]
    [InlineData("Por favor, avisar se houver problemas.")]
    [InlineData("Por favor, avisar se houverem problemas.")]
    [InlineData("Por favor, avisar se acontecerem problemas.")]
    [InlineData("Por favor, avisar se existirem dúvidas.")]
    [InlineData("Por favor, avisar se avistar o suspeito.")]
    [InlineData("Por favor, avisar se vires o suspeito.")]
    [InlineData("Por favor, avisar.")]
    [InlineData("É favor avisar se avistarem o suspeito.")]
    [InlineData("Nunca fazer funcionar a bomba a seco.")]
    [InlineData("Avisar se avistarem o suspeito.")]
    [InlineData("Por favor, avisar que eles chegaram.")]
    [InlineData("Por favor, informar os agentes se eles chegarem.")]
    public void ImpersonalAndDifferentSubjectInstructionsAreNotGuessed(string source) =>
        Assert.Equal(source, new PortugueseConverter().Convert(source));

    [Fact]
    public void OptionsAndLocalExceptionsWin()
    {
        const string source = "Por favor, avisar se avistarem o suspeito.";
        foreach (var options in new[]
        {
            new ConversionOptions(Colloquial: false),
            new ConversionOptions(Pronouns: false),
            new ConversionOptions(DisabledRules: "polite-infinitive-commands"),
        })
            Assert.Equal(source, new PortugueseConverter(options).Convert(source));
        var words = new PortugueseLocalWordList();
        words.Ignored.Add("avisar");
        Assert.Equal(source, new PortugueseConverter(localWordList: words).Convert(source));
        Assert.Equal("Por favor, avisem se avistarem o suspeito.",
            new PortugueseConverter(new ConversionOptions(YouToTu: false)).Convert(source));
    }

    [Fact]
    public void ExactDispatchCueRetainsTimingAndLineBreaks()
    {
        var cue = new SrtBlock { StartMs = 2096332, EndMs = 2099952,
            Text = "<i>54 anos. Por favor,\navisar se avistarem o suspeito.</i>" };
        var result = Assert.Single(SubtitleConversion.Convert(new[] { cue }, ConversionOptions.Default,
            english: new string?[] { "54 years of age. Please advise if contact is made with subject." }));
        Assert.Equal("<i>54 anos. Por favor,\navisem se avistarem o suspeito.</i>", result.Text);
        Assert.Equal(2096332, cue.StartMs);
        Assert.Equal(2099952, cue.EndMs);
    }
}
