using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubtitleEdit.Plugins.Shared;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace SubtitleEdit.Plugins.ArteCheck;

public enum FindingFilter
{
    All,
    Fixes,
    Alarms,
}

public sealed partial class MainViewModel : ObservableObject
{
    private const int SettingsVersion = 1;
    public const string EbuFormatName = "EBU STL";

    private readonly PluginRequest _request;
    private readonly bool _isEbu;
    private readonly bool _legacyRequest;
    private readonly long _videoOffsetMs;
    private readonly Stack<(List<PluginParagraph> Paragraphs, string? Header, double FrameRate, int Applied)> _undo = new();
    private readonly Dictionary<string, bool> _expandedGroups = new();
    private List<PluginParagraph> _paragraphs;
    private string? _header;
    private List<ArteFix> _fixes = new();
    private bool _suspendAnalysis;

    [ObservableProperty] private ArteProfile _selectedProfile;
    [ObservableProperty] private double _selectedFrameRate = 25.0;
    [ObservableProperty] private bool _shiftToStartTimeCode;
    [ObservableProperty] private string _targetStartTimeCode = "10:00:00:00";
    [ObservableProperty] private bool _isTargetStartTimeCodeValid = true;
    [ObservableProperty] private decimal? _teletextMaxCells = ArteOptions.PresetTeletextMaxCells;
    [ObservableProperty] private decimal? _minimumGapFrames = ArteOptions.PresetMinimumGapFrames;
    [ObservableProperty] private decimal? _readingTolerancePercent = (decimal)ArteOptions.PresetReadingDurationTolerancePercent;
    [ObservableProperty] private bool _acceptShortDurations;
    [ObservableProperty] private decimal? _shortMinimumFrames = ArteOptions.PresetShortMinimumFrames;
    [ObservableProperty] private FindingFilter _filter = FindingFilter.All;
    [ObservableProperty] private int _fixCount;
    [ObservableProperty] private int _alarmCount;
    [ObservableProperty] private int _selectedCount;
    [ObservableProperty] private int _appliedCount;
    [ObservableProperty] private string _statusText = string.Empty;
    [ObservableProperty] private bool _isPreset = true;
    [ObservableProperty] private bool _canUndo;

