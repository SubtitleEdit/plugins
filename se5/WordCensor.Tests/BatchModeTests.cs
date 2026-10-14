using SubtitleEdit.Plugins.Shared;
using SubtitleEdit.Plugins.WordCensor;
using System.Text.Json;

namespace WordCensor.Tests;

/// <summary>Subtitle Edit Batch convert: "batch" censors every line headless with the saved settings.</summary>
public class BatchModeTests
{
    private static PluginRequest Request(string? settingsJson, params string[] texts) => new()
    {
        RequestType = PluginRequestType.Batch,
        Subtitle = new PluginSubtitle
        {
            Format = "SubRip",
            SubRip = SubRipParser.Serialize(texts.Select((t, i) => new SrtBlock
            {
                StartMs = i * 1000,
                EndMs = i * 1000 + 900,
                Text = t,
            }).ToList()),
        },
        Settings = settingsJson == null ? null : JsonDocument.Parse(settingsJson).RootElement.Clone(),
    };

    private static List<string> Texts(PluginResponse response) =>
        SubRipParser.Parse(response.Subtitle!.Native).Select(b => b.Text).ToList();

    [Fact]
    public void Batch_DefaultSettings_GrawlixOnAllLines()
    {
        var response = BatchMode.BuildBatchResponse(Request(null, "Oh shit.", "Plain line", "Damn it"));

        Assert.Equal(PluginStatus.Ok, response.Status);
        Assert.Null(response.Settings);
        var texts = Texts(response);
        Assert.Equal(3, texts.Count);
        Assert.DoesNotContain("shit", texts[0]);
        Assert.EndsWith("it.", texts[0]);
        Assert.Equal("Plain line", texts[1]);
        Assert.DoesNotContain("Damn", texts[2]);
        Assert.EndsWith("mn it", texts[2]);
    }

    [Fact]
    public void Batch_NothingToCensor_ReturnsOkWithoutSubtitle()
    {
        var response = BatchMode.BuildBatchResponse(Request(null, "Hello there", "Nice day"));

        Assert.Equal(PluginStatus.Ok, response.Status);
        Assert.Null(response.Subtitle);
    }

    [Fact]
    public void Batch_UsesSavedReplacementAndColor()
    {
        var response = BatchMode.BuildBatchResponse(Request(
            "{\"mode\":\"Replacement\",\"replacement\":\"[beep]\",\"colorRed\":true}", "What the fuck?"));

        Assert.Equal(new[] { "What the <font color=\"#ff0000\">[beep]</font>?" }, Texts(response));
    }

    [Theory]
    [InlineData("Grawlix")]
    [InlineData("Alternative")]
    public void Batch_RandomModes_AreReproducible(string mode)
    {
        var settings = $"{{\"mode\":\"{mode}\"}}";
        var first = BatchMode.BuildBatchResponse(Request(settings, "Shit, shit, shit.", "Damn fuck damn"));
        var second = BatchMode.BuildBatchResponse(Request(settings, "Shit, shit, shit.", "Damn fuck damn"));

        Assert.Equal(Texts(first), Texts(second));
        Assert.NotEqual("Shit, shit, shit.", Texts(first)[0]);
    }

    [Fact]
    public void Settings_RoundTrip()
    {
        var options = new CensorOptions { Mode = CensorMode.Alternative, Replacement = "***", ColorRed = true };

        var loaded = CensorSettings.Load(CensorSettings.Build(options));

        Assert.Equal(CensorMode.Alternative, loaded.Mode);
        Assert.Equal("***", loaded.Replacement);
        Assert.True(loaded.ColorRed);
    }
}
