using System;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public static class Utilities
    {
        public static bool IsInteger(string s)
        {
            int i;
            if (int.TryParse(s, out i))
                return true;
            return false;
        }

        public static string RemoveHtmlTags(string s)
        {
            if (s == null)
                return null;

            if (!s.Contains("<"))
                return s;

            s = Regex.Replace(s, "(?i)</?[ubi]>", string.Empty);
            s = RemoveParagraphTag(s);
            return RemoveHtmlFontTag(s);
        }

        internal static string RemoveHtmlFontTag(string s)
        {
            s = Regex.Replace(s, "(?i)</?font>", string.Empty);
            while (s.ToLower().Contains("<font"))
            {
                int startIndex = s.ToLower().IndexOf("<font");
                int endIndex = s.IndexOf(">", startIndex);
                if (endIndex > -1)
                    s = s.Remove(startIndex, (endIndex - startIndex) + 1);
                else
                    break;
            }
            return s;
        }

        internal static string RemoveParagraphTag(string s)
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

        public static string AssemblyVersion { get { return Assembly.GetExecutingAssembly().GetName().Version.ToString(); } }

        public static Regex MakeWordSearchRegex(string word)
        {
            word = Regex.Escape(word);
            return new Regex(@"\b" + word + @"\b", RegexOptions.Compiled);
        }

        public static Regex MakeWordSearchEndRegex(string word)
        {
            word = Regex.Escape(word);
            return new Regex(@"\b" + word, RegexOptions.Compiled);
        }
    }
}