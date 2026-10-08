using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SubtitleEdit.Plugins.PersianErrors;

/// <summary>A rule group as shown in the sidebar: on/off, icon, color, example and current fix count.</summary>
public sealed partial class GroupOption : ObservableObject
{
    [ObservableProperty] private bool _isEnabled = true;
    [ObservableProperty] private int _fixCount;

    public GroupOption(RuleGroup group, GroupInfo info)
    {
        Group = group;
        Info = info;
        var color = Color.Parse(info.Color);
        Brush = new SolidColorBrush(color);
        Tint = new SolidColorBrush(Color.FromArgb(0x26, color.R, color.G, color.B));

        if (info.Example.Length > 0)
        {
            var after = PersianFixer.Fix(info.Example, new[] { group });
            (ExampleBefore, ExampleAfter) = TextDiff.Compute(info.Example, after);
        }
    }

    public RuleGroup Group { get; }
    public GroupInfo Info { get; }
    public string Name => Group.Name;
    public string PersianName => Info.PersianName;
    public bool HasPersianName => Info.PersianName.Length > 0;
    public string Description => Info.Description;
    public string Icon => Info.Icon;
    public IBrush Brush { get; }
    public IBrush Tint { get; }
    public string RuleCountText => Group.Rules.Count == 1 ? "1 rule" : $"{Group.Rules.Count:N0} rules";
    public IReadOnlyList<DiffSegment> ExampleBefore { get; } = Array.Empty<DiffSegment>();
    public IReadOnlyList<DiffSegment> ExampleAfter { get; } = Array.Empty<DiffSegment>();
    public bool HasExample => ExampleAfter.Count > 0;
    public bool HasFixes => FixCount > 0;

    partial void OnFixCountChanged(int value) => OnPropertyChanged(nameof(HasFixes));
}

/// <summary>A filter chip above the fix list: everything, or the fixes of one group.</summary>
public sealed partial class FilterChip : ObservableObject
{
    [ObservableProperty] private bool _isActive;

    public FilterChip(GroupOption? group, int count)
    {
        Group = group;
        Count = count;
    }

    public GroupOption? Group { get; }
    public int Count { get; }
    public string Label => Group?.Name ?? "All";
    public string Icon => Group?.Icon ?? "mdi-format-list-checks";
    public IBrush? Brush => Group?.Brush;
    public bool IsGroup => Group != null;
}
