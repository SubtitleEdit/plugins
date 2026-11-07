using SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;
using SubtitleEdit.Plugins.Shared;

namespace BrazilianToEuropeanPortuguese.Tests;

public sealed class DroppedSubjunctiveSubjectTests
{
    [Theory]
    [InlineData("Bem, talvez precise\ndo trabalho.", "Well, maybe you need the job.",
        "Bem, talvez precises\ndo trabalho.")]
    [InlineData("Talvez queira sair.", "Maybe you want to leave.", "Talvez queiras sair.")]
    [InlineData("Talvez possa ajudar.", "Perhaps you can help.", "Talvez possas ajudar.")]
    [InlineData("Bem, talvez tenha razão.", "Well, maybe you are right.", "Bem, talvez tenhas razão.")]
    [InlineData("Talvez saiba.", "Maybe you know.", "Talvez saibas.")]
    [InlineData("Talvez venha amanhã.", "Maybe you'll come tomorrow.", "Talvez venhas amanhã.")]
    [InlineData("Talvez consiga resolver.", "Maybe you can solve it.", "Talvez consigas resolver.")]
    [InlineData("Talvez encontre trabalho.", "Maybe you will find work.", "Talvez encontres trabalho.")]
    [InlineData("Talvez não queira sair.", "Maybe you don't want to leave.", "Talvez não queiras sair.")]
    [InlineData("E se precisar de um ombro\npara chorar...", "And if you ever need a shoulder to cry on...",
        "E se precisares de um ombro\npara chorar...")]
    [InlineData("Se precisar de ajuda.", "If you need help.", "Se precisares de ajuda.")]
    [InlineData("E se precisar de ajuda?", "What if you need help?", "E se precisares de ajuda?")]
    [InlineData("Mas se comprar uma casa.", "But if you buy a house.", "Mas se comprares uma casa.")]
    [InlineData("Ou se encontrar trabalho.", "Or if you find work.", "Ou se encontrares trabalho.")]
    [InlineData("E se conseguir entrar.", "And if you manage to enter.", "E se conseguires entrar.")]
    [InlineData("Quando precisar de ajuda.", "When you need help.", "Quando precisares de ajuda.")]
    [InlineData("Quando tiver tempo.", "When you have time.", "Quando tiveres tempo.")]
    [InlineData("E se puder ajudar.", "And if you can help.", "E se puderes ajudar.")]
    [InlineData("Se fizer isso.", "If you do that.", "Se fizeres isso.")]
    [InlineData("Se disser isso.", "If you say that.", "Se disseres isso.")]
    [InlineData("Quando vier.", "When you come.", "Quando vieres.")]
    [InlineData("Quando atinge certa idade,\nfaz esse tipo de coisa.",
        "When you reach a certain age, you do that sort of thing.",
        "Quando atinges certa idade,\nfazes esse tipo de coisa.")]
    [InlineData("Se precisar de ajuda, sabe onde estou.",
        "If you need help, you know where I am.",
        "Se precisares de ajuda, sabes onde estou.")]
    [InlineData("Se souber.", "If you know.", "Se souberes.")]
    [InlineData("A menos que precise de ajuda.", "Unless you need help.", "A menos que precises de ajuda.")]
    [InlineData("Assim que terminar.", "As soon as you finish.", "Assim que terminares.")]
    [InlineData("E se não precisar de ajuda.", "And if you don't need help.", "E se não precisares de ajuda.")]
    [InlineData("<i>E se precisar de ajuda...</i>", "And if you need help...", "<i>E se precisares de ajuda...</i>")]
    [InlineData("E SE PRECISAR DE AJUDA.", "And if you need help.", "E SE PRECISARES DE AJUDA.")]
    public void ReferenceConfirmedDroppedSubjectsAgree(string source, string english, string expected)
    {
        var converter = new PortugueseConverter();
        var reference = ReferenceHint.FromText(english);
        Assert.Equal(expected, converter.Convert(source, reference: reference));
        Assert.Equal(expected, converter.Convert(expected, reference: reference));
    }

