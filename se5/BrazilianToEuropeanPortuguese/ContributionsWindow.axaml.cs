using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;

namespace SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;

public partial class ContributionsWindow : Window
{
    private readonly ObservableCollection<VocabularyContribution> _entries = new();
    private readonly ListBox _list;
    private readonly TextBox[] _fields;
    private readonly TextBlock _status;
    private readonly TextBlock _error;
    private bool _loading;
    private bool _draft;
    private bool _dirty;
    private bool _busy;
    private bool _allowClose;
    private bool _confirmingClose;
    private string? _fileName;
    private int _editingIndex = -1;

    private void SetBusy(bool busy)
    {
        _busy = busy;
        if (Content is Control content) content.IsEnabled = !busy;
    }

    public ContributionsWindow() : this(PortugueseLocalWordList.GetDefaultPath()) { }

    internal ContributionsWindow(string localPath)
    {
        AvaloniaXamlLoader.Load(this);
        _list = this.FindControl<ListBox>("ContributionsList")!;
        _fields = new[] { "ContributionBrazilian", "ContributionBrazilianExample", "ContributionEuropean",
            "ContributionEuropeanExample", "ContributionDescription" }.Select(n => this.FindControl<TextBox>(n)!).ToArray();
        _status = this.FindControl<TextBlock>("ContributionStatus")!;
        _error = this.FindControl<TextBlock>("ContributionError")!;
        _list.ItemsSource = _entries;
        foreach (var field in _fields) field.PropertyChanged += (_, e) =>
        {
            if (!_loading && e.Property == TextBox.TextProperty) { _draft = true; ShowStatus(); }
        };
        InitializeLocalWords(localPath);
        Closing += OnClosing;
        ShowStatus();
    }

    private void ShowStatus() =>
        _status.Text = $"{_entries.Count} entry/entries." + (_dirty || _draft ? " Unsaved changes." : "") +
            (_fileName == null ? "" : $" File: {_fileName}");

    private VocabularyContribution ReadEntry()
    {
        var entry = new VocabularyContribution(_fields[0].Text?.Trim() ?? "", _fields[1].Text ?? "",
            _fields[2].Text?.Trim() ?? "", _fields[3].Text ?? "", _fields[4].Text ?? "");
        VocabularyContributions.Validate(entry);
        return entry;
    }

    private void Fill(VocabularyContribution? entry)
    {
        _loading = true;
        _editingIndex = entry == null ? -1 : _list.SelectedIndex;
        var values = entry == null ? new[] { "", "", "", "", "" } :
            new[] { entry.Brazilian, entry.BrazilianExample, entry.European, entry.EuropeanExample, entry.Description };
        for (var i = 0; i < _fields.Length; i++) _fields[i].Text = values[i];
        _loading = false;
        _draft = false;
        ShowStatus();
    }

    private bool StoreDraft(bool update)
    {
        try
        {
            var entry = ReadEntry();
            var index = _list.SelectedIndex;
            if (update && index < 0) throw new FormatException("Select an entry to update, or use Add entry.");
            if (update) _entries[index] = entry;
            else _entries.Add(entry);
            _dirty = true;
            _error.Text = "";
            _list.SelectedIndex = -1;
            Fill(null);
            return true;
        }
        catch (FormatException ex) { _error.Text = ex.Message; return false; }
    }

    private void OnAdd(object? sender, RoutedEventArgs e) => StoreDraft(false);
    private void OnUpdate(object? sender, RoutedEventArgs e) => StoreDraft(true);

    private async void OnNew(object? sender, RoutedEventArgs e)
    {
        if (_draft && !await ConfirmDiscard("Discard the unsaved entry in the form?")) return;
        _list.SelectedIndex = -1;
        Fill(null);
        _error.Text = "";
    }

