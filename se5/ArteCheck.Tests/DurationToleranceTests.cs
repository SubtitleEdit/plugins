using SubtitleEdit.Plugins.ArteCheck;
using SubtitleEdit.Plugins.Shared;

namespace ArteCheck.Tests;

/// <summary>25 fps, 25-frame minimum, 15% tolerance: 21.25 frames is shown as 21, so 21 frames is accepted.</summary>
public class DurationToleranceTests
{
    private const double Hour10 = 10 * 3_600_000.0;

    private static List<ArteFix> Analyze(int frames) => ArteChecker.Analyze(
        new[] { new PluginParagraph { StartMs = Hour10, EndMs = Hour10 + frames * ArteChecker.FrameMs, Text = "Hi", MarginV = "22" } },
        null,
        new ArteOptions { EnabledChecks = new() { ArteCheckType.DisplayDuration } });

    [Theory]
    [InlineData(20, true)]
    [InlineData(21, false)]
    [InlineData(22, false)]
    public void ToleratedMinimum_IsRoundedToWholeFrames(int frames, bool reported)
    {
        Assert.Equal(reported, Analyze(frames).Any(f => f.Kind == ArteFixKind.DisplayDuration));
    }

    [Fact]
    public void ToleratedMinimum_MessageShowsTheLimitTheCheckUses()
    {
        var fix = Assert.Single(Analyze(20), f => f.Kind == ArteFixKind.DisplayDuration);
        Assert.Contains("tolerated minimum of 0 s 21 fr", fix.Reason);
    }
}
