namespace PluginCoreLib.Utils
{
    using System;

    public static class StringExtensions
    {
        public static string[] SplitToLines(this string s)
        {
            return s.Replace(Environment.NewLine, "\n").Replace('\r', '\n').Split('\n');
        }

        public static string FixExtraSpaces(this string s)
        {
            if (string.IsNullOrEmpty(s))
                return s;
            int len = s.Length;
            int k = -1;
            for (int i = len - 1; i >= 0; i--)
            {
                char ch = s[i];
                if (k < 2)
                {
                    if (ch == 0x20)
                    {
                        k = i + 1;
                    }
                }
                else if (ch != 0x20)
                {
                    // Two or more white-spaces found!
                    if (k - (i + 1) > 1)
                    {
                        // Keep only one white-space.
                        s = s.Remove(i + 1, k - (i + 2));
                    }

                    // No white-space after/before line break.
                    if ((ch == '\n' || ch == '\r') && i + 1 < s.Length && s[i + 1] == 0x20)
                    {
                        s = s.Remove(i + 1, 1);
                    }
                    // Reset remove length.
                    k = -1;
                }
                if (ch == 0x20 && i + 1 < s.Length && (s[i + 1] == '\n' || s[i + 1] == '\r'))
                {
                    s = s.Remove(i, 1);
                }
            }
            return s;
        }

        public static string RemoveNullChar(this string line) => line.Replace('\0', ' ');

        public static bool Contains(this string s, char c) => s.Length > 0 && s.IndexOf(c) >= 0;

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

        public static bool StartsWith(this string s, char c) => s.Length > 0 && s[0] == c;

        public static bool EndsWith(this string s, char c) => s.Length > 0 && s[s.Length - 1] == c;

    }
}
