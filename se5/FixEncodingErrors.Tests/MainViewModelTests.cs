using SubtitleEdit.Plugins.FixEncodingErrors;
using SubtitleEdit.Plugins.Shared;

namespace FixEncodingErrors.Tests;

public class MainViewModelTests
{
    private static PluginRequest Request(params string[] texts) => new()
    {
        Subtitle = new PluginSubtitle
        {
            Format = "Advanced Sub Station Alpha",
            Paragraphs = texts.Select((t, i) => new PluginParagraph { StartMs = i * 1000, EndMs = i * 1000 + 900, Text = t, Style = "Top", Actor = "A" }).ToList(),
        },
    };

    [Fact]
    public void ListsBrokenLines()
    {
        var vm = new MainViewModel(Request("and dust off my rã©sumã©.", "Plain", "donâ€™t"));

        Assert.Equal(0, vm.SelectedEncoding.CodePage);
        Assert.Equal(1252, vm.EffectiveEncoding.CodePage);
        Assert.Equal(new[] { 0, 2 }, vm.Changes.Select(c => c.Index));
        Assert.Equal("and dust off my résumé.", vm.Changes[0].After);
    }

    [Fact]
    public void ConfirmedSubtitleFixesAmbiguousMatches()
    {
        // "BÃ–" alone is left; with a clear "Ã©" elsewhere in the subtitle it is fixed too.
        Assert.Empty(new MainViewModel(Request("BÃ–")).Changes);
        Assert.Equal(2, new MainViewModel(Request("BÃ–", "cafÃ©")).Changes.Count);
    }

    [Fact]
    public void ResponseKeepsParagraphFieldsAndSkipsUnchecked()
    {
        var vm = new MainViewModel(Request("cafÃ©", "rÃ©sumÃ©"));
        vm.Changes[1].IsSelected = false;

        var response = vm.BuildResponse();

        Assert.Equal(PluginStatus.Ok, response.Status);
        var paragraphs = response.Subtitle!.Paragraphs!;
        Assert.Equal("café", paragraphs[0].Text);
        Assert.Equal("rÃ©sumÃ©", paragraphs[1].Text);
        Assert.All(paragraphs, p => Assert.Equal("Top", p.Style));
    }

    [Fact]
    public void SelectedLinesScope()
    {
        var request = Request("cafÃ©", "rÃ©sumÃ©");
        request.SelectedIndices = new List<int> { 1 };
        var vm = new MainViewModel(request);

        Assert.True(vm.SelectedLinesOnly);
        Assert.Equal(1, Assert.Single(vm.Changes).Index);

        vm.AllLines = true;
        Assert.Equal(2, vm.Changes.Count);
    }

    [Fact]
    public void PickedCodePageIsRemembered()
    {
        var vm = new MainViewModel(Request("cafÃ©"));
        vm.SelectedEncoding = vm.EncodingOptions.Single(e => e.CodePage == 1251);
        Assert.Empty(vm.Changes);

        var cancel = vm.BuildCancelResponse();
        var request = Request("cafÃ©");
        request.Settings = cancel.Settings;
        request.SettingsVersion = cancel.SettingsVersion;
        Assert.Equal(1251, new MainViewModel(request).SelectedEncoding.CodePage);
    }

    [Fact]
    public void NothingSelectedCancels()
    {
        var vm = new MainViewModel(Request("cafÃ©"));
        vm.SelectNoneCommand.Execute(null);

        Assert.Equal(PluginStatus.Cancelled, vm.BuildResponse().Status);
    }

    [Fact]
    public void LegacyRequestUsesSubRip()
    {
        var vm = new MainViewModel(new PluginRequest
        {
            Subtitle = new PluginSubtitle { SubRip = "1\n00:00:01,000 --> 00:00:02,000\nrÃ©sumÃ©\n" },
        });

        var response = vm.BuildResponse();

        Assert.Equal("SubRip", response.Subtitle!.Format);
        Assert.Contains("résumé", response.Subtitle.Native);
    }
}
