using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Linq;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public static class HtmlUtils
    {

        public const string TagItalic = "i";
        public const string TagBold = "b";
        public const string TagUnderline = "u";
        public const string TagParagraph = "p";
        public const string TagFont = "font";
        public const string TagCyrillicI = "\u0456"; // Cyrillic Small Letter Byelorussian-Ukrainian i (http://graphemica.com/%D1%96)

        private static readonly Regex TagOpenRegex = new Regex(@"<\s*(?:/\s*)?(\w+)[^>]*>", RegexOptions.Compiled);

        /// <summary>
        /// Remove all of the specified opening and closing tags from the source HTML string.
        /// </summary>
        /// <param name="source">The source string to search for specified HTML tags.</param>
        /// <param name="tags">The HTML tags to remove.</param>
        /// <returns>A new string without the specified opening and closing tags.</returns>
        public static string RemoveOpenCloseTags(string source, params string[] tags)
        {
            // This pattern matches these tag formats:
            // <tag*>
            // < tag*>
            // </tag*>
            // < /tag*>
            // </ tag*>
            // < / tag*>
            return TagOpenRegex.Replace(
                source,
                m => tags.Contains(m.Groups[1].Value, StringComparer.OrdinalIgnoreCase) ? string.Empty : m.Value);
        }

        public static string RemoveTags(string s, bool alsoSsaTags = false)
        {
            if (s == null || s.Length < 3)
            {
                return s;
            }

            if (alsoSsaTags)
            {
                s = StringUtils.RemoveSsaTags(s);
            }

            if (!s.Contains('<'))
            {
                return s;
            }

            return RemoveOpenCloseTags(s, TagItalic, TagBold, TagUnderline, TagParagraph, TagFont, TagCyrillicI);
        }

        public static bool IsUrl(string text)
        {
            if (string.IsNullOrWhiteSpace(text) || text.Length < 6 || !text.Contains('.') || text.Contains(' '))
            {
                return false;
            }

            var allLower = text.ToLower();
            if (allLower.StartsWith("http://", StringComparison.Ordinal) || allLower.StartsWith("https://", StringComparison.Ordinal) ||
                allLower.StartsWith("www.", StringComparison.Ordinal) || allLower.EndsWith(".org", StringComparison.Ordinal) ||
                allLower.EndsWith(".com", StringComparison.Ordinal) || allLower.EndsWith(".net", StringComparison.Ordinal))
            {
                return true;
            }

            if (allLower.Contains(".org/") || allLower.Contains(".com/") || allLower.Contains(".net/"))
            {
                return true;
            }

            return false;
        }

        public static bool StartsWithUrl(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            var arr = text.Trim().TrimEnd('.').TrimEnd().Split();
            if (arr.Length == 0)
            {
                return false;
            }

            return IsUrl(arr[0]);
        }

        private static readonly string[] UppercaseTags = { "<I>", "<U>", "<B>", "<FONT", "</I>", "</U>", "</B>", "</FONT>" };

        public static string FixUpperTags(string text)
        {
            if (string.IsNullOrEmpty(text) || !text.Contains('<'))
            {
                return text;
            }

            var idx = text.IndexOfAny(UppercaseTags, StringComparison.Ordinal);
            while (idx >= 0)
            {
                var endIdx = text.IndexOf('>', idx + 2);
                if (endIdx < idx)
                {
                    break;
                }

                var tag = text.Substring(idx, endIdx - idx).ToLowerInvariant();
                text = text.Remove(idx, endIdx - idx).Insert(idx, tag);
                idx = text.IndexOfAny(UppercaseTags, StringComparison.Ordinal);
            }
            return text;
        }

        public static string ColorToHtml(Color c) => $"#{c.R:x2}{c.G:x2}{ c.B:x2}".ToLowerInvariant();

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
                    {
                        break;
                    }

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
                        startIndex = s.IndexOf("<font", startIndex + 6, StringComparison.Ordinal);
                    }
                    // s.IndexOf("<font", StringComparison.OrdinalIgnoreCase);
                    startIndex = s.IndexOf("<font", startIndex + 8, StringComparison.Ordinal);
                }
            }
            else
            {
                s = RemoveOpenCloseTags(s, TagFont);
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
