using SubtitleEdit.Plugins.ArteCheck;
using SubtitleEdit.Plugins.Shared;
using System.Text.Json;

namespace ArteCheck.Tests;

/// <summary>Subtitle Edit Batch convert: "batch" applies every correction headless, "configure" returns settings only.</summary>
public class BatchModeTests
{
    private const double Hour10 = 10 * 3_600_000.0;

    private static PluginRequest MakeRequest(string requestType, string? settingsJson = null) => new()
    {
        RequestType = requestType,
        Subtitle = new PluginSubtitle
        {
            Format = "SubRip",
            Paragraphs = requestType == PluginRequestType.Configure
                ? new List<PluginParagraph>()
                : new List<PluginParagraph>
                {
                    new() { StartMs = Hour10 + 1000, EndMs = Hour10 + 4000, Text = "  Hello there.  " },
                },
        },
        Settings = settingsJson == null ? null : JsonDocument.Parse(settingsJson).RootElement.Clone(),
        SettingsVersion = settingsJson == null ? null : 1,
    };

    [Fact]
    public void Batch_AppliesCorrections()
    {
        var response = new MainViewModel(MakeRequest(PluginRequestType.Batch)).BuildBatchResponse();

        Assert.Equal(PluginStatus.Ok, response.Status);
        Assert.NotNull(response.Subtitle?.Paragraphs);
        Assert.Contains(response.Subtitle!.Paragraphs!, p => p.Text == "Hello there.");
    }

    [Fact]
    public void Batch_UsesFrameRateSavedByConfigure()
    {
        var vm = new MainViewModel(MakeRequest(PluginRequestType.Batch, "{\"batchFrameRate\":29.97}"));

        Assert.Equal(29.97, vm.SelectedFrameRate, 3);
    }

    [Fact]
    public void Run_IgnoresButKeepsBatchSettings()
    {
        var vm = new MainViewModel(MakeRequest(PluginRequestType.Run, "{\"batchFrameRate\":29.97,\"batchShiftToStartTimeCode\":true}"));

        Assert.Equal(25.0, vm.SelectedFrameRate, 3);
        Assert.False(vm.ShiftToStartTimeCode);
        var settings = vm.BuildCancelResponse().Settings!.Value;
        Assert.Equal(29.97, settings.GetProperty("batchFrameRate").GetDouble(), 3);
        Assert.True(settings.GetProperty("batchShiftToStartTimeCode").GetBoolean());
    }

    [Fact]
    public void Configure_ReturnsSettingsWithoutSubtitle()
    {
        var vm = new MainViewModel(MakeRequest(PluginRequestType.Configure))
        {
            SelectedFrameRate = 30.0,
            MinimumGapFrames = 4,
        };

        var response = vm.BuildResponse();

        Assert.Equal(PluginStatus.Ok, response.Status);
        Assert.Null(response.Subtitle);
        Assert.Equal(1, response.SettingsVersion);
        Assert.Equal(30.0, response.Settings!.Value.GetProperty("batchFrameRate").GetDouble(), 3);
        Assert.Equal(4, response.Settings.Value.GetProperty("minimumGapFrames").GetInt32());
    }

    [Fact]
    public void Bootstrap_BatchRequest_RunsHandlerWithoutUi()
    {
        var dir = Directory.CreateTempSubdirectory("arte-batch-test");
        try
        {
            var request = MakeRequest(PluginRequestType.Batch);
            request.ResponseFilePath = Path.Combine(dir.FullName, "response.json");
            var requestPath = Path.Combine(dir.FullName, "request.json");
            File.WriteAllText(requestPath, JsonSerializer.Serialize(request, PluginBootstrap.JsonOpts));

            var exitCode = PluginBootstrap.Run<App>(new[] { requestPath }, r => new MainViewModel(r).BuildBatchResponse());

            Assert.Equal(0, exitCode);
            var response = JsonSerializer.Deserialize<PluginResponse>(File.ReadAllText(request.ResponseFilePath), PluginBootstrap.JsonOpts)!;
            Assert.Equal(PluginStatus.Ok, response.Status);
        }
        finally
        {
            dir.Delete(recursive: true);
        }
    }
}
