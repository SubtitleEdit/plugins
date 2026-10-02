using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using SubtitleEdit.Plugins.Shared;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace SubtitleEdit.Plugins.AmericanToBritish;

public sealed record WordPair(string Us, string Br);

/// <summary>
/// Edits the local word list: extra/override pairs and ignored built-in words.
/// Closes with true when the list was saved.
/// </summary>
public partial class WordListWindow : Window
{
    private readonly string _path;
    private readonly ObservableCollection<WordPair> _words = new();
    private readonly ObservableCollection<string> _ignored = new();

    private TextBlock _errorLabel = null!;
    private ListBox _wordsList = null!;
    private ListBox _ignoredList = null!;
    private TextBox _americanBox = null!;
    private TextBox _britishBox = null!;
    private TextBox _ignoreBox = null!;
    private Button _removeWordButton = null!;
    private Button _removeIgnoredButton = null!;

    public WordListWindow() : this(LocalWordList.GetDefaultPath("AmericanToBritish.xml"), new LocalWordList()) { }

    public WordListWindow(string path, LocalWordList list, string? loadError = null)
    {
        _path = path;
        InitializeComponent();

        if (loadError != null)
        {
            ShowError(loadError);
        }

        foreach (var (us, br) in list.Words)
        {
            _words.Add(new WordPair(us, br));
        }

        foreach (var us in list.Ignored)
        {
            _ignored.Add(us);
        }

        _wordsList.ItemsSource = _words;
        _ignoredList.ItemsSource = _ignored;
        this.FindControl<SelectableTextBlock>("FileLabel")!.Text = path;

        _wordsList.SelectionChanged += (_, _) => _removeWordButton.IsEnabled = _wordsList.SelectedItems?.Count > 0;
        _ignoredList.SelectionChanged += (_, _) => _removeIgnoredButton.IsEnabled = _ignoredList.SelectedItems?.Count > 0;
        _wordsList.SelectionChanged += (_, _) =>
        {
            if (_wordsList.SelectedItem is WordPair pair)
            {
                _americanBox.Text = pair.Us;
                _britishBox.Text = pair.Br;
            }
        };

        _americanBox.KeyDown += OnWordBoxKeyDown;
        _britishBox.KeyDown += OnWordBoxKeyDown;
        _ignoreBox.KeyDown += (_, e) =>
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                OnAddIgnored(null, new RoutedEventArgs());
            }
        };
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        _errorLabel = this.FindControl<TextBlock>("ErrorLabel")!;
        _wordsList = this.FindControl<ListBox>("WordsList")!;
        _ignoredList = this.FindControl<ListBox>("IgnoredList")!;
        _americanBox = this.FindControl<TextBox>("AmericanBox")!;
        _britishBox = this.FindControl<TextBox>("BritishBox")!;
        _ignoreBox = this.FindControl<TextBox>("IgnoreBox")!;
        _removeWordButton = this.FindControl<Button>("RemoveWordButton")!;
        _removeIgnoredButton = this.FindControl<Button>("RemoveIgnoredButton")!;
    }

    private void OnWordBoxKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            e.Handled = true;
            OnAddWord(null, new RoutedEventArgs());
        }
    }

    private void OnAddWord(object? sender, RoutedEventArgs e)
    {
        var us = _americanBox.Text?.Trim() ?? string.Empty;
        var br = _britishBox.Text?.Trim() ?? string.Empty;
        if (us.Length == 0 || br.Length == 0)
        {
            ShowError("Enter both the American and the British word.");
            return;
        }

        if (string.Equals(us, br, StringComparison.Ordinal))
        {
            ShowError("The American and British words are the same - use \"Ignored words\" to stop a conversion.");
            return;
        }

        HideError();

        // Same American spelling replaces the existing entry (edit in place).
        var existing = _words.FirstOrDefault(w => string.Equals(w.Us, us, StringComparison.OrdinalIgnoreCase));
        var pair = new WordPair(us, br);
        if (existing != null)
        {
            _words[_words.IndexOf(existing)] = pair;
        }
        else
        {
            _words.Add(pair);
        }

        RemoveIgnored(us);
        _wordsList.SelectedItem = pair;
        _wordsList.ScrollIntoView(pair);
        _americanBox.Text = string.Empty;
        _britishBox.Text = string.Empty;
        _americanBox.Focus();
    }

    private void OnRemoveWord(object? sender, RoutedEventArgs e)
    {
        foreach (var pair in _wordsList.SelectedItems?.OfType<WordPair>().ToList() ?? new())
        {
            _words.Remove(pair);
        }
    }

    private void OnAddIgnored(object? sender, RoutedEventArgs e)
    {
        var us = _ignoreBox.Text?.Trim() ?? string.Empty;
        if (us.Length == 0)
        {
            return;
        }

        HideError();
        if (!_ignored.Any(w => string.Equals(w, us, StringComparison.OrdinalIgnoreCase)))
        {
            _ignored.Add(us);
        }

        // A custom pair for the same word would still convert it.
        foreach (var pair in _words.Where(w => string.Equals(w.Us, us, StringComparison.OrdinalIgnoreCase)).ToList())
        {
            _words.Remove(pair);
        }

        _ignoreBox.Text = string.Empty;
        _ignoreBox.Focus();
    }

    private void OnRemoveIgnored(object? sender, RoutedEventArgs e)
    {
        foreach (var us in _ignoredList.SelectedItems?.OfType<string>().ToList() ?? new())
        {
            _ignored.Remove(us);
        }
    }

    private void RemoveIgnored(string us)
    {
        foreach (var word in _ignored.Where(w => string.Equals(w, us, StringComparison.OrdinalIgnoreCase)).ToList())
        {
            _ignored.Remove(word);
        }
    }

    private void ShowError(string message)
    {
        _errorLabel.Text = message;
        _errorLabel.IsVisible = true;
    }

    private void HideError() => _errorLabel.IsVisible = false;

    private void OnCancel(object? sender, RoutedEventArgs e) => Close(false);

    private void OnOk(object? sender, RoutedEventArgs e)
    {
        var list = new LocalWordList();
        list.Words.AddRange(_words.Select(w => (w.Us, w.Br)));
        list.Ignored.AddRange(_ignored);

        try
        {
            list.Save(_path);
        }
        catch (Exception ex)
        {
            ShowError("Could not save the word list: " + ex.Message);
            return;
        }

        Close(true);
    }
}
