using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubtitleEdit.Plugins.Shared;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text.Json;

namespace SubtitleEdit.Plugins.ItalicEachLine;

public sealed partial class MainViewModel : ObservableObject
{
    private const int SettingsVersion = 1;

    private readonly PluginRequest _request;
    private readonly bool _legacyRequest;
    private readonly List<PluginParagraph> _paragraphs;
    private readonly long _videoOffsetMs;
    private bool _loading;

    [ObservableProperty] private ItalicMode _mode = ItalicMode.EachLine;
    [ObservableProperty] private bool _selectedLinesOnly;
    [ObservableProperty] private bool _showRendered;
    [ObservableProperty] private int _selectedCount;
    [ObservableProperty] private int _oneTagCount;
    [ObservableProperty] private int _eachLineCount;

    public MainViewModel(PluginRequest request)
    {
        _request = request;
        _videoOffsetMs = request.Rules?.VideoOffsetMs ?? 0;
        _legacyRequest = request.Subtitle.Paragraphs == null;
        _paragraphs = _legacyRequest
            ? SubRipParser.Parse(request.Subtitle.SubRip)
                .Select(b => new PluginParagraph { StartMs = b.StartMs, EndMs = b.EndMs, Text = b.Text }).ToList()
            : request.Subtitle.Paragraphs!.Select(p => p.Clone()).ToList();
        SelectedIndices = request.SelectedIndices.Where(i => i >= 0 && i < _paragraphs.Count).Distinct().Order().ToList();

        _loading = true;
        LoadSettings();
        _loading = false;
        Analyze();
    }

    public ObservableCollection<ChangeItem> Changes { get; } = new();
    public IReadOnlyList<int> SelectedIndices { get; }

    public string SubtitleInfo
    {
        get
        {
            var name = string.IsNullOrEmpty(_request.Subtitle.FileName) ? "Untitled" : Path.GetFileName(_request.Subtitle.FileName);
            var format = string.IsNullOrEmpty(_request.Subtitle.Format) ? string.Empty : $"  ·  {_request.Subtitle.Format}";
            return $"{name}{format}  ·  {_paragraphs.Count:N0} lines";
        }
    }

    public bool IsOneTag
    {
        get => Mode == ItalicMode.OneTag;
        set
        {
            if (value)
            {
                Mode = ItalicMode.OneTag;
            }
        }
    }

    public bool IsEachLine
    {
        get => Mode == ItalicMode.EachLine;
        set
        {
            if (value)
            {
                Mode = ItalicMode.EachLine;
            }
        }
    }

    public bool ShowTags
    {
        get => !ShowRendered;
        set => ShowRendered = !value;
    }

    public bool HasSelectedLines => SelectedIndices.Count > 0;
    public string SelectedLinesText => SelectedIndices.Count == 1 ? "Selected line (1)" : $"Selected lines ({SelectedIndices.Count:N0})";
    public bool AllLines { get => !SelectedLinesOnly; set => SelectedLinesOnly = !value; }
    public int ChangeCount => Changes.Count;
    public bool HasChanges => Changes.Count > 0;
    public bool IsClean => Changes.Count == 0;
    public bool CanApply => SelectedCount > 0;
    public string OneTagCountText => CountText(OneTagCount);
    public string EachLineCountText => CountText(EachLineCount);

    public string CleanText => Mode == ItalicMode.EachLine
        ? "Every italic line already has its own tags."
        : "No italic lines to join into one tag.";

    public string StatusText => Changes.Count == 0
        ? $"{LinesInScope:N0} lines checked"
        : $"{SelectedCount:N0} of {Changes.Count:N0} lines selected";

    private int LinesInScope => SelectedLinesOnly && HasSelectedLines ? SelectedIndices.Count : _paragraphs.Count;

    partial void OnModeChanged(ItalicMode value)
    {
        OnPropertyChanged(nameof(IsOneTag));
        OnPropertyChanged(nameof(IsEachLine));
        OnPropertyChanged(nameof(CleanText));
        Analyze();
    }

    partial void OnSelectedLinesOnlyChanged(bool value)
    {
        OnPropertyChanged(nameof(AllLines));
        Analyze();
    }

    partial void OnShowRenderedChanged(bool value) => OnPropertyChanged(nameof(ShowTags));
    partial void OnOneTagCountChanged(int value) => OnPropertyChanged(nameof(OneTagCountText));
    partial void OnEachLineCountChanged(int value) => OnPropertyChanged(nameof(EachLineCountText));

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

        var oneTag = 0;
        var eachLine = 0;
        Changes.Clear();
        foreach (var index in IndicesInScope)
        {
            var text = _paragraphs[index].Text ?? string.Empty;
            var toOneTag = ItalicConverter.ToOneTag(text);
            var toEachLine = ItalicConverter.ToEachLine(text);
            oneTag += toOneTag != text ? 1 : 0;
            eachLine += toEachLine != text ? 1 : 0;

            var after = Mode == ItalicMode.EachLine ? toEachLine : toOneTag;
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

        OneTagCount = oneTag;
        EachLineCount = eachLine;
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

    /// <summary>The response for OK: the selected lines with their new tags.</summary>
    public PluginResponse BuildResponse()
    {
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

        var what = Mode == ItalicMode.EachLine ? "italic on each line" : "one italic tag";
        return new PluginResponse
        {
            Status = PluginStatus.Ok,
            Message = selected.Count == 1 ? $"Italic each line: 1 line changed to {what}." : $"Italic each line: {selected.Count:N0} lines changed to {what}.",
            UndoDescription = "Italic each line",
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
            ["mode"] = Mode.ToString(),
            ["showRendered"] = ShowRendered,
            ["selectedLinesOnly"] = SelectedLinesOnly,
        };
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

        if (settings.TryGetProperty("mode", out var mode) && Enum.TryParse<ItalicMode>(mode.GetString(), out var value))
        {
            Mode = value;
        }

        if (settings.TryGetProperty("showRendered", out var rendered) && rendered.ValueKind is JsonValueKind.True or JsonValueKind.False)
        {
            ShowRendered = rendered.GetBoolean();
        }

        if (settings.TryGetProperty("selectedLinesOnly", out var selectedOnly) && selectedOnly.ValueKind is JsonValueKind.True or JsonValueKind.False)
        {
            SelectedLinesOnly = selectedOnly.GetBoolean() && HasSelectedLines;
        }
    }

    private static string CountText(int count) => count switch
    {
        0 => "Nothing to change",
        1 => "1 line",
        _ => $"{count:N0} lines",
    };

    private string FormatTimeRange(PluginParagraph p) =>
        $"{FormatTime(p.StartMs + _videoOffsetMs)}  →  {FormatTime(p.EndMs + _videoOffsetMs)}";

    private static string FormatTime(double ms)
    {
        var time = TimeSpan.FromMilliseconds(Math.Max(0, ms));
        return string.Format(CultureInfo.InvariantCulture, "{0:00}:{1:00}:{2:00},{3:000}", (int)time.TotalHours, time.Minutes, time.Seconds, time.Milliseconds);
    }
}
