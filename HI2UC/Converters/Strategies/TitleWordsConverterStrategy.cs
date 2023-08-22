using System.Linq;

namespace Nikse.SubtitleEdit.PluginLogic.Converters.Strategies
{
    public class TitleWordsConverterStrategy : IConverterStrategy
    {
        public string Name => "Title case (word)";

        public string Execute(string input)
        {
            // this won't be affect even if one of the word has tag the tag is what we will try to
            // convert the casing instead of the tag-name so <i> won't turn out <I>
            input = string.Join(" ", input.Split(' ').Select(word => word.CapitalizeFirstLetter()));
            return string.Join("-", input.Split('-').Select(word => word.CapitalizeFirstLetter()));
        }

        public override string ToString() => Name;
    }
}