using CommunityToolkit.Mvvm.ComponentModel;
using SubtitleEdit.Plugins.HIColorer.Controls;

namespace SubtitleEdit.Plugins.HIColorer.ViewModels;

/// <summary>One subtitle line whose hearing-impaired text is colored.</summary>
public sealed partial class ChangeItem : ObservableObject
{
    [ObservableProperty] public partial bool IsSelected { get; set; }

    public ChangeItem(int index, string before, string after, string timeRange)
    {
        IsSelected = true;
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
