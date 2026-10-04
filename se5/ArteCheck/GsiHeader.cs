using System.Globalization;

namespace SubtitleEdit.Plugins.ArteCheck;

/// <summary>
/// The EBU STL GSI block as Subtitle Edit holds it: a 1024-character string in a single-byte
/// code page, so every field sits at its fixed byte offset (EBU Tech 3264).
/// </summary>
public sealed class GsiHeader
{
    public const int Length = 1024;

    private char[] _chars;

    private GsiHeader(string text) => _chars = text.ToCharArray();

    public static bool IsStlHeader(string? header)
    {
        if (header is null || header.Length != Length)
        {
            return false;
        }

        return int.TryParse(header.AsSpan(0, 3), NumberStyles.None, CultureInfo.InvariantCulture, out _) &&
               header.AsSpan(3, 3).SequenceEqual("STL");
    }

    public static GsiHeader Parse(string header) => new(header);

    /// <summary>Subtitle Edit's default header (the one written for a file without a GSI block).</summary>
    public static GsiHeader CreateDefault()
    {
        var text =
            "437" + "STL25.01" + "0" + "00" + "0A" +
            "No Title".PadRight(32) + new string(' ', 32) + new string(' ', 32) + new string(' ', 32) +
            new string(' ', 32) + new string(' ', 32) + "0".PadRight(16) +
            "101021" + "101021" + "01" + "00725" + "00725" + "001" + "40" + "23" + "1" +
            "00000000" + "00000001" + "1" + "1" + "USA" +
            new string(' ', 32) + new string(' ', 32) + new string(' ', 32) +
            new string(' ', 75) + new string(' ', 576);
        return new GsiHeader(text);
    }

    public string CodePage { get => Get(0, 3); set => Set(0, 3, value); }
    public string DiskFormatCode { get => Get(3, 8); set => Set(3, 8, value); }
    public string DisplayStandardCode { get => Get(11, 1); set => Set(11, 1, value); }
    public string CharacterCodeTable { get => Get(12, 2); set => Set(12, 2, value); }
    public string LanguageCode { get => Get(14, 2); set => Set(14, 2, value); }
    public string MaxCharactersPerRow { get => Get(251, 2); set => Set(251, 2, value); }
    public string MaxRows { get => Get(253, 2); set => Set(253, 2, value); }

    /// <summary>TCP - time code start of programme, HHMMSSFF.</summary>
    public string TimeCodeStartOfProgramme { get => Get(256, 8); set => Set(256, 8, value); }

    /// <summary>The fields that differ from <paramref name="other"/>, one per line, e.g. "Language: 08".</summary>
    public string DescribeDifferences(GsiHeader other)
    {
        var mine = Describe().Split('\n');
        var theirs = other.Describe().Split('\n');
        return string.Join("\n", mine.Where((line, i) => line != theirs[i]));
    }

    public string Describe() =>
        $"Code page: {CodePage}\nDisk format: {DiskFormatCode}\nDisplay standard: {DisplayStandardCode}\n" +
        $"Character table: {CharacterCodeTable}\nLanguage: {LanguageCode}\n" +
        $"Characters per row: {MaxCharactersPerRow}\nRows: {MaxRows}";

    public GsiHeader Clone() => new(ToString());

    public override string ToString() => new(_chars);

    private string Get(int offset, int length) => new(_chars, offset, length);

    private void Set(int offset, int length, string value)
    {
        var padded = value.Length >= length ? value[..length] : value.PadRight(length);
        padded.CopyTo(0, _chars, offset, length);
    }
}
