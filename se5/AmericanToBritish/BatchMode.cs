using SubtitleEdit.Plugins.Shared;
using System;
using System.Collections.Generic;

namespace SubtitleEdit.Plugins.AmericanToBritish;

/// <summary>
/// Subtitle Edit's Batch convert: a "batch" request converts every line headless with the bundled
/// word list plus the local word list (the plugin's only settings), a "configure" request has no
/// settings to return - the local word list is saved to its own file.
/// </summary>
public static class BatchMode
{
    public const string WordListFileName = "AmericanToBritish.xml";

    public static PluginResponse BuildBatchResponse(PluginRequest request) =>
        BuildBatchResponse(request, LocalWordList.GetDefaultPath(WordListFileName));

    /// <summary>
    /// Converts all lines, like the window with every proposal checked (its default). No changes is
    /// "ok" without a subtitle. An unreadable local word list is an error rather than silently
    /// converting words the user asked to ignore.
    /// </summary>
    public static PluginResponse BuildBatchResponse(PluginRequest request, string wordListPath)
    {
        LocalWordList localWordList;
        try
        {
            localWordList = LocalWordList.Load(wordListPath);
        }
        catch (Exception ex)
        {
            return new PluginResponse
            {
                Status = PluginStatus.Error,
                Message = $"American to British: could not read the local word list {wordListPath}: {ex.Message}",
            };
        }

        var blocks = SubRipParser.Parse(request.Subtitle.SubRip);
        var converter = new EnglishVariantConverter(EnglishVariantDirection.UsToBr, localWordList);
        var changesByLine = new Dictionary<int, string>();
        for (var i = 0; i < blocks.Count; i++)
        {
            if (converter.TryConvert(blocks[i].Text, out var converted))
            {
                changesByLine[i] = converted;
            }
        }

        if (changesByLine.Count == 0)
        {
            return new PluginResponse { Status = PluginStatus.Ok, Message = "American to British: nothing to convert." };
        }

        return BuildOkResponse(blocks, changesByLine);
    }

    /// <summary>A "configure" request: nothing to return, the local word list is saved by its own window.</summary>
    public static PluginResponse BuildConfigureResponse() =>
        new() { Status = PluginStatus.Ok, Message = "American to British uses the bundled and local word lists." };

    /// <summary>The "ok" response with the given lines converted.</summary>
    internal static PluginResponse BuildOkResponse(List<SrtBlock> blocks, IReadOnlyDictionary<int, string> changesByLine)
    {
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
            Message = $"Converted {changesByLine.Count} line(s) to British English.",
            UndoDescription = "American to British",
            Subtitle = new PluginSubtitle
            {
                Format = "SubRip",
                Native = SubRipParser.Serialize(blocks),
            },
        };
    }
}
