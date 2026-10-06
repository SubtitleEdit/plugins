using SubtitleEdit.Plugins.ArteCheck;
using SubtitleEdit.Plugins.Shared;

namespace ArteCheck.Tests;

public class DialogSplitTests
{
    private const double Start = 36_000_000;
    private const string Benedict = "-Sollen wir einfach Eier Benedict essen?\n-Ja, liebend gern.";
    private const string Herein = "-und warten darauf, dass sie jemand macht.\n-Herein.";

    private static ArteOptions Options(bool acceptShort = true) => new()
    {
        SourceFrameRate = 25, TeletextMaxCells = 37, MinimumGapFrames = 5,
        ReadingDurationTolerancePercent = 15, ShortMinimumFrames = 18,
        AcceptShortDurations = acceptShort,
        EnabledChecks = new()
        {
            ArteCheckType.TeletextLineLength, ArteCheckType.MaximumTwoLines,
            ArteCheckType.DisplayDuration, ArteCheckType.MinimumGaps,
            ArteCheckType.FrameAccurateTimeCodes, ArteCheckType.TeletextLinePosition,
        },
    };

    private static string Header()
    {
        var h = GsiHeader.CreateDefault();
        h.CodePage = "850"; h.DiskFormatCode = "STL25.01";
        h.DisplayStandardCode = "2"; h.LanguageCode = "08";
        return h.ToString();
    }

    private static PluginParagraph Paragraph(string text, int frames) => new()
    {
        Text = text, StartMs = Start, EndMs = Start + frames * 40, MarginV = "20",
    };

    [Theory]
    [InlineData(Benedict, 78, "-Sollen wir einfach\nEier Benedict essen?", "-Ja, liebend gern.")]
    [InlineData(Herein, 60, "-und warten darauf,\ndass sie jemand macht.", "-Herein.")]
    public void RealDialog_OffersValidSplitAndStableRecheck(string text, int frames, string first, string second)
    {
        var options = Options();
        var p = Paragraph(text, frames);
        p.Style = "Original style"; p.Actor = "Actor"; p.Extra = "Metadata";
        var previous = new PluginParagraph { Text = "Vorher.", StartMs = Start - 3000, EndMs = Start - 200, MarginV = "22" };
        var next = new PluginParagraph { Text = "Danach.", StartMs = p.EndMs + 200, EndMs = p.EndMs + 3200, MarginV = "22" };
        var source = new[] { previous, p, next };
        var fix = Assert.Single(ArteChecker.Analyze(source, Header(), options), f => f.Group == ArteChecker.GroupLayout);
        Assert.True(fix.CanBeFixed, fix.Reason);
        Assert.Equal(ArteFixKind.Split, fix.Kind);
        Assert.Equal(new[] { first, second }, fix.SplitParagraphs!.Select(x => x.Text));
        Assert.All(fix.SplitParagraphs!, part =>
        {
            Assert.True(TeletextText.Fits(part.Text, 37));
            Assert.True(part.EndMs - part.StartMs >= 18 * 40);
            Assert.Equal(0, part.StartMs % 40); Assert.Equal(0, part.EndMs % 40);
        });
        Assert.Equal(p.StartMs, fix.SplitParagraphs![0].StartMs);
        Assert.Equal(p.EndMs, fix.SplitParagraphs![1].EndMs);
        Assert.True(fix.SplitParagraphs![1].StartMs - fix.SplitParagraphs![0].EndMs >= 5 * 40);

        var result = ArteChecker.Apply(source, Header(), new[] { fix }, options);
        Assert.Equal(Header(), result.Header);
        Assert.Equal(4, result.Paragraphs.Count);
        Assert.Equal(new[] { "20", "22" }, result.Paragraphs.Skip(1).Take(2).Select(x => x.MarginV));
        Assert.All(result.Paragraphs.Skip(1).Take(2), x =>
        {
            Assert.Equal(p.Style, x.Style); Assert.Equal(p.Actor, x.Actor); Assert.Equal(p.Extra, x.Extra);
        });
        Assert.Equal(previous.Text, result.Paragraphs[0].Text);
        Assert.Equal(previous.StartMs, result.Paragraphs[0].StartMs); Assert.Equal(previous.EndMs, result.Paragraphs[0].EndMs);
        Assert.Equal(next.Text, result.Paragraphs[3].Text);
        Assert.Equal(next.StartMs, result.Paragraphs[3].StartMs); Assert.Equal(next.EndMs, result.Paragraphs[3].EndMs);
        Assert.Equal(text, p.Text); Assert.Equal(Start + frames * 40, p.EndMs);
        Assert.Empty(ArteChecker.Analyze(result.Paragraphs, result.Header, options));
    }

