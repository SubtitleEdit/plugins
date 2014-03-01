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

            s = s.Replace("<i>", string.Empty);
            s = s.Replace("</i>", string.Empty);
            s = s.Replace("<b>", string.Empty);
            s = s.Replace("</b>", string.Empty);
            s = s.Replace("<u>", string.Empty);
            s = s.Replace("</u>", string.Empty);
            s = s.Replace("<I>", string.Empty);
            s = s.Replace("</I>", string.Empty);
            s = s.Replace("<B>", string.Empty);
            s = s.Replace("</B>", string.Empty);
            s = s.Replace("<U>", string.Empty);
            s = s.Replace("</U>", string.Empty);
            s = RemoveParagraphTag(s);
            return RemoveHtmlFontTag(s);
        }

        internal static string RemoveHtmlFontTag(string s)
        {
            s = s.Replace("</font>", string.Empty);
            s = s.Replace("</FONT>", string.Empty);
            s = s.Replace("</Font>", string.Empty);
            s = s.Replace("<font>", string.Empty);
            s = s.Replace("<FONT>", string.Empty);
            s = s.Replace("<Font>", string.Empty);
            while (s.ToLower().Contains("<font"))
            {
                int startIndex = s.ToLower().IndexOf("<font");
                int endIndex = Math.Max(s.IndexOf(">"), startIndex + 4);
                s = s.Remove(startIndex, (endIndex - startIndex) + 1);
            }
            return s;
        }

        internal static string RemoveParagraphTag(string s)
        {
            s = s.Replace("</p>", string.Empty);
            s = s.Replace("</P>", string.Empty);
            s = s.Replace("<P>", string.Empty);
            s = s.Replace("<P>", string.Empty);
            while (s.ToLower().Contains("<p "))
            {
                int startIndex = s.ToLower().IndexOf("<p ");
                int endIndex = Math.Max(s.IndexOf(">"), startIndex + 4);
                s = s.Remove(startIndex, (endIndex - startIndex) + 1);
            }
            return s;
        }
     
        public static string AssemblyVersion
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }

        public static Regex MakeWordSearchRegex(string word)
        {
            string s = word.Replace("\\", "\\\\");
            s = s.Replace("*", "\\*");
            s = s.Replace(".", "\\.");
            s = s.Replace("?", "\\?");
            return new Regex(@"\b" + s + @"\b", RegexOptions.Compiled);
        }

        public static Regex MakeWordSearchEndRegex(string word)
        {
            string s = word.Replace("\\", "\\\\");
            s = s.Replace("*", "\\*");
            s = s.Replace(".", "\\.");
            s = s.Replace("?", "\\?");
            return new Regex(@"\b" + s, RegexOptions.Compiled);
        }

    }
}