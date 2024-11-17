using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public static class Utilities
    {

        internal static readonly char[] NewLineChars = Environment.NewLine.ToCharArray();

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

        public static bool Contains(this string source, string value, StringComparison comparisonType)
        {
            return source.IndexOf(value, comparisonType) >= 0;
        }

        internal static string AssemblyVersion
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }

        public static bool IsInteger(string s)
        {
            int i;
            return int.TryParse(s, out i);
        }

        private static bool IsPartOfNumber(string s, int position)
        {
            if (string.IsNullOrWhiteSpace(s))
                return false;

            if (position + 2 > s.Length)
                return false;

            if (@",.".Contains(s[position].ToString()))
            {
                if (position > 0 && position < s.Length - 1)
                {
                    return char.IsDigit(s[position - 1]) && char.IsDigit(s[position + 1]);
                }
            }
            return false;
        }
    }
}