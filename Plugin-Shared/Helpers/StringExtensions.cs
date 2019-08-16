using System;
using System.Globalization;
using System.Text;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public static class StringExtensions
    {
        public static bool StartsWith(this string s, char c)
        {
            return s.Length > 0 && s[0] == c;
        }

        public static bool StartsWith(this StringBuilder sb, char c)
        {
            return sb.Length > 0 && sb[0] == c;
        }

        public static bool EndsWith(this string s, char c)
        {
            return s.Length > 0 && s[s.Length - 1] == c;
        }

        public static bool EndsWith(this StringBuilder sb, char c)
        {
            return sb.Length > 0 && sb[sb.Length - 1] == c;
        }

        public static bool Contains(this string source, char value)
        {
            return source.IndexOf(value) >= 0;
        }

        public static bool Contains(this string source, char[] value)
        {
            return source.IndexOfAny(value) >= 0;
        }

        public static bool Contains(this string source, string value, StringComparison comparisonType)
        {
            return source.IndexOf(value, comparisonType) >= 0;
        }

        public static bool ContainsAny(this string s, char[] chars)
        {
            if (s.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < chars.Length; i++)
            {
                if (s.Contains(chars[i]))
                {
                    return true;
                }
            }
            return false;
        }

        public static int IndexOfAny(this string s, string[] words, StringComparison comparisonType)
        {
            if (words == null || string.IsNullOrEmpty(s))
            {
                return -1;
            }

            for (int i = 0; i < words.Length; i++)
            {
                var idx = s.IndexOf(words[i], comparisonType);
                if (idx >= 0)
                {
                    return idx;
                }
            }
            return -1;
        }

        public static string[] SplitToLines(this string source)
        {
            return source.Replace("\r\r\n", "\n").Replace("\r\n", "\n").Replace('\r', '\n').Replace('\u2028', '\n').Split('\n');
        }

        public static string FixExtraSpaces(this string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return s;
            }

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

        public static bool ContainsLetter(this string s)
        {
            if (s != null)
            {
                foreach (var index in StringInfo.ParseCombiningCharacters(s))
                {
                    var uc = CharUnicodeInfo.GetUnicodeCategory(s, index);
                    if (uc == UnicodeCategory.LowercaseLetter || uc == UnicodeCategory.UppercaseLetter || uc == UnicodeCategory.TitlecaseLetter || uc == UnicodeCategory.ModifierLetter || uc == UnicodeCategory.OtherLetter)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static string RemoveControlCharacters(this string s)
        {
            int max = s.Length;
            var newStr = new char[max];
            int newIdx = 0;
            for (int index = 0; index < max; index++)
            {
                var ch = s[index];
                if (!char.IsControl(ch))
                {
                    newStr[newIdx++] = ch;
                }
            }
            return new string(newStr, 0, newIdx);
        }

        public static string CapitalizeFirstLetter(this string s, CultureInfo ci = null)
        {
            var si = new StringInfo(s);
            if (ci == null)
            {
                ci = CultureInfo.CurrentCulture;
            }

            if (si.LengthInTextElements > 0)
            {
                s = si.SubstringByTextElements(0, 1).ToUpper(ci);
            }

            if (si.LengthInTextElements > 1)
            {
                s += si.SubstringByTextElements(1);
            }

            return s;
        }

        public static bool ContainsColor(this string s) => s?.IndexOf("<font", StringComparison.OrdinalIgnoreCase) >= 0;
    }
}
