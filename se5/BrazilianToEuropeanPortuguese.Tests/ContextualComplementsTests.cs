using System.Text.RegularExpressions;
using SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;
using SubtitleEdit.Plugins.Shared;

namespace BrazilianToEuropeanPortuguese.Tests;

public sealed class ContextualComplementsTests
{
    [Theory]
    [InlineData("Bem, parece o juiz caiu do telhado.", "Bem, parece que o juiz caiu do telhado.")]
    [InlineData("Parece ela chegou.", "Parece que ela chegou.")]
    [InlineData("Parecia os agentes chegaram.", "Parecia que os agentes chegaram.")]
    [InlineData("Pareceu a testemunha mentiu.", "Pareceu que a testemunha mentiu.")]
    [InlineData("Parece-me o juiz caiu.", "Parece-me que o juiz caiu.")]
    [InlineData("Não parece a testemunha mentiu.", "Não parece que a testemunha mentiu.")]
    [InlineData("Parece a minha nova equipa não se opõe.", "Parece que a minha nova equipa não se opõe.")]
    [InlineData("Parece o presidente da câmara caiu.", "Parece que o presidente da câmara caiu.")]
    [InlineData("Parece ela o viu.", "Parece que ela o viu.")]
    [InlineData("Parecerá o juiz foi enganado.", "Parecerá que o juiz foi enganado.")]
    [InlineData("Pareceria o juiz tinha razão.", "Pareceria que o juiz tinha razão.")]
    [InlineData("Não parece a testemunha disse tudo.", "Não parece que a testemunha disse tudo.")]
    [InlineData("Parece ela chegou e parece ele saiu.", "Parece que ela chegou e parece que ele saiu.")]
    [InlineData("PARECE ELA CHEGOU.", "PARECE QUE ELA CHEGOU.")]
    [InlineData("<i>Parece ela chegou.</i>", "<i>Parece que ela chegou.</i>")]
    public void FiniteComplementsRecoverQueAndRemainStable(string source, string expected)
    {
        var converter = new PortugueseConverter(new ConversionOptions(YouToTu: false));
        var result = converter.Convert(source);
        Assert.Equal(expected, result);
        Assert.Equal(result, converter.Convert(result));
        for (var i = 0; i < source.Length; i++)
        {
            if (source[i] != ' ') continue;
            foreach (var newline in new[] { "\n", "\r\n" })
            {
                var wrapped = converter.Convert(source[..i] + newline + source[(i + 1)..]);
                Assert.Equal(expected, Regex.Replace(wrapped, @"\s+", " "));
                Assert.Equal(1, wrapped.Count(c => c == '\n'));
            }
        }
    }

    [Theory]
    [InlineData("Bem, parece o juiz.")]
    [InlineData("Parece o juiz que caiu.")]
    [InlineData("Parece o juiz reformado.")]
    [InlineData("Parece a minha mãe.")]
    [InlineData("Parece uma testemunha credível.")]
    [InlineData("Parece um dia lento no escritório.")]
    [InlineData("O juiz parece estar cansado.")]
    [InlineData("Parece ter caído.")]
    [InlineData("Parece que o juiz caiu.")]
    [InlineData("Parece ela chegar.")]
    [InlineData("Parece o juiz e o advogado.")]
    [InlineData("Parece o juiz do caso.")]
    [InlineData("Parece o que aconteceu.")]
    [InlineData("Parece a minha mãe quando era jovem.")]
    [InlineData("Parece o homem com quem ela falou.")]
    [InlineData("Parece\n\nela chegou.")]
    [InlineData("- Parece\n- ela chegou.")]
    [InlineData("Parece um rapaz seguro.")]
    [InlineData("Parece um homem livre.")]
    [InlineData("Parece um homem são.")]
    [InlineData("-Parece o juiz.\n-Caiu do telhado.")]
    public void ResemblanceInfinitivesRelativesAndDifferentSpeakersAreProtected(string source) =>
        Assert.Equal(source, new PortugueseConverter(new ConversionOptions(YouToTu: false)).Convert(source));

