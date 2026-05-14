using System.Text.Json;

namespace SubtitleEdit.Plugins.Haxor;

// These types mirror the Subtitle Edit 5 plugin JSON contract.
// See https://github.com/SubtitleEdit/subtitleedit/blob/main/docs/plugin.md

/// <summary>Written by Subtitle Edit; its path is passed as the first command-line argument.</summary>
public sealed class PluginRequest
{
    public int ApiVersion { get; set; } = 1;
    public string RequestType { get; set; } = "run";

    /// <summary>Where this plugin must write its <see cref="PluginResponse"/>.</summary>
    public string ResponseFilePath { get; set; } = string.Empty;

    /// <summary>A scratch directory; deleted by Subtitle Edit after the run.</summary>
    public string TempDirectory { get; set; } = string.Empty;

    public PluginSubtitle Subtitle { get; set; } = new();

    /// <summary>Zero-based indices of the lines selected in the grid (empty if none).</summary>
    public List<int> SelectedIndices { get; set; } = new();

    public string VideoFileName { get; set; } = string.Empty;
    public double FrameRate { get; set; }
    public string UiLanguage { get; set; } = string.Empty;
    public string Theme { get; set; } = string.Empty;
    public string SeVersion { get; set; } = string.Empty;

    /// <summary>This plugin's settings as last persisted by Subtitle Edit (null on first run).</summary>
    public JsonElement? Settings { get; set; }
}

public sealed class PluginSubtitle
{
    /// <summary>Friendly format name, e.g. "SubRip".</summary>
    public string Format { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;

    /// <summary>Full subtitle text in <see cref="Format"/>.</summary>
    public string Native { get; set; } = string.Empty;

    /// <summary>Full subtitle text as SubRip (.srt) - always provided in requests.</summary>
    public string SubRip { get; set; } = string.Empty;
}

/// <summary>Written by this plugin to <see cref="PluginRequest.ResponseFilePath"/>.</summary>
public sealed class PluginResponse
{
    public int ApiVersion { get; set; } = 1;

    /// <summary>"ok": apply the subtitle; "cancelled": do nothing; "error": show the message.</summary>
    public string Status { get; set; } = "cancelled";

    public string? Message { get; set; }

    /// <summary>The modified subtitle. Only <see cref="PluginSubtitle.Format"/> and <see cref="PluginSubtitle.Native"/> are read.</summary>
    public PluginSubtitle? Subtitle { get; set; }

    /// <summary>Settings to persist; handed back unchanged in the next request.</summary>
    public JsonElement? Settings { get; set; }

    public string? UndoDescription { get; set; }
}
