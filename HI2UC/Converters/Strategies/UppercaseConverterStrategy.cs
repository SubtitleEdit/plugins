using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Nikse.SubtitleEdit.PluginLogic.Converters.Strategies
{
    public class UppercaseConverterStrategy : IConverterStrategy
    {
        public string Name => "Uppercase";

        public string Execute(string input)
        {
            var sb = new StringBuilder();
            foreach (var splitRange in Evaluate(input))
            {
                var content = input.Substring(splitRange.Start, splitRange.End - splitRange.Start);
                if (splitRange.IsTag)
                {
                    sb.Append(content);
                }
                else
                {
                    sb.Append(content.ToUpper(CultureInfo.CurrentCulture));
                }
            }

            return sb.ToString();
        }

        private bool IsTag(char ch) => ch == '<' | ch == '{';

        private char GetClosingPair(char openTag) => openTag == '<' ? '>' : '}';

        private bool IsWordBoundary(char ch) => ch == ' ' || ch == '-' || ch == '<';

        private IEnumerable<SplitRange> Evaluate(string input)
        {
            var len = input.Length;
            var rangeStart = 0;
            for (var i = 0; i < len; i++)
            {
                var ch = input[i];

                if (IsTag(ch))
                {
                    // process any pending
                    if (rangeStart != i)
                    {
                        yield return Range(rangeStart, i);
                        rangeStart = i;
                    }

                    var closingPair = GetClosingPair(ch);
                    while (ch != closingPair)
                    {
                        i++;

                        // end reached, no closing
                        if (i == len)
                        {
                            yield return Range(rangeStart, i);
                            rangeStart = i;
                        }

                        ch = input[i];
                    }

                    // closing pair found
                    yield return Range(rangeStart, i + 1, true);

                    // update the start to be the current end
                    rangeStart = i + 1;
                }
                else if (IsWordBoundary(ch) && i > rangeStart)
                {
                    yield return Range(rangeStart, i);
                    rangeStart = i;
                }
            }

            // still got pending range to process
            if (len - rangeStart > 0)
            {
                yield return Range(rangeStart, len);
            }
        }

        private SplitRange Range(int start, int end) => Range(start, end, false);
        private SplitRange Range(int start, int end, bool isTag) => new(start, end, isTag);

        public override string ToString() => Name;

        private struct SplitRange
        {
            public int Start { get; }
            public int End { get; }

            public bool IsTag { get; }

            public SplitRange(int start, int end) : this(start, end, false)
            {
            }

            public SplitRange(int start, int end, bool isTag)
            {
                Start = start;
                End = end;
                IsTag = isTag;
            }
        }
    }
}