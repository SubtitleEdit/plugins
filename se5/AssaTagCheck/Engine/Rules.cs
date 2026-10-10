namespace SubtitleEdit.Plugins.AssaTagCheck;

public enum Severity
{
    Error,
    Warning,
    Tidy,
}

public enum RuleId
{
    // Errors
    UnclosedBlock,
    StrayBrace,
    MissingBackslash,
    MissingParenthesis,
    ArgumentSeparator,
    ArgumentCount,
    HtmlColor,
    UnknownTag,
    InvalidValue,

    // Warnings
    UndefinedStyle,
    FirstWins,
    FadeTooLong,
    PositionOffScreen,

    // Tidy
    UnneededEndStart,
    Overridden,
    AlreadyInEffect,
    EmptyBlock,
    MergeBlocks,
}

/// <summary>One problem found on a line. <see cref="Fixable"/> problems are fixed in the line's new text.</summary>
public sealed record Issue(RuleId Rule, string Message, bool Fixable);

/// <summary>The checked line: its new text/style (unchanged when nothing could be fixed) and what was found.</summary>
public sealed record LineResult(string After, string? StyleAfter, IReadOnlyList<Issue> Issues)
{
    public bool HasIssues => Issues.Count > 0;
}

/// <summary>A problem with the whole script rather than one line, e.g. a font that is not installed.</summary>
public sealed record ScriptIssue(string Title, string Message);

public static class RuleCatalog
{
    public static Severity SeverityOf(RuleId rule) => rule switch
    {
        <= RuleId.InvalidValue => Severity.Error,
        <= RuleId.PositionOffScreen => Severity.Warning,
        _ => Severity.Tidy,
    };

    public static IReadOnlyList<RuleId> All { get; } = Enum.GetValues<RuleId>();
}
