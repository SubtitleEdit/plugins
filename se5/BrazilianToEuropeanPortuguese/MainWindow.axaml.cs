using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using SubtitleEdit.Plugins.Shared;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;

namespace SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;

public partial class MainWindow : Window
{
    private readonly PluginRequest _request;
    private readonly List<SrtBlock> _blocks;
    private readonly ObservableCollection<ChangeProposal> _proposals = new();
    private readonly string _wordListPath;
    private readonly bool _isConfigure;
    private bool _loadingOptions = true;
    private bool _dictionaryBusy;
    private ComboBox _orthographyChoice = null!;
    private CheckBox _validateSpellingCheck = null!;
    private TextBlock _dictionaryStatus = null!;

    private TextBlock _summaryLabel = null!;
    private TextBlock _subtitleLabel = null!;
    private TextBlock _noChangesLabel = null!;
    private ItemsControl _changesList = null!;
    private Button _applyButton = null!;
    private Control _selectButtons = null!;
    private Control _configurePanel = null!;
    private Control _changesBorder = null!;
    private CheckBox _spellingCheck = null!;
    private CheckBox _vocabularyCheck = null!;
    private CheckBox _progressiveCheck = null!;
    private CheckBox _pronounsCheck = null!;
    private CheckBox _colloquialCheck = null!;
    private CheckBox _youToTuCheck = null!;
    private Expander _referencePanel = null!;
    private TextBox _referencePath = null!;
    private TextBlock _referenceStatus = null!;
    private TextBlock _assetStatus = null!;
    private CheckBox _embeddingCheck = null!;
    private CheckBox _vulkanCheck = null!;
    private ComboBox _deviceChoice = null!;
    private Button _referenceCancelButton = null!;
    private Control[] _referenceButtons = Array.Empty<Control>();
    private EmbeddingSettings _embeddingSettings = new();
    private Control _referencePathRow = null!;
    private CheckBox _batchReferenceCheck = null!;
    private bool _loadingEmbedding = true;
    private List<SrtBlock>? _referenceBlocks;
    private ReferenceAlignment? _alignment;
    private CancellationTokenSource? _referenceCancel;
    private CheckBox _meaningCheck = null!;
    private CheckBox _judgeCheck = null!;
    private readonly List<CheckLine> _checkLines = new();
    private readonly Dictionary<CheckLine, CheckResult> _checks = new();
    private ComboBox _addressChoice = null!;
    private CheckBox _sentenceContextCheck = null!;
    private CheckBox _rewrapCheck = null!;
    private CheckBox _conversationalCheck = null!;
    private CheckBox _layoutWarningsCheck = null!;
    private NumericUpDown _lineLength = null!;
    private NumericUpDown _maxLines = null!;
    private NumericUpDown _charactersPerSecond = null!;
    private ComboBox _disableRuleChoice = null!;
    private readonly HashSet<string> _disabledForFile = new(StringComparer.Ordinal);
    private string _savedDisabledRules = "";
    private bool _syncingSentence;

    public MainWindow() : this(new PluginRequest()) { }

    public MainWindow(PluginRequest request) : this(request, PortugueseLocalWordList.GetDefaultPath()) { }

    internal MainWindow(PluginRequest request, string wordListPath)
    {
        _wordListPath = wordListPath;
        _request = request;
        _isConfigure = request.RequestType == PluginRequestType.Configure;
        InitializeComponent();
        LoadOptions(ConversionOptions.Load(request));

        _blocks = SubRipParser.Parse(request.Subtitle.SubRip);
        _changesList.ItemsSource = _proposals;
        LoadEmbeddingSettings();
        if (_isConfigure)
        {
            ShowConfigure();
            return;
        }

        RefreshProposals();
    }

    private ConversionOptions Options => new(
        _spellingCheck.IsChecked == true,
        _vocabularyCheck.IsChecked == true,
        _progressiveCheck.IsChecked == true,
        _pronounsCheck.IsChecked == true,
        _colloquialCheck.IsChecked == true,
        _youToTuCheck.IsChecked == true,
        _orthographyChoice.SelectedIndex == 0,
        _validateSpellingCheck.IsChecked == true,
        _batchReferenceCheck.IsChecked != false,
        (AddressMode)Math.Max(0, _addressChoice.SelectedIndex),
        _sentenceContextCheck.IsChecked == true,
        _rewrapCheck.IsChecked == true,
        (int)(_lineLength.Value ?? 43),
        (int)(_maxLines.Value ?? 2),
        (int)(_charactersPerSecond.Value ?? 20),
        string.Join(",", new[] { _savedDisabledRules }.Concat(_disabledForFile).Where(s => s.Length > 0)),
        _conversationalCheck.IsChecked == true,
        _layoutWarningsCheck.IsChecked == true);

