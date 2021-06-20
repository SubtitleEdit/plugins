using System;
using System.Reflection;
using System.Text.RegularExpressions;

namespace SubtitleEdit.Logic
{
    public static class Utilities
    {
        public static readonly string UppercaseLetters = "ABCDEFGHIJKLMNOPQRSTUVWZYXÆØÃÅÄÖÉÈÁÂÀÇÊÍÓÔÕÚŁАБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯĞİŞÜÙÁÌÑÎ";
        public static readonly string LowercaseLetters = UppercaseLetters.ToLowerInvariant();
        public static readonly string LowercaseLettersWithNumbers = LowercaseLetters + "0123456789";
        public static readonly string AllLetters = UppercaseLetters + LowercaseLetters;
        public static readonly string AllLettersAndNumbers = UppercaseLetters + LowercaseLettersWithNumbers;


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

            if (!s.Contains('<'))
                return s;
            s = Regex.Replace(s, "(?i)</?[ibu]>", string.Empty);
            while (s.Contains("  ")) s = s.Replace("  ", " ");
            return RemoveHtmlFontTag(s).Trim();
        }

        public static string RemoveSsaTags(string s)
        {
            const string tag = "{\\";
            var idx = s.IndexOf(tag, StringComparison.Ordinal);
            while (idx >= 0)
            {
                var endIdx = s.IndexOf('}');
                if (endIdx < idx)
                    break;
                s = s.Remove(idx, endIdx - idx + 1);
                idx = s.IndexOf(tag, StringComparison.Ordinal);
            }
            return s;
        }

        public const double SubtitleOptimalCharactersPerSeconds = 15.0;
        public const int SubtitleMinimumDisplayMilliseconds = 1000;
        public const int SubtitleMaximumDisplayMilliseconds = 8_000;

        internal static double GetOptimalDisplayMilliseconds(string text)
        {
            return GetOptimalDisplayMilliseconds(text, SubtitleOptimalCharactersPerSeconds);
        }

        public static double GetOptimalDisplayMilliseconds(string text, double optimalCharactersPerSecond)
        {
            if (optimalCharactersPerSecond < 2 || optimalCharactersPerSecond > 100)
            {
                optimalCharactersPerSecond = 14.7;
            }

            var duration = text.CountCharacters(false, true) / optimalCharactersPerSecond * 1000.0;

            if (duration < 1400)
            {
                duration *= 1.2;
            }
            else if (duration < 1400 * 1.2)
            {
                duration = 1400 * 1.2;
            }
            else if (duration > 2900)
            {
                duration = Math.Max(2900, duration * 0.96);
            }

            if (duration < SubtitleMinimumDisplayMilliseconds)
            {
                duration = SubtitleMinimumDisplayMilliseconds;
            }

            if (duration > SubtitleMaximumDisplayMilliseconds)
            {
                duration = SubtitleMaximumDisplayMilliseconds;
            }

            return duration;
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

        internal static int GetNumberOfLines(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return 0;
            }

            int lines = 1;
            int idx = text.IndexOf('\n');
            while (idx >= 0)
            {
                lines++;
                idx = text.IndexOf('\n', idx + 1);
            }
            return lines;
        }

        internal static unsafe int NumberOfLines(string text)
        {
            var ln = 1;
            fixed (char* tPtr = text)
            {
                char* ptr = tPtr;
                while (*ptr != '\0')
                {
                    if (*ptr == '\n')
                        ln++;
                    ptr++;
                }
            }
            return ln;
        }

        internal static int CountTagInText(string text, string tag)
        {
            int count = 0;
            int index = text.IndexOf(tag, StringComparison.Ordinal);
            while (index >= 0)
            {
                count++;
                index = index + tag.Length;
                if (index >= text.Length)
                {
                    return count;
                }

                index = text.IndexOf(tag, index, StringComparison.Ordinal);
            }
            return count;
        }

        internal static bool StartsAndEndsWithTag(string text, string startTag, string endTag)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            if (!text.Contains(startTag) || !text.Contains(endTag))
            {
                return false;
            }

            while (text.Contains("  "))
            {
                text = text.Replace("  ", " ");
            }

            var s1 = "- " + startTag;
            var s2 = "-" + startTag;
            var s3 = "- ..." + startTag;
            var s4 = "- " + startTag + "..."; // - <i>...

