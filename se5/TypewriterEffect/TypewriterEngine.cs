using System.Text;

namespace SubtitleEdit.Plugins.TypewriterEffect;

/// <summary>
/// Port of the SE4 EffectTypewriter logic: each selected paragraph is replaced
/// by N short paragraphs that progressively reveal the text character by
/// character, optionally followed by a final paragraph holding the full line
/// for <paramref name="endDelaySeconds"/> before the original end time.
/// </summary>
public static class TypewriterEngine
{
    public static (List<SrtBlock> Blocks, int ChangedCount) Apply(
        List<SrtBlock> blocks,
        HashSet<int> selectedIndices,
        double endDelaySeconds)
    {
        var output = new List<SrtBlock>(blocks.Count);
        var changed = 0;
        for (var i = 0; i < blocks.Count; i++)
        {
            var block = blocks[i];
            // Empty SelectedIndices means "apply to every line" (matches plugin contract).
            var isSelected = selectedIndices.Count == 0 || selectedIndices.Contains(i);
            if (!isSelected)
            {
                output.Add(block);
                continue;
            }

            var generated = GenerateForParagraph(block, endDelaySeconds);
            if (generated.Count > 0)
            {
                output.AddRange(generated);
                changed++;
            }
            else
            {
                output.Add(block);
            }
        }

        return (output, changed);
    }

    private static List<SrtBlock> GenerateForParagraph(SrtBlock p, double endDelaySeconds)
    {
        var totalMs = p.EndMs - p.StartMs;
        if (totalMs <= 500 || string.IsNullOrEmpty(p.Text))
        {
            return new List<SrtBlock>();
        }

        var endDelayMs = (long)(endDelaySeconds * 1000);
        if (endDelayMs >= totalMs - 200)
        {
            endDelayMs = Math.Max(0, totalMs - 200);
        }

        var workDurationMs = totalMs - endDelayMs;
        var visibleCharCount = CountVisibleChars(p.Text);
        if (visibleCharCount == 0)
        {
            return new List<SrtBlock>();
        }

        var stepMs = workDurationMs / (double)visibleCharCount;

        var result = new List<SrtBlock>();
        var text = string.Empty;
        var tag = string.Empty;
        var beforeEndTag = string.Empty;
        var alignment = string.Empty;
        var tagOn = false;
        var index = 0;
        var i = 0;

        // Eat any leading ASSA override block(s) into "alignment" so it prefixes every emitted paragraph.
        if (p.Text.StartsWith("{\\", StringComparison.Ordinal))
        {
            var j = 0;
            while (j < p.Text.Length && p.Text[j] == '{' && j + 1 < p.Text.Length && p.Text[j + 1] == '\\')
            {
                var close = p.Text.IndexOf('}', j);
                if (close < 0)
                {
                    break;
                }
                j = close + 1;
            }
            alignment = p.Text.Substring(0, j);
            i = j;
        }

        while (i < p.Text.Length)
        {
            if (p.Text[i] == '{' && i + 1 < p.Text.Length && p.Text[i + 1] == '\\')
            {
                var endIndex = p.Text.IndexOf('}', i);
                if (endIndex >= 0)
                {
                    // Inline ASSA override - keep it attached to the current text but don't tick the clock.
                    text += p.Text.Substring(i, endIndex - i + 1);
                    i = endIndex + 1;
                    continue;
                }
            }

            if (tagOn)
            {
                tag += p.Text[i];
                if (p.Text[i] == '>')
                {
                    tagOn = false;
                    var lowered = tag.ToLowerInvariant();
                    if (lowered.StartsWith("<font ", StringComparison.Ordinal))
                    {
                        beforeEndTag = "</font>";
                    }
                    else if (lowered == "<i>")
                    {
                        beforeEndTag = "</i>";
                    }
                    else if (lowered == "<b>")
                    {
                        beforeEndTag = "</b>";
                    }
                    else if (lowered == "<u>")
                    {
                        beforeEndTag = "</u>";
                    }
                    else if (lowered.StartsWith("</", StringComparison.Ordinal))
                    {
                        beforeEndTag = string.Empty;
                    }
                }
            }
            else if (p.Text[i] == '<')
            {
                tagOn = true;
                tag = "<";
            }
            else
            {
                text += tag + p.Text[i];
                tag = string.Empty;

                var startMs = p.StartMs + (long)(index * stepMs);
                var endMs = p.StartMs + (long)((index + 1) * stepMs) - 1;
                if (endMs < startMs)
                {
                    endMs = startMs;
                }
                result.Add(new SrtBlock
                {
                    StartMs = startMs,
                    EndMs = endMs,
                    Text = alignment + text + beforeEndTag,
                });
                index++;
            }

            i++;
        }

        if (endDelayMs > 0 && result.Count > 0)
        {
            var startMs = p.StartMs + workDurationMs;
            result.Add(new SrtBlock { StartMs = startMs, EndMs = p.EndMs, Text = p.Text });
        }
        else if (result.Count > 0)
        {
            result[^1].EndMs = p.EndMs;
        }

        return result;
    }

    private static int CountVisibleChars(string text)
    {
        var count = 0;
        var i = 0;
        var inTag = false;
        while (i < text.Length)
        {
            if (text[i] == '{' && i + 1 < text.Length && text[i + 1] == '\\')
            {
                var end = text.IndexOf('}', i);
                if (end >= 0)
                {
                    i = end + 1;
                    continue;
                }
            }

            if (text[i] == '<')
            {
                inTag = true;
                i++;
                continue;
            }

            if (text[i] == '>')
            {
                inTag = false;
                i++;
                continue;
            }

            if (!inTag)
            {
                count++;
            }

            i++;
        }
        return count;
    }
}
