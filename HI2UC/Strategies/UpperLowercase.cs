using System.Linq;

namespace Nikse.SubtitleEdit.PluginLogic.Strategies
{
    public class UpperLowercase : IStrategy
    {
        public string Name => "UpperLower case";

        public string Execute(string input)
        {
            var state = true;
            return string.Join(string.Empty, input.Select(ch =>
            {
                if (!char.IsLetter(ch))
                {
                    return ch;
                }

                char output = state ? char.ToUpper(ch) : char.ToLower(ch);
                state = !state;
                return output;
            }));
        }

        public override string ToString() => Name;
    }
}