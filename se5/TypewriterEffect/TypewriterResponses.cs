using System.Globalization;
using System.Text.Json;

namespace SubtitleEdit.Plugins.TypewriterEffect;

/// <summary>
/// Builds the plugin responses. With <c>"batch": true</c> in plugin.json, Subtitle Edit's Batch
/// convert sends requestType "batch" (no UI, every line, saved settings) and "configure" (show
/// the settings without a subtitle and return them). Older SE versions only send "run".
/// </summary>
public static class TypewriterResponses
{
    public const string RequestTypeBatch = "batch";
    public const string RequestTypeConfigure = "configure";
    public const double DefaultEndDelay = 0.5;

    public static bool IsConfigure(PluginRequest request) =>
        string.Equals(request.RequestType, RequestTypeConfigure, StringComparison.OrdinalIgnoreCase);

    public static bool IsBatch(PluginRequest request) =>
        string.Equals(request.RequestType, RequestTypeBatch, StringComparison.OrdinalIgnoreCase);

    /// <summary>The response for OK in the window ("run"): apply to the selected lines (all when none are selected).</summary>
    public static PluginResponse BuildRunResponse(PluginRequest request, double endDelaySec)
    {
        try
        {
            var blocks = SubRipParser.Parse(request.Subtitle.SubRip);
            var selected = new HashSet<int>(request.SelectedIndices);
            var (resultBlocks, changed) = TypewriterEngine.Apply(blocks, selected, endDelaySec);

            return new PluginResponse
            {
                Status = "ok",
                Message = changed == 0
                    ? "No lines changed."
                    : $"Typewriter applied to {changed} line(s).",
                UndoDescription = "Typewriter effect",
                Subtitle = new PluginSubtitle
                {
                    Format = "SubRip",
                    Native = SubRipParser.Serialize(resultBlocks),
                },
                Settings = BuildSettings(endDelaySec),
            };
        }
        catch (Exception ex)
        {
            return new PluginResponse { Status = "error", Message = ex.Message };
        }
    }

    /// <summary>
    /// A "batch" request: apply the effect to every line with the saved end delay, without UI.
    /// No line long enough to split is "ok" without a subtitle. Settings are not returned.
    /// </summary>
    public static PluginResponse BuildBatchResponse(PluginRequest request)
    {
        try
        {
            var endDelaySec = LoadEndDelaySetting(request.Settings, DefaultEndDelay);
            var blocks = SubRipParser.Parse(request.Subtitle.SubRip);
            var (resultBlocks, changed) = TypewriterEngine.Apply(blocks, new HashSet<int>(), endDelaySec);
            if (changed == 0)
            {
                return new PluginResponse { Status = "ok", Message = "Typewriter effect: no lines changed." };
            }

            return new PluginResponse
            {
                Status = "ok",
                Message = $"Typewriter applied to {changed} line(s).",
                UndoDescription = "Typewriter effect",
                Subtitle = new PluginSubtitle
                {
                    Format = "SubRip",
                    Native = SubRipParser.Serialize(resultBlocks),
                },
            };
        }
        catch (Exception ex)
        {
            return new PluginResponse { Status = "error", Message = ex.Message };
        }
    }

    /// <summary>A "configure" request: hand back the settings only, for the next Batch convert run.</summary>
    public static PluginResponse BuildConfigureResponse(double endDelaySec) =>
        new() { Status = "ok", Settings = BuildSettings(endDelaySec) };

    public static double LoadEndDelaySetting(JsonElement? settings, double defaultValue)
    {
        if (settings is null || settings.Value.ValueKind != JsonValueKind.Object)
        {
            return defaultValue;
        }
        if (settings.Value.TryGetProperty("endDelay", out var value) &&
            value.ValueKind == JsonValueKind.Number &&
            value.TryGetDouble(out var parsed))
        {
            return parsed;
        }
        return defaultValue;
    }

    public static JsonElement BuildSettings(double endDelay)
    {
        var json = $"{{\"endDelay\":{endDelay.ToString("0.###", CultureInfo.InvariantCulture)}}}";
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.Clone();
    }
}
