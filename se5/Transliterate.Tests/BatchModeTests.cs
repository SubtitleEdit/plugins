using SubtitleEdit.Plugins.Shared;
using SubtitleEdit.Plugins.Transliterate;
using SubtitleEdit.Plugins.Transliterate.Engine;
using System.Text.Json;

namespace Transliterate.Tests;

/// <summary>Subtitle Edit Batch convert: "batch" converts every line headless, "configure" returns the settings only.</summary>
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

    private static string[] Texts(PluginResponse response) => response.Subtitle!.Paragraphs!.Select(p => p.Text ?? string.Empty).ToArray();

    [Fact]
    public void Batch_DefaultConvertsDetectedLanguageToLatin()
    {
        var response = new MainViewModel(Request(PluginRequestType.Batch, null, "Ђорђе је ту.", "Hello", "<i>Љубав</i>")).BuildBatchResponse();

        Assert.Equal(PluginStatus.Ok, response.Status);
        Assert.Equal(new[] { "Đorđe je tu.", "Hello", "<i>Ljubav</i>" }, Texts(response));
        Assert.Null(response.Settings);
        Assert.Null(response.SettingsVersion);
    }

    [Fact]
    public void Batch_LanguageFollowsDetectionPerFile()
    {
        var settings = "{\"language\":\"sr\",\"direction\":\"ToLatin\"}";

        var greek = new MainViewModel(Request(PluginRequestType.Batch, settings, "Καλημέρα"));
        Assert.Equal("el", greek.SelectedLanguage.Language.Id);
        var response = greek.BuildBatchResponse();
        Assert.Equal(PluginStatus.Ok, response.Status);
        Assert.Equal(new[] { "Kalimera" }, Texts(response));
    }

    [Fact]
    public void Batch_UsesSavedSystem()
    {
        var response = new MainViewModel(Request(PluginRequestType.Batch, "{\"language\":\"ru\",\"direction\":\"ToLatin\",\"systems\":{\"ru\":1}}", "Щука")).BuildBatchResponse();

        Assert.Equal(new[] { "Ŝuka" }, Texts(response));
    }

    [Fact]
    public void Batch_ToLatin_LatinFileIsLeftAlone()
    {
        // The window would flip to Latin → Cyrillic for Serbian Latin; batch keeps the configured direction.
        var vm = new MainViewModel(Request(PluginRequestType.Batch, "{\"language\":\"sr\",\"direction\":\"ToLatin\"}", "Đorđe je tu."));
        var response = vm.BuildBatchResponse();

        Assert.Equal(PluginStatus.Ok, response.Status);
        Assert.Null(response.Subtitle);
    }

    [Fact]
    public void Batch_FromLatin_ConvertsDetectedLatin()
    {
        var response = new MainViewModel(Request(PluginRequestType.Batch, "{\"language\":\"sr\",\"direction\":\"FromLatin\"}", "Đorđe je tu.", "OK")).BuildBatchResponse();

        Assert.Equal(new[] { "Ђорђе је ту.", "OK" }, Texts(response));
    }

    [Fact]
    public void Batch_FromLatin_UndetectedLatinIsLeftAlone()
    {
        // No đ/ć/č/š/ž/ʻ: could be English, so Latin → Cyrillic is not run.
        var response = new MainViewModel(Request(PluginRequestType.Batch, "{\"language\":\"sr\",\"direction\":\"FromLatin\"}", "Good morning", "Dobro jutro")).BuildBatchResponse();

        Assert.Equal(PluginStatus.Ok, response.Status);
        Assert.Null(response.Subtitle);
    }

    [Fact]
    public void Batch_FromLatin_CyrillicFileIsLeftAlone()
    {
        var response = new MainViewModel(Request(PluginRequestType.Batch, "{\"language\":\"sr\",\"direction\":\"FromLatin\"}", "Ђорђе је ту.")).BuildBatchResponse();

        Assert.Equal(PluginStatus.Ok, response.Status);
        Assert.Null(response.Subtitle);
    }

    [Fact]
    public void Batch_NothingToConvert_ReturnsOkWithoutSubtitle()
    {
        var response = new MainViewModel(Request(PluginRequestType.Batch, null, "Hello", "World")).BuildBatchResponse();

        Assert.Equal(PluginStatus.Ok, response.Status);
        Assert.Null(response.Subtitle);
    }

    [Fact]
    public void Configure_ReturnsSettingsWithoutSubtitle()
    {
        var vm = new MainViewModel(Request(PluginRequestType.Configure, "{\"language\":\"sr\",\"direction\":\"ToLatin\",\"selectedLinesOnly\":true}"));
        Assert.True(vm.CanApply);
        Assert.False(vm.IsClean);
        Assert.True(vm.IsConfigure);
        Assert.False(vm.HasDetected);

        vm.SwapCommand.Execute(null);
        vm.KeepWordsText = "OK, TV";
        var response = vm.BuildResponse();

        Assert.Equal(PluginStatus.Ok, response.Status);
        Assert.Null(response.Subtitle);
        var settings = response.Settings!.Value;
        Assert.Equal("sr", settings.GetProperty("language").GetString());
        Assert.Equal(nameof(Direction.FromLatin), settings.GetProperty("direction").GetString());
        Assert.Equal("OK, TV", settings.GetProperty("keepWords").GetString());
        Assert.True(settings.GetProperty("selectedLinesOnly").GetBoolean()); // a normal run's scope is kept
    }

    [Fact]
    public void Run_UnchangedByBatchSupport()
    {
        // Same saved settings as Batch_ToLatin_LatinFileIsLeftAlone: the window still follows the detection.
        var vm = new MainViewModel(Request(PluginRequestType.Run, "{\"language\":\"sr\",\"direction\":\"ToLatin\"}", "Đorđe je tu."));

        Assert.Equal(Direction.FromLatin, vm.Direction);
        Assert.Equal("Ђорђе је ту.", vm.Changes.Single().After);
        Assert.NotNull(vm.BuildResponse().Settings);
    }
}
