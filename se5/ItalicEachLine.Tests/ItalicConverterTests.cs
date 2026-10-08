using SubtitleEdit.Plugins.ItalicEachLine;

namespace ItalicEachLine.Tests;

public class ItalicConverterTests
{
    [Theory]
    [InlineData("<i>Line one\nLine two</i>", "<i>Line one</i>\n<i>Line two</i>")]
    [InlineData("<i>Line one\r\nLine two</i>", "<i>Line one</i>\r\n<i>Line two</i>")]
    [InlineData("Hello <i>world\nand more</i> end", "Hello <i>world</i>\n<i>and more</i> end")]
    [InlineData("<i>- Hi!\n- Hello.\n- Bye.</i>", "<i>- Hi!</i>\n<i>- Hello.</i>\n<i>- Bye.</i>")]
    [InlineData("{\\an8}<i>Top\nlines</i>", "{\\an8}<i>Top</i>\n<i>lines</i>")]
    [InlineData("<I>Upper\ncase</I>", "<i>Upper</i>\n<i>case</i>")]
    [InlineData("<i>Unclosed\nitalic", "<i>Unclosed</i>\n<i>italic</i>")]
    [InlineData("<i>A\n\nB</i>", "<i>A</i>\n\n<i>B</i>")]
    [InlineData("Hello <i>\nWorld</i>", "Hello \n<i>World</i>")]
    [InlineData("<i><font color=\"red\">A\nB</font></i>", "<i><font color=\"red\">A</font></i>\n<i><font color=\"red\">B</font></i>")]
    [InlineData("<i><b>A</b>\nB</i>", "<i><b>A</b></i>\n<i>B</i>")]
    public void ToEachLine(string input, string expected) =>
        Assert.Equal(expected, ItalicConverter.ToEachLine(input));

    [Theory]
    [InlineData("<i>Line one</i>\n<i>Line two</i>")]
    [InlineData("<i>Single line</i>")]
    [InlineData("No italic\nat all")]
    [InlineData("<i>Italic</i>\nNot italic")]
    [InlineData("")]
    public void ToEachLineUnchanged(string input) =>
        Assert.Equal(input, ItalicConverter.ToEachLine(input));

    [Theory]
    [InlineData("<i>Line one</i>\n<i>Line two</i>", "<i>Line one\nLine two</i>")]
    [InlineData("<i>Line one</i>\r\n<i>Line two</i>", "<i>Line one\r\nLine two</i>")]
    [InlineData("<i>A</i>\n<i>B</i>\n<i>C</i>", "<i>A\nB\nC</i>")]
    [InlineData("Hello <i>world</i>\n<i>and more</i> end", "Hello <i>world\nand more</i> end")]
    [InlineData("<I>Upper</I>\n<I>case</I>", "<I>Upper\ncase</I>")]
    [InlineData("<i>A</i>\n\n<i>B</i>", "<i>A\n\nB</i>")]
    public void ToOneTag(string input, string expected) =>
        Assert.Equal(expected, ItalicConverter.ToOneTag(input));

    [Theory]
    [InlineData("<i>Line one\nLine two</i>")]
    [InlineData("<i>Italic</i>\nNot italic")]
    [InlineData("<i>Italic</i> <i>twice</i>")]
    [InlineData("Plain\ntext")]
    public void ToOneTagUnchanged(string input) =>
        Assert.Equal(input, ItalicConverter.ToOneTag(input));

    [Theory]
    [InlineData("<i>Line one\nLine two</i>")]
    [InlineData("Hello <i>world\nand more</i> end")]
    [InlineData("<i>- Hi!\r\n- Hello.\r\n- Bye.</i>")]
    [InlineData("<i><font color=\"red\">A\nB</font></i>")]
    public void RoundTrip(string oneTag)
    {
        var eachLine = ItalicConverter.ToEachLine(oneTag);
        Assert.NotEqual(oneTag, eachLine);
        Assert.Equal(eachLine, ItalicConverter.ToEachLine(eachLine));
        Assert.Equal(ItalicConverter.ToOneTag(eachLine), ItalicConverter.ToOneTag(ItalicConverter.ToOneTag(eachLine)));
    }
}
