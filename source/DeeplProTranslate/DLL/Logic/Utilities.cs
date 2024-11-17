using SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public static class Utilities
    {

        public static readonly string UppercaseLetters = "ABCDEFGHIJKLMNOPQRSTUVWZYXÆØÃÅÄÖÉÈÁÂÀÇÊÍÓÔÕÚŁАБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯĞİŞÜÙÁÌÑÎ";
        public static readonly string LowercaseLetters = UppercaseLetters.ToLower();
        public static readonly string LowercaseLettersWithNumbers = LowercaseLetters + "0123456789";
        public static readonly string AllLetters = UppercaseLetters + LowercaseLetters;
        public static readonly string AllLettersAndNumbers = UppercaseLetters + LowercaseLettersWithNumbers;

        #region StringExtension
        public static bool Contains(this string s, char c)
        {
            return s.Length > 0 && s.IndexOf(c) >= 0;
        }

        public static string[] SplitToLines(this string s)
        {
            return s.Replace(Environment.NewLine, "\n").Replace('\r', '\n').Split('\n');
        }
        #endregion
        internal static string AssemblyVersion => Assembly.GetExecutingAssembly().GetName().Version.ToString();

        public static bool IsInteger(string s)
        {
            return int.TryParse(s, out _);
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

        public static int CountTagInText(string text, string tag)
        {
            var count = 0;
            var index = text.IndexOf(tag, StringComparison.Ordinal);
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

        public static int GetNumberOfLines(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return 0;
            }

            var lines = 1;
            var idx = text.IndexOf('\n');
            while (idx >= 0)
            {
                lines++;
                idx = text.IndexOf('\n', idx + 1);
            }
            return lines;
        }

        public static bool StartsAndEndsWithTag(string text, string startTag, string endTag)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;
            if (!text.Contains(startTag) || !text.Contains(endTag))
                return false;

            while (text.Contains("  "))
                text = text.Replace("  ", " ");

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
                isStart = true;
            if (text.EndsWith(endTag, StringComparison.Ordinal) || text.EndsWith(e1, StringComparison.Ordinal) || text.EndsWith(e2, StringComparison.Ordinal) || text.EndsWith(e3, StringComparison.Ordinal) || text.EndsWith(e4, StringComparison.Ordinal) || text.EndsWith(e5, StringComparison.Ordinal))
                isEnd = true;
            return isStart && isEnd;
        }

        public static bool IsBetweenNumbers(string s, int position)
        {
            if (string.IsNullOrEmpty(s) || position < 1 || position + 2 > s.Length)
                return false;
            return char.IsDigit(s[position - 1]) && char.IsDigit(s[position + 1]);
        }

        public static string RemoveSpaceBeforeAfterTag(string text, string openTag)
        {
            text = HtmlUtil.FixUpperTags(text);
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
                closeTag = "</font>";

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
                text = text.Replace(close1, "!" + closeTag + Environment.NewLine);

            if (text.Contains(close2, StringComparison.Ordinal))
                text = text.Replace(close2, "?" + closeTag + Environment.NewLine);

            if (text.EndsWith(close3, StringComparison.Ordinal))
                text = text.Substring(0, text.Length - close3.Length) + closeTag;

            if (text.Contains(close4))
                text = text.Replace(close4, closeTag + Environment.NewLine);

            // e.g: ! </i><br>Foobar
            if (text.StartsWith(open1, StringComparison.Ordinal))
                text = openTag + text.Substring(open1.Length);

            // e.g.: <i>\r\n
            if (text.StartsWith(open3, StringComparison.Ordinal))
                text = text.Remove(openTag.Length, Environment.NewLine.Length);

            // e.g.: \r\n</i>
            if (text.EndsWith(close5, StringComparison.Ordinal))
                text = text.Remove(text.Length - openTag.Length - Environment.NewLine.Length - 1, Environment.NewLine.Length);

            if (text.Contains(open2, StringComparison.Ordinal))
                text = text.Replace(open2, Environment.NewLine + openTag);

            // Hi <i> bad</i> man! -> Hi <i>bad</i> man!
            text = text.Replace(" " + openTag + " ", " " + openTag);
            text = text.Replace(Environment.NewLine + openTag + " ", Environment.NewLine + openTag);

            // Hi <i>bad </i> man! -> Hi <i>bad</i> man!
            text = text.Replace(" " + closeTag + " ", closeTag + " ");
            text = text.Replace(" " + closeTag + Environment.NewLine, closeTag + Environment.NewLine);

            text = text.Trim();
            if (text.StartsWith(open1, StringComparison.Ordinal))
                text = openTag + text.Substring(open1.Length);

            return text;
        }

        public static string RemoveUnicodeControlChars(string input)
        {
            return input.Replace("\u200E", string.Empty)
                .Replace("\u200F", string.Empty)
                .Replace("\u202A", string.Empty)
                .Replace("\u202B", string.Empty)
                .Replace("\u202C", string.Empty)
                .Replace("\u202D", string.Empty)
                .Replace("\u202E", string.Empty)
                .Replace("\u00A0", " "); // no break space
        }

        public static string AutoBreakLine(string text, int maximumLength, int mergeLinesShorterThan, string language)
        {
            return AutoBreakLinePrivate(text, maximumLength, mergeLinesShorterThan, language, false);
        }

        public static string AutoBreakLine(string text, string language, bool autoBreakLineEndingEarly)
        {
            return AutoBreakLinePrivate(text, 43, 30, language, autoBreakLineEndingEarly);
        }

        public static string AutoBreakLinePrivate(string input, int maximumLength, int mergeLinesShorterThan, string language, bool autoBreakLineEndingEarly)
        {
            if (string.IsNullOrEmpty(input) || input.Length < 3)
            {
                return input;
            }

            var text = input.Replace('\u00a0', ' '); // replace non-break-space (160 decimal) ascii char with normal space
            if (!(text.IndexOf(' ') >= 0 || text.IndexOf('\n') >= 0))
            {
                if (new[] { "zh", "ja", "ko" }.Contains(language) == false)
                {
                    return input;
                }
            }

            // do not auto break dialogs or music symbol
            if (text.Contains(Environment.NewLine) && (text.IndexOf('-') >= 0 || text.IndexOf('♪') >= 0))
            {
                var sanitizedLines = RemoveUnicodeControlChars(HtmlUtil.RemoveHtmlTags(text, true)).SplitToLines();
                if (sanitizedLines.Length == 2)
                {
                    var arr0 = sanitizedLines[0].Trim().TrimEnd('"', '\'').TrimEnd();
                    if (language == "ar")
                    {
                        if (arr0.EndsWith('-') && sanitizedLines[1].TrimStart().EndsWith('-') && arr0.Length > 1 && (".?!)]♪؟".Contains(arr0[0]) || arr0.StartsWith("--", StringComparison.Ordinal) || arr0.StartsWith('–')))
                        {
                            return input;
                        }
                    }
                    else
                    {
                        if (arr0.StartsWith('-') && sanitizedLines[1].TrimStart().StartsWith('-') && arr0.Length > 1 && (".?!)]♪؟".Contains(arr0[arr0.Length - 1]) || arr0.EndsWith("--", StringComparison.Ordinal) || arr0.EndsWith('–') || arr0 == "- _" || arr0 == "-_"))
                        {
                            return input;
                        }
                    }
                    if (sanitizedLines[0].StartsWith('♪') && sanitizedLines[0].EndsWith('♪') || sanitizedLines[1].StartsWith('♪') && sanitizedLines[0].EndsWith('♪'))
                    {
                        return input;
                    }
                    if (sanitizedLines[0].StartsWith('[') && sanitizedLines[0].Length > 1 && (".?!)]♪؟".Contains(arr0[arr0.Length - 1]) && (sanitizedLines[1].StartsWith('-') || sanitizedLines[1].StartsWith('['))))
                    {
                        return input;
                    }
                    if (sanitizedLines[0].StartsWith('-') && sanitizedLines[0].Length > 1 && (".?!)]♪؟".Contains(arr0[arr0.Length - 1]) && (sanitizedLines[1].StartsWith('-') || sanitizedLines[1].StartsWith('['))))
                    {
                        if (true)
                        {
                            return input;
                        }
                    }
                }
            }

            var s = RemoveLineBreaks(text);
            if (s.Length < mergeLinesShorterThan)
            {
                var lastIndexOfDash = s.LastIndexOf(" -", StringComparison.Ordinal);
                if (lastIndexOfDash > 4 && s.Substring(0, lastIndexOfDash).HasSentenceEnding(language))
                {
                    s = s.Remove(lastIndexOfDash, 1).Insert(lastIndexOfDash, Environment.NewLine);
                }

                return s;
            }

            var htmlTags = new Dictionary<int, string>();
            var sb = new StringBuilder();
            var six = 0;
            while (six < s.Length)
            {
                var letter = s[six];
                var tagFound = false;
                if (letter == '<')
                {
                    var tagString = s.Substring(six);
                    tagFound = tagString.StartsWith("<font", StringComparison.OrdinalIgnoreCase)
                            || tagString.StartsWith("</font", StringComparison.OrdinalIgnoreCase)
                            || tagString.StartsWith("<u", StringComparison.OrdinalIgnoreCase)
                            || tagString.StartsWith("</u", StringComparison.OrdinalIgnoreCase)
                            || tagString.StartsWith("<b", StringComparison.OrdinalIgnoreCase)
                            || tagString.StartsWith("</b", StringComparison.OrdinalIgnoreCase)
                            || tagString.StartsWith("<i", StringComparison.OrdinalIgnoreCase)
                            || tagString.StartsWith("</i", StringComparison.OrdinalIgnoreCase);
                }
                else if (letter == '{' && s.Substring(six).StartsWith("{\\"))
                {
                    var tagString = s.Substring(six);
                    var endIndexAssTag = tagString.IndexOf('}') + 1;
                    if (endIndexAssTag > 0)
                    {
                        tagString = tagString.Substring(0, endIndexAssTag);
                        if (htmlTags.ContainsKey(six))
                        {
                            htmlTags[six] = htmlTags[six] + tagString;
                        }
                        else
                        {
                            htmlTags.Add(six, tagString);
                        }

                        s = s.Remove(six, endIndexAssTag);
                        continue;
                    }
                }

                var endIndex = -1;
                if (tagFound)
                {
                    endIndex = s.IndexOf('>', six + 1);
                }

                if (tagFound && endIndex > 0)
                {
                    var tag = s.Substring(six, endIndex - six + 1);
                    s = s.Remove(six, tag.Length);
                    if (htmlTags.ContainsKey(six))
                    {
                        htmlTags[six] += tag;
                    }
                    else
                    {
                        htmlTags.Add(six, tag);
                    }
                }
                else
                {
                    sb.Append(letter);
                    six++;
                }
            }
            s = sb.ToString();

            var textSplit = new TextSplit(s, maximumLength, language);
            var split = textSplit.AutoBreak(true, autoBreakLineEndingEarly, false, true);
            if (split != null)
            {
                s = split;
            }
            s = ReInsertHtmlTags(s.Replace(Environment.NewLine, " " + Environment.NewLine), htmlTags);
            var idx = s.IndexOf(Environment.NewLine + "</", StringComparison.Ordinal);
            if (idx > 2)
            {
                var endIdx = s.IndexOf('>', idx + 2);
                if (endIdx > idx)
                {
                    var tag = s.Substring(idx + Environment.NewLine.Length, endIdx - (idx + Environment.NewLine.Length) + 1);
                    s = s.Insert(idx, tag);
                    s = s.Remove(idx + tag.Length + Environment.NewLine.Length, tag.Length);
                }
            }
            s = s.Replace(" " + Environment.NewLine, Environment.NewLine);
            s = s.Replace(Environment.NewLine + " ", Environment.NewLine);
            return s.TrimEnd();
        }

        public static bool HasSentenceEnding(this string value, string twoLetterLanguageCode)
        {
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            var s = HtmlUtil.RemoveHtmlTags(value, true).TrimEnd('"').TrimEnd('”');
            if (s == string.Empty)
            {
                return false;
            }

            var last = s[s.Length - 1];
            return last == '.' || last == '!' || last == '?' || last == ']' || last == ')' || last == '…' || last == '♪' || last == '؟' ||
                   twoLetterLanguageCode == "el" && last == ';' || twoLetterLanguageCode == "el" && last == '\u037E' ||
                   last == '-' && s.Length > 3 && s.EndsWith("--", StringComparison.Ordinal) && char.IsLetter(s[s.Length - 3]) ||
                   last == '—' && s.Length > 2 && char.IsLetter(s[s.Length - 2]);
        }

        public static string RemoveLineBreaks(string input)
        {
            var s = HtmlUtil.FixUpperTags(input);

            s = s.Replace("</i> " + Environment.NewLine + "<i>", Environment.NewLine);
            s = s.Replace("</i>" + Environment.NewLine + " <i>", Environment.NewLine);
            s = s.Replace("</i>" + Environment.NewLine + "<i>", Environment.NewLine);

            s = s.Replace(Environment.NewLine + " </i>", "</i>" + Environment.NewLine);
            s = s.Replace(Environment.NewLine + " </b>", "</b>" + Environment.NewLine);
            s = s.Replace(Environment.NewLine + " </u>", "</u>" + Environment.NewLine);
            s = s.Replace(Environment.NewLine + " </font>", "</font>" + Environment.NewLine);

            s = s.Replace(" " + Environment.NewLine + "</i>", "</i>" + Environment.NewLine);
            s = s.Replace(" " + Environment.NewLine + "</b>", "</b>" + Environment.NewLine);
            s = s.Replace(" " + Environment.NewLine + "</u>", "</u>" + Environment.NewLine);
            s = s.Replace(" " + Environment.NewLine + "</font>", "</font>" + Environment.NewLine);

            s = s.Replace(Environment.NewLine + "</i>", "</i>" + Environment.NewLine);
            s = s.Replace(Environment.NewLine + "</b>", "</b>" + Environment.NewLine);
            s = s.Replace(Environment.NewLine + "</u>", "</u>" + Environment.NewLine);
            s = s.Replace(Environment.NewLine + "</font>", "</font>" + Environment.NewLine);

            while (s.Contains(" " + Environment.NewLine))
            {
                s = s.Replace(" " + Environment.NewLine, Environment.NewLine);
            }

            while (s.Contains(Environment.NewLine + " "))
            {
                s = s.Replace(Environment.NewLine + " ", Environment.NewLine);
            }

            s = s.Replace(Environment.NewLine, " ");
            return s.Trim();
        }

        /// <summary>
        /// Note: Requires a space before the NewLine
        /// </summary>
        private static string ReInsertHtmlTags(string s, Dictionary<int, string> htmlTags)
        {
            if (htmlTags.Count > 0)
            {
                var sb = new StringBuilder(s.Length);
                var six = 0;
                foreach (var letter in s)
                {
                    if (Environment.NewLine.Contains(letter))
                    {
                        sb.Append(letter);
                    }
                    else
                    {
                        if (htmlTags.ContainsKey(six))
                        {
                            sb.Append(htmlTags[six]);
                        }
                        sb.Append(letter);
                        six++;
                    }
                }

                for (var i = 0; i < 15; i++)
                {
                    if (htmlTags.ContainsKey(six + i))
                    {
                        sb.Append(htmlTags[six + i]);
                    }
                }

                return sb.ToString();
            }
            return s;
        }

        internal static bool CanBreak(string s, int index, string language)
        {
            char nextChar;
            if (index >= 0 && index < s.Length)
            {
                nextChar = s[index];
            }
            else
            {
                return false;
            }

            if (!"\r\n\t ".Contains(nextChar))
            {
                return false;
            }

            // Some words we don't like breaking after
            var s2 = s.Substring(0, index);
            if (s2.EndsWith(" mr.", StringComparison.OrdinalIgnoreCase) ||
                s2.EndsWith(" dr.", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (s2.EndsWith("? -", StringComparison.Ordinal) || s2.EndsWith("! -", StringComparison.Ordinal) || s2.EndsWith(". -", StringComparison.Ordinal))
            {
                return false;
            }

            if (nextChar == ' ' && language == "fr" && index + 1 < s.Length)
            {
                var nextNext = s[index + 1];
                if (nextNext == '?' || nextNext == '!' || nextNext == '.')
                {
                    return false;
                }
            }

            return true;
        }
    }
}