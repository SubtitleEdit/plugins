using SubtitleEdit.Plugins.ItalicEachLine;
using SubtitleEdit.Plugins.Shared;

namespace ItalicEachLine.Tests;

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
    public void CountsBothModesAndListsCurrentMode()
    {
        var vm = new MainViewModel(Request("<i>A\nB</i>", "<i>C</i>\n<i>D</i>", "Plain"));

        Assert.Equal(ItalicMode.EachLine, vm.Mode);
        Assert.Equal(1, vm.EachLineCount);
        Assert.Equal(1, vm.OneTagCount);
        Assert.Equal(0, Assert.Single(vm.Changes).Index);

        vm.Mode = ItalicMode.OneTag;
        Assert.Equal(1, Assert.Single(vm.Changes).Index);
    }

    [Fact]
    public void ResponseKeepsParagraphFieldsAndSkipsUnchecked()
    {
        var vm = new MainViewModel(Request("<i>A\nB</i>", "<i>C\nD</i>"));
        vm.Changes[1].IsSelected = false;

        var response = vm.BuildResponse();

        Assert.Equal(PluginStatus.Ok, response.Status);
        var paragraphs = response.Subtitle!.Paragraphs!;
        Assert.Equal("<i>A</i>\n<i>B</i>", paragraphs[0].Text);
        Assert.Equal("<i>C\nD</i>", paragraphs[1].Text);
        Assert.All(paragraphs, p => Assert.Equal("Top", p.Style));
    }

    [Fact]
    public void SelectedLinesScope()
    {
        var request = Request("<i>A\nB</i>", "<i>C\nD</i>");
        request.SelectedIndices = new List<int> { 1 };
        var vm = new MainViewModel(request);

        Assert.True(vm.SelectedLinesOnly);
        Assert.Equal(1, Assert.Single(vm.Changes).Index);

        vm.AllLines = true;
        Assert.Equal(2, vm.Changes.Count);
    }

    [Fact]
    public void NothingSelectedCancels()
    {
        var vm = new MainViewModel(Request("<i>A\nB</i>"));
        vm.SelectNoneCommand.Execute(null);

        Assert.Equal(PluginStatus.Cancelled, vm.BuildResponse().Status);
    }

    [Fact]
    public void LegacyRequestUsesSubRip()
    {
        var vm = new MainViewModel(new PluginRequest
        {
            Subtitle = new PluginSubtitle { SubRip = "1\n00:00:01,000 --> 00:00:02,000\n<i>A\nB</i>\n" },
        });

        var response = vm.BuildResponse();

        Assert.Equal("SubRip", response.Subtitle!.Format);
        Assert.Contains("<i>A</i>", response.Subtitle.Native);
        Assert.Contains("<i>B</i>", response.Subtitle.Native);
    }
}
