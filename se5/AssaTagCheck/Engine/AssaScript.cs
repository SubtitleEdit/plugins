using System.Globalization;
using System.Text;

namespace SubtitleEdit.Plugins.AssaTagCheck;

/// <summary>One style from [V4+ Styles], with the fields the checker compares against.</summary>
public sealed class AssaStyle
{
    public string Name { get; init; } = string.Empty;
    public string FontName { get; init; } = string.Empty;
    public Dictionary<string, string> Fields { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    public string Field(string name) => Fields.GetValueOrDefault(name) ?? string.Empty;
}

/// <summary>What the checker needs from the script header: resolution, styles and attached fonts.</summary>
public sealed class AssaScript
{
    public static AssaScript Empty { get; } = new();

    public int? PlayResX { get; private set; }
    public int? PlayResY { get; private set; }
    public List<AssaStyle> Styles { get; } = new();

    /// <summary>Family names of the fonts in [Fonts] (read from the font data), plus their file names.</summary>
    public HashSet<string> AttachedFonts { get; } = new(StringComparer.OrdinalIgnoreCase);

    public bool HasStyles => Styles.Count > 0;

    /// <summary>Exact match first (ignoring the leading '*' VSFilter skips), then a case-insensitive one.</summary>
    public AssaStyle? FindStyle(string? name, bool ignoreCase)
    {
        var wanted = (name ?? string.Empty).Trim().TrimStart('*');
        return Styles.FirstOrDefault(s => s.Name.TrimStart('*') == wanted) ??
               (ignoreCase ? Styles.FirstOrDefault(s => s.Name.TrimStart('*').Equals(wanted, StringComparison.OrdinalIgnoreCase)) : null);
    }

    public AssaStyle? DefaultStyle => FindStyle("Default", ignoreCase: true) ?? Styles.FirstOrDefault();

    public static AssaScript Parse(string? header)
    {
        var script = new AssaScript();
        if (string.IsNullOrEmpty(header))
        {
            return script;
        }

        var section = string.Empty;
        string[]? format = null;
        string? fontFile = null;
        var fontData = new StringBuilder();

        void FlushFont()
        {
            if (fontFile == null)
            {
                return;
            }

            script.AttachedFonts.Add(Path.GetFileNameWithoutExtension(fontFile));
            foreach (var family in FontNameReader.ReadFamilyNames(FontNameReader.UuDecode(fontData.ToString())))
            {
                script.AttachedFonts.Add(family);
            }

            fontFile = null;
            fontData.Clear();
        }

        foreach (var rawLine in header.Split('\n'))
        {
            var line = rawLine.TrimEnd('\r');
            var trimmed = line.Trim();
            if (trimmed.StartsWith('[') && trimmed.EndsWith(']'))
            {
                FlushFont();
                section = trimmed.ToLowerInvariant();
                format = null;
                continue;
            }

            if (section == "[script info]")
            {
                if (TryValue(trimmed, "PlayResX", out var x) && int.TryParse(x, NumberStyles.Integer, CultureInfo.InvariantCulture, out var resX))
                {
                    script.PlayResX = resX;
                }
                else if (TryValue(trimmed, "PlayResY", out var y) && int.TryParse(y, NumberStyles.Integer, CultureInfo.InvariantCulture, out var resY))
                {
                    script.PlayResY = resY;
                }
            }
            else if (section.Contains("styles"))
            {
                if (TryValue(trimmed, "Format", out var f))
                {
                    format = f.Split(',').Select(s => s.Trim()).ToArray();
                }
                else if (TryValue(trimmed, "Style", out var s))
                {
                    format ??= DefaultFormat;
                    var values = s.Split(',');
                    var fields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    for (var i = 0; i < format.Length && i < values.Length; i++)
                    {
                        fields[format[i]] = values[i].Trim();
                    }

                    script.Styles.Add(new AssaStyle
                    {
                        Name = fields.GetValueOrDefault("Name") ?? string.Empty,
                        FontName = fields.GetValueOrDefault("Fontname") ?? string.Empty,
                        Fields = fields,
                    });
                }
            }
            else if (section == "[fonts]")
            {
                if (TryValue(trimmed, "fontname", out var name))
                {
                    FlushFont();
                    fontFile = name;
                }
                else if (fontFile != null)
                {
                    fontData.Append(trimmed);
                }
            }
        }

        FlushFont();
        return script;
    }

