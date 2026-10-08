using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubtitleEdit.Plugins.Shared;
using SubtitleEdit.Plugins.Transliterate.Engine;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text.Json;

namespace SubtitleEdit.Plugins.Transliterate;

public sealed partial class MainViewModel : ObservableObject
{
    private const int SettingsVersion = 1;
    public const string DefaultKeepWords = "OK, TV, DJ, CD, DVD, PC, SMS, FBI, CIA, USB, GPS";

    private static readonly Dictionary<string, string> Glyphs = new()
    {
        ["Cyrillic"] = "Ж", ["Greek"] = "Ω", ["Hangul"] = "한", ["Latin"] = "A",
    };

    private readonly PluginRequest _request;
    private readonly bool _legacyRequest;
    private readonly List<PluginParagraph> _paragraphs;
    private readonly long _videoOffsetMs;
    private readonly Dictionary<string, int> _systemByLanguage = new();
    private bool _loading;

    [ObservableProperty] private LanguageOption _selectedLanguage;
    [ObservableProperty] private Direction _direction = Direction.ToLatin;
    [ObservableProperty] private int _systemIndex;
    [ObservableProperty] private string _keepWordsText = DefaultKeepWords;
    [ObservableProperty] private bool _selectedLinesOnly;
    [ObservableProperty] private int _selectedCount;
    [ObservableProperty] private string _detectedText = string.Empty;

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
        LanguageOptions = new ObservableCollection<LanguageOption>(Languages.All.Select(l => new LanguageOption(l)));
        _selectedLanguage = LanguageOptions[0];

        _loading = true;
        var detected = ScriptDetector.Detect(_paragraphs.Select(p => p.Text ?? string.Empty));
        if (detected is { } d)
        {
            var option = LanguageOptions.First(o => o.Language == d.Language);
            option.IsDetected = true;
            DetectedText = $"{d.Language.Name} · {(d.Direction == Direction.ToLatin ? d.Language.Script : "Latin")}";
        }

