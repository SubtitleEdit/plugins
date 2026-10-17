using System.Globalization;
using System.Text;

namespace SubtitleEdit.Plugins.HI2UC;

/// <summary>How the hearing-impaired text is cased.</summary>
public enum HiStyle
{
    UpperCase,
    UpperLowerCase,
    TitleCase,
    SentenceCase,
    LowerCase,
}

public interface IConverterStrategy
{
    string Name { get; }
    string Example { get; }
    string Execute(string input);
}

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

public class UppercaseConverter : IConverterStrategy
{
    private readonly ChunkReader _chunkReader;

    public UppercaseConverter(ChunkReader chunkReader) => _chunkReader = chunkReader;

    public string Name => "Uppercase";
    public string Example => "Foobar → FOOBAR";

    public string Execute(string input)
    {
        var sb = new StringBuilder();
        foreach (var chunk in _chunkReader.Read(input))
        {
            var content = input.Substring(chunk.Start, chunk.End - chunk.Start);
            sb.Append(chunk.IsTag ? content : content.ToUpper(CultureInfo.CurrentCulture));
        }

        return sb.ToString();
    }

    public override string ToString() => Name;
}

public class UpperLowercase : IConverterStrategy
{
    public string Name => "Toggle case";
    public string Example => "Foobar → FoObAr";

    public string Execute(string input)
    {
        var buffer = new char[input.Length];
        var len = input.Length;
        var writeTrack = 0;
        var caseState = true;
        for (var i = 0; i < len; i++)
        {
            var ch = input[i];
            // skip tags
            if (TextUtils.IsOpenTag(ch))
            {
                var closingPair = TextUtils.GetClosingPair(ch);
                // look for closing tag
                while (ch != closingPair)
                {
                    buffer[writeTrack++] = ch;
                    if (++i == len)
                    {
                        return input;
                    }

                    ch = input[i];
                }
            }

            if (!char.IsLetter(ch))
            {
                buffer[writeTrack++] = ch;
            }
            else
            {
                buffer[writeTrack++] = caseState ? char.ToUpper(ch) : char.ToLower(ch);
                caseState = !caseState;
            }
        }

        return new string(buffer, 0, writeTrack);
    }

    public override string ToString() => Name;
}

public class TitleWordsConverter : IConverterStrategy
{
    private static readonly char[] WordSplitChars = { ' ' };
    private readonly ChunkReader _chunkReader;

    public TitleWordsConverter(ChunkReader chunkReader) => _chunkReader = chunkReader;

    public string Name => "Title case";
    public string Example => "foobar foobar → Foobar Foobar";

    public string Execute(string input)
    {
        var sb = new StringBuilder();
        foreach (var chunk in _chunkReader.Read(input))
        {
            var content = input.Substring(chunk.Start, chunk.End - chunk.Start);
            sb.Append(chunk.IsTag
                ? content
                : string.Join(" ", content.Split(WordSplitChars).Select(word => word.CapitalizeFirstLetter())));
        }

        return sb.ToString();
    }

    public override string ToString() => Name;
}

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

public class LowercaseConverter : IConverterStrategy
{
    public string Name => "Lowercase";
    public string Example => "Foobar → foobar";

    public string Execute(string input) => input.ToLower(CultureInfo.CurrentCulture);

    public override string ToString() => Name;
}
