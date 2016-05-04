using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public class HearingImpaired
    {
        public Configuration Config { get; private set; }

        private static readonly char[] HIChars = { '(', '[' };
        private static readonly Regex RegexExtraSpaces = new Regex(@"(?<=[\(\[]) +| +(?=[\)\]])", RegexOptions.Compiled);
        private static readonly Regex RegexFirstChar = new Regex(@"\b\w", RegexOptions.Compiled);

        private readonly Lazy<StringBuilder> _lazySb = new Lazy<StringBuilder>();
        private StringBuilder _sb;

        public HearingImpaired(Configuration config)
        {
            Config = config;
        }

        public string NarratorToUppercase(string text)
        {
            string noTagText = Utilities.RemoveHtmlTags(text, true).TrimEnd().TrimEnd('"');
            int index = noTagText.IndexOf(':');

            // Skip single line that ends with ':'.
            if ((index < 0) || (index + 1 == noTagText.Length) || noTagText.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                return text;
            }
            string[] lines = text.SplitToLines();
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                string noTagLine = Utilities.RemoveHtmlTags(line, true);
                int colonIdx = noTagLine.IndexOf(':');

                // Only allow colon at last position if it's 1st line.
                if (i > 0 && colonIdx + 1 == noTagLine.Length)
                {
                    continue;
                }
                if (IsQualifiedNarrator(line, colonIdx))
                {
                    // Find index from original text.
                    colonIdx = line.IndexOf(':') + 1;
                    string preText = line.Substring(0, colonIdx);
                    preText = Customize(preText);
                    lines[i] = preText + line.Substring(colonIdx);
                }
            }
            return string.Join(Environment.NewLine, lines);
        }

        public string MoodsToUppercase(string text)
        {
            // Remove invalid tags.
            text = text.Replace("()", string.Empty);
            text = text.Replace("[]", string.Empty);

            if (!IsQualifedMoods(text))
            {
                return text;
            }

            int idx = text.IndexOfAny(HIChars);
            char moodStartChar = text[idx];
            char moodEndChar = (moodStartChar == '(') ? ')' : ']';
            do
            {
                int endIdx = text.IndexOf(moodEndChar, idx + 1); // ] or )
                // There most be at lease one chars inside brackets.
                if (endIdx < idx + 2)
                {
                    break;
                }
                string moodText = text.Substring(idx, endIdx - idx + 1);
                moodText = Customize(moodText);
                text = text.Remove(idx, endIdx - idx + 1).Insert(idx, moodText);
                idx = text.IndexOf(moodStartChar, endIdx + 1); // ( or [
            }
            while (idx >= 0);
            return text;
        }

        public string RemoveExtraSpacesInsideTag(string text) => RegexExtraSpaces.Replace(text, string.Empty);

        private string Customize(string capturedText)
        {
            var st = new StripableText(capturedText);
            string strippedText = st.StrippedText;
            switch (Config.Style)
            {
                case HIStyle.UpperLowerCase:
                    if (_sb == null)
                    {
                        _sb = _lazySb.Value;
                    }
                    else
                    {
                        _sb.Clear();
                    }
                    bool isUpperTime = true;
                    // TODO: Use StringInfo to fix issue with unicode chars?!
                    foreach (char myChar in strippedText)
                    {
                        if (!char.IsLetter(myChar))
                        {
                            _sb.Append(myChar);
                        }
                        else
                        {
                            _sb.Append(isUpperTime ? char.ToUpper(myChar) : char.ToLower(myChar));
                            isUpperTime = !isUpperTime;
                        }
                    }
                    strippedText = _sb.ToString();
                    break;
                case HIStyle.FirstUppercase:
                    // "foobar foobar" to (Foobar Foobar)
                    strippedText = RegexFirstChar.Replace(strippedText.ToLower(), x => x.Value.ToUpper());
                    break;
                case HIStyle.UpperCase:
                    strippedText = strippedText.ToUpper();
                    break;
                case HIStyle.LowerCase:
                    strippedText = strippedText.ToLower();
                    break;
            }
            return st.CombineWithPrePost(strippedText);
        }

        private static bool IsQualifiedNarrator(string line, int colonIdx)
        {
            string noTagCapturedText = Utilities.RemoveHtmlTags(line.Substring(0, colonIdx + 1), true);
            if (string.IsNullOrWhiteSpace(noTagCapturedText))
            {
                return false;
            }
            // e.g: 12:30am...
            if ((colonIdx + 1 < line.Length) && char.IsDigit(line[colonIdx + 1]))
            {
                return false;
            }
            if (noTagCapturedText.StartsWith("http", StringComparison.OrdinalIgnoreCase) ||
                noTagCapturedText.EndsWith("improved by", StringComparison.OrdinalIgnoreCase) ||
                noTagCapturedText.EndsWith("corrected by", StringComparison.OrdinalIgnoreCase) ||
                noTagCapturedText.EndsWith("https", StringComparison.OrdinalIgnoreCase) ||
                noTagCapturedText.EndsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            return true;
        }

        private static bool IsQualifedMoods(string text)
        {
            int idx = text.IndexOfAny(HIChars);
            return (idx >= 0 && idx < text.Length);
        }

    }
}