    private ConversionOptions SavedOptions => Options with { DisabledRules = _savedDisabledRules };

    private IEnumerable<CheckBox> OptionChecks =>
        new[] { _spellingCheck, _vocabularyCheck, _progressiveCheck, _pronounsCheck, _colloquialCheck, _youToTuCheck };

    private void LoadOptions(ConversionOptions options)
    {
        _loadingOptions = true;
        _spellingCheck.IsChecked = options.Spelling;
        _vocabularyCheck.IsChecked = options.Vocabulary;
        _progressiveCheck.IsChecked = options.Progressive;
        _pronounsCheck.IsChecked = options.Pronouns;
        _colloquialCheck.IsChecked = options.Colloquial;
        _youToTuCheck.IsChecked = options.YouToTu;
        _orthographyChoice.SelectedIndex = options.PostAgreement ? 0 : 1;
        _validateSpellingCheck.IsChecked = options.ValidateSpelling;
        _batchReferenceCheck.IsChecked = options.UseReference;
        _addressChoice.SelectedIndex = (int)options.Address;
        _sentenceContextCheck.IsChecked = options.SentenceContext;
        _rewrapCheck.IsChecked = options.Rewrap;
        _conversationalCheck.IsChecked = options.Conversational;
        _layoutWarningsCheck.IsChecked = options.LayoutWarnings;
        _lineLength.Value = options.LineLength;
        _maxLines.Value = options.MaxLines;
        _charactersPerSecond.Value = options.CharactersPerSecond;
        _savedDisabledRules = options.DisabledRules;
        _loadingOptions = false;

        foreach (var check in OptionChecks)
        {
            check.IsCheckedChanged += OnOptionChanged;
        }
    }

    private void OnOptionChanged(object? sender, RoutedEventArgs e)
    {
        if (!_loadingOptions && !_isConfigure)
        {
            RefreshProposals();
        }
    }

    private void OnOrthographyChanged(object? sender, SelectionChangedEventArgs e) =>
        OnOptionChanged(sender, e);

    private void OnLayoutChanged(object? sender, NumericUpDownValueChangedEventArgs e) => OnOptionChanged(sender, e);

    private void OnToggleSettings(object? sender, RoutedEventArgs e)
    {
        var panel = this.FindControl<StackPanel>("SettingsPanel")!;
        panel.IsVisible = !panel.IsVisible;
        var action = panel.IsVisible ? "Collapse settings" : "Expand settings";
        var button = this.FindControl<Button>("SettingsToggleButton")!;
        ToolTip.SetTip(button, action + (panel.IsVisible ? " to show more subtitle lines" : ""));
        Avalonia.Automation.AutomationProperties.SetName(button, action);
        this.FindControl<PathIcon>("SettingsToggleIcon")!.Data = Avalonia.Media.Geometry.Parse(
            panel.IsVisible ? "M 2,11 L 8,5 L 14,11" : "M 2,5 L 8,11 L 14,5");
    }

    private async void OnContributeVocabulary(object? sender, RoutedEventArgs e)
    {
        var editor = new ContributionsWindow(_wordListPath);
        await editor.ShowDialog(this);
        if (editor.LocalWordListSaved && !_isConfigure) RefreshProposals();
    }

    private async void OnCheckDictionary(object? sender, RoutedEventArgs e) => await UpdateDictionary(false);
    private async void OnDownloadDictionary(object? sender, RoutedEventArgs e) => await UpdateDictionary(true);

    private async Task UpdateDictionary(bool download)
    {
        if (_dictionaryBusy) return;
        _dictionaryBusy = true;
        var check = this.FindControl<Button>("DictionaryCheckButton")!;
        var reload = this.FindControl<Button>("DictionaryDownloadButton")!;
        check.IsEnabled = reload.IsEnabled = _orthographyChoice.IsEnabled = false;
        _dictionaryStatus.Text = "Contacting Natura (only the dictionary is downloaded; no subtitle text is sent)…";
        try
        {
            var result = await NaturaDictionary.UpdateAsync(Options.PostAgreement, download);
            if (download && !_isConfigure) RefreshProposals();
            _dictionaryStatus.Text = result;
        }
        catch (Exception ex) when (ex is HttpRequestException or IOException or OperationCanceledException or UnauthorizedAccessException or FormatException)
        {
            _dictionaryStatus.Text = $"Dictionary update failed: {ex.Message}. Existing dictionary kept.";
        }
        finally
        {
            check.IsEnabled = reload.IsEnabled = _orthographyChoice.IsEnabled = true;
            _dictionaryBusy = false;
        }
    }

