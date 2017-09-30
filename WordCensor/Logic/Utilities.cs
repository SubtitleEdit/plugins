using System;
using System.Drawing;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.PluginLogic
{
    internal static class Utilities
    {
        public static string AssemblyVersion => Assembly.GetExecutingAssembly().GetName().Version.ToString();
        private static readonly Regex _regexHtmlEngFontTag = new Regex("</?font>", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static bool IsInteger(string s) => int.TryParse(s, out int i);

        public static string RemoveHtmlFontTag(string s)
        {
            s = _regexHtmlEngFontTag.Replace(s, m => string.Empty);
            var idx = s.IndexOf("<font", StringComparison.OrdinalIgnoreCase);
            while (idx >= 0)
            {
                var endIdx = s.IndexOf('>', idx + 5);
                if (endIdx < idx)
                    break;
                s = s.Remove(idx, endIdx - idx + 1);
                idx = s.IndexOf("<font", idx, StringComparison.OrdinalIgnoreCase);
            }
            return s.Replace("  ", " ");
        }

        public static string RemoveHtmlTags(string s, bool alsoSsa = false)
        {
            if (string.IsNullOrEmpty(s))
                return null;

            if (alsoSsa)
            {
                var idx = s.IndexOf('{');
                while (idx >= 0)
                {
                    var endIdx = s.IndexOf('}', idx + 1);
                    if (endIdx < 0)
                        break;
                    s = s.Remove(idx, endIdx - idx + 1);
                    idx = s.IndexOf('{', idx);
                }
            }

            if (!s.Contains('<'))
                return s;
            s = Regex.Replace(s, "(?i)</?[uib]>", string.Empty);
            return RemoveHtmlFontTag(s).Trim();
        }

        public static string GetHtmlColorCode(Color c) => $"#{ c.R:x2}{ c.G:x2}{ c.B:x2}";

        public static string RemoveHtmlColorTags(string s)
        {
            // Todo: Unfinished
            //<font color="#008040" face="Niagara Solid">(Ivandro Ismael)</font>
            int startIndex = s.IndexOf("<font", StringComparison.OrdinalIgnoreCase);
            if (startIndex >= 0 && s.IndexOf("face=\"", StringComparison.OrdinalIgnoreCase) > 5)
            {
                while (startIndex >= 0)
                {
                    var endIndex = s.IndexOf('>', startIndex + 5);
                    if (endIndex < startIndex)
                        break;
                    var fontTag = s.Substring(startIndex, endIndex - startIndex + 1);
                    fontTag = StripColorAnnotation(fontTag);

                    var countStart = CountTagInText(s, "<font", StringComparison.InvariantCultureIgnoreCase);
                    var countEnd = CountTagInText(s, "</font>", StringComparison.InvariantCultureIgnoreCase);
                    var mod = countStart + countEnd % 2;
                    if (mod != 0 && countEnd > countStart)
                    {
                        while (--mod >= 0)
                        {
                            var idx = s.IndexOf("</font>", StringComparison.InvariantCultureIgnoreCase);
                            s = s.Remove(idx, 7);
                        }
                    }

                    if (fontTag.Length > 6)
                    {
                        s = s.Remove(startIndex, endIndex - startIndex + 1);
                        s = s.Insert(startIndex, fontTag);
                        startIndex = s.IndexOf("<font", startIndex + 6);
                    }
                    // s.IndexOf("<font", StringComparison.OrdinalIgnoreCase);
                    startIndex = s.IndexOf("<font", startIndex + 8);
                }
            }
            else
            {
                s = RemoveHtmlFontTag(s);
            }
            return s.Trim();
        }

        public static string StripColorAnnotation(string fontTag)
        {
            //<font color="#008040" face="Niagara Solid">(Ivandro Ismael)</font>
            int colorStart = fontTag.IndexOf("color=\"", StringComparison.OrdinalIgnoreCase);
            if (colorStart < 0)
                return fontTag;
            int colorEnd = fontTag.IndexOf('\"', colorStart + 7);
            if (colorEnd < 0)
                return fontTag;
            fontTag = fontTag.Remove(colorStart, colorEnd - colorStart + 1);
            while (fontTag.Contains("  "))
                fontTag = fontTag.Replace("  ", " ");
            fontTag = fontTag.Replace("<font >", string.Empty);
            fontTag = fontTag.Replace("<font>", string.Empty);

            return fontTag;
        }

        public static int CountTagInText(string text, string tag) => CountTagInText(text, tag, StringComparison.Ordinal);

        public static int CountTagInText(string text, string tag, StringComparison comparison)
        {
            var count = 0;
            var idx = text.IndexOf(tag, comparison);
            while (idx >= 0)
            {
                count++;
                idx = text.IndexOf(tag, idx + tag.Length, comparison);
            }
            return count;
        }
    }
}