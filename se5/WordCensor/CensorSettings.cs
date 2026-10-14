using System;
using System.Text.Json;

namespace SubtitleEdit.Plugins.WordCensor;

/// <summary>The saved plugin settings (mode, replacement text, red highlight) shared by the window and Batch convert.</summary>
public static class CensorSettings
{
    public const string DefaultReplacement = "[censored]";
    public const int SettingsVersion = 1;

    public static CensorOptions Load(JsonElement? settings)
    {
        var options = new CensorOptions { Mode = CensorMode.Grawlix, Replacement = DefaultReplacement, ColorRed = false };

        if (settings is null || settings.Value.ValueKind != JsonValueKind.Object)
        {
            return options;
        }

        var root = settings.Value;

        if (root.TryGetProperty("mode", out var modeProp) && modeProp.ValueKind == JsonValueKind.String &&
            Enum.TryParse<CensorMode>(modeProp.GetString(), ignoreCase: true, out var parsed))
        {
            options.Mode = parsed;
        }
        if (root.TryGetProperty("replacement", out var rep) && rep.ValueKind == JsonValueKind.String)
        {
            var s = rep.GetString();
            if (!string.IsNullOrEmpty(s))
            {
                options.Replacement = s;
            }
        }
        if (root.TryGetProperty("colorRed", out var cr) && cr.ValueKind == JsonValueKind.True)
        {
            options.ColorRed = true;
        }

        return options;
    }

    public static JsonElement Build(CensorOptions options)
    {
        var dto = new
        {
            mode = options.Mode.ToString(),
            replacement = options.Replacement,
            colorRed = options.ColorRed,
        };
        var json = JsonSerializer.Serialize(dto);
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.Clone();
    }
}
