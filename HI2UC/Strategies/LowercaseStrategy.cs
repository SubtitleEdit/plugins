namespace Nikse.SubtitleEdit.PluginLogic.Strategies
{
    public class LowercaseStrategy : IStrategy
    {
        // todo: add cultureinfo
        public string Name => "Lowercase";
        
        public string Execute(string input) => input.ToLower();

        public override string ToString() => Name;
    }
}