using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using SubtitleEdit.Plugins.Shared;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text.Json;

namespace SubtitleEdit.Plugins.WordCensor;

public partial class MainWindow : Window
{
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

    public MainWindow() : this(new PluginRequest()) { }

    public MainWindow(PluginRequest request)
    {
        _request = request;
        InitializeComponent();

        _engine = new WordCensorEngine();
        _blocks = SubRipParser.Parse(request.Subtitle.SubRip);

        _colorRedCheck.IsChecked = LoadColorRedSetting(request.Settings);
        _colorRedCheck.IsCheckedChanged += (_, _) => RebuildProposals();

        BuildProposals();
        _changesList.ItemsSource = _proposals;

        var scope = request.SelectedIndices.Count > 0
            ? $"the {request.SelectedIndices.Count} selected line(s)"
            : "all lines";
        _subtitleLabel.Text = $"Replace offensive words with grawlix characters (#@!$%) in {scope}.";

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
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        this.BringToForeground();
    }

    private void BuildProposals()
    {
        var colorRed = _colorRedCheck.IsChecked == true;
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

            if (_engine.TryCensor(_blocks[i].Text, colorRed, out var censored))
            {
                var proposal = new ChangeProposal(i, _blocks[i].Text, censored);
                proposal.PropertyChanged += OnProposalChanged;
                _proposals.Add(proposal);
            }
        }
    }

    private void RebuildProposals()
    {
        // Re-censor existing proposals so the colour-toggle is reflected in the preview.
        // The grawlix characters re-randomise, which is fine - the user is comparing approaches.
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
                Settings = BuildSettings(_colorRedCheck.IsChecked == true),
            };
        }
        catch (Exception ex)
        {
            App.Response = new PluginResponse { Status = PluginStatus.Error, Message = ex.Message };
        }

        Close();
    }

    private static bool LoadColorRedSetting(JsonElement? settings)
    {
        if (settings is null || settings.Value.ValueKind != JsonValueKind.Object)
        {
            return false;
        }
        if (settings.Value.TryGetProperty("colorRed", out var value) && value.ValueKind == JsonValueKind.True)
        {
            return true;
        }
        return false;
    }

    private static JsonElement BuildSettings(bool colorRed)
    {
        using var doc = JsonDocument.Parse($"{{\"colorRed\":{colorRed.ToString(CultureInfo.InvariantCulture).ToLowerInvariant()}}}");
        return doc.RootElement.Clone();
    }
}
