using SubtitleEdit.Plugins.PersianErrors;
using SubtitleEdit.Plugins.Shared;

namespace PersianErrors.Tests;

public class MainViewModelTests
{
    private static async Task<MainViewModel> CreateAsync(PluginRequest request)
    {
        var vm = new MainViewModel(request);
        await WaitAsync(vm);
        return vm;
    }

    private static async Task WaitAsync(MainViewModel vm)
    {
        for (var i = 0; i < 300 && vm.IsBusy; i++)
        {
            await Task.Delay(10);
        }

        Assert.False(vm.IsBusy);
    }

    private static PluginRequest Request(params string[] texts) => new()
    {
        Subtitle = new PluginSubtitle
        {
            Format = "Advanced Sub Station Alpha",
            Paragraphs = texts.Select((t, i) => new PluginParagraph { StartMs = i * 1000, EndMs = i * 1000 + 900, Text = t, Style = "Persian", Actor = "A" }).ToList(),
        },
    };

    [Fact]
    public async Task OkReturnsTheLinesInTheirOwnFormat()
    {
        var vm = await CreateAsync(Request("شدني", "درست"));

        var response = vm.BuildResponse();

        Assert.Equal(PluginStatus.Ok, response.Status);
        Assert.Equal("Advanced Sub Station Alpha", response.Subtitle!.Format);
        Assert.Equal(new[] { "شدنی", "درست" }, response.Subtitle.Paragraphs!.Select(p => p.Text));
        Assert.All(response.Subtitle.Paragraphs!, p => Assert.Equal("Persian", p.Style));
    }

    [Fact]
    public async Task UnselectedFixesAreLeftOut()
    {
        var vm = await CreateAsync(Request("شدني", "املاك"));
        vm.VisibleFixes.Single(f => f.Index == 0).IsSelected = false;

        var response = vm.BuildResponse();

        Assert.Equal(new[] { "شدني", "املاک" }, response.Subtitle!.Paragraphs!.Select(p => p.Text));
    }

    [Fact]
    public async Task NoFixesIsCancelled()
    {
        var vm = await CreateAsync(Request("درست"));

        Assert.Equal(PluginStatus.Cancelled, vm.BuildResponse().Status);
    }

    [Fact]
    public async Task OlderSubtitleEditGetsSubRipBack()
    {
        var request = new PluginRequest { Subtitle = new PluginSubtitle { SubRip = "1\n00:00:01,000 --> 00:00:02,000\nشدني\n" } };
        var vm = await CreateAsync(request);

        var response = vm.BuildResponse();

        Assert.Equal("SubRip", response.Subtitle!.Format);
        Assert.Contains("شدنی", response.Subtitle.Native);
        Assert.Null(response.Subtitle.Paragraphs);
    }

    [Fact]
    public async Task SettingsRoundTrip()
    {
        var first = await CreateAsync(Request("شدني"));
        first.Groups.Single(g => g.Name == "OCR").IsEnabled = false;
        first.ShowHidden = false;
        var saved = first.BuildCancelResponse();

        var request = Request("شدني");
        request.Settings = saved.Settings;
        request.SettingsVersion = saved.SettingsVersion;
        var second = await CreateAsync(request);

        Assert.False(second.Groups.Single(g => g.Name == "OCR").IsEnabled);
        Assert.False(second.ShowHidden);
        Assert.Equal(second.Groups.Count - 1, second.EnabledGroupCount);
    }

    [Fact]
    public async Task ApplyAndRecheckThenUndo()
    {
        var vm = await CreateAsync(Request("شدني"));

        vm.ApplyAndRecheckCommand.Execute(null);
        await WaitAsync(vm);
        Assert.Equal(1, vm.ChangedLineCount);
        Assert.Equal(0, vm.FixCount);

        vm.UndoCommand.Execute(null);
        await WaitAsync(vm);
        Assert.Equal(0, vm.ChangedLineCount);
        Assert.Equal(1, vm.FixCount);
    }
}
