namespace Nikse.SubtitleEdit.PluginLogic.Strategies
{
    public interface IController
    {
        void AddResult(string before, string after, string comment, Paragraph p);
    }
}