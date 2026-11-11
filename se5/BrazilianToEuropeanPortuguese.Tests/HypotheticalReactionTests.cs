using SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;

namespace BrazilianToEuropeanPortuguese.Tests;

public sealed class HypotheticalReactionTests
{
    [Theory]
    [InlineData("Eu também ficaria\nmuito brava se fosse ele.", "Eu também ficaria\nmuito zangada se fosse comigo.")]
    [InlineData("Eu ficaria zangado se eu fosse ela.", "Eu ficaria zangado se fosse comigo.")]
    [InlineData("Eu estaria triste se fosse ele.", "Eu estaria triste se fosse comigo.")]
    [InlineData("Eu não ficaria tão ofendida se fosse ela.", "Eu não ficaria tão ofendida se fosse comigo.")]
    [InlineData("Nós ficaríamos preocupados se fôssemos eles.", "Nós ficaríamos preocupados se fosse connosco.")]
    [InlineData("Tu ficarias furioso se fosses ele.", "Tu ficarias furioso se fosse contigo.")]
    [InlineData("Ela ficaria indignada se fosse ele.", "Ela ficaria indignada se fosse com ela.")]
    [InlineData("Eles ficariam abalados se fossem ela.", "Eles ficariam abalados se fosse com eles.")]
    [InlineData("Se eu fosse ele, eu ficaria zangada.", "Se fosse comigo, eu ficaria zangada.")]
    [InlineData("Se eu fosse ele, ficaria zangada.", "Se fosse comigo, ficaria zangada.")]
    [InlineData("Ficaria zangada se eu fosse ele.", "Ficaria zangada se fosse comigo.")]
    [InlineData("Se nós fôssemos eles, ficaríamos tristes.", "Se fosse connosco, ficaríamos tristes.")]
    [InlineData("<i>Eu ficaria zangada se fosse ele.</i>", "<i>Eu ficaria zangada se fosse comigo.</i>")]
    [InlineData("EU FICARIA ZANGADA SE FOSSE ELE.", "EU FICARIA ZANGADA SE FOSSE COMIGO.")]
    [InlineData("Tu estarias assustado se fosses ela.", "Tu estarias assustado se fosse contigo.")]
    [InlineData("Nós estaríamos revoltados se fôssemos ele.", "Nós estaríamos revoltados se fosse connosco.")]
    [InlineData("Elas estariam magoadas se fossem ele.", "Elas estariam magoadas se fosse com elas.")]
    public void ReactionsUseTheAffectedPerson(string source, string expected)
    {
        var converter = new PortugueseConverter();
        Assert.Equal(expected, converter.Convert(source));
        Assert.Equal(expected, converter.Convert(expected));
    }

    [Theory]
    [InlineData("Eu ficaria zangada se fosse ele.", "Eu ficaria zangada se fosse ele.")]
    [InlineData("Eu estaria triste se tivesse perdido.", "Eu estaria triste se tivesse perdido.")]
    [InlineData("Ela ficaria zangada se soubesse.", "Ela ficaria zangada se soubesse.")]
    [InlineData("Estaria contente se pudesse.", "Estaria contente se pudesse.")]
    [InlineData("Eu ficaria feliz se ganhasse.", "Eu ficaria feliz se ganhasse.")]
    [InlineData("Nós ficaríamos tristes se perdêssemos.", "Nós ficaríamos tristes se perdêssemos.")]
    public void ConditionalSeNeverBecomesReflexive(string source, string expected) =>
        Assert.Equal(expected, new PortugueseConverter(new ConversionOptions(Colloquial: false)).Convert(source));

    [Theory]
    [InlineData("Se eu fosse ele, procuraria um advogado.")]
    [InlineData("Eu ficaria famoso se fosse ele.")]
    [InlineData("Eu ficaria zangada se ela fosse ele.")]
    [InlineData("Eu ficaria zangada se fosse ele no palco.")]
    [InlineData("Eu ficaria zangada se fosse ele que mentiu.")]
    [InlineData("Eu ficaria zangada se não fosse ele.")]
    [InlineData("Eu ficaria zangada se fosse eu.")]
    [InlineData("Eu ficaria zangada como se fosse ele.")]
    [InlineData("Ficaria triste se fosse ele.")]
    public void IdentityAdviceAndAmbiguousReactionsAreNotReworded(string source) =>
        Assert.Equal(source, new PortugueseConverter().Convert(source));

    [Fact]
    public void ReferenceResolvesAnOmittedFirstPerson()
    {
        var converter = new PortugueseConverter();
        Assert.Equal("Ficaria triste se fosse comigo.", converter.Convert("Ficaria triste se fosse ele.",
            reference: ReferenceHint.FromText("I'd be sad if I were him.")));
        Assert.Equal("Ficaria triste se fosse ele.", converter.Convert("Ficaria triste se fosse ele.",
            reference: ReferenceHint.FromText("She'd be sad if she were him.")));
    }

    [Fact]
    public void OptionsIgnoredPhrasesAndTraceAreRespected()
    {
        const string source = "Eu ficaria zangada se fosse ele.";
        foreach (var options in new[]
        {
            new ConversionOptions(Colloquial: false), new ConversionOptions(Pronouns: false),
            new ConversionOptions(DisabledRules: "hypothetical-reactions"),
        })
            Assert.Equal(source, new PortugueseConverter(options).Convert(source));
        var words = new PortugueseLocalWordList();
        words.Ignored.Add("se fosse ele");
        Assert.Equal(source, new PortugueseConverter(localWordList: words).Convert(source));
        var trace = new List<RuleChange>();
        new PortugueseConverter().Convert(source, trace: trace);
        Assert.Contains(trace, r => r.Id == "hypothetical-reactions");
    }

    [Fact]
    public void RealReflexivesStillConvert()
    {
        Assert.Equal("O rapaz chama-se João.", new PortugueseConverter().Convert("O rapaz se chama João."));
        Assert.Equal("O rapaz sentou-se.", new PortugueseConverter().Convert("O rapaz se sentou."));
    }
}
