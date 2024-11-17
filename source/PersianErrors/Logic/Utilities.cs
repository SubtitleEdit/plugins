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
            if (int.TryParse(s, out i))
                return true;
            return false;
        }

        public static string RemoveHtmlFontTag(string s)
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

        public static string RemoveHtmlTags(string s)
        {
            if (string.IsNullOrEmpty(s))
                return null;
            //if (s == null)
            //    return null;
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
            return RemoveHtmlFontTag(s).Trim();
        }

        internal static string GetHtmlColorCode(Color color)
        {
            return string.Format("#{0:x2}{1:x2}{2:x2}", color.R, color.G, color.B);
        }

        internal static string RemoveBrackets(string inputString)
        {
            string pattern = @"^[\[\{\(]|[\]\}\)]$";
            return Regex.Replace(inputString, pattern, string.Empty).Trim();
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
    }
}