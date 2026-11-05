using SubtitleEdit.Plugins.FrenchTypography.Logic;
using SubtitleEdit.Plugins.FrenchTypography.ViewModels;
using SubtitleEdit.Plugins.Shared;
using System.Text.Json;

namespace FrenchTypography.Tests;

public class MainViewModelTests
{
    private const string S = " ";

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
        var vm = new MainViewModel(Request(PluginRequestType.Run, null, "Quoi?", "Bien.", "Non!"));
        Assert.Equal(2, vm.ChangeCount);
        vm.Changes[1].IsSelected = false;

        var response = vm.BuildResponse();
        var paragraphs = response.Subtitle!.Paragraphs!;
        Assert.Equal(new[] { "Quoi" + S + "?", "Bien.", "Non!" }, paragraphs.Select(p => p.Text));
        Assert.Equal("Actor0", paragraphs[0].Actor);
        Assert.Equal("French typography", response.UndoDescription);
    }

    [Fact]
    public void Run_SelectedLinesOnly()
    {
        var request = Request(PluginRequestType.Run, null, "Quoi?", "Non!");
        request.SelectedIndices = new List<int> { 1 };
        var vm = new MainViewModel(request);
        Assert.Equal(new[] { 1 }, vm.Changes.Select(c => c.Index));
    }

    [Fact]
    public void Run_SwitchingToCanada_Reanalyzes()
    {
        var vm = new MainViewModel(Request(PluginRequestType.Run, null, "Quoi ?", "Quoi?"));
        Assert.Equal(new[] { 0, 1 }, vm.Changes.Select(c => c.Index));

        vm.IsCanada = true;
        Assert.Equal(new[] { 0 }, vm.Changes.Select(c => c.Index));
        Assert.Equal("Quoi?", vm.Changes[0].After);
    }

    [Fact]
    public void Run_NothingToChange_IsClean()
    {
        var vm = new MainViewModel(Request(PluginRequestType.Run, null, "Quoi" + S + "?", "Plain"));
        Assert.True(vm.IsClean);
        Assert.False(vm.CanApply);
    }

    [Fact]
    public void Batch_UsesSavedOptions_WithoutSettingsInResponse()
    {
        var settings = """{"variant":"france","space":"narrow","convertQuotes":false,"convertEllipsis":false}""";
        var response = new MainViewModel(Request(PluginRequestType.Batch, settings, "Il dit \"oui\"... Quoi?")).BuildBatchResponse();
        Assert.Equal(PluginStatus.Ok, response.Status);
        Assert.Equal("Il dit \"oui\"... Quoi ?", response.Subtitle!.Paragraphs![0].Text);
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
    public void Configure_ReturnsOptionsWithoutSubtitle()
    {
        var vm = new MainViewModel(Request(PluginRequestType.Configure, null));
        Assert.True(vm.CanApply);
        vm.IsCanada = true;
        var response = vm.BuildResponse();
        Assert.Equal(PluginStatus.Ok, response.Status);
        Assert.Null(response.Subtitle);
        Assert.Equal("canada", response.Settings!.Value.GetProperty("variant").GetString());
    }

    [Theory]
    [InlineData("French", true)]
    [InlineData("fr-FR", true)]
    [InlineData("Français", true)]
    [InlineData("English", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void UiLanguage_PicksFrenchOrEnglish(string? uiLanguage, bool french) =>
        Assert.Equal(french, Strings.IsFrench(uiLanguage));

    [Fact]
    public void French_UiLanguage_LocalizesMessages()
    {
        var request = Request(PluginRequestType.Run, null, "Quoi?");
        request.UiLanguage = "French";
        var response = new MainViewModel(request).BuildResponse();
        Assert.Equal("Typographie française", response.UndoDescription);
        Assert.Equal("Typographie française" + S + ": 1 ligne modifiée.", response.Message);
    }
}
