namespace Nikse.SubtitleEdit.PluginLogic.Helpers;

public static class ListViewExtensions
{
    public static string ToListViewText(this string input) => input.Replace(Environment.NewLine, "<br/>");
    public static string ToDomainText(this string input) => input.Replace("<br/>", Environment.NewLine);
}