using System;
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
            return int.TryParse(s, out i);
        }

        public static string RemoveHtmlTags(string s)
        {
            if (string.IsNullOrEmpty(s))
                return string.Empty;
            if (s.IndexOf('<') < 0)
                return s;
            s = Regex.Replace(s, "(?i)</?[ibu]>", string.Empty);
            s = RemoveParagraphTag(s);
            while (s.Contains("  ")) s = s.Replace("  ", " ");
            return RemoveHtmlFontTag(s).Trim();
        }
        internal static string RemoveParagraphTag(string s)
        {
            s = Regex.Replace(s, "(?i)</?p>", string.Empty);
            var idx = s.IndexOf("<p", StringComparison.OrdinalIgnoreCase);
            while (idx >= 0)
            {
                var endIdx = s.IndexOf('>', idx + 1);
                if (endIdx < 1) break;
                s = s.Remove(idx, endIdx - idx + 1);
                idx = s.IndexOf("<p", StringComparison.OrdinalIgnoreCase);
            }
            return s;
        }

        public static string RemoveHtmlFontTag(string s)
        {
            s = Regex.Replace(s, "(?i)</?font>", string.Empty);
            var idx = s.IndexOf("<font", StringComparison.OrdinalIgnoreCase);
            while (idx >= 0)
            {
                var endIdx = s.IndexOf('>', idx + 1);
                if (endIdx < 0) break;
                s = s.Remove(idx, endIdx - idx + 1);
                idx = s.IndexOf("<font", StringComparison.OrdinalIgnoreCase);
            }
            return s;
        }

        internal static int NumberOfLines(string text)
        {
            if (text == null || text.Trim().Length == 0)
                return 0;
            var ln = 0;
            var idx = text.IndexOf('\n');
            while (idx > 0)
            {
                ln++;
                idx = text.IndexOf('\n', idx + 1);
            }
            return ln;
        }
    }
}