namespace Nikse.SubtitleEdit.PluginLogic.Common;

public static class TagUtils
{
    public static bool IsOpenTag(char ch) => ch == '<' || ch == '{';

    public static char GetClosingPair(char ch) => ch == '<' ? '>' : '}';
}