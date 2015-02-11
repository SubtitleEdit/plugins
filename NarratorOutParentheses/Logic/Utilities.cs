using System;
using System.Drawing;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.PluginLogic
{
    internal static class Utilities
    {
        internal static int NumberOfLines(string text)
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

        internal static string AssemblyVersion
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }

        internal static bool IsInteger(string s)
        {
            int i;
            return int.TryParse(s, out i);
        }

        internal static string RemoveHtmlFontTag(string s)
        {
            s = Regex.Replace(s, "(?i)</?font>", string.Empty);
            return RemoveTag(s, "<font");
        }

        internal static string RemoveHtmlTags(string s)
        {
            if (string.IsNullOrEmpty(s))
                return null;
            if (s.IndexOf('<') < 0)
                return s;
            s = Regex.Replace(s, "(?i)</?[uib]>", string.Empty);
            s = RemoveParagraphTag(s);
            return RemoveHtmlFontTag(s).Trim();
        }

        internal static string RemoveTag(string text, string tag)
        {
            var idx = text.IndexOf(tag, StringComparison.OrdinalIgnoreCase);
            while (idx > -1)
            {
                var endIndex = text.IndexOf('>', idx + tag.Length);
                if (endIndex < idx) break;
                text = text.Remove(idx, endIndex - idx + 1);
                idx = text.IndexOf(tag, StringComparison.OrdinalIgnoreCase);
            }
            return text;
        }

        internal static string RemoveBrackets(string inputString)
        {
            return Regex.Replace(inputString, @"^[\[\(]|[\]\)]$", string.Empty).Trim();
        }

        internal static string RemoveParagraphTag(string s)
        {
            s = Regex.Replace(s, "(?i)</?p>", string.Empty);
            return RemoveTag(s, "<p");
        }
    }
}