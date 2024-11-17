using System;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public static class StringUtils
    {
        /// <summary>
        /// PayPal donate url.
        /// </summary>
        public static string DonateUrl => "https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=9EFVREKVKC2VJ&source=url";

        public static string UnbreakLine(string text)
        {
            var lines = text.SplitToLines();
            if (lines.Length == 1)
            {
                return text;
            }

            var singleLine = string.Join(" ", lines);
            singleLine = singleLine.FixExtraSpaces();
            if (singleLine.Contains("</")) // Fix tag
            {
                // remove invalid tag
                foreach (string item in new[] { "</i> <i>", "</i><i>", "</b> <b>", "</b> <b>", "</b> <b>", "</b> <b>" })
                {
                    if (item.Length > singleLine.Length)
                    {
                        continue;
                    }
                    singleLine = singleLine.Replace(item, " ");
                }
            }
            return singleLine;
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

        public static bool IsInteger(string s) => int.TryParse(s, out int i);

        //public static string TryFixBrokenBrackets(string text, char bracketType, int nextIdx, Subtitle sub)
        //{
        //    var before = text;
        //    var idx = -1;
        //    // More than one lines
        //    if (text.IndexOf(Environment.NewLine, StringComparison.Ordinal) > -1)
        //    {
        //        var lines = text.SplitToLines();
        //        for (int i = 0; i < lines.Length; i++)
        //        {
        //            CheckForBrackets(ref lines[i], bracketType, nextIdx, sub, ref idx);
        //        }
        //    }
        //    else
        //    {
        //        CheckForBrackets(ref text, bracketType, nextIdx, sub, ref idx);
        //    }
        //    return text;
        //}

        private static void CheckForBrackets(ref string text, char bracketType, int nextIdx, Subtitle sub, ref int idx)
        {
            var doFix = false;
            for (int i = nextIdx; i < sub.Paragraphs.Count; i++)
            {
                var nextText = sub.Paragraphs[nextIdx].Text;
                idx = nextText.IndexOf('(');
                var close = nextText.IndexOf(')', idx);
                if (idx > -1 && close > idx)
                {
                    doFix = true;
                    break;
                }

                // do not keep looking... just exit
                if (nextIdx + 3 == i)
                {
                    break;
                }
            }

            if (doFix)
            {
                text = text + bracketType;
            }
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
                {
                    return count;
                }

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
                {
                    return count;
                }

                index = text.IndexOf(tag, index + 1);
            }
            return count;
        }

        public static bool IsBetweenNumbers(string text, int pos)
        {
            if (text.Length <= 2 || pos + 1 >= text.Length || pos - 1 < 0)
            {
                return false;
            }

            return (text[pos + 1] >= 0x30 && text[pos + 1] <= 0x39) && (text[pos - 1] >= 0x30 && text[pos - 1] <= 0x39);
        }

        public static string RemoveBadChars(string line) => line.Replace('\0', ' ');

        public static string RemoveSsaTags(string s)
        {
            var idx = s.IndexOf('{');
            while (idx >= 0)
            {
                var endIdx = s.IndexOf('}', idx + 1);
                if (endIdx < 0)
                {
                    break;
                }

                s = s.Remove(idx, endIdx - idx + 1);
                idx = s.IndexOf('{', idx);
            }
            return s;
        }

        public static string GetListViewString(string text, bool noTag)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }
            if (text.Length < 2)
            {
                return text;
            }
            if (noTag)
            {
                text = HtmlUtils.RemoveTags(text, true);
            }
            return text.Replace(Environment.NewLine, Options.UILineBreak);
        }
    }
}