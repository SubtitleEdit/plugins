using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubtitleEdit.Plugins.Shared;
using SubtitleEdit.Plugins.FrenchTypography.Logic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text.Json;

namespace SubtitleEdit.Plugins.FrenchTypography.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
    private const int SettingsVersion = 1;

    private readonly PluginRequest _request;
    private readonly bool _legacyRequest;
    private readonly List<PluginParagraph> _paragraphs;
    private readonly long _videoOffsetMs;
    private readonly bool _isConfigure;
    private bool _loading;

    [ObservableProperty] public partial bool SelectedLinesOnly { get; set; }
    [ObservableProperty] public partial int SelectedCount { get; set; }
    [ObservableProperty] public partial bool IsCanada { get; set; }
    [ObservableProperty] public partial bool IsNarrowSpace { get; set; }
    [ObservableProperty] public partial bool ConvertQuotes { get; set; } = true;
    [ObservableProperty] public partial bool ConvertEllipsis { get; set; } = true;

    public MainViewModel(PluginRequest request)
    {
        // Property changes during setup must not analyze yet.
        _loading = true;

        _request = request;
        T = Strings.For(request.UiLanguage);
        _isConfigure = request.RequestType == PluginRequestType.Configure;
        _videoOffsetMs = request.Rules?.VideoOffsetMs ?? 0;
        _legacyRequest = request.Subtitle.Paragraphs == null;
        _paragraphs = _legacyRequest
            ? SubRipParser.Parse(request.Subtitle.SubRip)
                .Select(b => new PluginParagraph { StartMs = b.StartMs, EndMs = b.EndMs, Text = b.Text }).ToList()
            : request.Subtitle.Paragraphs!.Select(p => p.Clone()).ToList();
        SelectedIndices = request.SelectedIndices.Where(i => i >= 0 && i < _paragraphs.Count).Distinct().Order().ToList();

        LoadSettings();
        _loading = false;
        Analyze();
    }

    /// <summary>UI texts in Subtitle Edit's language (French or English).</summary>
    public Strings T { get; }

    public ObservableCollection<ChangeItem> Changes { get; } = new();
    public IReadOnlyList<int> SelectedIndices { get; }

    public string SubtitleInfo
    {
        get
        {
            if (_isConfigure)
            {
                return T.SettingsForBatch;
            }

            var name = string.IsNullOrEmpty(_request.Subtitle.FileName) ? T.Untitled : Path.GetFileName(_request.Subtitle.FileName);
            var format = string.IsNullOrEmpty(_request.Subtitle.Format) ? string.Empty : $"  ·  {_request.Subtitle.Format}";
            return $"{name}{format}  ·  {string.Format(T.LinesFormat, _paragraphs.Count)}";
        }
    }

    /// <summary>Opened from Batch convert's Configure button: options only, there is no subtitle.</summary>
    public bool IsConfigure => _isConfigure;
    public bool IsNotConfigure => !_isConfigure;

    public bool HasSelectedLines => SelectedIndices.Count > 0;
    public string SelectedLinesText => SelectedIndices.Count == 1 ? T.SelectedLine : string.Format(T.SelectedLinesFormat, SelectedIndices.Count);
    public bool IsFrance { get => !IsCanada; set => IsCanada = !value; }
    public bool IsNoBreakSpace { get => !IsNarrowSpace; set => IsNarrowSpace = !value; }
    public string ChangeCountText => string.Format(T.LinesToChangeFormat, Changes.Count);

    public FrenchTypographyOptions Options => new()
    {
        Variant = IsCanada ? FrenchVariant.Canada : FrenchVariant.France,
        Space = IsNarrowSpace ? FrenchSpace.Narrow : FrenchSpace.NoBreak,
        ConvertQuotes = ConvertQuotes,
        ConvertEllipsis = ConvertEllipsis,
    };
    public bool AllLines { get => !SelectedLinesOnly; set => SelectedLinesOnly = !value; }
    public int ChangeCount => Changes.Count;
    public bool HasChanges => Changes.Count > 0;
    public bool IsClean => Changes.Count == 0 && !_isConfigure;
    public bool CanApply => SelectedCount > 0 || _isConfigure;

    public string StatusText => _isConfigure
        ? T.UsedForBatch
        : Changes.Count == 0
        ? string.Format(T.LinesCheckedFormat, LinesInScope)
        : string.Format(T.SelectedOfFormat, SelectedCount, Changes.Count);

    private int LinesInScope => SelectedLinesOnly && HasSelectedLines ? SelectedIndices.Count : _paragraphs.Count;

    partial void OnSelectedLinesOnlyChanged(bool value)
    {
        OnPropertyChanged(nameof(AllLines));
        Analyze();
    }

    partial void OnIsCanadaChanged(bool value)
    {
        OnPropertyChanged(nameof(IsFrance));
        Analyze();
    }

    partial void OnIsNarrowSpaceChanged(bool value)
    {
        OnPropertyChanged(nameof(IsNoBreakSpace));
        Analyze();
    }

    partial void OnConvertQuotesChanged(bool value) => Analyze();

    partial void OnConvertEllipsisChanged(bool value) => Analyze();

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

        Changes.Clear();
        if (_isConfigure)
        {
            return;
        }

        var options = Options;
        foreach (var index in IndicesInScope)
        {
            var text = _paragraphs[index].Text ?? string.Empty;
            var after = FrenchTypographer.Fix(text, options);
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
        OnPropertyChanged(nameof(ChangeCountText));
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
    /// A "batch" request (Batch convert): fix every line with the saved options, without UI.
    /// No changes is "ok" without a subtitle.
    /// </summary>
    public PluginResponse BuildBatchResponse()
    {
        if (Changes.Count == 0)
        {
            return new PluginResponse { Status = PluginStatus.Ok, Message = T.NothingToChangeMessage };
        }

        var response = BuildResponse();
        response.Settings = null;
        response.SettingsVersion = null;
        return response;
    }

    /// <summary>A "configure" request: hand back the settings only, for the next Batch convert run.</summary>
    public PluginResponse BuildConfigureResponse() =>
        new() { Status = PluginStatus.Ok, Settings = BuildSettings(), SettingsVersion = SettingsVersion };

    /// <summary>The response for OK: the checked lines fixed.</summary>
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
                ? T.LineChangedMessage
                : string.Format(T.LinesChangedMessageFormat, selected.Count),
            UndoDescription = T.Title,
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
            ["selectedLinesOnly"] = SelectedLinesOnly,
            ["variant"] = IsCanada ? "canada" : "france",
            ["space"] = IsNarrowSpace ? "narrow" : "noBreak",
            ["convertQuotes"] = ConvertQuotes,
            ["convertEllipsis"] = ConvertEllipsis,
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

        if (settings.TryGetProperty("selectedLinesOnly", out var selectedOnly) && selectedOnly.ValueKind is JsonValueKind.True or JsonValueKind.False)
        {
            SelectedLinesOnly = selectedOnly.GetBoolean() && HasSelectedLines;
        }

        if (settings.TryGetProperty("variant", out var variant) && variant.ValueKind == JsonValueKind.String)
        {
            IsCanada = variant.GetString() == "canada";
        }

        if (settings.TryGetProperty("space", out var space) && space.ValueKind == JsonValueKind.String)
        {
            IsNarrowSpace = space.GetString() == "narrow";
        }

        if (settings.TryGetProperty("convertQuotes", out var quotes) && quotes.ValueKind is JsonValueKind.True or JsonValueKind.False)
        {
            ConvertQuotes = quotes.GetBoolean();
        }

        if (settings.TryGetProperty("convertEllipsis", out var ellipsis) && ellipsis.ValueKind is JsonValueKind.True or JsonValueKind.False)
        {
            ConvertEllipsis = ellipsis.GetBoolean();
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
