using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SubtitleEdit.Plugins.AssaTagCheck;

/// <summary>Colors and labels per severity.</summary>
public static class SeverityLook
{
    public static Color ColorOf(Severity severity) => severity switch
    {
        Severity.Error => Color.Parse("#E5484D"),
        Severity.Warning => Color.Parse("#E08A00"),
        _ => Color.Parse("#12A594"),
    };

    public static IBrush Brush(Severity severity) => new SolidColorBrush(ColorOf(severity));

    public static IBrush Tint(Severity severity, byte alpha = 0x26)
    {
        var color = ColorOf(severity);
        return new SolidColorBrush(Color.FromArgb(alpha, color.R, color.G, color.B));
    }

    public static string Icon(Severity severity) => severity switch
    {
        Severity.Error => "mdi-close-octagon",
        Severity.Warning => "mdi-alert",
        _ => "mdi-broom",
    };
}

/// <summary>A rule in the sidebar: on/off, icon, example and how many lines it finds.</summary>
public sealed partial class RuleOption : ObservableObject
{
    private static readonly AssaScript ExampleScript = AssaScript.Parse(
        "[Script Info]\nPlayResX: 1920\nPlayResY: 1080\n[V4+ Styles]\n" +
        "Format: Name, Fontname, Fontsize, PrimaryColour, SecondaryColour, OutlineColour, BackColour, Bold, Italic, Underline, StrikeOut, ScaleX, ScaleY, Spacing, Angle, BorderStyle, Outline, Shadow, Alignment, MarginL, MarginR, MarginV, Encoding\n" +
        "Style: Default,Arial,48,&H00FFFFFF,&H000000FF,&H00000000,&H80000000,0,0,0,0,100,100,0,0,1,2,1,2,10,10,10,1\n");

    [ObservableProperty] private bool _isEnabled = true;
    [ObservableProperty] private int _count;

    public RuleOption(RuleInfo info)
    {
        Info = info;
        Severity = RuleCatalog.SeverityOf(info.Rule);
        Brush = SeverityLook.Brush(Severity);
        Tint = SeverityLook.Tint(Severity);

        var result = TagChecker.CheckLine(info.Example, info.Style, info.DurationMs, ExampleScript, new HashSet<RuleId> { info.Rule });
        ExampleIsFixable = result.After != info.Example;
        (ExampleBefore, ExampleAfter) = TextDiff.Compute(info.Example, result.After);
        ExampleMessage = result.Issues.FirstOrDefault()?.Message ?? string.Empty;
        if (result.StyleAfter != info.Style)
        {
            ExampleMessage = $"Style \"{info.Style}\" → \"{result.StyleAfter}\"";
        }
    }

    public RuleInfo Info { get; }
    public RuleId Rule => Info.Rule;
    public Severity Severity { get; }
    public string Name => Info.Name;
    public string Description => Info.Description;
    public string Icon => Info.Icon;
    public IBrush Brush { get; }
    public IBrush Tint { get; }
    public IReadOnlyList<DiffSegment> ExampleBefore { get; }
    public IReadOnlyList<DiffSegment> ExampleAfter { get; }
    public bool ExampleIsFixable { get; }
    public string ExampleMessage { get; }
    public bool HasExampleMessage => ExampleMessage.Length > 0;
    public bool HasCount => Count > 0;
    public string FixKindText => ExampleIsFixable ? "Fixed automatically" : "Shown only - fix by hand";
    public string FixKindIcon => ExampleIsFixable ? "mdi-auto-fix" : "mdi-hand-back-right-outline";

    partial void OnCountChanged(int value) => OnPropertyChanged(nameof(HasCount));
}

/// <summary>The rules of one severity, as a sidebar section.</summary>
public sealed partial class RuleSection : ObservableObject
{
    public RuleSection(Severity severity, string title, IReadOnlyList<RuleOption> rules)
    {
        Severity = severity;
        Title = title;
        Rules = rules;
        Brush = SeverityLook.Brush(severity);
        Tint = SeverityLook.Tint(severity);
        Icon = SeverityLook.Icon(severity);
        foreach (var rule in rules)
        {
            rule.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName is nameof(RuleOption.Count) or nameof(RuleOption.IsEnabled))
                {
                    OnPropertyChanged(nameof(Count));
                    OnPropertyChanged(nameof(HasCount));
                    OnPropertyChanged(nameof(AllEnabled));
                }
            };
        }
    }

    public Severity Severity { get; }
    public string Title { get; }
    public IReadOnlyList<RuleOption> Rules { get; }
    public IBrush Brush { get; }
    public IBrush Tint { get; }
    public string Icon { get; }
    public int Count => Rules.Where(r => r.IsEnabled).Sum(r => r.Count);
    public bool HasCount => Count > 0;

    public bool AllEnabled
    {
        get => Rules.All(r => r.IsEnabled);
        set
        {
            foreach (var rule in Rules)
            {
                rule.IsEnabled = value;
            }
        }
    }
}

/// <summary>A filter chip above the line list.</summary>
public sealed partial class FilterChip : ObservableObject
{
    [ObservableProperty] private bool _isActive;

    public FilterChip(string label, string icon, IBrush? brush, int count, Func<LineItem, bool> matches)
    {
        Label = label;
        Icon = icon;
        Brush = brush ?? NeutralBrush;
        Count = count;
        Matches = matches;
    }

    private static readonly IBrush NeutralBrush = new SolidColorBrush(Color.Parse("#8A8F98"));

    public string Label { get; }
    public string Icon { get; }
    public IBrush Brush { get; }

    /// <summary>Set for a single-rule filter, picked from the sidebar.</summary>
    public RuleOption? Rule { get; init; }
    public int Count { get; }
    public Func<LineItem, bool> Matches { get; }
}
