using SubtitleEdit.Plugins.Shared;
using System.Text.Json;

namespace SubtitleEdit.Plugins.SplitDialogs;

/// <summary>The two remembered options, shared by the window and Batch convert.</summary>
public sealed record SplitSettings(bool RequireSentenceEnding, int GapMs)
{
    public const int Version = 1;
    public const bool DefaultRequireSentenceEnding = true;
    public const int DefaultGapMs = 24; // Subtitle Edit's default "minimum gap between lines"

    public static SplitSettings Default => new(DefaultRequireSentenceEnding, DefaultGapMs);

    public JsonElement ToJson() => JsonSerializer.SerializeToElement(new
    {
        requireSentenceEnding = RequireSentenceEnding,
        gapMs = GapMs,
    });

    public static SplitSettings Load(PluginRequest request)
    {
        var requireSentenceEnding = DefaultRequireSentenceEnding;
        var gapMs = DefaultGapMs;
        var settings = request.Settings;
        if (request.SettingsVersion != Version || settings is not { ValueKind: JsonValueKind.Object })
        {
            return new SplitSettings(requireSentenceEnding, gapMs);
        }

        if (settings.Value.TryGetProperty("requireSentenceEnding", out var require) &&
            require.ValueKind is JsonValueKind.True or JsonValueKind.False)
        {
            requireSentenceEnding = require.GetBoolean();
        }

        if (settings.Value.TryGetProperty("gapMs", out var gap) && gap.TryGetInt32(out var value) && value is >= 0 and <= 2000)
        {
            gapMs = value;
        }

        return new SplitSettings(requireSentenceEnding, gapMs);
    }
}
