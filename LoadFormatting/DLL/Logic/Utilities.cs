using System;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public static class Utilities
    {

        #region StringExtension
        public static bool Contains(this string s, char c)
        {
            return s.Length > 0 && s.IndexOf(c) > 0;
        }
        public static string[] SplitToLines(this string s)
        {
            return s.Replace(Environment.NewLine, "\n").Replace('\r', '\n').Split('\n');
        }

        public static int IndexOfAny(this string s, string[] words, StringComparison comparisonType)
        {
            if (words == null || string.IsNullOrEmpty(s))
            {
                return -1;
            }

            for (int i = 0; i < words.Length; i++)
            {
                var idx = s.IndexOf(words[i], comparisonType);
                if (idx >= 0)
                {
                    return idx;
                }
            }

            return -1;
        }
        #endregion


        public static readonly string UppercaseLetters = Configuration.UppercaseLetters.ToUpperInvariant();
        public static readonly string LowercaseLetters = Configuration.UppercaseLetters.ToLowerInvariant();
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

        internal unsafe static int NumberOfLines(string text)
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
            int count = 0;
            int index = text.IndexOf(tag, StringComparison.Ordinal);
            while (index >= 0)
            {
                count++;
                index += tag.Length;
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

            int lines = 1;
            int idx = text.IndexOf('\n');
            while (idx >= 0)
            {
                lines++;
                idx = text.IndexOf('\n', idx + 1);
            }
            return lines;
        }

        private static string ReverseParenthesis(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return s;
            }
            int len = s.Length;
            var chars = new char[len];
            for (int i = 0; i < len; i++)
            {
                char ch = s[i];
                switch (ch)
                {
                    case '(':
                        ch = ')';
                        break;
                    case ')':
                        ch = '(';
                        break;
                    case '[':
                        ch = ']';
                        break;
                    case ']':
                        ch = '[';
                        break;
                }
                chars[i] = ch;
            }
            return new string(chars);
        }


        public static string FixEnglishTextInRightToLeftLanguage(string text, string reverseChars)
        {
            var sb = new StringBuilder();
            var lines = text.SplitToLines();
            foreach (string line in lines)
            {
                string s = ReverseParenthesis(line.Trim());
                bool numbersOn = false;
                string numbers = string.Empty;
                for (int i = 0; i < s.Length; i++)
                {
                    if (numbersOn && reverseChars.Contains(s[i]))
                    {
                        numbers = s[i] + numbers;
                    }
                    else if (numbersOn)
                    {
                        numbersOn = false;
                        s = s.Remove(i - numbers.Length, numbers.Length).Insert(i - numbers.Length, numbers);
                        numbers = string.Empty;
                    }
                    else if (reverseChars.Contains(s[i]))
                    {
                        numbers = s[i] + numbers;
                        numbersOn = true;
                    }
                }
                if (numbersOn)
                {
                    int i = s.Length;
                    s = s.Remove(i - numbers.Length, numbers.Length).Insert(i - numbers.Length, numbers);
                }

                sb.AppendLine(s);
            }
            return sb.ToString().Trim();
        }

        public static string RemoveControlCharactersButWhiteSpace(string s)
        {
            int max = s.Length;
            var newStr = new char[max];
            int newIdx = 0;
            for (int index = 0; index < max; index++)
            {
                var ch = s[index];
                if (!char.IsControl(ch) || ch == '\u000d' || ch == '\u000a' || ch == '\u0009')
                {
                    newStr[newIdx++] = ch;
                }
            }

            return new string(newStr, 0, newIdx);
        }

        public static bool StartsAndEndsWithTag(string text, string startTag, string endTag)
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

            if (closeTag.Length == 0 && openTag.Contains("<font "))
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

            if (text.Contains(close1))
            {
                text = text.Replace(close1, "!" + closeTag + Environment.NewLine);
            }

            if (text.Contains(close2))
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

            if (text.Contains(open2))
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