using System;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public static class StringExtensions
    {
        public static string[] SplitToLines(this string s)
        {
            return s.Replace(Environment.NewLine, "\n").Replace('\r', '\n').Split('\n');
        }

        public static bool Contains(this string s, char c) => s.Length > 0 && s.IndexOf(c) >= 0;

        public static bool Contains(this string s, string find, StringComparison comparison)
        {
            return s.Length >= find.Length && s.IndexOf(find, comparison) >= 0;
        }

        public static string FixExtraSpaces(this string s)
        {
            while (s.Contains("  "))
                s = s.Replace("  ", " ");
            s = s.Replace("\r\n ", Environment.NewLine);
            s = s.Replace(" \r\n", Environment.NewLine);
            return s;
        }

        public static bool StartsWith(this string s, char c) => s.Length > 0 && s[0] == c;

        public static bool EndsWith(this string s, char c) => s.Length > 0 && s[s.Length - 1] == c;
    }
}
