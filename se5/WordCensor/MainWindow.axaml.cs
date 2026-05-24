using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using SubtitleEdit.Plugins.Shared;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text.Json;

namespace SubtitleEdit.Plugins.WordCensor;

public partial class MainWindow : Window
{
    private const string DefaultReplacement = "[censored]";

    private readonly PluginRequest _request;
    private readonly List<SrtBlock> _blocks;
    private readonly WordCensorEngine _engine;
    private readonly ObservableCollection<ChangeProposal> _proposals = new();

    private TextBlock _summaryLabel = null!;
    private TextBlock _subtitleLabel = null!;
    private TextBlock _noChangesLabel = null!;
    private ListBox _changesList = null!;
    private Button _applyButton = null!;
    private CheckBox _colorRedCheck = null!;
    private RadioButton _grawlixRadio = null!;
    private RadioButton _replacementRadio = null!;
    private RadioButton _alternativeRadio = null!;
    private TextBox _replacementBox = null!;

    public MainWindow() : this(new PluginRequest()) { }

    public MainWindow(PluginRequest request)
    {
        _request = request;
        InitializeComponent();

        _engine = new WordCensorEngine();
        _blocks = SubRipParser.Parse(request.Subtitle.SubRip);

        var (mode, replacement, colorRed) = LoadSettings(request.Settings);
        SetMode(mode);
        _replacementBox.Text = replacement;
        _colorRedCheck.IsChecked = colorRed;

        _grawlixRadio.IsCheckedChanged += (_, _) => OnModeOrOptionChanged();
        _replacementRadio.IsCheckedChanged += (_, _) => OnModeOrOptionChanged();
        _alternativeRadio.IsCheckedChanged += (_, _) => OnModeOrOptionChanged();
        _colorRedCheck.IsCheckedChanged += (_, _) => OnModeOrOptionChanged();
        _replacementBox.TextChanged += (_, _) =>
        {
            // Only re-build when the textbox actually affects the preview.
            if (CurrentMode() == CensorMode.Replacement)
            {
                OnModeOrOptionChanged();
            }
        };

        BuildProposals();
        _changesList.ItemsSource = _proposals;

        var scope = request.SelectedIndices.Count > 0
            ? $"the {request.SelectedIndices.Count} selected line(s)"
            : "all lines";
        _subtitleLabel.Text = $"Replace offensive words in {scope}. Pick a style below.";

        UpdateUiForProposals();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        _summaryLabel = this.FindControl<TextBlock>("SummaryLabel")!;
        _subtitleLabel = this.FindControl<TextBlock>("SubtitleLabel")!;
        _noChangesLabel = this.FindControl<TextBlock>("NoChangesLabel")!;
        _changesList = this.FindControl<ListBox>("ChangesList")!;
        _applyButton = this.FindControl<Button>("ApplyButton")!;
        _colorRedCheck = this.FindControl<CheckBox>("ColorRedCheck")!;
        _grawlixRadio = this.FindControl<RadioButton>("GrawlixModeRadio")!;
        _replacementRadio = this.FindControl<RadioButton>("ReplacementModeRadio")!;
        _alternativeRadio = this.FindControl<RadioButton>("AlternativeModeRadio")!;
        _replacementBox = this.FindControl<TextBox>("ReplacementBox")!;
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        this.BringToForeground();
    }

    private CensorOptions BuildOptions() => new()
    {
        Mode = CurrentMode(),
        Replacement = string.IsNullOrEmpty(_replacementBox.Text) ? DefaultReplacement : _replacementBox.Text,
        ColorRed = _colorRedCheck.IsChecked == true,
    };

    private CensorMode CurrentMode()
    {
        if (_replacementRadio.IsChecked == true) return CensorMode.Replacement;
        if (_alternativeRadio.IsChecked == true) return CensorMode.Alternative;
        return CensorMode.Grawlix;
    }

    private void SetMode(CensorMode mode)
    {
        _grawlixRadio.IsChecked = mode == CensorMode.Grawlix;
        _replacementRadio.IsChecked = mode == CensorMode.Replacement;
        _alternativeRadio.IsChecked = mode == CensorMode.Alternative;
    }

    private void BuildProposals()
    {
        var options = BuildOptions();
        var selected = new HashSet<int>(_request.SelectedIndices);
        var applyToAll = selected.Count == 0;

        foreach (var p in _proposals)
        {
            p.PropertyChanged -= OnProposalChanged;
        }
        _proposals.Clear();

        for (var i = 0; i < _blocks.Count; i++)
        {
            if (!applyToAll && !selected.Contains(i))
            {
                continue;
            }

            if (_engine.TryCensor(_blocks[i].Text, options, out var censored))
            {
                var proposal = new ChangeProposal(i, _blocks[i].Text, censored);
                proposal.PropertyChanged += OnProposalChanged;
                _proposals.Add(proposal);
            }
        }
    }

