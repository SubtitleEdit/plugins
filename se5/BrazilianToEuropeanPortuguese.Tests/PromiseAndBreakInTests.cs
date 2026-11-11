using SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;
using SubtitleEdit.Plugins.Shared;

namespace BrazilianToEuropeanPortuguese.Tests;

public sealed class PromiseAndBreakInTests
{
    [Theory]
    [InlineData("Elas vão lacear.", "They'll break in.", "Elas vão ceder.")]
    [InlineData("Os sapatos lacearam.", null, "Os sapatos cederam.")]
    [InlineData("As botas laceiam com o uso.", null, "As botas cedem com o uso.")]
    [InlineData("O sapato laceará.", null, "O sapato cederá.")]
    [InlineData("Se laceasse, servia.", null, "Se cedesse, servia.")]
    [InlineData("Ela está laceando os sapatos.", null, "Ela está a alargar os sapatos.")]
    [InlineData("Vou lacear os sapatos.", null, "Vou alargar os sapatos.")]
    [InlineData("Vou lacear sapatos.", null, "Vou alargar sapatos.")]
    [InlineData("Vou lacear meus sapatos.", null, "Vou alargar os meus sapatos.")]
    [InlineData("Vou lacear um pouco as botas.", null, "Vou alargar um pouco as botas.")]
    [InlineData("Laceei as luvas.", null, "Alarguei as luvas.")]
    [InlineData("Ela laceou o couro.", null, "Ela amoleceu o couro.")]
    [InlineData("O couro vai lacear.", null, "O couro vai amolecer.")]
    [InlineData("Os sapatos estão laceados.", null, "Os sapatos estão alargados.")]
    [InlineData("Vou laceá-los.", null, "Vou alargá-los.")]
    [InlineData("ELAS VÃO LACEAR.", null, "ELAS VÃO CEDER.")]
    [InlineData("<i>Elas vão lacear.</i>", null, "<i>Elas vão ceder.</i>")]
    public void PhysicalBreakInUsesTheAppropriateEuropeanVerb(string source, string? english, string expected)
    {
        var converter = new PortugueseConverter();
        var hint = ReferenceHint.FromText(english);
        Assert.Equal(expected, converter.Convert(source, reference: hint));
        Assert.Equal(expected, converter.Convert(expected, reference: hint));
    }

    [Theory]
    [InlineData("Ela laceou o vestido com fitas.")]
    [InlineData("Vou lacear o vestido com laços.")]
    public void DecoratingWithBowsIsNotPhysicalStretching(string source) =>
        Assert.Equal(source, new PortugueseConverter().Convert(source));

    [Theory]
    [InlineData("Promete não contar\npara ninguém?", "Promise not to tell anybody?", "Prometes não contar\na ninguém?")]
    [InlineData("Promete não rir?", "Promise not to laugh?", "Prometes não te rir?")]
    [InlineData("Promete não contar?", "Promise not to tell?", "Prometes não contar?")]
    [InlineData("Promete guardar segredo?", "Do you promise to keep a secret?", "Prometes guardar segredo?")]
    [InlineData("Promete não rir?", null, "Prometes não te rir?")]
    [InlineData("Prometo não rir.", null, "Prometo não me rir.")]
    [InlineData("Prometemos não rir.", null, "Prometemos não nos rir.")]
    [InlineData("Ela promete não rir.", "She promises not to laugh.", "Ela promete não se rir.")]
    [InlineData("Eu prometia não rir.", null, "Eu prometia não me rir.")]
    [InlineData("Eu também prometia não rir.", null, "Eu também prometia não me rir.")]
    [InlineData("Eu prometeria não rir.", null, "Eu prometeria não me rir.")]
    [InlineData("Ele prometia não rir.", null, "Ele prometia não se rir.")]
    [InlineData("Prometes rir?", null, "Prometes rir-te?")]
    [InlineData("Prometes não te rir?", null, "Prometes não te rir?")]
    [InlineData("Prometes não rir-se?", null, "Prometes não te rir?")]
    [InlineData("Prometes não se rir?", null, "Prometes não te rir?")]
    [InlineData("Prometo nunca rir.", null, "Prometo nunca me rir.")]
    [InlineData("Prometeram não rir.", null, "Prometeram não se rir.")]
    [InlineData("Prometeste não rir.", null, "Prometeste não te rir.")]
    [InlineData("Nós prometíamos não rir.", null, "Nós prometíamos não nos rir.")]
    [InlineData("Não contaria para ninguém.", null, "Não contaria a ninguém.")]
    [InlineData("PROMETE NÃO RIR?", "Promise not to laugh?", "PROMETES NÃO TE RIR?")]
    [InlineData("<i>Promete não rir?</i>", "Promise not to laugh?", "<i>Prometes não te rir?</i>")]
    [InlineData("Não contei para ninguém.", null, "Não contei a ninguém.")]
    [InlineData("Não disse para ninguém.", null, "Não disse a ninguém.")]
    [InlineData("Não disseste para ninguém.", null, "Não disseste a ninguém.")]
    [InlineData("Não diríamos para ninguém.", null, "Não diríamos a ninguém.")]
    public void PromiseRequestsAndTheirComplementsUseConsistentAddress(string source, string? english, string expected)
    {
        var converter = new PortugueseConverter();
        var hint = ReferenceHint.FromText(english);
        Assert.Equal(expected, converter.Convert(source, reference: hint));
        Assert.Equal(expected, converter.Convert(expected, reference: hint));
    }