    private void OnRemove(object? sender, RoutedEventArgs e)
    {
        if (_list.SelectedIndex < 0) { _error.Text = "Select an entry to remove."; return; }
        if (_draft) { _error.Text = "Add/update the entry or choose New entry before removing a row."; return; }
        var index = _list.SelectedIndex;
        _list.SelectedIndex = -1;
        _entries.RemoveAt(index);
        _dirty = true;
        Fill(null);
        _error.Text = "";
    }

    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_loading || _list == null || _fields == null || _list.SelectedItem is not VocabularyContribution entry) return;
        if (_draft)
        {
            _loading = true;
            _list.SelectedIndex = _editingIndex;
            _loading = false;
            _error.Text = "Add/update your unsaved entry or choose New entry before selecting another row.";
            return;
        }
        Fill(entry);
    }

    internal void LoadCsv(string csv)
    {
        var entries = VocabularyContributions.Parse(csv);
        _list.SelectedIndex = -1;
        _entries.Clear();
        foreach (var entry in entries) _entries.Add(entry);
        _dirty = false;
        Fill(null);
        _error.Text = "";
    }

    internal string ExportCsv()
    {
        if (_draft && !StoreDraft(_list.SelectedIndex >= 0))
            throw new FormatException("The unsaved form entry is invalid. PT-BR and PT-PT are required.");
        return VocabularyContributions.Serialize(_entries);
    }

    private static FilePickerFileType CsvType => new("CSV contributions") { Patterns = new[] { "*.csv" } };

    private async void OnOpen(object? sender, RoutedEventArgs e)
    {
        if (_busy) return;
        SetBusy(true);
        try
        {
            if ((_dirty || _draft) && !await ConfirmDiscard("Discard unsaved contributions and open another CSV?")) return;
            var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Open vocabulary contributions", AllowMultiple = false, FileTypeFilter = new[] { CsvType },
            });
            if (files.Count == 0) return;
            await using var stream = await files[0].OpenReadAsync();
            using var reader = new StreamReader(stream, new UTF8Encoding(false, true), true);
            var csv = await reader.ReadToEndAsync();
            LoadCsv(csv);
            _fileName = files[0].Name;
            ShowStatus();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or FormatException or
            DecoderFallbackException or NotSupportedException)
        { _error.Text = $"Could not open CSV: {ex.Message}"; }
        finally { SetBusy(false); }
    }

    private async void OnSave(object? sender, RoutedEventArgs e)
    {
        if (_busy) return;
        SetBusy(true);
        try
        {
            var csv = ExportCsv();
            var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Save vocabulary contributions", SuggestedFileName = _fileName ?? "ptbr-ptpt-contributions.csv",
                DefaultExtension = "csv", FileTypeChoices = new[] { CsvType }, ShowOverwritePrompt = true,
            });
            if (file == null) return;
            await using (var stream = await file.OpenWriteAsync())
            {
                stream.SetLength(0);
                await using var writer = new StreamWriter(stream, new UTF8Encoding(true));
                await writer.WriteAsync(csv);
            }
            _dirty = false;
            _fileName = file.Name;
            _error.Text = "";
            ShowStatus();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or FormatException or NotSupportedException)
        { _error.Text = $"Could not save CSV: {ex.Message}"; }
        finally { SetBusy(false); }
    }

    private async Task<bool> ConfirmDiscard(string message)
    {
        var dialog = new Window
        {
            Title = "Unsaved contributions", Width = 430, SizeToContent = SizeToContent.Height,
            CanResize = false, WindowStartupLocation = WindowStartupLocation.CenterOwner,
        };
        var keep = new Button { Content = "Keep editing", IsCancel = true };
        var discard = new Button { Content = "Discard" };
        keep.Click += (_, _) => dialog.Close(false);
        discard.Click += (_, _) => dialog.Close(true);
        dialog.Content = new StackPanel
        {
            Margin = new Avalonia.Thickness(16), Spacing = 12,
            Children =
            {
                new TextBlock { Text = message, TextWrapping = Avalonia.Media.TextWrapping.Wrap },
                new StackPanel { Orientation = Avalonia.Layout.Orientation.Horizontal, Spacing = 8, Children = { keep, discard } },
            },
        };
        return await dialog.ShowDialog<bool>(this);
    }

    private void OnClose(object? sender, RoutedEventArgs e) => Close();

    private async void OnClosing(object? sender, WindowClosingEventArgs e)
    {
        if (_allowClose) return;
        if (_busy) { e.Cancel = true; _error.Text = "Finish the open/save operation before closing."; return; }
        if (!_dirty && !_draft && !UnsavedLocal) return;
        e.Cancel = true;
        if (_confirmingClose) return;
        _confirmingClose = true;
        try
        {
            if (await ConfirmDiscard("Discard unsaved contributions and local word-list changes and close?"))
            { _allowClose = true; Close(); }
        }
        finally { _confirmingClose = false; }
    }
}
