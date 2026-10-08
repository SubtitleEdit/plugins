using SubtitleEdit.Plugins.ArteCheck;
using SubtitleEdit.Plugins.Shared;
namespace ArteCheck.Tests;
public class TwoLinePositionTests
{
    private static ArteOptions Options(bool doubleHeight=true,bool sdh=false)=>new(){DoubleHeight=doubleHeight,Profile=sdh?ArteProfile.All.First(p=>p.IsSdh):ArteProfile.All[0],EnabledChecks=new(){ArteCheckType.TeletextLinePosition}};
    private static PluginParagraph P(string text,string row,double start)=>new(){Text=text,MarginV=row,StartMs=start,EndMs=start+3000,Style="style",Actor="actor",Extra="extra"};
    // Correctly positioned anchor avoids activating the independent whole-file shift rule.
    [Theory]
    [InlineData(19,false)][InlineData(20,false)][InlineData(21,true)][InlineData(22,true)][InlineData(23,true)]
    public void TwoLinesOnlyInvalidBottomRowsChange(int row,bool changes)
    {
        var text="{\\an1}<box><font color='Yellow'><i>Erste Zeile\nZweite Zeile</i></font></box>";
        var rows=new[]{P("Anker","22",0),P(text,row.ToString(),4000)};var o=Options();
        var fixes=ArteChecker.Analyze(rows,null,o).Where(f=>f.Kind==ArteFixKind.TeletextLinePosition&&f.Index==2).ToArray();
        Assert.Equal(changes?1:0,fixes.Length);
        if(!changes)return;
        Assert.Equal("20",fixes[0].After);var result=ArteChecker.Apply(rows,null,fixes,o);
        var p=result.Paragraphs[1];Assert.Equal("20",p.MarginV);Assert.Equal(text,p.Text);
        Assert.Equal(rows[1].StartMs,p.StartMs);Assert.Equal(rows[1].EndMs,p.EndMs);
        Assert.Equal(rows[1].Style,p.Style);Assert.Equal(rows[1].Actor,p.Actor);Assert.Equal(rows[1].Extra,p.Extra);
        Assert.Null(result.Header);Assert.Equal(rows[0].MarginV,result.Paragraphs[0].MarginV);
        Assert.DoesNotContain(ArteChecker.Analyze(result.Paragraphs,null,o),f=>f.Kind==ArteFixKind.TeletextLinePosition&&f.Index==2);
    }
    [Theory][InlineData(21,false)][InlineData(22,false)][InlineData(23,true)]
    public void SingleLineKeepsExistingPolicy(int row,bool changes)
    {
        var rows=new[]{P("Anker","22",0),P("Text",row.ToString(),4000)};
        var fixes=ArteChecker.Analyze(rows,null,Options()).Where(f=>f.Kind==ArteFixKind.TeletextLinePosition&&f.Index==2).ToArray();
        Assert.Equal(changes?1:0,fixes.Length);if(changes)Assert.Equal("22",fixes[0].After);
    }
    [Theory][InlineData(true,false)][InlineData(false,false)][InlineData(true,true)][InlineData(false,true)]
    public void SamePositionPathForSdhAndDoubleHeight(bool doubleHeight,bool sdh)
    {
        var rows=new[]{P("Anker","22",0),P("<font color='Cyan'>A\nB</font>","21",4000)};
        var f=Assert.Single(ArteChecker.Analyze(rows,null,Options(doubleHeight,sdh)),f=>f.Kind==ArteFixKind.TeletextLinePosition&&f.Index==2);
        Assert.Equal("20",f.After);
    }
    [Fact] public void IndependentWholeFileShiftRuleRemains()
    {
        var f=Assert.Single(ArteChecker.Analyze(new[]{P("A\nB","19",0)},null,Options()),f=>f.Kind==ArteFixKind.TeletextLinePosition);
        Assert.Equal("20",f.After);Assert.Contains("whole file",f.Reason);
    }
    [Fact] public void Mnr11HeaderMovesBottomRowsTo23RowPage()
    {
        var header=GsiHeader.CreateDefault();header.MaxRows="11";
        var rows=new[]{P("Eins","11",0),P("Eins\nZwei","10",4000),P("Titel","1",8000),P("Mitte","6",12000)};var o=Options();
        var fixes=ArteChecker.Analyze(rows,header.ToString(),o);
        Assert.Equal("23",GsiHeader.Parse(Assert.Single(fixes,f=>f.Kind==ArteFixKind.Header).ProposedHeader!).MaxRows);
        var positions=fixes.Where(f=>f.Kind==ArteFixKind.TeletextLinePosition).ToArray();
        Assert.Equal(new[]{1,2},positions.Select(f=>f.Index));
        var result=ArteChecker.Apply(rows,header.ToString(),fixes,o);
        Assert.Equal(new[]{"22","20","1","6"},result.Paragraphs.Select(p=>p.MarginV));
    }
    [Fact] public void Mnr23HeaderKeepsRow11()
    {
        var rows=new[]{P("Anker","22",0),P("Eins","11",4000)};
        Assert.DoesNotContain(ArteChecker.Analyze(rows,GsiHeader.CreateDefault().ToString(),Options()),f=>f.Kind==ArteFixKind.TeletextLinePosition);
    }
}