    [Fact]
    public void SplitFiniteComplementsUseOnlyARecentSelectedSameSpeakerContinuation()
    {
        var blocks = new[] {
            new SrtBlock { StartMs = 297064, EndMs = 299887, Text = "Bem, parece o Juiz" },
            new SrtBlock { StartMs = 299900, EndMs = 302191, Text = "caiu de cabeça\ndo telhado." },
        };
        var options = new ConversionOptions(YouToTu: false);
        var result = SubtitleConversion.Convert(blocks, options);
        Assert.Equal("Bem, parece que o Juiz", result[0].Text);
        Assert.Equal("caiu de cabeça\ndo telhado.", result[1].Text);
        Assert.Equal("Bem, parece o Juiz", SubtitleConversion.Convert(blocks, options, selected: new HashSet<int> { 0 })[0].Text);
        Assert.Equal("Bem, parece o Juiz", SubtitleConversion.Convert(blocks, options, speakers: new[] { "A", "B" })[0].Text);
        blocks[1].StartMs = 310000;
        Assert.Equal("Bem, parece o Juiz", SubtitleConversion.Convert(blocks, options)[0].Text);
        Assert.Equal("Bem, parece o Juiz", blocks[0].Text);
        Assert.Equal(299887, blocks[0].EndMs);
    }

    [Theory]
    [InlineData("Ela está aqui.", "Mande entrar.", "Mande-a entrar.")]
    [InlineData("Ele está aqui.", "Mande entrar.", "Mande-o entrar.")]
    [InlineData("Elas estão aqui.", "Mande entrar.", "Mande-as entrar.")]
    [InlineData("Eles estão aqui.", "Mande entrar.", "Mande-os entrar.")]
    [InlineData("Ela chegou.", "Deixe entrar.", "Deixe-a entrar.")]
    [InlineData("Ela está à espera.", "Mande subir.", "Mande-a subir.")]
    [InlineData("Ela está aqui.", "Não mande entrar.", "Não a mande entrar.")]
    [InlineData("ELA ESTÁ AQUI.", "MANDE ENTRAR.", "MANDE-A ENTRAR.")]
    [InlineData("Ela está aqui.", "<i>Mande entrar.</i>", "<i>Mande-a entrar.</i>")]
    public void ExplicitArrivalReferentsControlGenderAndNumberWithoutChangingCommandPerson(
        string previous, string command, string expected)
    {
        var blocks = Pair(previous, command);
        var options = new ConversionOptions(Address: AddressMode.Formal);
        var result = SubtitleConversion.Convert(blocks, options);
        Assert.Equal(expected, result[1].Text);
        blocks[1].Text = result[1].Text;
        Assert.Equal(expected, SubtitleConversion.Convert(blocks, options)[1].Text);
    }

    [Theory]
    [InlineData("Ela está aqui.", "Mande entrar.", "Let her in.", "Mande-a entrar.")]
    [InlineData("Ela está aqui.", "Mande entrar.", "Let's have her.", "Mande-a entrar.")]
    [InlineData("Ela está aqui.", "Mande entrar.", "Send him in.", "Mande entrar.")]
    [InlineData("Ela está aqui.", "Mande entrar.", "Send her and him in.", "Mande entrar.")]
    [InlineData("Ela está aqui.", "Mande entrar.", "Let her out.", "Mande entrar.")]
    [InlineData("Ela está aqui.", "Mande entrar.", "Send money in.", "Mande entrar.")]
    [InlineData("Ela está aqui.", "Mande entrar.", "Her father is coming.", "Mande entrar.")]
    [InlineData("Ela está aqui.", "Não mande entrar.", "Don't let her in.", "Não a mande entrar.")]
    [InlineData("Ela está aqui.", "Mande entrar.", "Don't let her in.", "Mande entrar.")]
    [InlineData("Ela está aqui.", "Não mande entrar.", "Let her in.", "Não mande entrar.")]
    [InlineData("Ela está aqui, com ele.", "Mande entrar.", "Send her in.", "Mande entrar.")]
    [InlineData("Ela não está aqui.", "Mande entrar.", "Send her in.", "Mande entrar.")]
    [InlineData("Ele disse que ela está aqui.", "Mande entrar.", "Send her in.", "Mande entrar.")]
    [InlineData("Ela está aqui.", "Mande o agente entrar.", "Send him in.", "Mande o agente entrar.")]
    [InlineData("Ela está aqui.", "Mande-a entrar.", "Send her in.", "Mande-a entrar.")]
    [InlineData("Ela está aqui.", "Mande entrar o agente.", "Send him in.", "Mande entrar o agente.")]
    [InlineData("Ela está aqui.", "Mande entrar dinheiro.", "Send money in.", "Mande entrar dinheiro.")]
    public void ConflictsMultipleReferentsAndExpressedObjectsAreProtected(
        string previous, string command, string english, string expected) =>
        Assert.Equal(expected, SubtitleConversion.Convert(Pair(previous, command),
            new ConversionOptions(Address: AddressMode.Formal), english: new string?[] { null, english })[1].Text);

