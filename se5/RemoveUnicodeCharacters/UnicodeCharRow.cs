using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Linq;

namespace SubtitleEdit.Plugins.RemoveUnicodeCharacters;

public partial class UnicodeCharRow : ObservableObject
{
    [ObservableProperty] private bool _include = true;
    [ObservableProperty] private string _replacement = string.Empty;

    public char Character { get; }

    /// <summary>Unicode hex code as a four-digit "U+XXXX" string.</summary>
    public string HexCode { get; }

    /// <summary>The character itself, rendered as a glyph.</summary>
    public string Glyph { get; }

    /// <summary>1-based line numbers where the character occurs, joined by ", ".</summary>
    public string LinesText { get; }

    /// <summary>Total number of occurrences across all lines.</summary>
    public int Count { get; }

    public IReadOnlyList<int> LineIndices { get; }

    public UnicodeCharRow(char character, int count, IEnumerable<int> lineIndices, string replacement)
    {
        Character = character;
        HexCode = "U+" + ((int)character).ToString("X4");
        Glyph = character.ToString();
        Count = count;
        LineIndices = lineIndices.OrderBy(i => i).ToArray();
        LinesText = string.Join(", ", LineIndices.Select(i => (i + 1).ToString()));
        _replacement = replacement;
    }
}
