namespace SubtitleEdit.Plugins.PersianErrors;

/// <summary>The result of running the enabled groups over one subtitle line.</summary>
public sealed record LineFix(int Index, string Before, string After, IReadOnlyList<string> Groups);

/// <summary>
/// Runs rule groups over subtitle text the way the original plugin did: <c>&lt;br /&gt;</c> becomes a
/// line break, every enabled group's rules run in file order, and the result is trimmed.
/// </summary>
public static class PersianFixer
{
    /// <summary>Fixes one text. <paramref name="changedGroups"/> gets the groups that changed it.</summary>
    public static string Fix(string text, IEnumerable<RuleGroup> groups, ICollection<string>? changedGroups = null)
    {
        var newLine = DetectNewLine(text);
        var working = ToRuleText(text);
        var before = working;

        foreach (var group in groups)
        {
            var result = group.Apply(working);
            if (!string.Equals(result, working, StringComparison.Ordinal))
            {
                changedGroups?.Add(group.Name);
                working = result;
            }
        }

        working = working.Trim();
        return string.Equals(working, before, StringComparison.Ordinal)
            ? text
            : working.Replace(RuleSet.NewLine, newLine, StringComparison.Ordinal);
    }

    /// <summary>Returns a fix for every line in <paramref name="indices"/> that the groups change.</summary>
    public static List<LineFix> FindFixes(
        IReadOnlyList<string> texts,
        IEnumerable<int> indices,
        IReadOnlyList<RuleGroup> groups,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var fixes = new List<LineFix>();
        var list = indices.ToList();
        for (var i = 0; i < list.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var index = list[i];
            var changed = new List<string>();
            var after = Fix(texts[index], groups, changed);
            if (!string.Equals(after, texts[index], StringComparison.Ordinal))
            {
                fixes.Add(new LineFix(index, texts[index], after, changed));
            }

            if (progress != null && (i % 25 == 0 || i == list.Count - 1))
            {
                progress.Report((i + 1) / (double)list.Count);
            }
        }

        return fixes;
    }

    /// <summary>The rules were written for Windows text: <c>\r\n</c> line breaks, <c>&lt;br /&gt;</c> as a break.</summary>
    private static string ToRuleText(string text) => text
        .Replace("<br />", "\n", StringComparison.Ordinal)
        .Replace("</ br>", "\n", StringComparison.Ordinal)
        .Replace("\r\n", "\n", StringComparison.Ordinal)
        .Replace('\r', '\n')
        .Replace("\n", RuleSet.NewLine, StringComparison.Ordinal);

    private static string DetectNewLine(string text)
    {
        if (text.Contains("\r\n", StringComparison.Ordinal))
        {
            return "\r\n";
        }

        return text.Contains('\n') ? "\n" : Environment.NewLine;
    }
}
