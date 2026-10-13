using SubtitleEdit.Plugins.FixEncodingErrors;
using System.Text;

namespace FixEncodingErrors.Tests;

public class EncodingFixerTests
{
    static EncodingFixerTests() => Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

    /// <summary>What a UTF-8 text looks like after being read with <paramref name="codePage"/>.</summary>
    private static string Break(string text, int codePage = 1252) =>
        Encoding.GetEncoding(codePage).GetString(Encoding.UTF8.GetBytes(text));

    [Theory]
    [InlineData("rÃ©sumÃ©", "résumé")]
    [InlineData("donâ€™t", "don’t")]
    [InlineData("â€œQuoteâ€\u009D", "“Quote”")]
    [InlineData("â€œQuoteâ€", "“Quote”")] // 0x9D lost
    [InlineData("25Â°C", "25°C")]
    [InlineData("ï»¿Hello", "Hello")]
    [InlineData("Wait Ã  bientÃ´t", "Wait à bientôt")] // A0 turned into a space
    [InlineData("ðŸ˜€ hi", "😀 hi")]
    public void FixesWestern(string broken, string expected)
    {
        Assert.Equal(expected, EncodingFixer.Fix(broken, 1252, confirmed: true));
    }

    [Theory]
    [InlineData("cache of such communiquã©s,", "cache of such communiqués,")]
    [InlineData("and dust off my rã©sumã©.", "and dust off my résumé.")]
    [InlineData("madame la secrã©taire.", "madame la secrétaire.")]
    public void FixesTextLowercasedAfterItBroke(string broken, string expected)
    {
        // Ed's Madam Secretary subtitles: © alone tells the subtitle is broken.
        Assert.Equal(expected, EncodingFixer.Fix(broken, 1252, confirmed: false));
    }

    [Theory]
    [InlineData("Ça va? Où êtes-vous? À bientôt, Œdipe!")]
    [InlineData("Größe, Übermaß, naïve, Señor, Ñandú, Ærø")]
    [InlineData("“Smart quotes” – dashes — and… ellipsis ‘ok’ 😀")]
    public void RoundTripsWestern(string text)
    {
        Assert.Equal(text, EncodingFixer.Fix(Break(text), 1252, confirmed: true));
    }

    [Theory]
    [InlineData("Привет, как дела? Всё хорошо.", 1251)]
    [InlineData("Příliš žluťoučký kůň úpěl ďábelské ódy.", 1250)]
    [InlineData("Καλημέρα κόσμε", 1253)]
    [InlineData("Günaydın, nasılsın? Şimdi değil.", 1254)]
    public void RoundTripsOtherCodePages(string text, int codePage)
    {
        // Detection may pick a code page that reads the same bytes the same way (1252/1254 for Turkish).
        var broken = Break(text, codePage);
        var detected = EncodingFixer.Detect(new[] { broken }).CodePage;
        Assert.Equal(text, EncodingFixer.Fix(broken, detected, confirmed: true));
        Assert.Equal(text, EncodingFixer.Fix(broken, codePage, confirmed: true));
    }

    [Fact]
    public void DetectsCyrillic()
    {
        Assert.Equal(1251, EncodingFixer.Detect(new[] { Break("Привет, как дела?", 1251) }).CodePage);
    }

    [Theory]
    [InlineData("Привет, как дела?")]
    [InlineData("Καλημέρα κόσμε")]
    [InlineData("שלום עולם")]
    [InlineData("مرحبا بالعالم")]
    public void FixesOtherScriptsMisreadAsWestern(string text)
    {
        var broken = Break(text);
        Assert.Equal(1252, EncodingFixer.Detect(new[] { broken }).CodePage);
        Assert.Equal(text, EncodingFixer.Fix(broken, 1252, EncodingFixer.IsBroken(new[] { broken }, 1252)));
    }

    [Fact]
    public void FixesDoubleBroken()
    {
        Assert.Equal("café", EncodingFixer.Fix(Break(Break("café")), 1252, confirmed: true));
    }

    [Theory]
    [InlineData("Não”")]
    [InlineData("A minha irmã… e a mãe")]
    [InlineData("«Não»")]
    [InlineData("Irmã Lúcia")]
    [InlineData("Ça coûte 25 €, café»")]
    [InlineData("Plain ASCII text")]
    public void LeavesRealTextAlone(string text)
    {
        Assert.Equal(0, EncodingFixer.CountTelling(new[] { text }, 1252));
        Assert.Equal(text, EncodingFixer.Fix(text, 1252, confirmed: false));
    }

    [Theory]
    [InlineData("Привет, Сёма! В доме.")]
    [InlineData("Příliš žluťoučký kůň")]
    public void LeavesRealTextAloneInItsOwnCodePage(string text)
    {
        Assert.Equal(1252, EncodingFixer.Detect(new[] { text }).CodePage);
        Assert.Equal(text, EncodingFixer.Fix(text, 1252, confirmed: false));
    }

    [Fact]
    public void AmbiguousMatchNeedsConfirmation()
    {
        // "Ã–" (Ö) at a word end could be real text; fixed once the subtitle is known to be broken.
        Assert.Equal("BÃ–", EncodingFixer.Fix("BÃ–", 1252, confirmed: false));
        Assert.Equal("BÖ", EncodingFixer.Fix("BÃ–", 1252, confirmed: true));
        Assert.True(EncodingFixer.IsBroken(new[] { "Ã–sterreich", "cafÃ©" }, 1252));
        Assert.False(EncodingFixer.IsBroken(new[] { "Ã–sterreich" }, 1252));
    }

    [Fact]
    public void DetectsWesternForEd()
    {
        var lines = new[] { "and dust off my rã©sumã©.", "I'll send you his rã©sumã©." };
        Assert.Equal(1252, EncodingFixer.Detect(lines).CodePage);
        Assert.Equal(4, EncodingFixer.CountTelling(lines, 1252));
    }

    [Fact]
    public void RealMacedonianIsNotBroken()
    {
        // ј, џ, ќ, ѓ are continuation bytes in Windows-1251: "ој" must not become Greek "μ", "рџ" not "п".
        var lines = new[] { "Тој е мојот брат.", "Џорџ веќе учење, која боја." };
        Assert.False(EncodingFixer.IsBroken(lines, 1251));
        Assert.Equal(1252, EncodingFixer.Detect(lines).CodePage);
        Assert.All(lines, l => Assert.Equal(l, EncodingFixer.Fix(l, 1251, confirmed: false)));
    }

    [Fact]
    public void KeepsTagsAndLineBreaks()
    {
        Assert.Equal("<i>résumé</i>\r\n{\\an8}café", EncodingFixer.Fix("<i>rÃ©sumÃ©</i>\r\n{\\an8}cafÃ©", 1252, confirmed: false));
    }
}
