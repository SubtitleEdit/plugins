using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace WebViewTranslate.Logic
{
    public static class Utilities
    {

        public static readonly string UppercaseLetters = "ABCDEFGHIJKLMNOPQRSTUVWZYXÆØÃÅÄÖÉÈÁÂÀÇÊÍÓÔÕÚŁАБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯĞİŞÜÙÁÌÑÎ";
        public static readonly string LowercaseLetters = UppercaseLetters.ToLower();
        public static readonly string LowercaseLettersWithNumbers = LowercaseLetters + "0123456789";
        public static readonly string AllLetters = UppercaseLetters + LowercaseLetters;
        public static readonly string AllLettersAndNumbers = UppercaseLetters + LowercaseLettersWithNumbers;

        //#region StringExtension
        public static bool Contains(this string s, char c)
        {
            return s.Length > 0 && s.IndexOf(c) > 0;
        }
        //public static string[] SplitToLines(this string s)
        //{
        //    return s.Replace(Environment.NewLine, "\n").Replace('\r', '\n').Split('\n');
        //}
        //#endregion
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

            if (!Contains(s, '<'))
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

        public static int CountTagInText(string text, string tag)
        {
            int count = 0;
            int index = text.IndexOf(tag, StringComparison.Ordinal);
            while (index >= 0)
            {
                count++;
                index = index + tag.Length;
                if (index >= text.Length)
                    return count;
                index = text.IndexOf(tag, index, StringComparison.Ordinal);
            }
            return count;
        }

        public static string FixExtraSpaces(this string s)
        {
            if (string.IsNullOrEmpty(s))
                return s;
            int len = s.Length;
            int k = -1;
            for (int i = len - 1; i >= 0; i--)
            {
                char ch = s[i];
                if (k < 2)
                {
                    if (ch == 0x20)
                    {
                        k = i + 1;
                    }
                }
                else if (ch != 0x20)
                {
                    // Two or more white-spaces found!
                    if (k - (i + 1) > 1)
                    {
                        // Keep only one white-space.
                        s = s.Remove(i + 1, k - (i + 2));
                    }

                    // No white-space after/before line break.
                    if ((ch == '\n' || ch == '\r') && i + 1 < s.Length && s[i + 1] == 0x20)
                    {
                        s = s.Remove(i + 1, 1);
                    }
                    // Reset remove length.
                    k = -1;
                }
                if (ch == 0x20 && i + 1 < s.Length && (s[i + 1] == '\n' || s[i + 1] == '\r'))
                {
                    s = s.Remove(i, 1);
                }
            }
            return s;
        }

        public static int GetNumberOfLines(string text)
        {
            if (string.IsNullOrEmpty(text))
                return 0;

            int lines = 1;
            int idx = text.IndexOf('\n');
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



        public static string AutoBreakLine(string text, string language)
        {
            return AutoBreakLine(text, 43, 22, language);
        }

        public static string AutoBreakLine(string text)
        {
            return AutoBreakLine(text, string.Empty); // no language
        }

        private static bool CanBreak(string s, int index, string language)
        {
            char nextChar;
            if (index >= 0 && index < s.Length)
                nextChar = s[index];
            else
                return false;
            if (!Contains("\r\n\t ", nextChar))
                return false;

            // Some words we don't like breaking after
            string s2 = s.Substring(0, index);
            if (s2.EndsWith("? -", StringComparison.Ordinal) || s2.EndsWith("! -", StringComparison.Ordinal) || s2.EndsWith(". -", StringComparison.Ordinal))
                return false;

            return true;
        }

        public static string AutoBreakLineMoreThanTwoLines(string text, int maximumLineLength, string language)
        {
            if (text == null || text.Length < 3)
                return text;

            string s = AutoBreakLine(text, 0, 0, language);

            var arr = s.SplitToLines();
            if ((arr.Length < 2 && arr[0].Length <= maximumLineLength) || (arr[0].Length <= maximumLineLength && arr[1].Length <= maximumLineLength))
                return s;

            s = RemoveLineBreaks(s);

            var htmlTags = new Dictionary<int, string>();
            var sb = new StringBuilder(s.Length);
            int six = 0;
            while (six < s.Length)
            {
                var letter = s[six];
                var tagFound = letter == '<' && (s.Substring(six).StartsWith("<font", StringComparison.OrdinalIgnoreCase)
                                                 || s.Substring(six).StartsWith("</font", StringComparison.OrdinalIgnoreCase)
                                                 || s.Substring(six).StartsWith("<u", StringComparison.OrdinalIgnoreCase)
                                                 || s.Substring(six).StartsWith("</u", StringComparison.OrdinalIgnoreCase)
                                                 || s.Substring(six).StartsWith("<b", StringComparison.OrdinalIgnoreCase)
                                                 || s.Substring(six).StartsWith("</b", StringComparison.OrdinalIgnoreCase)
                                                 || s.Substring(six).StartsWith("<i", StringComparison.OrdinalIgnoreCase)
                                                 || s.Substring(six).StartsWith("</i", StringComparison.OrdinalIgnoreCase));
                int endIndex = -1;
                if (tagFound)
                    endIndex = s.IndexOf('>', six + 1);

                if (tagFound && endIndex > 0)
                {
                    string tag = s.Substring(six, endIndex - six + 1);
                    s = s.Remove(six, tag.Length);
                    if (htmlTags.ContainsKey(six))
                        htmlTags[six] = htmlTags[six] + tag;
                    else
                        htmlTags.Add(six, tag);
                }
                else
                {
                    sb.Append(letter);
                    six++;
                }
            }
            s = sb.ToString();

            var words = s.Split(' ');
            for (int numberOfLines = 3; numberOfLines < 9999; numberOfLines++)
            {
                int average = s.Length / numberOfLines + 1;
                for (int len = average; len < maximumLineLength; len++)
                {
                    List<int> list = SplitToX(words, numberOfLines, len);
                    bool allOk = true;
                    foreach (var lineLength in list)
                    {
                        if (lineLength > maximumLineLength)
                            allOk = false;
                    }
                    if (allOk)
                    {
                        int index = 0;
                        foreach (var item in list)
                        {
                            index += item;
                            htmlTags.Add(index, Environment.NewLine);
                        }
                        s = ReInsertHtmlTags(s, htmlTags);
                        s = s.Replace(" " + Environment.NewLine, Environment.NewLine);
                        s = s.Replace(Environment.NewLine + " ", Environment.NewLine);
                        s = s.Replace(Environment.NewLine + "</i>", "</i>" + Environment.NewLine);
                        s = s.Replace(Environment.NewLine + "</b>", "</b>" + Environment.NewLine);
                        s = s.Replace(Environment.NewLine + "</u>", "</u>" + Environment.NewLine);
                        s = s.Replace(Environment.NewLine + "</font>", "</font>" + Environment.NewLine);
                        return s.TrimEnd();
                    }
                }
            }

            return text;
        }

        private static List<int> SplitToX(string[] words, int count, int average)
        {
            var list = new List<int>();
            int currentIdx = 0;
            int currentCount = 0;
            foreach (string word in words)
            {
                if (currentCount + word.Length + 3 > average && currentIdx < count)
                {
                    list.Add(currentCount);
                    currentIdx++;
                    currentCount = 0;
                }
                currentCount += word.Length + 1;
            }
            if (currentIdx < count)
                list.Add(currentCount);
            else
                list[list.Count - 1] += currentCount;
            return list;
        }

        public static string AutoBreakLine(string text, int maximumLength, int mergeLinesShorterThan, string language)
        {
            if (text == null || text.Length < 3)
                return text;

            // do not autobreak dialogs
            if (Contains(text, '-') && text.Contains(Environment.NewLine))
            {
                var noTagLines = HtmlUtil.RemoveHtmlTags(text, true).SplitToLines();
                if (noTagLines.Length == 2)
                {
                    var arr0 = noTagLines[0].Trim().TrimEnd('"', '\'').TrimEnd();
                    if (arr0.StartsWith('-') && noTagLines[1].TrimStart().StartsWith('-') && arr0.Length > 1 && (Contains(".?!)]", arr0[arr0.Length - 1]) || arr0.EndsWith("--", StringComparison.Ordinal) || arr0.EndsWith('–')))
                        return text;
                }
            }

            string s = RemoveLineBreaks(text);
            if (HtmlUtil.RemoveHtmlTags(s, true).Length < mergeLinesShorterThan)
            {
                return s;
            }

            var htmlTags = new Dictionary<int, string>();
            var sb = new StringBuilder();
            int six = 0;
            while (six < s.Length)
            {
                var letter = s[six];
                bool tagFound = false;
                if (letter == '<')
                {
                    string tagString = s.Substring(six);
                    tagFound = tagString.StartsWith("<font", StringComparison.OrdinalIgnoreCase)
                               || tagString.StartsWith("</font", StringComparison.OrdinalIgnoreCase)
                               || tagString.StartsWith("<u", StringComparison.OrdinalIgnoreCase)
                               || tagString.StartsWith("</u", StringComparison.OrdinalIgnoreCase)
                               || tagString.StartsWith("<b", StringComparison.OrdinalIgnoreCase)
                               || tagString.StartsWith("</b", StringComparison.OrdinalIgnoreCase)
                               || tagString.StartsWith("<i", StringComparison.OrdinalIgnoreCase)
                               || tagString.StartsWith("</i", StringComparison.OrdinalIgnoreCase);
                }

                int endIndex = -1;
                if (tagFound)
                    endIndex = s.IndexOf('>', six + 1);

                if (tagFound && endIndex > 0)
                {
                    string tag = s.Substring(six, endIndex - six + 1);
                    s = s.Remove(six, tag.Length);
                    if (htmlTags.ContainsKey(six))
                        htmlTags[six] = htmlTags[six] + tag;
                    else
                        htmlTags.Add(six, tag);
                }
                else
                {
                    sb.Append(letter);
                    six++;
                }
            }
            s = sb.ToString();

            int splitPos = -1;
            int mid = s.Length / 2;

            // try to find " - " with uppercase letter after (dialog)
            if (s.Contains(" - "))
            {
                for (int j = 0; j <= (maximumLength / 2) + 5; j++)
                {
                    if (mid + j + 4 < s.Length)
                    {
                        if (s[mid + j] == '-' && s[mid + j + 1] == ' ' && s[mid + j - 1] == ' ')
                        {
                            string rest = s.Substring(mid + j + 1).TrimStart();
                            if (rest.Length > 0 && char.IsUpper(rest[0]))
                            {
                                splitPos = mid + j;
                                break;
                            }
                        }
                    }
                    if (mid - (j + 1) > 4)
                    {
                        if (s[mid - j] == '-' && s[mid - j + 1] == ' ' && s[mid - j - 1] == ' ')
                        {
                            string rest = s.Substring(mid - j + 1).TrimStart();
                            if (rest.Length > 0 && char.IsUpper(rest[0]))
                            {
                                if (mid - j > 5 && s[mid - j - 1] == ' ')
                                {
                                    if (Contains("!?.", s[mid - j - 2]))
                                    {
                                        splitPos = mid - j;
                                        break;
                                    }
                                    var first = s.Substring(0, mid - j - 1);
                                    if (first.EndsWith(".\"", StringComparison.Ordinal) || first.EndsWith("!\"", StringComparison.Ordinal) || first.EndsWith("?\"", StringComparison.Ordinal))
                                    {
                                        splitPos = mid - j;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (splitPos == maximumLength + 1 && s[maximumLength] != ' ') // only allow space for last char (as it does not count)
                splitPos = -1;

            if (splitPos < 0)
            {
                const string expectedChars1 = ".!?0123456789";
                const string expectedChars2 = ".!?";
                for (int j = 0; j < 15; j++)
                {
                    if (mid + j + 1 < s.Length && mid + j > 0)
                    {
                        if (Contains(expectedChars2, s[mid + j]) && !IsPartOfNumber(s, mid + j) && CanBreak(s, mid + j + 1, language))
                        {
                            splitPos = mid + j + 1;
                            if (Contains(expectedChars1, s[splitPos]))
                            { // do not break double/tripple end lines like "!!!" or "..."
                                splitPos++;
                                if (Contains(expectedChars1, s[mid + j + 1]))
                                    splitPos++;
                            }
                            break;
                        }
                        if (Contains(expectedChars2, s[mid - j]) && !IsPartOfNumber(s, mid - j) && CanBreak(s, mid - j, language))
                        {
                            splitPos = mid - j;
                            splitPos++;
                            break;
                        }
                    }
                }
            }

            if (splitPos > maximumLength) // too long first line
            {
                if (splitPos != maximumLength + 1 || s[maximumLength] != ' ') // allow for maxlength+1 char to be space (does not count)
                    splitPos = -1;
            }
            else if (splitPos >= 0 && s.Length - splitPos > maximumLength) // too long second line
            {
                splitPos = -1;
            }

            if (splitPos < 0)
            {
                const string expectedChars1 = ".!?, ";
                const string expectedChars2 = " .!?";
                const string expectedChars3 = ".!?";
                for (int j = 0; j < 25; j++)
                {
                    if (mid + j + 1 < s.Length && mid + j > 0)
                    {
                        if (Contains(expectedChars1, s[mid + j]) && !IsPartOfNumber(s, mid + j) && s.Length > mid + j + 2 && CanBreak(s, mid + j, language))
                        {
                            splitPos = mid + j;
                            if (Contains(expectedChars2, s[mid + j + 1]))
                            {
                                splitPos++;
                                if (Contains(expectedChars2, s[mid + j + 2]))
                                    splitPos++;
                            }
                            break;
                        }
                        if (Contains(expectedChars1, s[mid - j]) && !IsPartOfNumber(s, mid - j) && s.Length > mid + j + 2 && CanBreak(s, mid - j, language))
                        {
                            splitPos = mid - j;
                            if (Contains(expectedChars3, s[splitPos]))
                                splitPos--;
                            if (Contains(expectedChars3, s[splitPos]))
                                splitPos--;
                            if (Contains(expectedChars3, s[splitPos]))
                                splitPos--;
                            break;
                        }
                    }
                }
            }

            if (splitPos < 0)
            {
                splitPos = mid;
                s = s.Insert(mid - 1, Environment.NewLine);
                s = ReInsertHtmlTags(s, htmlTags);
                htmlTags = new Dictionary<int, string>();
                s = s.Replace(Environment.NewLine, "-");
            }
            if (splitPos < s.Length - 2)
                s = s.Substring(0, splitPos) + Environment.NewLine + s.Substring(splitPos);

            s = ReInsertHtmlTags(s, htmlTags);
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

        public static string RemoveLineBreaks(string s)
        {
            s = HtmlUtil.FixUpperTags(s);
            s = s.Replace(Environment.NewLine + "</i>", "</i>" + Environment.NewLine);
            s = s.Replace(Environment.NewLine + "</b>", "</b>" + Environment.NewLine);
            s = s.Replace(Environment.NewLine + "</u>", "</u>" + Environment.NewLine);
            s = s.Replace(Environment.NewLine + "</font>", "</font>" + Environment.NewLine);
            s = s.Replace("</i> " + Environment.NewLine + "<i>", " ");
            s = s.Replace("</i>" + Environment.NewLine + " <i>", " ");
            s = s.Replace("</i>" + Environment.NewLine + "<i>", " ");
            s = s.Replace(Environment.NewLine, " ");
            s = s.Replace(" </i>", "</i> ");
            s = s.Replace(" </b>", "</b> ");
            s = s.Replace(" </u>", "</u> ");
            s = s.Replace(" </font>", "</font> ");
            s = FixExtraSpaces(s);
            return s.Trim();
        }

        private static string ReInsertHtmlTags(string s, Dictionary<int, string> htmlTags)
        {
            if (htmlTags.Count > 0)
            {
                var sb = new StringBuilder(s.Length);
                int six = 0;
                foreach (var letter in s)
                {
                    if (Contains(Environment.NewLine, letter))
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
                if (htmlTags.ContainsKey(six))
                {
                    sb.Append(htmlTags[six]);
                }
                return sb.ToString();
            }
            return s;
        }

        private static bool IsPartOfNumber(string s, int position)
        {
            if (string.IsNullOrWhiteSpace(s) || position + 1 >= s.Length)
                return false;

            if (position > 0 && Contains(@",.", s[position]))
            {
                return char.IsDigit(s[position - 1]) && char.IsDigit(s[position + 1]);
            }
            return false;
        }

    }
}