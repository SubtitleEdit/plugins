using System.Collections.Generic;

namespace Nikse.SubtitleEdit.PluginLogic.Common;

public class ChunkReader
{
    private bool IsWordBoundary(char ch) => ch == ' ' || ch == '-' || ch == '<';

    public IEnumerable<Chunk> Read(string input)
    {
        var len = input.Length;
        var rangeStart = 0;
        for (var i = 0; i < len; i++)
        {
            var ch = input[i];

            if (TagUtils.IsOpenTag(ch))
            {
                // process any pending
                if (rangeStart != i)
                {
                    yield return NewWordChunk(rangeStart, i);
                    rangeStart = i;
                }

                var closingPair = TagUtils.GetClosingPair(ch);
                while (ch != closingPair)
                {
                    i++;

                    // end reached, no closing
                    if (i == len)
                    {
                        yield return NewWordChunk(rangeStart, i);
                        rangeStart = i;
                    }

                    ch = input[i];
                }

                // closing pair found
                yield return NewTagChunk(rangeStart, i + 1, true);

                // update the start to be the current end
                rangeStart = i + 1;
            }
            else if (IsWordBoundary(ch) && i > rangeStart)
            {
                yield return NewWordChunk(rangeStart, i);
                rangeStart = i;
            }
        }

        // still got pending range to process
        if (len - rangeStart > 0)
        {
            yield return NewWordChunk(rangeStart, len);
        }
    }

    private Chunk NewWordChunk(int start, int end) => new Chunk(start, end, false);

    private Chunk NewTagChunk(int start, int end, bool isTag) => new Chunk(start, end, isTag);

    public readonly struct Chunk
    {
        public int Start { get; }
        public int End { get; }
        public bool IsTag { get; }

        public Chunk(int start, int end, bool isTag)
        {
            Start = start;
            End = end;
            IsTag = isTag;
        }
    }
}