    public MainViewModel(PluginRequest request)
    {
        _request = request;
        _isEbu = string.Equals(request.Subtitle.Format, EbuFormatName, StringComparison.OrdinalIgnoreCase);
        _videoOffsetMs = request.Rules?.VideoOffsetMs ?? 0;
        _legacyRequest = request.Subtitle.Paragraphs == null;
        _paragraphs = _legacyRequest
            ? SubRipParser.Parse(request.Subtitle.SubRip)
                .Select(b => new PluginParagraph { StartMs = b.StartMs, EndMs = b.EndMs, Text = b.Text }).ToList()
            : request.Subtitle.Paragraphs!.Select(p => p.Clone()).ToList();

        // Rows hold video-relative times; ARTE time codes (10:00:00:00, ...) are what the file shows.
        foreach (var p in _paragraphs)
        {
            p.StartMs += _videoOffsetMs;
            p.EndMs += _videoOffsetMs;
        }

        _header = request.Subtitle.Header;
        var headerLanguage = GsiHeader.IsStlHeader(_header) ? GsiHeader.Parse(_header!).LanguageCode : null;
        _selectedProfile = ArteProfile.FromLanguageCode(headerLanguage);
        if (_paragraphs.Count > 0)
        {
            _targetStartTimeCode = ArteChecker.FormatTimeCode(ArteChecker.GetStartTimeCodeReference(_paragraphs));
        }

        Checks = CheckItem.CreateAll();
        foreach (var check in Checks)
        {
            check.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(CheckItem.IsEnabled))
                {
                    Analyze();
                }
            };
        }

        _suspendAnalysis = true;
        LoadSettings();
        _suspendAnalysis = false;
        Analyze();
    }

    public IReadOnlyList<ArteProfile> Profiles => ArteProfile.All;
    public IReadOnlyList<double> FrameRates { get; } = new[] { 23.976, 24.0, 25.0, 29.97, 30.0 };
    public IReadOnlyList<CheckItem> Checks { get; }
    public ObservableCollection<FixGroup> Groups { get; } = new();

    public string SubtitleInfo
    {
        get
        {
            var name = string.IsNullOrEmpty(_request.Subtitle.FileName) ? "Untitled" : Path.GetFileName(_request.Subtitle.FileName);
            var format = string.IsNullOrEmpty(_request.Subtitle.Format) ? string.Empty : $"  ·  {_request.Subtitle.Format}";
            return $"{name}{format}  ·  {_paragraphs.Count} subtitles";
        }
    }

    public bool ShowFormatWarning => !_isEbu;
    public string FormatWarning => _legacyRequest
        ? "This Subtitle Edit version only sends SubRip to plugins, so teletext rows and the EBU header are not checked. Update Subtitle Edit for the full check."
        : $"The subtitle is {(_request.Subtitle.Format is { Length: > 0 } f ? f : "not EBU STL")}. Header and teletext row checks need EBU STL - save as EBU STL for delivery.";

    public bool HasFindings => FixCount + AlarmCount > 0;
    public bool IsClean => !HasFindings;
    public bool ShowAll { get => Filter == FindingFilter.All; set { if (value) Filter = FindingFilter.All; } }
    public bool ShowFixes { get => Filter == FindingFilter.Fixes; set { if (value) Filter = FindingFilter.Fixes; } }
    public bool ShowAlarms { get => Filter == FindingFilter.Alarms; set { if (value) Filter = FindingFilter.Alarms; } }
    public string PresetLabel => IsPreset ? "ARTE preset" : "Custom";
    public bool HasSelection => SelectedCount > 0;

    partial void OnSelectedProfileChanged(ArteProfile value) => Analyze();
    partial void OnSelectedFrameRateChanged(double value) => Analyze();
    partial void OnShiftToStartTimeCodeChanged(bool value) => Analyze();
    partial void OnTeletextMaxCellsChanged(decimal? value) => Analyze();
    partial void OnMinimumGapFramesChanged(decimal? value) => Analyze();
    partial void OnReadingTolerancePercentChanged(decimal? value) => Analyze();
    partial void OnAcceptShortDurationsChanged(bool value) => Analyze();
    partial void OnShortMinimumFramesChanged(decimal? value) => Analyze();

    partial void OnTargetStartTimeCodeChanged(string value)
    {
        IsTargetStartTimeCodeValid = ArteChecker.TryParseTimeCode(value, out _);
        if (ShiftToStartTimeCode)
        {
            Analyze();
        }
    }

    partial void OnFilterChanged(FindingFilter value)
    {
        OnPropertyChanged(nameof(ShowAll));
        OnPropertyChanged(nameof(ShowFixes));
        OnPropertyChanged(nameof(ShowAlarms));
        RebuildGroups();
    }

    partial void OnIsPresetChanged(bool value) => OnPropertyChanged(nameof(PresetLabel));
    partial void OnSelectedCountChanged(int value) => OnPropertyChanged(nameof(HasSelection));
    partial void OnAppliedCountChanged(int value) => OnPropertyChanged(nameof(HasApplied));

    public bool HasApplied => AppliedCount > 0;

    private ArteOptions BuildOptions()
    {
        var rules = _request.Rules;
        var options = new ArteOptions
        {
            Profile = SelectedProfile,
            SourceFrameRate = SelectedFrameRate,
            ShiftToStartTimeCode = ShiftToStartTimeCode && IsTargetStartTimeCodeValid,
            TeletextMaxCells = (int)(TeletextMaxCells ?? ArteOptions.PresetTeletextMaxCells),
            MinimumGapFrames = (int)(MinimumGapFrames ?? ArteOptions.PresetMinimumGapFrames),
            ReadingDurationTolerancePercent = (double)(ReadingTolerancePercent ?? 0),
            AcceptShortDurations = AcceptShortDurations,
            ShortMinimumFrames = (int)(ShortMinimumFrames ?? ArteOptions.PresetShortMinimumFrames),
            EnabledChecks = Checks.Where(c => c.IsEnabled).Select(c => c.Type).ToHashSet(),
        };

        if (ArteChecker.TryParseTimeCode(TargetStartTimeCode, out var targetMs))
        {
            options.TargetStartMs = targetMs;
        }

        if (rules != null)
        {
            options.MinimumDurationMs = rules.SubtitleMinimumDisplayMilliseconds;
            options.MaximumDurationMs = rules.SubtitleMaximumDisplayMilliseconds;
            options.MaximumCharactersPerSecond = rules.SubtitleMaximumCharactersPerSeconds;
            options.DoubleHeight = rules.EbuStlTeletextUseDoubleHeight;
        }

        if (!_isEbu)
        {
            options.EnabledChecks.Remove(ArteCheckType.TeletextLinePosition);
        }

        return options;
    }

    private void Analyze()
    {
        if (_suspendAnalysis)
        {
            return;
        }

        var options = BuildOptions();
        IsPreset = options.IsPreset;
        _fixes = ArteChecker.Analyze(_paragraphs, _header, options);
        if (!_isEbu)
        {
            _fixes.RemoveAll(f => f.Kind == ArteFixKind.Header);
        }

        foreach (var fix in _fixes)
        {
            fix.PropertyChanged += OnFixChanged;
        }

        FixCount = _fixes.Count(f => f.CanBeFixed);
        AlarmCount = _fixes.Count(f => f.IsAlarm);
        OnPropertyChanged(nameof(HasFindings));
        OnPropertyChanged(nameof(IsClean));
        UpdateSelectedCount();
        RebuildGroups();
    }

    private void RebuildGroups()
    {
        foreach (var group in Groups)
        {
            _expandedGroups[group.Name] = group.IsExpanded;
        }

        Groups.Clear();
        var visible = _fixes.Where(f => Filter switch
        {
            FindingFilter.Fixes => f.CanBeFixed,
            FindingFilter.Alarms => f.IsAlarm,
            _ => true,
        });

        var first = true;
        foreach (var group in visible.GroupBy(f => f.Group))
        {
            var expanded = _expandedGroups.TryGetValue(group.Key, out var wasExpanded) ? wasExpanded : first;
            Groups.Add(new FixGroup(group.Key, group.ToList(), expanded));
            first = false;
        }
    }

    private void OnFixChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ArteFix.Apply))
        {
            UpdateSelectedCount();
        }
    }

    private void UpdateSelectedCount()
    {
        SelectedCount = _fixes.Count(f => f.CanBeFixed && f.Apply);
        var frameRateNote = Math.Abs(SelectedFrameRate - 25.0) > 0.001 ? $" + {SelectedFrameRate:0.###} → 25 fps conversion" : string.Empty;
        StatusText = HasFindings
            ? $"{SelectedCount} of {FixCount} correction(s) selected{frameRateNote}"
            : $"No issues found{frameRateNote}";
    }

    [RelayCommand]
    private void ApplyPreset()
    {
        _suspendAnalysis = true;
        TeletextMaxCells = ArteOptions.PresetTeletextMaxCells;
        MinimumGapFrames = ArteOptions.PresetMinimumGapFrames;
        ReadingTolerancePercent = (decimal)ArteOptions.PresetReadingDurationTolerancePercent;
        AcceptShortDurations = false;
        ShortMinimumFrames = ArteOptions.PresetShortMinimumFrames;
        _suspendAnalysis = false;
        Analyze();
    }

    [RelayCommand]
    private void SelectAll()
    {
        foreach (var fix in _fixes.Where(f => f.CanBeFixed))
        {
            fix.Apply = true;
        }
    }

    [RelayCommand]
    private void SelectNone()
    {
        foreach (var fix in _fixes)
        {
            fix.Apply = false;
        }
    }

    [RelayCommand]
    private void ExpandAll()
    {
        foreach (var group in Groups)
        {
            group.IsExpanded = true;
        }
    }

    [RelayCommand]
    private void CollapseAll()
    {
        foreach (var group in Groups)
        {
            group.IsExpanded = false;
        }
    }

    /// <summary>Applies the selected corrections to the working copy and checks again (fixes can enable further fixes).</summary>
    [RelayCommand]
    private void ApplySelected()
    {
        if (!ApplyPending())
        {
            return;
        }

        Analyze();
    }

    private bool ApplyPending()
    {
        var options = BuildOptions();
        var hasConversion = Math.Abs(options.SourceFrameRate - 25.0) > 0.001;
        if (SelectedCount == 0 && !hasConversion)
        {
            return false;
        }

        var result = ArteChecker.Apply(_paragraphs, _header, _fixes, options);
        _undo.Push((_paragraphs, _header, SelectedFrameRate, AppliedCount));
        CanUndo = true;
        _paragraphs = result.Paragraphs;
        _header = result.Header;
        AppliedCount += result.Applied;

        // The working copy is on the 25 fps timeline now.
        _suspendAnalysis = true;
        SelectedFrameRate = 25.0;
        _suspendAnalysis = false;
        OnPropertyChanged(nameof(SubtitleInfo));
        return true;
    }

    [RelayCommand]
    private void Undo()
    {
        if (_undo.Count == 0)
        {
            return;
        }

        var (paragraphs, header, frameRate, applied) = _undo.Pop();
        _paragraphs = paragraphs;
        _header = header;
        AppliedCount = applied;
        CanUndo = _undo.Count > 0;
        _suspendAnalysis = true;
        SelectedFrameRate = frameRate;
        _suspendAnalysis = false;
        OnPropertyChanged(nameof(SubtitleInfo));
        Analyze();
    }

    public string BuildReport()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"ARTE check - {SelectedProfile}");
        sb.AppendLine(SubtitleInfo);
        sb.AppendLine($"{FixCount} correction(s), {AlarmCount} alarm(s)");
        foreach (var group in _fixes.GroupBy(f => f.Group))
        {
            sb.AppendLine();
            sb.AppendLine($"{group.Key} ({group.Count()})");
            sb.AppendLine(new string('-', group.Key.Length + 5));
            foreach (var fix in group)
            {
                var location = fix.IndexDisplay;
                if (fix.HasTimeRange)
                {
                    location += $"  {fix.TimeRange}";
                }

                sb.AppendLine($"{(fix.CanBeFixed ? "FIX  " : "ALARM")}  {location}");
                sb.AppendLine($"       {fix.Reason}");
            }
        }

        return sb.ToString();
    }

    /// <summary>The response for OK: applies what is still selected, then hands the working copy back.</summary>
    public PluginResponse BuildResponse()
    {
        ApplyPending();
        if (AppliedCount == 0)
        {
            return new PluginResponse { Status = PluginStatus.Cancelled, Settings = BuildSettings(), SettingsVersion = SettingsVersion };
        }

        var paragraphs = _paragraphs.Select(p =>
        {
            var copy = p.Clone();
            copy.StartMs -= _videoOffsetMs;
            copy.EndMs -= _videoOffsetMs;
            return copy;
        }).ToList();

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
            : new PluginSubtitle
            {
                Format = _request.Subtitle.Format,
                Header = _isEbu ? _header : null,
                Paragraphs = paragraphs,
            };

        return new PluginResponse
        {
            Status = PluginStatus.Ok,
            Message = $"ARTE check: {AppliedCount} correction(s) applied.",
            UndoDescription = "ARTE check",
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
            ["teletextMaxCells"] = (int)(TeletextMaxCells ?? ArteOptions.PresetTeletextMaxCells),
            ["minimumGapFrames"] = (int)(MinimumGapFrames ?? ArteOptions.PresetMinimumGapFrames),
            ["readingTolerancePercent"] = (double)(ReadingTolerancePercent ?? 0),
            ["acceptShortDurations"] = AcceptShortDurations,
            ["shortMinimumFrames"] = (int)(ShortMinimumFrames ?? ArteOptions.PresetShortMinimumFrames),
            ["disabledChecks"] = Checks.Where(c => !c.IsEnabled).Select(c => c.Type.ToString()).ToArray(),
        };
        return JsonSerializer.SerializeToElement(payload);
    }

    private void LoadSettings()
    {
        if (_request.SettingsVersion != SettingsVersion || _request.Settings is not { ValueKind: JsonValueKind.Object } settings)
        {
            return;
        }

        if (settings.TryGetProperty("teletextMaxCells", out var cells) && cells.TryGetInt32(out var c) && c is >= 10 and <= 40)
        {
            TeletextMaxCells = c;
        }

        if (settings.TryGetProperty("minimumGapFrames", out var gap) && gap.TryGetInt32(out var g) && g is >= 0 and <= 50)
        {
            MinimumGapFrames = g;
        }

        if (settings.TryGetProperty("readingTolerancePercent", out var tolerance) && tolerance.TryGetDouble(out var t) && t is >= 0 and <= 100)
        {
            ReadingTolerancePercent = (decimal)t;
        }

        if (settings.TryGetProperty("acceptShortDurations", out var shortDurations) && shortDurations.ValueKind is JsonValueKind.True or JsonValueKind.False)
        {
            AcceptShortDurations = shortDurations.GetBoolean();
        }

        if (settings.TryGetProperty("shortMinimumFrames", out var shortFrames) && shortFrames.TryGetInt32(out var s) && s is >= 1 and <= 250)
        {
            ShortMinimumFrames = s;
        }

        if (settings.TryGetProperty("disabledChecks", out var disabled) && disabled.ValueKind == JsonValueKind.Array)
        {
            var names = disabled.EnumerateArray().Select(e => e.GetString()).ToHashSet();
            foreach (var check in Checks)
            {
                check.IsEnabled = !names.Contains(check.Type.ToString());
            }
        }
    }

    public static string FormatFrameRate(double fps) => fps.ToString("0.###", CultureInfo.InvariantCulture) + " fps";
}
