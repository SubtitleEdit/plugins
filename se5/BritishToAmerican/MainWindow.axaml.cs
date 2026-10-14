using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using SubtitleEdit.Plugins.Shared;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace SubtitleEdit.Plugins.BritishToAmerican;

public partial class MainWindow : Window
{
    private readonly PluginRequest _request;
    private readonly List<SrtBlock> _blocks;
    private readonly ObservableCollection<ChangeProposal> _proposals = new();
    private readonly bool _isConfigure;

    private TextBlock _summaryLabel = null!;
    private TextBlock _subtitleLabel = null!;
    private TextBlock _noChangesLabel = null!;
    private ListBox _changesList = null!;
    private Button _applyButton = null!;
    private Control _selectButtons = null!;
    private Control _changesBorder = null!;
    private Control _configurePanel = null!;

    public MainWindow() : this(new PluginRequest()) { }

    public MainWindow(PluginRequest request)
    {
        _request = request;
        _isConfigure = request.RequestType == PluginRequestType.Configure;
        InitializeComponent();

        if (_isConfigure)
        {
            // Opened from Batch convert's Configure button: no subtitle, and nothing to configure.
            _blocks = new List<SrtBlock>();
            _subtitleLabel.Text = "Settings for Batch convert.";
            _selectButtons.IsVisible = false;
            _changesBorder.IsVisible = false;
            _configurePanel.IsVisible = true;
            _applyButton.Content = "OK";
            _applyButton.IsEnabled = true;
            _summaryLabel.Text = string.Empty;
            return;
        }

        _blocks = SubRipParser.Parse(request.Subtitle.SubRip);
        BuildProposals();

        _changesList.ItemsSource = _proposals;

        var scope = request.SelectedIndices.Count > 0
            ? $"the {request.SelectedIndices.Count} selected line(s)"
            : "all lines";
        _subtitleLabel.Text = $"Convert British to American English spellings in {scope}.";

        if (_proposals.Count == 0)
        {
            _noChangesLabel.IsVisible = true;
            _changesList.IsVisible = false;
            _applyButton.IsEnabled = false;
            _summaryLabel.Text = string.Empty;
        }
        else
        {
            foreach (var proposal in _proposals)
            {
                proposal.PropertyChanged += OnProposalChanged;
            }
            UpdateSummary();
        }
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        _summaryLabel = this.FindControl<TextBlock>("SummaryLabel")!;
        _subtitleLabel = this.FindControl<TextBlock>("SubtitleLabel")!;
        _noChangesLabel = this.FindControl<TextBlock>("NoChangesLabel")!;
        _changesList = this.FindControl<ListBox>("ChangesList")!;
        _applyButton = this.FindControl<Button>("ApplyButton")!;
        _selectButtons = this.FindControl<Control>("SelectButtons")!;
        _changesBorder = this.FindControl<Control>("ChangesBorder")!;
        _configurePanel = this.FindControl<Control>("ConfigurePanel")!;
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        this.BringToForeground();
    }

    private void BuildProposals()
    {
        foreach (var proposal in Conversion.FindChanges(_blocks, _request.SelectedIndices))
        {
            _proposals.Add(proposal);
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

    private void OnCancel(object? sender, RoutedEventArgs e)
    {
        App.Response = new PluginResponse { Status = PluginStatus.Cancelled };
        Close();
    }

    private void OnApply(object? sender, RoutedEventArgs e)
    {
        if (_isConfigure)
        {
            App.Response = Conversion.BuildConfigureResponse();
            Close();
            return;
        }

        try
        {
            var response = Conversion.Apply(_blocks, _proposals);
            if (response == null)
            {
                App.Response = new PluginResponse { Status = PluginStatus.Cancelled };
                Close();
                return;
            }

            App.Response = response;
        }
        catch (Exception ex)
        {
            App.Response = new PluginResponse { Status = PluginStatus.Error, Message = ex.Message };
        }

        Close();
    }
}
