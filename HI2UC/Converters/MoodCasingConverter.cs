using System;
using System.Collections.Generic;
using Nikse.SubtitleEdit.PluginLogic.Converters.Strategies;

namespace Nikse.SubtitleEdit.PluginLogic.Converters
{
    public class MoodCasingConverter : ICasingConverter
    {
        private static readonly char[] HiChars = {'(', '['};

        private IConverterStrategy ConverterStrategy { get; }

        public MoodCasingConverter(IConverterStrategy converterStrategy)
        {
            ConverterStrategy = converterStrategy;
        }

        public void Convert(IList<Paragraph> paragraphs, ConverterContext converterContext)
        {
            foreach (var paragraph in paragraphs)
            {
                var text = paragraph.Text;

                // doesn't have balanced brackets. O(2n)
                if (!HasBalancedParentheses(text))
                {
                    converterContext.AddResult(text, text, "Line contains unbalanced []/()", paragraph);
                }

                var output = MoodsToUppercase(text);

                if (!output.Equals(paragraph.Text, StringComparison.Ordinal))
                {
                    converterContext.AddResult(paragraph.Text, output, "Moods", paragraph);
                }
            }
        }

        private string RemoveInvalid(string input)
        {
            input = input.Replace("()", string.Empty);
            input = input.Replace("()", string.Empty);
            input = input.Replace("( )", string.Empty);
            return input.Replace("[ ]", string.Empty);
        }

        public string MoodsToUppercase(string text)
        {
            text = RemoveInvalid(text);

            if (!HasValidMood(text))
            {
                return text;
            }

            var openSymbolIndex = text.IndexOfAny(HiChars);
            while (openSymbolIndex >= 0)
            {
                var closingPair = GetClosingPair(text[openSymbolIndex]);
                var closeSymbolIndex = text.IndexOf(closingPair, openSymbolIndex + 1); // ] or )
                // There most be at lease one chars inside brackets.
                if (closeSymbolIndex < openSymbolIndex + 2)
                {
                    break;
                }

                var mood = text.Substring(openSymbolIndex + 1, closeSymbolIndex - (openSymbolIndex + 1));

                var moodToken = new MoodToken(mood, text[openSymbolIndex], text[closeSymbolIndex]);
                if (moodToken.IsConvertible())
                {
                    // take out the mood (including open/start symbol)
                    text = text.Remove(openSymbolIndex, closeSymbolIndex - openSymbolIndex + 1);
                    // convert, reinsert and restore wrapping token e.g: () or []
                    text = text.Insert(openSymbolIndex, moodToken.Tokenize(ConverterStrategy.Execute(mood)));
                }

                openSymbolIndex = text.IndexOfAny(HiChars, closeSymbolIndex + 1); // ( or [
            }

            return text;
        }

        private static char GetClosingPair(char openToken)
        {
            switch (openToken)
            {
                case '(': return ')';
                case '[': return ']';
                case '{': return '}';
            }

            return '\0';
        }

        public bool HasBalancedParentheses(string input)
        {
            var countParentheses = 0;
            var countBrackets = 0;
            for (var i = input.Length - 1; i >= 0; i--)
            {
                var ch = input[i];
                switch (ch)
                {
                    case '(':
                        countParentheses++;
                        break;
                    case ')':
                        countParentheses--;
                        break;
                    case '[':
                        countBrackets++;
                        break;
                    case ']':
                        countBrackets--;
                        break;
                }

                // even if you check to the end there won't be enough to balance
                if (i - countBrackets < 0 || i - countParentheses < 0)
                {
                    return false;
                }
            }

            return countBrackets == 0 && countParentheses == 0;
        }
        
        private static bool HasValidMood(string text)
        {
            if (text == null)
            {
                return false;
            }

            var idx = text.IndexOfAny(HiChars);
            return (idx >= 0 && idx + 1 < text.Length);
        }
        
        public struct MoodToken
        {
            private readonly string _moodText;
            private readonly char _startToken;
            private readonly char _endToken;

            public MoodToken(string moodText, char startToken, char endToken)
            {
                _moodText = moodText;
                _startToken = startToken;
                _endToken = endToken;
            }

            public bool IsConvertible()
            {
                if (string.IsNullOrWhiteSpace(_moodText))
                {
                    return false;
                }
                
                // skip tag inside mood
                var indexFromStart = 0;
                var len = _moodText.Length;
                
                // skip all adjacent open tags e.: <i><b>...
                while (_moodText[indexFromStart] == '<')
                {
                    var tagCloseIndex = _moodText.IndexOf('>', indexFromStart) + 1;
                    if (tagCloseIndex < indexFromStart) return true;
                    indexFromStart = tagCloseIndex; // will contains the next char after the closing
                    if (indexFromStart >= len) return false;
                }
                
                // skip all adjacent closing tags </i></i>
                var indexFromEnd = len - 1;
                while (_moodText[indexFromEnd] == '>')
                {
                    var tagOpenIndex = _moodText.LastIndexOf('<', indexFromEnd - 1) - 1;
                    if (tagOpenIndex < indexFromStart) return false;
                    indexFromEnd = tagOpenIndex;
                }

                return indexFromStart < indexFromEnd;
            }

            public string Tokenize(string input) => $"{_startToken}{input}{_endToken}";
        }
    }
}