using SubtitleEdit.Plugins.LinesUnbreaker.Logic;

namespace LinesUnbreaker.Tests;

public class LineUnbreakerTests
{
    private static readonly UnbreakOptions Defaults = new();

    [Fact]
    public void JoinsShortLines()
    {
        Assert.Equal("Where are you going?", LineUnbreaker.Unbreak("Where are\nyou going?", Defaults));
        Assert.Equal("Where are you going?", LineUnbreaker.Unbreak("Where are\r\nyou going?", Defaults));
    }

    [Fact]
    public void SingleLine_IsUnchanged()
    {
        Assert.Equal("Hello there.", LineUnbreaker.Unbreak("Hello there.", Defaults));
    }

    [Fact]
    public void TooLong_IsUnchanged()
    {
        var text = "This first line is quite long already\nand so is this second one.";
        Assert.Equal(text, LineUnbreaker.Unbreak(text, Defaults));
    }

    [Fact]
    public void TagsAreNotCounted()
    {
        // 20 visible characters, far more with the tags.
        var text = "<font color=\"#ff0000\"><i>Where are</i></font>\n<i>you going?</i>";
        var options = new UnbreakOptions { MaxLength = 20 };
        Assert.Equal("<font color=\"#ff0000\"><i>Where are</i></font> <i>you going?</i>", LineUnbreaker.Unbreak(text, options));
    }

    [Fact]
    public void MaxLength_IsInclusive()
    {
        Assert.Equal("abcd efgh", LineUnbreaker.Unbreak("abcd\nefgh", new UnbreakOptions { MaxLength = 9 }));
        Assert.Equal("abcd\nefgh", LineUnbreaker.Unbreak("abcd\nefgh", new UnbreakOptions { MaxLength = 8 }));
    }

    [Fact]
    public void ItalicOnEachLine_BecomesOneTag()
    {
        Assert.Equal("<i>Where are you going?</i>", LineUnbreaker.Unbreak("<i>Where are</i>\n<i>you going?</i>", Defaults));
    }

    [Fact]
    public void ExtraSpaces_AreCollapsed()
    {
        Assert.Equal("Where are you?", LineUnbreaker.Unbreak("Where are  \n  you?", Defaults));
    }

    [Fact]
    public void Dialog_IsSkippedByDefault()
    {
        var text = "- Hi!\n- Hello.";
        Assert.Equal(text, LineUnbreaker.Unbreak(text, Defaults));
        Assert.Equal("- Hi! - Hello.", LineUnbreaker.Unbreak(text, new UnbreakOptions { SkipDialogs = false }));
    }

    [Fact]
    public void ItalicDialog_IsSkipped()
    {
        var text = "<i>- Hi!</i>\n<i>- Hello.</i>";
        Assert.Equal(text, LineUnbreaker.Unbreak(text, Defaults));
    }

    [Fact]
    public void Narrator_IsSkippedByDefault()
    {
        var text = "JOHN: Wait\nfor me.";
        Assert.Equal(text, LineUnbreaker.Unbreak(text, Defaults));
        Assert.Equal("JOHN: Wait for me.", LineUnbreaker.Unbreak(text, new UnbreakOptions { SkipNarrator = false }));
    }

    [Fact]
    public void Time_IsNotANarrator()
    {
        Assert.Equal("See you at 12:30 tomorrow.", LineUnbreaker.Unbreak("See you at 12:30\ntomorrow.", Defaults));
    }

    [Fact]
    public void Moods_SkippedOnlyWhenAsked()
    {
        var text = "(sighs) Well,\nmaybe.";
        Assert.Equal("(sighs) Well, maybe.", LineUnbreaker.Unbreak(text, Defaults));
        Assert.Equal(text, LineUnbreaker.Unbreak(text, new UnbreakOptions { SkipMoods = true }));
    }

    [Fact]
    public void VisibleLength_IgnoresTagsAndBreaks()
    {
        Assert.Equal(6, LineUnbreaker.VisibleLength("{\\an8}<i>abc</i>\r\ndef"));
    }
}
