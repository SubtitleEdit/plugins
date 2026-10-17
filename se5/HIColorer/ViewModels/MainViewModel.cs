using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubtitleEdit.Plugins.HIColorer.Logic;
using SubtitleEdit.Plugins.Shared;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text.Json;

namespace SubtitleEdit.Plugins.HIColorer.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
    private const int SettingsVersion = 1;

    private readonly PluginRequest _request;
    private readonly bool _legacyRequest;
    private readonly List<PluginParagraph> _paragraphs;
    private readonly long _videoOffsetMs;
    private readonly bool _isConfigure;
    private bool _loading;

    [ObservableProperty] private ColorMode _mode = ColorMode.Color;
    [ObservableProperty] private bool _narrator = true;
    [ObservableProperty] private Color _narratorColor = Color.Parse(ColorOptions.DefaultNarratorColor);
    [ObservableProperty] private bool _moods = true;
    [ObservableProperty] private Color _moodsColor = Color.Parse(ColorOptions.DefaultMoodsColor);
    [ObservableProperty] private bool _music = true;
    [ObservableProperty] private Color _musicColor = Color.Parse(ColorOptions.DefaultMusicColor);
    [ObservableProperty] private bool _showRendered = true;
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

        _loading = true;
        LoadSettings();
        _loading = false;
        Analyze();
    }

    public ObservableCollection<ChangeItem> Changes { get; } = new();
    public IReadOnlyList<int> SelectedIndices { get; }

    public ColorOptions Options => new()
    {
        Narrator = Narrator,
        NarratorColor = ToHex(NarratorColor),
        Moods = Moods,
        MoodsColor = ToHex(MoodsColor),
        Music = Music,
        MusicColor = ToHex(MusicColor),
    };

    public bool IsColorMode
    {
        get => Mode == ColorMode.Color;
        set
        {
            if (value)
            {
                Mode = ColorMode.Color;
            }
        }
    }

    public bool IsRemoveMode
    {
        get => Mode == ColorMode.Remove;
        set
        {
            if (value)
            {
                Mode = ColorMode.Remove;
            }
        }
    }

    public bool ShowTags
    {
        get => !ShowRendered;
        set => ShowRendered = !value;
    }

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

    public string CleanText => Mode == ColorMode.Remove
        ? "No line has a <font> tag."
        : !Narrator && !Moods && !Music
        ? "Turn on narrators, moods or music to color anything."
        : "All hearing-impaired text already has these colors.";

    public string StatusText => _isConfigure
        ? "Used for every file in Batch convert"
        : Changes.Count == 0
        ? $"{LinesInScope:N0} lines checked"
        : $"{SelectedCount:N0} of {Changes.Count:N0} lines selected";

    private int LinesInScope => SelectedLinesOnly && HasSelectedLines ? SelectedIndices.Count : _paragraphs.Count;

    partial void OnModeChanged(ColorMode value)
    {
        OnPropertyChanged(nameof(IsColorMode));
        OnPropertyChanged(nameof(IsRemoveMode));
        OnPropertyChanged(nameof(CleanText));
        Analyze();
    }

    partial void OnNarratorChanged(bool value) => OptionChanged();
    partial void OnNarratorColorChanged(Color value) => OptionChanged();
    partial void OnMoodsChanged(bool value) => OptionChanged();
    partial void OnMoodsColorChanged(Color value) => OptionChanged();
    partial void OnMusicChanged(bool value) => OptionChanged();
    partial void OnMusicColorChanged(Color value) => OptionChanged();
    partial void OnShowRenderedChanged(bool value) => OnPropertyChanged(nameof(ShowTags));

    private void OptionChanged()
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
        var mode = Mode;
        Changes.Clear();
        foreach (var index in IndicesInScope)
        {
            var text = _paragraphs[index].Text ?? string.Empty;
            var after = HiColorer.Apply(text, mode, options);
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
    /// A "batch" request (Batch convert): color (or uncolor) every line with the saved options, without UI.
    /// No changes is "ok" without a subtitle.
    /// </summary>
    public PluginResponse BuildBatchResponse()
    {
        if (Changes.Count == 0)
        {
            return new PluginResponse { Status = PluginStatus.Ok, Message = "Hearing impaired colorer: nothing to change." };
        }

        var response = BuildResponse();
        response.Settings = null;
        response.SettingsVersion = null;
        return response;
    }

    /// <summary>A "configure" request: hand back the settings only, for the next Batch convert run.</summary>
    public PluginResponse BuildConfigureResponse() =>
        new() { Status = PluginStatus.Ok, Settings = BuildSettings(), SettingsVersion = SettingsVersion };

    /// <summary>The response for OK: the selected lines with their new colors.</summary>
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
                ? "Hearing impaired colorer: 1 line changed."
                : $"Hearing impaired colorer: {selected.Count:N0} lines changed.",
            UndoDescription = "Hearing impaired colorer",
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
            ["narrator"] = Narrator,
            ["narratorColor"] = ToHex(NarratorColor),
            ["moods"] = Moods,
            ["moodsColor"] = ToHex(MoodsColor),
            ["music"] = Music,
            ["musicColor"] = ToHex(MusicColor),
            ["showRendered"] = ShowRendered,
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

        if (settings.TryGetProperty("mode", out var mode) && mode.ValueKind == JsonValueKind.String && Enum.TryParse<ColorMode>(mode.GetString(), out var modeValue))
        {
            Mode = modeValue;
        }

        Narrator = ReadBool(settings, "narrator", Narrator);
        NarratorColor = ReadColor(settings, "narratorColor", NarratorColor);
        Moods = ReadBool(settings, "moods", Moods);
        MoodsColor = ReadColor(settings, "moodsColor", MoodsColor);
        Music = ReadBool(settings, "music", Music);
        MusicColor = ReadColor(settings, "musicColor", MusicColor);
        ShowRendered = ReadBool(settings, "showRendered", ShowRendered);

        if (settings.TryGetProperty("selectedLinesOnly", out var selectedOnly) && selectedOnly.ValueKind is JsonValueKind.True or JsonValueKind.False)
        {
            SelectedLinesOnly = selectedOnly.GetBoolean() && HasSelectedLines;
        }
    }

    private static bool ReadBool(JsonElement settings, string name, bool fallback) =>
        settings.TryGetProperty(name, out var value) && value.ValueKind is JsonValueKind.True or JsonValueKind.False ? value.GetBoolean() : fallback;

    private static Color ReadColor(JsonElement settings, string name, Color fallback) =>
        settings.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String && Color.TryParse(value.GetString(), out var color)
            ? color
            : fallback;

    /// <summary>"#rrggbb", the way Subtitle Edit writes font colors.</summary>
    public static string ToHex(Color color) => $"#{color.R:x2}{color.G:x2}{color.B:x2}";

    private string FormatTimeRange(PluginParagraph p) =>
        $"{FormatTime(p.StartMs + _videoOffsetMs)}  →  {FormatTime(p.EndMs + _videoOffsetMs)}";

    private static string FormatTime(double ms)
    {
        var time = TimeSpan.FromMilliseconds(Math.Max(0, ms));
        return string.Format(CultureInfo.InvariantCulture, "{0:00}:{1:00}:{2:00},{3:000}", (int)time.TotalHours, time.Minutes, time.Seconds, time.Milliseconds);
    }
}
