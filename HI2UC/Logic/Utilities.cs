﻿using System;
using System.Drawing;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public static class Utilities
    {
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
            return (int.TryParse(s, out i));
        }

        public static string RemoveHtmlFontTag(string s)
        {
            s = Regex.Replace(s, "(?i)</?font>", string.Empty);
            return RemoveTag(s, "<font");
        }

        public static string RemoveHtmlTags(string s)
        {
            if (string.IsNullOrEmpty(s))
                return null;

            if (!s.Contains("<"))
                return s;
            s = Regex.Replace(s, "(?i)</?[uib]>", string.Empty);
            s = RemoveParagraphTag(s);
            return RemoveHtmlFontTag(s).Trim();
        }

        private static string RemoveTag(string text, string tag)
        {
            var idx = text.IndexOf(tag, StringComparison.OrdinalIgnoreCase);
            while (idx > -1)
            {
                var endIndex = text.IndexOf('>', idx + tag.Length);
                if (endIndex < 0) break;
                text = text.Remove(idx, (endIndex - idx) + 1);
                idx = text.IndexOf(tag, StringComparison.OrdinalIgnoreCase);
            }
            return text;
        }

        internal static string RemoveBrackets(string inputString)
        {
            string pattern = @"^[\[\(]|[\]\)]$";
            return Regex.Replace(inputString, pattern, string.Empty).Trim();
        }

        internal static string RemoveParagraphTag(string s)
        {
            s = Regex.Replace(s, "(?i)</?p>", string.Empty);
            return RemoveTag(s, "<p");
        }

        internal static string TryFixBrokenBrackets(string text, char bracketType, int nextIdx, Subtitle sub)
        {
            var before = text;
            var idx = -1;
            // More than one lines
            if (text.IndexOf(Environment.NewLine, StringComparison.Ordinal) > -1)
            {
                var lines = text.Replace("\r\n", "\n").Split('\n');
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