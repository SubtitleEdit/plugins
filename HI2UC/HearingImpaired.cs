using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public class HearingImpaired
    {
        public HIConfigs Config { get; private set; }
        private static readonly char[] _lineCloseChars = new[] { '!', '?' };
        private static readonly char[] HIChars = { '(', '[' };
        private static readonly Regex RegexExtraSpaces = new Regex(@"(?<=[\(\[]) +| +(?=[\)\]])", RegexOptions.Compiled);
        private static readonly Regex RegexFirstChar = new Regex(@"\b\w", RegexOptions.Compiled);

        private readonly Lazy<StringBuilder> _lazySb = new Lazy<StringBuilder>();
        private StringBuilder _sb;

        public HearingImpaired(HIConfigs config)
        {
            Config = config;
        }

        public string NarratorToUppercase(string text)
        {
            string noTagText = HtmlUtils.RemoveTags(text, true).TrimEnd().TrimEnd('"');

            if (noTagText.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                return text;
            }

            // Skip single line that ends with ':'.
            int index = noTagText.IndexOf(':');
            if (index < 0)
            {
                return text;
            }

            // Lena:
            // A ring?!
            int newLineIdx = noTagText.IndexOf(Environment.NewLine, StringComparison.Ordinal);
            if (!Config.SingleLineNarrator && index + 1 == newLineIdx)
            {
                return text;
            }

            string[] lines = text.SplitToLines();
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                string noTagLine = HtmlUtils.RemoveTags(line, true);

                int colonIdx = noTagLine.IndexOf(':');
                if (colonIdx < 0)
                {
                    continue;
                }
                // Only allow colon at last position if it's 1st line.
                if (colonIdx + 1 == noTagLine.Length)
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
            // <font color="#ff00ff">(SIGHS)</font>
            // Remove invalid tags.
            text = text.Replace("()", string.Empty);
            text = text.Replace("()", string.Empty);
            text = text.Replace("( )", string.Empty);
            text = text.Replace("[ ]", string.Empty);

            if (!IsQualifedMoods(text))
            {
                return text;
            }

            int idx = text.IndexOfAny(HIChars);
            char openChar = text[idx];
            char closeChar = openChar == '(' ? ')' : ']';
            do
            {
                int endIdx = text.IndexOf(closeChar, idx + 1); // ] or )
                // There most be at lease one chars inside brackets.
                if (endIdx < idx + 2)
                {
                    break;
                }
                string parensText = text.Substring(idx, endIdx - idx + 1);
                // remove parents with empty text
                string noTagTextInsideParens = HtmlUtils.RemoveTags(parensText, true);
                text = text.Remove(idx, parensText.Length);
                if (string.IsNullOrWhiteSpace(noTagTextInsideParens))
                {
                    idx = text.IndexOf(openChar, idx);
                }
                else
                {
                    text = text.Insert(idx, Customize(parensText));
                    idx = text.IndexOf(openChar, endIdx + 1); // ( or [
                }
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
                    // TODO: Use StringInfo to fix issue with unicode chars?
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
                case HIStyle.TitleCase:
                    // "foobar foobar" to (Foobar Foobar)
                    strippedText = RegexFirstChar.Replace(strippedText.ToLower(), x => x.Value.ToUpper(CultureInfo.CurrentCulture));
                    break;
                case HIStyle.UpperCase:
                    strippedText = strippedText.ToUpper(CultureInfo.CurrentCulture);
                    break;
                case HIStyle.LowerCase:
                    strippedText = strippedText.ToLower(CultureInfo.CurrentCulture);
                    break;
            }
            return st.CombineWithPrePost(strippedText);
        }

        private static bool IsQualifiedNarrator(string line, int colonIdx)
        {
            string noTagCapturedText = HtmlUtils.RemoveTags(line.Substring(0, colonIdx + 1), true);
            if (string.IsNullOrWhiteSpace(noTagCapturedText))
            {
                return false;
            }
            // e.g: 12:30am...
            if (colonIdx + 1 < line.Length)
            {
                char lastChar = line[colonIdx + 1];
                if (char.IsDigit(lastChar))
                {
                    return false;
                }
                // slash after https://
                if (lastChar == '/')
                {
                    return false;
                }
            }

            // Foobar[?!] Narrator: Hello (Note: not really sure if "." (dot) should be include since there are names
            // that are prefixed with Mr. Ivandro Ismael)
            return !noTagCapturedText.ContainsAny(_lineCloseChars) &&
               (!noTagCapturedText.StartsWith("http", StringComparison.OrdinalIgnoreCase) &&
                !noTagCapturedText.EndsWith("improved by", StringComparison.OrdinalIgnoreCase) &&
                !noTagCapturedText.EndsWith("corrected by", StringComparison.OrdinalIgnoreCase) &&
                !noTagCapturedText.EndsWith("https", StringComparison.OrdinalIgnoreCase) &&
                !noTagCapturedText.EndsWith("http", StringComparison.OrdinalIgnoreCase));
        }

        private static bool IsQualifedMoods(string text)
        {
            int idx = text.IndexOfAny(HIChars);
            return (idx >= 0 && idx + 1 < text.Length);
        }

    }
}
