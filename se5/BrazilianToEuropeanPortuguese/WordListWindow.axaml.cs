using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;

public sealed record WordPair(string Brazilian, string European);

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
    private TextBox _brazilianBox = null!;
    private TextBox _portugueseBox = null!;
    private TextBox _ignoreBox = null!;
    private Button _removeWordButton = null!;
    private Button _removeIgnoredButton = null!;

    public WordListWindow() : this(PortugueseLocalWordList.GetDefaultPath(), new PortugueseLocalWordList()) { }

    public WordListWindow(string path, PortugueseLocalWordList list, string? loadError = null)
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
                _brazilianBox.Text = pair.Brazilian;
                _portugueseBox.Text = pair.European;
            }
        };

        _brazilianBox.KeyDown += OnWordBoxKeyDown;
        _portugueseBox.KeyDown += OnWordBoxKeyDown;
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
        _brazilianBox = this.FindControl<TextBox>("BrazilianBox")!;
        _portugueseBox = this.FindControl<TextBox>("PortugueseBox")!;
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
        var us = _brazilianBox.Text?.Trim() ?? string.Empty;
        var br = _portugueseBox.Text?.Trim() ?? string.Empty;
        if (us.Length == 0 || br.Length == 0)
        {
            ShowError("Enter both the Brazilian and the European Portuguese word.");
            return;
        }

        if (string.Equals(us, br, StringComparison.Ordinal))
        {
            ShowError("The Brazilian and European words are the same - use \"Ignored words\" to stop a conversion.");
            return;
        }

        HideError();

        // Same Brazilian word replaces the existing entry (edit in place).
        var existing = _words.FirstOrDefault(w => string.Equals(w.Brazilian, us, StringComparison.OrdinalIgnoreCase));
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
        _brazilianBox.Text = string.Empty;
        _portugueseBox.Text = string.Empty;
        _brazilianBox.Focus();
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
        foreach (var pair in _words.Where(w => string.Equals(w.Brazilian, us, StringComparison.OrdinalIgnoreCase)).ToList())
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
        var list = new PortugueseLocalWordList();
        list.Words.AddRange(_words.Select(w => (w.Brazilian, w.European)));
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
