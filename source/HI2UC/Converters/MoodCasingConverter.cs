using System;
using System.Collections.Generic;
using System.Diagnostics;
using Nikse.SubtitleEdit.PluginLogic.Converters.Strategies;

namespace Nikse.SubtitleEdit.PluginLogic.Converters;

public class MoodCasingConverter : ICasingConverter
{
    private static readonly char[] MoodChars = {'(', '['};

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
        
    public string MoodsToUppercase(string text)
    {
        text = RemoveInvalidParentheses(text);

        if (!HasValidMood(text))
        {
            return text;
        }

        var openSymbolIndex = text.IndexOfAny(MoodChars);
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

            // mood token
            var moodToken = new Mood(mood, text[openSymbolIndex], text[closeSymbolIndex]);
            if (moodToken.IsConvertible())
            {
                // take out the mood (including open/start symbol)
                text = text.Remove(openSymbolIndex, closeSymbolIndex - openSymbolIndex + 1);
                // convert, reinsert and restore wrapping token e.g: () or []
                text = text.Insert(openSymbolIndex, moodToken.Tokenize(ConverterStrategy.Execute(mood)));
            }

            openSymbolIndex = text.IndexOfAny(MoodChars, closeSymbolIndex + 1); // ( or [
        }

        return text;
    }
        
    // private Mood NewMood(string text, int index, int len)
        
    private string RemoveInvalidParentheses(string input)
    {
        input = input.Replace("()", string.Empty);
        input = input.Replace("[]", string.Empty);
        input = input.Replace("( )", string.Empty);
        return input.Replace("[ ]", string.Empty);
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
        if (string.IsNullOrEmpty(text))
        {
            return false;
        }

        var moodStartIndex = text.IndexOfAny(MoodChars);
        if (moodStartIndex < 0)
        {
            return false;
        }

        // skip closing html tag at end
        var lastCharIndex = text.Length - 1;
        while (lastCharIndex > 0 && text[lastCharIndex] == '>')
        {
            lastCharIndex = text.LastIndexOf('<', lastCharIndex - 1) - 1;
        }
        if (lastCharIndex < 0) return false;

        // check if open parentheses/bracket is at the end of the text
        if (moodStartIndex + 1 < text.Length && text[moodStartIndex + 1] == '<')
        {
            return false;
        }

        // ensure mood char is not the last character in sentense
        return moodStartIndex + 1 < text.Length;
    }
        
    public struct Mood
    {
        private readonly string _content;
        private readonly char _startToken;
        private readonly char _endToken;

        public Mood(string content, char startToken, char endToken)
        {
            _content = content;
            _startToken = startToken;
            _endToken = endToken;
        }

        public bool IsConvertible()
        {
            if (string.IsNullOrWhiteSpace(_content))
            {
                return false;
            }
                
            // skip tag inside mood
            var indexFromStart = 0;
            var len = _content.Length;
                
            // skip all adjacent open tags e.: <i><b>...
            while (_content[indexFromStart] == '<')
            {
                var tagCloseIndex = _content.IndexOf('>', indexFromStart) + 1;
                if (tagCloseIndex <= indexFromStart) return true;
                indexFromStart = tagCloseIndex; // will contains the next char after the closing
                if (indexFromStart >= len) return false;
            }
                
            // skip all adjacent closing tags </i></i>
            var indexFromEnd = len - 1;
            while (_content[indexFromEnd] == '>')
            {
                var tagOpenIndex = _content.LastIndexOf('<', indexFromEnd - 1) - 1;
                if (tagOpenIndex < 0 || tagOpenIndex <= indexFromStart) return false;
                indexFromEnd = tagOpenIndex;
            }
                
            return indexFromStart < indexFromEnd;
        }

        public string Tokenize(string input) => $"{_startToken}{input}{_endToken}";
    }
}