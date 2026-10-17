namespace SubtitleEdit.Plugins.HI2UC;

/// <summary>Splits text into word and tag chunks, so casing never touches a tag like &lt;font color="..."&gt;.</summary>
public class ChunkReader
{
    private static bool IsWordBoundary(char ch) => ch == ' ' || ch == '-' || ch == '<';

    public IEnumerable<Chunk> Read(string input)
    {
        var len = input.Length;
        var rangeStart = 0;
        for (var i = 0; i < len; i++)
        {
            var ch = input[i];

            if (TextUtils.IsOpenTag(ch))
            {
                // process any pending
                if (rangeStart != i)
                {
                    yield return new Chunk(rangeStart, i, false);
                    rangeStart = i;
                }

                var closingPair = TextUtils.GetClosingPair(ch);
                while (ch != closingPair)
                {
                    i++;

                    // end reached, no closing: the rest is text
                    if (i == len)
                    {
                        yield return new Chunk(rangeStart, len, false);
                        yield break;
                    }

                    ch = input[i];
                }

                // closing pair found
                yield return new Chunk(rangeStart, i + 1, true);

                // update the start to be the current end
                rangeStart = i + 1;
            }
            else if (IsWordBoundary(ch) && i > rangeStart)
            {
                yield return new Chunk(rangeStart, i, false);
                rangeStart = i;
            }
        }

        // still got pending range to process
        if (len - rangeStart > 0)
        {
            yield return new Chunk(rangeStart, len, false);
        }
    }

    public readonly record struct Chunk(int Start, int End, bool IsTag);
}
