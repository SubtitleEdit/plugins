using SubtitleEdit.Plugins.AssaTagCheck;

namespace AssaTagCheck.Tests;

public class TagCheckerTests
{
    private const string Header =
        "[Script Info]\nScriptType: v4.00+\nPlayResX: 1920\nPlayResY: 1080\n\n" +
        "[V4+ Styles]\n" +
        "Format: Name, Fontname, Fontsize, PrimaryColour, SecondaryColour, OutlineColour, BackColour, Bold, Italic, Underline, StrikeOut, ScaleX, ScaleY, Spacing, Angle, BorderStyle, Outline, Shadow, Alignment, MarginL, MarginR, MarginV, Encoding\n" +
        "Style: Default,Arial,48,&H00FFFFFF,&H000000FF,&H00000000,&H80000000,0,0,0,0,100,100,0,0,1,2,1,2,10,10,10,1\n" +
        "Style: Italic,Arial,48,&H00FFFFFF,&H000000FF,&H00000000,&H80000000,0,-1,0,0,100,100,0,0,1,2,1,2,10,10,10,1\n";

    private static readonly AssaScript Script = AssaScript.Parse(Header);
    private static readonly HashSet<RuleId> All = RuleCatalog.All.ToHashSet();

    private static LineResult Check(string text, string style = "Default", double duration = 2000, params RuleId[] only) =>
        TagChecker.CheckLine(text, style, duration, Script, only.Length > 0 ? only.ToHashSet() : All);

    private static string Fix(string text, string style = "Default") => Check(text, style).After;

    [Theory]
    [InlineData("{\\an8}Hello", "{\\an8}Hello")]
    [InlineData("<i>Hello</i>", "<i>Hello</i>")]
    [InlineData("{\\pos(960,540)\\fad(200,200)}Hi", "{\\pos(960,540)\\fad(200,200)}Hi")]
    [InlineData("{\\t(0,500,\\fs60)}Grow", "{\\t(0,500,\\fs60)}Grow")]
    [InlineData("{\\clip(m 0 0 l 100 0 100 100)}Clip", "{\\clip(m 0 0 l 100 0 100 100)}Clip")]
    [InlineData("{a comment}Text", "{a comment}Text")]
    [InlineData("{\\k20}Ka{\\k30}ra{\\k25}o{\\k40}ke", "{\\k20}Ka{\\k30}ra{\\k25}o{\\k40}ke")]
    [InlineData("Price is {fair}", "Price is {fair}")]
    public void CleanLinesStayUnchanged(string text, string expected)
    {
        var result = Check(text);
        Assert.Equal(expected, result.After);
        Assert.Empty(result.Issues);
    }

    [Theory]
    [InlineData("<i>One</i><i>Two</i>", "<i>OneTwo</i>")]
    [InlineData("<i>One</i>\n<i>Two</i>", "<i>One\nTwo</i>")]
    [InlineData("<i>One</i> <i>Two</i>", "<i>One Two</i>")]
    [InlineData("{\\i1}One{\\i0}{\\i1}Two{\\i0}", "{\\i1}OneTwo{\\i0}")]
    [InlineData("{\\b1}One{\\b0\\b1}Two", "{\\b1}OneTwo")]
    [InlineData("<i>A<i>B</i>", "<i>AB</i>")]
    [InlineData("<i></i>Text", "Text")]
    [InlineData("{\\i0}Text", "Text")]
    [InlineData("<b>A</b></b>", "<b>A</b>")]
    public void RemovesUnneededEndStartTags(string text, string expected)
    {
        var result = Check(text);
        Assert.Equal(expected, result.After);
        Assert.Contains(result.Issues, i => i.Rule == RuleId.UnneededEndStart);
    }

    [Fact]
    public void UnderlineAcrossSpaceIsKept()
    {
        // Joining would underline the space between the words.
        Assert.Equal("<u>One</u> <u>Two</u>", Fix("<u>One</u> <u>Two</u>"));
    }

    [Fact]
    public void ItalicStyleKeepsEndTagThatMatters()
    {
        // The style is italic, so {\i0} turns italic off - only the {\i1} is redundant.
        Assert.Equal("{\\i0}Upright", Fix("{\\i1}{\\i0}Upright", "Italic"));
        Assert.Equal("Text", Fix("{\\i1}Text", "Italic"));
    }

