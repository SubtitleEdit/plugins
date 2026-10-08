namespace SubtitleEdit.Plugins.ItalicEachLine;

public enum DiffKind
{
    Same,
    Removed,
    Added,
}

public sealed record DiffSegment(string Text, DiffKind Kind);

/// <summary>Character diff (LCS) used to highlight what a fix changed.</summary>
public static class TextDiff
{
    private const int MaxCells = 1_000_000;

    /// <summary>Returns the segments of the old text (same/removed) and of the new text (same/added).</summary>
    public static (List<DiffSegment> Before, List<DiffSegment> After) Compute(string before, string after)
    {
        // Same number of lines: diff line by line, so line breaks never pair up with the wrong line.
        var beforeLines = before.Split('\n');
        var afterLines = after.Split('\n');
        if (beforeLines.Length > 1 && beforeLines.Length == afterLines.Length)
        {
            var beforeSegments = new List<DiffSegment>();
            var afterSegments = new List<DiffSegment>();
            for (var i = 0; i < beforeLines.Length; i++)
            {
                if (i > 0)
                {
                    Add(beforeSegments, "\n", DiffKind.Same);
                    Add(afterSegments, "\n", DiffKind.Same);
                }

                var (b, a) = ComputeLine(beforeLines[i], afterLines[i]);
                b.ForEach(segment => Add(beforeSegments, segment.Text, segment.Kind));
                a.ForEach(segment => Add(afterSegments, segment.Text, segment.Kind));
            }

            return (beforeSegments, afterSegments);
        }

        return ComputeLine(before, after);
    }

    private static (List<DiffSegment> Before, List<DiffSegment> After) ComputeLine(string before, string after)
    {
        var prefix = 0;
        while (prefix < before.Length && prefix < after.Length && before[prefix] == after[prefix])
        {
            prefix++;
        }

        var suffix = 0;
        while (suffix < before.Length - prefix && suffix < after.Length - prefix &&
               before[before.Length - 1 - suffix] == after[after.Length - 1 - suffix])
        {
            suffix++;
        }

        var a = before.Substring(prefix, before.Length - prefix - suffix);
        var b = after.Substring(prefix, after.Length - prefix - suffix);
        var beforeSegments = new List<DiffSegment>();
        var afterSegments = new List<DiffSegment>();
        Add(beforeSegments, before[..prefix], DiffKind.Same);
        Add(afterSegments, after[..prefix], DiffKind.Same);

        if ((long)a.Length * b.Length > MaxCells)
        {
            Add(beforeSegments, a, DiffKind.Removed);
            Add(afterSegments, b, DiffKind.Added);
        }
        else
        {
            Lcs(a, b, beforeSegments, afterSegments);
        }

        Add(beforeSegments, before[(before.Length - suffix)..], DiffKind.Same);
        Add(afterSegments, after[(after.Length - suffix)..], DiffKind.Same);
        return (beforeSegments, afterSegments);
    }

    private static void Lcs(string a, string b, List<DiffSegment> beforeSegments, List<DiffSegment> afterSegments)
    {
        var lengths = new int[a.Length + 1, b.Length + 1];
        for (var i = a.Length - 1; i >= 0; i--)
        {
            for (var j = b.Length - 1; j >= 0; j--)
            {
                lengths[i, j] = a[i] == b[j]
                    ? lengths[i + 1, j + 1] + 1
                    : Math.Max(lengths[i + 1, j], lengths[i, j + 1]);
            }
        }

        int x = 0, y = 0;
        while (x < a.Length && y < b.Length)
        {
            if (a[x] == b[y])
            {
                Add(beforeSegments, a[x].ToString(), DiffKind.Same);
                Add(afterSegments, b[y].ToString(), DiffKind.Same);
                x++;
                y++;
            }
            else if (lengths[x + 1, y] >= lengths[x, y + 1])
            {
                Add(beforeSegments, a[x].ToString(), DiffKind.Removed);
                x++;
            }
            else
            {
                Add(afterSegments, b[y].ToString(), DiffKind.Added);
                y++;
            }
        }

        Add(beforeSegments, a[x..], DiffKind.Removed);
        Add(afterSegments, b[y..], DiffKind.Added);
    }

    private static void Add(List<DiffSegment> segments, string text, DiffKind kind)
    {
        if (text.Length == 0)
        {
            return;
        }

        if (segments.Count > 0 && segments[^1].Kind == kind)
        {
            segments[^1] = segments[^1] with { Text = segments[^1].Text + text };
        }
        else
        {
            segments.Add(new DiffSegment(text, kind));
        }
    }
}
