using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SubtitleEdit.Plugins.PersianErrors;

/// <summary>One changed subtitle line in the fix list.</summary>
public sealed partial class FixItem : ObservableObject
{
    [ObservableProperty] private bool _isSelected = true;

    public FixItem(LineFix fix, string timeRange, IReadOnlyList<GroupOption> groups)
    {
        Fix = fix;
        TimeRange = timeRange;
        Groups = groups;
        (BeforeSegments, AfterSegments) = TextDiff.Compute(fix.Before, fix.After);
    }

    public LineFix Fix { get; }
    public int Index => Fix.Index;
    public string Number => "#" + (Fix.Index + 1);
    public string TimeRange { get; }
    public IReadOnlyList<GroupOption> Groups { get; }
    public IBrush? AccentBrush => Groups.Count > 0 ? Groups[0].Brush : null;
    public IReadOnlyList<DiffSegment> BeforeSegments { get; }
    public IReadOnlyList<DiffSegment> AfterSegments { get; }

    public bool Matches(string search) =>
        Fix.Before.Contains(search, StringComparison.OrdinalIgnoreCase) ||
        Fix.After.Contains(search, StringComparison.OrdinalIgnoreCase) ||
        Number.Equals(search, StringComparison.Ordinal) ||
        (Fix.Index + 1).ToString() == search;
}
