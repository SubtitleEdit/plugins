using SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;
using SubtitleEdit.Plugins.Shared;

namespace BrazilianToEuropeanPortuguese.Tests;

public sealed class DativeRecipientTests
{
    [Theory]
    [InlineData("Não contou a ela?", "Não lhe contou?")]
    [InlineData("Não contou a ele?", "Não lhe contou?")]
    [InlineData("Não contou a elas?", "Não lhes contou?")]
    [InlineData("Não contou a mim?", "Não me contou?")]
    [InlineData("Não contou a ti?", "Não te contou?")]
    [InlineData("Não contou a nós?", "Não nos contou?")]
    [InlineData("Nunca contei a eles.", "Nunca lhes contei.")]
    [InlineData("Contei a ela.", "Contei-lhe.")]
    [InlineData("Ele explicou a elas.", "Ele explicou-lhes.")]
    [InlineData("Eu perguntei a ele.", "Eu perguntei-lhe.")]
    [InlineData("Ela respondeu a mim.", "Ela respondeu-me.")]
    [InlineData("Ele escreveu a ti.", "Ele escreveu-te.")]
    [InlineData("Ela telefonou a nós.", "Ela telefonou-nos.")]
    [InlineData("Eu paguei a ela.", "Eu paguei-lhe.")]
    [InlineData("Ela mentiu a ele.", "Ela mentiu-lhe.")]
    [InlineData("Ele enviou a elas.", "Ele enviou-lhes.")]
    [InlineData("Disse a ela a verdade.", "Disse-lhe a verdade.")]
    [InlineData("Dei a ela tudo.", "Dei-lhe tudo.")]
    [InlineData("Não entregou a ela?", "Não lhe entregou?")]
    [InlineData("Ela emprestou a ele.", "Ela emprestou-lhe.")]
    [InlineData("Contava a ela.", "Contava-lhe.")]
    [InlineData("Contámos a ela.", "Contámos-lhe.")]
    [InlineData("Se contássemos a ela.", "Se lhe contássemos.")]
    [InlineData("Se déssemos a ela.", "Se lhe déssemos.")]
    [InlineData("Se dermos a ela.", "Se lhe dermos.")]
    [InlineData("Não daríamos a elas.", "Não lhes daríamos.")]
    [InlineData("Se respondermos a eles.", "Se lhes respondermos.")]
    [InlineData("Se contar a ela.", "Se lhe contar.")]
    [InlineData("Se contasse a ela.", "Se lhe contasse.")]
    [InlineData("Contarei a ela.", "Contar-lhe-ei.")]
    [InlineData("Contaria a elas.", "Contar-lhes-ia.")]
    [InlineData("Não contaria a ela.", "Não lhe contaria.")]
    [InlineData("Vou contar a ela.", "Vou contar-lhe.")]
    [InlineData("Pedi a ela para sair.", "Pedi-lhe para sair.")]
    [InlineData("Sei que a Maria contou a ela.", "Sei que a Maria lhe contou.")]
    [InlineData("Não contou isso a ela?", "Não lhe contou isso?")]
    [InlineData("Contou tudo a eles.", "Contou-lhes tudo.")]
    [InlineData("Ele deu o livro a ela.", "Ele deu-lhe o livro.")]
    [InlineData("Não mostrou a casa a ele.", "Não lhe mostrou a casa.")]
    [InlineData("Contei a verdade a ela.", "Contei-lhe a verdade.")]
    [InlineData("Não contou\na ela?", "Não\nlhe contou?")]
    [InlineData("Não contou para ela?", "Não lhe contou?")]
    [InlineData("Não contou isso para ela?", "Não lhe contou isso?")]
    [InlineData("Contou a ela e saiu.", "Contou-lhe e saiu.")]
    [InlineData("Ele lhe contou a ela.", "Ele contou-lhe a ela.")]
    [InlineData("NÃO CONTOU A ELA?", "NÃO LHE CONTOU?")]
    [InlineData("<i>Não contou a ela?</i>", "<i>Não lhe contou?</i>")]
    public void NeutralRecipientsUseDativeClitics(string source, string expected)
    {
        var converter = new PortugueseConverter();
        Assert.Equal(expected, converter.Convert(source));
        Assert.Equal(expected, converter.Convert(expected));
    }

    [Theory]
    [InlineData("Contou a ela mesma.")]
    [InlineData("Contou para ela mesma.")]
    [InlineData("Contou a ela própria.")]
    [InlineData("Contei a ELA.")]
    [InlineData("Não contou a ela nem a ele.")]
    [InlineData("Contou a ela, mas não a ele.")]
    [InlineData("Contou a ela e à mãe.")]
    [InlineData("Contou a ela e a mim.")]
    [InlineData("Contou a ela ou a ele.")]
    [InlineData("Contou a ela e ele.")]
    [InlineData("Contou a ela, não a ele.")]
    [InlineData("Contou isso a ela e a ele.")]
    [InlineData("Contou a verdade a ela mesma.")]
    [InlineData("Ele contou-lhe a ela.")]
    [InlineData("Contou apenas a ela.")]
    [InlineData("Isso não conta para ela.")]
    [InlineData("Isso dá para ela.")]
    [InlineData("A conta a ela pertence.")]
    [InlineData("Ela voltou a ele.")]
    [InlineData("Ela pensou a respeito.")]
    [InlineData("Vou comprar um presente a ela.")]
    [InlineData("Telefonei para ela saber.")]
    public void EmphasisCoordinationAndNonDativeUsesArePreserved(string source) =>
        Assert.Equal(source, new PortugueseConverter().Convert(source));

    [Fact]
    public void PronounOptionsAndDisabledStageAreRespected()
    {
        foreach (var options in new[]
        {
            new ConversionOptions(Pronouns: false),
            new ConversionOptions(DisabledRules: "pronouns"),
        })
            Assert.Equal("Não contou a ela?", new PortugueseConverter(options).Convert("Não contou a ela?"));
    }

    [Fact]
    public void ReportedCueKeepsItsTenseAndTimingInFormalAndDefaultConversion()
    {
        foreach (var options in new[] { ConversionOptions.Default, new ConversionOptions(Address: AddressMode.Formal) })
        {
            var cue = new SrtBlock { StartMs = 1934133, EndMs = 1936092, Text = "Não contou a ela?" };
            var converted = Assert.Single(SubtitleConversion.Convert(new[] { cue }, options,
                english: new string?[] { "What, you didn't tell her?" }));
            Assert.Equal("Não lhe contou?", converted.Text);
            Assert.Equal(1934133, cue.StartMs);
            Assert.Equal(1936092, cue.EndMs);
        }
    }
}
