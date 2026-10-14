using SubtitleEdit.Plugins.Shared;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace SubtitleEdit.Plugins.RemoveUnicodeCharacters;

/// <summary>
/// Subtitle Edit Batch convert support. The window asks per character, so a batch run only
/// touches the characters in the <i>batch list</i> (edited via Configure...). Until the list has
/// been configured it holds the built-in defaults plus every non-blank replacement typed in the
/// window. Other non-ANSI characters are kept unless "remove all others" is switched on.
/// </summary>
public static class BatchMode
{
    public const int SettingsVersion = 1;

    /// <summary>Window replacements (char hex → replacement), saved by every normal run.</summary>
    public const string ReplacementsKey = "replacements";

    /// <summary>Batch-only list (char hex → replacement); written by Configure, kept but ignored by a normal run.</summary>
    public const string BatchReplacementsKey = "batchReplacements";

    /// <summary>Batch-only: also remove non-ANSI characters that are not in the batch list.</summary>
    public const string BatchRemoveOthersKey = "batchRemoveOthers";

    /// <summary>Built-in default replacements (used when the user has no persisted setting for a character).</summary>
    public static readonly IReadOnlyDictionary<char, string> DefaultReplacements = new Dictionary<char, string>
    {
        ['♪'] = "#",
        ['♫'] = "#",
    };

    public static bool IsNonAnsi(char c) => c > 255;

    public static string ToHex(char c) => ((int)c).ToString("X4");

    public static Dictionary<string, string> LoadReplacements(JsonElement? settings, string key = ReplacementsKey)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (settings is null || settings.Value.ValueKind != JsonValueKind.Object)
        {
            return result;
        }

        if (!settings.Value.TryGetProperty(key, out var replacements)
            || replacements.ValueKind != JsonValueKind.Object)
        {
            return result;
        }

        foreach (var property in replacements.EnumerateObject())
        {
            if (property.Value.ValueKind == JsonValueKind.String)
            {
                result[property.Name] = property.Value.GetString() ?? string.Empty;
            }
        }

