using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubtitleEdit.Plugins.Shared;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text.Json;

namespace SubtitleEdit.Plugins.HI2UC;

public sealed record StyleOption(HiStyle Style, string Name, string Example)
{
    public override string ToString() => Name;
}

public sealed partial class MainViewModel : ObservableObject
{
    private const int SettingsVersion = 1;

    private readonly PluginRequest _request;
    private readonly bool _legacyRequest;
    private readonly List<PluginParagraph> _paragraphs;
    private readonly long _videoOffsetMs;
    private readonly bool _isConfigure;
    private bool _loading;

    [ObservableProperty] private StyleOption _selectedStyle;
    [ObservableProperty] private bool _moods = true;
    [ObservableProperty] private bool _narrator = true;
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

        StyleOptions = Enum.GetValues<HiStyle>().Select(style =>
        {
            var strategy = CasingStrategies.Create(style);
            return new StyleOption(style, strategy.Name, strategy.Example);
        }).ToList();
        _selectedStyle = StyleOptions[0];

        _loading = true;
        LoadSettings();
        _loading = false;
        Analyze();
    }

    public ObservableCollection<ChangeItem> Changes { get; } = new();
    public IReadOnlyList<StyleOption> StyleOptions { get; }
    public IReadOnlyList<int> SelectedIndices { get; }

    public HiOptions Options => new() { Style = SelectedStyle.Style, Moods = Moods, Narrator = Narrator };

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
    public string StyleExample => SelectedStyle.Example;

    public string CleanText => !Moods && !Narrator
        ? "Turn on moods or narrators to change anything."
        : $"All hearing-impaired text is already in {SelectedStyle.Name.ToLowerInvariant()}.";

    public string StatusText => _isConfigure
        ? "Used for every file in Batch convert"
        : Changes.Count == 0
        ? $"{LinesInScope:N0} lines checked"
        : $"{SelectedCount:N0} of {Changes.Count:N0} lines selected";

    private int LinesInScope => SelectedLinesOnly && HasSelectedLines ? SelectedIndices.Count : _paragraphs.Count;

    partial void OnSelectedStyleChanged(StyleOption value)
    {
        OnPropertyChanged(nameof(StyleExample));
        OnPropertyChanged(nameof(CleanText));
        Analyze();
    }

    partial void OnMoodsChanged(bool value)
    {
        OnPropertyChanged(nameof(CleanText));
        Analyze();
    }

    partial void OnNarratorChanged(bool value)
    {
        OnPropertyChanged(nameof(CleanText));
        Analyze();
    }

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
            var result = HiCasing.Convert(text, options);
            if (result.After != text)
            {
                var item = new ChangeItem(index, text, result.After, FormatTimeRange(_paragraphs[index]), result.Comment, result.Unbalanced);
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
    /// A "batch" request (Batch convert): change every line with the saved options, without UI.
    /// No changes is "ok" without a subtitle.
    /// </summary>
    public PluginResponse BuildBatchResponse()
    {
        if (Changes.Count == 0)
        {
            return new PluginResponse { Status = PluginStatus.Ok, Message = "Hearing impaired to uppercase: nothing to change." };
        }

        var response = BuildResponse();
        response.Settings = null;
        response.SettingsVersion = null;
        return response;
    }

    /// <summary>A "configure" request: hand back the settings only, for the next Batch convert run.</summary>
    public PluginResponse BuildConfigureResponse() =>
        new() { Status = PluginStatus.Ok, Settings = BuildSettings(), SettingsVersion = SettingsVersion };

    /// <summary>The response for OK: the selected lines with their new casing.</summary>
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
                ? "Hearing impaired to uppercase: 1 line changed."
                : $"Hearing impaired to uppercase: {selected.Count:N0} lines changed.",
            UndoDescription = "Hearing impaired to uppercase",
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
            ["style"] = SelectedStyle.Style.ToString(),
            ["moods"] = Moods,
            ["narrator"] = Narrator,
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

        if (settings.TryGetProperty("style", out var style) && style.ValueKind == JsonValueKind.String &&
            Enum.TryParse<HiStyle>(style.GetString(), out var value) && StyleOptions.FirstOrDefault(o => o.Style == value) is { } option)
        {
            SelectedStyle = option;
        }

        if (settings.TryGetProperty("moods", out var moods) && moods.ValueKind is JsonValueKind.True or JsonValueKind.False)
        {
            Moods = moods.GetBoolean();
        }

        if (settings.TryGetProperty("narrator", out var narrator) && narrator.ValueKind is JsonValueKind.True or JsonValueKind.False)
        {
            Narrator = narrator.GetBoolean();
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
