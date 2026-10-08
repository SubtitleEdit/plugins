using CommunityToolkit.Mvvm.ComponentModel;

namespace SubtitleEdit.Plugins.ItalicEachLine;

/// <summary>One subtitle line whose italic tags change.</summary>
public sealed partial class ChangeItem : ObservableObject
{
    [ObservableProperty] private bool _isSelected = true;

    public ChangeItem(int index, string before, string after, string timeRange)
    {
        Index = index;
        Before = before;
        After = after;
        TimeRange = timeRange;
        (BeforeSegments, AfterSegments) = TextDiff.Compute(before.Replace("\r\n", "\n"), after.Replace("\r\n", "\n"));
    }

    public int Index { get; }
    public string Before { get; }
    public string After { get; }
    public string Number => "#" + (Index + 1);
    public string TimeRange { get; }
    public IReadOnlyList<DiffSegment> BeforeSegments { get; }
    public IReadOnlyList<DiffSegment> AfterSegments { get; }
}
