using SubtitleEdit.Plugins.FixEncodingErrors;
using SubtitleEdit.Plugins.Shared;
using System.Text.Json;

namespace FixEncodingErrors.Tests;

/// <summary>Subtitle Edit Batch convert: "batch" fixes every line headless, "configure" returns the code page only.</summary>
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
    public void Batch_FixesAllBrokenLines()
    {
        var response = new MainViewModel(Request(PluginRequestType.Batch, null, "cafÃ©", "Plain", "donâ€™t")).BuildBatchResponse();

        Assert.Equal(PluginStatus.Ok, response.Status);
        Assert.Equal(new[] { "café", "Plain", "don’t" }, response.Subtitle!.Paragraphs!.Select(p => p.Text));
        Assert.Null(response.Settings);
    }

    [Fact]
    public void Batch_NothingToFix_ReturnsOkWithoutSubtitle()
    {
        var response = new MainViewModel(Request(PluginRequestType.Batch, null, "Plain", "résumé")).BuildBatchResponse();

        Assert.Equal(PluginStatus.Ok, response.Status);
        Assert.Null(response.Subtitle);
    }

    [Fact]
    public void Batch_UsesSavedCodePage()
    {
        var vm = new MainViewModel(Request(PluginRequestType.Batch, "{\"codePage\":1251}", "cafÃ©"));

        Assert.Equal(1251, vm.SelectedEncoding.CodePage);
    }

    [Fact]
    public void Configure_ReturnsSettingsWithoutSubtitle()
    {
        var vm = new MainViewModel(Request(PluginRequestType.Configure, null));
        Assert.True(vm.CanApply);
        Assert.False(vm.IsClean);
        vm.SelectedEncoding = vm.EncodingOptions.First(e => e.CodePage == 1250);

        var response = vm.BuildResponse();

        Assert.Equal(PluginStatus.Ok, response.Status);
        Assert.Null(response.Subtitle);
        Assert.Equal(1250, response.Settings!.Value.GetProperty("codePage").GetInt32());
    }
}
