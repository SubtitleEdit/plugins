namespace Nikse.SubtitleEdit.PluginLogic.Strategies
{
    public class UppercaseStrategy : IStrategy
    {
        public string Name => "Uppercase";
        
        public string Execute(string input) => input.ToUpper();

        public override string ToString() => Name;
    }
}