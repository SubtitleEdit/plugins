using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SubtitleEdit.Plugins.AssaTagCheck;

/// <summary>One problem as a row in a line card.</summary>
public sealed class IssueRow
{
    public IssueRow(Issue issue, RuleOption rule)
    {
        Issue = issue;
        Rule = rule;
    }

    public Issue Issue { get; }
    public RuleOption Rule { get; }
    public string Message => Issue.Message;
    public string RuleName => Rule.Name;
    public string Icon => Rule.Icon;
    public IBrush Brush => Rule.Brush;
    public IBrush Tint => Rule.Tint;
    public bool IsManual => !Issue.Fixable;
}

/// <summary>A subtitle line with problems: what was found, and the fixed text when something could be fixed.</summary>
public sealed partial class LineItem : ObservableObject
{
    [ObservableProperty] private bool _isSelected;

    public LineItem(int index, string timeRange, string before, string? styleBefore, LineResult result, IReadOnlyList<IssueRow> issues)
    {
        Index = index;
        TimeRange = timeRange;
        Before = before;
        After = result.After;
        StyleBefore = styleBefore;
        StyleAfter = result.StyleAfter;
        Issues = issues;
        IsFixable = After != Before || StyleAfter != StyleBefore;
        _isSelected = IsFixable;
        Severity = issues.Min(i => i.Rule.Severity);
        AccentBrush = SeverityLook.Brush(Severity);
        (BeforeSegments, AfterSegments) = TextDiff.Compute(before.Replace("\r\n", "\n"), After.Replace("\r\n", "\n"));
    }

    public int Index { get; }
    public string Number => "#" + (Index + 1);
    public string TimeRange { get; }
    public string Before { get; }
    public string After { get; }
    public string? StyleBefore { get; }
    public string? StyleAfter { get; }
    public bool HasStyle => !string.IsNullOrEmpty(StyleBefore);
    public bool StyleChanged => StyleAfter != StyleBefore;
    public string StyleChangeText => $"{StyleBefore} → {StyleAfter}";
    public IReadOnlyList<IssueRow> Issues { get; }
    public bool IsFixable { get; }
    public bool IsManualOnly => !IsFixable;
    public bool TextChanged => After != Before;
    public bool ShowSingleText => !TextChanged;
    public Severity Severity { get; }
    public IBrush AccentBrush { get; }
    public IReadOnlyList<DiffSegment> BeforeSegments { get; }
    public IReadOnlyList<DiffSegment> AfterSegments { get; }

    public bool Matches(string search) =>
        Before.Contains(search, StringComparison.OrdinalIgnoreCase) ||
        Issues.Any(i => i.Message.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
        (Index + 1).ToString() == search.TrimStart('#');
}