    [Theory]
    [InlineData("O trabalho não precisa\nmais de mim.", "The job doesn't need me anymore.",
        "O trabalho não precisa\nmais de mim.")]
    [InlineData("Bem, talvez precise\ndo trabalho.", "Well, maybe she needs the job.",
        "Bem, talvez precise\ndo trabalho.")]
    [InlineData("Talvez ele precise de ajuda.", "Maybe you should help him.", "Talvez ele precise de ajuda.")]
    [InlineData("E se precisar de ajuda.", "And if she needs help.", "E se precisar de ajuda.")]
    [InlineData("Se ele precisar de ajuda.", "If you help him.", "Se ele precisar de ajuda.")]
    [InlineData("Se Maria comprar uma casa.", "If you help Maria buy a house.", "Se Maria comprar uma casa.")]
    [InlineData("Quando o trabalho terminar.", "When you finish the job.", "Quando o trabalho terminar.")]
    [InlineData("E se precisar de ajuda.", null, "E se precisar de ajuda.")]
    [InlineData("Talvez precise de ajuda.", null, "Talvez precise de ajuda.")]
    [InlineData("E se precisares de ajuda.", null, "E se precisares de ajuda.")]
    [InlineData("E se precisarem de ajuda.", null, "E se precisarem de ajuda.")]
    [InlineData("Se precisar de ajuda.", "If you and Maria need help.", "Se precisar de ajuda.")]
    [InlineData("Ouça, não vai contar à minha mãe, certo?",
        "You're not going to tell my mom, right?", "Ouça, não vai contar à minha mãe, certo?")]
    public void DifferentAndUncertainSubjectsAreNotGuessed(string source, string? english, string expected) =>
        Assert.Equal(expected, new PortugueseConverter().Convert(source, reference: ReferenceHint.FromText(english)));

    [Theory]
    [InlineData("E se precisar de ajuda.", "And if you need help.")]
    [InlineData("Bem, talvez precise do trabalho.", "Well, maybe you need the job.")]
    public void FormalAddressAndDisabledRulesStayProtected(string source, string english)
    {
        var reference = ReferenceHint.FromText(english)!;
        Assert.Equal(source, new PortugueseConverter().Convert(source,
            reference: reference with { FormalAddress = true }));
        Assert.Equal(source, new PortugueseConverter(new ConversionOptions(YouToTu: false))
            .Convert(source, reference: reference));
        Assert.Equal(source, new PortugueseConverter(new ConversionOptions(DisabledRules: "address"))
            .Convert(source, reference: reference));
    }

    [Fact]
    public void ConditionalConjunctionIsPreservedWithPronounsDisabled() =>
        Assert.Equal("E se precisares de ajuda.",
            new PortugueseConverter(new ConversionOptions(Pronouns: false)).Convert("E se precisar de ajuda.",
                reference: ReferenceHint.FromText("And if you need help.")));

    [Fact]
    public void IgnoredComplementsStayUntouched()
    {
        var words = new PortugueseLocalWordList();
        words.Ignored.Add("se precisar de ajuda");
        Assert.Equal("E se precisar de ajuda.", new PortugueseConverter(localWordList: words)
            .Convert("E se precisar de ajuda.", reference: ReferenceHint.FromText("And if you need help.")));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SharedCuePipelinePreservesSubjectsAndTimes(bool sentenceContext)
    {
        var source = "689\n00:35:37,468 --> 00:35:40,486\nO trabalho não precisa\nmais de mim.\n\n" +
            "690\n00:35:40,490 --> 00:35:43,642\nBem, talvez precise\ndo trabalho.\n\n" +
            "693\n00:35:53,818 --> 00:35:58,204\nE se precisar de um ombro\npara chorar...\n\n";
        var blocks = SubRipParser.Parse(source);
        var result = SubtitleConversion.Convert(blocks, new ConversionOptions(SentenceContext: sentenceContext),
            english: new string?[] { "The job doesn't need me anymore.", "Well, maybe you need the job.",
                "And if you ever need a shoulder to cry on..." });
        Assert.Equal("O trabalho não precisa\nmais de mim.", result[0].Text);
        Assert.Equal("Bem, talvez precises\ndo trabalho.", result[1].Text);
        Assert.Equal("E se precisares de um ombro\npara chorar...", result[2].Text);
        Assert.Equal(3, result.Count);
        Assert.Equal(2153818L, blocks[2].StartMs);
        Assert.Equal(2158204L, blocks[2].EndMs);
    }
}