    [Fact]
    public void AlreadyValidDialog_IsNotSplit()
    {
        Assert.Empty(ArteChecker.Analyze(new[] { Paragraph("-Guten Abend.\n-Guten Abend.", 65) }, Header(), Options()));
    }

    [Fact]
    public void InsufficientTime_ReturnsTimingAlarm()
    {
        var fix = Assert.Single(ArteChecker.Analyze(new[] { Paragraph(Herein, 40) }, Header(), Options()), f => f.Group == ArteChecker.GroupLayout);
        Assert.True(fix.IsAlarm);
        Assert.Contains("Not enough time", fix.Reason);
        Assert.Null(fix.SplitParagraphs);
    }

    [Fact]
    public void AcceptShortOff_UsesExistingReadingMinimum()
    {
        var options = Options(false);
        var p = Paragraph(Herein, 60);
        var fix = Assert.Single(ArteChecker.Analyze(new[] { p }, Header(), options), f => f.Group == ArteChecker.GroupLayout);
        Assert.True(fix.IsAlarm); Assert.Contains("Not enough time", fix.Reason);
        p.EndMs = Start + 100 * 40;
        fix = Assert.Single(ArteChecker.Analyze(new[] { p }, Header(), options), f => f.Group == ArteChecker.GroupLayout);
        Assert.True(fix.CanBeFixed);
        var result = ArteChecker.Apply(new[] { p }, Header(), new[] { fix }, options);
        Assert.Empty(ArteChecker.Analyze(result.Paragraphs, result.Header, options));
    }

    [Fact]
    public void ItalicBeforeSpeaker_UsesExistingExplicitRemovalPolicy()
    {
        var text = "<i>-Sollen wir einfach Eier Benedict essen?</i>\n<i>-Ja, liebend gern.</i>";
        var fix = Assert.Single(ArteChecker.Analyze(new[] { Paragraph(text, 78) }, Header(), Options()), f => f.Group == ArteChecker.GroupLayout);
        Assert.True(fix.CanBeFixed); Assert.Contains("Removes the italic tags", fix.Reason);
        Assert.All(fix.SplitParagraphs!, part => Assert.DoesNotContain("<i>", part.Text));
        Assert.Equal(TeletextText.Words(text), TeletextText.Words(string.Join(" ", fix.SplitParagraphs!.Select(x => x.Text))));
    }

    [Fact]
    public void ColoredDialog_KeepsExistingManualAlarm()
    {
        var text = "<font color=\"Yellow\">" + Benedict + "</font>";
        var fix = Assert.Single(ArteChecker.Analyze(new[] { Paragraph(text, 78) }, Header(), Options()), f => f.Group == ArteChecker.GroupLayout);
        Assert.True(fix.IsAlarm); Assert.Contains("Colored text is not split", fix.Reason);
    }

    [Fact]
    public void LongUnbreakableWord_DoesNotOfferInvalidSplit()
    {
        var text = "-" + new string('a', 80) + "\n-Kurz.";
        var fix = Assert.Single(ArteChecker.Analyze(new[] { Paragraph(text, 150) }, Header(), Options()), f => f.Group == ArteChecker.GroupLayout);
        Assert.True(fix.IsAlarm); Assert.Null(fix.SplitParagraphs);
    }

    [Fact]
    public void AlignmentBeforeSpeaker_IsPreserved()
    {
        var text = "{\\an2}" + Benedict;
        var fix = Assert.Single(ArteChecker.Analyze(new[] { Paragraph(text, 78) }, Header(), Options()), f => f.Group == ArteChecker.GroupLayout);
        Assert.True(fix.CanBeFixed);
        Assert.StartsWith("{\\an2}-Sollen", fix.SplitParagraphs![0].Text);
        Assert.Equal("-Ja, liebend gern.", fix.SplitParagraphs![1].Text);
    }

    [Fact]
    public void FormattingAcrossSpeakers_IsLeftForManualEditing()
    {
        var text = "<u>" + Benedict + "</u>";
        var fix = Assert.Single(ArteChecker.Analyze(new[] { Paragraph(text, 78) }, Header(), Options()), f => f.Group == ArteChecker.GroupLayout);
        Assert.True(fix.IsAlarm); Assert.Contains("Formatted text", fix.Reason);
        Assert.Null(fix.SplitParagraphs); Assert.Equal(text, fix.Before);
    }

    [Fact]
    public void EnDash_PreservesExistingNonDialogSemantics()
    {
        var text = Benedict.Replace('-', '–');
        var fix = Assert.Single(ArteChecker.Analyze(new[] { Paragraph(text, 78) }, Header(), Options()), f => f.Group == ArteChecker.GroupLayout);
        Assert.Equal(ArteFixKind.Rebalance, fix.Kind);
    }
}
