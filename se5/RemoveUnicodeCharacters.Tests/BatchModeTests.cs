using SubtitleEdit.Plugins.RemoveUnicodeCharacters;
using SubtitleEdit.Plugins.Shared;
using System.Text.Json;

namespace RemoveUnicodeCharacters.Tests;

/// <summary>Subtitle Edit Batch convert: "batch" applies the batch list headless; Configure edits that list.</summary>
public class BatchModeTests
{
    private static JsonElement Json(string json) => JsonDocument.Parse(json).RootElement.Clone();

    private static PluginRequest Request(string? settingsJson, params string[] texts) => new()
    {
        RequestType = PluginRequestType.Batch,
        Subtitle = new PluginSubtitle
        {
            Format = "Advanced Sub Station Alpha",
            Paragraphs = texts.Select((t, i) => new PluginParagraph { StartMs = i * 1000, EndMs = i * 1000 + 900, Text = t, Style = "Top" }).ToList(),
        },
        Settings = settingsJson == null ? null : Json(settingsJson),
    };

    [Fact]
    public void Batch_NoSettings_ReplacesOnlyBuiltInDefaults()
    {
        var response = BatchMode.BuildBatchResponse(Request(null, "♪ Привет ♫", "don’t", "Plain"));

        Assert.Equal(PluginStatus.Ok, response.Status);
        Assert.Equal(new[] { "# Привет #", "don’t", "Plain" }, response.Subtitle!.Paragraphs!.Select(p => p.Text));
        Assert.Equal("Advanced Sub Station Alpha", response.Subtitle.Format);
        Assert.All(response.Subtitle.Paragraphs!, p => Assert.Equal("Top", p.Style));
        Assert.Null(response.Settings);
        Assert.Null(response.SettingsVersion);
    }

    [Fact]
    public void Batch_NothingToChange_ReturnsOkWithoutSubtitle()
    {
        var response = BatchMode.BuildBatchResponse(Request(null, "Привет", "café"));

        Assert.Equal(PluginStatus.Ok, response.Status);
        Assert.Null(response.Subtitle);
    }

    [Fact]
    public void Batch_NotConfigured_UsesNonBlankWindowReplacementsOnly()
    {
        // Blank window entries are also saved for unchecked rows - they must not remove anything in batch.
        var settings = "{\"replacements\":{\"2019\":\"'\",\"0410\":\"\",\"266A\":\"\"}}";

        var response = BatchMode.BuildBatchResponse(Request(settings, "don’t А ♪ ♫"));

        Assert.Equal("don't А ♪ #", response.Subtitle!.Paragraphs![0].Text);
    }

    [Fact]
    public void Batch_ConfiguredList_ReplacesListedAndKeepsOthers()
    {
        var settings = "{\"replacements\":{\"2019\":\"'\"},\"batchReplacements\":{\"200B\":\"\",\"2014\":\"-\"},\"batchRemoveOthers\":false}";

        var response = BatchMode.BuildBatchResponse(Request(settings, "a​b — don’t ♪"));

        Assert.Equal("ab - don’t ♪", response.Subtitle!.Paragraphs![0].Text);
        Assert.Contains("2 ", response.Message);
    }

    [Fact]
    public void Batch_RemoveOthers_RemovesUnlistedNonAnsi()
    {
        var settings = "{\"batchReplacements\":{\"266A\":\"#\"},\"batchRemoveOthers\":true}";

        var response = BatchMode.BuildBatchResponse(Request(settings, "♪ Привет café ♪"));

        Assert.Equal("#  café #", response.Subtitle!.Paragraphs![0].Text);
    }

    [Fact]
    public void Batch_LegacyRequestWithoutParagraphs_ReturnsSubRip()
    {
        var request = new PluginRequest
        {
            RequestType = PluginRequestType.Batch,
            Subtitle = new PluginSubtitle { Format = "SubRip", SubRip = "1\n00:00:01,000 --> 00:00:02,000\n♪ Hi\n\n" },
        };

        var response = BatchMode.BuildBatchResponse(request);

        Assert.Equal("SubRip", response.Subtitle!.Format);
        Assert.Contains("# Hi", response.Subtitle.Native);
    }

    [Fact]
    public void ConfigureSettings_KeepWindowReplacements()
    {
        var list = new SortedDictionary<char, string> { ['​'] = "" };
        var settings = BatchMode.BuildSettings(new Dictionary<string, string> { ["2019"] = "'" }, null, list, removeOthers: true);

        Assert.Equal("'", settings.GetProperty("replacements").GetProperty("2019").GetString());
        Assert.Equal("", settings.GetProperty("batchReplacements").GetProperty("200B").GetString());
        Assert.True(settings.GetProperty("batchRemoveOthers").GetBoolean());
        Assert.Equal(new[] { '​' }, BatchMode.LoadBatchList(settings).Keys);
    }

    [Fact]
    public void RunSettings_KeepBatchConfiguration()
    {
        var previous = Json("{\"replacements\":{},\"batchReplacements\":{\"2014\":\"-\"},\"batchRemoveOthers\":true}");

        var settings = BatchMode.BuildSettings(new Dictionary<string, string> { ["266A"] = "*" }, previous);

        Assert.Equal("*", settings.GetProperty("replacements").GetProperty("266A").GetString());
        Assert.Equal("-", settings.GetProperty("batchReplacements").GetProperty("2014").GetString());
        Assert.True(BatchMode.LoadRemoveOthers(settings));
    }

    [Fact]
    public void ParseCharacters_AcceptsGlyphsAndCodes()
    {
        Assert.Equal(new[] { '—', '​', '♪' }, BatchMode.ParseCharacters("—abc U+200B u+266a ♪ U+0041"));
    }
}
