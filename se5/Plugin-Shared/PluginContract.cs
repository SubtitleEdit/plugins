using System.Collections.Generic;
using System.Text.Json;

namespace SubtitleEdit.Plugins.Shared;

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

    /// <summary>Read-only snapshot of the user's subtitle rules. Null on older SE versions.</summary>
    public PluginRules? Rules { get; set; }

    public JsonElement? Settings { get; set; }

    /// <summary>
    /// Schema version this plugin attached to <see cref="Settings"/> in its last response.
    /// Null on first run, when settings were saved without a version, or on older SE versions.
    /// Use it to migrate or reset settings written by an older build of this plugin.
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

    /// <summary>The subtitle in its own format. Empty for binary formats (EBU STL, PAC, ...).</summary>
    public string Native { get; set; } = string.Empty;

    public string SubRip { get; set; } = string.Empty;

    /// <summary>
    /// In-memory header: the EBU STL GSI block (1024 characters), ASSA [Script Info]/styles, ...
    /// Null on older SE versions. In a response, null keeps SE's current header.
    /// </summary>
    public string? Header { get; set; }

    /// <summary>
    /// The lines exactly as SE holds them. Null on older SE versions. In a response, non-empty
    /// paragraphs win over <see cref="Native"/>: SE applies them as-is and keeps its format.
    /// </summary>
    public List<PluginParagraph>? Paragraphs { get; set; }
}

/// <summary>One subtitle line with every field SE keeps (not only what survives SubRip).</summary>
public sealed class PluginParagraph
{
    public double StartMs { get; set; }
    public double EndMs { get; set; }
    public string Text { get; set; } = string.Empty;
    public string? Style { get; set; }
    public string? Actor { get; set; }
    public string? Language { get; set; }
    public string? Region { get; set; }
    public string? Effect { get; set; }
    public string? Extra { get; set; }
    public string? MarginL { get; set; }
    public string? MarginR { get; set; }

    /// <summary>For EBU STL / DVB Teletext: the teletext row (vertical position) the line starts on.</summary>
    public string? MarginV { get; set; }

    public int Layer { get; set; }
    public bool IsComment { get; set; }
    public bool Forced { get; set; }
    public bool NewSection { get; set; }
    public string? Bookmark { get; set; }

    public PluginParagraph Clone() => (PluginParagraph)MemberwiseClone();
}

/// <summary>Read-only snapshot of the user's Subtitle Edit rules.</summary>
public sealed class PluginRules
{
    public int SubtitleMinimumDisplayMilliseconds { get; set; }
    public int SubtitleMaximumDisplayMilliseconds { get; set; }
    public double SubtitleMaximumCharactersPerSeconds { get; set; }
    public double SubtitleOptimalCharactersPerSeconds { get; set; }
    public int SubtitleLineMaximumLength { get; set; }
    public int MaxNumberOfLines { get; set; }
    public int MinimumMillisecondsBetweenLines { get; set; }
    public int MinimumFramesBetweenLines { get; set; }
    public bool UseFrameMode { get; set; }
    public bool EbuStlTeletextUseBox { get; set; }
    public bool EbuStlTeletextUseDoubleHeight { get; set; }

    /// <summary>Paragraph times are video-relative; the grid and the saved file show time + this offset.</summary>
    public long VideoOffsetMs { get; set; }
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
    /// settings so you can detect (and migrate or reset) stale data on the next run via
    /// <see cref="PluginRequest.SettingsVersion"/>. Optional; null = "unversioned".
    /// </summary>
    public int? SettingsVersion { get; set; }

    public string? UndoDescription { get; set; }
}

public static class PluginStatus
{
    public const string Ok = "ok";
    public const string Cancelled = "cancelled";
    public const string Error = "error";
}
