using SubtitleEdit.Plugins.Shared;
using SubtitleEdit.Plugins.TightHyphen;
using System.Text.Json;

namespace TightHyphen.Tests;

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
    public void Run_OnlyCheckedLinesChange_KeepsFields()
    {
        var vm = new MainViewModel(Request(PluginRequestType.Run, null, "- A\n- B", "Plain", "- C"));
        Assert.Equal(2, vm.ChangeCount);
        vm.Changes[1].IsSelected = false;

        var response = vm.BuildResponse();
        var paragraphs = response.Subtitle!.Paragraphs!;
        Assert.Equal(new[] { "-A\n-B", "Plain", "- C" }, paragraphs.Select(p => p.Text));
        Assert.Equal("Actor0", paragraphs[0].Actor);
        Assert.Equal("Tight hyphen", response.UndoDescription);
    }

    [Fact]
    public void Run_SelectedLinesOnly()
    {
        var request = Request(PluginRequestType.Run, null, "- A", "- B");
        request.SelectedIndices = new List<int> { 1 };
        var vm = new MainViewModel(request);
        Assert.Equal(new[] { 1 }, vm.Changes.Select(c => c.Index));
    }

    [Fact]
    public void Run_NothingToChange_IsClean()
    {
        var vm = new MainViewModel(Request(PluginRequestType.Run, null, "-A", "Plain"));
        Assert.True(vm.IsClean);
        Assert.False(vm.CanApply);
    }

    [Fact]
    public void Batch_ChangesAll_WithoutSettings()
    {
        var response = new MainViewModel(Request(PluginRequestType.Batch, null, "- A\n- B")).BuildBatchResponse();
        Assert.Equal(PluginStatus.Ok, response.Status);
        Assert.Equal("-A\n-B", response.Subtitle!.Paragraphs![0].Text);
        Assert.Null(response.Settings);
    }

    [Fact]
    public void Batch_NothingToChange_ReturnsOkWithoutSubtitle()
    {
        var response = new MainViewModel(Request(PluginRequestType.Batch, null, "-A")).BuildBatchResponse();
        Assert.Equal(PluginStatus.Ok, response.Status);
        Assert.Null(response.Subtitle);
    }

    [Fact]
    public void Configure_ReturnsOkWithoutSubtitle()
    {
        var vm = new MainViewModel(Request(PluginRequestType.Configure, null));
        Assert.True(vm.CanApply);
        var response = vm.BuildResponse();
        Assert.Equal(PluginStatus.Ok, response.Status);
        Assert.Null(response.Subtitle);
    }
}
