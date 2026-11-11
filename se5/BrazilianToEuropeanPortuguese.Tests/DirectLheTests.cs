using SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;

namespace BrazilianToEuropeanPortuguese.Tests;

public sealed class DirectLheTests
{
    [Theory]
    [InlineData("Eu lhe respeito imensamente.", "I have so much respect for you.", "Eu respeito-o imensamente.")]
    [InlineData("Eu respeito-lhe imensamente.", "I respect you immensely.", "Eu respeito-o imensamente.")]
    [InlineData("Eu não lhe respeito.", "I don't respect you.", "Eu não o respeito.")]
    [InlineData("Sei que ele lhe respeita.", "I know he respects you.", "Sei que ele o respeita.")]
    [InlineData("Eu lhe admirava.", "I admired you.", "Eu admirava-o.")]
    [InlineData("Eu lhe admiro.", "I admire her.", "Eu admiro-a.")]
    [InlineData("Eu lhes admiro.", "I admire them.", "Eu admiro-os.")]
    [InlineData("Eu lhe amo.", "I love you.", "Eu amo-o.")]
    [InlineData("Ela lhe conhece.", "She knows him.", "Ela conhece-o.")]
    [InlineData("Ele lhe abraçou.", "He hugged her.", "Ele abraçou-a.")]
    [InlineData("Eu lhe protegerei.", "I will protect you.", "Eu protegê-lo-ei.")]
    [InlineData("Eu respeitar-lhe-ei.", "I will respect you.", "Eu respeitá-lo-ei.")]
    [InlineData("Eu não lhe respeitaria.", "I would not respect you.", "Eu não o respeitaria.")]
    [InlineData("<i>Eu lhe respeito.</i>", "I respect you.", "<i>Eu respeito-o.</i>")]
    [InlineData("EU LHE RESPEITO.", "I respect you.", "EU RESPEITO-O.")]
    [InlineData("Lhe respeito.", "I respect you.", "Respeito-o.")]
    [InlineData("Senhora, eu lhe respeito.", "Ma'am, I respect you.", "Senhora, eu respeito-a.")]
    public void DirectObjectsAreNotDatives(string source, string english, string expected)
    {
        var converter = new PortugueseConverter();
        var reference = ReferenceHint.FromText(english)! with { FormalAddress = true };
        Assert.Equal(expected, converter.Convert(source, reference: reference));
        Assert.Equal(expected, converter.Convert(expected, reference: reference));
    }

    [Theory]
    [InlineData("Eu lhe respeito imensamente.", "I have so much respect for you.", "Eu respeito-te imensamente.")]
    [InlineData("Eu respeito-lhe.", "I respect you.", "Eu respeito-te.")]
    [InlineData("Eu não lhe respeito.", "I don't respect you.", "Eu não te respeito.")]
    [InlineData("Eu lhe admirava.", "I admired you.", "Eu admirava-te.")]
    [InlineData("Eu lhe protegerei.", "I will protect you.", "Eu proteger-te-ei.")]
    [InlineData("Eu lhe respeito.", "I respect him.", "Eu respeito-o.")]
    public void InformalAddressChangesOnlyTheConfirmedAddressee(string source, string english, string expected) =>
        Assert.Equal(expected, new PortugueseConverter().Convert(source,
            reference: ReferenceHint.FromText(english)! with { ForceInformal = true }));

    [Theory]
    [InlineData("Eu lhe respeito.", null, "Eu respeito-lhe.")]
    [InlineData("Eu lhe admiro a coragem.", "I admire your courage.", "Eu admiro-lhe a coragem.")]
    [InlineData("Eu admiro-lhe muito a coragem.", "I admire you for your courage.", "Eu admiro-lhe muito a coragem.")]
    [InlineData("Eu lhe conheço os defeitos.", "I know his flaws.", "Eu conheço-lhe os defeitos.")]
    [InlineData("Eu lhe telefono.", "I phone you.", "Eu telefono-lhe.")]
    [InlineData("Ele lhe mentiu.", "He lied to you.", "Ele mentiu-lhe.")]
    [InlineData("Eu lhe respeito e lhe admiro.", "I respect and admire you.", "Eu respeito-lhe e admiro-lhe.")]
    [InlineData("Eu lhe respeito.", "I respect you and her.", "Eu respeito-lhe.")]
    [InlineData("Eu lhe respeito.", "I respect them.", "Eu respeito-lhe.")]
    [InlineData("Eu lhes respeito.", "I respect him.", "Eu respeito-lhes.")]
    public void PossessiveDativesAndAmbiguousRecipientsAreProtected(string source, string? english, string expected)
    {
        var hint = ReferenceHint.FromText(english);
        var converter = new PortugueseConverter();
        var actual = converter.Convert(source, reference: hint is null ? null : hint with { FormalAddress = true });
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void DisabledSettingsAndLocalOverridesWin()
    {
        foreach (var options in new[] { new ConversionOptions(Pronouns: false),
            new ConversionOptions(DisabledRules: "pronouns") })
            Assert.Equal("Eu lhe respeito.", new PortugueseConverter(options)
                .Convert("Eu lhe respeito.", reference: ReferenceHint.FromText("I respect you.")));
        var words = new PortugueseLocalWordList();
        words.Words.Add(("respeitar", "respeitar"));
        Assert.Equal("Eu respeito-lhe.", new PortugueseConverter(localWordList: words)
            .Convert("Eu lhe respeito.", reference: ReferenceHint.FromText("I respect you.")));
    }
}
