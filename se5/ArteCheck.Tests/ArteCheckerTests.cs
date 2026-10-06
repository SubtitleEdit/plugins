using System.Text.RegularExpressions;
using Projektanker.Icons.Avalonia.MaterialDesign;
using SubtitleEdit.Plugins.ArteCheck;
using SubtitleEdit.Plugins.Shared;

namespace ArteCheck.Tests;

public class ArteCheckerTests
{
    private const double Hour10 = 10 * 3_600_000.0;

    private static PluginParagraph P(double startMs, double endMs, string text, string? row = "22") =>
        new() { StartMs = startMs, EndMs = endMs, Text = text, MarginV = row };

    private static ArteOptions Options(params ArteCheckType[] checks) => new()
    {
        EnabledChecks = checks.Length == 0 ? Enum.GetValues<ArteCheckType>().ToHashSet() : checks.ToHashSet(),
    };

    private static string ArteHeader()
    {
        var header = GsiHeader.CreateDefault();
        header.CodePage = "850";
        header.DiskFormatCode = "STL25.01";
        header.DisplayStandardCode = "2";
        header.LanguageCode = "08";
        return header.ToString();
    }

    [Fact]
    public void GsiHeader_DefaultIs1024AndRoundTrips()
    {
        var header = GsiHeader.CreateDefault();
        Assert.Equal(1024, header.ToString().Length);
        Assert.True(GsiHeader.IsStlHeader(header.ToString()));
        header.LanguageCode = "2D";
        header.TimeCodeStartOfProgramme = "10000000";
        var parsed = GsiHeader.Parse(header.ToString());
        Assert.Equal("2D", parsed.LanguageCode);
        Assert.Equal("10000000", parsed.TimeCodeStartOfProgramme);
        Assert.Equal("STL25.01", parsed.DiskFormatCode);
    }

    [Fact]
    public void Header_ProposesArteValues()
    {
        var fixes = ArteChecker.Analyze(new[] { P(Hour10, Hour10 + 200, "") }, GsiHeader.CreateDefault().ToString(), Options());
        var fix = Assert.Single(fixes, f => f.Kind == ArteFixKind.Header);
        var header = GsiHeader.Parse(fix.ProposedHeader!);
        Assert.Equal("850", header.CodePage);
        Assert.Equal("2", header.DisplayStandardCode);
        Assert.Equal("08", header.LanguageCode);
    }

    [Fact]
    public void BlankSubtitle_MissingIsInsertedAtTheHour()
    {
        var source = new[] { P(Hour10 + 5000, Hour10 + 8000, "Hallo") };
        var fixes = ArteChecker.Analyze(source, ArteHeader(), Options(ArteCheckType.BlankSubtitle));
        var fix = Assert.Single(fixes);
        Assert.Equal(ArteFixKind.CreateBlankSubtitle, fix.Kind);

        var result = ArteChecker.Apply(source, ArteHeader(), fixes, Options());
        Assert.Equal(2, result.Paragraphs.Count);
        Assert.Equal(Hour10, result.Paragraphs[0].StartMs);
        Assert.Equal(Hour10 + 200, result.Paragraphs[0].EndMs);
        Assert.Equal(string.Empty, result.Paragraphs[0].Text);
    }

    [Fact]
    public void FrameRateConversion_KeepsTheProgrammeStart()
    {
        var source = new List<PluginParagraph> { P(Hour10, Hour10 + 200, ""), P(Hour10 + 60_000, Hour10 + 62_000, "Hallo") };
        var converted = ArteChecker.ConvertFrameRate(source.Select(p => p.Clone()).ToList(), 23.976);
        Assert.Equal(Hour10, converted[0].StartMs);
        Assert.Equal(Hour10 + 60_000 * (24000.0 / 1001.0) / 25.0, converted[1].StartMs, 3);
    }

    [Fact]
    public void FrameAccurate_RoundsToWholeFrames()
    {
        var source = new[] { P(Hour10 + 1010, Hour10 + 3030, "Hallo") };
        var fix = Assert.Single(ArteChecker.Analyze(source, ArteHeader(), Options(ArteCheckType.FrameAccurateTimeCodes)));
        Assert.Equal(Hour10 + 1000, fix.ProposedStartMs);
        Assert.Equal(Hour10 + 3040, fix.ProposedEndMs);
    }

