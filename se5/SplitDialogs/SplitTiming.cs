namespace SubtitleEdit.Plugins.SplitDialogs;

public sealed record SplitPart(long StartMs, long EndMs, string Text);

/// <summary>
/// Shares a subtitle's time between its speakers the way Subtitle Edit's "Split" does
/// (SplitManager): by text length when the parts differ by more than a couple of characters,
/// no part below a minimum share (25% for two parts), and the gap centered on each cut.
/// </summary>
public static class SplitTiming
{
    public static List<SplitPart> Distribute(long startMs, long endMs, IReadOnlyList<string> texts, int gapMs)
    {
        var count = texts.Count;
        var parts = new List<SplitPart>(count);
        var duration = endMs - startMs;
        if (duration <= 0)
        {
            foreach (var text in texts)
            {
                parts.Add(new SplitPart(startMs, endMs, text));
            }

            return parts;
        }

        var shares = GetShares(texts);
        var cuts = new double[count + 1];
        cuts[0] = startMs;
        for (var i = 0; i < count; i++)
        {
            cuts[i + 1] = cuts[i] + duration * shares[i];
        }

        cuts[count] = endMs;

        // Give up the gap before giving up a positive duration on any part.
        var halfGap = Math.Max(0, gapMs) / 2.0;
        for (var i = 0; i < count; i++)
        {
            var length = cuts[i + 1] - cuts[i] - (i > 0 ? halfGap : 0) - (i < count - 1 ? halfGap : 0);
            if (length < 1)
            {
                halfGap = 0;
                break;
            }
        }

        for (var i = 0; i < count; i++)
        {
            var start = cuts[i] + (i > 0 ? halfGap : 0);
            var end = cuts[i + 1] - (i < count - 1 ? halfGap : 0);
            parts.Add(new SplitPart((long)Math.Round(start), (long)Math.Round(end), texts[i]));
        }

        return parts;
    }

    public static string FormatTime(long ms)
    {
        if (ms < 0)
        {
            ms = 0;
        }

        return $"{ms / 3_600_000:D2}:{ms / 60_000 % 60:D2}:{ms / 1000 % 60:D2},{ms % 1000:D3}";
    }

    private static double[] GetShares(IReadOnlyList<string> texts)
    {
        var count = texts.Count;
        var lengths = texts.Select(t => (double)DialogSplitter.StripTags(t).Replace("\n", string.Empty).Length).ToArray();
        var shares = new double[count];
        if (lengths.Max() - lengths.Min() <= 2 || lengths.Sum() <= 0)
        {
            Array.Fill(shares, 1.0 / count);
            return shares;
        }

        // Proportional to length, but a short part keeps at least minShare: clamp it and hand
        // the rest out again among the parts not clamped yet.
        var minShare = 0.5 / count;
        var clamped = new bool[count];
        bool clampedAny;
        do
        {
            clampedAny = false;
            var clampedCount = clamped.Count(c => c);
            var freeShare = 1.0 - clampedCount * minShare;
            var freeLength = lengths.Where((_, i) => !clamped[i]).Sum();
            for (var i = 0; i < count; i++)
            {
                if (clamped[i])
                {
                    shares[i] = minShare;
                    continue;
                }

                shares[i] = freeLength > 0 ? freeShare * lengths[i] / freeLength : freeShare / (count - clampedCount);
                if (shares[i] < minShare)
                {
                    clamped[i] = true;
                    clampedAny = true;
                }
            }
        } while (clampedAny);

        return shares;
    }
}
