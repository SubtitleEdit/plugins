using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using SubtitleEdit.Plugins.Shared;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace SubtitleEdit.Plugins.AmericanToBritish;

public partial class MainWindow : Window
{
    private readonly PluginRequest _request;
    private readonly List<SrtBlock> _blocks;
    private readonly ObservableCollection<ChangeProposal> _proposals = new();
    private readonly string _wordListPath = LocalWordList.GetDefaultPath("AmericanToBritish.xml");

    private TextBlock _summaryLabel = null!;
    private TextBlock _subtitleLabel = null!;
    private TextBlock _noChangesLabel = null!;
    private ListBox _changesList = null!;
    private Button _applyButton = null!;

    public MainWindow() : this(new PluginRequest()) { }

    public MainWindow(PluginRequest request)
    {
        _request = request;
        InitializeComponent();

        _blocks = SubRipParser.Parse(request.Subtitle.SubRip);
        _changesList.ItemsSource = _proposals;
        RefreshProposals();
    }

    private void RefreshProposals()
    {
        foreach (var proposal in _proposals)
        {
            proposal.PropertyChanged -= OnProposalChanged;
        }
        _proposals.Clear();

        var scope = _request.SelectedIndices.Count > 0
            ? $"the {_request.SelectedIndices.Count} selected line(s)"
            : "all lines";
        _subtitleLabel.Text = $"Convert American to British English spellings in {scope}.";

        LocalWordList? localWordList = null;
        try
        {
            localWordList = LocalWordList.Load(_wordListPath);
        }
        catch (Exception ex)
        {
            _subtitleLabel.Text += $" Local word list ignored - could not read {_wordListPath}: {ex.Message}";
        }

        BuildProposals(localWordList);

        var hasChanges = _proposals.Count > 0;
        _noChangesLabel.IsVisible = !hasChanges;
        _changesList.IsVisible = hasChanges;
        if (!hasChanges)
        {
            _applyButton.IsEnabled = false;
            _summaryLabel.Text = string.Empty;
            return;
        }

        foreach (var proposal in _proposals)
        {
            proposal.PropertyChanged += OnProposalChanged;
        }
        UpdateSummary();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        _summaryLabel = this.FindControl<TextBlock>("SummaryLabel")!;
        _subtitleLabel = this.FindControl<TextBlock>("SubtitleLabel")!;
        _noChangesLabel = this.FindControl<TextBlock>("NoChangesLabel")!;
        _changesList = this.FindControl<ListBox>("ChangesList")!;
        _applyButton = this.FindControl<Button>("ApplyButton")!;
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        this.BringToForeground();
    }

    private void BuildProposals(LocalWordList? localWordList)
    {
        var converter = new EnglishVariantConverter(EnglishVariantDirection.UsToBr, localWordList);
        var selected = new HashSet<int>(_request.SelectedIndices);
        var applyToAll = selected.Count == 0;

        for (var i = 0; i < _blocks.Count; i++)
        {
            if (!applyToAll && !selected.Contains(i))
            {
                continue;
            }

            if (converter.TryConvert(_blocks[i].Text, out var converted))
            {
                _proposals.Add(new ChangeProposal(i, _blocks[i].Text, converted));
            }
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
        _summaryLabel.Text = $"{selected} of {total} change(s) selected.";
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

    private async void OnEditWordList(object? sender, RoutedEventArgs e)
    {
        var list = new LocalWordList();
        string? loadError = null;
        try
        {
            list = LocalWordList.Load(_wordListPath);
        }
        catch (Exception ex)
        {
            loadError = $"Could not read the existing file ({ex.Message}). Clicking OK replaces it with the list below.";
        }

        var saved = await new WordListWindow(_wordListPath, list, loadError).ShowDialog<bool>(this);
        if (saved)
        {
            RefreshProposals();
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
                .ToDictionary(p => p.LineIndex, p => p.ConvertedText);

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
                Message = $"Converted {changesByLine.Count} line(s) to British English.",
                UndoDescription = "American to British",
                Subtitle = new PluginSubtitle
                {
                    Format = "SubRip",
                    Native = SubRipParser.Serialize(_blocks),
                },
            };
        }
        catch (Exception ex)
        {
            App.Response = new PluginResponse { Status = PluginStatus.Error, Message = ex.Message };
        }

        Close();
    }
}