    [Fact]
    public void MinimumGap_SharedBetweenOutAndInTime()
    {
        var source = new[] { P(Hour10, Hour10 + 4000, "Eins"), P(Hour10 + 4000, Hour10 + 8000, "Zwei") };
        var fix = Assert.Single(ArteChecker.Analyze(source, ArteHeader(), Options(ArteCheckType.MinimumGaps)));
        Assert.True(fix.CanBeFixed);
        var result = ArteChecker.Apply(source, ArteHeader(), new[] { fix }, Options());
        Assert.Equal(Hour10 + 4000 - 2 * 40, result.Paragraphs[0].EndMs);
        Assert.Equal(Hour10 + 4000 + 3 * 40, result.Paragraphs[1].StartMs);
    }

    [Fact]
    public void MinimumGap_NoRoomIsAnAlarm()
    {
        var source = new[] { P(Hour10, Hour10 + 900, "Eins zwei drei vier"), P(Hour10 + 900, Hour10 + 1800, "Fünf sechs sieben acht") };
        var fix = Assert.Single(ArteChecker.Analyze(source, ArteHeader(), Options(ArteCheckType.MinimumGaps)));
        Assert.True(fix.IsAlarm);
    }

    [Fact]
    public void LinePosition_OneLineOnRow23MovesTo22()
    {
        var source = new[] { P(Hour10, Hour10 + 3000, "Hallo", "23"), P(Hour10 + 4000, Hour10 + 7000, "Eins\nZwei", "22") };
        var fixes = ArteChecker.Analyze(source, ArteHeader(), Options(ArteCheckType.TeletextLinePosition));
        Assert.Equal(new[] { "22", "20" }, fixes.Select(f => f.After));
    }

    [Fact]
    public void Colors_OtherColorsDoNotOutvoteUncoloredSubtitles()
    {
        var source = new[] { P(Hour10, Hour10 + 3000, "<font color=\"#00ffff\">Hallo</font>"), P(Hour10 + 4000, Hour10 + 7000, "Welt") };
        var fixes = ArteChecker.Analyze(source, ArteHeader(), Options(ArteCheckType.TeletextColors));
        var fix = Assert.Single(fixes);
        Assert.Equal("Hallo", fix.After);
    }

    [Fact]
    public void Colors_SdhKeepsStandardTeletextColorExactly()
    {
        var options = Options(ArteCheckType.TeletextColors);
        options.Profile = ArteProfile.All.Single(p => p.Code == "HG-DEU");
        var source = new[] { P(Hour10, Hour10 + 3000, "<font color=\"#00ffff\">Hallo</font>") };
        Assert.DoesNotContain(ArteChecker.Analyze(source, ArteHeader(), options), f => f.Group == ArteChecker.GroupColors);
    }

    [Fact]
    public void LineLength_ColorCellCountsAgainstTheLimit()
    {
        var line = new string('a', 36);
        Assert.True(TeletextText.Fits(line + "b", 37));
        Assert.False(TeletextText.Fits("<font color=\"Yellow\">" + line + "b</font>", 37));
        Assert.True(TeletextText.Fits("<font color=\"Yellow\">" + line + "</font>", 37));
    }

    [Fact]
    public void LineLength_TooLongIsRebalanced()
    {
        var source = new[] { P(Hour10, Hour10 + 5000, "Das ist eine ziemlich lange Zeile, die nicht passt", "22") };
        var fix = Assert.Single(ArteChecker.Analyze(source, ArteHeader(), Options(ArteCheckType.TeletextLineLength)));
        Assert.Equal(ArteFixKind.Rebalance, fix.Kind);
        Assert.Equal("Das ist eine ziemlich lange Zeile,\ndie nicht passt", fix.After);

        var result = ArteChecker.Apply(source, ArteHeader(), new[] { fix }, Options());
        Assert.Equal("20", result.Paragraphs[0].MarginV); // two double-height lines keep the bottom edge
    }

