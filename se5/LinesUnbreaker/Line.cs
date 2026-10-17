namespace SubtitleEdit.Plugins.LinesUnbreaker;

/// <summary>One line of a subtitle, classified on its text without tags: dialog, mood or narrator.</summary>
public readonly struct Line
{
    public Line(string line)
    {
        Content = line;
        var noTagLine = LineUnbreaker.RemoveTags(line);
        HasMood = noTagLine.Contains('(') || noTagLine.Contains('[');
        IsDialog = noTagLine.TrimStart().StartsWith('-');
        HasNarrator = CheckNarrator(noTagLine);
    }

    public string Content { get; }
    public bool IsDialog { get; }
    public bool HasMood { get; }
    public bool HasNarrator { get; }

    private static bool CheckNarrator(string line)
    {
        var colonIndex = line.IndexOf(':');

        // doesn't have colon or colon is last char
        if (colonIndex < 0 || colonIndex + 1 == line.Length)
        {
            return false;
        }

        // e.g. 12:30
        if (char.IsDigit(line[colonIndex + 1]))
        {
            return false;
        }

        // ? . , shouldn't exist in a narrator label
        var narratorCandidate = line.Substring(0, colonIndex);
        return narratorCandidate.IndexOfAny(new[] { '?', '.', ',' }) < 0;
    }

    public override string ToString() => Content;
}
