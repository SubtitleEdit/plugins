using System.Linq;

namespace Nikse.SubtitleEdit.PluginLogic.Converters.Strategies
{
    public class TitleWordsConverterStrategy : IConverterStrategy
    {
        public string Name => "Title words";

        public string Execute(string input)
        {
            return string.Join(" ", input.Split(' ', '-').Select(w => w.CapitalizeFirstLetter()));
        }

        public override string ToString() => Name;
    }
}