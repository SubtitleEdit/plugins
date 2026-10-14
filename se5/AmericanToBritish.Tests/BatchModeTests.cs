using SubtitleEdit.Plugins.AmericanToBritish;
using SubtitleEdit.Plugins.Shared;

namespace AmericanToBritish.Tests;

/// <summary>Subtitle Edit Batch convert: "batch" converts every line headless, "configure" returns no settings.</summary>
public sealed class BatchModeTests : IDisposable
{
    private readonly string _folder = Path.Combine(Path.GetTempPath(), "AmericanToBritishTests-" + Guid.NewGuid().ToString("N"));

    public BatchModeTests() => Directory.CreateDirectory(_folder);

    public void Dispose() => Directory.Delete(_folder, true);

    private string WordListPath => Path.Combine(_folder, BatchMode.WordListFileName);

    private static PluginRequest Request(params string[] texts)
    {
        var blocks = texts.Select((t, i) => new SrtBlock { StartMs = i * 1000, EndMs = i * 1000 + 900, Text = t }).ToList();
        return new PluginRequest
        {
            RequestType = PluginRequestType.Batch,
            Subtitle = new PluginSubtitle { Format = "SubRip", SubRip = SubRipParser.Serialize(blocks) },
        };
    }

    private static List<string> Texts(PluginResponse response) =>
        SubRipParser.Parse(response.Subtitle!.Native).Select(b => b.Text).ToList();

    [Fact]
    public void Batch_ConvertsAllLines()
    {
        var response = BatchMode.BuildBatchResponse(Request("What color is it?", "Plain text", "I like your humor."), WordListPath);

        Assert.Equal(PluginStatus.Ok, response.Status);
        Assert.Equal(new[] { "What colour is it?", "Plain text", "I like your humour." }, Texts(response));
        Assert.Null(response.Settings);
        Assert.Null(response.SettingsVersion);
    }

    [Fact]
    public void Batch_KeepsTimings()
    {
        var response = BatchMode.BuildBatchResponse(Request("Plain", "color"), WordListPath);

        var blocks = SubRipParser.Parse(response.Subtitle!.Native);
        Assert.Equal(new long[] { 0, 1000 }, blocks.Select(b => b.StartMs));
        Assert.Equal(new long[] { 900, 1900 }, blocks.Select(b => b.EndMs));
    }

    [Fact]
    public void Batch_IgnoresSelectedIndices()
    {
        var request = Request("color", "humor");
        request.SelectedIndices = new List<int> { 0 };

        var response = BatchMode.BuildBatchResponse(request, WordListPath);

        Assert.Equal(new[] { "colour", "humour" }, Texts(response));
    }

    [Fact]
    public void Batch_NothingToConvert_ReturnsOkWithoutSubtitle()
    {
        var response = BatchMode.BuildBatchResponse(Request("Plain text", "Already colour"), WordListPath);

        Assert.Equal(PluginStatus.Ok, response.Status);
        Assert.Null(response.Subtitle);
    }

    [Fact]
    public void Batch_UsesLocalWordList()
    {
        File.WriteAllText(WordListPath, """
            <?xml version="1.0" encoding="utf-8"?>
            <Words>
              <Word us="dumb" br="stupid" />
              <Ignore us="color" />
            </Words>
            """);

        var response = BatchMode.BuildBatchResponse(Request("That is dumb", "What color is it?", "humor"), WordListPath);

        Assert.Equal(new[] { "That is stupid", "What color is it?", "humour" }, Texts(response));
    }

    [Fact]
    public void Batch_UnreadableLocalWordList_ReturnsError()
    {
        File.WriteAllText(WordListPath, "<Words><Word");

        var response = BatchMode.BuildBatchResponse(Request("color"), WordListPath);

        Assert.Equal(PluginStatus.Error, response.Status);
        Assert.Null(response.Subtitle);
        Assert.Contains(BatchMode.WordListFileName, response.Message);
    }

    [Fact]
    public void Configure_ReturnsOkWithoutSubtitleOrSettings()
    {
        var response = BatchMode.BuildConfigureResponse();

        Assert.Equal(PluginStatus.Ok, response.Status);
        Assert.Null(response.Subtitle);
        Assert.Null(response.Settings);
    }
}
