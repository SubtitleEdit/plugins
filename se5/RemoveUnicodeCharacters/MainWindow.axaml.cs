using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using SubtitleEdit.Plugins.Shared;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;

namespace SubtitleEdit.Plugins.RemoveUnicodeCharacters;

public partial class MainWindow : Window
{
    private readonly PluginRequest _request;
    private readonly bool _isConfigure;
    private readonly List<SrtBlock> _blocks;
    private readonly Dictionary<string, string> _persistedReplacements;
    private readonly ObservableCollection<UnicodeCharRow> _rows = new();

    private TextBlock _summaryLabel = null!;
    private TextBlock _subtitleLabel = null!;
    private TextBlock _noCharsLabel = null!;
    private Border _headerBorder = null!;
    private Border _listBorder = null!;
    private ListBox _charsList = null!;
    private Button _applyButton = null!;
    private Button _googleButton = null!;
    private Border _configurePanel = null!;
    private TextBox _addCharsBox = null!;
    private CheckBox _removeOthersCheckBox = null!;

    public MainWindow() : this(new PluginRequest()) { }

    public MainWindow(PluginRequest request)
    {
        _request = request;
        _isConfigure = request.RequestType == PluginRequestType.Configure;
        InitializeComponent();

        _blocks = SubRipParser.Parse(request.Subtitle.SubRip);
        _persistedReplacements = BatchMode.LoadReplacements(request.Settings);

        _charsList.ItemsSource = _rows;
        _charsList.SelectionChanged += (_, _) => UpdateGoogleButton();

        if (_isConfigure)
        {
            InitConfigure();
            return;
        }

        BuildRows();

        var scope = request.SelectedIndices.Count > 0
            ? $"the {request.SelectedIndices.Count} selected line(s)"
            : "all lines";
        _subtitleLabel.Text = $"Detected non-ANSI characters in {scope}. Edit the replacement for each character — leave blank to simply remove it.";

        UpdateUiForRows();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        _summaryLabel = this.FindControl<TextBlock>("SummaryLabel")!;
        _subtitleLabel = this.FindControl<TextBlock>("SubtitleLabel")!;
        _noCharsLabel = this.FindControl<TextBlock>("NoCharsLabel")!;
        _headerBorder = this.FindControl<Border>("HeaderBorder")!;
        _listBorder = this.FindControl<Border>("ListBorder")!;
        _charsList = this.FindControl<ListBox>("CharsList")!;
        _applyButton = this.FindControl<Button>("ApplyButton")!;
        _googleButton = this.FindControl<Button>("GoogleButton")!;
        _configurePanel = this.FindControl<Border>("ConfigurePanel")!;
        _addCharsBox = this.FindControl<TextBox>("AddCharsBox")!;
        _removeOthersCheckBox = this.FindControl<CheckBox>("RemoveOthersCheckBox")!;
    }

    /// <summary>
    /// Opened from Batch convert's Configure button: there is no subtitle, so the list shows the
    /// batch list - the only characters a batch run replaces (unless "remove all others" is on).
    /// </summary>
    private void InitConfigure()
    {
        Title = "Remove Unicode characters - Batch convert settings";
        _subtitleLabel.Text = "Settings for Batch convert";
        _configurePanel.IsVisible = true;
        _noCharsLabel.IsVisible = false;
        _headerBorder.IsVisible = true;
        _listBorder.IsVisible = true;
        this.FindControl<TextBlock>("CountHeader")!.IsVisible = false;
        this.FindControl<TextBlock>("LinesHeader")!.IsVisible = false;
        _applyButton.Content = "OK";
        _applyButton.IsEnabled = true;
        _removeOthersCheckBox.IsChecked = BatchMode.LoadRemoveOthers(_request.Settings);

        foreach (var (c, replacement) in BatchMode.LoadBatchList(_request.Settings))
        {
            AddRow(new UnicodeCharRow(c, 0, Array.Empty<int>(), replacement));
        }

        UpdateSummary();
        UpdateGoogleButton();
    }

    private void AddRow(UnicodeCharRow row)
    {
        row.PropertyChanged += OnRowChanged;
        _rows.Add(row);
    }

    private void OnAddCharacters(object? sender, RoutedEventArgs e)
    {
        foreach (var c in BatchMode.ParseCharacters(_addCharsBox.Text))
        {
            var existing = _rows.FirstOrDefault(r => r.Character == c);
            if (existing != null)
            {
                existing.Include = true;
                continue;
            }

            var hex = BatchMode.ToHex(c);
            var replacement = _persistedReplacements.TryGetValue(hex, out var persisted) && persisted.Length > 0
                ? persisted
                : BatchMode.DefaultReplacements.TryGetValue(c, out var builtIn) ? builtIn : string.Empty;
            var row = new UnicodeCharRow(c, 0, Array.Empty<int>(), replacement);
            var index = 0;
            while (index < _rows.Count && _rows[index].Character < c)
            {
                index++;
            }

            row.PropertyChanged += OnRowChanged;
            _rows.Insert(index, row);
        }

        _addCharsBox.Text = string.Empty;
        UpdateSummary();
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        this.BringToForeground();
    }

    private void BuildRows()
    {
        var selected = new HashSet<int>(_request.SelectedIndices);
        var applyToAll = selected.Count == 0;

        var counts = new Dictionary<char, int>();
        var lineSets = new Dictionary<char, SortedSet<int>>();

        for (var i = 0; i < _blocks.Count; i++)
        {
            if (!applyToAll && !selected.Contains(i))
            {
                continue;
            }

            foreach (var c in _blocks[i].Text)
            {
                if (c <= 255)
                {
                    continue;
                }

                counts[c] = counts.TryGetValue(c, out var existing) ? existing + 1 : 1;
                if (!lineSets.TryGetValue(c, out var lines))
                {
                    lines = new SortedSet<int>();
                    lineSets[c] = lines;
                }
                lines.Add(i);
            }
        }

        foreach (var c in counts.Keys.OrderBy(c => c))
        {
            AddRow(new UnicodeCharRow(c, counts[c], lineSets[c], ResolveDefaultReplacement(c)));
        }
    }

