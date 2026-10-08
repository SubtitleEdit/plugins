using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubtitleEdit.Plugins.Shared;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text.Json;

namespace SubtitleEdit.Plugins.PersianErrors;

public sealed partial class MainViewModel : ObservableObject
{
    private const int SettingsVersion = 1;

    private readonly PluginRequest _request;
    private readonly bool _legacyRequest;
    private readonly List<PluginParagraph> _paragraphs;
    private readonly string[] _originalTexts;
    private readonly string[] _texts;
    private readonly long _videoOffsetMs;
    private readonly Stack<string[]> _undo = new();
    private List<FixItem> _fixes = new();
    private CancellationTokenSource? _analysisCancellation;
    private bool _suspendAnalysis;

    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private double _progress;
    [ObservableProperty] private bool _showHidden = true;
    [ObservableProperty] private bool _selectedLinesOnly;
    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private FilterChip? _activeFilter;
    [ObservableProperty] private int _fixCount;
    [ObservableProperty] private int _selectedCount;
    [ObservableProperty] private int _changedLineCount;
    [ObservableProperty] private bool _canUndo;

    public MainViewModel(PluginRequest request)
    {
        _request = request;
        _videoOffsetMs = request.Rules?.VideoOffsetMs ?? 0;
        _legacyRequest = request.Subtitle.Paragraphs == null;
        _paragraphs = _legacyRequest
            ? SubRipParser.Parse(request.Subtitle.SubRip)
                .Select(b => new PluginParagraph { StartMs = b.StartMs, EndMs = b.EndMs, Text = b.Text }).ToList()
            : request.Subtitle.Paragraphs!.Select(p => p.Clone()).ToList();
        _originalTexts = _paragraphs.Select(p => p.Text ?? string.Empty).ToArray();
        _texts = _originalTexts.ToArray();
        SelectedIndices = request.SelectedIndices.Where(i => i >= 0 && i < _paragraphs.Count).Distinct().Order().ToList();

        var ruleGroups = RuleSet.Default;
        Groups = new ObservableCollection<GroupOption>(ruleGroups.Select((g, i) => new GroupOption(g, GroupInfo.For(g.Name, i))));
        foreach (var group in Groups)
        {
            group.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(GroupOption.IsEnabled))
                {
                    OnPropertyChanged(nameof(EnabledGroupCount));
                    Analyze();
                }
            };
        }

        _suspendAnalysis = true;
        LoadSettings();
        _suspendAnalysis = false;
        Analyze();
    }

    public ObservableCollection<GroupOption> Groups { get; }
    public ObservableCollection<FixItem> VisibleFixes { get; } = new();
    public ObservableCollection<FilterChip> Filters { get; } = new();
    public IReadOnlyList<int> SelectedIndices { get; }

    public string Title => "Persian Subtitle Fixes";

    public string SubtitleInfo
    {
        get
        {
            var name = string.IsNullOrEmpty(_request.Subtitle.FileName) ? "Untitled" : Path.GetFileName(_request.Subtitle.FileName);
            var format = string.IsNullOrEmpty(_request.Subtitle.Format) ? string.Empty : $"  ·  {_request.Subtitle.Format}";
            return $"{name}{format}  ·  {_paragraphs.Count:N0} lines";
        }
    }

    public int EnabledGroupCount => Groups.Count(g => g.IsEnabled);
    public int TotalRuleCount => Groups.Sum(g => g.Group.Rules.Count);
    public bool HasSelectedLines => SelectedIndices.Count > 0;
    public string SelectedLinesText => SelectedIndices.Count == 1 ? "Selected line (1)" : $"Selected lines ({SelectedIndices.Count:N0})";
    public bool AllLines { get => !SelectedLinesOnly; set => SelectedLinesOnly = !value; }
    public bool HasFixes => FixCount > 0;
    public bool HasVisibleFixes => VisibleFixes.Count > 0;
    public bool IsClean => !IsBusy && FixCount == 0;
    public bool IsFilteredEmpty => !IsBusy && FixCount > 0 && VisibleFixes.Count == 0;
    public bool HasChanges => ChangedLineCount > 0;
    public bool CanApply => !IsBusy && SelectedCount > 0;
    public string CleanTitle => EnabledGroupCount == 0 ? "No fix groups enabled" : "No errors found";
    public string CleanText => EnabledGroupCount == 0
        ? "Turn on one or more groups in the sidebar."
        : ChangedLineCount > 0 ? "Every enabled rule passes. Click OK to keep the changes." : "Every enabled rule passes.";

    public string StatusText
    {
        get
        {
            if (IsBusy)
            {
                return $"Checking {LinesInScope:N0} lines against {Groups.Where(g => g.IsEnabled).Sum(g => g.Group.Rules.Count):N0} rules...";
            }

            return FixCount == 0
                ? $"{LinesInScope:N0} lines checked"
                : $"{SelectedCount:N0} of {FixCount:N0} fixes selected";
        }
    }

    private int LinesInScope => SelectedLinesOnly && HasSelectedLines ? SelectedIndices.Count : _paragraphs.Count;

    partial void OnIsBusyChanged(bool value) => RaiseState();
    partial void OnFixCountChanged(int value) => RaiseState();
    partial void OnSelectedCountChanged(int value) => RaiseState();
    partial void OnChangedLineCountChanged(int value) => RaiseState();
    partial void OnSearchTextChanged(string value) => RebuildVisible();

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
        OnPropertyChanged(nameof(HasFixes));
        OnPropertyChanged(nameof(IsClean));
        OnPropertyChanged(nameof(IsFilteredEmpty));
        OnPropertyChanged(nameof(HasChanges));
        OnPropertyChanged(nameof(CanApply));
        OnPropertyChanged(nameof(CleanTitle));
        OnPropertyChanged(nameof(CleanText));
        OnPropertyChanged(nameof(StatusText));
    }

    private async void Analyze()
    {
        if (_suspendAnalysis)
        {
            return;
        }

        _analysisCancellation?.Cancel();
        var cancellation = new CancellationTokenSource();
        _analysisCancellation = cancellation;

        var groups = Groups.Where(g => g.IsEnabled).Select(g => g.Group).ToList();
        var indices = SelectedLinesOnly && HasSelectedLines ? SelectedIndices.ToList() : Enumerable.Range(0, _texts.Length).ToList();
        var texts = _texts.ToArray();
        IsBusy = true;
        Progress = 0;
        var progress = new Progress<double>(p =>
        {
            if (_analysisCancellation == cancellation)
            {
                Progress = p * 100;
            }
        });

        List<LineFix> fixes;
        try
        {
            fixes = await Task.Run(() => PersianFixer.FindFixes(texts, indices, groups, progress, cancellation.Token), cancellation.Token);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        if (_analysisCancellation != cancellation)
        {
            return;
        }

        var byName = Groups.ToDictionary(g => g.Name);
        _fixes = fixes.Select(f => new FixItem(f, FormatTimeRange(_paragraphs[f.Index]), f.Groups.Select(n => byName[n]).ToList())).ToList();
        foreach (var fix in _fixes)
        {
            fix.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(FixItem.IsSelected))
                {
                    SelectedCount = _fixes.Count(f => f.IsSelected);
                }
            };
        }

        foreach (var group in Groups)
        {
            group.FixCount = fixes.Count(f => f.Groups.Contains(group.Name));
        }

        FixCount = _fixes.Count;
        SelectedCount = _fixes.Count;
        RebuildFilters();
        IsBusy = false;
    }

    private void RebuildFilters()
    {
        var activeGroup = ActiveFilter?.Group;
        Filters.Clear();
        Filters.Add(new FilterChip(null, FixCount));
        foreach (var group in Groups.Where(g => g.IsEnabled && g.FixCount > 0))
        {
            Filters.Add(new FilterChip(group, group.FixCount));
        }

        var active = Filters.FirstOrDefault(f => f.Group == activeGroup) ?? Filters[0];
        if (active == ActiveFilter)
        {
            active.IsActive = true;
            RebuildVisible();
        }
        else
        {
            ActiveFilter = active;
        }
    }

    private void RebuildVisible()
    {
        var group = ActiveFilter?.Group;
        var search = SearchText.Trim();
        VisibleFixes.Clear();
        foreach (var fix in _fixes)
        {
            if ((group == null || fix.Groups.Contains(group)) && (search.Length == 0 || fix.Matches(search)))
            {
                VisibleFixes.Add(fix);
            }
        }

        OnPropertyChanged(nameof(HasVisibleFixes));
        OnPropertyChanged(nameof(IsFilteredEmpty));
    }

    [RelayCommand]
    private void SetFilter(FilterChip? chip) => ActiveFilter = chip;

    [RelayCommand]
    private void SelectAll() => SetSelection(_ => true);

    [RelayCommand]
    private void SelectNone() => SetSelection(_ => false);

    [RelayCommand]
    private void InvertSelection() => SetSelection(f => !f.IsSelected);

    private void SetSelection(Func<FixItem, bool> selected)
    {
        foreach (var fix in VisibleFixes)
        {
            fix.IsSelected = selected(fix);
        }
    }

    [RelayCommand]
    private void EnableAllGroups() => SetGroups(true);

    [RelayCommand]
    private void DisableAllGroups() => SetGroups(false);

    private void SetGroups(bool enabled)
    {
        _suspendAnalysis = true;
        foreach (var group in Groups)
        {
            group.IsEnabled = enabled;
        }

        _suspendAnalysis = false;
        Analyze();
    }

    [RelayCommand]
    private void ApplyAndRecheck()
    {
        if (IsBusy || ApplySelected() == 0)
        {
            return;
        }

        Analyze();
    }

    [RelayCommand]
    private void Undo()
    {
        if (_undo.Count == 0)
        {
            return;
        }

        var previous = _undo.Pop();
        Array.Copy(previous, _texts, _texts.Length);
        CanUndo = _undo.Count > 0;
        UpdateChangedLineCount();
        Analyze();
    }

    /// <summary>Writes the selected fixes into the working texts. Returns how many were applied.</summary>
    private int ApplySelected()
    {
        var selected = _fixes.Where(f => f.IsSelected).ToList();
        if (selected.Count == 0)
        {
            return 0;
        }

        _undo.Push(_texts.ToArray());
        CanUndo = true;
        foreach (var fix in selected)
        {
            _texts[fix.Index] = fix.Fix.After;
        }

        UpdateChangedLineCount();
        return selected.Count;
    }

    private void UpdateChangedLineCount() =>
        ChangedLineCount = _texts.Where((t, i) => !string.Equals(t, _originalTexts[i], StringComparison.Ordinal)).Count();

    /// <summary>The response for OK: applies what is still selected, then hands the lines back.</summary>
    public PluginResponse BuildResponse()
    {
        if (!IsBusy)
        {
            ApplySelected();
        }

        if (ChangedLineCount == 0)
        {
            return BuildCancelResponse();
        }

        PluginSubtitle subtitle;
        if (_legacyRequest)
        {
            subtitle = new PluginSubtitle
            {
                Format = "SubRip",
                Native = SubRipParser.Serialize(_paragraphs.Select((p, i) => new SrtBlock
                {
                    StartMs = (long)Math.Round(p.StartMs),
                    EndMs = (long)Math.Round(p.EndMs),
                    Text = _texts[i],
                }).ToList()),
            };
        }
        else
        {
            subtitle = new PluginSubtitle
            {
                Format = _request.Subtitle.Format,
                Paragraphs = _paragraphs.Select((p, i) =>
                {
                    var copy = p.Clone();
                    copy.Text = _texts[i];
                    return copy;
                }).ToList(),
            };
        }

        return new PluginResponse
        {
            Status = PluginStatus.Ok,
            Message = ChangedLineCount == 1 ? "Persian Subtitle Fixes: 1 line fixed." : $"Persian Subtitle Fixes: {ChangedLineCount:N0} lines fixed.",
            UndoDescription = "Persian Subtitle Fixes",
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
            ["disabledGroups"] = Groups.Where(g => !g.IsEnabled).Select(g => g.Name).ToArray(),
            ["showHidden"] = ShowHidden,
            ["selectedLinesOnly"] = SelectedLinesOnly,
        };
        return JsonSerializer.SerializeToElement(payload);
    }

    private void LoadSettings()
    {
        if (_request.SettingsVersion != SettingsVersion || _request.Settings is not { ValueKind: JsonValueKind.Object } settings)
        {
            return;
        }

        if (settings.TryGetProperty("disabledGroups", out var disabled) && disabled.ValueKind == JsonValueKind.Array)
        {
            var names = disabled.EnumerateArray().Select(e => e.GetString()).ToHashSet();
            foreach (var group in Groups)
            {
                group.IsEnabled = !names.Contains(group.Name);
            }
        }

        if (settings.TryGetProperty("showHidden", out var showHidden) && showHidden.ValueKind is JsonValueKind.True or JsonValueKind.False)
        {
            ShowHidden = showHidden.GetBoolean();
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
        return string.Format(CultureInfo.InvariantCulture, "{0:00}:{1:00}:{2:00},{3:000}", (int)time.TotalHours, time.Minutes, time.Seconds, time.Milliseconds);
    }
}
