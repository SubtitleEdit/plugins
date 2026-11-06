using System.Collections.ObjectModel;
using System.Text;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;

namespace SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;

public partial class ContributionsWindow
{
    private readonly ObservableCollection<WordPair> _words = new();
    private readonly ObservableCollection<string> _ignored = new();
    private string _localPath = "";
    private ListBox _wordsList = null!;
    private ListBox _ignoredList = null!;
    private TextBox _brazilianBox = null!;
    private TextBox _portugueseBox = null!;
    private TextBox _ignoreBox = null!;
    private WordPair? _wordBaseline;
    private string _ignoreBaseline = "";
    private bool _localDirty;
    private bool _selectingLocal;
    private bool _localLoadFailed;

    internal bool LocalWordListSaved { get; private set; }
    private bool WordDraft => (_brazilianBox.Text ?? "") != (_wordBaseline?.Brazilian ?? "") ||
        (_portugueseBox.Text ?? "") != (_wordBaseline?.European ?? "");
    private bool IgnoreDraft => (_ignoreBox.Text ?? "") != _ignoreBaseline;
    private bool UnsavedLocal => _localDirty || WordDraft || IgnoreDraft;

    private void InitializeLocalWords(string path)
    {
        _localPath = path;
        _wordsList = this.FindControl<ListBox>("WordsList")!;
        _ignoredList = this.FindControl<ListBox>("IgnoredList")!;
        _brazilianBox = this.FindControl<TextBox>("BrazilianBox")!;
        _portugueseBox = this.FindControl<TextBox>("PortugueseBox")!;
        _ignoreBox = this.FindControl<TextBox>("IgnoreBox")!;
        _wordsList.ItemsSource = _words;
        _ignoredList.ItemsSource = _ignored;
        try
        {
            var list = PortugueseLocalWordList.Load(path);
            foreach (var (br, pt) in list.Words) _words.Add(new WordPair(br, pt));
            foreach (var br in list.Ignored) _ignored.Add(br);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Xml.XmlException)
        {
            _localLoadFailed = true;
            LocalError($"Could not read the existing local word list: {ex.Message}. Saving is disabled to protect it.");
            this.FindControl<Button>("SaveLocalWordsButton")!.IsEnabled = false;
            this.FindControl<Button>("SaveLocalIgnoredButton")!.IsEnabled = false;
        }
        foreach (var field in new[] { _brazilianBox, _portugueseBox, _ignoreBox })
            field.PropertyChanged += (_, e) => { if (e.Property == TextBox.TextProperty) LocalStatus(); };
        _wordsList.SelectionChanged += (_, _) =>
        {
            if (_selectingLocal || _wordsList.SelectedItem is not WordPair pair) return;
            if (WordDraft)
            {
                _selectingLocal = true;
                _wordsList.SelectedItem = _wordBaseline;
                _selectingLocal = false;
                LocalError("Add/update or clear the custom-word draft before selecting another entry.");
                return;
            }
            _wordBaseline = pair;
            _brazilianBox.Text = pair.Brazilian;
            _portugueseBox.Text = pair.European;
        };
        _ignoredList.SelectionChanged += (_, _) =>
        {
            if (_selectingLocal || _ignoredList.SelectedItem is not string word) return;
            if (IgnoreDraft)
            {
                _selectingLocal = true;
                _ignoredList.SelectedItem = _ignoreBaseline.Length == 0 ? null : _ignoreBaseline;
                _selectingLocal = false;
                LocalError("Add/update or clear the ignored-word draft before selecting another entry.");
                return;
            }
            _ignoreBaseline = word;
            _ignoreBox.Text = word;
        };
        LocalStatus();
    }

    private void LocalError(string message)
    {
        this.FindControl<TextBlock>("LocalErrorWords")!.Text = message;
        this.FindControl<TextBlock>("LocalErrorIgnored")!.Text = message;
    }

    private void LocalStatus()
    {
        var status = $"{_words.Count} custom entries; {_ignored.Count} ignored entries." +
            (UnsavedLocal ? " Unsaved local changes." : "") + $" File: {_localPath}";
        this.FindControl<TextBlock>("LocalStatusWords")!.Text = status;
        this.FindControl<TextBlock>("LocalStatusIgnored")!.Text = status;
    }

