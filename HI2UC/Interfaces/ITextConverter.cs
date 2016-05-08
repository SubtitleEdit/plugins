namespace Nikse.SubtitleEdit.PluginLogic
{
    interface ITextConverter
    {
        string ToText(Subtitle subtitle, string title);
    }
}
