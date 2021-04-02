namespace Nikse.SubtitleEdit.PluginLogic.Strategies
{
    public interface IStrategy
    {
        string Name { get; }
        string Execute(string input);
    }
}