using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.PluginLogic.Logic
{
    public static class Utilities
    {

        internal static readonly char[] NewLineChars = Environment.NewLine.ToCharArray();

        public static bool StartsWith(this string s, char c)
        {
            return s.Length > 0 && s[0] == c;
        }

        public static bool StartsWith(this StringBuilder sb, char c)
        {
            return sb.Length > 0 && sb[0] == c;
        }

        public static bool EndsWith(this string s, char c)
        {
            return s.Length > 0 && s[s.Length - 1] == c;
        }

        public static bool EndsWith(this StringBuilder sb, char c)
        {
            return sb.Length > 0 && sb[sb.Length - 1] == c;
        }

        public static bool Contains(this string source, char value)
        {
            return source.IndexOf(value) >= 0;
        }

        public static bool Contains(this string source, string value, StringComparison comparisonType)
        {
            return source.IndexOf(value, comparisonType) >= 0;
        }


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
                int endIndex = Math.Max(s.IndexOf(">"), startIndex + 4);
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
            s = Regex.Replace(s, "(?i)</?[iіbu]>", string.Empty);
            s = RemoveParagraphTag(s);
            return RemoveHtmlFontTag(s).Trim();
        }

        internal static string GetHtmlColorCode(Color color)
        {
            return string.Format("#{0:x2}{1:x2}{2:x2}", color.R, color.G, color.B);
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

        private static bool IsPartOfNumber(string s, int position)
        {
            if (string.IsNullOrWhiteSpace(s))
                return false;

            if (position + 2 > s.Length)
                return false;

            if (@",.".Contains(s[position].ToString()))
            {
                if (position > 0 && position < s.Length - 1)
                {
                    return char.IsDigit(s[position - 1]) && char.IsDigit(s[position + 1]);
                }
            }
            return false;
        }


        private static bool CanBreak(string s, int index, string language)
        {
            char nextChar = ' ';
            if (index < s.Length)
                nextChar = s[index];
            if (!"\r\n\t ".Contains(nextChar.ToString()))
                return false;

            string s2 = s.Substring(0, index);         
            if (s2.EndsWith("? -", StringComparison.Ordinal) || s2.EndsWith("! -", StringComparison.Ordinal) || s2.EndsWith(". -", StringComparison.Ordinal))
                return false;

            return true;
        }


        internal static string AutoBreakLine(string p)
        {
            return AutoBreakLine(p, 43, 10, "en");
        }



        public static string AutoBreakLine(string text, int maximumLength, int mergeLinesShorterThan, string language)
        {
            if (text == null || text.Length < 3)
                return text;

            // do not autobreak dialogs
            if (text.Contains("-") && text.Contains(Environment.NewLine))
            {
                string dialogS = RemoveHtmlTags(text);
                var arr = dialogS.Replace(Environment.NewLine, "\n").Split('\n');
                if (arr.Length == 2)
                {
                    string arr0 = arr[0].Trim().TrimEnd('"').TrimEnd('\'').TrimEnd();
                    if (arr0.StartsWith("-") && arr[1].TrimStart().StartsWith("-") && (arr0.EndsWith(".") || arr0.EndsWith("!") || arr0.EndsWith("?") || arr0.EndsWith("--", StringComparison.Ordinal) || arr0.EndsWith("–")))
                        return text;
                }
            }

            string s = text;
            s = s.Replace("</i> " + Environment.NewLine + "<i>", " ");
            s = s.Replace("</i>" + Environment.NewLine + " <i>", " ");
            s = s.Replace("</i>" + Environment.NewLine + "<i>", " ");
            s = s.Replace(Environment.NewLine, " ");
            s = s.Replace("   ", " ");
            s = s.Replace("  ", " ");
            s = s.Replace("  ", " ");
            s = s.Replace("  ", " ");

            string temp = RemoveHtmlTags(s);

            if (temp.Length < mergeLinesShorterThan)
            {
                string[] lines = text.Split(NewLineChars, StringSplitOptions.RemoveEmptyEntries);
                if (lines.Length > 1)
                {
                    bool isDialog = true;
                    foreach (string line in lines)
                    {
                        string cleanLine = RemoveHtmlTags(line).Trim();
                        isDialog = isDialog && (cleanLine.StartsWith("-") || cleanLine.StartsWith("—"));
                    }
                    if (isDialog)
                    {
                        return text;
                    }
                }
                return s;
            }

            var htmlTags = new Dictionary<int, string>();
            var sb = new StringBuilder();
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

            int splitPos = -1;
            int mid = s.Length / 2;

            // try to find " - " with uppercase letter after (dialog)
            if (splitPos == -1 && s.Contains(" - "))
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
                                if (mid - j > 5 && s[mid - j - 1] == ' ' && @"!?.".Contains(s[mid - j - 2].ToString()))
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

            if (splitPos == -1)
            {
                for (int j = 0; j < 15; j++)
                {
                    if (mid + j + 1 < s.Length && mid + j > 0)
                    {
                        if (@".!?".Contains(s[mid + j].ToString()) && !IsPartOfNumber(s, mid + j) && CanBreak(s, mid + j + 1, language))
                        {
                            splitPos = mid + j + 1;
                            if (@".!?0123456789".Contains(s[splitPos].ToString()))
                            { // do not break double/tripple end lines like "!!!" or "..."
                                splitPos++;
                                if (@".!?0123456789".Contains(s[mid + j + 1]))
                                    splitPos++;
                            }
                            break;
                        }
                        if (@".!?".Contains(s[mid - j]) && !IsPartOfNumber(s, mid - j) && CanBreak(s, mid - j, language))
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

            if (splitPos == -1)
            {
                for (int j = 0; j < 25; j++)
                {
                    if (mid + j + 1 < s.Length && mid + j > 0)
                    {
                        if (@".!?, ".Contains(s[mid + j]) && !IsPartOfNumber(s, mid + j) && s.Length > mid + j + 2 && CanBreak(s, mid + j, language))
                        {
                            splitPos = mid + j;
                            if (@" .!?".Contains(s[mid + j + 1]))
                            {
                                splitPos++;
                                if (@" .!?".Contains(s[mid + j + 2]))
                                    splitPos++;
                            }
                            break;
                        }
                        if (@".!?, ".Contains(s[mid - j]) && !IsPartOfNumber(s, mid - j) && s.Length > mid + j + 2 && CanBreak(s, mid - j, language))
                        {
                            splitPos = mid - j;
                            if (@".!?".Contains(s[splitPos]))
                                splitPos--;
                            if (@".!?".Contains(s[splitPos]))
                                splitPos--;
                            if (@".!?".Contains(s[splitPos]))
                                splitPos--;
                            break;
                        }
                    }
                }
            }

            if (splitPos == -1)
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
            s = s.Replace(" " + Environment.NewLine, Environment.NewLine);
            s = s.Replace(Environment.NewLine + " ", Environment.NewLine);

            return s.TrimEnd();
        }

        private static string ReInsertHtmlTags(string s, Dictionary<int, string> htmlTags)
        {
            if (htmlTags.Count > 0)
            {
                var sb = new StringBuilder();
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
                s = sb.ToString();
            }
            return s;
        }

        internal static int GetNumberOfLines(string text)
        {
            if (string.IsNullOrEmpty(text))
                return 0;

            int lines = 1;
            int idx = text.IndexOf('\n');
            while (idx != -1)
            {
                lines++;
                idx = text.IndexOf('\n', idx + 1);
            }
            return lines;
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

        public static int CountTagInText(string text, char tag)
        {
            int count = 0;
            int index = text.IndexOf(tag);
            while (index >= 0)
            {
                count++;
                if ((index + 1) == text.Length)
                    return count;
                index = text.IndexOf(tag, index + 1);
            }
            return count;
        }


        private const string UppercaseLetters = "ABCDEFGHIJKLMNOPQRSTUVWZYXÆØÃÅÄÖÉÈÁÂÀÇÊÍÓÔÕÚŁАБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯĞİŞÜÙÁÌÑÎ";

        private static string GetLetters(bool uppercase, bool lowercase, bool numbers)
        {
            var sb = new StringBuilder();

            if (uppercase)
                sb.Append(UppercaseLetters.ToUpper());

            if (lowercase)
                sb.Append(UppercaseLetters.ToLower());

            if (numbers)
                sb.Append("0123456789");

            return sb.ToString();
        }

        public static readonly string AllLetters = GetLetters(true, true, false);

    }
}