    [Fact]
    public void ContextRecoveryRespectsSettingsSelectionAndLocalOverrides()
    {
        var blocks = Pair("Ela está aqui.", "Mande entrar.");
        var options = new ConversionOptions(Address: AddressMode.Formal);
        foreach (var disabled in new[] { options with { Pronouns = false }, options with { DisabledRules = "arrival-object" } })
            Assert.Equal("Mande entrar.", SubtitleConversion.Convert(blocks, disabled)[1].Text);
        Assert.Equal("Mande entrar.", SubtitleConversion.Convert(blocks, options, selected: new HashSet<int> { 1 })[1].Text);
        Assert.Equal("Manda-a entrar.", SubtitleConversion.Convert(blocks, options with { Address = AddressMode.Informal })[1].Text);
        blocks[1].StartMs = 10000;
        Assert.Equal("Mande entrar.", SubtitleConversion.Convert(blocks, options)[1].Text);
        var words = new PortugueseLocalWordList();
        words.Ignored.Add("Mande entrar");
        Assert.Equal("Mande entrar.", SubtitleConversion.Convert(Pair("Ela está aqui.", "Mande entrar."), options, words)[1].Text);
        foreach (var disabled in new[] { new ConversionOptions(Colloquial: false),
            new ConversionOptions(DisabledRules: "parecer-complement") })
            Assert.Equal("Parece ela chegou.", new PortugueseConverter(disabled).Convert("Parece ela chegou."));
        words.Ignored.Clear();
        words.Ignored.Add("parece");
        Assert.Equal("Parece ela chegou.", new PortugueseConverter(localWordList: words).Convert("Parece ela chegou."));
        words.Ignored.Clear();
        words.Words.Add(("parece", "aparenta"));
        Assert.Equal("Aparenta ela chegou.", new PortugueseConverter(localWordList: words).Convert("Parece ela chegou."));
        words.Words.Clear();
        words.Words.Add(("mande", "peça"));
        Assert.Equal("Peça entrar.", SubtitleConversion.Convert(Pair("Ela está aqui.", "Mande entrar."), options, words)[1].Text);
    }

    [Theory]
    [InlineData("Mande", "entrar", "Mande-a entrar.")]
    [InlineData("Mandem", "sair", "Mandem-na sair.")]
    [InlineData("Deixem", "passar", "Deixem-na passar.")]
    [InlineData("Deixe", "esperar", "Deixe-a esperar.")]
    [InlineData("Mande", "descer", "Mande-a descer.")]
    public void CommandsPreserveWrappingSoundChangesAndReviewNotes(string verb, string infinitive, string expected)
    {
        foreach (var newline in new[] { " ", "\n", "\r\n" })
        {
            var blocks = Pair("Ela está" + newline + "aqui.", verb + newline + infinitive + ".");
            var result = SubtitleConversion.Convert(blocks, new ConversionOptions(Address: AddressMode.Formal));
            Assert.Equal(expected, Regex.Replace(result[1].Text, @"\s+", " "));
            Assert.Equal(newline.Count(c => c == '\n'), result[1].Text.Count(c => c == '\n'));
            Assert.Contains("referent", result[1].ContextNote);
        }
    }

    [Theory]
    [InlineData("caiu.", "Parece que o juiz")]
    [InlineData("não caiu.", "Parece que o juiz")]
    [InlineData("se opõe.", "Parece que o juiz")]
    [InlineData("-Caiu.", "Parece o juiz")]
    [InlineData("parece cansado.", "Parece o juiz")]
    public void ContinuationsRequireACompleteSubjectInTheOriginalCue(string next, string expected)
    {
        var blocks = Pair("Parece o juiz", next);
        blocks[1].StartMs = 2500;
        Assert.Equal(expected, SubtitleConversion.Convert(blocks, new ConversionOptions(YouToTu: false))[0].Text);
    }

    private static SrtBlock[] Pair(string previous, string command) =>
    [
        new SrtBlock { StartMs = 1000, EndMs = 2400, Text = previous },
        new SrtBlock { StartMs = 4000, EndMs = 5500, Text = command },
    ];
}