    [Fact]
    public void ThreeLines_SplitIntoTwoSubtitlesInsideTheTimeRange()
    {
        var text = "Wir müssen jetzt sofort gehen, sonst kommen wir zu spät.\nDer Zug wartet nicht auf uns.\nNiemand wartet auf uns.";
        var source = new[] { P(Hour10, Hour10 + 9000, text, "18") };
        var fix = Assert.Single(ArteChecker.Analyze(source, ArteHeader(), Options(ArteCheckType.MaximumTwoLines)));
        Assert.Equal(ArteFixKind.Split, fix.Kind);

        var result = ArteChecker.Apply(source, ArteHeader(), new[] { fix }, Options());
        Assert.True(result.Paragraphs.Count >= 2);
        Assert.Equal(Hour10, result.Paragraphs[0].StartMs);
        Assert.Equal(Hour10 + 9000, result.Paragraphs[^1].EndMs);
        Assert.All(result.Paragraphs, p => Assert.True(TeletextText.Fits(p.Text, 37)));
        for (var i = 1; i < result.Paragraphs.Count; i++)
        {
            Assert.True(result.Paragraphs[i].StartMs - result.Paragraphs[i - 1].EndMs >= 5 * 40);
        }

        Assert.Equal(TeletextText.Words(text), TeletextText.Words(string.Join(" ", result.Paragraphs.Select(p => p.Text))));
    }

    [Fact]
    public void Italic_IsRemoved()
    {
        var fix = Assert.Single(ArteChecker.Analyze(new[] { P(Hour10, Hour10 + 3000, "<i>Hallo</i>") }, ArteHeader(), Options(ArteCheckType.Italic)));
        Assert.Equal("Hallo", fix.After);
    }

    [Fact]
    public void ShiftStartTimeCode_MovesEverythingAndUpdatesTcp()
    {
        var source = new[] { P(3_600_000, 3_600_200, ""), P(3_605_000, 3_608_000, "Hallo") };
        var options = Options(ArteCheckType.BlankSubtitle);
        options.ShiftToStartTimeCode = true;
        options.TargetStartMs = Hour10;
        var fixes = ArteChecker.Analyze(source, ArteHeader(), options);
        var result = ArteChecker.Apply(source, ArteHeader(), fixes.Where(f => f.Kind == ArteFixKind.ShiftStartTimeCode).ToList(), options);
        Assert.Equal(Hour10, result.Paragraphs[0].StartMs);
        Assert.Equal(Hour10 + 5000, result.Paragraphs[1].StartMs);
        Assert.Equal("10000000", GsiHeader.Parse(result.Header!).TimeCodeStartOfProgramme);
    }

    [Fact]
    public void CleanFile_HasNoFindings()
    {
        var source = new[]
        {
            P(Hour10, Hour10 + 200, ""),
            P(Hour10 + 1000, Hour10 + 4000, "Guten Tag."),
            P(Hour10 + 4200, Hour10 + 8000, "Wie geht es Ihnen\nheute Abend?", "20"),
        };
        Assert.Empty(ArteChecker.Analyze(source, ArteHeader(), Options()));
    }

    [Fact]
    public void EveryIconNameExists()
    {
        var files = Directory.GetFiles(FindSourceFolder(), "*.*", SearchOption.TopDirectoryOnly)
            .Where(f => f.EndsWith(".cs") || f.EndsWith(".axaml"));
        var names = files.SelectMany(f => Regex.Matches(File.ReadAllText(f), "\"(mdi-[a-z0-9-]+)\"").Select(m => m.Groups[1].Value)).Distinct().ToList();
        Assert.NotEmpty(names);
        var provider = new MaterialDesignIconProvider();
        var getIcon = typeof(MaterialDesignIconProvider).GetMethods()
            .First(m => m.ReturnType != typeof(void) && m.GetParameters() is [{ ParameterType: var t }] && t == typeof(string));
        var missing = names.Where(n =>
        {
            try
            {
                getIcon.Invoke(provider, new object[] { n });
                return false;
            }
            catch
            {
                return true;
            }
        }).ToList();
        Assert.Empty(missing);
    }

    private static string FindSourceFolder()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "ArteCheck")))
        {
            dir = dir.Parent;
        }

        return Path.Combine(dir!.FullName, "ArteCheck");
    }
}
