namespace SubtitleEdit.Plugins.LinesUnbreaker.Logic;

public sealed class UnbreakOptions
{
    public const int DefaultMaxLength = 43;

    /// <summary>Join only when the result, without tags, is at most this many characters.</summary>
    public int MaxLength { get; set; } = DefaultMaxLength;
    public bool SkipDialogs { get; set; } = true;
    public bool SkipMoods { get; set; }
    public bool SkipNarrator { get; set; } = true;
}
