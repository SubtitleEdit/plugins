using SubtitleEdit.Plugins.PersianErrors;

namespace PersianErrors.Tests;

public class PersianFixerTests
{
    private static RuleGroup Group(string name) => RuleSet.Default.Single(g => g.Name == name);

    [Fact]
    public void LoadsAllGroupsInFileOrder()
    {
        var names = RuleSet.Default.Select(g => g.Name).ToList();

        Assert.Equal(GroupInfo.All.Select(g => g.Name), names);
        Assert.True(RuleSet.Default.Sum(g => g.Rules.Count) > 1000);
    }

    [Fact]
    public void EveryGroupExampleIsChangedByItsGroup()
    {
        foreach (var info in GroupInfo.All)
        {
            var after = PersianFixer.Fix(info.Example, new[] { Group(info.Name) });
            Assert.True(after != info.Example, $"{info.Name} does not change its example");
        }
    }

    [Theory]
    [InlineData("Change Arabic Chars to Persian", "املاك شدني", "املاک شدنی")]
    [InlineData("Add Missing Spaces", "سلام،عزیزم", "سلام، عزیزم")]
    [InlineData("Space to Invisible Space", "میشود", "می\u200Cشود")]
    [InlineData("Fix Abbreviations", "اف بی آی", "اف.بی.آی")]
    [InlineData("OCR", "سوسابقه", "سوءسابقه")]
    [InlineData("Fix Unicode Control Char", "\u202B\u202B سلام", "\u202Bسلام")]
    [InlineData("Remove Unneeded Spaces", "گفت :  باشه", "گفت: باشه")]
    [InlineData("Fix Wrong Chars", "سلام,, خوبی?", "سلام، خوبی؟")]
    [InlineData("Space to Invisible Space", "میشود خانه ها", "می\u200Cشود خانه\u200Cها")]
    [InlineData("Remove Leading Dots", "در ادامه...", "در ادامه")]
    [InlineData("Remove Dot from the End of Line", ".می\u200Cشود", "می\u200Cشود")]
    public void FixesWithOneGroup(string group, string input, string expected)
    {
        Assert.Equal(expected, PersianFixer.Fix(input, new[] { Group(group) }));
    }

    [Fact]
    public void PlainRuleWithQuoteMatches()
    {
        // The original plugin escaped quotes in plain rules too, so these never matched.
        Assert.Equal("\"سلام\"", PersianFixer.Fix("\"\u200Cسلام\u200C\"", new[] { Group("Space to Invisible Space") }));
    }

    [Fact]
    public void ReportsTheGroupsThatChangedTheText()
    {
        var changed = new List<string>();

        PersianFixer.Fix("كتاب ي", RuleSet.Default, changed);

        Assert.Equal(new[] { "Change Arabic Chars to Persian", "Space to Invisible Space" }, changed);
    }

    [Theory]
    [InlineData("\n")]
    [InlineData("\r\n")]
    public void KeepsTheLineBreakStyle(string newLine)
    {
        var result = PersianFixer.Fix("سلام،عزیزم" + newLine + "شدني", RuleSet.Default);

        Assert.Equal("سلام، عزیزم" + newLine + "شدنی", result);
    }

    [Fact]
    public void UnchangedTextIsReturnedAsIs()
    {
        const string text = "این یک جمله درست است<br />خط دوم";

        Assert.Same(text, PersianFixer.Fix(text, RuleSet.Default));
    }

    [Fact]
    public void FindFixesOnlyChecksTheGivenLines()
    {
        var texts = new[] { "شدني", "شدني", "درست" };

        var fixes = PersianFixer.FindFixes(texts, new[] { 1, 2 }, RuleSet.Default);

        var fix = Assert.Single(fixes);
        Assert.Equal(1, fix.Index);
        Assert.Equal("شدنی", fix.After);
    }
}
