namespace Nikse.SubtitleEdit.PluginLogic.Strategies
{
    public class NoneStrategy : IStrategy
    {
        public string Name => "None";
        public string Execute(string input) => input;
        public override string ToString() => Name;
    }
}