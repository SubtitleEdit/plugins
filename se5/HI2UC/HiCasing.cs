namespace SubtitleEdit.Plugins.HI2UC;

public sealed class HiOptions
{
    public HiStyle Style { get; set; } = HiStyle.UpperCase;
    public bool Moods { get; set; } = true;
    public bool Narrator { get; set; } = true;
}

/// <summary>The result for one subtitle text; <see cref="Unbalanced"/> flags mismatched ()/[] that may need a manual look.</summary>
public sealed record HiResult(string After, bool MoodsChanged, bool NarratorChanged, bool Unbalanced)
{
    public string Comment
    {
        get
        {
            var parts = new List<string>();
            if (MoodsChanged)
            {
                parts.Add("Moods");
            }

            if (NarratorChanged)
            {
                parts.Add("Narrator");
            }

            if (Unbalanced)
            {
                parts.Add("Unbalanced ( ) or [ ] - check this line");
            }

            return string.Join("  ·  ", parts);
        }
    }
}

public static class HiCasing
{
    /// <summary>Cases moods first, then the narrator label, on the same text.</summary>
    public static HiResult Convert(string text, HiOptions options)
    {
        var strategy = CasingStrategies.Create(options.Style);
        var after = text;

        var moodsChanged = false;
        if (options.Moods)
        {
            var moods = new MoodCasingConverter(strategy).Convert(after);
            moodsChanged = moods != after;
            after = moods;
        }

        var narratorChanged = false;
        if (options.Narrator)
        {
            var narrator = new NarratorCasingConverter(strategy).Convert(after);
            narratorChanged = narrator != after;
            after = narrator;
        }

        var unbalanced = options.Moods && !MoodCasingConverter.HasBalancedParentheses(text);
        return new HiResult(after, moodsChanged, narratorChanged, unbalanced);
    }
}
