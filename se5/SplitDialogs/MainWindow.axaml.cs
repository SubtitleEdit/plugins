using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using SubtitleEdit.Plugins.Shared;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace SubtitleEdit.Plugins.SplitDialogs;

public partial class MainWindow : Window
{
    private readonly PluginRequest _request;
    private readonly bool _isConfigure;
    private readonly CueDocument _document;
    private readonly ObservableCollection<DialogRow> _rows = new();
    private readonly HashSet<int> _excludedCues = new();
    private readonly string _scope;

    private TextBlock _summaryLabel = null!;
    private TextBlock _subtitleLabel = null!;
    private TextBlock _noDialogsLabel = null!;
    private CheckBox _requireSentenceEndingCheckBox = null!;
    private NumericUpDown _gapInput = null!;
    private Border _headerBorder = null!;
    private Border _listBorder = null!;
    private ListBox _dialogsList = null!;
    private Button _applyButton = null!;
    private StackPanel _selectButtonsPanel = null!;
    private StackPanel _configurePanel = null!;

    public MainWindow() : this(new PluginRequest()) { }

    public MainWindow(PluginRequest request)
    {
        _request = request;
        _isConfigure = request.RequestType == PluginRequestType.Configure;
        InitializeComponent();

        _document = CueDocument.Create(request.Subtitle);
        _scope = request.SelectedIndices.Count > 0
            ? $"the {request.SelectedIndices.Count} selected line(s)"
            : "all lines";
        _subtitleLabel.Text = $"Dialog lines found in {_scope}. Each checked line becomes one subtitle per speaker, without the dash, and its time is shared out by text length.";

        var settings = SplitSettings.Load(request);
        _requireSentenceEndingCheckBox.IsChecked = settings.RequireSentenceEnding;
        _gapInput.Value = settings.GapMs;

        if (_isConfigure)
        {
            // Opened from Batch convert's Configure button: no subtitle, only the two options.
            _subtitleLabel.Text = "Settings for Batch convert.";
            _selectButtonsPanel.IsVisible = false;
            _headerBorder.IsVisible = false;
            _listBorder.IsVisible = false;
            _configurePanel.IsVisible = true;
            _summaryLabel.Text = "Used for every file in Batch convert";
            _applyButton.Content = "OK";
            _applyButton.IsEnabled = true;
            return;
        }

        _requireSentenceEndingCheckBox.IsCheckedChanged += (_, _) => BuildRows();
        _gapInput.ValueChanged += (_, _) => UpdateParts();

        _dialogsList.ItemsSource = _rows;
        BuildRows();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        _summaryLabel = this.FindControl<TextBlock>("SummaryLabel")!;
        _subtitleLabel = this.FindControl<TextBlock>("SubtitleLabel")!;
        _noDialogsLabel = this.FindControl<TextBlock>("NoDialogsLabel")!;
        _requireSentenceEndingCheckBox = this.FindControl<CheckBox>("RequireSentenceEndingCheckBox")!;
        _gapInput = this.FindControl<NumericUpDown>("GapInput")!;
        _headerBorder = this.FindControl<Border>("HeaderBorder")!;
        _listBorder = this.FindControl<Border>("ListBorder")!;
        _dialogsList = this.FindControl<ListBox>("DialogsList")!;
        _applyButton = this.FindControl<Button>("ApplyButton")!;
        _selectButtonsPanel = this.FindControl<StackPanel>("SelectButtonsPanel")!;
        _configurePanel = this.FindControl<StackPanel>("ConfigurePanel")!;
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        this.BringToForeground();
    }

    private int GapMs => (int)(_gapInput.Value ?? SplitSettings.DefaultGapMs);

    private void BuildRows()
    {
        // Toggling the sentence-ending rule rebuilds the list; lines the user unchecked stay unchecked.
        foreach (var row in _rows)
        {
            if (row.Include)
            {
                _excludedCues.Remove(row.Cue.Index);
            }
            else
            {
                _excludedCues.Add(row.Cue.Index);
            }

            row.PropertyChanged -= OnRowChanged;
        }

        _rows.Clear();

        var selected = new HashSet<int>(_request.SelectedIndices);
        var requireSentenceEnding = _requireSentenceEndingCheckBox.IsChecked == true;
        foreach (var cue in _document.Cues)
        {
            if (!cue.CanSplit || (selected.Count > 0 && !selected.Contains(cue.Index)))
            {
                continue;
            }

            var texts = DialogSplitter.Split(cue.Lines, requireSentenceEnding);
            if (texts == null)
            {
                continue;
            }

            var row = new DialogRow(cue, texts) { Include = !_excludedCues.Contains(cue.Index) };
            row.UpdateParts(GapMs);
            row.PropertyChanged += OnRowChanged;
            _rows.Add(row);
        }

        var hasRows = _rows.Count > 0;
        _noDialogsLabel.Text = $"No dialog lines found in {_scope}.";
        _noDialogsLabel.IsVisible = !hasRows;
        _headerBorder.IsVisible = hasRows;
        _listBorder.IsVisible = hasRows;
        UpdateSummary();
    }

    private void UpdateParts()
    {
        var gapMs = GapMs;
        foreach (var row in _rows)
        {
            row.UpdateParts(gapMs);
        }
    }

    private void OnRowChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(DialogRow.Include))
        {
            UpdateSummary();
        }
    }

    private void UpdateSummary()
    {
        var included = _rows.Where(r => r.Include).ToList();
        var newLines = included.Sum(r => r.Texts.Count - 1);
        _summaryLabel.Text = _rows.Count == 0
            ? string.Empty
            : $"{included.Count} of {_rows.Count} dialog line(s) selected - {newLines} subtitle line(s) will be added.";
        _applyButton.IsEnabled = included.Count > 0;
    }

    private void OnSelectAll(object? sender, RoutedEventArgs e)
    {
        foreach (var row in _rows)
        {
            row.Include = true;
        }
    }

    private void OnSelectNone(object? sender, RoutedEventArgs e)
    {
        foreach (var row in _rows)
        {
            row.Include = false;
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
            App.Response = BatchMode.BuildConfigureResponse(CurrentSettings);
            Close();
            return;
        }

        try
        {
            var included = _rows.Where(r => r.Include).ToList();
            if (included.Count == 0)
            {
                App.Response = new PluginResponse { Status = PluginStatus.Cancelled };
                Close();
                return;
            }

            var splits = included.ToDictionary(r => r.Cue.Index, r => r.Parts);
            var lineCount = included.Sum(r => r.Parts.Count);
            App.Response = new PluginResponse
            {
                Status = PluginStatus.Ok,
                Message = $"Split {included.Count} dialog line(s) into {lineCount} lines.",
                UndoDescription = "Split dialogs",
                Subtitle = new PluginSubtitle
                {
                    Format = _document.FormatName,
                    Native = _document.Build(splits),
                },
                Settings = CurrentSettings.ToJson(),
                SettingsVersion = SplitSettings.Version,
            };
        }
        catch (Exception ex)
        {
            App.Response = new PluginResponse { Status = PluginStatus.Error, Message = ex.Message };
        }

        Close();
    }

    private SplitSettings CurrentSettings => new(_requireSentenceEndingCheckBox.IsChecked == true, GapMs);
}
