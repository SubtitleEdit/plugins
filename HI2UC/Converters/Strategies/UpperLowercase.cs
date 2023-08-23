using System.Linq;
using Nikse.SubtitleEdit.PluginLogic.Common;

namespace Nikse.SubtitleEdit.PluginLogic.Converters.Strategies
{
    public class UpperLowercase : IConverterStrategy
    {
        public string Name => "Toggle case";
        
        public string Execute(string input)
        {
            var buffer = new char[input.Length];
            var len = input.Length;
            var writeTrack = 0;
            var caseState = true;
            for (var i = 0; i < len; i++)
            {
                var ch = input[i];
                // skip tags
                if (TagUtils.IsOpenTag(ch))
                {
                    var closingPair = TagUtils.GetClosingPair(ch);
                    // look for closing tag
                    while (input[i] != closingPair)
                    {
                        buffer[writeTrack++] = ch;
                        if (++i == len)
                        {
                            return input;
                        }

                        ch = input[i];
                    }
                }

                if (!char.IsLetter(ch))
                {
                    buffer[writeTrack++] = ch;
                }
                else
                {
                    buffer[writeTrack++] = caseState ? char.ToUpper(ch) : char.ToLower(ch);
                    caseState = !caseState;
                }
            }

            return new string(buffer, 0, writeTrack);
        }

        public override string ToString() => Name;
    }
}