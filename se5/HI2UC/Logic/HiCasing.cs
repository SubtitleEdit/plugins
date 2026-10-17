using SubtitleEdit.Plugins.HI2UC.Logic.Strategies;

namespace SubtitleEdit.Plugins.HI2UC.Logic;

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