    /// <summary>Opened from Batch convert's Configure button: choose the rule groups, edit the local word list.</summary>
    private void ShowConfigure()
    {
        this.FindControl<Button>("SettingsToggleButton")!.IsVisible = false;
        this.FindControl<Expander>("ReviewTools")!.IsVisible = false;
        this.FindControl<Grid>("ReviewLayoutPanels")!.ColumnDefinitions[0].Width = new GridLength(0);
        Title = $"{BatchMode.Name} - Batch convert settings";
        _selectButtons.IsVisible = false;
        _referencePathRow.IsVisible = false;
        _batchReferenceCheck.IsVisible = true;
        _referencePanel.IsExpanded = true;
        _referencePanel.Header = "English reference subtitle";
        _referenceStatus.Text = "For each file, Batch convert uses the English .srt next to it (same name with .en/.eng/.english, " +
                                "or the only English .srt of the same SxxEyy), aligned with the MiniLM settings above.";
        _changesBorder.IsVisible = false;
        _noChangesLabel.IsVisible = false;
        _configurePanel.IsVisible = true;
        _subtitleLabel.Text = "Settings for Batch convert.";
        _summaryLabel.Text = "Used for every file in Batch convert";
        _applyButton.Content = "OK";
        _applyButton.IsEnabled = true;
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
        _subtitleLabel.Text = $"Convert Brazilian Portuguese to European Portuguese in {scope}. Review every change before applying.";

        PortugueseLocalWordList? localWordList = null;
        try
        {
            localWordList = PortugueseLocalWordList.Load(_wordListPath);
        }
        catch (Exception ex)
        {
            _subtitleLabel.Text += $" Local word list ignored - could not read {_wordListPath}: {ex.Message}";
        }

        try
        {
            BuildProposals(localWordList);
            _dictionaryStatus.Text = Options.ValidateSpelling
                ? ValidationSummary()
                : "Natura dictionaries available offline. Spelling selection applies when Spelling is enabled.";
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or FormatException)
        {
            _proposals.Clear();
            _dictionaryStatus.Text = $"Could not load the Natura dictionary: {ex.Message}. Conversion stopped.";
            _changesBorder.IsVisible = false;
            _applyButton.IsEnabled = false;
            return;
        }

        var hasChanges = _proposals.Count > 0;
        _noChangesLabel.IsVisible = !hasChanges;
        _changesBorder.IsVisible = hasChanges;
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
        UpdateFindingFilters();
        UpdateSummary();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        _summaryLabel = this.FindControl<TextBlock>("SummaryLabel")!;
        _subtitleLabel = this.FindControl<TextBlock>("SubtitleLabel")!;
        _noChangesLabel = this.FindControl<TextBlock>("NoChangesLabel")!;
        _changesList = this.FindControl<ItemsControl>("ChangesList")!;
        _applyButton = this.FindControl<Button>("ApplyButton")!;
        _selectButtons = this.FindControl<Control>("SelectButtons")!;
        _configurePanel = this.FindControl<Control>("ConfigurePanel")!;
        _changesBorder = this.FindControl<Control>("ChangesBorder")!;
        _spellingCheck = this.FindControl<CheckBox>("SpellingCheck")!;
        _vocabularyCheck = this.FindControl<CheckBox>("VocabularyCheck")!;
        _progressiveCheck = this.FindControl<CheckBox>("ProgressiveCheck")!;
        _pronounsCheck = this.FindControl<CheckBox>("PronounsCheck")!;
        _colloquialCheck = this.FindControl<CheckBox>("ColloquialCheck")!;
        _youToTuCheck = this.FindControl<CheckBox>("YouToTuCheck")!;
        _orthographyChoice = this.FindControl<ComboBox>("OrthographyChoice")!;
        _validateSpellingCheck = this.FindControl<CheckBox>("ValidateSpellingCheck")!;
        _dictionaryStatus = this.FindControl<TextBlock>("DictionaryStatus")!;
        _referencePanel = this.FindControl<Expander>("ReferencePanel")!;
        _referencePath = this.FindControl<TextBox>("ReferencePath")!;
        _referencePathRow = this.FindControl<Control>("ReferencePathRow")!;
        _batchReferenceCheck = this.FindControl<CheckBox>("BatchReferenceCheck")!;
        _referenceStatus = this.FindControl<TextBlock>("ReferenceStatus")!;
        _assetStatus = this.FindControl<TextBlock>("AssetStatus")!;
        _embeddingCheck = this.FindControl<CheckBox>("EmbeddingCheck")!;
        _vulkanCheck = this.FindControl<CheckBox>("VulkanCheck")!;
        _deviceChoice = this.FindControl<ComboBox>("DeviceChoice")!;
        _referenceCancelButton = this.FindControl<Button>("ReferenceCancelButton")!;
        _meaningCheck = this.FindControl<CheckBox>("MeaningCheck")!;
        _judgeCheck = this.FindControl<CheckBox>("JudgeCheck")!;
        _addressChoice = this.FindControl<ComboBox>("AddressChoice")!;
        _sentenceContextCheck = this.FindControl<CheckBox>("SentenceContextCheck")!;
        _rewrapCheck = this.FindControl<CheckBox>("RewrapCheck")!;
        _conversationalCheck = this.FindControl<CheckBox>("ConversationalCheck")!;
        _layoutWarningsCheck = this.FindControl<CheckBox>("LayoutWarningsCheck")!;
        _lineLength = this.FindControl<NumericUpDown>("LineLength")!;
        _maxLines = this.FindControl<NumericUpDown>("MaxLines")!;
        _charactersPerSecond = this.FindControl<NumericUpDown>("CharactersPerSecond")!;
        _disableRuleChoice = this.FindControl<ComboBox>("DisableRuleChoice")!;
        _referenceButtons = new Control[]
        {
            this.FindControl<Button>("ReferenceBrowseButton")!, this.FindControl<Button>("ReferenceClearButton")!,
            this.FindControl<Button>("ListDevicesButton")!, this.FindControl<Button>("DownloadModelButton")!, this.FindControl<Button>("DownloadRuntimeButton")!,
            this.FindControl<Button>("DownloadJudgeButton")!, this.FindControl<Button>("RunChecksButton")!,
            _embeddingCheck, _vulkanCheck, _deviceChoice, _meaningCheck, _judgeCheck,
        };
    }

