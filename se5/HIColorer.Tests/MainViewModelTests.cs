using Avalonia.Media;
using SubtitleEdit.Plugins.HIColorer.Logic;
using SubtitleEdit.Plugins.HIColorer.ViewModels;
using SubtitleEdit.Plugins.Shared;
using System.Text.Json;

namespace HIColorer.Tests;

public class MainViewModelTests
{
    private static PluginRequest Request(string requestType, string? settingsJson, params string[] texts) => new()
    {
        RequestType = requestType,
        Subtitle = new PluginSubtitle
        {
            Format = "Advanced Sub Station Alpha",
            Paragraphs = texts.Select((t, i) => new PluginParagraph
            {
                StartMs = i * 1000, EndMs = i * 1000 + 900, Text = t, Style = "Default", Actor = "Actor" + i,
            }).ToList(),
        },
        Settings = settingsJson == null ? null : JsonDocument.Parse(settingsJson).RootElement.Clone(),
        SettingsVersion = settingsJson == null ? null : 1,
    };

    [Fact]
    public void Run_ColorsCheckedLines_KeepsFields()
    {
        var vm = new MainViewModel(Request(PluginRequestType.Run, null, "(sighs)", "Plain", "JOHN: Hi."));
        Assert.Equal(2, vm.ChangeCount);
        vm.Changes[1].IsSelected = false;

        var response = vm.BuildResponse();
        var paragraphs = response.Subtitle!.Paragraphs!;
        Assert.Equal($"<font color=\"{ColorOptions.DefaultMoodsColor}\">(sighs)</font>", paragraphs[0].Text);
        Assert.Equal("Plain", paragraphs[1].Text);
        Assert.Equal("JOHN: Hi.", paragraphs[2].Text);
        Assert.Equal("Actor0", paragraphs[0].Actor);
    }

    [Fact]
    public void ChangingAColor_UpdatesThePreview()
    {
        var vm = new MainViewModel(Request(PluginRequestType.Run, null, "(sighs)"));
        vm.MoodsColor = Color.Parse("#123456");
        Assert.Equal("<font color=\"#123456\">(sighs)</font>", vm.Changes.Single().After);
    }

    [Fact]
    public void MusicToggle_OnlyAffectsMusic()
    {
        var vm = new MainViewModel(Request(PluginRequestType.Run, null, "♪ La ♪", "JOHN: Hi."));
        vm.Music = false;
        Assert.Equal(new[] { 1 }, vm.Changes.Select(c => c.Index));
        Assert.True(vm.Narrator);
    }

    [Fact]
    public void RemoveMode_StripsColors()
    {
        var vm = new MainViewModel(Request(PluginRequestType.Run, null, "<font color=\"#ff0000\">Red</font>", "Plain"));
        Assert.False(vm.HasChanges);

        vm.IsRemoveMode = true;
        Assert.Equal("Red", vm.Changes.Single().After);
    }

    [Fact]
    public void SavedSettings_AreLoaded_AndReturned()
    {
        var settings = "{\"mode\":\"Color\",\"narrator\":false,\"moodsColor\":\"#abcdef\",\"music\":true,\"showRendered\":false}";
        var vm = new MainViewModel(Request(PluginRequestType.Run, settings, "(sighs)", "JOHN: Hi."));
        Assert.False(vm.Narrator);
        Assert.False(vm.ShowRendered);
        Assert.Equal("<font color=\"#abcdef\">(sighs)</font>", vm.Changes.Single().After);

        var response = vm.BuildCancelResponse();
        Assert.Equal("#abcdef", response.Settings!.Value.GetProperty("moodsColor").GetString());
        Assert.False(response.Settings!.Value.GetProperty("narrator").GetBoolean());
        Assert.Equal(1, response.SettingsVersion);
    }

    [Fact]
    public void Batch_UsesSavedMode_WithoutSettings()
    {
        var response = new MainViewModel(Request(PluginRequestType.Batch, "{\"mode\":\"Remove\"}", "<font color=\"#ff0000\">(sighs)</font>")).BuildBatchResponse();
        Assert.Equal(PluginStatus.Ok, response.Status);
        Assert.Equal("(sighs)", response.Subtitle!.Paragraphs![0].Text);
        Assert.Null(response.Settings);
    }

    [Fact]
    public void Batch_NothingToChange_ReturnsOkWithoutSubtitle()
    {
        var response = new MainViewModel(Request(PluginRequestType.Batch, null, "Plain")).BuildBatchResponse();
        Assert.Equal(PluginStatus.Ok, response.Status);
        Assert.Null(response.Subtitle);
    }

    [Fact]
    public void Configure_ReturnsSettingsOnly()
    {
        var vm = new MainViewModel(Request(PluginRequestType.Configure, null));
        vm.NarratorColor = Color.Parse("#010203");

        var response = vm.BuildResponse();
        Assert.Equal(PluginStatus.Ok, response.Status);
        Assert.Null(response.Subtitle);
        Assert.Equal("#010203", response.Settings!.Value.GetProperty("narratorColor").GetString());
    }
}
