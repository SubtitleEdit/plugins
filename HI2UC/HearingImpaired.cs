using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public class HearingImpaired : IHearingImpaired
    {
        public static readonly char[] ExpectedHIChars = { '(', '[' };
        private HIStyle _style;

        private readonly Regex _regexFirstChar = new Regex(@"\b\w", RegexOptions.Compiled);
        private readonly Regex _RegexFirstChar = new Regex("(?<!<)\\w", RegexOptions.Compiled); // Will also avoid matching <i> or <b>...

        private bool _moodsMatched;
        private bool _namesMatched;

        public HIStyle Style
        {
            get
            {
                return _style;
            }
            set
            {
                _style = value;
            }
        }

        public HearingImpaired(HIStyle style = HIStyle.UpperCase) // take enums style
        {
            _style = style;
        }

        public string ChangeMoodsToUppercase(string text)
        {
            text = text.Replace("()", string.Empty);
            text = text.Replace("[]", string.Empty);
            if (text.ContainsAny(ExpectedHIChars))
            {
                text = FilterMoods(text);
            }
            //var bIdx = text.IndexOfAny(ExpectedHIChars);
            //if (bIdx >= 0)
            //    FindBrackets(text[bIdx], bIdx);
            return text.FixExtraSpaces();
        }

        private string FilterMoods(string text)
        {
            var idx = text.IndexOfAny(ExpectedHIChars);

            char openBracket = text[idx];
            char closeBracket = '\0';
            switch (openBracket)
            {
                case '(':
                    closeBracket = ')';
                    break;
                case '[':
                    closeBracket = ']';
                    break;
            }

            var pre = string.Empty;
            var post = string.Empty;
            while (idx >= 0)
            {
                int endIdx = text.IndexOf(closeBracket, idx + 1); // ] or )
                if (endIdx < idx)
                    break;

                var moodText = text.Substring(idx + 1, endIdx - idx - 1).Trim();
                // pre:
                // three length html-tags
                if (moodText.Length > 3 && moodText[0] == '<' && moodText[2] == '>')
                {
                    pre = moodText.Substring(0, 3);
                    moodText = moodText.Substring(3).TrimStart();
                }
                // font-tags
                if (moodText.Length > 5 && moodText.StartsWith("<font", StringComparison.OrdinalIgnoreCase))
                {
                    var closeIdx = moodText.IndexOf('>', 5);
                    if (closeIdx > 0)
                    {
                        pre += moodText.Substring(0, closeIdx + 1);
                        moodText = moodText.Substring(closeIdx + 1).TrimStart();
                    }
                }
                // post:
                // three length close tags e.g: </i>, </b>...
                if (moodText.Length > 4 && moodText[moodText.Length - 4] == '<' && moodText[moodText.Length - 1] == '>')
                {
                    post = moodText.Substring(moodText.Length - 4);
                    moodText = moodText.Substring(0, moodText.Length - 4);
                }
                // </font>
                if (moodText.Length > 8 && moodText[moodText.Length - 7] == '<' && moodText[moodText.Length - 1] == '>')
                {
                    post += moodText.Substring(moodText.Length - 7);
                    moodText = moodText.Substring(0, moodText.Length - 7);
                }
                moodText = SetStyle(moodText);
                moodText = pre + moodText + post;
                if (_moodsMatched)
                    text = text.Remove(idx + 1, endIdx - idx - 1).Insert(idx + 1, moodText);

                idx = text.IndexOf(openBracket, endIdx + 1); // ( or [
            }

            return text;
        }

        private string SetStyle(string text)
        {
            var sb = new StringBuilder();
            var before = text;
            switch (_style)
            {
                case HIStyle.UpperLowerCase:
                    sb.Clear();
                    bool isUpperTime = true;
                    foreach (char myChar in text)
                    {
                        if (!char.IsLetter(myChar))
                        {
                            sb.Append(myChar);
                        }
                        else
                        {
                            sb.Append(isUpperTime ? char.ToUpper(myChar) : char.ToLower(myChar));
                            isUpperTime = !isUpperTime;
                        }
                    }
                    text = sb.ToString();
                    break;
                case HIStyle.FirstUppercase:
                    text = _regexFirstChar.Replace(text.ToLower(), x => x.Value.ToUpper()); // foobar to Foobar
                    break;
                case HIStyle.UpperCase:
                    text = text.ToUpper();
                    break;
                case HIStyle.LowerCase:
                    text = text.ToLower();
                    break;
            }
            _moodsMatched = text != before;
            return text;
        }


        //delegate void RefAction<in T>(ref T obj);
        public string NarratorToUpper(string text)
        {
            string before = text;
            var t = Utilities.RemoveHtmlTags(text, true).TrimEnd().TrimEnd('"');
            var index = t.IndexOf(':');

            // like: "Ivandro Says:"
            if (index == t.Length - 1 || text.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                return text;
            }

            var lines = text.SplitToLines();
            for (int i = 0; i < lines.Length; i++)
            {
                var noTagText = Utilities.RemoveHtmlTags(lines[i], true).Trim();
                index = noTagText.IndexOf(':');
                if (!CanUpper(noTagText, index))
                    continue;

                // Ivandro ismael:
                // hello world!
                if (i > 0 && index >= noTagText.Length)
                    continue;

                // Now find : in original text
                index = lines[i].IndexOf(':');
                if (index > 0)
                {
                    lines[i] = ConvertToUpper(lines[i], index);
                }
            }
            text = string.Join(Environment.NewLine, lines);

            if (before != text)
                _namesMatched = true;
            return text;
        }


        private bool CanUpper(string line, int index)
        {
            if (index <= 0 || (index + 1 >= line.Length) || char.IsDigit(line[index + 1]))
                return false;
            line = line.Substring(0, index);
            if (line.EndsWith("improved by", StringComparison.OrdinalIgnoreCase) ||
                line.EndsWith("corrected by", StringComparison.OrdinalIgnoreCase))
                return false;
            return true;
        }

        private string ConvertToUpper(string s, int colonIdx)
        {
            var pre = s.Substring(0, colonIdx);

            // (Adele: ...)
            if (pre.ContainsAny(ExpectedHIChars) || s.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                return s;

            if (Utilities.RemoveHtmlTags(pre, true).Trim().Length > 1)
            {
                var firstChr = _RegexFirstChar.Match(pre).Value; // Note: this will prevent to fix tag pre narrator <i>John: Hey!</i>
                int idx = pre.IndexOf(firstChr, StringComparison.Ordinal);
                if (idx >= 0)
                {
                    var narrator = pre.Substring(idx, colonIdx - idx);
                    var oldNarrator = narrator;
                    // Filter http protocols
                    if (narrator.TrimEnd().EndsWith("HTTPS", StringComparison.OrdinalIgnoreCase) ||
                        narrator.TrimEnd().EndsWith("HTTP", StringComparison.OrdinalIgnoreCase))
                        return s;

                    narrator = narrator.ToUpper();
                    // Return if narrator is already uppercase
                    if (narrator == oldNarrator)
                        return s;
                    pre = pre.Remove(idx, colonIdx - idx).Insert(idx, narrator);
                    s = s.Remove(0, colonIdx).Insert(0, pre);
                }
            }
            return s;
        }
    }
}