    private void ResetWordForm()
    {
        _wordsList.SelectedIndex = -1;
        _wordBaseline = null;
        _brazilianBox.Text = "";
        _portugueseBox.Text = "";
    }

    private void ResetIgnoredForm()
    {
        _ignoredList.SelectedIndex = -1;
        _ignoreBaseline = "";
        _ignoreBox.Text = "";
    }

    private void MergeWord(WordPair pair)
    {
        foreach (var item in _words.Where(w => string.Equals(w.Brazilian, pair.Brazilian,
                     StringComparison.OrdinalIgnoreCase)).ToList()) _words.Remove(item);
        foreach (var item in _ignored.Where(w => string.Equals(w, pair.Brazilian,
                     StringComparison.OrdinalIgnoreCase)).ToList()) _ignored.Remove(item);
        _words.Add(pair);
    }

    private void MergeIgnored(string word)
    {
        foreach (var pair in _words.Where(w => string.Equals(w.Brazilian, word,
                     StringComparison.OrdinalIgnoreCase)).ToList()) _words.Remove(pair);
        if (!_ignored.Any(w => string.Equals(w, word, StringComparison.OrdinalIgnoreCase))) _ignored.Add(word);
    }

    private bool StoreWord(bool update)
    {
        try
        {
            var pair = new WordPair(_brazilianBox.Text?.Trim() ?? "", _portugueseBox.Text?.Trim() ?? "");
            LocalWordListCsv.ValidateWord(pair);
            if (update && _wordBaseline == null) throw new FormatException("Select a custom entry to update.");
            _selectingLocal = true;
            if (update) _words.Remove(_wordBaseline!);
            MergeWord(pair);
            ResetWordForm();
            if (!IgnoreDraft) ResetIgnoredForm();
            _localDirty = true;
            LocalError("");
            LocalStatus();
            return true;
        }
        catch (FormatException ex) { LocalError(ex.Message); return false; }
        finally { _selectingLocal = false; }
    }

    private bool StoreIgnored(bool update)
    {
        var word = _ignoreBox.Text?.Trim() ?? "";
        if (word.Length == 0) { LocalError("Enter a word or expression to ignore."); return false; }
        if (update && _ignoreBaseline.Length == 0) { LocalError("Select an ignored entry to update."); return false; }
        _selectingLocal = true;
        if (update) _ignored.Remove(_ignoreBaseline);
        MergeIgnored(word);
        if (!WordDraft) ResetWordForm();
        ResetIgnoredForm();
        _selectingLocal = false;
        _localDirty = true;
        LocalError("");
        LocalStatus();
        return true;
    }

    private void OnAddWord(object? sender, RoutedEventArgs e) => StoreWord(false);
    private void OnUpdateWord(object? sender, RoutedEventArgs e) => StoreWord(true);
    private void OnAddIgnored(object? sender, RoutedEventArgs e) => StoreIgnored(false);
    private void OnUpdateIgnored(object? sender, RoutedEventArgs e) => StoreIgnored(true);

    private async void OnClearWord(object? sender, RoutedEventArgs e)
    {
        if (WordDraft && !await ConfirmDiscard("Discard the custom-word draft?")) return;
        ResetWordForm();
    }

    private async void OnClearIgnored(object? sender, RoutedEventArgs e)
    {
        if (IgnoreDraft && !await ConfirmDiscard("Discard the ignored-word draft?")) return;
        ResetIgnoredForm();
    }

    private void OnRemoveWord(object? sender, RoutedEventArgs e)
    {
        if (WordDraft) { LocalError("Add/update or clear the custom-word draft before removing an entry."); return; }
        if (_wordsList.SelectedItem is not WordPair pair) { LocalError("Select a custom entry to remove."); return; }
        _words.Remove(pair);
        ResetWordForm();
        _localDirty = true;
        LocalError("");
        LocalStatus();
    }

