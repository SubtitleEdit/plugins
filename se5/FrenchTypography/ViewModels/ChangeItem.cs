using CommunityToolkit.Mvvm.ComponentModel;
using SubtitleEdit.Plugins.FrenchTypography.Controls;

namespace SubtitleEdit.Plugins.FrenchTypography.ViewModels;

/// <summary>One subtitle line whose punctuation is fixed.</summary>
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
        (BeforeSegments, AfterSegments) = TextDiff.Compute(ForDisplay(before), ForDisplay(after));
    }

    public int Index { get; }
    public string Before { get; }
    public string After { get; }
    public string Number => "#" + (Index + 1);
    public string TimeRange { get; }
    public IReadOnlyList<DiffSegment> BeforeSegments { get; }
    public IReadOnlyList<DiffSegment> AfterSegments { get; }

    /// <summary>No-break spaces are invisible, so the preview shows them as a middle dot.</summary>
    private static string ForDisplay(string text) =>
        text.Replace("\r\n", "\n").Replace('\u00A0', '·').Replace('\u202F', '·');
}
