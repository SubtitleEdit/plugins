using SubtitleEdit.Plugins.Shared;
using System;

namespace SubtitleEdit.Plugins.WordCensor;

/// <summary>
/// Subtitle Edit's Batch convert ("batch" request): censor every line with the saved settings,
/// without UI. All lines are changed, like the window's default where every change is checked.
/// </summary>
public static class BatchMode
{
    public static PluginResponse BuildBatchResponse(PluginRequest request)
    {
        var options = CensorSettings.Load(request.Settings);
        var blocks = SubRipParser.Parse(request.Subtitle.SubRip);

        // Grawlix and "random alternative" are random by design. Seed from the file's text so a
        // batch run is reproducible: the same file with the same settings gives the same output.
        var engine = new WordCensorEngine(StableSeed(request.Subtitle.SubRip));

        var changed = 0;
        foreach (var block in blocks)
        {
            if (engine.TryCensor(block.Text, options, out var censored))
            {
                block.Text = censored;
                changed++;
            }
        }

        if (changed == 0)
        {
            return new PluginResponse { Status = PluginStatus.Ok, Message = "Word censor: no offensive words found." };
        }

        return new PluginResponse
        {
            Status = PluginStatus.Ok,
            Message = $"Censored words in {changed} line(s).",
            UndoDescription = "Word censor",
            Subtitle = new PluginSubtitle
            {
                Format = "SubRip",
                Native = SubRipParser.Serialize(blocks),
            },
        };
    }

    /// <summary>FNV-1a over the text (string.GetHashCode is randomized per process).</summary>
    private static int StableSeed(string? text)
    {
        unchecked
        {
            var hash = 2166136261u;
            foreach (var c in text ?? string.Empty)
            {
                hash = (hash ^ c) * 16777619u;
            }
            return (int)hash;
        }
    }
}
