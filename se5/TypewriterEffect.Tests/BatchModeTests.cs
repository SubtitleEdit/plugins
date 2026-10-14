using SubtitleEdit.Plugins.TypewriterEffect;
using System.Text.Json;

namespace TypewriterEffect.Tests;

/// <summary>Subtitle Edit Batch convert: "batch" applies the effect to every line headless, "configure" returns the end delay only.</summary>
public class BatchModeTests
{
    private const string TwoLines = "1\n00:00:01,000 --> 00:00:03,000\nHi\n\n2\n00:00:04,000 --> 00:00:06,000\nYo\n";

    private static PluginRequest Request(string requestType, string srt, string? settingsJson = null, params int[] selected) => new()
    {
        RequestType = requestType,
        Subtitle = new PluginSubtitle { Format = "SubRip", SubRip = srt },
        SelectedIndices = selected.ToList(),
        Settings = settingsJson == null ? null : JsonDocument.Parse(settingsJson).RootElement.Clone(),
    };

    [Fact]
    public void Batch_AppliesToAllLinesWithDefaultEndDelay()
    {
        var response = TypewriterResponses.BuildBatchResponse(Request("batch", TwoLines));

        Assert.Equal("ok", response.Status);
        Assert.Null(response.Settings);
        Assert.Null(response.SettingsVersion);
        var blocks = SubRipParser.Parse(response.Subtitle!.Native);
        // Each line: "H", "Hi" + a 0.5 s end-delay hold of the full line.
        Assert.Equal(new[] { "H", "Hi", "Hi", "Y", "Yo", "Yo" }, blocks.Select(b => b.Text));
        Assert.Equal(2500, blocks[2].StartMs);
        Assert.Equal(3000, blocks[2].EndMs);
    }

    [Fact]
    public void Batch_UsesSavedEndDelay()
    {
        var response = TypewriterResponses.BuildBatchResponse(Request("batch", TwoLines, "{\"endDelay\":0}"));

        var blocks = SubRipParser.Parse(response.Subtitle!.Native);
        Assert.Equal(new[] { "H", "Hi", "Y", "Yo" }, blocks.Select(b => b.Text));
        Assert.Equal(3000, blocks[1].EndMs);
    }

    [Fact]
    public void Batch_NothingToChange_ReturnsOkWithoutSubtitle()
    {
        // Lines of 500 ms or less are too short to split.
        var srt = "1\n00:00:01,000 --> 00:00:01,400\nHi\n";
        var response = TypewriterResponses.BuildBatchResponse(Request("batch", srt));

        Assert.Equal("ok", response.Status);
        Assert.Null(response.Subtitle);
    }

    [Fact]
    public void Run_StillHonoursSelectionAndReturnsSettings()
    {
        var response = TypewriterResponses.BuildRunResponse(Request("run", TwoLines, null, 1), 0.5);

        Assert.Equal("ok", response.Status);
        var blocks = SubRipParser.Parse(response.Subtitle!.Native);
        Assert.Equal(new[] { "Hi", "Y", "Yo", "Yo" }, blocks.Select(b => b.Text));
        Assert.Equal(0.5, response.Settings!.Value.GetProperty("endDelay").GetDouble());
    }

    [Fact]
    public void Configure_ReturnsSettingsWithoutSubtitle()
    {
        Assert.True(TypewriterResponses.IsConfigure(Request("configure", string.Empty)));

        var response = TypewriterResponses.BuildConfigureResponse(1.5);

        Assert.Equal("ok", response.Status);
        Assert.Null(response.Subtitle);
        Assert.Equal(1.5, response.Settings!.Value.GetProperty("endDelay").GetDouble());
    }
}
