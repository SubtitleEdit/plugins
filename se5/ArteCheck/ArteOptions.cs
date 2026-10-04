namespace SubtitleEdit.Plugins.ArteCheck;

public enum ArteCheckType
{
    BlankSubtitle,
    FrameAccurateTimeCodes,
    DisplayDuration,
    MaximumTwoLines,
    TeletextLinePosition,
    TeletextColors,
    TeletextLineLength,
    Italic,
    UnneededSpaces,
    MinimumGaps,
}

public sealed class ArteOptions
{
    public const int PresetTeletextMaxCells = 37;
    public const int PresetMinimumGapFrames = 5;
    public const double PresetReadingDurationTolerancePercent = 15.0;
    public const int PresetShortMinimumFrames = 18;

    public ArteProfile Profile { get; set; } = ArteProfile.All[0];

    /// <summary>Frame rate the subtitle was timed for; ARTE always delivers 25 fps.</summary>
    public double SourceFrameRate { get; set; } = 25.0;

    public bool ShiftToStartTimeCode { get; set; }
    public double TargetStartMs { get; set; }

    /// <summary>Working limit of visible characters per row; the encoded row is always 40 cells.</summary>
    public int TeletextMaxCells { get; set; } = PresetTeletextMaxCells;

    public int MinimumGapFrames { get; set; } = PresetMinimumGapFrames;
    public double ReadingDurationTolerancePercent { get; set; } = PresetReadingDurationTolerancePercent;
    public bool AcceptShortDurations { get; set; }
    public int ShortMinimumFrames { get; set; } = PresetShortMinimumFrames;

    public double MinimumDurationMs { get; set; } = 1000;
    public double MaximumDurationMs { get; set; } = 8000;
    public double MaximumCharactersPerSecond { get; set; } = 25;
    public bool DoubleHeight { get; set; } = true;

    public HashSet<ArteCheckType> EnabledChecks { get; set; } = Enum.GetValues<ArteCheckType>().ToHashSet();

    public bool IsPreset =>
        TeletextMaxCells == PresetTeletextMaxCells &&
        MinimumGapFrames == PresetMinimumGapFrames &&
        Math.Abs(ReadingDurationTolerancePercent - PresetReadingDurationTolerancePercent) < 0.001 &&
        !AcceptShortDurations &&
        ShortMinimumFrames == PresetShortMinimumFrames;

    public void ApplyPreset()
    {
        TeletextMaxCells = PresetTeletextMaxCells;
        MinimumGapFrames = PresetMinimumGapFrames;
        ReadingDurationTolerancePercent = PresetReadingDurationTolerancePercent;
        AcceptShortDurations = false;
        ShortMinimumFrames = PresetShortMinimumFrames;
    }
}
