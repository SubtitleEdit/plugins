using SubtitleEdit.Plugins.BritishToAmerican;
using SubtitleEdit.Plugins.Shared;

namespace BritishToAmerican.Tests;

/// <summary>Subtitle Edit Batch convert: "batch" converts every line headless, "configure" returns no settings.</summary>
public class BatchModeTests
{
    private static PluginRequest Request(string requestType, params string[] texts) => new()
    {
        RequestType = requestType,
        Subtitle = new PluginSubtitle
        {
            Format = "SubRip",
            SubRip = SubRipParser.Serialize(texts.Select((t, i) => new SrtBlock { StartMs = i * 1000, EndMs = i * 1000 + 900, Text = t }).ToList()),
        },
    };

    [Fact]
    public void Batch_ConvertsAllLines()
    {
        var response = Conversion.BuildBatchResponse(Request(PluginRequestType.Batch, "What a lovely colour.", "Plain line", "My favourite car."));

        Assert.Equal(PluginStatus.Ok, response.Status);
        Assert.Null(response.Settings);
        Assert.Null(response.SettingsVersion);
        var texts = SubRipParser.Parse(response.Subtitle!.Native).Select(b => b.Text).ToList();
        Assert.Equal(new[] { "What a lovely color.", "Plain line", "My favorite car." }, texts);
    }

    [Fact]
    public void Batch_KeepsTimings()
    {
        var response = Conversion.BuildBatchResponse(Request(PluginRequestType.Batch, "Plain", "colour"));

        var blocks = SubRipParser.Parse(response.Subtitle!.Native);
        Assert.Equal(2, blocks.Count);
        Assert.Equal(1000, blocks[1].StartMs);
        Assert.Equal(1900, blocks[1].EndMs);
    }

    [Fact]
    public void Batch_IgnoresSelectedIndices()
    {
        var request = Request(PluginRequestType.Batch, "colour", "colour");
        request.SelectedIndices.Add(0);

        var response = Conversion.BuildBatchResponse(request);

        Assert.Equal(new[] { "color", "color" }, SubRipParser.Parse(response.Subtitle!.Native).Select(b => b.Text));
    }

    [Fact]
    public void Batch_NothingToConvert_ReturnsOkWithoutSubtitle()
    {
        var response = Conversion.BuildBatchResponse(Request(PluginRequestType.Batch, "Plain", "Get in the car."));

        Assert.Equal(PluginStatus.Ok, response.Status);
        Assert.Null(response.Subtitle);
    }

    [Fact]
    public void Batch_EmptySubtitle_ReturnsOkWithoutSubtitle()
    {
        var response = Conversion.BuildBatchResponse(Request(PluginRequestType.Batch));

        Assert.Equal(PluginStatus.Ok, response.Status);
        Assert.Null(response.Subtitle);
    }

    [Fact]
    public void Run_SelectedLinesOnly()
    {
        var blocks = SubRipParser.Parse(Request(PluginRequestType.Run, "colour", "colour").Subtitle.SubRip);

        var proposals = Conversion.FindChanges(blocks, new[] { 1 });

        Assert.Equal(1, Assert.Single(proposals).LineIndex);
    }

    [Fact]
    public void Run_UncheckedProposalsAreNotApplied()
    {
        var blocks = SubRipParser.Parse(Request(PluginRequestType.Run, "colour", "honour").Subtitle.SubRip);
        var proposals = Conversion.FindChanges(blocks, Array.Empty<int>());
        proposals[0].Include = false;

        var response = Conversion.Apply(blocks, proposals);

        Assert.Equal(new[] { "colour", "honor" }, SubRipParser.Parse(response!.Subtitle!.Native).Select(b => b.Text));
        Assert.Null(Conversion.Apply(blocks, proposals.Select(p => { p.Include = false; return p; }).ToList()));
    }

    [Fact]
    public void Configure_ReturnsOkWithoutSettingsOrSubtitle()
    {
        var response = Conversion.BuildConfigureResponse();

        Assert.Equal(PluginStatus.Ok, response.Status);
        Assert.Null(response.Subtitle);
        Assert.Null(response.Settings);
    }
}
