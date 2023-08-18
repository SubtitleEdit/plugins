namespace Nikse.SubtitleEdit.PluginLogic.Converters.Strategies
{
    public class UppercaseConverterStrategy : IConverterStrategy
    {
        public string Name => "Uppercase";
        
        public string Execute(string input) => input.ToUpper();

        public override string ToString() => Name;
    }
}