    private string ValidationSummary()
    {
        var selected = new HashSet<int>(_request.SelectedIndices);
        var converted = _proposals.Where(p => p.Include).ToDictionary(p => p.LineIndex, p => p.ConvertedText);
        var texts = _blocks.Select((block, index) => (block, index))
            .Where(item => selected.Count == 0 || selected.Contains(item.index))
            .Select(item => converted.TryGetValue(item.index, out var text) ? text : item.block.Text);
        var unknown = NaturaDictionary.Load(Options.PostAgreement).UnknownWords(texts);
        return unknown.Length == 0 ? "Dictionary validation: all words recognised."
            : $"Dictionary validation: {unknown.Length} unrecognised words (names and foreign words may be valid): " +
              string.Join(", ", unknown.Take(30)) + (unknown.Length > 30 ? "…" : "");
    }

    protected override async void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        this.BringToForeground();
        if (_isConfigure) return;
        await FindReferenceAutomatically();
        if (ChecksReady) await RunChecks();
    }

    /// <summary>Loads "Name.en.srt" / "Name.English.srt" next to the subtitle or video, like Sub_sync.</summary>
    internal async Task FindReferenceAutomatically()
    {
        if (_referenceBlocks != null) return;
        string? path;
        try
        {
            path = ReferenceFinder.Find(_request.Subtitle.FileName, _request.VideoFileName);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        {
            return;
        }
        if (path == null) return;
        try
        {
            _referenceBlocks = ReferenceAligner.Load(path);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidDataException)
        {
            _referenceStatus.Text = $"Found {Path.GetFileName(path)} but could not read it: {ex.Message}";
            return;
        }
        _referencePath.Text = path;
        _referencePanel.IsExpanded = true;
        _referenceStatus.Text = $"Found {Path.GetFileName(path)} automatically. Aligning...";
        await AlignReference();
    }

    private void BuildProposals(PortugueseLocalWordList? localWordList)
    {
        var selected = new HashSet<int>(_request.SelectedIndices);
        var applyToAll = selected.Count == 0;
        var speakers = SubtitleConversion.Speakers(_request, _blocks, out var speakerNote);
        if (speakerNote != null) _subtitleLabel.Text += " " + speakerNote;
        var conversions = SubtitleConversion.Convert(_blocks, Options, localWordList, _alignment?.Texts, selected, speakers);
        var convertedLines = new Dictionary<int, string>();
        _checkLines.Clear();

        for (var i = 0; i < _blocks.Count; i++)
        {
            if (!applyToAll && !selected.Contains(i))
            {
                continue;
            }

            var reference = _alignment?.Texts[i];
            var hint = conversions[i].Hint;
            var text = conversions[i].Text;
            var reviewNotes = new List<string?> { conversions[i].ContextNote };
            var rules = conversions[i].Rules.ToList();
            var previous = i == 0 ? null : convertedLines.TryGetValue(i - 1, out var before) ? before : _blocks[i - 1].Text;
            convertedLines[i] = text;
            var checkLine = new CheckLine(i, _blocks[i].Text, text, previous, reference);
            _checkLines.Add(checkLine);
            if (_checks.TryGetValue(checkLine, out var check))
            {
                if (check.Text != text) rules.Add(new RuleChange("local-judge", "Gervásio chose a fixed alternative", "Local model ranking; see review note"));
                text = check.Text;
                reviewNotes.Add(check.Note);
            }
            var wrapped = SubtitleLayout.Rewrap(text, Options);
            reviewNotes.AddRange(new[] { ReferenceReview.Note(text, hint), GrammarReview.Note(text),
                IdiomHints.Note(text, reference) });
            var review = string.Join("\n", reviewNotes.Where(n => !string.IsNullOrWhiteSpace(n)).Distinct());
            var wrappedReview = SubtitleLayout.Review(wrapped, _blocks[i], Options);
            var languageReview = SubtitleLayout.Review(text, _blocks[i], Options);
            if (wrapped != text) rules.Add(new RuleChange("rewrap", "Optional grammar-aware layout (timings unchanged)"));
            if (text != _blocks[i].Text || wrapped != text || review.Length > 0 || wrappedReview != null)
            {
                _proposals.Add(new ChangeProposal(i, _blocks[i].Text, text, reference, review,
                    rules, wrapped, conversions[i].SentenceGroup, wrappedReview, languageReview));
            }
        }
        _disableRuleChoice.ItemsSource = _proposals.SelectMany(p => p.Rules).Where(r => r.Id != "local-judge")
            .Select(r => r.Id).Distinct().OrderBy(s => s).ToArray();
        _disableRuleChoice.SelectedIndex = -1;
    }

    private void OnProposalChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ChangeProposal.Include))
        {
            if (!_syncingSentence && sender is ChangeProposal { SentenceGroup: >= 0 } changed)
            {
                _syncingSentence = true;
                foreach (var peer in _proposals.Where(p => p.SentenceGroup == changed.SentenceGroup && p.IsChange))
                    peer.Include = changed.Include;
                _syncingSentence = false;
            }
            UpdateSummary();
            if (Options.ValidateSpelling) _dictionaryStatus.Text = ValidationSummary();
        }
        if (e.PropertyName is nameof(ChangeProposal.IncludeLayout) or nameof(ChangeProposal.ConvertedText))
        {
            UpdateFindingFilters();
            UpdateSummary();
            if (Options.ValidateSpelling) _dictionaryStatus.Text = ValidationSummary();
        }
    }

    private void OnDisableRule(object? sender, RoutedEventArgs e)
    {
        if (_disableRuleChoice.SelectedItem is not string rule) return;
        _disabledForFile.Add(rule);
        _checks.Clear();
        RefreshProposals();
        _subtitleLabel.Text += $" Disabled for this file: {string.Join(", ", _disabledForFile)}.";
    }

    private void OnRestoreRules(object? sender, RoutedEventArgs e)
    {
        _disabledForFile.Clear();
        _checks.Clear();
        RefreshProposals();
    }

    private async void OnExportCorrections(object? sender, RoutedEventArgs e)
    {
        try
        {
            var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Export reviewed corrections (Portuguese text may contain personal information)",
                SuggestedFileName = "ptpt-regressions.json",
                FileTypeChoices = new[] { new FilePickerFileType("JSON") { Patterns = new[] { "*.json" } } },
            });
            if (file == null) return;
            await using var stream = await file.OpenWriteAsync();
            stream.SetLength(0);
            await JsonSerializer.SerializeAsync(stream, CorrectionExport.Create(_proposals), new JsonSerializerOptions { WriteIndented = true });
            _subtitleLabel.Text = "Corrections exported. No filenames, timestamps or English reference text were included.";
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or NotSupportedException)
        {
            _subtitleLabel.Text = $"Correction export failed: {ex.Message}";
        }
    }

    private void UpdateSummary()
    {
        var total = _proposals.Count(p => p.IsChange);
        var selected = _proposals.Count(p => p.Include && p.ConvertedText != p.OriginalText);
        var review = _proposals.Count(p => p.HasReview);
        _summaryLabel.Text = $"{selected} of {total} change(s) selected." + (review > 0 ? $" {review} to review." : string.Empty) +
            $" Showing {_proposals.Count(p => p.VisibleInReview)} of {_proposals.Count} row(s).";
        _applyButton.IsEnabled = selected > 0;
    }

    private void OnFindingFilterChanged(object? sender, RoutedEventArgs e)
    {
        UpdateFindingFilters();
        UpdateSummary();
    }

    private void UpdateFindingFilters()
    {
        var enabled = FindingType.None;
        foreach (var (name, type) in new[]
        {
            ("RuleBasedFilter", FindingType.RuleBased), ("MiniLmFilter", FindingType.MiniLm),
            ("JudgeFilter", FindingType.Judge), ("OtherReviewFilter", FindingType.OtherReview),
            ("ManualEditFilter", FindingType.ManualEdit),
        })
            if (this.FindControl<ToggleButton>(name)!.IsChecked == true) enabled |= type;
        foreach (var proposal in _proposals)
            proposal.VisibleInReview = (proposal.FindingTypes & enabled) != 0;
    }

    private void OnSelectAll(object? sender, RoutedEventArgs e)
    {
        foreach (var proposal in _proposals.Where(p => p.IsChange))
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

    private void LoadEmbeddingSettings()
    {
        _loadingEmbedding = true;
        _embeddingSettings = EmbeddingAssets.Load();
        _embeddingCheck.IsChecked = _embeddingSettings.UseEmbeddings;
        _vulkanCheck.IsChecked = _embeddingSettings.Vulkan;
        _meaningCheck.IsChecked = _embeddingSettings.CheckMeaning;
        _judgeCheck.IsChecked = _embeddingSettings.UseJudge;
        SetDevices(new[] { (_embeddingSettings.Device, _embeddingSettings.Device) });
        _loadingEmbedding = false;
        if (!EmbeddingAssets.IsInstalled(_embeddingSettings))
            _referenceStatus.Text += " MiniLM is not installed: timing only until you download the model and llama-server.";
        UpdateAssetStatus();
    }

    private void SetDevices(IEnumerable<(string Id, string Name)> devices)
    {
        var items = devices.Where(d => d.Id.Length > 0)
            .Select(d => new ComboBoxItem { Content = d.Id == d.Name ? d.Id : $"{d.Id}: {d.Name}", Tag = d.Id }).ToList();
        if (items.Count == 0) items.Add(new ComboBoxItem { Content = "Vulkan0", Tag = "Vulkan0" });
        _deviceChoice.ItemsSource = items;
        _deviceChoice.SelectedIndex = Math.Max(0, items.FindIndex(i => (string)i.Tag! == _embeddingSettings.Device));
    }

    private string SelectedDevice => (_deviceChoice.SelectedItem as ComboBoxItem)?.Tag as string ?? "Vulkan0";

    private void SaveEmbeddingSettings()
    {
        _embeddingSettings = _embeddingSettings with
        {
            UseEmbeddings = _embeddingCheck.IsChecked == true,
            Vulkan = _vulkanCheck.IsChecked == true,
            Device = SelectedDevice,
            CheckMeaning = _meaningCheck.IsChecked == true,
            UseJudge = _judgeCheck.IsChecked == true,
        };
        try
        {
            EmbeddingAssets.Save(_embeddingSettings);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            _referenceStatus.Text = $"Could not save the MiniLM settings: {ex.Message}";
        }
    }

    private async void OnEmbeddingSettingChanged(object? sender, RoutedEventArgs e)
    {
        if (_loadingEmbedding) return;
        SaveEmbeddingSettings();
        if (_referenceBlocks != null && sender == _embeddingCheck) await AlignReference();
        if (!_isConfigure && (sender == _meaningCheck || sender == _judgeCheck))
        {
            _checks.Clear();
            RefreshProposals();
            _referenceStatus.Text = "Local check results cleared because settings changed. Run checks to review again.";
            if (((CheckBox)sender).IsChecked == true && ChecksReady) await RunChecks();
        }
    }

    private bool ChecksReady
    {
        get
        {
            var settings = EmbeddingAssets.WithInstalledFiles(_embeddingSettings);
            return LocalChecks.JudgeReady(settings) || LocalChecks.MeaningReady(settings);
        }
    }

    private async void OnRunChecks(object? sender, RoutedEventArgs e) => await RunChecks();

    /// <summary>Runs Gervásio and/or MiniLM on the lines as the rules converted them; results are kept while the window is open.</summary>
    internal async Task RunChecks()
    {
        if (_isConfigure) return;
        await RunReferenceJob(async (progress, token) =>
        {
            _embeddingSettings = EmbeddingAssets.WithInstalledFiles(_embeddingSettings);
            if (!ChecksReady)
                throw new InvalidOperationException("Tick a check and download its model and llama-server first.");
            var pending = _checkLines.ToList();
            var (results, summary) = await LocalChecks.RunAsync(pending, _embeddingSettings, progress, token);
            ApplyCheckResults(pending, results);
            _referenceStatus.Text = "Checks done. " + summary;
        });
    }

    internal string ChecksStatusText => _referenceStatus.Text ?? string.Empty;

    internal void ApplyCheckResults(IReadOnlyList<CheckLine> lines, IReadOnlyDictionary<int, CheckResult> results)
    {
        foreach (var line in lines)
            _checks[line] = results.TryGetValue(line.Index, out var result)
                ? result : new CheckResult(line.Converted, null);
        RefreshProposals();
    }

    private void OnDeviceChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (!_loadingEmbedding) SaveEmbeddingSettings();
    }

    /// <summary>Runs one reference job at a time with a Stop button; errors land in the status line.</summary>
    private async Task RunReferenceJob(Func<IProgress<string>, CancellationToken, Task> job)
    {
        if (_referenceCancel != null) return;
        using var cancel = new CancellationTokenSource();
        _referenceCancel = cancel;
        foreach (var control in _referenceButtons) control.IsEnabled = false;
        _referenceCancelButton.IsEnabled = true;
        _applyButton.IsEnabled = false;
        var progress = new Progress<string>(text => _referenceStatus.Text = text);
        try
        {
            await job(progress, cancel.Token);
        }
        catch (OperationCanceledException)
        {
            _referenceStatus.Text = "Stopped.";
        }
        catch (Exception ex) when (ex is IOException or InvalidDataException or HttpRequestException or UnauthorizedAccessException
                                       or PlatformNotSupportedException or System.ComponentModel.Win32Exception or InvalidOperationException
                                       or System.Text.Json.JsonException)
        {
            _referenceStatus.Text = ex.Message;
        }
        finally
        {
            _referenceCancel = null;
            foreach (var control in _referenceButtons) control.IsEnabled = true;
            _referenceCancelButton.IsEnabled = false;
            if (!_isConfigure) UpdateSummaryOrEmpty();
        }
    }

    private void UpdateSummaryOrEmpty()
    {
        if (_proposals.Count > 0) UpdateSummary();
        else _applyButton.IsEnabled = false;
    }

    private void OnCancelReference(object? sender, RoutedEventArgs e) => _referenceCancel?.Cancel();

    private async Task<string?> PickFile(string title, IReadOnlyList<FilePickerFileType>? types)
    {
        IStorageFolder? start = null;
        var folder = Path.GetDirectoryName(_request.Subtitle.FileName);
        if (!string.IsNullOrEmpty(folder) && Directory.Exists(folder))
        {
            start = await StorageProvider.TryGetFolderFromPathAsync(folder);
        }
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = title, AllowMultiple = false, FileTypeFilter = types, SuggestedStartLocation = start,
        });
        return files.Count == 0 ? null : files[0].TryGetLocalPath() ?? throw new IOException("Only local files are supported.");
    }

    private async void OnBrowseReference(object? sender, RoutedEventArgs e)
    {
        var loaded = false;
        await RunReferenceJob(async (_, _) =>
        {
            var path = await PickFile("English reference subtitle of the same release",
                new[] { new FilePickerFileType("SubRip subtitle") { Patterns = new[] { "*.srt" } }, FilePickerFileTypes.All });
            if (path == null) return;
            _referenceBlocks = ReferenceAligner.Load(path);
            _alignment = null;
            _referencePath.Text = path;
            loaded = true;
        });
        if (loaded) await AlignReference();
    }

    private void OnClearReference(object? sender, RoutedEventArgs e)
    {
        _referenceBlocks = null;
        _alignment = null;
        _referencePath.Text = string.Empty;
        _referenceStatus.Text = "No reference.";
        RefreshProposals();
    }

    /// <summary>Loads a reference without the picker (tests and automation).</summary>
    internal async Task LoadReferenceAsync(string path)
    {
        _referenceBlocks = ReferenceAligner.Load(path);
        _referencePath.Text = path;
        await AlignReference();
    }

    internal ReferenceAlignment? Alignment => _alignment;
    internal IReadOnlyList<ChangeProposal> Proposals => _proposals;
    internal string ReferenceStatusText => _referenceStatus.Text ?? string.Empty;

    private async Task AlignReference()
    {
        if (_referenceBlocks == null) return;
        var reference = _referenceBlocks;
        await RunReferenceJob(async (progress, token) =>
        {
            _embeddingSettings = EmbeddingAssets.WithInstalledFiles(_embeddingSettings) with { UseEmbeddings = _embeddingCheck.IsChecked == true };
            var (alignment, note) = await ReferenceRunner.AlignAsync(_blocks, reference, _embeddingSettings, progress, token);
            _alignment = alignment;
            RefreshProposals();
            var review = _proposals.Count(p => p.HasReview);
            _referenceStatus.Text = alignment.Describe() + note +
                                    (review > 0 ? $" {review} line(s) marked for review (you/your kept as você/seu)." : string.Empty);
        });
    }

    private async void OnListDevices(object? sender, RoutedEventArgs e) =>
        await RunReferenceJob(async (progress, token) =>
        {
            _embeddingSettings = EmbeddingAssets.WithInstalledFiles(_embeddingSettings);
            if (!File.Exists(_embeddingSettings.RuntimePath))
                throw new InvalidOperationException("Click 'Download llama-server' first.");
            progress.Report("Asking the runtime for Vulkan devices...");
            var devices = await EmbeddingRuntime.ListDevicesAsync(_embeddingSettings.RuntimePath, token);
            _loadingEmbedding = true;
            SetDevices(devices);
            _loadingEmbedding = false;
            SaveEmbeddingSettings();
            _referenceStatus.Text = devices.Count == 0
                ? "No hardware Vulkan device found; untick Vulkan GPU to use the CPU."
                : "Vulkan devices: " + string.Join("; ", devices.Select(d => $"{d.Id}: {d.Name}"));
        });

    private async void OnDownloadModel(object? sender, RoutedEventArgs e) =>
        await DownloadAsset(async (progress, token) =>
        {
            var model = await EmbeddingAssets.DownloadModelAsync(progress, token);
            _embeddingSettings = _embeddingSettings with { ModelPath = model };
            return "MiniLM model downloaded and verified (SHA-256).";
        });

    private async void OnDownloadRuntime(object? sender, RoutedEventArgs e) =>
        await DownloadAsset(async (progress, token) =>
        {
            var runtime = await EmbeddingAssets.DownloadRuntimeAsync(progress, token);
            EmbeddingAssets.RemoveOtherRuntimes(runtime);
            _embeddingSettings = _embeddingSettings with { RuntimePath = runtime };
            return "llama-server downloaded and verified (SHA-256). Use List devices to pick the GPU.";
        });

    /// <summary>Downloads one asset; once both are installed, MiniLM is switched on and the reference re-aligned.</summary>
    private async Task DownloadAsset(Func<IProgress<string>, CancellationToken, Task<string>> download)
    {
        var ready = false;
        await RunReferenceJob(async (progress, token) =>
        {
            var message = await download(progress, token);
            _embeddingSettings = EmbeddingAssets.WithInstalledFiles(_embeddingSettings);
            ready = EmbeddingAssets.IsInstalled(_embeddingSettings);
            if (ready)
            {
                _loadingEmbedding = true;
                _embeddingCheck.IsChecked = true;
                _loadingEmbedding = false;
            }
            SaveEmbeddingSettings();
            _referenceStatus.Text = message + (ready ? string.Empty
                : File.Exists(_embeddingSettings.ModelPath) ? " Now download llama-server." : " Now download the MiniLM model.");
        });
        UpdateAssetStatus();
        if (ready && _referenceBlocks != null) await AlignReference();
    }

    private void UpdateAssetStatus()
    {
        var settings = EmbeddingAssets.WithInstalledFiles(_embeddingSettings);
        string Describe(string name, string path) => File.Exists(path) ? $"{name}: {path}" : $"{name}: not downloaded";
        _assetStatus.Text = Describe("Model", settings.ModelPath) + "\n" + Describe("llama-server", settings.RuntimePath) + "\n" +
                            Describe("Gervásio", settings.JudgeModelPath);
    }

    private async void OnDownloadJudge(object? sender, RoutedEventArgs e)
    {
        var ready = false;
        await RunReferenceJob(async (progress, token) =>
        {
            await JudgeAssets.DownloadModelAsync(progress, token);
            _embeddingSettings = EmbeddingAssets.WithInstalledFiles(_embeddingSettings);
            _loadingEmbedding = true;
            _judgeCheck.IsChecked = true;
            _loadingEmbedding = false;
            SaveEmbeddingSettings();
            ready = File.Exists(_embeddingSettings.RuntimePath);
            _referenceStatus.Text = "Gervásio PT-PT downloaded and verified (SHA-256)." + (ready ? string.Empty : " Now download llama-server.");
        });
        UpdateAssetStatus();
        if (ready) await RunChecks();
    }

    internal string AssetStatusText => _assetStatus.Text ?? string.Empty;

    protected override void OnClosed(EventArgs e)
    {
        _referenceCancel?.Cancel();
        EmbeddingRuntime.KillAll();
        base.OnClosed(e);
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
            App.Response = BatchMode.BuildConfigureResponse(SavedOptions);
            Close();
            return;
        }

        try
        {
            var changesByLine = _proposals
                .Where(p => p.Include && p.ConvertedText != p.OriginalText)
                .ToDictionary(p => p.LineIndex, p => p.ConvertedText);

            if (changesByLine.Count == 0)
            {
                App.Response = new PluginResponse { Status = PluginStatus.Cancelled };
                Close();
                return;
            }

            App.Response = BatchMode.BuildOkResponse(_blocks, changesByLine, SavedOptions);
        }
        catch (Exception ex)
        {
            App.Response = new PluginResponse { Status = PluginStatus.Error, Message = ex.Message };
        }

        Close();
    }
}
