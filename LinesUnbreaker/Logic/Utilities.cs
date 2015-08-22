using System;
using System.Drawing;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public static class Utilities
    {
        #region Extension Methods

        public static string[] SplitToLines(this string s)
        {
            return s.Replace(Environment.NewLine, "\n").Replace('\r', '\n').Split('\n');
        }
        public static bool StartsWith(this string s, char c)
        {
            return s.Length > 0 && s[0] == c;
        }
        public static bool EndsWith(this string s, char c)
        {
            return s.Length > 0 && s[s.Length - 1] == c;
        }
        public static bool Contains(this string s, char c)
        {
            return s.Length > 0 && s.IndexOf(c) >= 0;
        }

        #endregion

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
            if (int.TryParse(s, out i))
                return true;
            return false;
        }

        public static string RemoveHtmlFontTag(string s)
        {
            s = Regex.Replace(s, "(?i)</?font>", string.Empty);
            while (s.ToLower().Contains("<font"))
            {
                int startIndex = s.ToLower().IndexOf("<font");
                int endIndex = Math.Max(s.IndexOf(">"), startIndex + 4);
                s = s.Remove(startIndex, (endIndex - startIndex) + 1);
            }
            return s;
        }

        public static string RemoveHtmlTags(string s)
        {
            if (string.IsNullOrEmpty(s))
                return null;
            if (!s.Contains("<"))
                return s;
            s = Regex.Replace(s, "(?i)</?[iіbu]>", string.Empty);
            s = RemoveParagraphTag(s);
            return RemoveHtmlFontTag(s).Trim();
        }

        public static string GetHtmlColorCode(Color color)
        {
            return string.Format("#{0:x2}{1:x2}{2:x2}", color.R, color.G, color.B);
        }

        public static string RemoveParagraphTag(string s)
        {
            s = Regex.Replace(s, "(?i)</?p>", string.Empty);
            while (s.ToLower().Contains("<p "))
            {
                int startIndex = s.ToLower().IndexOf("<p ");
                int endIndex = Math.Max(s.IndexOf(">"), startIndex + 4);
                s = s.Remove(startIndex, (endIndex - startIndex) + 1);
            }
            return s;
        }

        public static int GetNumberOfLines(string s)
        {
            if (string.IsNullOrEmpty(s))
                return 0;
            var totalLines = 1;
            var idx = s.IndexOf('\n');
            while (idx >= 0)
            {
                totalLines++;
                idx = s.IndexOf('\n', idx + 1);
            }
            return totalLines;
        }
    }
}