    [Theory]
    [InlineData("{\\fs20\\fs30}Text", "{\\fs30}Text")]
    [InlineData("{\\c&H0000FF&}{\\c&H00FF00&}Text", "{\\c&H00FF00&}Text")]
    public void RemovesOverriddenTags(string text, string expected)
    {
        var result = Check(text);
        Assert.Equal(expected, result.After);
        Assert.Contains(result.Issues, i => i.Rule == RuleId.Overridden);
    }

    [Theory]
    [InlineData("{\\c&H0000FF&}Red {\\c&H0000FF&}still red", "{\\c&H0000FF&}Red still red")]
    [InlineData("{\\fs48}Same as style", "Same as style")]
    [InlineData("{\\c&HFFFFFF&}White", "White")]
    [InlineData("{\\bord2\\shad1}Style values", "Style values")]
    public void RemovesTagsAlreadyInEffect(string text, string expected)
    {
        var result = Check(text);
        Assert.Equal(expected, result.After);
        Assert.Contains(result.Issues, i => i.Rule == RuleId.AlreadyInEffect);
    }

    [Fact]
    public void TransformStopsTidying()
    {
        Assert.Equal("{\\fs60\\t(\\fs30)\\fs60}Text", Fix("{\\fs60\\t(\\fs30)\\fs60}Text"));
    }

    [Fact]
    public void DrawingsAreNotTidied()
    {
        Assert.Equal("{\\p1\\c&HFFFFFF&}m 0 0 l 10 0{\\p0}", Fix("{\\p1\\c&HFFFFFF&}m 0 0 l 10 0{\\p0}"));
    }

    [Fact]
    public void MergesAdjacentBlocks()
    {
        var result = Check("{\\an8}{\\pos(960,100)}Top");
        Assert.Equal("{\\an8\\pos(960,100)}Top", result.After);
        Assert.Contains(result.Issues, i => i.Rule == RuleId.MergeBlocks);
    }

    [Theory]
    [InlineData("{\\an8Hello", "{\\an8}Hello")]
    [InlineData("{\\i1\\c&H0000FF&Red", "{\\i1\\c&H0000FF&}Red")]
    [InlineData("Text{", "Text")]
    public void ClosesUnclosedBlocks(string text, string expected)
    {
        var result = Check(text, only: RuleId.UnclosedBlock);
        Assert.Equal(expected, result.After);
        Assert.Contains(result.Issues, i => i.Rule == RuleId.UnclosedBlock && i.Fixable);
    }

    [Fact]
    public void UnclosedCommentIsReportedNotFixed()
    {
        var result = Check("Hello {world", only: RuleId.UnclosedBlock);
        Assert.Equal("Hello {world", result.After);
        Assert.False(Assert.Single(result.Issues).Fixable);
    }

    [Theory]
    [InlineData("\\an8}Hello", "{\\an8}Hello")]
    [InlineData("{\\an8}}Hello", "{\\an8}Hello")]
    public void FixesStrayBraces(string text, string expected)
    {
        Assert.Equal(expected, Check(text, only: RuleId.StrayBrace).After);
    }

    [Theory]
    [InlineData("{an8}Top", "{\\an8}Top")]
    [InlineData("{/an8}Top", "{\\an8}Top")]
    [InlineData("{i1}Italic", "{\\i1}Italic")]
    public void AddsMissingBackslash(string text, string expected)
    {
        Assert.Equal(expected, Check(text, only: RuleId.MissingBackslash).After);
    }

    [Theory]
    [InlineData("{\\pos(10,20}Text", "{\\pos(10,20)}Text")]
    [InlineData("{\\pos(10,20\\i1}Text", "{\\pos(10,20)\\i1}Text")]
    [InlineData("{\\pos10,20}Text", "{\\pos(10,20)}Text")]
    [InlineData("{\\t(0,500,\\fs60}Text", "{\\t(0,500,\\fs60)}Text")]
    public void AddsMissingParenthesis(string text, string expected)
    {
        Assert.Equal(expected, Check(text, only: RuleId.MissingParenthesis).After);
    }

    [Theory]
    [InlineData("{\\pos(10 20)}Text", "{\\pos(10,20)}Text")]
    [InlineData("{\\pos(10;20)}Text", "{\\pos(10,20)}Text")]
    [InlineData("{\\move(0 0 100 100)}Text", "{\\move(0,0,100,100)}Text")]
    public void FixesArgumentSeparators(string text, string expected)
    {
        Assert.Equal(expected, Check(text, only: RuleId.ArgumentSeparator).After);
    }

