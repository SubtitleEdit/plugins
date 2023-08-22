using System.Globalization;

namespace Nikse.SubtitleEdit.PluginLogic.Converters.Strategies
{
    public class LowercaseConverterStrategy : IConverterStrategy
    {
        public string Name => "Lowercase";
        
        public string Execute(string input) => input.ToLower(CultureInfo.CurrentCulture);

        public override string ToString() => Name;
    }
}