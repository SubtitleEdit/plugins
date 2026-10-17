using SubtitleEdit.Plugins.HI2UC.Logic;
using SubtitleEdit.Plugins.HI2UC.Logic.Strategies;

namespace HI2UC.Tests;

public class CasingTests
{
    private static readonly HiOptions Upper = new();

    [Fact]
    public void Uppercase_SkipsTags()
    {
        var sut = new UppercaseConverter(new ChunkReader());
        Assert.Equal("<font color=\"#0000ff\">FOOBAR</font>: MMM.", sut.Execute("<font color=\"#0000ff\">Foobar</font>: Mmm."));
    }

    [Fact]
    public void Uppercase_PlainText()
    {
        var sut = new UppercaseConverter(new ChunkReader());
        Assert.Equal("CREATURE GROWLING", sut.Execute("creature growling"));
    }

    [Fact]
    public void Uppercase_MultiLineWithTags()
    {
        var sut = new UppercaseConverter(new ChunkReader());
        var input = "- <font color=\"#800000\">(CREATURE)</font>\n- <font color=\"#800000\">(objects crashing and)</font>";
        var expected = "- <font color=\"#800000\">(CREATURE)</font>\n- <font color=\"#800000\">(OBJECTS CRASHING AND)</font>";
        Assert.Equal(expected, sut.Execute(input));
        Assert.Equal(expected, sut.Execute(expected));
    }

    [Fact]
    public void ChunkReader_UnclosedTag_DoesNotThrow()
    {
        var sut = new UppercaseConverter(new ChunkReader());
        Assert.Equal("ABC <I", sut.Execute("abc <i"));
    }

    [Theory]
    [InlineData(HiStyle.UpperCase, "(door slams)", "(DOOR SLAMS)")]
    [InlineData(HiStyle.TitleCase, "(door slams)", "(Door Slams)")]
    [InlineData(HiStyle.SentenceCase, "(DOOR SLAMS)", "(Door slams)")]
    [InlineData(HiStyle.LowerCase, "(DOOR SLAMS)", "(door slams)")]
    [InlineData(HiStyle.UpperLowerCase, "(door)", "(DoOr)")]
    public void Moods_UseStyle(HiStyle style, string input, string expected)
    {
        Assert.Equal(expected, HiCasing.Convert(input, new HiOptions { Style = style, Narrator = false }).After);
    }

    [Fact]
    public void Moods_BracketsAndItalic()
    {
        Assert.Equal("<i>[LAUGHS]</i> Okay.", HiCasing.Convert("<i>[laughs]</i> Okay.", Upper).After);
    }

    [Fact]
    public void Narrator_LabelOnly()
    {
        Assert.Equal("JOHN: Where are you?", HiCasing.Convert("John: Where are you?", Upper).After);
    }

    [Fact]
    public void Narrator_EachLine_KeepsLineBreak()
    {
        Assert.Equal("- JOHN: Hi.\r\n- MARY: Hello.", HiCasing.Convert("- John: Hi.\r\n- Mary: Hello.", Upper).After);
        Assert.Equal("- JOHN: Hi.\n- MARY: Hello.", HiCasing.Convert("- John: Hi.\n- Mary: Hello.", Upper).After);
    }

    [Theory]
    [InlineData("See you at 12:30.")]
    [InlineData("Visit http://example.com")]
    [InlineData("Why? Because: no.")]
    [InlineData("Subtitles corrected by: someone")]
    [InlineData("Line one\nline two")]
    public void Narrator_LeavesNonLabels(string text)
    {
        Assert.Equal(text, HiCasing.Convert(text, new HiOptions { Moods = false }).After);
    }

    [Fact]
    public void MoodsAndNarrator_BothApplyToTheSameLine()
    {
        var result = HiCasing.Convert("(laughs) John: Hi.", Upper);
        Assert.Equal("(LAUGHS) JOHN: Hi.", result.After);
        Assert.True(result.MoodsChanged);
        Assert.True(result.NarratorChanged);
    }

    [Fact]
    public void Unbalanced_IsFlagged()
    {
        var result = HiCasing.Convert("(laughs) and (sighs", Upper);
        Assert.True(result.Unbalanced);
        Assert.Contains("Unbalanced", result.Comment);
    }

    [Fact]
    public void NothingToDo_IsUnchanged()
    {
        var result = HiCasing.Convert("(LAUGHS) Okay.", Upper);
        Assert.Equal("(LAUGHS) Okay.", result.After);
        Assert.False(result.MoodsChanged);
    }
}
