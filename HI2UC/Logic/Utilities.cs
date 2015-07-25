using System;
using System.Drawing;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.PluginLogic
{
    internal static class Utilities
    {
        #region ExtensionMethod

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
        #endregion

        public static int NumberOfLines(string text)
        {
            var ln = 0;
            var idx = -1;
            do
            {
                ln++;
                idx = text.IndexOf('\n', idx + 1);
            } while (idx > -1);
            return ln;
        }

        public static string AssemblyVersion
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

        public static string RemoveHtmlTags(string s, bool alsoSSA)
        {
            if (s == null || s.Length < 3 || !s.Contains(s))
                return s;
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
                    startIdx = s.IndexOf(ssaStart);
                }
            }
            var idx = s.IndexOf('<');
            while (idx >= 0)
            {
                var endIdx = s.IndexOf('>', idx + 1);
                if (endIdx < idx) break;
                s = s.Remove(idx, endIdx - idx + 1);
                idx = s.IndexOf('<', idx);
            }
            return s;
        }

        public static string TryFixBrokenBrackets(string text, char bracketType, int nextIdx, Subtitle sub)
        {
            var before = text;
            var idx = -1;
            // More than one lines
            if (text.IndexOf(Environment.NewLine, StringComparison.Ordinal) > -1)
            {
                var lines = text.SplitToLines();
                for (int i = 0; i < lines.Length; i++)
                {
                    CheckForBrackets(ref lines[i], bracketType, nextIdx, sub, ref idx);
                }
            }
            else
            {
                CheckForBrackets(ref text, bracketType, nextIdx, sub, ref idx);
            }
            return text;
        }

        private static void CheckForBrackets(ref string text, char bracketType, int nextIdx, Subtitle sub, ref int idx)
        {
            var doFix = false;
            for (int i = nextIdx; i < sub.Paragraphs.Count; i++)
            {
                var nextText = sub.Paragraphs[nextIdx].Text;
                idx = nextText.IndexOf('(');
                var close = nextText.IndexOf(')', idx);
                if (idx > -1 && close > idx)
                {
                    doFix = true;
                    break;
                }

                // do not keep looking... just exit
                if (nextIdx + 3 == i)
                    break;
            }

            if (doFix)
            {
                text = text + bracketType;
            }
        }
    }
}