using SubtitleEdit.Plugins.AssaTagCheck;
using SubtitleEdit.Plugins.Shared;

namespace AssaTagCheck.Tests;

public class MainViewModelTests
{
    private const string Header =
        "[Script Info]\nPlayResX: 1920\nPlayResY: 1080\n\n[V4+ Styles]\n" +
        "Format: Name, Fontname, Fontsize, PrimaryColour, SecondaryColour, OutlineColour, BackColour, Bold, Italic, Underline, StrikeOut, ScaleX, ScaleY, Spacing, Angle, BorderStyle, Outline, Shadow, Alignment, MarginL, MarginR, MarginV, Encoding\n" +
        "Style: Default,Arial,48,&H00FFFFFF,&H000000FF,&H00000000,&H80000000,0,0,0,0,100,100,0,0,1,2,1,2,10,10,10,1\n";

    private static PluginRequest Request(params string[] texts) => new()
    {
        Subtitle = new PluginSubtitle
        {
            Format = "Advanced Sub Station Alpha",
            Header = Header,
            Paragraphs = texts.Select((t, i) => new PluginParagraph
            {
                StartMs = i * 2000, EndMs = i * 2000 + 1500, Text = t, Style = "Default", Actor = "A", Layer = 1,
            }).ToList(),
        },
    };

    [Fact]
    public void ListsLinesWithIssues()
    {
        var vm = new MainViewModel(Request("<i>A</i><i>B</i>", "Fine", "{\\fsbig}Manual"));

        Assert.Equal(2, vm.LineCount);
        Assert.Equal(1, vm.TidyCount);
        Assert.Equal(1, vm.ErrorCount);
        Assert.Equal(1, vm.SelectedCount); // the manual-only line can't be selected
    }

    [Fact]
    public void ResponseKeepsFieldsAndSkipsUnchecked()
    {
        var vm = new MainViewModel(Request("<i>A</i><i>B</i>", "{\\pos(1 2)}C"));
        vm.VisibleLines[1].IsSelected = false;

        var response = vm.BuildResponse();

        Assert.Equal(PluginStatus.Ok, response.Status);
        var paragraphs = response.Subtitle!.Paragraphs!;
        Assert.Equal("<i>AB</i>", paragraphs[0].Text);
        Assert.Equal("{\\pos(1 2)}C", paragraphs[1].Text);
        Assert.All(paragraphs, p => Assert.Equal("A", p.Actor));
        Assert.All(paragraphs, p => Assert.Equal(1, p.Layer));
    }

    [Fact]
    public void TurningOffRuleRemovesLines()
    {
        var vm = new MainViewModel(Request("<i>A</i><i>B</i>"));
        vm.Rules.Single(r => r.Rule == RuleId.UnneededEndStart).IsEnabled = false;

        Assert.True(vm.IsClean);
        Assert.Equal(PluginStatus.Cancelled, vm.BuildResponse().Status);
    }

    [Fact]
    public void ReportsMissingFont()
    {
        var vm = new MainViewModel(Request("{\\fnNo Such Font}Hi"), name => name == "Arial");
        Assert.Single(vm.ScriptIssues);
    }

    [Fact]
    public void EveryRuleExampleTriggersItsRule()
    {
        foreach (var rule in new MainViewModel(Request("x")).Rules)
        {
            Assert.True(rule.HasExampleMessage, rule.Name);
        }
    }
}
