using System.Globalization;
using Nikse.SubtitleEdit.PluginLogic.Common;

namespace Nikse.SubtitleEdit.PluginLogic.Converters.Strategies;

public static class ExtensionMethods
{
    public static string CapitalizeFirstLetter(this string s, CultureInfo ci = null)
    {
        var si = new StringInfo(s);
        if (ci == null)
        {
            ci = CultureInfo.CurrentCulture;
            // todo: use instead?
            // ci.TextInfo.ToTitleCase()
        }

        if (si.LengthInTextElements > 0)
        {
            s = si.SubstringByTextElements(0, 1).ToUpper(ci);
        }

        if (si.LengthInTextElements > 1)
        {
            s += si.SubstringByTextElements(1).ToLower();
        }

        return s;
    }
}

public class SentenceCaseConverter : IConverterStrategy
{
    public string Name => "Sentence case";
    public string Example => "Foobar Foobar => Foobar foobar";

    public string Execute(string input)
    {
        input = Normalize(input);

        // skip tag
        var firstLetterIndex = FindFirstLetterIndex(input);
        if (firstLetterIndex > 0)
        {
            // first part can be tag or not
            return input.Substring(0, firstLetterIndex) + input.Substring(firstLetterIndex).CapitalizeFirstLetter();
        }

        return input.CapitalizeFirstLetter();
    }

    private static string Normalize(string input) => input.ToLower(CultureInfo.CurrentCulture);

    private static int FindFirstLetterIndex(string input)
    {
        const int invalidIndex = -1;

        // invalid input
        if (input.Length == 0) return invalidIndex;
        var len = input.Length;

        for (int i = 0; i < len; i++)
        {
            var ch = input[i];

            // tag?
            if (TagUtils.IsOpenTag(ch))
            {
                // skip tag
                var closingPair = TagUtils.GetClosingPair(ch);
                var closingPairIndex = input.IndexOf(closingPair, i + 1);
                if (closingPairIndex < i) return invalidIndex;
                i = closingPairIndex;
            }
            else if (char.IsLetter(ch))
            {
                return i;
            }
        }

        return invalidIndex;
    }

    public override string ToString() => Name;
}