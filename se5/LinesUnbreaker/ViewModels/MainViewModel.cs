using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubtitleEdit.Plugins.LinesUnbreaker.Logic;
using SubtitleEdit.Plugins.Shared;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text.Json;

namespace SubtitleEdit.Plugins.LinesUnbreaker.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
    private const int SettingsVersion = 1;

    private readonly PluginRequest _request;
    private readonly bool _legacyRequest;
    private readonly List<PluginParagraph> _paragraphs;
    private readonly long _videoOffsetMs;
    private readonly bool _isConfigure;
    private bool _loading;

    [ObservableProperty] private int _maxLength = UnbreakOptions.DefaultMaxLength;
    [ObservableProperty] private bool _skipDialogs = true;
    [ObservableProperty] private bool _skipMoods;
    [ObservableProperty] private bool _skipNarrator = true;
    [ObservableProperty] private bool _selectedLinesOnly;
    [ObservableProperty] private int _selectedCount;

    public MainViewModel(PluginRequest request)
    {
        _request = request;
        _isConfigure = request.RequestType == PluginRequestType.Configure;
        _videoOffsetMs = request.Rules?.VideoOffsetMs ?? 0;
        _legacyRequest = request.Subtitle.Paragraphs == null;
        _paragraphs = _legacyRequest
            ? SubRipParser.Parse(request.Subtitle.SubRip)
                .Select(b => new PluginParagraph { StartMs = b.StartMs, EndMs = b.EndMs, Text = b.Text }).ToList()
            : request.Subtitle.Paragraphs!.Select(p => p.Clone()).ToList();
        SelectedIndices = request.SelectedIndices.Where(i => i >= 0 && i < _paragraphs.Count).Distinct().Order().ToList();

        // Default to Subtitle Edit's "single line max. length".
        if (request.Rules is { SubtitleLineMaximumLength: > 0 } rules)
        {
            _maxLength = rules.SubtitleLineMaximumLength;
        }

        _loading = true;
        LoadSettings();
        _loading = false;
        Analyze();
    }

    public ObservableCollection<ChangeItem> Changes { get; } = new();
    public IReadOnlyList<int> SelectedIndices { get; }

    public UnbreakOptions Options => new() { MaxLength = MaxLength, SkipDialogs = SkipDialogs, SkipMoods = SkipMoods, SkipNarrator = SkipNarrator };

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
            return $"{name}{format}  ·  {_paragraphs.Count:N0} lines";
        }
    }

    /// <summary>Opened from Batch convert's Configure button: only the options matter, there is no subtitle.</summary>
    public bool IsConfigure => _isConfigure;
    public bool IsNotConfigure => !_isConfigure;

    public bool HasSelectedLines => SelectedIndices.Count > 0;
    public string SelectedLinesText => SelectedIndices.Count == 1 ? "Selected line (1)" : $"Selected lines ({SelectedIndices.Count:N0})";
    public bool AllLines { get => !SelectedLinesOnly; set => SelectedLinesOnly = !value; }
    public int ChangeCount => Changes.Count;
    public bool HasChanges => Changes.Count > 0;
    public bool IsClean => Changes.Count == 0 && !_isConfigure;
    public bool CanApply => SelectedCount > 0 || _isConfigure;

    /// <summary><see cref="MaxLength"/> for the NumericUpDown, which works with decimals.</summary>
    public decimal? MaxLengthValue
    {
        get => MaxLength;
        set => MaxLength = Math.Clamp((int)(value ?? UnbreakOptions.DefaultMaxLength), 1, 999);
    }

    public string CleanText => $"No subtitle with more than one line fits in {MaxLength} characters.";

    public string StatusText => _isConfigure
        ? "Used for every file in Batch convert"
        : Changes.Count == 0
        ? $"{LinesInScope:N0} lines checked"
        : $"{SelectedCount:N0} of {Changes.Count:N0} lines selected";

    private int LinesInScope => SelectedLinesOnly && HasSelectedLines ? SelectedIndices.Count : _paragraphs.Count;

    partial void OnMaxLengthChanged(int value)
    {
        OnPropertyChanged(nameof(MaxLengthValue));
        OnPropertyChanged(nameof(CleanText));
        Analyze();
    }

    partial void OnSkipDialogsChanged(bool value) => Analyze();
    partial void OnSkipMoodsChanged(bool value) => Analyze();
    partial void OnSkipNarratorChanged(bool value) => Analyze();

    partial void OnSelectedLinesOnlyChanged(bool value)
    {
        OnPropertyChanged(nameof(AllLines));
        Analyze();
    }

    partial void OnSelectedCountChanged(int value)
    {
        OnPropertyChanged(nameof(CanApply));
        OnPropertyChanged(nameof(StatusText));
    }

    private IEnumerable<int> IndicesInScope =>
        SelectedLinesOnly && HasSelectedLines ? SelectedIndices : Enumerable.Range(0, _paragraphs.Count);

    private void Analyze()
    {
        if (_loading)
        {
            return;
        }

        var options = Options;
        Changes.Clear();
        foreach (var index in IndicesInScope)
        {
            var text = _paragraphs[index].Text ?? string.Empty;
            var after = LineUnbreaker.Unbreak(text, options);
            if (after != text)
            {
                var item = new ChangeItem(index, text, after, FormatTimeRange(_paragraphs[index]));
                item.PropertyChanged += (_, e) =>
                {
                    if (e.PropertyName == nameof(ChangeItem.IsSelected))
                    {
                        SelectedCount = Changes.Count(c => c.IsSelected);
                    }
                };
                Changes.Add(item);
            }
        }

        SelectedCount = Changes.Count;
        OnPropertyChanged(nameof(ChangeCount));
        OnPropertyChanged(nameof(HasChanges));
        OnPropertyChanged(nameof(IsClean));
        OnPropertyChanged(nameof(StatusText));
    }

    [RelayCommand]
    private void SelectAll() => SetSelection(_ => true);

    [RelayCommand]
    private void SelectNone() => SetSelection(_ => false);

    [RelayCommand]
    private void InvertSelection() => SetSelection(c => !c.IsSelected);

    private void SetSelection(Func<ChangeItem, bool> selected)
    {
        foreach (var change in Changes)
        {
            change.IsSelected = selected(change);
        }
    }

    /// <summary>
    /// A "batch" request (Batch convert): unbreak every short subtitle with the saved options, without UI.
    /// No changes is "ok" without a subtitle.
    /// </summary>
    public PluginResponse BuildBatchResponse()
    {
        if (Changes.Count == 0)
        {
            return new PluginResponse { Status = PluginStatus.Ok, Message = "Lines unbreaker: nothing to unbreak." };
        }

        var response = BuildResponse();
        response.Settings = null;
        response.SettingsVersion = null;
        return response;
    }

    /// <summary>A "configure" request: hand back the settings only, for the next Batch convert run.</summary>
    public PluginResponse BuildConfigureResponse() =>
        new() { Status = PluginStatus.Ok, Settings = BuildSettings(), SettingsVersion = SettingsVersion };

    /// <summary>The response for OK: the selected subtitles joined to one line.</summary>
    public PluginResponse BuildResponse()
    {
        if (_isConfigure)
        {
            return BuildConfigureResponse();
        }

        var selected = Changes.Where(c => c.IsSelected).ToList();
        if (selected.Count == 0)
        {
            return BuildCancelResponse();
        }

        var texts = _paragraphs.Select(p => p.Text ?? string.Empty).ToArray();
        foreach (var change in selected)
        {
            texts[change.Index] = change.After;
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
                    Text = texts[i],
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
                    copy.Text = texts[i];
                    return copy;
                }).ToList(),
            };
        }

        return new PluginResponse
        {
            Status = PluginStatus.Ok,
            Message = selected.Count == 1
                ? "Lines unbreaker: 1 subtitle unbroken."
                : $"Lines unbreaker: {selected.Count:N0} subtitles unbroken.",
            UndoDescription = "Lines unbreaker",
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
            ["maxLength"] = MaxLength,
            ["skipDialogs"] = SkipDialogs,
            ["skipMoods"] = SkipMoods,
            ["skipNarrator"] = SkipNarrator,
            ["selectedLinesOnly"] = SelectedLinesOnly,
        };

        // Configure has no lines selected, so keep the scope a normal run saved.
        if (_isConfigure && _request.SettingsVersion == SettingsVersion && _request.Settings is { ValueKind: JsonValueKind.Object } stored
            && stored.TryGetProperty("selectedLinesOnly", out var selectedOnly) && selectedOnly.ValueKind is JsonValueKind.True or JsonValueKind.False)
        {
            payload["selectedLinesOnly"] = selectedOnly.GetBoolean();
        }

        return JsonSerializer.SerializeToElement(payload);
    }

    private void LoadSettings()
    {
        // Lines selected in Subtitle Edit mean "work on these" - default to them.
        SelectedLinesOnly = HasSelectedLines;
        if (_request.SettingsVersion != SettingsVersion || _request.Settings is not { ValueKind: JsonValueKind.Object } settings)
        {
            return;
        }

        if (settings.TryGetProperty("maxLength", out var maxLength) && maxLength.TryGetInt32(out var length) && length > 0)
        {
            MaxLength = length;
        }

        SkipDialogs = ReadBool(settings, "skipDialogs", SkipDialogs);
        SkipMoods = ReadBool(settings, "skipMoods", SkipMoods);
        SkipNarrator = ReadBool(settings, "skipNarrator", SkipNarrator);

        if (settings.TryGetProperty("selectedLinesOnly", out var selectedOnly) && selectedOnly.ValueKind is JsonValueKind.True or JsonValueKind.False)
        {
            SelectedLinesOnly = selectedOnly.GetBoolean() && HasSelectedLines;
        }
    }

    private static bool ReadBool(JsonElement settings, string name, bool fallback) =>
        settings.TryGetProperty(name, out var value) && value.ValueKind is JsonValueKind.True or JsonValueKind.False ? value.GetBoolean() : fallback;

    private string FormatTimeRange(PluginParagraph p) =>
        $"{FormatTime(p.StartMs + _videoOffsetMs)}  →  {FormatTime(p.EndMs + _videoOffsetMs)}";

    private static string FormatTime(double ms)
    {
        var time = TimeSpan.FromMilliseconds(Math.Max(0, ms));
        return string.Format(CultureInfo.InvariantCulture, "{0:00}:{1:00}:{2:00},{3:000}", (int)time.TotalHours, time.Minutes, time.Seconds, time.Milliseconds);
    }
}
