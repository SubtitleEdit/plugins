using CommunityToolkit.Mvvm.ComponentModel;

namespace SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;

[Flags]
public enum FindingType
{
    None = 0,
    RuleBased = 1,
    MiniLm = 2,
    Judge = 4,
    OtherReview = 8,
    ManualEdit = 16,
    Grammar = 32,
}

public partial class ChangeProposal : ObservableObject
{
    [ObservableProperty] private bool _include;
    [ObservableProperty] private bool _includeLayout = true;
    [ObservableProperty] private string _expectedText = "";
    [ObservableProperty] private bool _visibleInReview = true;

    public int LineIndex { get; }
    public string LineNumber => (LineIndex + 1).ToString();
    public string OriginalText { get; }
    public string LanguageText { get; }
    public string WrappedText { get; }
    private string? _edited;
    /// <summary>The text that will be applied: the user's manual edit when there is one, otherwise the plugin's result.</summary>
    public string ConvertedText
    {
        get => _edited ?? (IncludeLayout ? WrappedText : LanguageText);
        set
        {
            var text = NormalizeBreaks(value ?? string.Empty);
            var automatic = IncludeLayout ? WrappedText : LanguageText;
            var edited = text == automatic ? null : text;
            if (edited == _edited) return;
            _edited = edited;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsChange));
            ExpectedText = ConvertedText;
            Include = IsChange;
        }
    }
    public bool HasLayoutChange => LanguageText != WrappedText;
    public IReadOnlyList<RuleChange> Rules { get; }
    public string Explanation => string.Join("\n", Rules.Select(r => $"{r.Id}: {r.Description} [{r.Basis}]")) +
        (Rules.Count > 0 ? "\nRule-based result; context/reference inferences still need review." : "");
    public bool HasExplanation => Rules.Count > 0;
    public int SentenceGroup { get; }
    public string ReferenceText { get; }
    public bool HasReference => ReferenceText.Length > 0;
    private readonly string _reviewNote;
    private readonly string _wrappedLayoutNote;
    private readonly string _languageLayoutNote;
    public string ReviewNote => string.Join("\n", new[] { _reviewNote, IncludeLayout ? _wrappedLayoutNote : _languageLayoutNote }
        .Where(n => n.Length > 0));
    public bool HasReview => ReviewNote.Length > 0;
    public FindingType FindingTypes
    {
        get
        {
            var types = FindingType.None;
            if (Rules.Any(r => r.Id != "local-judge") ||
                Rules.Count == 0 && (OriginalText != LanguageText || HasLayoutChange))
                types |= FindingType.RuleBased;
            if (Rules.Any(r => r.Id == "local-judge")) types |= FindingType.Judge;
            if (Rules.Any(r => r.Id == "languagetool")) types |= FindingType.Grammar;
            foreach (var note in ReviewNote.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                types |= note.StartsWith("MiniLM", StringComparison.Ordinal) ? FindingType.MiniLm :
                    note.StartsWith("Gervásio", StringComparison.Ordinal) ? FindingType.Judge :
                    note.StartsWith("LanguageTool", StringComparison.Ordinal) || note.StartsWith("Grammar:", StringComparison.Ordinal)
                        ? FindingType.Grammar : FindingType.OtherReview;
            if (_edited != null) types |= FindingType.ManualEdit;
            return types == FindingType.None ? FindingType.OtherReview : types;
        }
    }
    /// <summary>False for review-only lines: there is nothing to apply.</summary>
    public bool IsChange => OriginalText != LanguageText || OriginalText != WrappedText || _edited != null && _edited != OriginalText;

    public ChangeProposal(int lineIndex, string originalText, string convertedText, string? referenceText = null, string? reviewNote = null,
        IReadOnlyList<RuleChange>? rules = null, string? wrappedText = null, int sentenceGroup = -1,
        string? wrappedLayoutNote = null, string? languageLayoutNote = null)
    {
        LineIndex = lineIndex;
        OriginalText = originalText;
        LanguageText = convertedText;
        WrappedText = wrappedText ?? convertedText;
        Rules = rules ?? Array.Empty<RuleChange>();
        SentenceGroup = sentenceGroup;
        _expectedText = WrappedText;
        ReferenceText = string.IsNullOrWhiteSpace(referenceText) ? string.Empty : "EN: " + referenceText.Replace('\n', ' ');
        _reviewNote = reviewNote ?? string.Empty;
        _wrappedLayoutNote = wrappedLayoutNote ?? string.Empty;
        _languageLayoutNote = languageLayoutNote ?? string.Empty;
        _include = IsChange;
    }

    partial void OnIncludeChanged(bool value)
    {
        if (value && !IsChange) Include = false;
    }

    private string NormalizeBreaks(string text)
    {
        var lines = text.Replace("\r\n", "\n");
        return OriginalText.Contains("\r\n") ? lines.Replace("\n", "\r\n") : lines;
    }

    partial void OnIncludeLayoutChanged(bool value)
    {
        _edited = null;
        OnPropertyChanged(nameof(IsChange));
        OnPropertyChanged(nameof(ConvertedText));
        OnPropertyChanged(nameof(ReviewNote));
        OnPropertyChanged(nameof(HasReview));
        ExpectedText = ConvertedText;
    }
}
