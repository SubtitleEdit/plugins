using SubtitleEdit.Plugins.Shared;

namespace SubtitleEdit.Plugins.SplitDialogs;

/// <summary>Subtitle Edit's Batch convert: "batch" splits headless, "configure" returns the settings only.</summary>
public static class BatchMode
{
    /// <summary>
    /// A "batch" request: split every dialog line in the file with the saved options, without
    /// UI - what the window would do with its default "all rows checked". No dialogs is "ok"
    /// without a subtitle.
    /// </summary>
    public static PluginResponse BuildBatchResponse(PluginRequest request)
    {
        var settings = SplitSettings.Load(request);
        var document = CueDocument.Create(request.Subtitle);
        var splits = new Dictionary<int, IReadOnlyList<SplitPart>>();
        foreach (var cue in document.Cues)
        {
            if (!cue.CanSplit)
            {
                continue;
            }

            var texts = DialogSplitter.Split(cue.Lines, settings.RequireSentenceEnding);
            if (texts != null)
            {
                splits[cue.Index] = SplitTiming.Distribute(cue.StartMs, cue.EndMs, texts, settings.GapMs);
            }
        }

        if (splits.Count == 0)
        {
            return new PluginResponse { Status = PluginStatus.Ok, Message = "Split dialogs: no dialog lines found." };
        }

        var lineCount = splits.Values.Sum(p => p.Count);
        return new PluginResponse
        {
            Status = PluginStatus.Ok,
            Message = $"Split {splits.Count} dialog line(s) into {lineCount} lines.",
            UndoDescription = "Split dialogs",
            Subtitle = new PluginSubtitle
            {
                Format = document.FormatName,
                Native = document.Build(splits),
            },
        };
    }

    /// <summary>A "configure" request: hand back the settings only, for the next Batch convert run.</summary>
    public static PluginResponse BuildConfigureResponse(SplitSettings settings) =>
        new() { Status = PluginStatus.Ok, Settings = settings.ToJson(), SettingsVersion = SplitSettings.Version };
}
