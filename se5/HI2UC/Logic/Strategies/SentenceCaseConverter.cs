using System.Globalization;

namespace SubtitleEdit.Plugins.HI2UC.Logic.Strategies;

public class SentenceCaseConverter : IConverterStrategy
{
    public string Name => "Sentence case";
    public string Example => "FOOBAR FOOBAR → Foobar foobar";

    public string Execute(string input)
    {
        input = input.ToLower(CultureInfo.CurrentCulture);

        // skip tag
        var firstLetterIndex = FindFirstLetterIndex(input);
        if (firstLetterIndex > 0)
        {
            // first part can be tag or not
            return input.Substring(0, firstLetterIndex) + input.Substring(firstLetterIndex).CapitalizeFirstLetter();
        }

        return input.CapitalizeFirstLetter();
    }

    private static int FindFirstLetterIndex(string input)
    {
        for (var i = 0; i < input.Length; i++)
        {
            var ch = input[i];
            if (TextUtils.IsOpenTag(ch))
            {
                // skip tag
                var closingPairIndex = input.IndexOf(TextUtils.GetClosingPair(ch), i + 1);
                if (closingPairIndex < i)
                {
                    return -1;
                }

                i = closingPairIndex;
            }
            else if (char.IsLetter(ch))
            {
                return i;
            }
        }

        return -1;
    }

    public override string ToString() => Name;
}
