using System.Text;

namespace SubtitleEdit.Plugins.Haxor;

/// <summary>Lower-cases text and swaps a handful of letters for their "haxor" look-alikes.</summary>
public static class HaxorTranslator
{
    private static readonly Dictionary<char, char> Map = new()
    {
        ['a'] = '4',
        ['c'] = '©',
        ['e'] = '3',
        ['h'] = 'H',
        ['i'] = '!',
        ['k'] = 'K',
        ['n'] = 'ñ',
        ['o'] = '0',
        ['s'] = '$',
        ['y'] = '¥',
    };

    public static string Translate(string text)
    {
        var sb = new StringBuilder(text.ToLowerInvariant());
        for (var i = 0; i < sb.Length; i++)
        {
            if (Map.TryGetValue(sb[i], out var mapped))
            {
                sb[i] = mapped;
            }
        }

        return sb.ToString();
    }
}
