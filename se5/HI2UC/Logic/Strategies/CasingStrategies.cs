using System.Globalization;

namespace SubtitleEdit.Plugins.HI2UC.Logic.Strategies;

public static class CasingStrategies
{
    private static readonly ChunkReader ChunkReader = new();

    public static IConverterStrategy Create(HiStyle style) => style switch
    {
        HiStyle.UpperLowerCase => new UpperLowercase(),
        HiStyle.TitleCase => new TitleWordsConverter(ChunkReader),
        HiStyle.SentenceCase => new SentenceCaseConverter(),
        HiStyle.LowerCase => new LowercaseConverter(),
        _ => new UppercaseConverter(ChunkReader),
    };

    public static string CapitalizeFirstLetter(this string s, CultureInfo? ci = null)
    {
        var si = new StringInfo(s);
        ci ??= CultureInfo.CurrentCulture;

        if (si.LengthInTextElements > 0)
        {
            s = si.SubstringByTextElements(0, 1).ToUpper(ci);
        }

        if (si.LengthInTextElements > 1)
        {
            s += si.SubstringByTextElements(1).ToLower(ci);
        }

        return s;
    }
}
