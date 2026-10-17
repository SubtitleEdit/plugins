using SubtitleEdit.Plugins.LinesUnbreaker.ViewModels;
using SubtitleEdit.Plugins.Shared;
using System.Text.Json;

namespace LinesUnbreaker.Tests;

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
    public void Run_OnlyCheckedSubtitlesAreJoined()
    {
        var vm = new MainViewModel(Request(PluginRequestType.Run, null, "One\ntwo", "Three\nfour", "Single"));
        Assert.Equal(2, vm.ChangeCount);
        vm.Changes[1].IsSelected = false;

        var response = vm.BuildResponse();
        Assert.Equal(PluginStatus.Ok, response.Status);
        var paragraphs = response.Subtitle!.Paragraphs!;
        Assert.Equal(new[] { "One two", "Three\nfour", "Single" }, paragraphs.Select(p => p.Text));
        Assert.Equal("Actor0", paragraphs[0].Actor);
        Assert.Equal("Default", paragraphs[0].Style);
    }

    [Fact]
    public void MaxLength_DefaultsToSubtitleEditRule()
    {
        var request = Request(PluginRequestType.Run, null, "abcd\nefgh");
        request.Rules = new PluginRules { SubtitleLineMaximumLength = 8 };
        var vm = new MainViewModel(request);
        Assert.Equal(8, vm.MaxLength);
        Assert.False(vm.HasChanges);

        vm.MaxLengthValue = 9;
        Assert.Equal(9, vm.MaxLength);
        Assert.True(vm.HasChanges);
    }

    [Fact]
    public void SavedSettings_WinOverRule()
    {
        var request = Request(PluginRequestType.Run, "{\"maxLength\":30,\"skipDialogs\":false,\"skipMoods\":true,\"skipNarrator\":false}", "- A\n- B");
        request.Rules = new PluginRules { SubtitleLineMaximumLength = 43 };
        var vm = new MainViewModel(request);
        Assert.Equal(30, vm.MaxLength);
        Assert.False(vm.SkipDialogs);
        Assert.True(vm.SkipMoods);
        Assert.False(vm.SkipNarrator);
        Assert.Equal("- A - B", vm.Changes.Single().After);
    }

    [Fact]
    public void Cancel_ReturnsSettings()
    {
        var vm = new MainViewModel(Request(PluginRequestType.Run, null, "a\nb"));
        vm.MaxLength = 50;
        vm.SkipMoods = true;

        var response = vm.BuildCancelResponse();
        Assert.Equal(PluginStatus.Cancelled, response.Status);
        Assert.Equal(50, response.Settings!.Value.GetProperty("maxLength").GetInt32());
        Assert.True(response.Settings!.Value.GetProperty("skipMoods").GetBoolean());
    }

    [Fact]
    public void Batch_JoinsAll_WithoutSettings()
    {
        var response = new MainViewModel(Request(PluginRequestType.Batch, null, "One\ntwo", "- Hi!\n- Hello.")).BuildBatchResponse();
        Assert.Equal(PluginStatus.Ok, response.Status);
        Assert.Equal(new[] { "One two", "- Hi!\n- Hello." }, response.Subtitle!.Paragraphs!.Select(p => p.Text));
        Assert.Null(response.Settings);
    }

    [Fact]
    public void Batch_NothingToChange_ReturnsOkWithoutSubtitle()
    {
        var response = new MainViewModel(Request(PluginRequestType.Batch, null, "Single")).BuildBatchResponse();
        Assert.Equal(PluginStatus.Ok, response.Status);
        Assert.Null(response.Subtitle);
    }

    [Fact]
    public void Configure_ReturnsSettingsOnly()
    {
        var vm = new MainViewModel(Request(PluginRequestType.Configure, null));
        Assert.True(vm.CanApply);
        vm.MaxLength = 37;

        var response = vm.BuildResponse();
        Assert.Equal(PluginStatus.Ok, response.Status);
        Assert.Null(response.Subtitle);
        Assert.Equal(37, response.Settings!.Value.GetProperty("maxLength").GetInt32());
    }
}
