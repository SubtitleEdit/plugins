using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SubtitleEdit.Plugins.ArteCheck;

public sealed partial class FixGroup : ObservableObject
{
    [ObservableProperty] private bool _isExpanded;

    public FixGroup(string name, IReadOnlyList<ArteFix> items, bool isExpanded)
    {
        Name = name;
        Items = items;
        _isExpanded = isExpanded;
        Icon = IconFor(name);
    }

    public string Name { get; }
    public string Icon { get; }
    public IReadOnlyList<ArteFix> Items { get; }
    public int FixCount => Items.Count(i => i.CanBeFixed);
    public int AlarmCount => Items.Count(i => i.IsAlarm);
    public bool HasFixes => FixCount > 0;
    public bool HasAlarms => AlarmCount > 0;

    [RelayCommand]
    private void SelectAll()
    {
        foreach (var item in Items.Where(i => i.CanBeFixed))
        {
            item.Apply = true;
        }
    }

    [RelayCommand]
    private void SelectNone()
    {
        foreach (var item in Items)
        {
            item.Apply = false;
        }
    }

    public static string IconFor(string group) => group switch
    {
        ArteChecker.GroupHeader => "mdi-file-cog-outline",
        ArteChecker.GroupStartTimeCode => "mdi-clock-start",
        ArteChecker.GroupBlank => "mdi-checkbox-blank-outline",
        ArteChecker.GroupFrames => "mdi-filmstrip",
        ArteChecker.GroupDuration => "mdi-timer-outline",
        ArteChecker.GroupLayout => "mdi-call-split",
        ArteChecker.GroupPosition => "mdi-format-vertical-align-bottom",
        ArteChecker.GroupColors => "mdi-palette-outline",
        ArteChecker.GroupItalic => "mdi-format-italic",
        ArteChecker.GroupSpaces => "mdi-keyboard-space",
        ArteChecker.GroupGaps => "mdi-arrow-expand-horizontal",
        _ => "mdi-alert-circle-outline",
    };
}

public sealed partial class CheckItem : ObservableObject
{
    [ObservableProperty] private bool _isEnabled = true;

    public CheckItem(ArteCheckType type, string name, string description, string icon)
    {
        Type = type;
        Name = name;
        Description = description;
        Icon = icon;
    }

    public ArteCheckType Type { get; }
    public string Name { get; }
    public string Description { get; }
    public string Icon { get; }

    public static IReadOnlyList<CheckItem> CreateAll() => new[]
    {
        new CheckItem(ArteCheckType.BlankSubtitle, "Blank control subtitle", "5-frame blank subtitle at the programme start", FixGroup.IconFor(ArteChecker.GroupBlank)),
        new CheckItem(ArteCheckType.FrameAccurateTimeCodes, "Frame-accurate time codes", "Whole 25 fps frames", FixGroup.IconFor(ArteChecker.GroupFrames)),
        new CheckItem(ArteCheckType.DisplayDuration, "Display duration", "Reading time, minimum and maximum", FixGroup.IconFor(ArteChecker.GroupDuration)),
        new CheckItem(ArteCheckType.MaximumTwoLines, "Maximum two lines", "Rebalance or split longer subtitles", "mdi-format-line-spacing"),
        new CheckItem(ArteCheckType.TeletextLinePosition, "Teletext row", "Rows 22 / 20 with double height", FixGroup.IconFor(ArteChecker.GroupPosition)),
        new CheckItem(ArteCheckType.TeletextColors, "Teletext colors", "Yellow or uncolored, SDH: 8 colors", FixGroup.IconFor(ArteChecker.GroupColors)),
        new CheckItem(ArteCheckType.TeletextLineLength, "Characters per row", "Visible characters + color cells", "mdi-ruler"),
        new CheckItem(ArteCheckType.Italic, "No italics", "Teletext cannot show italics", FixGroup.IconFor(ArteChecker.GroupItalic)),
        new CheckItem(ArteCheckType.UnneededSpaces, "Unneeded spaces", "Leading and trailing spaces", FixGroup.IconFor(ArteChecker.GroupSpaces)),
        new CheckItem(ArteCheckType.MinimumGaps, "Minimum gaps", "Frames between subtitles", FixGroup.IconFor(ArteChecker.GroupGaps)),
    };
}
