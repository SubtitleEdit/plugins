using SubtitleEdit.Plugins.HI2UC;
using SubtitleEdit.Plugins.Shared;
using System.Text.Json;

namespace HI2UC.Tests;

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
    public void Run_ListsChangedLines_KeepsParagraphFields()
    {
        var vm = new MainViewModel(Request(PluginRequestType.Run, null, "(laughs)", "Plain", "John: Hi."));
        Assert.Equal(2, vm.ChangeCount);

        var response = vm.BuildResponse();
        Assert.Equal(PluginStatus.Ok, response.Status);
        var paragraphs = response.Subtitle!.Paragraphs!;
        Assert.Equal(new[] { "(LAUGHS)", "Plain", "JOHN: Hi." }, paragraphs.Select(p => p.Text));
        Assert.Equal("Actor2", paragraphs[2].Actor);
        Assert.Equal("Default", paragraphs[2].Style);
        Assert.Equal("Advanced Sub Station Alpha", response.Subtitle.Format);
    }

    [Fact]
    public void Run_UncheckedLinesAreNotApplied()
    {
        var vm = new MainViewModel(Request(PluginRequestType.Run, null, "(laughs)", "(sighs)"));
        vm.Changes[0].IsSelected = false;

        var response = vm.BuildResponse();
        Assert.Equal(new[] { "(laughs)", "(SIGHS)" }, response.Subtitle!.Paragraphs!.Select(p => p.Text));
    }

    [Fact]
    public void Run_NothingChecked_IsCancelled()
    {
        var vm = new MainViewModel(Request(PluginRequestType.Run, null, "(laughs)"));
        vm.SelectNoneCommand.Execute(null);
        Assert.False(vm.CanApply);
        Assert.Equal(PluginStatus.Cancelled, vm.BuildResponse().Status);
    }

    [Fact]
    public void Run_SavedStyleIsApplied()
    {
        var vm = new MainViewModel(Request(PluginRequestType.Run, "{\"style\":\"TitleCase\",\"moods\":true,\"narrator\":false}", "(door slams)", "john: Hi."));
        Assert.Equal(HiStyle.TitleCase, vm.SelectedStyle.Style);
        Assert.False(vm.Narrator);
        Assert.Equal(new[] { "(Door Slams)" }, vm.Changes.Select(c => c.After));
    }

    [Fact]
    public void Run_SelectedLinesOnly()
    {
        var request = Request(PluginRequestType.Run, null, "(aa)", "(bb)", "(cc)");
        request.SelectedIndices = new List<int> { 1 };
        var vm = new MainViewModel(request);
        Assert.True(vm.SelectedLinesOnly);
        Assert.Equal(new[] { 1 }, vm.Changes.Select(c => c.Index));

        vm.AllLines = true;
        Assert.Equal(3, vm.ChangeCount);
    }

    [Fact]
    public void Run_LegacyRequest_FallsBackToSubRip()
    {
        var request = new PluginRequest
        {
            RequestType = PluginRequestType.Run,
            Subtitle = new PluginSubtitle { Format = "SubRip", SubRip = "1\r\n00:00:01,000 --> 00:00:02,000\r\n(laughs)\r\n\r\n" },
        };

        var response = new MainViewModel(request).BuildResponse();
        Assert.Contains("(LAUGHS)", response.Subtitle!.Native);
        Assert.Null(response.Subtitle.Paragraphs);
    }

    [Fact]
    public void Cancel_ReturnsSettings()
    {
        var vm = new MainViewModel(Request(PluginRequestType.Run, null, "(laughs)"));
        vm.SelectedStyle = vm.StyleOptions.First(o => o.Style == HiStyle.LowerCase);
        vm.Moods = false;

        var response = vm.BuildCancelResponse();
        Assert.Equal(PluginStatus.Cancelled, response.Status);
        Assert.Equal("LowerCase", response.Settings!.Value.GetProperty("style").GetString());
        Assert.False(response.Settings!.Value.GetProperty("moods").GetBoolean());
        Assert.Equal(1, response.SettingsVersion);
    }

    [Fact]
    public void Batch_ChangesAllLines_WithoutSettings()
    {
        var response = new MainViewModel(Request(PluginRequestType.Batch, "{\"style\":\"UpperCase\",\"selectedLinesOnly\":true}", "(laughs)", "Mary: Hi.")).BuildBatchResponse();
        Assert.Equal(PluginStatus.Ok, response.Status);
        Assert.Equal(new[] { "(LAUGHS)", "MARY: Hi." }, response.Subtitle!.Paragraphs!.Select(p => p.Text));
        Assert.Null(response.Settings);
        Assert.Null(response.SettingsVersion);
    }

    [Fact]
    public void Batch_NothingToChange_ReturnsOkWithoutSubtitle()
    {
        var response = new MainViewModel(Request(PluginRequestType.Batch, null, "Plain", "(LAUGHS)")).BuildBatchResponse();
        Assert.Equal(PluginStatus.Ok, response.Status);
        Assert.Null(response.Subtitle);
    }

    [Fact]
    public void Configure_ReturnsSettingsOnly_KeepsSavedScope()
    {
        var vm = new MainViewModel(Request(PluginRequestType.Configure, "{\"style\":\"UpperCase\",\"selectedLinesOnly\":true}"));
        Assert.True(vm.IsConfigure);
        Assert.True(vm.CanApply);
        vm.SelectedStyle = vm.StyleOptions.First(o => o.Style == HiStyle.SentenceCase);

        var response = vm.BuildResponse();
        Assert.Equal(PluginStatus.Ok, response.Status);
        Assert.Null(response.Subtitle);
        Assert.Equal("SentenceCase", response.Settings!.Value.GetProperty("style").GetString());
        Assert.True(response.Settings!.Value.GetProperty("selectedLinesOnly").GetBoolean());
    }
}