            var e1 = endTag + ".";
            var e2 = endTag + "!";
            var e3 = endTag + "?";
            var e4 = endTag + "...";
            var e5 = endTag + "-";

            bool isStart = false;
            bool isEnd = false;
            if (text.StartsWith(startTag, StringComparison.Ordinal) || text.StartsWith(s1, StringComparison.Ordinal) || text.StartsWith(s2, StringComparison.Ordinal) || text.StartsWith(s3, StringComparison.Ordinal) || text.StartsWith(s4, StringComparison.Ordinal))
            {
                isStart = true;
            }

            if (text.EndsWith(endTag, StringComparison.Ordinal) || text.EndsWith(e1, StringComparison.Ordinal) || text.EndsWith(e2, StringComparison.Ordinal) || text.EndsWith(e3, StringComparison.Ordinal) || text.EndsWith(e4, StringComparison.Ordinal) || text.EndsWith(e5, StringComparison.Ordinal))
            {
                isEnd = true;
            }

            return isStart && isEnd;
        }

        public static bool IsBetweenNumbers(string s, int position)
        {
            if (string.IsNullOrEmpty(s) || position < 1 || position + 2 > s.Length)
            {
                return false;
            }

            return char.IsDigit(s[position - 1]) && char.IsDigit(s[position + 1]);
        }

        public static string RemoveSpaceBeforeAfterTag(string input, string openTag)
        {
            var text = HtmlUtil.FixUpperTags(input);
            var closeTag = string.Empty;
            switch (openTag)
            {
                case "<i>":
                    closeTag = "</i>";
                    break;
                case "<b>":
                    closeTag = "</b>";
                    break;
                case "<u>":
                    closeTag = "</u>";
                    break;
            }

            if (closeTag.Length == 0 && openTag.Contains("<font ", StringComparison.Ordinal))
            {
                closeTag = "</font>";
            }

            // Open tags
            var open1 = openTag + " ";
            var open2 = Environment.NewLine + openTag + " ";
            var open3 = openTag + Environment.NewLine;

            // Closing tags
            var close1 = "! " + closeTag + Environment.NewLine;
            var close2 = "? " + closeTag + Environment.NewLine;
            var close3 = " " + closeTag;
            var close4 = " " + closeTag + Environment.NewLine;
            var close5 = Environment.NewLine + closeTag;

            if (text.Contains(close1, StringComparison.Ordinal))
            {
                text = text.Replace(close1, "!" + closeTag + Environment.NewLine);
            }

            if (text.Contains(close2, StringComparison.Ordinal))
            {
                text = text.Replace(close2, "?" + closeTag + Environment.NewLine);
            }

            if (text.EndsWith(close3, StringComparison.Ordinal))
            {
                text = text.Substring(0, text.Length - close3.Length) + closeTag;
            }

            if (text.Contains(close4))
            {
                text = text.Replace(close4, closeTag + Environment.NewLine);
            }

            // e.g: ! </i><br>Foobar
            if (text.StartsWith(open1, StringComparison.Ordinal))
            {
                text = openTag + text.Substring(open1.Length);
            }

            // e.g.: <i>\r\n
            if (text.StartsWith(open3, StringComparison.Ordinal))
            {
                text = text.Remove(openTag.Length, Environment.NewLine.Length);
            }

            // e.g.: \r\n</i>
            if (text.EndsWith(close5, StringComparison.Ordinal))
            {
                text = text.Remove(text.Length - openTag.Length - Environment.NewLine.Length - 1, Environment.NewLine.Length);
            }

            if (text.Contains(open2, StringComparison.Ordinal))
            {
                text = text.Replace(open2, Environment.NewLine + openTag);
            }

            // Hi <i> bad</i> man! -> Hi <i>bad</i> man!
            text = text.Replace(" " + openTag + " ", " " + openTag);
            text = text.Replace(Environment.NewLine + openTag + " ", Environment.NewLine + openTag);

            // Hi <i>bad </i> man! -> Hi <i>bad</i> man!
            text = text.Replace(" " + closeTag + " ", closeTag + " ");
            text = text.Replace(" " + closeTag + Environment.NewLine, closeTag + Environment.NewLine);

            text = text.Trim();
            if (text.StartsWith(open1, StringComparison.Ordinal))
            {
                text = openTag + text.Substring(open1.Length);
            }

            return text;
        }
    }
}