using SubtitleEdit.Plugins.ArteCheck;
using SubtitleEdit.Plugins.Shared;
namespace ArteCheck.Tests;
public class ColorDecisionTests
{
    private static ArteOptions Options(bool length = false) => new() { EnabledChecks = length ? new() { ArteCheckType.TeletextColors, ArteCheckType.TeletextLineLength } : new() { ArteCheckType.TeletextColors } };
    private static PluginParagraph[] Rows(params string[] texts) => texts.Select((t,i) => new PluginParagraph { Text=t, StartMs=i*4000, EndMs=i*4000+3000 }).ToArray();
    private static string Y(string t) => "<font color=\"Yellow\">"+t+"</font>";
    private static List<ArteFix> Colors(PluginParagraph[] rows, ArteOptions o) => ArteChecker.Analyze(rows,null,o).Where(f=>f.Kind==ArteFixKind.TeletextColor).ToList();
    [Theory]
    [InlineData("<font color='Yellow'>Text</font>")]
    [InlineData("{\\an1}<font color=\"Yellow\"><i>Text</i></font>")]
    [InlineData("{\\an2}<font color=\"#ffff00\">Text\nZeile</font>")]
    [InlineData("<box><font color=\"Yellow\"><b>Text</b></font></box>")]
    public void YellowNoOp(string text)
    {
        var rows=Rows(text); var o=Options(); var fixes=Colors(rows,o); Assert.Empty(fixes);
        Assert.Equal(text,Assert.Single(ArteChecker.Apply(rows,null,fixes,o).Paragraphs).Text);
    }
    [Theory]
    [InlineData("Text")][InlineData("{\\an1}<i>Text</i>")][InlineData("{\\an2}Text")][InlineData("<box>Text</box>")]
    public void PlainNoOp(string text)
    {
        var rows=Rows(text); var o=Options(); var fixes=Colors(rows,o); Assert.Empty(fixes);
        Assert.Equal(text,Assert.Single(ArteChecker.Apply(rows,null,fixes,o).Paragraphs).Text);
    }
    [Theory]
    [InlineData(2,1,1)][InlineData(1,2,1)][InlineData(1,1,1)][InlineData(0,0,0)]
    public void Majority(int yellow,int plain,int changes)
    {
        var rows=Rows(Enumerable.Repeat(Y("Gelb"),yellow).Concat(Enumerable.Repeat("Text",plain)).ToArray());
        var o=Options(); var fixes=Colors(rows,o); Assert.Equal(changes,fixes.Count);
        var applied=ArteChecker.Apply(rows,null,fixes,o).Paragraphs.ToArray();
        Assert.Empty(Colors(applied,o));
        Assert.All(applied,p=>Assert.Equal(yellow>plain,TeletextText.HasFontColor(p.Text)));
    }
    [Theory][InlineData("{\\an1}")][InlineData("{\\an2}")][InlineData("<i> </i>")]
    public void InvisibleDoesNotVote(string blank) => Assert.Empty(Colors(Rows(Y("Text"),blank),Options()));
    [Theory][InlineData("{\\an1}")][InlineData("{\\an2}")]
    public void AddYellowKeepsAlignment(string alignment)
    {
        var rows=Rows(Y("A"),Y("B"),alignment+"<i>Text\nZeile</i>"); var o=Options();
        var f=Assert.Single(Colors(rows,o)); Assert.Equal(alignment+Y("<i>Text\nZeile</i>"),f.After);
        Assert.Empty(Colors(ArteChecker.Apply(rows,null,new[]{f},o).Paragraphs.ToArray(),o));
    }
    [Fact] public void RemoveYellowKeepsStructure()
    {
        var rows=Rows("{\\an1}"+Y("<box><i>A\nB</i></box>"),"C","D");
        Assert.Equal("{\\an1}<box><i>A\nB</i></box>",Assert.Single(Colors(rows,Options())).After);
    }
    [Fact] public void OtherColorsDoNotVote()
    {
        var f=Colors(Rows(Y("A"),"B","<font color=\"Red\">C</font>"),Options());
        Assert.Equal(2,f.Count); Assert.All(f,x=>Assert.False(TeletextText.HasFontColor(x.After)));
    }
    [Theory][InlineData(1)][InlineData(100)]
    public void ThirtySevenOverridesWholeFile(int count)
    {
        var rows=Rows(Enumerable.Range(0,count).Select(i=>Y(i==0?new string('x',37):"Text")).ToArray()); var o=Options(true);
        var fixes=Colors(rows,o); Assert.Equal(count,fixes.Count);
        Assert.DoesNotContain(ArteChecker.Analyze(rows,null,o),f=>f.Group==ArteChecker.GroupLayout);
        var applied=ArteChecker.Apply(rows,null,fixes,o).Paragraphs.ToArray(); Assert.Empty(Colors(applied,o));
        Assert.DoesNotContain(ArteChecker.Analyze(applied,null,o),f=>f.Group==ArteChecker.GroupLayout);
    }
    [Fact] public void ThirtyEightIsStillLengthIssue()
    {
        var rows=Rows(Y(new string('x',38))); var o=Options(true); Assert.Empty(Colors(rows,o));
        Assert.Contains(ArteChecker.Analyze(rows,null,o),f=>f.Group==ArteChecker.GroupLayout);
    }
    [Fact] public void SdhStandardColorsStayExact()
    {
        var o=Options(); o.Profile=ArteProfile.All.First(p=>p.IsSdh);
        Assert.Empty(Colors(Rows("{\\an1}<font color='Red'>A</font>","<font color=\"#ffff00\">B</font>"),o));
    }
    [Fact] public void SdhNonStandardStillMapsAndRechecks()
    {
        var o=Options(); o.Profile=ArteProfile.All.First(p=>p.IsSdh);
        var rows=Rows("<font color=\"#10ee10\">A</font>");
        var fix=Assert.Single(Colors(rows,o)); Assert.Equal("<font color=\"Green\">A</font>",fix.After);
        Assert.Empty(Colors(ArteChecker.Apply(rows,null,new[]{fix},o).Paragraphs.ToArray(),o));
    }
    [Fact] public void ForegroundRemovalPreservesOtherFontAttributesAndNestedTags()
    {
        var rows=Rows("{\\an2}<font face='Arial' color='Yellow'><font size='20'><i>A</i></font></font>","B","C");
        var fix=Assert.Single(Colors(rows,Options()));
        Assert.Equal("{\\an2}<font face='Arial' ><font size='20'><i>A</i></font></font>",fix.After);
    }
    [Fact] public void EmptyColoredTagsAndMixedRunsDoNotVote()
    {
        var rows=Rows("<font color='Yellow'></font>","{\\an1}"," ");
        Assert.Empty(Colors(rows,Options()));
        Assert.Equal("Other",TeletextText.EffectiveForeground("<font color='Yellow'>A</font>B"));
    }
    [Fact] public void ThirtySevenPlainInYellowMajorityAlsoOverrides()
    {
        var rows=Rows(Y("A"),Y("B"),new string('x',37)); var o=Options(true);
        var fixes=Colors(rows,o); Assert.Equal(2,fixes.Count);
        Assert.Empty(Colors(ArteChecker.Apply(rows,null,fixes,o).Paragraphs.ToArray(),o));
    }
}