    private static readonly string[] DefaultFormat =
    {
        "Name", "Fontname", "Fontsize", "PrimaryColour", "SecondaryColour", "OutlineColour", "BackColour", "Bold", "Italic",
        "Underline", "StrikeOut", "ScaleX", "ScaleY", "Spacing", "Angle", "BorderStyle", "Outline", "Shadow", "Alignment",
        "MarginL", "MarginR", "MarginV", "Encoding",
    };

    private static bool TryValue(string line, string key, out string value)
    {
        value = string.Empty;
        if (line.Length > key.Length && line.StartsWith(key, StringComparison.OrdinalIgnoreCase) && line[key.Length] == ':')
        {
            value = line[(key.Length + 1)..].Trim();
            return true;
        }

        return false;
    }
}

/// <summary>Reads the family names out of a font attached in [Fonts], so "arial_B0.ttf" can be matched to "Arial".</summary>
public static class FontNameReader
{
    /// <summary>The [Fonts] encoding: groups of four characters, each 6 bits plus 33.</summary>
    public static byte[] UuDecode(string data)
    {
        var bytes = new List<byte>(data.Length * 3 / 4);
        var i = 0;
        for (; i + 4 <= data.Length; i += 4)
        {
            var v = ((data[i] - 33) << 18) | ((data[i + 1] - 33) << 12) | ((data[i + 2] - 33) << 6) | (data[i + 3] - 33);
            bytes.Add((byte)(v >> 16));
            bytes.Add((byte)(v >> 8));
            bytes.Add((byte)v);
        }

        var rest = data.Length - i;
        if (rest == 2)
        {
            var v = ((data[i] - 33) << 18) | ((data[i + 1] - 33) << 12);
            bytes.Add((byte)(v >> 16));
        }
        else if (rest == 3)
        {
            var v = ((data[i] - 33) << 18) | ((data[i + 1] - 33) << 12) | ((data[i + 2] - 33) << 6);
            bytes.Add((byte)(v >> 16));
            bytes.Add((byte)(v >> 8));
        }

        return bytes.ToArray();
    }

    /// <summary>Family (name id 1) and full names (id 4) from a TrueType/OpenType font or the first font of a collection.</summary>
    public static IReadOnlyList<string> ReadFamilyNames(byte[] font)
    {
        var names = new List<string>();
        try
        {
            var offset = 0;
            if (font.Length >= 16 && Encoding.ASCII.GetString(font, 0, 4) == "ttcf")
            {
                offset = (int)U32(font, 12);
            }

            var tableCount = U16(font, offset + 4);
            for (var t = 0; t < tableCount; t++)
            {
                var record = offset + 12 + t * 16;
                if (Encoding.ASCII.GetString(font, record, 4) != "name")
                {
                    continue;
                }

                var table = (int)U32(font, record + 8);
                var count = U16(font, table + 2);
                var strings = table + U16(font, table + 4);
                for (var n = 0; n < count; n++)
                {
                    var r = table + 6 + n * 12;
                    var platform = U16(font, r);
                    var nameId = U16(font, r + 6);
                    var length = U16(font, r + 8);
                    var start = strings + U16(font, r + 10);
                    if ((nameId != 1 && nameId != 4) || start + length > font.Length)
                    {
                        continue;
                    }

                    var name = platform is 0 or 3
                        ? Encoding.BigEndianUnicode.GetString(font, start, length)
                        : Encoding.Latin1.GetString(font, start, length);
                    if (name.Length > 0 && !names.Contains(name))
                    {
                        names.Add(name);
                    }
                }

                break;
            }
        }
        catch (ArgumentException)
        {
            // Not a font we can read - the file name is still matched.
        }
        catch (IndexOutOfRangeException)
        {
        }

        return names;
    }

    private static int U16(byte[] b, int i) => (b[i] << 8) | b[i + 1];
    private static uint U32(byte[] b, int i) => (uint)((b[i] << 24) | (b[i + 1] << 16) | (b[i + 2] << 8) | b[i + 3]);
}
