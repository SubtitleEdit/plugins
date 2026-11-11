using SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;
using SubtitleEdit.Plugins.Shared;

namespace BrazilianToEuropeanPortuguese.Tests;

public sealed class ReflexiveCoverageTests
{
    public static IEnumerable<object[]> RegularPronominalExamples()
    {
        foreach (var (first, plural) in new[]
        {
            ("lavo", "lavamos"), ("penteio", "penteamos"), ("barbeio", "barbeamos"),
            ("deito", "deitamos"), ("levanto", "levantamos"), ("sento", "sentamos"),
            ("afasto", "afastamos"), ("aproximo", "aproximamos"), ("preparo", "preparamos"),
            ("apresento", "apresentamos"), ("lembro", "lembramos"), ("esqueço", "esquecemos"),
            ("queixo", "queixamos"), ("arrependo", "arrependemos"), ("esforço", "esforçamos"),
            ("atrevo", "atrevemos"), ("resigno", "resignamos"), ("reconcilio", "reconciliamos"),
            ("preocupo", "preocupamos"), ("interesso", "interessamos"), ("importo", "importamos"),
            ("comporto", "comportamos"), ("concentro", "concentramos"), ("habituo", "habituamos"),
            ("adapto", "adaptamos"), ("recuso", "recusamos"), ("dedico", "dedicamos"),
            ("engano", "enganamos"), ("magoo", "magoamos"), ("protejo", "protegemos"),
            ("defendo", "defendemos"), ("recolho", "recolhemos"), ("escondo", "escondemos"),
        })
        {
            yield return new object[] { "Eu me " + first + ".", "Eu " + first + "-me." };
            yield return new object[] { "Nos " + plural + ".", char.ToUpperInvariant(plural[0]) + plural[1..^1] + "-nos." };
            yield return new object[] { "Nunca nos " + plural + ".", "Nunca nos " + plural + "." };
        }
    }

    [Theory]
    [MemberData(nameof(RegularPronominalExamples))]
    public void RegularPronominalFamiliesUseSharedMechanics(string source, string expected)
    {
        var converter = new PortugueseConverter();
        Assert.Equal(expected, converter.Convert(source));
        Assert.Equal(expected, converter.Convert(expected));
    }

    [Theory]
    [InlineData("\"Nos conhecemos\nhá muito tempo\"?", "\"Conhecemo-nos\nhá muito tempo\"?")]
    [InlineData("Nos conhecemos há imenso tempo.", "Conhecemo-nos há imenso tempo.")]
    [InlineData("«Nos encontramos amanhã.»", "«Encontramo-nos amanhã.»")]
    [InlineData("'Nos abraçamos.'", "'Abraçamo-nos.'")]
    [InlineData("<i>\"Nos conhecemos.\"</i>", "<i>\"Conhecemo-nos.\"</i>")]
    [InlineData("- Nos conhecemos.", "- Conhecemo-nos.")]
    [InlineData("NOS CONHECEMOS.", "CONHECEMO-NOS.")]
    [InlineData("nos conhecemos.", "conhecemo-nos.")]
    [InlineData("\"Nós nos conhecemos.\"", "\"Nós conhecemo-nos.\"")]
    [InlineData("Nos arrependemos.", "Arrependemo-nos.")]
    [InlineData("Nos esforçamos.", "Esforçamo-nos.")]
    [InlineData("Nos queixamos.", "Queixamo-nos.")]
    [InlineData("Nos atrevemos.", "Atrevemo-nos.")]
    [InlineData("Nos abstemos.", "Abstemo-nos.")]
    [InlineData("Nos resignamos.", "Resignamo-nos.")]
    [InlineData("Nos entreolhamos.", "Entreolhamo-nos.")]
    [InlineData("Nos reconciliamos.", "Reconciliamo-nos.")]
    [InlineData("Nos aproximámos.", "Aproximámo-nos.")]
    [InlineData("Nos levantávamos.", "Levantávamo-nos.")]
    [InlineData("Nos conheceremos.", "Conhecer-nos-emos.")]
    [InlineData("Nos arrependeríamos.", "Arrepender-nos-íamos.")]
    [InlineData("Nos resignaremos.", "Resignar-nos-emos.")]
    [InlineData("\"Me arrependo.\"", "\"Arrependo-me.\"")]
    [InlineData("Me resigno.", "Resigno-me.")]
    [InlineData("me arrependo.", "arrependo-me.")]
    [InlineData("Me queixei.", "Queixei-me.")]
    [InlineData("Me esforcei.", "Esforcei-me.")]
    [InlineData("Eu me resigno.", "Eu resigno-me.")]
    [InlineData("Ela se resignou.", "Ela resignou-se.")]
    [InlineData("A equipa se reconciliou.", "A equipa reconciliou-se.")]
    [InlineData("Os irmãos se entreolharam.", "Os irmãos entreolharam-se.")]
    [InlineData("Eu me resignarei.", "Eu resignar-me-ei.")]
    [InlineData("Me tenho lavado.", "Tenho-me lavado.")]
    [InlineData("Eu quero vestir-se.", "Eu quero vestir-me.")]
    [InlineData("Nós podemos esquecer-se.", "Nós podemos esquecer-nos.")]
    [InlineData("Nós estamos a preparar-se.", "Nós estamos a preparar-nos.")]
    [InlineData("Eu preciso de concentrar-se.", "Eu preciso de concentrar-me.")]
    [InlineData("Vós podeis defender-se.", "Vós podeis defender-vos.")]
    [InlineData("Conhecemos-nos.", "Conhecemo-nos.")]
    [InlineData("Encontrámos-nos.", "Encontrámo-nos.")]
    [InlineData("Ele não lava-se.", "Ele não se lava.")]
    [InlineData("Nós nunca conhecemo-nos.", "Nós nunca nos conhecemos.")]
    [InlineData("Ela disse que arrependeu-se.", "Ela disse que se arrependeu.")]
    [InlineData("Resignarei-me.", "Resignar-me-ei.")]
    [InlineData("Eu não resignar-me-ei.", "Eu não me resignarei.")]
    [InlineData("Ela não arrepender-se-ia.", "Ela não se arrependeria.")]
    [InlineData("Vendem-se casas.", "Vendem-se casas.")]
    [InlineData("Aqui trabalha-se.", "Aqui trabalha-se.")]
    [InlineData("Me dar um sermão.", "Dar-me um sermão.")]
    [InlineData("Certo. Te digo.", "Certo. Digo-te.")]
    [InlineData("Te dou vinte minutos.", "Dou-te vinte minutos.")]
    [InlineData("Me deram um bolo.", "Deram-me um bolo.")]
    [InlineData("-Só cala-te um minuto.", "-Cala-te só um minuto.")]
    [InlineData("Sem escuta,\napenas sente-se com ele.", "Sem escuta,\napenas sente-se com ele.")]
    public void SharedPlacementCoversReflexiveReciprocalAndInherentPronouns(string source, string expected)
    {
        var converter = new PortugueseConverter();
        Assert.Equal(expected, converter.Convert(source));
        Assert.Equal(expected, converter.Convert(expected));
    }

