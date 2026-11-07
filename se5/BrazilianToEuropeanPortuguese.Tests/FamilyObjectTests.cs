using SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;
using SubtitleEdit.Plugins.Shared;

namespace BrazilianToEuropeanPortuguese.Tests;

public sealed class FamilyObjectTests
{
    [Theory]
    [InlineData("Mãe e tio Pedro", "Mom and Uncle Peter", "A Mãe e o tio Pedro")]
    [InlineData("Pai e tia Ana", "Dad and Aunt Anne", "O Pai e a tia Ana")]
    [InlineData("Mãe chegou.", "Mom arrived.", "A Mãe chegou.")]
    [InlineData("Pai está acordado.", "Dad's awake.", "O Pai está acordado.")]
    [InlineData("MÃE E TIO PEDRO", "Mom and Uncle Peter", "A MÃE E O TIO PEDRO")]
    [InlineData("Mãe e tio Pedro\no encontraram e\no levaram ao hospital.",
        "Mom and Uncle Peter found you and got you to the hospital.",
        "A Mãe e o tio Pedro\nencontraram-te e\nlevaram-te ao hospital.")]
    [InlineData("o encontraram e\no levaram ao hospital.",
        "are the ones that found you and got you to the hospital.",
        "encontraram-te e\nlevaram-te ao hospital.")]
    [InlineData("Eles o ajudaram.", "They helped you.", "Eles ajudaram-te.")]
    [InlineData("Ela a viu.", "She saw you.", "Ela viu-te.")]
    [InlineData("Não o encontraram.", "They didn't find you.", "Não te encontraram.")]
    [InlineData("Ela disse que o encontraram.", "She said they found you.", "Ela disse que te encontraram.")]
    [InlineData("Quando o encontraram, chamaram ajuda.", "When they found you, they called for help.",
        "Quando te encontraram, chamaram ajuda.")]
    [InlineData("Ela o levou ao hospital.", "She brought you to the hospital.",
        "Ela trouxe-te ao hospital.")]
    public void ConfirmedSubjectsAndObjectsAreStable(string source, string english, string expected)
    {
        var converter = new PortugueseConverter();
        var hint = ReferenceHint.FromText(english);
        Assert.Equal(expected, converter.Convert(source, reference: hint));
        Assert.Equal(expected, converter.Convert(expected, reference: hint));
    }

    [Theory]
    [InlineData("Mãe!", "Mom!", "Mãe!")]
    [InlineData("Mãe, ajuda!", "Mom, help!", "Mãe, ajuda!")]
    [InlineData("Mãe e filho", "Mother and child", "Mãe e filho")]
    [InlineData("Mãe e tio Pedro", null, "Mãe e tio Pedro")]
    [InlineData("A Mãe e o tio Pedro", "Mom and Uncle Peter", "A Mãe e o tio Pedro")]
    [InlineData("o encontraram.", "They found him.", "o encontraram.")]
    [InlineData("o encontraram.", null, "o encontraram.")]
    [InlineData("o encontraram.", "They found you and him.", "o encontraram.")]
    [InlineData("o encontraram e a ajudaram.", "They found you and helped her.",
        "o encontraram e a ajudaram.")]
    [InlineData("o levaram ao hospital.", "They took him to the hospital.", "o levaram ao hospital.")]
    [InlineData("o levaram ao hospital.", "They got you a room.", "o levaram ao hospital.")]
    [InlineData("o encontraram.", "They found you a room.", "o encontraram.")]
    public void OtherSubjectsAndVocativesArePreserved(string source, string? english, string expected) =>
        Assert.Equal(expected, new PortugueseConverter().Convert(source, reference: ReferenceHint.FromText(english)));

    [Fact]
    public void OptionsAndIgnoredPhrasesAreRespected()
    {
        const string source = "o encontraram.";
        var hint = ReferenceHint.FromText("They found you.")!;
        foreach (var options in new[] { new ConversionOptions(YouToTu: false),
                     new ConversionOptions(Pronouns: false), new ConversionOptions(DisabledRules: "reference-object-clitics") })
        {
            var result = new PortugueseConverter(options).Convert(source, reference: hint);
            Assert.DoesNotContain("te", result);
        }
        Assert.Equal("o encontraram.", new PortugueseConverter().Convert(source,
            reference: hint with { FormalAddress = true }));
        Assert.Equal("Mãe e tio Pedro", new PortugueseConverter(new ConversionOptions(Pronouns: false))
            .Convert("Mãe e tio Pedro", reference: ReferenceHint.FromText("Mom and Uncle Peter")));
        var words = new PortugueseLocalWordList();
        words.Ignored.Add("Mãe e tio Pedro");
        words.Ignored.Add("o encontraram");
        var converter = new PortugueseConverter(localWordList: words);
        Assert.Equal("Mãe e tio Pedro", converter.Convert("Mãe e tio Pedro",
            reference: ReferenceHint.FromText("Mom and Uncle Peter")));
        Assert.Equal(source, converter.Convert(source, reference: hint));
    }

    [Fact]
    public void SplitFamilySubjectAndPastObjectsWorkWithoutSentenceJoining()
    {
        var blocks = SubRipParser.Parse(
            "1\n00:00:01,000 --> 00:00:03,000\nMãe e tio Pedro\n\n" +
            "2\n00:00:03,010 --> 00:00:05,000\no encontraram e\no levaram ao hospital.\n\n");
        var result = SubtitleConversion.Convert(blocks, new ConversionOptions(), english: new string?[] {
            "Mom and Uncle Peter", "are the ones that found you and got you to the hospital." });
        Assert.Equal("A Mãe e o tio Pedro", result[0].Text);
        Assert.Equal("encontraram-te e\nlevaram-te ao hospital.", result[1].Text);
    }
}