    [Theory]
    [InlineData("Ela promete não contar?", "Does she promise not to tell?")]
    [InlineData("Promete não contar?", "Does he promise not to tell?")]
    [InlineData("Promete não contar.", null)]
    [InlineData("Prometia não rir.", null)]
    [InlineData("Prometeria não rir.", null)]
    [InlineData("Ele fez isso para ninguém rir.", null)]
    [InlineData("Vou comprar para ninguém.", null)]
    public void OtherSubjectsAndUnrelatedPrepositionsArePreserved(string source, string? english) =>
        Assert.Equal(source, new PortugueseConverter().Convert(source, reference: ReferenceHint.FromText(english)));

    [Fact]
    public void EllipticalEnglishPromiseIdentifiesTheAddressee()
    {
        var hint = ReferenceHint.FromText("Promise not to laugh?")!;
        Assert.True(hint.SecondPerson);
        Assert.True(hint.SecondPersonSubject);
        Assert.True(hint.StartsWithSecondPersonSubject);
        Assert.False(ReferenceHint.FromText("I promise not to laugh.")!.StartsWithSecondPersonSubject);
    }

    [Fact]
    public void FormalAndDisabledOptionsKeepTheirIntendedBehavior()
    {
        var cue = new SrtBlock { StartMs = 1000, EndMs = 2000, Text = "Promete não rir?" };
        Assert.Equal("Promete não se rir?", Assert.Single(SubtitleConversion.Convert(new[] { cue },
            new ConversionOptions(Address: AddressMode.Formal), english: new string?[] { "Promise not to laugh?" })).Text);
        Assert.Equal("Promete não rir?", new PortugueseConverter(new ConversionOptions(YouToTu: false, Pronouns: false))
            .Convert(cue.Text));
        Assert.Equal("Elas vão lacear.", new PortugueseConverter(new ConversionOptions(Vocabulary: false))
            .Convert("Elas vão lacear."));
        Assert.Equal("Elas vão lacear.", new PortugueseConverter(new ConversionOptions(DisabledRules: "reviewed-verb-senses"))
            .Convert("Elas vão lacear."));
        Assert.Equal("Prometes não rir?", new PortugueseConverter(new ConversionOptions(Pronouns: false))
            .Convert("Promete não rir?", reference: ReferenceHint.FromText("Promise not to laugh?")));
        Assert.Equal("Promete não se rir?", new PortugueseConverter(new ConversionOptions(DisabledRules: "address"))
            .Convert("Promete não rir?", reference: ReferenceHint.FromText("Promise not to laugh?")));
        var words = new PortugueseLocalWordList();
        words.Ignored.Add("lacear");
        Assert.Equal("Elas vão lacear.", new PortugueseConverter(localWordList: words).Convert("Elas vão lacear."));
    }
}
