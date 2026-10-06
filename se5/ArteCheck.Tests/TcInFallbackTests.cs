using SubtitleEdit.Plugins.ArteCheck;
using SubtitleEdit.Plugins.Shared;
namespace ArteCheck.Tests;
public class TcInFallbackTests
{
    private static ArteOptions Options(bool accept = true) => new() { AcceptShortDurations=accept, EnabledChecks=new(){ArteCheckType.DisplayDuration} };
    private static PluginParagraph P(double start,double end,string text="A")=>new(){StartMs=start,EndMs=end,Text=text,MarginV="20",Style="style",Actor="actor",Extra="extra"};
    private static PluginParagraph[] Rows(int before,int after,int duration=12)=>new[]{P(0,4000-before*40),P(4000,4000+duration*40,"{\\an1}<font color='Yellow'><i>A</i></font>"),P(4000+(duration+after)*40,9000)};
    private static ArteFix Fix(PluginParagraph[] rows,ArteOptions o,int index=2)=>Assert.Single(ArteChecker.Analyze(rows,null,o),f=>f.Kind==ArteFixKind.DisplayDuration&&f.Index==index);
    [Theory]
    [InlineData(16,16,true,false)][InlineData(16,11,true,false)][InlineData(16,10,true,true)]
    [InlineData(16,5,true,true)][InlineData(11,5,true,true)][InlineData(10,5,false,false)]
    [InlineData(9,7,false,false)][InlineData(11,11,true,false)]
    public void WholeSideOnlyAndInclusiveGap(int front,int back,bool possible,bool useStart)
    {
        var rows=Rows(front,back);var o=Options();var f=Fix(rows,o);Assert.Equal(possible,f.CanBeFixed);
        if(!possible){Assert.Null(f.ProposedStartMs);Assert.Null(f.ProposedEndMs);return;}
        Assert.Equal(useStart?3760:null,f.ProposedStartMs);Assert.Equal(useStart?null:4720,f.ProposedEndMs);
        // Duration proposals remain optional; explicitly select the tested correction.
        f.Apply=true;var result=ArteChecker.Apply(rows,null,new[]{f},o).Paragraphs;
        Assert.Equal(useStart?3760:4000,result[1].StartMs);Assert.Equal(useStart?4480:4720,result[1].EndMs);
        Assert.Equal(720,result[1].EndMs-result[1].StartMs);
        foreach(var i in new[]{0,2}){Assert.Equal(rows[i].StartMs,result[i].StartMs);Assert.Equal(rows[i].EndMs,result[i].EndMs);}
        Assert.Equal(rows[1].Text,result[1].Text);Assert.Equal(rows[1].MarginV,result[1].MarginV);
        Assert.Equal(rows[1].Style,result[1].Style);Assert.Equal(rows[1].Actor,result[1].Actor);Assert.Equal(rows[1].Extra,result[1].Extra);
        Assert.DoesNotContain(ArteChecker.Analyze(result,null,o),x=>x.Kind==ArteFixKind.DisplayDuration&&x.Index==2);
    }
    [Fact] public void FirstCannotGoNegative()
    {
        var rows=new[]{P(0,480),P(680,4000)};var f=Fix(rows,Options(),1);Assert.False(f.CanBeFixed);Assert.Null(f.ProposedStartMs);
    }
    [Fact] public void FirstMayUseTimelineRoom()
    {
        var rows=new[]{P(400,880),P(1080,4000)};var f=Fix(rows,Options(),1);Assert.True(f.CanBeFixed);Assert.Equal(160,f.ProposedStartMs);
    }
    [Fact] public void LastPrefersOut()
    {
        var f=Fix(new[]{P(4000,4480)},Options(),1);Assert.True(f.CanBeFixed);Assert.Null(f.ProposedStartMs);Assert.Equal(4720,f.ProposedEndMs);
    }
    [Theory][InlineData(17,true)][InlineData(18,false)]
    public void AcceptShortBoundary(int frames,bool correction)
    {
        var rows=Rows(16,16,frames);Assert.Equal(correction,ArteChecker.Analyze(rows,null,Options()).Any(f=>f.Kind==ArteFixKind.DisplayDuration&&f.Index==2));
    }
    [Fact] public void StrictPolicyStillTargetsReadingMinimum()
    {
        var rows=Rows(25,5,20);var f=Fix(rows,Options(false));Assert.True(f.CanBeFixed);Assert.Equal(3800,f.ProposedStartMs);Assert.Null(f.ProposedEndMs);
    }
    [Fact] public void OffGridEndUsesSafeCompleteFramesAndStableRecheck()
    {
        var rows=Rows(16,5);rows[1].EndMs+=1;var o=Options();var f=Fix(rows,o);Assert.Equal(3760,f.ProposedStartMs);
        f.Apply=true;var result=ArteChecker.Apply(rows,null,new[]{f},o).Paragraphs;Assert.Equal(4481,result[1].EndMs);
        Assert.DoesNotContain(ArteChecker.Analyze(result,null,o),x=>x.Kind==ArteFixKind.DisplayDuration&&x.Index==2);
    }
    [Fact] public void SourceFrameRateUsesExistingConversionBeforeFallback()
    {
        var rows=Rows(16,5);foreach(var p in rows){p.StartMs/=1.2;p.EndMs/=1.2;}
        var o=Options();o.SourceFrameRate=30;var f=Fix(rows,o);Assert.Equal(3760,f.ProposedStartMs);
        f.Apply=true;var result=ArteChecker.Apply(rows,null,new[]{f},o).Paragraphs;
        Assert.Equal(3760,result[1].StartMs);Assert.Equal(4480,result[1].EndMs);
        o.SourceFrameRate=25;Assert.DoesNotContain(ArteChecker.Analyze(result,null,o),x=>x.Kind==ArteFixKind.DisplayDuration&&x.Index==2);
    }
    [Fact] public void MaximumDurationStillOnlyShortensOut()
    {
        var f=Fix(new[]{P(0,9000)},Options(),1);Assert.Null(f.ProposedStartMs);Assert.Equal(8000,f.ProposedEndMs);
    }
}
