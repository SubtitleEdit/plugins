using System;
using System.Reflection;

namespace Nikse.SubtitleEdit.PluginLogic
{
    internal static class Utilities
    {
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
            if (s == null || s.Length < 3)
            {
                return s;
            }
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