using SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;
using SubtitleEdit.Plugins.Shared;

namespace BrazilianToEuropeanPortuguese.Tests;

public sealed class EnglishPraiseIdiomTests
{
    [Theory]
    [InlineData("Sim, verdadeiro, sim.", "Yeah, he's the real deal, yeah.", "Sim, ele é mesmo bom, sim.")]
    [InlineData("Sim, verdadeira, sim.", "Yeah, she's the real deal, yeah.", "Sim, ela é mesmo boa, sim.")]
    [InlineData("Ele é o negócio real.", "He's the real deal.", "Ele é mesmo bom.")]
    [InlineData("Ela é a coisa real.", "She is the real deal.", "Ela é mesmo boa.")]
    [InlineData("Eles são verdadeiros.", "They are the real deal.", "Eles são mesmo bons.")]
    [InlineData("Elas são verdadeiras.", "They're the real deal.", "Elas são mesmo boas.")]
    [InlineData("Ela era a coisa real.", "She was the real deal.", "Ela era mesmo boa.")]
    [InlineData("Ela\né a coisa real.", "She is the real deal.", "Ela\né mesmo boa.")]
    [InlineData("Ela é\na coisa real.", "She is the real deal.", "Ela é\nmesmo boa.")]
    [InlineData("Ele é o artigo genuíno.", "He's the genuine article.", "Ele é autêntico.")]
    [InlineData("Ela é o negócio real.", "She's the real McCoy.", "Ela é autêntica.")]
    [InlineData("Ele é verdadeiro.", "He's the real deal, not an impostor.", "Ele é autêntico.")]
    [InlineData("SIM, VERDADEIRO, SIM.", "Yeah, he's the real deal, yeah.", "SIM, ELE É MESMO BOM, SIM.")]
    [InlineData("<i>Sim, verdadeiro, sim.</i>", "Yeah, he's the real deal, yeah.",
        "<i>Sim, ele é mesmo bom, sim.</i>")]
    [InlineData("Sim. Esses caras conhecem\nmesmo suas coisas.", "Yeah. This guys really knows his stuff.",
        "Sim. Estes tipos sabem\nmesmo o que fazem.")]
    [InlineData("Ela conhece as suas coisas.", "She knows her stuff.", "Ela sabe o que faz.")]
    [InlineData("Ele sabe suas coisas.", "He knows his stuff.", "Ele sabe o que faz.")]
    [InlineData("Nós conhecemos nossas coisas.", "We know our stuff.", "Nós sabemos o que fazemos.")]
    [InlineData("Eu conheço minhas coisas.", "I know my stuff.", "Eu sei o que faço.")]
    [InlineData("Tu conheces tuas coisas.", "You know your stuff.", "Tu sabes o que fazes.")]
    [InlineData("Eles conheciam suas coisas.", "They knew their stuff.", "Eles sabiam o que faziam.")]
    [InlineData("ELAS CONHECEM SUAS COISAS.", "They know their stuff.", "ELAS SABEM O QUE FAZEM.")]
    [InlineData("<i>Ela conhece suas coisas.</i>", "She knows her stuff.", "<i>Ela sabe o que faz.</i>")]
    public void ConfirmedIdiomsUseNaturalPortuguese(string source, string english, string expected)
    {
        var converter = new PortugueseConverter();
        var reference = ReferenceHint.FromText(english);
        Assert.Equal(expected, converter.Convert(source, reference: reference));
        Assert.Equal(expected, converter.Convert(expected, reference: reference));
    }

    [Theory]
    [InlineData("Sim, verdadeiro, sim.", null)]
    [InlineData("Ele é verdadeiro.", "He is honest.")]
    [InlineData("Ele é um verdadeiro amigo.", "He's the real deal.")]
    [InlineData("O negócio real foi bom.", "He's the real deal.")]
    [InlineData("Ela é verdadeira.", "He's the real deal.")]
    [InlineData("Ele era verdadeiro.", "He's the real deal.")]
    [InlineData("Verdadeiros.", "They're the real deal.")]
    [InlineData("Ele é verdadeiro.", "He isn't the real deal.")]
    [InlineData("Ele não é verdadeiro.", "He's not the real deal.")]
    [InlineData("A realidade dos agentes.", "The real deal from our officers.")]
    [InlineData("A coisa real.", "This painting is the genuine article.")]
    [InlineData("Sim, verdadeiro, sim.", "He's the real dealer.")]
    [InlineData("Ele é verdadeiro.", "He's the real deal. She's the real deal.")]
    [InlineData("Ele é verdadeiro. Ela é verdadeira.", "He's the real deal.")]
    [InlineData("Ele conhece as suas coisas.", "He recognizes his belongings.")]
    [InlineData("Ele conhece as suas coisas.", "He knows her stuff.")]
    [InlineData("Ele conhece as suas coisas.", "He knows his stuff is downstairs.")]
    [InlineData("Ele conhece as suas coisas antigas.", "He knows his stuff.")]
    [InlineData("Ele conhece as suas coisas.", "He knows his stuff. She knows her stuff.")]
    public void MissingAmbiguousAndLiteralEvidenceIsProtected(string source, string? english) =>
        Assert.Equal(source, new PortugueseConverter().Convert(source, reference: ReferenceHint.FromText(english)));

    [Fact]
    public void SettingsAndLocalChoicesWin()
    {
        const string source = "Sim, verdadeiro, sim.";
        var reference = ReferenceHint.FromText("Yeah, he's the real deal, yeah.");
        foreach (var options in new[] { new ConversionOptions(Vocabulary: false),
            new ConversionOptions(Colloquial: false), new ConversionOptions(DisabledRules: "english-praise-idioms") })
            Assert.Equal(source, new PortugueseConverter(options).Convert(source, reference: reference));
        var words = new PortugueseLocalWordList();
        words.Ignored.Add("Sim, verdadeiro, sim");
        Assert.Equal(source, new PortugueseConverter(localWordList: words).Convert(source, reference: reference));
        words = new PortugueseLocalWordList();
        words.Words.Add(("verdadeiro", "genuíno"));
        Assert.Equal("Sim, genuíno, sim.", new PortugueseConverter(localWordList: words).Convert(source, reference: reference));
        words = new PortugueseLocalWordList();
        words.Words.Add(("conhecer", "reconhecer"));
        Assert.Equal("Ele conhece as suas coisas.", new PortugueseConverter(localWordList: words)
            .Convert("Ele conhece as suas coisas.", reference: ReferenceHint.FromText("He knows his stuff.")));
    }

    [Fact]
    public void EpisodeCueKeepsTimesAndOriginal()
    {
        const string source = "Sim, verdadeiro, sim.\nPode parar de falar agora?";
        var block = new SrtBlock { StartMs = 25126, EndMs = 28555, Text = source };
        var conversion = Assert.Single(SubtitleConversion.Convert(new[] { block }, ConversionOptions.Default,
            english: new string?[] { "Yeah, he's the real deal, yeah. Will you just... just stop talking now?" }));
        Assert.Equal("Sim, ele é mesmo bom, sim.\nPodes parar de falar agora?", conversion.Text);
        Assert.Equal(source, block.Text);
        Assert.Equal(25126, block.StartMs);
        Assert.Equal(28555, block.EndMs);
        Assert.Contains(conversion.Rules, rule => rule.Id == "english-praise-idioms");
    }
}