        return result;
    }

    public static bool LoadRemoveOthers(JsonElement? settings) =>
        settings is { ValueKind: JsonValueKind.Object } s
        && s.TryGetProperty(BatchRemoveOthersKey, out var value)
        && value.ValueKind == JsonValueKind.True;

    /// <summary>
    /// The characters a batch run replaces. Configured list if there is one, else the built-in
    /// defaults plus the window's non-blank replacements (blank ones are also saved for rows the
    /// user left unchecked, so they are no proof the user wants that character removed).
    /// </summary>
    public static SortedDictionary<char, string> LoadBatchList(JsonElement? settings)
    {
        var result = new SortedDictionary<char, string>();
        if (settings is { ValueKind: JsonValueKind.Object } s && s.TryGetProperty(BatchReplacementsKey, out var configured)
            && configured.ValueKind == JsonValueKind.Object)
        {
            foreach (var (hex, replacement) in LoadReplacements(settings, BatchReplacementsKey))
            {
                if (TryParseHex(hex, out var c))
                {
                    result[c] = replacement;
                }
            }

            return result;
        }

        foreach (var (c, replacement) in DefaultReplacements)
        {
            result[c] = replacement;
        }

        foreach (var (hex, replacement) in LoadReplacements(settings))
        {
            if (TryParseHex(hex, out var c))
            {
                if (replacement.Length > 0)
                {
                    result[c] = replacement;
                }
                else
                {
                    result.Remove(c); // the user cleared a built-in default
                }
            }
        }

        return result;
    }

    private static bool TryParseHex(string hex, out char c)
    {
        c = default;
        if (int.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var code) && code is > 255 and <= 0xFFFF)
        {
            c = (char)code;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Characters to add to the batch list from what the user typed: each non-ANSI character as
    /// is, and "U+XXXX" codes for characters that are hard to type (e.g. U+200B zero-width space).
    /// </summary>
    public static List<char> ParseCharacters(string? input)
    {
        var result = new List<char>();
        if (string.IsNullOrEmpty(input))
        {
            return result;
        }

        for (var i = 0; i < input.Length; i++)
        {
            if ((input[i] == 'U' || input[i] == 'u') && i + 1 < input.Length && input[i + 1] == '+')
            {
                var end = i + 2;
                while (end < input.Length && end - (i + 2) < 4 && Uri.IsHexDigit(input[end]))
                {
                    end++;
                }

                if (end - (i + 2) == 4 && TryParseHex(input.Substring(i + 2, 4), out var coded))
                {
                    if (!result.Contains(coded))
                    {
                        result.Add(coded);
                    }

                    i = end - 1;
                    continue;
                }
            }

            if (IsNonAnsi(input[i]) && !result.Contains(input[i]))
            {
                result.Add(input[i]);
            }
        }

        return result;
    }

    /// <summary>Replaces the listed characters (and removes the rest if <paramref name="removeOthers"/>).</summary>
    public static string Apply(string text, IReadOnlyDictionary<char, string> list, bool removeOthers, ref int occurrences)
    {
        StringBuilder? sb = null;
        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];
            string? replacement = null;
            if (IsNonAnsi(c))
            {
                if (list.TryGetValue(c, out var listed))
                {
                    replacement = listed;
                }
                else if (removeOthers)
                {
                    replacement = string.Empty;
                }
            }

            if (replacement == null)
            {
                sb?.Append(c);
                continue;
            }

            sb ??= new StringBuilder(text.Length).Append(text, 0, i);
            sb.Append(replacement);
            occurrences++;
        }

        return sb?.ToString() ?? text;
    }

    /// <summary>
    /// A "batch" request (Batch convert): apply the batch list to all lines, without UI.
    /// Nothing to change is "ok" without a subtitle.
    /// </summary>
    public static PluginResponse BuildBatchResponse(PluginRequest request)
    {
        var list = LoadBatchList(request.Settings);
        var removeOthers = LoadRemoveOthers(request.Settings);
        var occurrences = 0;
        var changedLines = 0;

        PluginSubtitle subtitle;
        if (request.Subtitle.Paragraphs is { } paragraphs)
        {
            var result = new List<PluginParagraph>(paragraphs.Count);
            foreach (var paragraph in paragraphs)
            {
                var copy = paragraph.Clone();
                var text = paragraph.Text ?? string.Empty;
                copy.Text = Apply(text, list, removeOthers, ref occurrences);
                if (!ReferenceEquals(copy.Text, text))
                {
                    changedLines++;
                }

                result.Add(copy);
            }

            subtitle = new PluginSubtitle { Format = request.Subtitle.Format, Paragraphs = result };
        }
        else
        {
            var blocks = SubRipParser.Parse(request.Subtitle.SubRip);
            foreach (var block in blocks)
            {
                var text = block.Text;
                block.Text = Apply(text, list, removeOthers, ref occurrences);
                if (!ReferenceEquals(block.Text, text))
                {
                    changedLines++;
                }
            }

            subtitle = new PluginSubtitle { Format = "SubRip", Native = SubRipParser.Serialize(blocks) };
        }

        if (occurrences == 0)
        {
            return new PluginResponse { Status = PluginStatus.Ok, Message = "Remove Unicode characters: nothing to change." };
        }

        return new PluginResponse
        {
            Status = PluginStatus.Ok,
            Message = $"Replaced {occurrences} Unicode character occurrence(s) across {changedLines} line(s).",
            UndoDescription = "Remove Unicode characters",
            Subtitle = subtitle,
        };
    }

    /// <summary>
    /// The settings JSON. <paramref name="batchList"/> null keeps the batch keys from
    /// <paramref name="previous"/> (a normal run must not touch the batch configuration).
    /// </summary>
    public static JsonElement BuildSettings(
        IDictionary<string, string> replacements,
        JsonElement? previous,
        IReadOnlyDictionary<char, string>? batchList = null,
        bool removeOthers = false)
    {
        var root = new JsonObject
        {
            [ReplacementsKey] = ToJson(replacements),
        };

        if (batchList != null)
        {
            root[BatchReplacementsKey] = ToJson(batchList.ToDictionary(kvp => ToHex(kvp.Key), kvp => kvp.Value));
            root[BatchRemoveOthersKey] = removeOthers;
        }
        else if (previous is { ValueKind: JsonValueKind.Object } p)
        {
            foreach (var key in new[] { BatchReplacementsKey, BatchRemoveOthersKey })
            {
                if (p.TryGetProperty(key, out var value))
                {
                    root[key] = JsonNode.Parse(value.GetRawText());
                }
            }
        }

        using var doc = JsonDocument.Parse(root.ToJsonString());
        return doc.RootElement.Clone();
    }

    private static JsonObject ToJson(IEnumerable<KeyValuePair<string, string>> map)
    {
        var obj = new JsonObject();
        foreach (var (key, value) in map.OrderBy(kvp => kvp.Key, StringComparer.OrdinalIgnoreCase))
        {
            obj[key] = value;
        }

        return obj;
    }
}
