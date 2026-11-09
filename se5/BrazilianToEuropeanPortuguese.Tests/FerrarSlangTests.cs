using SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;
using SubtitleEdit.Plugins.Shared;

namespace BrazilianToEuropeanPortuguese.Tests;

public sealed class FerrarSlangTests
{
    [Theory]
    [InlineData("Ferrei tudo,", "Lixei tudo,")]
    [InlineData("Ferraste tudo.", "Lixaste tudo.")]
    [InlineData("Ela ferrou o negócio.", "Ela lixou o negócio.")]
    [InlineData("Ferramos tudo.", "Lixamos tudo.")]
    [InlineData("Ferrámos tudo.", "Lixámos tudo.")]
    [InlineData("Ferraram a minha vida.", "Lixaram a minha vida.")]
    [InlineData("Estou ferrado.", "Estou lixado.")]
    [InlineData("Estou completamente ferrado.", "Estou completamente lixado.")]
    [InlineData("Ferrei com tudo.", "Lixei tudo.")]
    [InlineData("Estamos ferradas.", "Estamos lixadas.")]
    [InlineData("Eu me ferrei.", "Eu lixei-me.")]
    [InlineData("Não se ferre.", "Não te lixes.")]
    [InlineData("Ferrar tudo.", "Lixar tudo.")]
    [InlineData("FERREI TUDO!", "LIXEI TUDO!")]
    [InlineData("<i>Ferrei tudo.</i>", "<i>Lixei tudo.</i>")]
    [InlineData("-Ferrei tudo.", "-Lixei tudo.")]
    [InlineData("Ele ferrou você.", "Ele lixou-te.")]
    [InlineData("Não me ferres.", "Não me lixes.")]
    [InlineData("Vou ferrar-me.", "Vou lixar-me.")]
    [InlineData("Ferraria tudo.", "Lixaria tudo.")]
    [InlineData("Se ferrasse tudo.", "Se lixasse tudo.")]
    [InlineData("Ela está ferrando tudo.", "Ela está a lixar tudo.")]
    [InlineData("Vou ferrá-lo.", "Vou ferrá-lo.")]
    public void QualifiedSlangUsesLixarAcrossInflections(string source, string expected)
    {
        var converter = new PortugueseConverter();
        Assert.Equal(expected, converter.Convert(source));
        Assert.Equal(expected, converter.Convert(expected));
    }

    [Theory]
    [InlineData("Eu ferrei a viagem.", "I screwed up the trip.", "Eu lixei a viagem.")]
    [InlineData("Ele ferrou o cliente.", "He screwed the client over.", "Ele lixou o cliente.")]
    [InlineData("Ela ferrou.", "She screwed up.", "Ela lixou.")]
    public void EnglishSlangCanResolveOtherObjects(string source, string english, string expected) =>
        Assert.Equal(expected, new PortugueseConverter().Convert(source, reference: ReferenceHint.FromText(english)));

    [Theory]
    [InlineData("Ferrei o cavalo.")]
    [InlineData("Ele ferrou o gado.")]
    [InlineData("O gato ferrou as unhas.")]
    [InlineData("Ela ferrou no sono.")]
    [InlineData("Ferrou-lhe uma reprimenda.")]
    [InlineData("Ela ferrou.")]
    [InlineData("Ele ferrou-se a trabalhar.")]
    [InlineData("Ela ferrou-se a uma ideia.")]
    [InlineData("Ele ferrou-a pelo braço.")]
    [InlineData("Ferraram-se a dormir.")]
    public void LiteralAndUnresolvedUsesArePreserved(string source) =>
        Assert.Equal(source, new PortugueseConverter().Convert(source));

    [Theory]
    [InlineData("Ferrei tudo.", "Lixei tudo.")]
    [InlineData("Não se ferre.", "Não se lixe.")]
    public void FormalAddressDoesNotDisableVocabulary(string source, string expected) =>
        Assert.Equal(expected, Assert.Single(SubtitleConversion.Convert(
            new[] { new SrtBlock { StartMs = 1000, EndMs = 2000, Text = source } },
            new ConversionOptions(Address: AddressMode.Formal))).Text);

    [Fact]
    public void OptionsStageAndLocalOverrideAreRespected()
    {
        Assert.Equal("Ferrei tudo.", new PortugueseConverter(new ConversionOptions(Vocabulary: false)).Convert("Ferrei tudo."));
        Assert.Equal("Ferrei tudo.", new PortugueseConverter(new ConversionOptions(DisabledRules: "construction-vocabulary")).Convert("Ferrei tudo."));
        var words = new PortugueseLocalWordList();
        words.Ignored.Add("ferrar");
        Assert.Equal("Ferrei tudo.", new PortugueseConverter(localWordList: words).Convert("Ferrei tudo."));
    }
}
