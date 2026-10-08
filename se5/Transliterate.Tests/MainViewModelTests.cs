using SubtitleEdit.Plugins.Shared;
using SubtitleEdit.Plugins.Transliterate;
using SubtitleEdit.Plugins.Transliterate.Engine;
using System.Text.Json;

namespace Transliterate.Tests;

public class MainViewModelTests
{
    private static PluginRequest Request(params string[] texts) => new()
    {
        Subtitle = new PluginSubtitle
        {
            Format = "Advanced Sub Station Alpha",
            Paragraphs = texts.Select((t, i) => new PluginParagraph { StartMs = i * 1000, EndMs = i * 1000 + 900, Text = t, Style = "Top" }).ToList(),
        },
    };

    [Fact]
    public void DetectsLanguageAndListsChanges()
    {
        var vm = new MainViewModel(Request("Ђорђе је ту.", "Hello", "<i>Љубав</i>"));

        Assert.Equal("sr", vm.SelectedLanguage.Language.Id);
        Assert.True(vm.SelectedLanguage.IsDetected);
        Assert.Equal(Direction.ToLatin, vm.Direction);
        Assert.Equal(new[] { 0, 2 }, vm.Changes.Select(c => c.Index));
        Assert.Equal("<i>Ljubav</i>", vm.Changes[1].After);
    }

    [Fact]
    public void SwapAndOneWayLanguages()
    {
        var vm = new MainViewModel(Request("Ђорђе је ту."));
        vm.SwapCommand.Execute(null);
        Assert.Equal(Direction.FromLatin, vm.Direction);
        Assert.Empty(vm.Changes);

        vm.SelectLanguageCommand.Execute(vm.LanguageOptions.First(o => o.Language.Id == "ru"));
        Assert.Equal(Direction.ToLatin, vm.Direction);
        Assert.False(vm.CanSwap);
    }

    [Fact]
    public void SystemChoiceIsRememberedPerLanguage()
    {
        var vm = new MainViewModel(Request("Щука"));
        Assert.Equal("ru", vm.SelectedLanguage.Language.Id);
        Assert.Equal("Shchuka", vm.Changes[0].After);

        vm.SelectSystemCommand.Execute(vm.Systems[1]);
        Assert.Equal("Ŝuka", vm.Changes[0].After);

        var settings = vm.BuildCancelResponse().Settings!.Value;
        var request = Request("Щука");
        request.Settings = settings;
        request.SettingsVersion = vm.BuildCancelResponse().SettingsVersion;
        var again = new MainViewModel(request);
        Assert.Equal(1, again.SystemIndex);
        Assert.Equal("Ŝuka", again.Changes[0].After);
    }

    [Fact]
    public void SavedLanguageWinsOverWeakGuess()
    {
        var request = Request("Это просто текст");
        request.Settings = JsonSerializer.SerializeToElement(new { language = "bg", direction = "ToLatin" });
        request.SettingsVersion = 1;

        Assert.Equal("bg", new MainViewModel(request).SelectedLanguage.Language.Id);
    }

    [Fact]
    public void ResponseKeepsFieldsAndSkipsUnchecked()
    {
        var vm = new MainViewModel(Request("Ђорђе", "Џеп"));
        vm.Changes[1].IsSelected = false;

        var response = vm.BuildResponse();

        Assert.Equal(PluginStatus.Ok, response.Status);
        Assert.Equal(new[] { "Đorđe", "Џеп" }, response.Subtitle!.Paragraphs!.Select(p => p.Text));
        Assert.All(response.Subtitle.Paragraphs!, p => Assert.Equal("Top", p.Style));
    }

    [Fact]
    public void KeepWordsApplyFromLatin()
    {
        var vm = new MainViewModel(Request("Gledaj TV, Đorđe"));
        Assert.Equal(Direction.FromLatin, vm.Direction);
        Assert.Equal("Гледај TV, Ђорђе", vm.Changes[0].After);

        vm.KeepWordsText = "";
        Assert.Equal("Гледај ТВ, Ђорђе", vm.Changes[0].After);
    }
}
