using SubtitleEdit.Plugins.TightHyphen.Logic;

namespace TightHyphen.Tests;

public class HyphenTightenerTests
{
    [Theory]
    [InlineData("- Hi!\n- Hello.", "-Hi!\n-Hello.")]
    [InlineData("- Hi!\r\n- Hello.", "-Hi!\r\n-Hello.")]
    [InlineData("-  Two spaces", "-Two spaces")]
    [InlineData("- Only one line", "-Only one line")]
    [InlineData("Hi!\n- Hello.", "Hi!\n-Hello.")]
    public void Tightens(string input, string expected)
    {
        Assert.Equal(expected, HyphenTightener.Tighten(input));
    }

    [Theory]
    [InlineData("<i>- Hi!</i>\n<i>- Hello.</i>", "<i>-Hi!</i>\n<i>-Hello.</i>")]
    [InlineData("<font color=\"#ff0000\">- Hi!</font>", "<font color=\"#ff0000\">-Hi!</font>")]
    [InlineData("{\\an8}- Hi!", "{\\an8}-Hi!")]
    public void Tightens_AfterTags(string input, string expected)
    {
        Assert.Equal(expected, HyphenTightener.Tighten(input));
    }

    [Theory]
    [InlineData("-Hi!\n-Hello.")]
    [InlineData("Well - maybe.")]
    [InlineData("Not a dialog")]
    [InlineData("- 5 dollars?")]
    [InlineData("- ")]
    [InlineData("")]
    public void LeavesUnchanged(string input)
    {
        Assert.Equal(input, HyphenTightener.Tighten(input));
    }
}
