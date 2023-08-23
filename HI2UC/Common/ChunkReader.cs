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
                    yield return ChunkFromRange(rangeStart, i);
                    rangeStart = i;
                }

                var closingPair = TagUtils.GetClosingPair(ch);
                while (ch != closingPair)
                {
                    i++;

                    // end reached, no closing
                    if (i == len)
                    {
                        yield return ChunkFromRange(rangeStart, i);
                        rangeStart = i;
                    }

                    ch = input[i];
                }

                // closing pair found
                yield return ChunkFromRange(rangeStart, i + 1, true);

                // update the start to be the current end
                rangeStart = i + 1;
            }
            else if (IsWordBoundary(ch) && i > rangeStart)
            {
                yield return ChunkFromRange(rangeStart, i);
                rangeStart = i;
            }
        }

        // still got pending range to process
        if (len - rangeStart > 0)
        {
            yield return ChunkFromRange(rangeStart, len);
        }
    }

    private Chunk ChunkFromRange(int start, int end) => ChunkFromRange(start, end, false);

    private Chunk ChunkFromRange(int start, int end, bool isTag) => new(start, end, isTag);

    public struct Chunk
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