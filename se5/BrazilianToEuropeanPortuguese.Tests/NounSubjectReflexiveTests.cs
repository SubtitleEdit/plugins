using SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;

namespace BrazilianToEuropeanPortuguese.Tests;

public sealed class NounSubjectReflexiveTests
{
    [Theory]
    [InlineData("A revolta se espalhou para\noutro bloco de celas.", "A revolta espalhou-se para\noutro bloco de celas.")]
    [InlineData("A notícia se espalhou.", "A notícia espalhou-se.")]
    [InlineData("A família se reuniu.", "A família reuniu-se.")]
    [InlineData("O rapaz se levantou.", "O rapaz levantou-se.")]
    [InlineData("Maria se levantou.", "Maria levantou-se.")]
    [InlineData("Os alunos se levantaram.", "Os alunos levantaram-se.")]
    [InlineData("A REVOLTA SE ESPALHOU.", "A REVOLTA ESPALHOU-SE.")]
    [InlineData("<i>A revolta se espalhou.</i>", "<i>A revolta espalhou-se.</i>")]
    public void NounSubjectsAlsoSupportReflexiveSe(string source, string expected)
    {
        var converter = new PortugueseConverter();
        Assert.Equal(expected, converter.Convert(source));
        Assert.Equal(expected, converter.Convert(expected));
    }

    [Theory]
    [InlineData("A revolta não se espalhou.")]
    [InlineData("A revolta já se espalhou.")]
    [InlineData("Disse que a revolta se espalhou.")]
    [InlineData("Se a revolta se espalhou.")]
    [InlineData("A revolta se espalhar.")]
    [InlineData("A revolta se espalhará.")]
    [InlineData("A revolta se espalharia.")]
    [InlineData("Pergunta se conhece Maria.")]
    [InlineData("Não sei se precisa.")]
    public void AttractorsNonFiniteUsesAndConditionalSeArePreserved(string source) =>
        Assert.Equal(source, new PortugueseConverter().Convert(source));

    [Fact]
    public void OptionAndStageAreRespected()
    {
        const string source = "A revolta se espalhou.";
        Assert.Equal(source, new PortugueseConverter(new ConversionOptions(Pronouns: false)).Convert(source));
        Assert.Equal(source, new PortugueseConverter(new ConversionOptions(DisabledRules: "pronouns")).Convert(source));
    }

    [Fact]
    public void CoordinatedCommandsKeepInformalReflexives() =>
        Assert.Equal("Vem e junta-te a nós.", new PortugueseConverter().Convert("Vem e se junte a nós."));
}
