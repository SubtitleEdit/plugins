using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SubtitleEdit
{
    internal class AutoBreaker
    {

        public class NoBreakAfterItem : IComparable<NoBreakAfterItem>
        {
            public readonly Regex Regex;
            public readonly string Text;

            public NoBreakAfterItem(Regex regex, string text)
            {
                Regex = regex;
                Text = text;
            }

            public NoBreakAfterItem(string text)
            {
                Text = text;
            }

            public bool IsMatch(string line)
            {
                // Make sure that both *line and *Text are not null.
                if (string.IsNullOrEmpty(line) || string.IsNullOrEmpty(Text))
                    return false;
                if (Regex != null)
                    return Regex.IsMatch(line);
                return line.EndsWith(Text, StringComparison.Ordinal);
            }

            public override string ToString()
            {
                return Text;
            }

            public int CompareTo(NoBreakAfterItem obj)
            {
                if (obj == null)
                    return -1;
                if (obj.Text == null && Text == null)
                    return 0;
                else if (obj.Text == null)
                    return -1;
                return string.Compare(Text, obj.Text, StringComparison.Ordinal);
            }
        }

        private static bool IsPartOfNumber(string s, int position)
        {
            if (string.IsNullOrWhiteSpace(s) || position + 1 >= s.Length)
                return false;

            if (position > 0 && @",.".Contains(s[position]))
            {
                return char.IsDigit(s[position - 1]) && char.IsDigit(s[position + 1]);
            }
            return false;
        }

        public static string AutoBreakLine(string text, string language)
        {
            return AutoBreakLine(text, 43, 40, language);
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
            if (!"\r\n\t ".Contains(nextChar))
                return false;

            // Some words we don't like breaking after
            string s2 = s.Substring(0, index);
            if (true)
            {
                foreach (NoBreakAfterItem ending in NoBreakAfterList(language))
                {
                    if (ending.IsMatch(s2))
                        return false;
                }
            }

            if (s2.EndsWith("? -", StringComparison.Ordinal) || s2.EndsWith("! -", StringComparison.Ordinal) || s2.EndsWith(". -", StringComparison.Ordinal))
                return false;

            return true;
        }

        private static List<NoBreakAfterItem> _lastNoBreakAfterList = new List<NoBreakAfterItem>();
        private static IEnumerable<NoBreakAfterItem> NoBreakAfterList(string languageName)
        {
            if (string.IsNullOrEmpty(languageName) || languageName != "en")
                return new List<NoBreakAfterItem>();

            _lastNoBreakAfterList = new List<NoBreakAfterItem>
            {
                new NoBreakAfterItem("the"),
                new NoBreakAfterItem("a"),
            };
            return _lastNoBreakAfterList;
        }

        public static string AutoBreakLineMoreThanTwoLines(string text, int maximumLineLength, string language)
        {
            if (text == null || text.Length < 3)
                return text;

            string s = AutoBreakLine(text, 0, 0, language);

            var arr = s.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
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
            if (text.Contains('-') && text.Contains(Environment.NewLine))
            {
                var noTagLines = HtmlUtil.RemoveHtmlTags(text, true).Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                if (noTagLines.Length == 2)
                {
                    var arr0 = noTagLines[0].Trim().TrimEnd('"', '\'').TrimEnd();
                    if (arr0.StartsWith('-') && noTagLines[1].TrimStart().StartsWith('-') && arr0.Length > 1 && (".?!)]".Contains(arr0[arr0.Length - 1]) || arr0.EndsWith("--", StringComparison.Ordinal) || arr0.EndsWith('–')))
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
                                if (mid - j > 5 && s[mid - j - 1] == ' ' && @"!?.".Contains(s[mid - j - 2]))
                                {
                                    splitPos = mid - j;
                                    break;
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
                        if (expectedChars2.Contains(s[mid + j]) && !IsPartOfNumber(s, mid + j) && CanBreak(s, mid + j + 1, language))
                        {
                            splitPos = mid + j + 1;
                            if (expectedChars1.Contains(s[splitPos]))
                            { // do not break double/tripple end lines like "!!!" or "..."
                                splitPos++;
                                if (expectedChars1.Contains(s[mid + j + 1]))
                                    splitPos++;
                            }
                            break;
                        }
                        if (expectedChars2.Contains(s[mid - j]) && !IsPartOfNumber(s, mid - j) && CanBreak(s, mid - j, language))
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
                        if (expectedChars1.Contains(s[mid + j]) && !IsPartOfNumber(s, mid + j) && s.Length > mid + j + 2 && CanBreak(s, mid + j, language))
                        {
                            splitPos = mid + j;
                            if (expectedChars2.Contains(s[mid + j + 1]))
                            {
                                splitPos++;
                                if (expectedChars2.Contains(s[mid + j + 2]))
                                    splitPos++;
                            }
                            break;
                        }
                        if (expectedChars1.Contains(s[mid - j]) && !IsPartOfNumber(s, mid - j) && s.Length > mid + j + 2 && CanBreak(s, mid - j, language))
                        {
                            splitPos = mid - j;
                            if (expectedChars3.Contains(s[splitPos]))
                                splitPos--;
                            if (expectedChars3.Contains(s[splitPos]))
                                splitPos--;
                            if (expectedChars3.Contains(s[splitPos]))
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
            s = s.FixExtraSpaces();
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
                if (htmlTags.ContainsKey(six))
                {
                    sb.Append(htmlTags[six]);
                }
                return sb.ToString();
            }
            return s;
        }

        public static string UnbreakLine(string text)
        {
            var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length == 1)
                return text;

            var singleLine = string.Join(" ", lines);
            while (singleLine.Contains("  "))
                singleLine = singleLine.Replace("  ", " ");

            if (singleLine.Contains("</")) // Fix tag
            {
                singleLine = singleLine.Replace("</i> <i>", " ");
                singleLine = singleLine.Replace("</i><i>", " ");

                singleLine = singleLine.Replace("</b> <b>", " ");
                singleLine = singleLine.Replace("</b><b>", " ");

                singleLine = singleLine.Replace("</u> <u>", " ");
                singleLine = singleLine.Replace("</u><u>", " ");
            }
            return singleLine;
        }

        public static string RemoveSsaTags(string s)
        {
            int k = s.IndexOf('{');
            while (k >= 0)
            {
                int l = s.IndexOf('}', k + 1);
                if (l < k) break;
                s = s.Remove(k, l - k + 1);
                k = s.IndexOf('{', k);
            }
            return s;
        }
    }
}
