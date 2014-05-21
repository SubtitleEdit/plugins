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
            s = Regex.Replace(s, "(?i)</?font>", string.Empty);
            while (s.ToLower().Contains("<font"))
            {
                int startIndex = s.ToLower().IndexOf("<font");
                int endIndex = Math.Max(s.IndexOf(">"), startIndex + 6);
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
            s = Regex.Replace(s, "(?i)</?[uib]>", string.Empty);
            s = RemoveParagraphTag(s);
            return RemoveHtmlFontTag(s).Trim();
        }

        internal static string GetHtmlColorCode(Color color)
        {
            return string.Format("#{0:x2}{1:x2}{2:x2}", color.R, color.G, color.B);
        }

        internal static string RemoveHtmlColorTags(string s)
        {
            if (!s.ToLower().Contains("<font "))
                return s;
            //<font color="#008040" face="Niagara Solid">(Ivandro Ismael)</font>
            bool removeEndTag = true;
            int startIndex = -1;
            int endIndex = -1;
            if (s.Contains("face=\""))
            {
                endIndex = s.IndexOf('>', startIndex + 1);
                //s.Substring(startIndex, (endIndex - startIndex) + 1).Contains("face=")
                removeEndTag = false;
                while (startIndex > -1)
                {
                    string coloredText = s.Substring(startIndex, (endIndex - startIndex) + 1);
                    string oldText = coloredText;
                    coloredText = ContainsFaceTags(s, out removeEndTag);
                    if (oldText != coloredText)
                    {
                        s = s.Remove(startIndex, (endIndex - startIndex) + 1);
                        s = s.Insert(startIndex, coloredText);
                        startIndex = s.IndexOf("<font ", startIndex);

                        if (removeEndTag)
                        {
                            int fontClose = s.IndexOf("</font>");
                            if (fontClose > -1)
                                s = s.Remove(fontClose, 8);
                        }
                    }
                    else
                    {
                        startIndex = s.IndexOf("<font ", startIndex + 8);
                    }
                }
            }
            else
            {
                while (s.ToLower().Contains("<font"))
                {
                    startIndex = s.ToLower().IndexOf("<font");
                    endIndex = s.IndexOf('>', startIndex + 6);
                    if (endIndex > startIndex)
                    {
                        s = s.Remove(startIndex, (endIndex - startIndex) + 1);
                        s = s.Replace("  ", " ").Trim();
                    }
                }
            }

            if (removeEndTag)
            {
                s = Regex.Replace(s, "(?i)</?font>", string.Empty, RegexOptions.Compiled);
            }
            return s.Trim();
        }

        private static string ContainsFaceTags(string s, out bool removeEndTag)
        {
            removeEndTag = false;
            int colorStart = s.IndexOf("color=\"");
            if (colorStart < 0)
            {
                return s;
            }
            int colorEnd = s.IndexOf('\"', colorStart + 8);
            s = s.Remove(colorEnd, (colorEnd - colorStart) + 1);
            s = s.Replace("  ", "_@_");
            s = s.Replace(" _@_ ", " ");
            s = s.Replace("_@_ ", " ");
            s = s.Replace(" _@_", " ");
            s = s.Replace("_@_", " ");
            // TODO: Do this code in a loop 'cause line may contain more than 1 font tag
            s = s.Replace("<font >", string.Empty);
            s = s.Replace("<font>", string.Empty);

            int index = s.IndexOf('<');
            if (index > -1 && s[index + 1] == '/')
            {
                // <font> is removed and </font> remains there
                int closeIndex = s.IndexOf('>', index);
                if (closeIndex > index)
                    s = s.Remove(index, (closeIndex - index) + 1);
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
    }
}