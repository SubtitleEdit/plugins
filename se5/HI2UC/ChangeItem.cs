using CommunityToolkit.Mvvm.ComponentModel;

namespace SubtitleEdit.Plugins.HI2UC;

/// <summary>One subtitle line whose hearing-impaired text changes case.</summary>
public sealed partial class ChangeItem : ObservableObject
{
    [ObservableProperty] private bool _isSelected = true;

    public ChangeItem(int index, string before, string after, string timeRange, string comment, bool isWarning)
    {
        Index = index;
        Before = before;
        After = after;
        TimeRange = timeRange;
        Comment = comment;
        IsWarning = isWarning;
        (BeforeSegments, AfterSegments) = TextDiff.Compute(before.Replace("\r\n", "\n"), after.Replace("\r\n", "\n"));
    }

    public int Index { get; }
    public string Before { get; }
    public string After { get; }
    public string Number => "#" + (Index + 1);
    public string TimeRange { get; }
    public string Comment { get; }
    public bool IsWarning { get; }
    public IReadOnlyList<DiffSegment> BeforeSegments { get; }
    public IReadOnlyList<DiffSegment> AfterSegments { get; }
}
