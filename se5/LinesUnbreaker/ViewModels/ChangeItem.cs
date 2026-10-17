using CommunityToolkit.Mvvm.ComponentModel;
using SubtitleEdit.Plugins.LinesUnbreaker.Controls;
using SubtitleEdit.Plugins.LinesUnbreaker.Logic;

namespace SubtitleEdit.Plugins.LinesUnbreaker.ViewModels;

/// <summary>One subtitle whose lines are joined into one.</summary>
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
        Length = LineUnbreaker.VisibleLength(after);
        (BeforeSegments, AfterSegments) = TextDiff.Compute(before.Replace("\r\n", "\n"), after.Replace("\r\n", "\n"));
    }

    public int Index { get; }
    public string Before { get; }
    public string After { get; }
    public string Number => "#" + (Index + 1);
    public string TimeRange { get; }
    public int Length { get; }
    public string LengthText => $"{Length} characters";
    public IReadOnlyList<DiffSegment> BeforeSegments { get; }
    public IReadOnlyList<DiffSegment> AfterSegments { get; }
}
