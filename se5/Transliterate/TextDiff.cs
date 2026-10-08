namespace SubtitleEdit.Plugins.Transliterate;

public enum DiffKind
{
    Same,
    Removed,
    Added,
}

public sealed record DiffSegment(string Text, DiffKind Kind);