        SelectedLinesOnly = HasSelectedLines;
        LoadSettings(detected);
        _loading = false;
        SyncLanguage();
        Analyze();
    }

    public ObservableCollection<LanguageOption> LanguageOptions { get; }
    public ObservableCollection<SystemOption> Systems { get; } = new();
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

    private Transliterator Language => SelectedLanguage.Language;
    public bool HasDetected => DetectedText.Length > 0;
    public bool CanSwap => Language.CanReverse;
    public string SwapToolTip => CanSwap ? "Swap direction" : $"{Language.Name} converts to Latin only";
    public bool IsFromLatin => Direction == Direction.FromLatin;
    public bool HasSystems => Systems.Count > 1 && Direction == Direction.ToLatin;
    public string SystemDescription => HasSystems ? Systems[Math.Clamp(SystemIndex, 0, Systems.Count - 1)].Description : string.Empty;
    public string FromScript => Direction == Direction.ToLatin ? Language.Script : "Latin";
    public string ToScript => Direction == Direction.ToLatin ? "Latin" : Language.Script;
    public string FromGlyph => Glyphs[FromScript];
    public string ToGlyph => Glyphs[ToScript];
    public string LanguageTitle => $"{Language.Name}";

    public string ExampleBefore => Direction == Direction.ToLatin
        ? Language.Sample
        : TextTransliterator.Convert(Language.Sample, Language, Direction.ToLatin, 0);

    public string ExampleAfter => TextTransliterator.Convert(ExampleBefore, Language, Direction, SystemIndex, KeepWords);

    public bool HasSelectedLines => SelectedIndices.Count > 0;
    public string SelectedLinesText => SelectedIndices.Count == 1 ? "Selected line (1)" : $"Selected lines ({SelectedIndices.Count:N0})";
    public bool AllLines { get => !SelectedLinesOnly; set => SelectedLinesOnly = !value; }
    public int ChangeCount => Changes.Count;
    public bool HasChanges => Changes.Count > 0;
    public bool IsClean => Changes.Count == 0;
    public bool CanApply => SelectedCount > 0;
    public string CleanText => $"No {FromScript} text found in {(SelectedLinesOnly && HasSelectedLines ? "the selected lines" : "the subtitle")}.";

    public string StatusText => Changes.Count == 0
        ? $"{LinesInScope:N0} lines checked"
        : $"{SelectedCount:N0} of {Changes.Count:N0} lines selected";

    private int LinesInScope => SelectedLinesOnly && HasSelectedLines ? SelectedIndices.Count : _paragraphs.Count;

    private IReadOnlySet<string> KeepWords => KeepWordsText
        .Split(new[] { ',', ';', ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries)
        .ToHashSet(StringComparer.OrdinalIgnoreCase);

    partial void OnSelectedLanguageChanged(LanguageOption value)
    {
        if (_loading)
        {
            return;
        }

        if (!value.Language.CanReverse)
        {
            Direction = Direction.ToLatin;
        }

        SyncLanguage();
        Analyze();
    }

    partial void OnDirectionChanged(Direction value)
    {
        RaiseLanguageState();
        Analyze();
    }

    partial void OnSystemIndexChanged(int value)
    {
        if (_loading)
        {
            return;
        }

        _systemByLanguage[Language.Id] = value;
        foreach (var system in Systems)
        {
            system.IsSelected = system.Index == value;
        }

        RaiseLanguageState();
        Analyze();
    }

    partial void OnKeepWordsTextChanged(string value)
    {
        OnPropertyChanged(nameof(ExampleAfter));
        Analyze();
    }

    partial void OnSelectedLinesOnlyChanged(bool value)
    {
        OnPropertyChanged(nameof(AllLines));
        OnPropertyChanged(nameof(CleanText));
        Analyze();
    }

    partial void OnDetectedTextChanged(string value) => OnPropertyChanged(nameof(HasDetected));

    partial void OnSelectedCountChanged(int value)
    {
        OnPropertyChanged(nameof(CanApply));
        OnPropertyChanged(nameof(StatusText));
    }

    /// <summary>Selection highlight, system buttons and the direction card for the current language.</summary>
    private void SyncLanguage()
    {
        foreach (var option in LanguageOptions)
        {
            option.IsSelected = option == SelectedLanguage;
        }

        Systems.Clear();
        var systems = Language.Systems;
        for (var i = 0; i < systems.Count; i++)
        {
            Systems.Add(new SystemOption(i, systems[i]));
        }

        var index = _systemByLanguage.TryGetValue(Language.Id, out var saved) && saved < systems.Count ? saved : 0;
        _loading = true;
        SystemIndex = index;
        _loading = false;
        foreach (var system in Systems)
        {
            system.IsSelected = system.Index == index;
        }

        RaiseLanguageState();
    }

    private void RaiseLanguageState()
    {
        foreach (var name in new[]
                 {
                     nameof(CanSwap), nameof(SwapToolTip), nameof(IsFromLatin), nameof(HasSystems), nameof(SystemDescription),
                     nameof(FromScript), nameof(ToScript), nameof(FromGlyph), nameof(ToGlyph), nameof(LanguageTitle),
                     nameof(ExampleBefore), nameof(ExampleAfter), nameof(CleanText),
                 })
        {
            OnPropertyChanged(name);
        }
    }

    private void Analyze()
    {
        if (_loading)
        {
            return;
        }

        var keep = KeepWords;
        var indices = SelectedLinesOnly && HasSelectedLines ? SelectedIndices : Enumerable.Range(0, _paragraphs.Count);
        Changes.Clear();
        foreach (var index in indices)
        {
            var text = _paragraphs[index].Text ?? string.Empty;
            var after = TextTransliterator.Convert(text, Language, Direction, SystemIndex, keep);
            if (after == text)
            {
                continue;
            }

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

        SelectedCount = Changes.Count;
        OnPropertyChanged(nameof(ChangeCount));
        OnPropertyChanged(nameof(HasChanges));
        OnPropertyChanged(nameof(IsClean));
        OnPropertyChanged(nameof(StatusText));
    }

    [RelayCommand]
    private void SelectLanguage(LanguageOption option) => SelectedLanguage = option;

    [RelayCommand]
    private void SelectSystem(SystemOption option) => SystemIndex = option.Index;

    [RelayCommand]
    private void Swap()
    {
        if (CanSwap)
        {
            Direction = Direction == Direction.ToLatin ? Direction.FromLatin : Direction.ToLatin;
        }
    }

    [RelayCommand]
    private void ResetKeepWords() => KeepWordsText = DefaultKeepWords;

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

    /// <summary>The response for OK: the selected lines in the new script.</summary>
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

        var what = $"{Language.Name} {FromScript} → {ToScript}";
        return new PluginResponse
        {
            Status = PluginStatus.Ok,
            Message = selected.Count == 1 ? $"Transliterate: 1 line, {what}." : $"Transliterate: {selected.Count:N0} lines, {what}.",
            UndoDescription = $"Transliterate ({what})",
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
            ["language"] = Language.Id,
            ["direction"] = Direction.ToString(),
            ["systems"] = _systemByLanguage,
            ["keepWords"] = KeepWordsText,
            ["selectedLinesOnly"] = SelectedLinesOnly,
        };
        return JsonSerializer.SerializeToElement(payload);
    }

    private void LoadSettings((Transliterator Language, Direction Direction)? detected)
    {
        Transliterator? language = null;
        var direction = Direction.ToLatin;
        if (_request.SettingsVersion == SettingsVersion && _request.Settings is { ValueKind: JsonValueKind.Object } settings)
        {
            if (settings.TryGetProperty("language", out var id) && id.ValueKind == JsonValueKind.String)
            {
                language = Languages.Find(id.GetString());
            }

            if (settings.TryGetProperty("direction", out var dir) && Enum.TryParse<Direction>(dir.GetString(), out var parsed))
            {
                direction = parsed;
            }

            if (settings.TryGetProperty("systems", out var systems) && systems.ValueKind == JsonValueKind.Object)
            {
                foreach (var p in systems.EnumerateObject())
                {
                    if (p.Value.TryGetInt32(out var index) && index >= 0)
                    {
                        _systemByLanguage[p.Name] = index;
                    }
                }
            }

            if (settings.TryGetProperty("keepWords", out var keep) && keep.ValueKind == JsonValueKind.String)
            {
                KeepWordsText = keep.GetString() ?? DefaultKeepWords;
            }

            if (settings.TryGetProperty("selectedLinesOnly", out var selectedOnly) && selectedOnly.ValueKind is JsonValueKind.True or JsonValueKind.False)
            {
                SelectedLinesOnly = selectedOnly.GetBoolean() && HasSelectedLines;
            }
        }

        // Follow the detection, except where it is only a guess between related languages: then the
        // language used last time wins (a Cyrillic file looks Russian unless it has telltale letters).
        if (detected is { } found)
        {
            var weakGuess = found.Direction == Direction.FromLatin || found.Language.Id == "ru";
            var keepSaved = language != null && weakGuess && direction == found.Direction &&
                            language.Script == found.Language.Script && (direction == Direction.ToLatin || language.CanReverse);
            if (!keepSaved)
            {
                language = found.Language;
                direction = found.Direction;
            }
        }

        language ??= Languages.All[0];
        SelectedLanguage = LanguageOptions.First(o => o.Language == language);
        Direction = language.CanReverse ? direction : Direction.ToLatin;
    }

    private string FormatTimeRange(PluginParagraph p) =>
        $"{FormatTime(p.StartMs + _videoOffsetMs)}  →  {FormatTime(p.EndMs + _videoOffsetMs)}";

    private static string FormatTime(double ms)
    {
        var time = TimeSpan.FromMilliseconds(Math.Max(0, ms));
        return string.Format(CultureInfo.InvariantCulture, "{0:00}:{1:00}:{2:00},{3:000}", (int)time.TotalHours, time.Minutes, time.Seconds, time.Milliseconds);
    }
}
