using System;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public static class StringExtensions
    {
        public static string[] SplitToLines(this string s)
        {
            return s.Replace(Environment.NewLine, "\n").Replace('\r', '\n').Split('\n');
        }

        public static bool Contains(this string s, char c)
        {
            return s.Length > 0 && s.IndexOf(c) >= 0;
        }

        public static bool ContainsAny(this string s, char[] chars)
        {
            if (s.Length == 0)
                return false;
            for (int i = 0; i < chars.Length; i++)
            {
                if (s.Contains(chars[i]))
                    return true;
            }
            return false;
        }

        public static string FixExtraSpaces(this string s)
        {
            while (s.Contains("  "))
                s = s.Replace("  ", " ");
            s = s.Replace("\r\n ", Environment.NewLine);
            s = s.Replace(" \r\n", Environment.NewLine);
            return s;
        }
    }
}
