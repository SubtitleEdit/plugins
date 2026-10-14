using SubtitleEdit.Plugins.Shared;
using SubtitleEdit.Plugins.SplitDialogs;
using System.Text.Json;

namespace SplitDialogs.Tests;

/// <summary>Subtitle Edit Batch convert: "batch" splits every dialog headless, "configure" returns the options only.</summary>
public class BatchModeTests
{
    private const string DialogSrt =
        "1\n00:00:01,000 --> 00:00:04,000\n- Andro!\n- Ben, I've been oozed.\n\n" +
        "2\n00:00:05,000 --> 00:00:07,000\nPlain line.\n\n" +
        "3\n00:00:08,000 --> 00:00:10,000\n- How are you\n- Fine.\n";

    private static PluginRequest Request(string requestType, string srt, string? settingsJson = null) => new()
    {
        RequestType = requestType,
        Subtitle = new PluginSubtitle { Format = "SubRip", SubRip = srt, Native = srt },
        Settings = settingsJson == null ? null : JsonDocument.Parse(settingsJson).RootElement.Clone(),
        SettingsVersion = settingsJson == null ? null : 1,
    };

    private static List<string> Texts(PluginResponse response) =>
        new SrtCueDocument(response.Subtitle!.Native).Cues.Select(c => string.Join("\n", c.Lines)).ToList();

    [Fact]
    public void Batch_SplitsAllDialogs_WithDefaultSettings()
    {
        var response = BatchMode.BuildBatchResponse(Request(PluginRequestType.Batch, DialogSrt));

        Assert.Equal(PluginStatus.Ok, response.Status);
        Assert.Equal("SubRip", response.Subtitle!.Format);
        // Line 3 is not split by default: "How are you" does not end a sentence.
        Assert.Equal(new[] { "Andro!", "Ben, I've been oozed.", "Plain line.", "- How are you\n- Fine." }, Texts(response));
        Assert.Null(response.Settings);
        Assert.Null(response.SettingsVersion);
    }

    [Fact]
    public void Batch_UsesSavedSettings()
    {
        var response = BatchMode.BuildBatchResponse(
            Request(PluginRequestType.Batch, DialogSrt, "{\"requireSentenceEnding\":false,\"gapMs\":100}"));

        var cues = new SrtCueDocument(response.Subtitle!.Native).Cues;
        Assert.Equal(new[] { "Andro!", "Ben, I've been oozed.", "Plain line.", "How are you", "Fine." },
            cues.Select(c => string.Join("\n", c.Lines)));
        Assert.Equal(100, cues[4].StartMs - cues[3].EndMs);
        Assert.Equal(1000, cues[0].StartMs);
        Assert.Equal(10000, cues[4].EndMs);
    }

    [Fact]
    public void Batch_NoDialogs_ReturnsOkWithoutSubtitle()
    {
        var response = BatchMode.BuildBatchResponse(
            Request(PluginRequestType.Batch, "1\n00:00:01,000 --> 00:00:02,000\nHello.\n"));

        Assert.Equal(PluginStatus.Ok, response.Status);
        Assert.Null(response.Subtitle);
    }

    [Fact]
    public void Batch_Assa_KeepsNativeFormat()
    {
        const string assa =
            "[Script Info]\nScriptType: v4.00+\n\n[Events]\n" +
            "Format: Layer, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text\n" +
            "Dialogue: 0,0:00:01.00,0:00:04.00,Top,Bob,0,0,0,,- Hi!\\N- Hello.\n";
        var request = new PluginRequest
        {
            RequestType = PluginRequestType.Batch,
            Subtitle = new PluginSubtitle { Format = "Advanced Sub Station Alpha", Native = assa },
        };

        var response = BatchMode.BuildBatchResponse(request);

        Assert.Equal("Advanced Sub Station Alpha", response.Subtitle!.Format);
        var dialogues = response.Subtitle.Native.Split('\n').Where(l => l.StartsWith("Dialogue:")).ToList();
        Assert.Equal(2, dialogues.Count);
        Assert.All(dialogues, d => Assert.Contains(",Top,Bob,", d));
        Assert.EndsWith(",Hi!", dialogues[0]);
        Assert.EndsWith(",Hello.", dialogues[1]);
    }

    [Fact]
    public void Configure_ReturnsSettingsWithoutSubtitle()
    {
        var response = BatchMode.BuildConfigureResponse(new SplitSettings(false, 80));

        Assert.Equal(PluginStatus.Ok, response.Status);
        Assert.Null(response.Subtitle);
        Assert.Equal(1, response.SettingsVersion);
        Assert.False(response.Settings!.Value.GetProperty("requireSentenceEnding").GetBoolean());
        Assert.Equal(80, response.Settings.Value.GetProperty("gapMs").GetInt32());
    }

    [Fact]
    public void Settings_RoundTrip_AndDefaults()
    {
        var saved = new SplitSettings(false, 42);
        var request = new PluginRequest { Settings = saved.ToJson(), SettingsVersion = SplitSettings.Version };

        Assert.Equal(saved, SplitSettings.Load(request));
        Assert.Equal(SplitSettings.Default, SplitSettings.Load(new PluginRequest()));
        Assert.Equal(SplitSettings.Default, SplitSettings.Load(new PluginRequest { Settings = saved.ToJson(), SettingsVersion = 99 }));
    }
}
