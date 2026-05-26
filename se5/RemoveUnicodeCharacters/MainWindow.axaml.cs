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
    /// <summary>Built-in default replacements (used when the user has no persisted setting for a character).</summary>
    private static readonly Dictionary<char, string> DefaultReplacements = new()
    {
        ['♪'] = "#",
        ['♫'] = "#",
    };

    private readonly PluginRequest _request;
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

    public MainWindow() : this(new PluginRequest()) { }

    public MainWindow(PluginRequest request)
    {
        _request = request;
        InitializeComponent();

        _blocks = SubRipParser.Parse(request.Subtitle.SubRip);
        _persistedReplacements = LoadReplacementsFromSettings(request.Settings);

        BuildRows();
        _charsList.ItemsSource = _rows;
        _charsList.SelectionChanged += (_, _) => UpdateGoogleButton();

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
            var row = new UnicodeCharRow(c, counts[c], lineSets[c], ResolveDefaultReplacement(c));
            row.PropertyChanged += OnRowChanged;
            _rows.Add(row);
        }
    }

    private string ResolveDefaultReplacement(char c)
    {
        var hex = ((int)c).ToString("X4");
        if (_persistedReplacements.TryGetValue(hex, out var persisted))
        {
            return persisted;
        }
        return DefaultReplacements.TryGetValue(c, out var builtIn) ? builtIn : string.Empty;
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

        var payload = new { replacements = merged };
        var json = JsonSerializer.Serialize(payload);
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.Clone();
    }

    private static Dictionary<string, string> LoadReplacementsFromSettings(JsonElement? settings)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (settings is null || settings.Value.ValueKind != JsonValueKind.Object)
        {
            return result;
        }

        if (!settings.Value.TryGetProperty("replacements", out var replacements)
            || replacements.ValueKind != JsonValueKind.Object)
        {
            return result;
        }

        foreach (var property in replacements.EnumerateObject())
        {
            if (property.Value.ValueKind == JsonValueKind.String)
            {
                result[property.Name] = property.Value.GetString() ?? string.Empty;
            }
        }

        return result;
    }
}
