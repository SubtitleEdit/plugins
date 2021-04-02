using System.Linq;

namespace Nikse.SubtitleEdit.PluginLogic.Strategies
{
    public class TitleWordsStrategy : IStrategy
    {
        public string Name => "Title words";

        public string Execute(string input)
        {
            return string.Join(" ", input.Split(' ', '-').Select(w => w.CapitalizeFirstLetter()));
        }

        public override string ToString() => Name;
    }
}