    [Fact]
    public void ReportsWrongArgumentCount()
    {
        var result = Check("{\\move(0,0,100)}Text", only: RuleId.ArgumentCount);
        Assert.Equal("{\\move(0,0,100)}Text", result.After);
        Assert.Contains("4 or 6", Assert.Single(result.Issues).Message);
    }

    [Fact]
    public void ConvertsHtmlColor()
    {
        // #RRGGBB -> &HBBGGRR&
        Assert.Equal("{\\c&H3366FF&}Text", Check("{\\c#FF6633}Text", only: RuleId.HtmlColor).After);
    }

    [Theory]
    [InlineData("{\\I1}Text", "{\\i1}Text")]
    [InlineData("{\\AN8}Text", "{\\an8}Text")]
    [InlineData("{\\xyz5\\an8}Text", "{\\an8}Text")]
    public void FixesUnknownTags(string text, string expected)
    {
        Assert.Equal(expected, Check(text, only: RuleId.UnknownTag).After);
    }

    [Theory]
    [InlineData("{\\fsbig}Text")]
    [InlineData("{\\an0}Text")]
    [InlineData("{\\c&HZZ&}Text")]
    [InlineData("{\\bold}Text")]
    public void ReportsInvalidValues(string text)
    {
        var result = Check(text, only: RuleId.InvalidValue);
        Assert.Equal(text, result.After);
        Assert.Single(result.Issues);
    }

    [Fact]
    public void FirstPositionWins()
    {
        var result = Check("{\\pos(10,10)}A{\\move(0,0,5,5)\\an8\\an2}B", only: RuleId.FirstWins);
        Assert.Equal("{\\pos(10,10)}A{\\an8}B", result.After);
        Assert.Equal(2, result.Issues.Count);
    }

    [Fact]
    public void ShortensTooLongFade()
    {
        Assert.Equal("{\\fad(333,667)}Text", Check("{\\fad(500,1000)}Text", duration: 1000, only: RuleId.FadeTooLong).After);
    }

    [Fact]
    public void ReportsPositionOffScreen()
    {
        var result = Check("{\\pos(2500,540)}Text", only: RuleId.PositionOffScreen);
        Assert.Contains("1920x1080", Assert.Single(result.Issues).Message);
    }

    [Fact]
    public void FixesUndefinedStyles()
    {
        Assert.Equal("Default", Check("Text", "default", only: RuleId.UndefinedStyle).StyleAfter);
        Assert.Equal("Default", Check("Text", "Missing", only: RuleId.UndefinedStyle).StyleAfter);
        Assert.Equal("{\\r}Text", Check("{\\rNope}Text", only: RuleId.UndefinedStyle).After);
        Assert.Equal("{\\rItalic}Text", Check("{\\ritalic}Text", only: RuleId.UndefinedStyle).After);
    }

    [Fact]
    public void DisabledRulesChangeNothing()
    {
        var result = TagChecker.CheckLine("<i>A</i><i>B</i>{\\pos(1 2)}", "Default", 1000, Script, new HashSet<RuleId>());
        Assert.Equal("<i>A</i><i>B</i>{\\pos(1 2)}", result.After);
        Assert.Empty(result.Issues);
    }

    [Fact]
    public void KeepsWindowsLineBreaks()
    {
        Assert.Equal("<i>One\r\nTwo</i>", Fix("<i>One</i>\r\n<i>Two</i>"));
    }

    [Fact]
    public void ReportsMissingFonts()
    {
        var lines = new List<(string, string?)> { ("Text", "Default"), ("{\\fnComic Neue}Hi", "Default") };
        var issues = TagChecker.CheckScript(lines, Script, name => name == "Arial");
        Assert.Equal("Font \"Comic Neue\" not found", Assert.Single(issues).Title);
    }

    [Fact]
    public void AttachedFontsCount()
    {
        var script = AssaScript.Parse(Header + "\n[Fonts]\nfontname: comicneue_0.ttf\n!!!!\n");
        var lines = new List<(string, string?)> { ("{\\fnComic Neue}Hi", "Default") };
        Assert.Empty(TagChecker.CheckScript(lines, script, name => name == "Arial"));
    }
}
