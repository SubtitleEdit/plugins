using System.Globalization;
using System.Linq;

namespace Nikse.SubtitleEdit.PluginLogic.Converters.Strategies
{
    public class UppercaseConverterStrategy : IConverterStrategy
    {
        public string Name => "Uppercase";

        public string Execute(string input)
        {
            input = string.Join(" ", input.Split(' ').Select(word => Convert(word)));
            return string.Join("-", input.Split('-').Select(word => Convert(word)));
        }

        public string Convert(string word)
        {
            if (!word.StartsWith('<'))
            {
                return word.ToUpper(CultureInfo.CurrentCulture);
            }

            var closingIndex = word.IndexOf('>');
            if (closingIndex < 0) return word.ToUpper(CultureInfo.CurrentCulture);

            // do not change tags casing
            return word.Substring(0, closingIndex + 1) +
                   word.Substring(closingIndex + 1).ToUpper(CultureInfo.CurrentCulture);
        }

        public override string ToString() => Name;
    }
}