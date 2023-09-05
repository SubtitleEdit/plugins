namespace Nikse.SubtitleEdit.PluginLogic.Extensions
{
    public static class CharExtensions
    {
        public static bool IsTagStart(this char ch) => ch == '<' || ch == '{';
        public static char ClosingPair(this char ch) => ch == '<' ? '>' : '}';
        public static bool IsClosed(this char ch) => ch == '.' || ch == '?' || ch == '!' || ch == ']' || ch == ')';
    }
}