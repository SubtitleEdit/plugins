namespace SubtitleEdit.Plugins.HI2UC;

/// <summary>Cases the speaker label before a colon - "John: Hi." becomes "JOHN: Hi.".</summary>
public class NarratorCasingConverter
{
    private static readonly char[] LineCloseChars = { '!', '?', '¿', '¡' };
    private static readonly char[] Symbols = { '.', '!', '?', ')', ']' };

    private readonly IConverterStrategy _converterStrategy;

    public NarratorCasingConverter(IConverterStrategy converterStrategy) => _converterStrategy = converterStrategy;

    public string Convert(string text)
    {
        var noTagText = TextUtils.RemoveTags(text, true).TrimEnd().TrimEnd('"');
        if (noTagText.StartsWith("http", StringComparison.Ordinal) || !text.Contains(':'))
        {
            return text;
        }

        var changed = false;
        var lines = text.SplitToLines();
        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            var noTagLine = TextUtils.RemoveTags(line, true);
            if (!noTagLine.Contains(':'))
            {
                continue;
            }

            if (IsConvertible(noTagLine, noTagLine.IndexOf(':')))
            {
                // Find index from original text.
                var colonIdx = line.IndexOf(':') + 1;
                var converted = _converterStrategy.Execute(line.Substring(0, colonIdx)) + line.Substring(colonIdx);
                if (converted != line)
                {
                    lines[i] = converted;
                    changed = true;
                }
            }
        }

        return changed ? string.Join(TextUtils.DetectNewLine(text), lines) : text;
    }

    private static bool IsConvertible(string noTagsLine, int colonIdx)
    {
        if (colonIdx <= 1 || colonIdx + 1 >= noTagsLine.Length)
        {
            return false;
        }

        var noTagCapturedText = noTagsLine.Substring(0, colonIdx + 1);
        if (string.IsNullOrWhiteSpace(noTagCapturedText))
        {
            return false;
        }

        // e.g: 12:30am... or the slash in https://
        var charAfterColon = noTagsLine[colonIdx + 1];
        if (char.IsDigit(charAfterColon) || charAfterColon == '/')
        {
            return false;
        }

        // ignore: - where it's safest. BRAN: No.
        var symbolIdx = noTagCapturedText.LastIndexOfAny(Symbols, colonIdx);
        if (symbolIdx > 0)
        {
            // text before the symbol
            var preText = noTagCapturedText.Substring(0, symbolIdx).Trim();
            // text after the symbol, without the colon
            var textAfterSymbols = noTagCapturedText.Substring(symbolIdx + 1, colonIdx - symbolIdx - 1).Trim();

            // the label after the symbol is already uppercase - the text before it is not a label
            if (textAfterSymbols.Equals(textAfterSymbols.ToUpper()) && !preText.Equals(preText.ToUpper()))
            {
                return false;
            }
        }

        // Foobar[?!] Narrator: Hello ("." is allowed: names like "Mr. Smith:")
        return !noTagCapturedText.ContainsAny(LineCloseChars) &&
               !noTagCapturedText.StartsWith("http", StringComparison.OrdinalIgnoreCase) &&
               !noTagCapturedText.EndsWith("improved by:", StringComparison.OrdinalIgnoreCase) &&
               !noTagCapturedText.EndsWith("corrected by:", StringComparison.OrdinalIgnoreCase) &&
               !noTagCapturedText.EndsWith("https:", StringComparison.OrdinalIgnoreCase) &&
               !noTagCapturedText.EndsWith("http:", StringComparison.OrdinalIgnoreCase);
    }
}
