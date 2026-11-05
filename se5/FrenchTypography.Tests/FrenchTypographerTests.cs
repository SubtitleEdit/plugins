using SubtitleEdit.Plugins.FrenchTypography.Logic;

namespace FrenchTypography.Tests;

public class FrenchTypographerTests
{
    private const string S = " ";
    private const string N = " ";

    private static readonly FrenchTypographyOptions France = new();
    private static readonly FrenchTypographyOptions Canada = new() { Variant = FrenchVariant.Canada };

    [Theory]
    [InlineData("Quoi?", "Quoi" + S + "?")]
    [InlineData("Quoi ?", "Quoi" + S + "?")]
    [InlineData("Quoi   !", "Quoi" + S + "!")]
    [InlineData("Attention: danger", "Attention" + S + ": danger")]
    [InlineData("Oui; non", "Oui" + S + "; non")]
    [InlineData("Quoi?!", "Quoi" + S + "?!")]
    [InlineData("Quoi ? !", "Quoi" + S + "? !")]
    [InlineData("(oui)?", "(oui)" + S + "?")]
    [InlineData("<i>Quoi</i>?", "<i>Quoi</i>" + S + "?")]
    [InlineData("{\\an8}Quoi?", "{\\an8}Quoi" + S + "?")]
    [InlineData("- Quoi?\n- Rien!", "- Quoi" + S + "?\n- Rien" + S + "!")]
    [InlineData("Quoi" + N + "?", "Quoi" + S + "?")]
    public void France_SpaceBeforeHighPunctuation(string input, string expected) =>
        Assert.Equal(expected, FrenchTypographer.Fix(input, France));

    [Theory]
    [InlineData("Il est 10:30.")]
    [InlineData("Va sur http://exemple.fr")]
    [InlineData("Format 16:9")]
    [InlineData("? Quoi")]
    [InlineData("Et alors…?")]
    [InlineData("<font color=\"red\">Bonjour</font>")]
    [InlineData("Quoi" + S + "?")]
    public void France_LeavesAlone(string input) =>
        Assert.Equal(input, FrenchTypographer.Fix(input, France));

    [Fact]
    public void Narrow_UsesNarrowNoBreakSpace() =>
        Assert.Equal("Quoi" + N + "?", FrenchTypographer.Fix("Quoi ?", France with { Space = FrenchSpace.Narrow }));

    [Theory]
    [InlineData("Quoi ?", "Quoi?")]
    [InlineData("Quoi" + S + "!", "Quoi!")]
    [InlineData("Oui ; non", "Oui; non")]
    [InlineData("Attention: danger", "Attention" + S + ": danger")]
    public void Canada_OnlyColonIsSpaced(string input, string expected) =>
        Assert.Equal(expected, FrenchTypographer.Fix(input, Canada));

    [Theory]
    [InlineData("Il a dit \"bonjour\".", "Il a dit «" + S + "bonjour" + S + "».")]
    [InlineData("Il a dit « bonjour ».", "Il a dit «" + S + "bonjour" + S + "».")]
    [InlineData("Il a dit «bonjour».", "Il a dit «" + S + "bonjour" + S + "».")]
    [InlineData("\"Viens\nici\"", "«" + S + "Viens\nici" + S + "»")]
    [InlineData("Un seul \" guillemet", "Un seul \" guillemet")]
    public void Guillemets(string input, string expected) =>
        Assert.Equal(expected, FrenchTypographer.Fix(input, France));

    [Fact]
    public void Quotes_Off_KeepsStraightQuotes() =>
        Assert.Equal("Il a dit \"bonjour\".", FrenchTypographer.Fix("Il a dit \"bonjour\".", France with { ConvertQuotes = false }));

    [Theory]
    [InlineData("Je sais...", "Je sais…")]
    [InlineData("...et puis", "…et puis")]
    [InlineData("Attends....", "Attends....")]
    public void Ellipsis(string input, string expected) =>
        Assert.Equal(expected, FrenchTypographer.Fix(input, France));

    [Fact]
    public void Ellipsis_Off() =>
        Assert.Equal("Je sais...", FrenchTypographer.Fix("Je sais...", France with { ConvertEllipsis = false }));

    [Fact]
    public void KeepsWindowsLineBreaks() =>
        Assert.Equal("Quoi" + S + "?\r\nRien" + S + "!", FrenchTypographer.Fix("Quoi?\r\nRien!", France));
}
