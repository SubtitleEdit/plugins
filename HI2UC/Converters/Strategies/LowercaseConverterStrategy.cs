namespace Nikse.SubtitleEdit.PluginLogic.Converters.Strategies
{
    public class LowercaseConverterStrategy : IConverterStrategy
    {
        // todo: add cultureinfo
        public string Name => "Lowercase";
        
        public string Execute(string input) => input.ToLower();

        public override string ToString() => Name;
    }
}