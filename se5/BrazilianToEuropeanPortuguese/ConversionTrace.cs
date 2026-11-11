namespace SubtitleEdit.Plugins.BrazilianToEuropeanPortuguese;

public sealed record RuleChange(string Id, string Description, string Basis = "Rule-based");

public sealed record CueConversion(string Text, ReferenceHint? Hint, IReadOnlyList<RuleChange> Rules,
    int SentenceGroup, string? ContextNote)
{
    internal SentenceMeaning? MeaningSentence { get; init; }
}

internal sealed record SentenceMeaning(int First, int Last, string Original, string? Reference);
