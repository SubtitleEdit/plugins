using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubtitleEdit.Plugins.Shared;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text.Json;

namespace SubtitleEdit.Plugins.AssaTagCheck;

public sealed partial class MainViewModel : ObservableObject
{
    private const int SettingsVersion = 1;

    private readonly PluginRequest _request;
    private readonly bool _legacyRequest;
    private readonly List<PluginParagraph> _paragraphs;
    private readonly AssaScript _script;
    private readonly Func<string, bool> _isFontInstalled;
    private readonly long _videoOffsetMs;
    private readonly bool _isConfigure;
    private List<LineItem> _lines = new();
    private bool _suspend;

    [ObservableProperty] private bool _selectedLinesOnly;
    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private FilterChip? _activeFilter;
    [ObservableProperty] private int _selectedCount;
    [ObservableProperty] private int _errorCount;
    [ObservableProperty] private int _warningCount;
    [ObservableProperty] private int _tidyCount;
    [ObservableProperty] private int _fixableCount;

    public MainViewModel(PluginRequest request, Func<string, bool>? isFontInstalled = null)
    {
        _request = request;
        _isConfigure = request.RequestType == PluginRequestType.Configure;
        _isFontInstalled = isFontInstalled ?? (_ => true);
        _videoOffsetMs = request.Rules?.VideoOffsetMs ?? 0;
        _legacyRequest = request.Subtitle.Paragraphs == null;
        _paragraphs = _legacyRequest
            ? SubRipParser.Parse(request.Subtitle.SubRip)
                .Select(b => new PluginParagraph { StartMs = b.StartMs, EndMs = b.EndMs, Text = b.Text }).ToList()
            : request.Subtitle.Paragraphs!.Select(p => p.Clone()).ToList();
        _script = AssaScript.Parse(request.Subtitle.Header);
        SelectedIndices = request.SelectedIndices.Where(i => i >= 0 && i < _paragraphs.Count).Distinct().Order().ToList();

        Rules = RuleInfo.All.Select(r => new RuleOption(r)).ToList();
        Sections = new List<RuleSection>
        {
            new(Severity.Error, "ERRORS", Rules.Where(r => r.Severity == Severity.Error).ToList()),
            new(Severity.Warning, "WARNINGS", Rules.Where(r => r.Severity == Severity.Warning).ToList()),
            new(Severity.Tidy, "TIDY UP", Rules.Where(r => r.Severity == Severity.Tidy).ToList()),
        };
        foreach (var rule in Rules)
        {
            rule.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(RuleOption.IsEnabled))
                {
                    Analyze();
                }
            };
        }

        _suspend = true;
        LoadSettings();
        _suspend = false;
        Analyze();
    }

    public IReadOnlyList<RuleOption> Rules { get; }
    public IReadOnlyList<RuleSection> Sections { get; }
    public ObservableCollection<LineItem> VisibleLines { get; } = new();
    public ObservableCollection<FilterChip> Filters { get; } = new();
    public ObservableCollection<ScriptIssue> ScriptIssues { get; } = new();
    public IReadOnlyList<int> SelectedIndices { get; }

    /// <summary>Opened from Batch convert's Configure button: only the rules matter, there is no subtitle.</summary>
    public bool IsConfigure => _isConfigure;
    public bool IsNotConfigure => !_isConfigure;

    public string SubtitleInfo
    {
        get
        {
            if (_isConfigure)
            {
                return "Settings for Batch convert";
            }

            var name = string.IsNullOrEmpty(_request.Subtitle.FileName) ? "Untitled" : Path.GetFileName(_request.Subtitle.FileName);
            var format = string.IsNullOrEmpty(_request.Subtitle.Format) ? string.Empty : $"  ·  {_request.Subtitle.Format}";
            var resolution = _script.PlayResX is { } x && _script.PlayResY is { } y ? $"  ·  {x}x{y}" : string.Empty;
            var styles = _script.HasStyles ? $"  ·  {_script.Styles.Count} styles" : string.Empty;
            return $"{name}{format}  ·  {_paragraphs.Count:N0} lines{resolution}{styles}";
        }
    }

    public bool IsAssa => _request.Subtitle.Format.Contains("Sub Station Alpha", StringComparison.OrdinalIgnoreCase) || _script.HasStyles;
    public bool IsNotAssa => !IsAssa && !_isConfigure;
    public string NotAssaText => $"This subtitle is {(_request.Subtitle.Format.Length > 0 ? _request.Subtitle.Format : "not ASSA")} - tags are checked, style and font checks need an .ass file.";

    public bool HasSelectedLines => SelectedIndices.Count > 0;
    public string SelectedLinesText => SelectedIndices.Count == 1 ? "Selected line (1)" : $"Selected lines ({SelectedIndices.Count:N0})";
    public bool AllLines { get => !SelectedLinesOnly; set => SelectedLinesOnly = !value; }
    public int LineCount => _lines.Count;
    public bool HasLines => _lines.Count > 0;
    public bool HasVisibleLines => VisibleLines.Count > 0;
    public bool HasScriptIssues => ScriptIssues.Count > 0;
    public bool IsClean => _lines.Count == 0 && !_isConfigure;
    public bool IsFilteredEmpty => _lines.Count > 0 && VisibleLines.Count == 0;
    public bool CanApply => SelectedCount > 0 || _isConfigure;
    public bool HasErrors => ErrorCount > 0;
    public bool HasWarnings => WarningCount > 0;
    public bool HasTidy => TidyCount > 0;
    public int EnabledRuleCount => Rules.Count(r => r.IsEnabled);
    public string CleanTitle => EnabledRuleCount == 0 ? "No rules turned on" : "All tags look good";

    public string CleanText => EnabledRuleCount == 0
        ? "Turn on one or more rules in the sidebar."
        : $"{LinesInScope:N0} lines checked against {EnabledRuleCount} rules - nothing to fix.";

    public string StatusText => _isConfigure
        ? $"{EnabledRuleCount} rules on - used for every file in Batch convert"
        : _lines.Count == 0
        ? $"{LinesInScope:N0} lines checked"
        : $"{SelectedCount:N0} of {FixableCount:N0} fixable lines selected  ·  {_lines.Count - FixableCount:N0} to check by hand";

    private int LinesInScope => SelectedLinesOnly && HasSelectedLines ? SelectedIndices.Count : _paragraphs.Count;

    partial void OnSearchTextChanged(string value) => RebuildVisible();
    partial void OnSelectedCountChanged(int value) => RaiseState();
    partial void OnErrorCountChanged(int value) => OnPropertyChanged(nameof(HasErrors));
    partial void OnWarningCountChanged(int value) => OnPropertyChanged(nameof(HasWarnings));
    partial void OnTidyCountChanged(int value) => OnPropertyChanged(nameof(HasTidy));

    partial void OnSelectedLinesOnlyChanged(bool value)
    {
        OnPropertyChanged(nameof(AllLines));
        Analyze();
    }

    partial void OnActiveFilterChanged(FilterChip? value)
    {
        foreach (var chip in Filters)
        {
            chip.IsActive = chip == value;
        }

        RebuildVisible();
    }

    private void RaiseState()
    {
        OnPropertyChanged(nameof(LineCount));
        OnPropertyChanged(nameof(HasLines));
        OnPropertyChanged(nameof(IsClean));
        OnPropertyChanged(nameof(IsFilteredEmpty));
        OnPropertyChanged(nameof(HasVisibleLines));
        OnPropertyChanged(nameof(CanApply));
        OnPropertyChanged(nameof(EnabledRuleCount));
        OnPropertyChanged(nameof(CleanTitle));
        OnPropertyChanged(nameof(CleanText));
        OnPropertyChanged(nameof(StatusText));
        OnPropertyChanged(nameof(HasScriptIssues));
    }

    private void Analyze()
    {
        if (_suspend)
        {
            return;
        }

        var enabled = Rules.Where(r => r.IsEnabled).Select(r => r.Rule).ToHashSet();
        var byRule = Rules.ToDictionary(r => r.Rule);
        var indices = SelectedLinesOnly && HasSelectedLines ? SelectedIndices : (IReadOnlyList<int>)Enumerable.Range(0, _paragraphs.Count).ToList();
        var lines = new List<LineItem>();
        foreach (var index in indices)
        {
            var p = _paragraphs[index];
            var text = p.Text ?? string.Empty;
            var result = TagChecker.CheckLine(text, p.Style, p.EndMs - p.StartMs, _script, enabled);
            if (!result.HasIssues)
            {
                continue;
            }

            var rows = result.Issues.Select(i => new IssueRow(i, byRule[i.Rule])).OrderBy(r => r.Rule.Severity).ToList();
            var item = new LineItem(index, FormatTimeRange(p), text, p.Style, result, rows);
            item.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(LineItem.IsSelected))
                {
                    SelectedCount = _lines.Count(l => l.IsSelected);
                }
            };
            lines.Add(item);
        }

        _lines = lines;
        foreach (var rule in Rules)
        {
            rule.Count = lines.Count(l => l.Issues.Any(i => i.Rule == rule));
        }

        ErrorCount = lines.Count(l => l.Issues.Any(i => i.Rule.Severity == Severity.Error));
        WarningCount = lines.Count(l => l.Issues.Any(i => i.Rule.Severity == Severity.Warning));
        TidyCount = lines.Count(l => l.Issues.Any(i => i.Rule.Severity == Severity.Tidy));
        FixableCount = lines.Count(l => l.IsFixable);

        ScriptIssues.Clear();
        var scope = indices.Select(i => (_paragraphs[i].Text ?? string.Empty, _paragraphs[i].Style)).ToList();
        foreach (var issue in TagChecker.CheckScript(scope, _script, _isFontInstalled))
        {
            ScriptIssues.Add(issue);
        }

        BuildFilters();
        SelectedCount = lines.Count(l => l.IsSelected);
        RaiseState();
    }

    private void BuildFilters()
    {
        var previous = ActiveFilter?.Label;
        var previousRule = ActiveFilter?.Rule;
        Filters.Clear();
        Filters.Add(new FilterChip("All", "mdi-format-list-checks", null, _lines.Count, _ => true));
        foreach (var severity in new[] { Severity.Error, Severity.Warning, Severity.Tidy })
        {
            var count = _lines.Count(l => l.Issues.Any(i => i.Rule.Severity == severity));
            if (count > 0)
            {
                var label = severity switch { Severity.Error => "Errors", Severity.Warning => "Warnings", _ => "Tidy" };
                Filters.Add(new FilterChip(label, SeverityLook.Icon(severity), SeverityLook.Brush(severity), count,
                    l => l.Issues.Any(i => i.Rule.Severity == severity)));
            }
        }

        var manual = _lines.Count(l => l.Issues.Any(i => i.IsManual));
        if (manual > 0)
        {
            Filters.Add(new FilterChip("Fix by hand", "mdi-hand-back-right-outline", null, manual, l => l.Issues.Any(i => i.IsManual)));
        }

        if (previousRule is { Count: > 0 })
        {
            Filters.Add(RuleChip(previousRule));
        }

        ActiveFilter = Filters.FirstOrDefault(f => f.Label == previous) ?? Filters[0];
    }

    private void RebuildVisible()
    {
        VisibleLines.Clear();
        var filter = ActiveFilter?.Matches ?? (_ => true);
        var search = SearchText.Trim();
        foreach (var line in _lines.Where(l => filter(l) && (search.Length == 0 || l.Matches(search))))
        {
            VisibleLines.Add(line);
        }

        OnPropertyChanged(nameof(HasVisibleLines));
        OnPropertyChanged(nameof(IsFilteredEmpty));
    }

    [RelayCommand]
    private void SetFilter(FilterChip chip) => ActiveFilter = chip;

    /// <summary>Shows only the lines of one rule - picked by clicking its count in the sidebar.</summary>
    public void FilterByRule(RuleOption rule)
    {
        if (rule.Count == 0)
        {
            return;
        }

        foreach (var old in Filters.Where(f => f.Rule != null).ToList())
        {
            Filters.Remove(old);
        }

        var chip = RuleChip(rule);
        Filters.Add(chip);
        ActiveFilter = chip;
    }

    private static FilterChip RuleChip(RuleOption rule) =>
        new(rule.Name, rule.Icon, rule.Brush, rule.Count, l => l.Issues.Any(i => i.Rule == rule)) { Rule = rule };

    [RelayCommand]
    private void SelectAll() => SetSelection(_ => true);

    [RelayCommand]
    private void SelectNone() => SetSelection(_ => false);

    [RelayCommand]
    private void InvertSelection() => SetSelection(l => !l.IsSelected);

    [RelayCommand]
    private void EnableAll() => SetRules(true);

    [RelayCommand]
    private void ErrorsOnly()
    {
        _suspend = true;
        foreach (var rule in Rules)
        {
            rule.IsEnabled = rule.Severity == Severity.Error;
        }

        _suspend = false;
        Analyze();
    }

    private void SetRules(bool enabled)
    {
        _suspend = true;
        foreach (var rule in Rules)
        {
            rule.IsEnabled = enabled;
        }

        _suspend = false;
        Analyze();
    }

    private void SetSelection(Func<LineItem, bool> selected)
    {
        foreach (var line in VisibleLines.Where(l => l.IsFixable))
        {
            line.IsSelected = selected(line);
        }
    }

    /// <summary>
    /// A "batch" request (Batch convert): fix every fixable line with the saved rules, without UI -
    /// what OK does with the default selection. Lines that need a manual edit are only counted.
    /// </summary>
    public PluginResponse BuildBatchResponse()
    {
        var manual = _lines.Count(l => !l.IsFixable);
        if (!_lines.Any(l => l.IsFixable))
        {
            return new PluginResponse
            {
                Status = PluginStatus.Ok,
                Message = manual > 0 ? $"ASSA tag check: nothing to fix, {manual:N0} line(s) to check by hand." : "ASSA tag check: all tags look good.",
            };
        }

        var response = BuildResponse();
        if (manual > 0)
        {
            response.Message += $" {manual:N0} line(s) to check by hand.";
        }

        response.Settings = null;
        response.SettingsVersion = null;
        return response;
    }

    /// <summary>A "configure" request: hand back the rules only, for the next Batch convert run.</summary>
    public PluginResponse BuildConfigureResponse() =>
        new() { Status = PluginStatus.Ok, Settings = BuildSettings(), SettingsVersion = SettingsVersion };

    /// <summary>The response for OK: the selected lines with their fixed tags and styles.</summary>
    public PluginResponse BuildResponse()
    {
        if (_isConfigure)
        {
            return BuildConfigureResponse();
        }

        var selected = _lines.Where(l => l.IsSelected && l.IsFixable).ToList();
        if (selected.Count == 0)
        {
            return BuildCancelResponse();
        }

        var paragraphs = _paragraphs.Select(p => p.Clone()).ToList();
        foreach (var line in selected)
        {
            paragraphs[line.Index].Text = line.After;
            paragraphs[line.Index].Style = line.StyleAfter;
        }

        var subtitle = _legacyRequest
            ? new PluginSubtitle
            {
                Format = "SubRip",
                Native = SubRipParser.Serialize(paragraphs.Select(p => new SrtBlock
                {
                    StartMs = (long)Math.Round(p.StartMs),
                    EndMs = (long)Math.Round(p.EndMs),
                    Text = p.Text,
                }).ToList()),
            }
            : new PluginSubtitle { Format = _request.Subtitle.Format, Paragraphs = paragraphs };

        return new PluginResponse
        {
            Status = PluginStatus.Ok,
            Message = selected.Count == 1 ? "ASSA tag check: 1 line fixed." : $"ASSA tag check: {selected.Count:N0} lines fixed.",
            UndoDescription = "ASSA tag check",
            Subtitle = subtitle,
            Settings = BuildSettings(),
            SettingsVersion = SettingsVersion,
        };
    }

    public PluginResponse BuildCancelResponse() =>
        new() { Status = PluginStatus.Cancelled, Settings = BuildSettings(), SettingsVersion = SettingsVersion };

    private JsonElement BuildSettings()
    {
        var payload = new Dictionary<string, object>
        {
            ["disabledRules"] = Rules.Where(r => !r.IsEnabled).Select(r => r.Rule.ToString()).ToList(),
            // Configure has no selection - keep what the last normal run saved.
            ["selectedLinesOnly"] = _isConfigure ? StoredSelectedLinesOnly() : SelectedLinesOnly,
        };
        return JsonSerializer.SerializeToElement(payload);
    }

    private bool StoredSelectedLinesOnly() =>
        _request.SettingsVersion == SettingsVersion &&
        _request.Settings is { ValueKind: JsonValueKind.Object } settings &&
        settings.TryGetProperty("selectedLinesOnly", out var value) && value.ValueKind == JsonValueKind.True;

    private void LoadSettings()
    {
        SelectedLinesOnly = HasSelectedLines;
        if (_request.SettingsVersion != SettingsVersion || _request.Settings is not { ValueKind: JsonValueKind.Object } settings)
        {
            return;
        }

        if (settings.TryGetProperty("disabledRules", out var disabled) && disabled.ValueKind == JsonValueKind.Array)
        {
            var names = disabled.EnumerateArray().Select(e => e.GetString()).ToHashSet();
            foreach (var rule in Rules)
            {
                rule.IsEnabled = !names.Contains(rule.Rule.ToString());
            }
        }

        if (settings.TryGetProperty("selectedLinesOnly", out var selectedOnly) && selectedOnly.ValueKind is JsonValueKind.True or JsonValueKind.False)
        {
            SelectedLinesOnly = selectedOnly.GetBoolean() && HasSelectedLines;
        }
    }

    private string FormatTimeRange(PluginParagraph p) =>
        $"{FormatTime(p.StartMs + _videoOffsetMs)}  →  {FormatTime(p.EndMs + _videoOffsetMs)}";

    private static string FormatTime(double ms)
    {
        var time = TimeSpan.FromMilliseconds(Math.Max(0, ms));
        return string.Format(CultureInfo.InvariantCulture, "{0:00}:{1:00}:{2:00}.{3:00}", (int)time.TotalHours, time.Minutes, time.Seconds, time.Milliseconds / 10);
    }
}
