namespace SubtitleEdit.Plugins.HI2UC;

/// <summary>Cases the text inside (...) and [...] - sounds and moods like "(door slams)".</summary>
public class MoodCasingConverter
{
    private static readonly char[] MoodChars = { '(', '[' };

    private readonly IConverterStrategy _converterStrategy;

    public MoodCasingConverter(IConverterStrategy converterStrategy) => _converterStrategy = converterStrategy;

    public string Convert(string text)
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
            // There must be at least one char inside the brackets.
            if (closeSymbolIndex < openSymbolIndex + 2)
            {
                break;
            }

            var mood = text.Substring(openSymbolIndex + 1, closeSymbolIndex - (openSymbolIndex + 1));
            var moodToken = new Mood(mood, text[openSymbolIndex], text[closeSymbolIndex]);
            if (moodToken.IsConvertible())
            {
                // take out the mood (including open/close symbol), convert, reinsert and restore the wrapping () or []
                text = text.Remove(openSymbolIndex, closeSymbolIndex - openSymbolIndex + 1);
                text = text.Insert(openSymbolIndex, moodToken.Tokenize(_converterStrategy.Execute(mood)));
            }

            openSymbolIndex = text.IndexOfAny(MoodChars, closeSymbolIndex + 1); // ( or [
        }

        return text;
    }

    public static bool HasBalancedParentheses(string input)
    {
        var countParentheses = 0;
        var countBrackets = 0;
        for (var i = input.Length - 1; i >= 0; i--)
        {
            switch (input[i])
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

    private static string RemoveInvalidParentheses(string input) =>
        input.Replace("()", string.Empty).Replace("[]", string.Empty).Replace("( )", string.Empty).Replace("[ ]", string.Empty);

    private static char GetClosingPair(char openToken) => openToken switch
    {
        '(' => ')',
        '[' => ']',
        '{' => '}',
        _ => '\0',
    };

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

        if (lastCharIndex < 0)
        {
            return false;
        }

        // open parenthesis/bracket followed by a tag
        if (moodStartIndex + 1 < text.Length && text[moodStartIndex + 1] == '<')
        {
            return false;
        }

        // ensure the mood char is not the last character
        return moodStartIndex + 1 < text.Length;
    }

    private readonly struct Mood
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

            var indexFromStart = 0;
            var len = _content.Length;

            // skip all adjacent open tags e.g. <i><b>...
            while (_content[indexFromStart] == '<')
            {
                var tagCloseIndex = _content.IndexOf('>', indexFromStart) + 1;
                if (tagCloseIndex <= indexFromStart)
                {
                    return true;
                }

                indexFromStart = tagCloseIndex; // the next char after the closing
                if (indexFromStart >= len)
                {
                    return false;
                }
            }

            // skip all adjacent closing tags </i></i>
            var indexFromEnd = len - 1;
            while (_content[indexFromEnd] == '>')
            {
                var tagOpenIndex = _content.LastIndexOf('<', indexFromEnd - 1) - 1;
                if (tagOpenIndex < 0 || tagOpenIndex <= indexFromStart)
                {
                    return false;
                }

                indexFromEnd = tagOpenIndex;
            }

            return indexFromStart < indexFromEnd;
        }

        public string Tokenize(string input) => $"{_startToken}{input}{_endToken}";
    }
}
