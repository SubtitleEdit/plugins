using System.Linq;

namespace Nikse.SubtitleEdit.PluginLogic.Converters.Strategies
{
    public class UpperLowercase : IConverterStrategy
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