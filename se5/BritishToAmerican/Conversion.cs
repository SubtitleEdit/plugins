using SubtitleEdit.Plugins.Shared;
using System.Collections.Generic;
using System.Linq;

namespace SubtitleEdit.Plugins.BritishToAmerican;

/// <summary>Finds and applies British-to-American changes; shared by the window and Batch convert.</summary>
public static class Conversion
{
    /// <summary>A proposal for every line in scope (all lines when nothing is selected) that would change.</summary>
    public static List<ChangeProposal> FindChanges(IReadOnlyList<SrtBlock> blocks, IReadOnlyCollection<int> selectedIndices)
    {
        var converter = new EnglishVariantConverter(EnglishVariantDirection.BrToUs);
        var selected = new HashSet<int>(selectedIndices);
        var applyToAll = selected.Count == 0;
        var proposals = new List<ChangeProposal>();

        for (var i = 0; i < blocks.Count; i++)
        {
            if (!applyToAll && !selected.Contains(i))
            {
                continue;
            }

            if (converter.TryConvert(blocks[i].Text, out var converted))
            {
                proposals.Add(new ChangeProposal(i, blocks[i].Text, converted));
            }
        }

        return proposals;
    }

    /// <summary>
    /// Writes the included proposals into <paramref name="blocks"/> and returns the "ok" response,
    /// or null when no proposal is included.
    /// </summary>
    public static PluginResponse? Apply(List<SrtBlock> blocks, IEnumerable<ChangeProposal> proposals)
    {
        var changesByLine = proposals
            .Where(p => p.Include)
            .ToDictionary(p => p.LineIndex, p => p.ConvertedText);

        if (changesByLine.Count == 0)
        {
            return null;
        }

        for (var i = 0; i < blocks.Count; i++)
        {
            if (changesByLine.TryGetValue(i, out var newText))
            {
                blocks[i].Text = newText;
            }
        }

        return new PluginResponse
        {
            Status = PluginStatus.Ok,
            Message = $"Converted {changesByLine.Count} line(s) to American English.",
            UndoDescription = "British to American",
            Subtitle = new PluginSubtitle
            {
                Format = "SubRip",
                Native = SubRipParser.Serialize(blocks),
            },
        };
    }

    /// <summary>
    /// A "batch" request (Batch convert): convert every line, exactly what the window preselects
    /// (all proposals are checked by default), without UI. No changes is "ok" without a subtitle.
    /// </summary>
    public static PluginResponse BuildBatchResponse(PluginRequest request)
    {
        var blocks = SubRipParser.Parse(request.Subtitle.SubRip);
        var proposals = FindChanges(blocks, new List<int>());
        return Apply(blocks, proposals)
            ?? new PluginResponse { Status = PluginStatus.Ok, Message = "British to American: nothing to convert." };
    }

    /// <summary>A "configure" request: British to American has no settings, so OK returns none.</summary>
    public static PluginResponse BuildConfigureResponse() =>
        new() { Status = PluginStatus.Ok, Message = "British to American has no settings." };
}
