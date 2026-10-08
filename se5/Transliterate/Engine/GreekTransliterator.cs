using System.Text;

namespace SubtitleEdit.Plugins.Transliterate.Engine;

/// <summary>
/// Greek → Latin with ELOT 743 (the Greek national standard, as on passports and road signs):
/// αυ/ευ/ηυ = av/ev/iv or af/ef/if, ου = ou, γγ = ng, μπ = b at the start of a word, accents dropped.
/// </summary>
public sealed class GreekTransliterator : Transliterator
{
    private const string VoicelessConsonants = "θκξπστφχψς";

    private static readonly Dictionary<char, string> Letters = new()
    {
        ['α'] = "a", ['β'] = "v", ['γ'] = "g", ['δ'] = "d", ['ε'] = "e", ['ζ'] = "z", ['η'] = "i", ['θ'] = "th",
        ['ι'] = "i", ['κ'] = "k", ['λ'] = "l", ['μ'] = "m", ['ν'] = "n", ['ξ'] = "x", ['ο'] = "o", ['π'] = "p",
        ['ρ'] = "r", ['σ'] = "s", ['ς'] = "s", ['τ'] = "t", ['υ'] = "y", ['φ'] = "f", ['χ'] = "ch", ['ψ'] = "ps",
        ['ω'] = "o",
    };

    public override string Id => "el";
    public override string Name => "Greek";
    public override string NativeName => "Ελληνικά";
    public override string Badge => "EL";
    public override string Script => "Greek";
    public override string Sample => "Καλημέρα! Ευχαριστώ πολύ, Γιώργο. Πού είσαι;";

    public override bool IsNativeLetter(char c) => c is >= 'Ͱ' and <= 'Ͽ' or >= 'ἀ' and <= '῿';

    /// <summary>The Greek question mark ";" (and U+037E) is "?" in Latin text; the ano teleia "·" is ";".</summary>
    public override char MapPunctuation(char c, Direction direction) =>
        direction == Direction.ToLatin ? c switch { ';' or ';' => '?', '·' => ';', _ => c } : c;

    public override string ToLatinWord(string word, int system)
    {
        if (!word.Any(IsNativeLetter))
        {
            return word;
        }

        // Strip accents but remember the diaeresis: "ϊ"/"ϋ" break a diphthong (Ευρώπη = Evropi, but Ταϋγετος = Taygetos).
        var letters = new List<(char Lower, bool Upper, bool Diaeresis, char Original)>();
        foreach (var c in word.Normalize(NormalizationForm.FormD))
        {
            if (c == '̈' && letters.Count > 0)
            {
                var last = letters[^1];
                letters[^1] = last with { Diaeresis = true };
            }
            else if (char.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark)
            {
                letters.Add((char.ToLowerInvariant(c), char.IsUpper(c), false, c));
            }
        }

        var allCaps = letters.Count(l => char.IsLetter(l.Original)) >= 2 && letters.All(l => !char.IsLetter(l.Original) || l.Upper);
        var sb = new StringBuilder();
        for (var i = 0; i < letters.Count; i++)
        {
            var current = letters[i].Lower;
            var next = i + 1 < letters.Count ? letters[i + 1].Lower : '\0';
            var nextNext = i + 2 < letters.Count ? letters[i + 2].Lower : '\0';
            var length = 1;
            string? output;

            if (current is 'α' or 'ε' or 'η' && next == 'υ' && !letters[i + 1].Diaeresis)
            {
                var v = nextNext == '\0' || VoicelessConsonants.Contains(nextNext) ? "f" : "v";
                output = Letters[current] + v;
                length = 2;
            }
            else if (current == 'ο' && next == 'υ' && !letters[i + 1].Diaeresis)
            {
                output = "ou";
                length = 2;
            }
            else if (current == 'γ' && next is 'γ' or 'ξ' or 'χ')
            {
                output = "n" + Letters[next];
                length = 2;
            }
            else if (current == 'μ' && next == 'π' && (i == 0 || i + 2 == letters.Count))
            {
                output = "b";
                length = 2;
            }
            else if (!Letters.TryGetValue(current, out output))
            {
                sb.Append(letters[i].Original);
                continue;
            }

            var source = string.Concat(letters.Skip(i).Take(length).Select(l => l.Upper ? char.ToUpperInvariant(l.Lower) : l.Lower));
            sb.Append(ApplyCase(source, output, allCaps));
            i += length - 1;
        }

        return sb.ToString();
    }
}
