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

    /// <summary>Total video duration in seconds. Null when no video is loaded or on older SE versions.</summary>
    public double? VideoDurationSeconds { get; set; }

    /// <summary>Video frame width in pixels. Null when no video is loaded or on older SE versions.</summary>
    public int? VideoWidth { get; set; }

    /// <summary>Video frame height in pixels. Null when no video is loaded or on older SE versions.</summary>
    public int? VideoHeight { get; set; }

    public string UiLanguage { get; set; } = string.Empty;
    public string Theme { get; set; } = string.Empty;

    /// <summary>Active theme's colors so the plugin's UI can match Subtitle Edit. Null on older SE versions.</summary>
    public PluginThemeColors? ThemeColors { get; set; }

    public string SeVersion { get; set; } = string.Empty;
    public JsonElement? Settings { get; set; }

    /// <summary>
    /// Schema version this plugin attached to <see cref="Settings"/> in its last response.
    /// Null on first run, when settings were saved without a version, or on older SE versions.
    /// </summary>
    public int? SettingsVersion { get; set; }
}

/// <summary>Active theme colors. All values are <c>#AARRGGBB</c> hex strings.</summary>
public sealed class PluginThemeColors
{
    public bool IsDark { get; set; }
    public string BackgroundColor { get; set; } = string.Empty;
    public string ForegroundColor { get; set; } = string.Empty;
    public string AccentColor { get; set; } = string.Empty;
    public string BackgroundColorLighter { get; set; } = string.Empty;
    public string BackgroundColorHeader { get; set; } = string.Empty;
    public string BookmarkColor { get; set; } = string.Empty;
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

    /// <summary>
    /// Schema version for <see cref="Settings"/>. Bump when you change the shape of your
    /// settings so you can migrate or reset on the next run. Optional; null = "unversioned".
    /// </summary>
    public int? SettingsVersion { get; set; }

    public string? UndoDescription { get; set; }
}