    private string ResolveDefaultReplacement(char c)
    {
        var hex = ((int)c).ToString("X4");
        if (_persistedReplacements.TryGetValue(hex, out var persisted))
        {
            return persisted;
        }
        return BatchMode.DefaultReplacements.TryGetValue(c, out var builtIn) ? builtIn : string.Empty;
    }

    private void UpdateUiForRows()
    {
        if (_rows.Count == 0)
        {
            _noCharsLabel.IsVisible = true;
            _headerBorder.IsVisible = false;
            _listBorder.IsVisible = false;
            _applyButton.IsEnabled = false;
            _summaryLabel.Text = string.Empty;
        }
        else
        {
            _noCharsLabel.IsVisible = false;
            _headerBorder.IsVisible = true;
            _listBorder.IsVisible = true;
            UpdateSummary();
        }

        UpdateGoogleButton();
    }

    private void OnRowChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(UnicodeCharRow.Include))
        {
            UpdateSummary();
        }
    }

    private void UpdateSummary()
    {
        if (_isConfigure)
        {
            _summaryLabel.Text = $"{_rows.Count(r => r.Include)} character(s) in the batch list.";
            return;
        }

        var totalRows = _rows.Count;
        var selectedRows = _rows.Count(r => r.Include);
        var selectedOccurrences = _rows.Where(r => r.Include).Sum(r => r.Count);
        _summaryLabel.Text = $"{selectedRows} of {totalRows} character(s) selected — {selectedOccurrences} occurrence(s) will be replaced.";
        _applyButton.IsEnabled = selectedRows > 0;
    }

    private void UpdateGoogleButton()
    {
        _googleButton.IsEnabled = _charsList.SelectedItem is UnicodeCharRow;
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

    private void OnGoogleSelected(object? sender, RoutedEventArgs e)
    {
        if (_charsList.SelectedItem is not UnicodeCharRow row)
        {
            return;
        }

        var url = "https://www.google.com/search?q=" + Uri.EscapeDataString(row.HexCode + " Unicode");
        TryOpenUrl(url);
    }

    private static void TryOpenUrl(string url)
    {
        try
        {
            ProcessStartInfo psi;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                psi = new ProcessStartInfo("cmd", "/c start \"\" \"" + url + "\"") { CreateNoWindow = true };
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                psi = new ProcessStartInfo("open", url);
            }
            else
            {
                psi = new ProcessStartInfo("xdg-open", url);
            }

            Process.Start(psi);
        }
        catch
        {
            // Best-effort - if the platform has no browser, silently ignore.
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
            App.Response = BuildConfigureResponse();
            Close();
            return;
        }

        try
        {
            var activeRows = _rows.Where(r => r.Include).ToList();
            if (activeRows.Count == 0)
            {
                App.Response = new PluginResponse { Status = PluginStatus.Cancelled };
                Close();
                return;
            }

            var lineIndices = new HashSet<int>();
            var occurrences = 0;
            foreach (var row in activeRows)
            {
                foreach (var lineIndex in row.LineIndices)
                {
                    lineIndices.Add(lineIndex);
                }
                occurrences += row.Count;
            }

            foreach (var lineIndex in lineIndices)
            {
                var sb = new StringBuilder(_blocks[lineIndex].Text.Length);
                foreach (var c in _blocks[lineIndex].Text)
                {
                    var matched = false;
                    foreach (var row in activeRows)
                    {
                        if (row.Character == c)
                        {
                            sb.Append(row.Replacement);
                            matched = true;
                            break;
                        }
                    }
                    if (!matched)
                    {
                        sb.Append(c);
                    }
                }
                _blocks[lineIndex].Text = sb.ToString();
            }

            App.Response = new PluginResponse
            {
                Status = PluginStatus.Ok,
                Message = $"Replaced {occurrences} Unicode character occurrence(s) across {lineIndices.Count} line(s).",
                UndoDescription = "Remove Unicode characters",
                Subtitle = new PluginSubtitle
                {
                    Format = "SubRip",
                    Native = SubRipParser.Serialize(_blocks),
                },
                Settings = BuildSettings(),
            };
        }
        catch (Exception ex)
        {
            App.Response = new PluginResponse { Status = PluginStatus.Error, Message = ex.Message };
        }

        Close();
    }

    /// <summary>A "configure" request: hand back the settings only (batch list + option), no subtitle.</summary>
    private PluginResponse BuildConfigureResponse()
    {
        var batchList = new SortedDictionary<char, string>();
        foreach (var row in _rows.Where(r => r.Include))
        {
            batchList[row.Character] = row.Replacement ?? string.Empty;
        }

        return new PluginResponse
        {
            Status = PluginStatus.Ok,
            Settings = BatchMode.BuildSettings(_persistedReplacements, _request.Settings, batchList, _removeOthersCheckBox.IsChecked == true),
            SettingsVersion = BatchMode.SettingsVersion,
        };
    }

    private JsonElement BuildSettings()
    {
        var merged = new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var kvp in _persistedReplacements)
        {
            merged[kvp.Key] = kvp.Value;
        }

        foreach (var row in _rows)
        {
            var hex = ((int)row.Character).ToString("X4");
            merged[hex] = row.Replacement ?? string.Empty;
        }

        return BatchMode.BuildSettings(merged, _request.Settings);
    }
}
