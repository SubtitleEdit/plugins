using SubtitleEdit.Plugins.ItalicEachLine;
using SubtitleEdit.Plugins.Shared;
using System.Text.Json;

namespace ItalicEachLine.Tests;

/// <summary>Subtitle Edit Batch convert: "batch" converts every line headless, "configure" returns the direction only.</summary>
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
    public void Batch_DefaultsToEachLine_ConvertsAllLines()
    {
        var response = new MainViewModel(Request(PluginRequestType.Batch, null, "<i>A\nB</i>", "Plain", "<i>C\nD</i>")).BuildBatchResponse();

        Assert.Equal(PluginStatus.Ok, response.Status);
        Assert.Equal(new[] { "<i>A</i>\n<i>B</i>", "Plain", "<i>C</i>\n<i>D</i>" }, response.Subtitle!.Paragraphs!.Select(p => p.Text));
        Assert.Null(response.Settings);
        Assert.Null(response.SettingsVersion);
    }

    [Fact]
    public void Batch_UsesSavedMode_IgnoresSavedSelectedLinesOnly()
    {
        var settings = "{\"mode\":\"OneTag\",\"selectedLinesOnly\":true}";
        var response = new MainViewModel(Request(PluginRequestType.Batch, settings, "<i>A</i>\n<i>B</i>", "<i>C</i>\n<i>D</i>")).BuildBatchResponse();

        Assert.Equal(PluginStatus.Ok, response.Status);
        Assert.Equal(new[] { "<i>A\nB</i>", "<i>C\nD</i>" }, response.Subtitle!.Paragraphs!.Select(p => p.Text));
    }

    [Fact]
    public void Batch_NothingToChange_ReturnsOkWithoutSubtitle()
    {
        var response = new MainViewModel(Request(PluginRequestType.Batch, null, "Plain", "<i>A</i>\n<i>B</i>")).BuildBatchResponse();

        Assert.Equal(PluginStatus.Ok, response.Status);
        Assert.Null(response.Subtitle);
    }

    [Fact]
    public void Configure_ReturnsSettingsWithoutSubtitle()
    {
        var vm = new MainViewModel(Request(PluginRequestType.Configure, "{\"mode\":\"EachLine\",\"showRendered\":true,\"selectedLinesOnly\":true}"));
        Assert.True(vm.IsConfigure);
        Assert.True(vm.CanApply);
        Assert.False(vm.IsClean);
        vm.Mode = ItalicMode.OneTag;

        var response = vm.BuildResponse();

        Assert.Equal(PluginStatus.Ok, response.Status);
        Assert.Null(response.Subtitle);
        Assert.Equal(1, response.SettingsVersion);
        Assert.Equal("OneTag", response.Settings!.Value.GetProperty("mode").GetString());
        Assert.True(response.Settings!.Value.GetProperty("showRendered").GetBoolean());
        Assert.True(response.Settings!.Value.GetProperty("selectedLinesOnly").GetBoolean());
    }

    [Fact]
    public void Run_IsUnchanged()
    {
        var vm = new MainViewModel(Request(PluginRequestType.Run, null, "<i>A\nB</i>"));
        Assert.False(vm.IsConfigure);
        Assert.Equal("<i>A</i>\n<i>B</i>", vm.BuildResponse().Subtitle!.Paragraphs![0].Text);
    }
}
