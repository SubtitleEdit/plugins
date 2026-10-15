using SubtitleEdit.Plugins.AssaTagCheck;
using SubtitleEdit.Plugins.Shared;
using System.Text.Json;

namespace AssaTagCheck.Tests;

/// <summary>Subtitle Edit Batch convert: "batch" fixes every fixable line headless, "configure" returns the rules only.</summary>
public class BatchModeTests
{
    private static PluginRequest Request(string requestType, string? settingsJson, params string[] texts) => new()
    {
        RequestType = requestType,
        Subtitle = new PluginSubtitle
        {
            Format = "Advanced Sub Station Alpha",
            Paragraphs = texts.Select((t, i) => new PluginParagraph
            {
                StartMs = i * 2000, EndMs = i * 2000 + 1500, Text = t, Style = "Default", Actor = "A",
            }).ToList(),
        },
        Settings = settingsJson == null ? null : JsonDocument.Parse(settingsJson).RootElement.Clone(),
        SettingsVersion = settingsJson == null ? null : 1,
    };

    [Fact]
    public void Batch_FixesEveryFixableLine_AndCountsManualOnes()
    {
        var response = new MainViewModel(Request(PluginRequestType.Batch, null, "<i>A</i><i>B</i>", "Fine", "{\\fsbig}Manual"))
            .BuildBatchResponse();

        Assert.Equal(PluginStatus.Ok, response.Status);
        var paragraphs = response.Subtitle!.Paragraphs!;
        Assert.Equal(new[] { "<i>AB</i>", "Fine", "{\\fsbig}Manual" }, paragraphs.Select(p => p.Text));
        Assert.All(paragraphs, p => Assert.Equal("A", p.Actor));
        Assert.Contains("1 line(s) to check by hand", response.Message);
        Assert.Null(response.Settings);
    }

    [Fact]
    public void Batch_NothingFixable_ReturnsOkWithoutSubtitle()
    {
        var response = new MainViewModel(Request(PluginRequestType.Batch, null, "Fine", "Also fine")).BuildBatchResponse();

        Assert.Equal(PluginStatus.Ok, response.Status);
        Assert.Null(response.Subtitle);
    }

    [Fact]
    public void Batch_UsesSavedRules()
    {
        var response = new MainViewModel(Request(PluginRequestType.Batch, "{\"disabledRules\":[\"UnneededEndStart\"]}", "<i>A</i><i>B</i>"))
            .BuildBatchResponse();

        Assert.Null(response.Subtitle);
    }

    [Fact]
    public void Configure_ReturnsRulesWithoutSubtitle_AndKeepsScope()
    {
        var vm = new MainViewModel(Request(PluginRequestType.Configure, "{\"disabledRules\":[],\"selectedLinesOnly\":true}"));
        Assert.True(vm.CanApply);
        Assert.False(vm.IsClean);
        vm.Rules.Single(r => r.Rule == RuleId.EmptyBlock).IsEnabled = false;

        var response = vm.BuildResponse();

        Assert.Equal(PluginStatus.Ok, response.Status);
        Assert.Null(response.Subtitle);
        var settings = response.Settings!.Value;
        Assert.Equal(new[] { "EmptyBlock" }, settings.GetProperty("disabledRules").EnumerateArray().Select(e => e.GetString()));
        Assert.True(settings.GetProperty("selectedLinesOnly").GetBoolean());
    }
}
