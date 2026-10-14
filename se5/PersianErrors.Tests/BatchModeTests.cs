using SubtitleEdit.Plugins.PersianErrors;
using SubtitleEdit.Plugins.Shared;
using System.Text.Json;

namespace PersianErrors.Tests;

/// <summary>Subtitle Edit Batch convert: "batch" fixes every line headless, "configure" returns the groups only.</summary>
public class BatchModeTests
{
    private static PluginRequest Request(string requestType, string? settingsJson, params string[] texts) => new()
    {
        RequestType = requestType,
        Subtitle = new PluginSubtitle
        {
            Format = "SubRip",
            Paragraphs = texts.Select((t, i) => new PluginParagraph { StartMs = i * 1000, EndMs = i * 1000 + 900, Text = t }).ToList(),
        },
        Settings = settingsJson == null ? null : JsonDocument.Parse(settingsJson).RootElement.Clone(),
        SettingsVersion = settingsJson == null ? null : 1,
    };

    [Fact]
    public void Batch_FixesAllLines()
    {
        var response = new MainViewModel(Request(PluginRequestType.Batch, null, "شدني", "درست", "املاك")).BuildBatchResponse();

        Assert.Equal(PluginStatus.Ok, response.Status);
        Assert.Equal(new[] { "شدنی", "درست", "املاک" }, response.Subtitle!.Paragraphs!.Select(p => p.Text));
        Assert.Null(response.Settings);
        Assert.Null(response.SettingsVersion);
    }

    [Fact]
    public void Batch_NothingToFix_ReturnsOkWithoutSubtitle()
    {
        var response = new MainViewModel(Request(PluginRequestType.Batch, null, "درست")).BuildBatchResponse();

        Assert.Equal(PluginStatus.Ok, response.Status);
        Assert.Null(response.Subtitle);
    }

    [Fact]
    public void Batch_SkipsDisabledGroups()
    {
        var settings = "{\"disabledGroups\":[\"Change Arabic Chars to Persian\"]}";

        var response = new MainViewModel(Request(PluginRequestType.Batch, settings, "شدني")).BuildBatchResponse();

        Assert.Equal(PluginStatus.Ok, response.Status);
        Assert.Null(response.Subtitle);
    }

    [Fact]
    public void Batch_IgnoresSelectedLinesOnly()
    {
        var request = Request(PluginRequestType.Batch, "{\"selectedLinesOnly\":true}", "شدني", "املاك");
        request.SelectedIndices = new List<int> { 0 };

        var response = new MainViewModel(request).BuildBatchResponse();

        Assert.Equal(new[] { "شدنی", "املاک" }, response.Subtitle!.Paragraphs!.Select(p => p.Text));
    }

    [Fact]
    public void Configure_ReturnsSettingsWithoutSubtitle()
    {
        var vm = new MainViewModel(Request(PluginRequestType.Configure, "{\"selectedLinesOnly\":true,\"showHidden\":false}"));
        Assert.True(vm.IsConfigure);
        Assert.False(vm.IsClean);
        Assert.False(vm.IsBusy);
        vm.Groups.Single(g => g.Name == "OCR").IsEnabled = false;

        var response = vm.BuildResponse();

        Assert.Equal(PluginStatus.Ok, response.Status);
        Assert.Null(response.Subtitle);
        Assert.Equal(1, response.SettingsVersion);
        var settings = response.Settings!.Value;
        Assert.Equal(new[] { "OCR" }, settings.GetProperty("disabledGroups").EnumerateArray().Select(e => e.GetString()));
        Assert.True(settings.GetProperty("selectedLinesOnly").GetBoolean());
        Assert.False(settings.GetProperty("showHidden").GetBoolean());
    }
}
