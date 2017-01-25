using System;

namespace SubtitleEdit
{
    public static class StringExtensions
    {

        public static bool StartsWith(this string s, char c)
        {
            return s.Length > 0 && s[0] == c;
        }

        public static int IndexOfAny(this string s, string[] words, StringComparison comparisonType)
        {
            if (words == null || string.IsNullOrEmpty(s))
                return -1;
            for (int i = 0; i < words.Length; i++)
            {
                var idx = s.IndexOf(words[i], comparisonType);
                if (idx >= 0)
                    return idx;
            }
            return -1;
        }

        public static bool EndsWith(this string s, char c)
        {
            return s.Length > 0 && s[s.Length - 1] == c;
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

    }
}