    [Theory]
    [InlineData("Não nos conhecemos.")]
    [InlineData("Já nos conhecemos.")]
    [InlineData("Também nos conhecemos.")]
    [InlineData("Nunca nos resignamos.")]
    [InlineData("Disse que nos conhecemos.")]
    [InlineData("Disse que nós nos conhecemos.")]
    [InlineData("Quando nos encontramos.")]
    [InlineData("Se nos conhecermos.")]
    [InlineData("Para nos conhecermos.")]
    [InlineData("Nos casos graves.")]
    [InlineData("Nos ramos das árvores.")]
    [InlineData("Nos termos do contrato.")]
    [InlineData("\"Nos próximos dias.\"")]
    [InlineData("Nos mesmos sítios.")]
    [InlineData("Se me conhecesse, saberia.")]
    [InlineData("Perguntou se nos conhecemos.")]
    [InlineData("Se resignou, ficou sem emprego.")]
    [InlineData("Se lavar a roupa, avisa.")]
    [InlineData("Ele lava o carro.")]
    [InlineData("Ela lembra o pai.")]
    [InlineData("Eu deixo lavar-se.")]
    [InlineData("Eu quero lavar-me.")]
    [InlineData("Para conhecermo-nos.")]
    [InlineData("Eles me queriam também.")]
    [InlineData("Por favor, comunicar à central se virem o suspeito.")]
    [InlineData("A não ser que Malcom James\nse declare culpado")]
    [InlineData("Aposto que se perguntasse,\nela se disporia")]
    public void AttractorsPrepositionsConditionsAndNonReflexiveUsesRemain(string source) =>
        Assert.Equal(source, new PortugueseConverter(new ConversionOptions(Colloquial: false)).Convert(source));

    [Theory]
    [InlineData("Se arrependeu.", "He regretted it.", "Arrependeu-se.")]
    [InlineData("\"Se resignou.\"", "She resigned herself to it.", "\"Resignou-se.\"")]
    [InlineData("Se esforçou?", "Did he make an effort?", "Esforçou-se?")]
    [InlineData("Se resignou, ficou sem emprego.", "If he quit, he lost his job.", "Se resignou, ficou sem emprego.")]
    [InlineData("Se resignou.", "I wonder whether he resigned.", "Se resignou.")]
    [InlineData("Se está numa delegacia\nprecisa de advogado.", "You're in a police station, you sure as hell need a lawyer.",
        "Se está numa esquadra\nprecisas de advogado.")]
    public void ReferenceDistinguishesLeadingSeFromConditionalSe(string source, string english, string expected) =>
        Assert.Equal(expected, new PortugueseConverter().Convert(source, reference: ReferenceHint.FromText(english)));

    [Theory]
    [InlineData(false, "")]
    [InlineData(true, "pronouns")]
    public void SharedFixesRespectPronounSettings(bool pronouns, string disabled)
    {
        const string source = "\"Nos conhecemos.\"";
        Assert.Equal(source, new PortugueseConverter(new ConversionOptions(Pronouns: pronouns, DisabledRules: disabled))
            .Convert(source));
    }

    [Fact]
    public void EpisodeQuoteKeepsOriginalAndTimings()
    {
        const string source = "\"Nos conhecemos\nhá muito tempo\"?";
        var blocks = new[] { new SrtBlock { StartMs = 1571854, EndMs = 1574677, Text = source } };
        var result = Assert.Single(SubtitleConversion.Convert(blocks, new ConversionOptions(),
            english: ["\"We've known each other a long time\"?"]));
        Assert.Equal("\"Conhecemo-nos\nhá muito tempo\"?", result.Text);
        Assert.Equal(source, blocks[0].Text);
        Assert.Equal(1571854, blocks[0].StartMs);
        Assert.Equal(1574677, blocks[0].EndMs);
    }
}
