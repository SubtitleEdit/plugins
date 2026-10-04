using CommunityToolkit.Mvvm.ComponentModel;
using SubtitleEdit.Plugins.Shared;

namespace SubtitleEdit.Plugins.ArteCheck;

public enum ArteFixKind
{
    None,
    Header,
    ShiftStartTimeCode,
    CreateBlankSubtitle,
    FrameAccurateTimeCode,
    DisplayDuration,
    MinimumGap,
    TeletextLinePosition,
    Rebalance,
    Split,
    TeletextColor,
    RemoveItalic,
    UnneededSpaces,
}

/// <summary>One finding: a correction that can be applied, or an alarm that needs a manual edit.</summary>
public sealed partial class ArteFix : ObservableObject
{
    [ObservableProperty] private bool _apply;

    public ArteFix(string group, bool canBeFixed, int index, string before, string after, string reason,
        ArteFixKind kind = ArteFixKind.None, bool applyByDefault = true)
    {
        Group = group;
        CanBeFixed = canBeFixed;
        Index = index;
        Before = before;
        After = after;
        Reason = reason;
        Kind = kind;
        _apply = canBeFixed && applyByDefault;
    }

    public string Group { get; }
    public bool CanBeFixed { get; }
    public bool IsAlarm => !CanBeFixed;

    /// <summary>1-based subtitle number; 0 for file-level findings (header, start time code, ...).</summary>
    public int Index { get; }

    public string IndexDisplay => Index > 0 ? "#" + Index : "File";
    public string Before { get; }
    public string After { get; }
    public bool HasAfter => !string.IsNullOrEmpty(After);
    public string Reason { get; }
    public ArteFixKind Kind { get; }
    public string TimeRange { get; set; } = string.Empty;
    public bool HasTimeRange => !string.IsNullOrEmpty(TimeRange);

    /// <summary>Before/after are subtitle text (shown as a teletext preview) rather than time codes or header values.</summary>
    public bool IsTextChange => Kind is ArteFixKind.Rebalance or ArteFixKind.TeletextColor or ArteFixKind.RemoveItalic or ArteFixKind.UnneededSpaces ||
                                (IsAlarm && Group is ArteChecker.GroupLayout or ArteChecker.GroupColors);

    public bool IsPlainChange => !IsTextChange;
    public string SeverityIcon => CanBeFixed ? "mdi-wrench-outline" : "mdi-alert-octagon-outline";

    internal string? ProposedHeader { get; init; }
    internal double? ProposedStartMs { get; init; }
    internal double? ProposedEndMs { get; init; }
    internal PluginParagraph? ProposedParagraph { get; init; }
    internal List<PluginParagraph>? SplitParagraphs { get; init; }
}
