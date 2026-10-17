using SubtitleEdit.Plugins.HIColorer.Logic;

namespace HIColorer.Tests;

public class HiColorerTests
{
    private static readonly ColorOptions Options = new() { NarratorColor = "#ffff00", MoodsColor = "#00ffff", MusicColor = "#ff00ff" };

    [Fact]
    public void Moods_AreColored()
    {
        Assert.Equal("<font color=\"#00ffff\">(door slams)</font> Hi.", HiColorer.Paint("(door slams) Hi.", Options));
        Assert.Equal("Hi <font color=\"#00ffff\">[laughs]</font>", HiColorer.Paint("Hi [laughs]", Options));
    }

    [Fact]
    public void Narrator_IsColored_EachLine()
    {
        Assert.Equal("- <font color=\"#ffff00\">MAN</font>: Baby.\n- <font color=\"#ffff00\">JUNE</font>: No.",
            HiColorer.Paint("- MAN: Baby.\n- JUNE: No.", Options));
    }

    [Fact]
    public void Narrator_KeepsCrLf()
    {
        Assert.Equal("<font color=\"#ffff00\">MAN</font>: Hi.\r\nBye.", HiColorer.Paint("MAN: Hi.\r\nBye.", Options));
    }

    [Fact]
    public void Narrator_InsideItalic_KeepsTagsOutsideTheColor()
    {
        Assert.Equal("<i><font color=\"#ffff00\">MAN</font></i>: Hi.", HiColorer.Paint("<i>MAN</i>: Hi.", Options));
        Assert.Equal("<i><font color=\"#ffff00\">MAN</font>: Hi.</i>", HiColorer.Paint("<i>MAN: Hi.</i>", Options));
    }

    [Theory]
    [InlineData("See you at 12:30.")]
    [InlineData("Subtitles by: someone")]
    [InlineData("Visit https://example.com")]
    [InlineData("(laughs) John: Hi")]
    public void Narrator_Ignored(string text)
    {
        var options = new ColorOptions { Moods = false, Music = false };
        Assert.Equal(text, HiColorer.Paint(text, options));
    }

    [Fact]
    public void Narrator_NameContainingBy_IsColored()
    {
        Assert.Equal("<font color=\"#ffff00\">BOBBY</font>: Hey.", HiColorer.Paint("BOBBY: Hey.", Options));
    }

    [Fact]
    public void Music_WholeTextIsColored()
    {
        Assert.Equal("<font color=\"#ff00ff\">♪ La la la ♪</font>", HiColorer.Paint("♪ La la la ♪", Options));
        Assert.Equal("<font color=\"#ff00ff\"><i>♫ Hum ♫</i></font>", HiColorer.Paint("<i>♫ Hum ♫</i>", Options));
    }

    [Fact]
    public void Music_OnlyWhenEnabled()
    {
        var options = new ColorOptions { Music = false };
        Assert.Equal("♪ La la ♪", HiColorer.Paint("♪ La la ♪", options));
    }

    [Fact]
    public void Recolor_ReplacesOldColor()
    {
        Assert.Equal("<font color=\"#00ffff\">(sighs)</font>", HiColorer.Paint("<font color=\"#ff0000\">(sighs)</font>", Options));
    }

    [Fact]
    public void AlreadyColored_IsUnchanged()
    {
        var text = "<font color=\"#00ffff\">(sighs)</font>";
        Assert.Equal(text, HiColorer.Paint(text, Options));
    }

    [Fact]
    public void LineWithNothingToColor_KeepsItsFontTags()
    {
        var text = "<font color=\"#ff0000\">Red words</font>";
        Assert.Equal(text, HiColorer.Paint(text, Options));
    }

    [Fact]
    public void Remove_StripsFontTagsOnly()
    {
        Assert.Equal("<i>(sighs)</i> Hi", HiColorer.Apply("<i><font color=\"#00ffff\">(sighs)</font></i> Hi", ColorMode.Remove, Options));
        Assert.Equal("Plain", HiColorer.Apply("Plain", ColorMode.Remove, Options));
    }
}
