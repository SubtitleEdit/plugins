using System.Text.Json;

namespace SubtitleEdit.Plugins.TypewriterEffect;

// Mirrors the Subtitle Edit 5 plugin JSON contract.
// See https://github.com/SubtitleEdit/subtitleedit/blob/main/docs/plugin.md

public sealed class PluginRequest
{
    public int ApiVersion { get; set; } = 1;
    public string RequestType { get; set; } = "run";
    public string ResponseFilePath { get; set; } = string.Empty;
    public string TempDirectory { get; set; } = string.Empty;
    public PluginSubtitle Subtitle { get; set; } = new();
    public List<int> SelectedIndices { get; set; } = new();
    public string VideoFileName { get; set; } = string.Empty;
    public double FrameRate { get; set; }
    public string UiLanguage { get; set; } = string.Empty;
    public string Theme { get; set; } = string.Empty;
    public string SeVersion { get; set; } = string.Empty;
    public JsonElement? Settings { get; set; }
}

public sealed class PluginSubtitle
{
    public string Format { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string Native { get; set; } = string.Empty;
    public string SubRip { get; set; } = string.Empty;
}

public sealed class PluginResponse
{
    public int ApiVersion { get; set; } = 1;
    public string Status { get; set; } = "cancelled";
    public string? Message { get; set; }
    public PluginSubtitle? Subtitle { get; set; }
    public JsonElement? Settings { get; set; }
    public string? UndoDescription { get; set; }
}