    private void OnModeOrOptionChanged()
    {
        // Re-censor existing proposals so the new mode/colour/text is reflected in the preview.
        // Grawlix re-randomises and Alternative re-rolls, which is fine — the user is comparing approaches.
        var previousInclude = _proposals.ToDictionary(p => p.LineIndex, p => p.Include);
        BuildProposals();
        foreach (var p in _proposals)
        {
            if (previousInclude.TryGetValue(p.LineIndex, out var inc))
            {
                p.Include = inc;
            }
        }
        UpdateUiForProposals();
    }

    private void UpdateUiForProposals()
    {
        if (_proposals.Count == 0)
        {
            _noChangesLabel.IsVisible = true;
            _changesList.IsVisible = false;
            _applyButton.IsEnabled = false;
            _summaryLabel.Text = string.Empty;
        }
        else
        {
            _noChangesLabel.IsVisible = false;
            _changesList.IsVisible = true;
            UpdateSummary();
        }
    }

    private void OnProposalChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ChangeProposal.Include))
        {
            UpdateSummary();
        }
    }

    private void UpdateSummary()
    {
        var total = _proposals.Count;
        var selected = _proposals.Count(p => p.Include);
        _summaryLabel.Text = $"{selected} of {total} line(s) selected.";
        _applyButton.IsEnabled = selected > 0;
    }

    private void OnSelectAll(object? sender, RoutedEventArgs e)
    {
        foreach (var proposal in _proposals)
        {
            proposal.Include = true;
        }
    }

    private void OnSelectNone(object? sender, RoutedEventArgs e)
    {
        foreach (var proposal in _proposals)
        {
            proposal.Include = false;
        }
    }

    private void OnCancel(object? sender, RoutedEventArgs e)
    {
        App.Response = new PluginResponse { Status = PluginStatus.Cancelled };
        Close();
    }

    private void OnApply(object? sender, RoutedEventArgs e)
    {
        try
        {
            var changesByLine = _proposals
                .Where(p => p.Include)
                .ToDictionary(p => p.LineIndex, p => p.CensoredText);

            if (changesByLine.Count == 0)
            {
                App.Response = new PluginResponse { Status = PluginStatus.Cancelled };
                Close();
                return;
            }

            for (var i = 0; i < _blocks.Count; i++)
            {
                if (changesByLine.TryGetValue(i, out var newText))
                {
                    _blocks[i].Text = newText;
                }
            }

            App.Response = new PluginResponse
            {
                Status = PluginStatus.Ok,
                Message = $"Censored words in {changesByLine.Count} line(s).",
                UndoDescription = "Word censor",
                Subtitle = new PluginSubtitle
                {
                    Format = "SubRip",
                    Native = SubRipParser.Serialize(_blocks),
                },
                Settings = BuildSettings(BuildOptions()),
            };
        }
        catch (Exception ex)
        {
            App.Response = new PluginResponse { Status = PluginStatus.Error, Message = ex.Message };
        }

        Close();
    }

    private static (CensorMode mode, string replacement, bool colorRed) LoadSettings(JsonElement? settings)
    {
        var mode = CensorMode.Grawlix;
        var replacement = DefaultReplacement;
        var colorRed = false;

        if (settings is null || settings.Value.ValueKind != JsonValueKind.Object)
        {
            return (mode, replacement, colorRed);
        }

        var root = settings.Value;

        if (root.TryGetProperty("mode", out var modeProp) && modeProp.ValueKind == JsonValueKind.String &&
            Enum.TryParse<CensorMode>(modeProp.GetString(), ignoreCase: true, out var parsed))
        {
            mode = parsed;
        }
        if (root.TryGetProperty("replacement", out var rep) && rep.ValueKind == JsonValueKind.String)
        {
            var s = rep.GetString();
            if (!string.IsNullOrEmpty(s))
            {
                replacement = s;
            }
        }
        if (root.TryGetProperty("colorRed", out var cr) && cr.ValueKind == JsonValueKind.True)
        {
            colorRed = true;
        }

        return (mode, replacement, colorRed);
    }

    private static JsonElement BuildSettings(CensorOptions options)
    {
        var dto = new
        {
            mode = options.Mode.ToString(),
            replacement = options.Replacement,
            colorRed = options.ColorRed,
        };
        var json = JsonSerializer.Serialize(dto);
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.Clone();
    }
}
