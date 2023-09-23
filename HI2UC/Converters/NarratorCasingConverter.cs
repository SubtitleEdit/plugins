using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Nikse.SubtitleEdit.PluginLogic.Converters.Strategies;

namespace Nikse.SubtitleEdit.PluginLogic.Converters;

public class NarratorCasingConverter : ICasingConverter
{
    private static readonly char[] LineCloseChars = {'!', '?', '¿', '¡'};
    private static readonly char[] Symbols = {'.', '!', '?', ')', ']'};

    public IConverterStrategy ConverterStrategy { get; }

    public NarratorCasingConverter(IConverterStrategy converterStrategy)
    {
        ConverterStrategy = converterStrategy;
    }

    public void Convert(IList<Paragraph> paragraphs, ConverterContext converterContext)
    {
        foreach (var p in paragraphs)
        {
            var input = p.Text;
            var output = NarratorToUppercase(p.Text);
            if (!input.Equals(output, StringComparison.Ordinal))
            {
                converterContext.AddResult(input, output, "Narrator converted", p);
            }
        }
    }

    public string NarratorToUppercase(string text)
    {
        var noTagText = HtmlUtils.RemoveTags(text, true).TrimEnd().TrimEnd('"');

        if (IsHttp(noTagText))
        {
            return text;
        }

        // Skip single line that ends with ':'.
        if (!HasColon(text))
        {
            return text;
        }

        var lines = text.SplitToLines();
        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            var noTagLine = HtmlUtils.RemoveTags(line, true);

            if (!HasColon(noTagLine))
            {
                continue;
            }

            var colonIdx = noTagLine.IndexOf(':');

            if (IsConvertible(noTagLine, colonIdx))
            {
                // Find index from original text.
                colonIdx = line.IndexOf(':') + 1;
                // skip italic start tag
                var preText = line.Substring(0, colonIdx);
                preText = ConverterStrategy.Execute(preText);
                lines[i] = preText + line.Substring(colonIdx);
            }
        }

        return string.Join(Environment.NewLine, lines);
    }

    private bool IsHttp(string text) => text.StartsWith("http");
    
    private bool HasColon(string text) => text.Contains(':');

    private static bool IsColonAtValidPosition(string text, int colonIndex) => colonIndex > 1 && colonIndex + 1 < text.Length;

    private static bool IsConvertible(string noTagsLine, int colonIdx)
    {
        if (!IsColonAtValidPosition(noTagsLine, colonIdx))
        {
            return false;
        }
        
        var noTagCapturedText = noTagsLine.Substring(0, colonIdx + 1);
        if (string.IsNullOrWhiteSpace(noTagCapturedText))
        {
            return false;
        }

        // e.g: 12:30am...
        if (colonIdx + 1 < noTagsLine.Length)
        {
            var charAfterColon = noTagsLine[colonIdx + 1];
            if (char.IsDigit(charAfterColon))
            {
                return false;
            }

            // slash after https://
            if (charAfterColon == '/')
            {
                return false;
            }
        }

        // ignore: - where it's safest. BRAN: No.
        var symbolIdx = noTagCapturedText.LastIndexOfAny(Symbols, colonIdx);
        if (symbolIdx > 0)
        {
            // text before symbol
            var preText = noTagCapturedText.Substring(0, symbolIdx).Trim();
            // text after symbols exclude colon
            var textAfterSymbols = noTagCapturedText.Substring(symbolIdx + 1, colonIdx - symbolIdx - 1).Trim();

            // post symbol is uppercase - pre unnecessary
            if (textAfterSymbols.Equals(textAfterSymbols.ToUpper()) && preText.Equals(preText.ToUpper()) == false)
            {
                return false;
            }
        }

        // Foobar[?!] Narrator: Hello (Note: not really sure if "." (dot) should be include since there are names
        // that are prefixed with Mr. Ivandro Ismael)
        return !noTagCapturedText.ContainsAny(LineCloseChars) &&
               !noTagCapturedText.StartsWith("http", StringComparison.OrdinalIgnoreCase) &&
               !noTagCapturedText.EndsWith("improved by", StringComparison.OrdinalIgnoreCase) &&
               !noTagCapturedText.EndsWith("corrected by", StringComparison.OrdinalIgnoreCase) &&
               !noTagCapturedText.EndsWith("https", StringComparison.OrdinalIgnoreCase) &&
               !noTagCapturedText.EndsWith("http", StringComparison.OrdinalIgnoreCase);
    }
}