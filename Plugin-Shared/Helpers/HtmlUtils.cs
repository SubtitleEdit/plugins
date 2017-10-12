using System;
using System.Drawing;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public static class HtmlUtils
    {
        public static string RemoveTags(string text, Tags tag = Tags.All)
        {
            if (tag == Tags.All)
            {
                int idx = text.IndexOf('<');
                while (idx >= 0)
                {
                    int endIdx = text.IndexOf('>', idx + 1);
                    // invalid tag
                    if (endIdx < idx)
                    {
                        break;
                    }
                    text = text.Remove(idx, endIdx - idx + 1);
                    idx = text.IndexOf('<', idx);
                }
                return text;
            }

            if ((tag & Tags.Font) == Tags.Font)
            {
            }
            if ((tag & Tags.Italic) == Tags.Italic)
            {
            }
            if ((tag & Tags.Bold) == Tags.Bold)
            {
            }
            if ((tag & Tags.Underline) == Tags.Underline)
            {
            }
            if ((tag & Tags.Strike) == Tags.Strike)
            {
            }
            //switch (Tags)
            //{
            //    case Tags.Font | Tags.Bold | Tags.Italic | Tags.Underline | 
            //        break;
            //    case Tags.Italic:
            //        break;
            //    case Tags.Bold:
            //        break;
            //    case Tags.Underline:
            //        break;
            //    case Tags.Strike:
            //        break;
            //    default:
            //        break;
            //}
            return text;
        }

        public static string RemoveAssTags(string s)
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
            return s;
        }

        public static string ColorToHtml(Color c) => $"#{c.R:x2}{c.G:x2}{ c.B:x2}";

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

                    var countStart = StringUtils.CountTagInText(s, "<font");
                    var countEnd = StringUtils.CountTagInText(s, "</font>");
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
                s = RemoveTags(s, Tags.Font);
            }
            return s.Trim();
        }

        public static string StripColorAnnotation(string fontTag)
        {
            //<font color="#008040" face="Niagara Solid">(Ivandro Ismael)</font>
            int colorStart = fontTag.IndexOf("color=\"", StringComparison.OrdinalIgnoreCase);
            if (colorStart < 0)
            {
                return fontTag;
            }
            int colorEnd = fontTag.IndexOf('\"', colorStart + 7);
            if (colorEnd < 0)
            {
                return fontTag;
            }
            fontTag = fontTag.Remove(colorStart, colorEnd - colorStart + 1);
            fontTag = fontTag.FixExtraSpaces();
            fontTag = fontTag.Replace("<font >", string.Empty);
            fontTag = fontTag.Replace("<font>", string.Empty);
            return fontTag;
        }

    }
}
