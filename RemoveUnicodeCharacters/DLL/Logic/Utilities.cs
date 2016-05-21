using System;
using System.Reflection;

namespace SubtitleEdit.Logic
{
    public static class Utilities
    {
        #region ExtensionMethods

        public static string[] SplitToLines(this string source)
        {
            return source.Replace("\r\n", "\n").Replace('\r', '\n').Replace('\u2028', '\n').Split('\n');
        }

        public static bool Contains(this string s, char c)
        {
            return s.Length > 0 && s.IndexOf(c) >= 0;
        }

        public static bool StartsWith(this string s, char c)
        {
            return s.Length > 0 && s[0] == c;
        }

        public static bool EndsWith(this string s, char c)
        {
            return s.Length > 0 && s[s.Length - 1] == c;
        }

        #endregion

        public static bool IsInteger(string s)
        {
            int i;
            return int.TryParse(s, out i);
        }

        public static int CountTagInText(string text, char tag)
        {
            if (text == null)
                return 0;

            return text.Length - text.Replace(tag.ToString(), string.Empty).Length;
        }

        public static string RemoveHtmlTags(string s, bool alsoSSA = false)
        {
            if (string.IsNullOrEmpty(s) && !s.Contains('<'))
                return s;

            // {\an7}
            if (alsoSSA)
            {
                const string ssaStart = "{\\";
                var startIdx = s.IndexOf(ssaStart, StringComparison.Ordinal);
                while (startIdx >= 0)
                {
                    var endIdx = s.IndexOf('}', startIdx + 2);
                    if (endIdx < startIdx) // Invalid SSA
                        break;
                    s = s.Remove(startIdx, endIdx - startIdx + 1);
                    startIdx = s.IndexOf(ssaStart, startIdx);
                }
            }

            // <i>, <b>, <p>...
            var idx = s.IndexOf('<');
            while (idx >= 0)
            {
                var endIdx = s.IndexOf('>', idx + 1);
                if (endIdx < idx) break;
                s = s.Remove(idx, endIdx - idx + 1);
                idx = s.IndexOf('<', idx);
            }
            return s.Trim();
        }

        public static string AssemblyVersion => Assembly.GetExecutingAssembly().GetName().Version.ToString();

        public static int GetNumberOfLines(string text)
        {
            if (string.IsNullOrEmpty(text))
                return 0;

            int lines = 1;
            int idx = text.IndexOf('\n');
            while (idx >= 0)
            {
                lines++;
                idx = text.IndexOf('\n', idx + 1);
            }
            return lines;
        }
    }
}