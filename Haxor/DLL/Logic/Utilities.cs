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

        public static string RemoveHtmlTags(string s, bool alsoSSA = false)
        {
            if (string.IsNullOrEmpty(s))
                return string.Empty;

            if (alsoSSA)
                s = RemoveSsaTags(s);

            if (s.IndexOf('<') < 0)
                return s;
            s = Regex.Replace(s, "(?i)</?[ibu]>", string.Empty);
            s = RemoveParagraphTag(s);
            while (s.Contains("  ")) s = s.Replace("  ", " ");
            return RemoveHtmlFontTag(s).Trim();
        }

        public static string RemoveSsaTags(string s)
        {
            int k = s.IndexOf('{');
            while (k >= 0)
            {
                int l = s.IndexOf('}', k);
                if (l > k)
                {
                    s = s.Remove(k, l - k + 1);
                    if (s.Length > 1 && s.Length > k)
                        k = s.IndexOf('{', k);
                    else
                        break;
                }
                else
                {
                    break;
                }
            }
            return s;
        }

        internal static string RemoveParagraphTag(string s)
        {
            s = Regex.Replace(s, "(?i)</?p>", string.Empty);
            var idx = s.IndexOf("<p", StringComparison.OrdinalIgnoreCase);
            while (idx >= 0)
            {
                var endIdx = s.IndexOf('>', idx + 2);
                if (endIdx < idx) break;
                s = s.Remove(idx, endIdx - idx + 1);
                idx = s.IndexOf("<p", idx, StringComparison.OrdinalIgnoreCase);
            }
            return s;
        }

        public static string RemoveHtmlFontTag(string s)
        {
            s = Regex.Replace(s, "(?i)</?font>", string.Empty);
            var idx = s.IndexOf("<font", StringComparison.OrdinalIgnoreCase);
            while (idx >= 0)
            {
                var endIdx = s.IndexOf('>', idx + 5);
                if (endIdx < idx) break;
                s = s.Remove(idx, endIdx - idx + 1);
                idx = s.IndexOf("<font", idx, StringComparison.OrdinalIgnoreCase);
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