    private void OnRemoveIgnored(object? sender, RoutedEventArgs e)
    {
        if (IgnoreDraft) { LocalError("Add/update or clear the ignored-word draft before removing an entry."); return; }
        if (_ignoredList.SelectedItem is not string word) { LocalError("Select an ignored entry to remove."); return; }
        _ignored.Remove(word);
        ResetIgnoredForm();
        _localDirty = true;
        LocalError("");
        LocalStatus();
    }

    internal void ImportLocalCsv(string csv, bool ignored)
    {
        if (WordDraft || IgnoreDraft) throw new FormatException("Add/update or clear local drafts before importing CSV.");
        var ignoreEntries = ignored ? LocalWordListCsv.ParseIgnored(csv) : Array.Empty<string>();
        var wordEntries = ignored ? Array.Empty<WordPair>() : LocalWordListCsv.ParseWords(csv);
        _selectingLocal = true;
        foreach (var word in ignoreEntries) MergeIgnored(word);
        foreach (var pair in wordEntries) MergeWord(pair);
        ResetWordForm();
        ResetIgnoredForm();
        _selectingLocal = false;
        _localDirty = true;
        LocalError("");
        LocalStatus();
    }

    internal string ExportLocalCsv(bool ignored)
    {
        if (WordDraft || IgnoreDraft) throw new FormatException("Add/update or clear local drafts before exporting CSV.");
        return ignored ? LocalWordListCsv.SerializeIgnored(_ignored) : LocalWordListCsv.SerializeWords(_words);
    }

    internal void SaveLocalWords()
    {
        if (_localLoadFailed) throw new IOException("The existing word list could not be read. Repair it before saving.");
        if (WordDraft || IgnoreDraft) throw new FormatException("Add/update or clear local drafts before saving.");
        var list = new PortugueseLocalWordList();
        list.Words.AddRange(_words.Select(w => (w.Brazilian, w.European)));
        list.Ignored.AddRange(_ignored);
        var temporary = _localPath + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            list.Save(temporary);
            File.Move(temporary, _localPath, overwrite: true);
        }
        finally { if (File.Exists(temporary)) File.Delete(temporary); }
        _localDirty = false;
        LocalWordListSaved = true;
        LocalError("");
        LocalStatus();
    }

    private void OnSaveLocalWords(object? sender, RoutedEventArgs e)
    {
        try { SaveLocalWords(); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or FormatException)
        { LocalError($"Could not save local word lists: {ex.Message}"); }
    }

    private async void OnImportWords(object? sender, RoutedEventArgs e) => await TransferLocalCsv(false, true);
    private async void OnImportIgnored(object? sender, RoutedEventArgs e) => await TransferLocalCsv(true, true);
    private async void OnExportWords(object? sender, RoutedEventArgs e) => await TransferLocalCsv(false, false);
    private async void OnExportIgnored(object? sender, RoutedEventArgs e) => await TransferLocalCsv(true, false);

    private async Task TransferLocalCsv(bool ignored, bool import)
    {
        if (_busy) return;
        SetBusy(true);
        try
        {
            if (import)
            {
                var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                {
                    Title = "Import local word list CSV", AllowMultiple = false, FileTypeFilter = new[] { CsvType },
                });
                if (files.Count == 0) return;
                await using var stream = await files[0].OpenReadAsync();
                using var reader = new StreamReader(stream, new UTF8Encoding(false, true), true);
                ImportLocalCsv(await reader.ReadToEndAsync(), ignored);
            }
            else
            {
                var csv = ExportLocalCsv(ignored);
                var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
                {
                    Title = "Export local word list CSV", DefaultExtension = "csv",
                    SuggestedFileName = ignored ? "ignored-words.csv" : "custom-words.csv",
                    FileTypeChoices = new[] { CsvType }, ShowOverwritePrompt = true,
                });
                if (file == null) return;
                await using var stream = await file.OpenWriteAsync();
                stream.SetLength(0);
                await using var writer = new StreamWriter(stream, new UTF8Encoding(true));
                await writer.WriteAsync(csv);
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or FormatException or
            DecoderFallbackException or NotSupportedException)
        { LocalError($"Could not {(import ? "import" : "export")} CSV: {ex.Message}"); }
        finally { SetBusy(false); }
    }
}
