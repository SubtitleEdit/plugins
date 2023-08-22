using System;
using System.Collections.Generic;
using Nikse.SubtitleEdit.PluginLogic.Converters.Strategies;

namespace Nikse.SubtitleEdit.PluginLogic.Converters
{
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

            if (noTagText.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                return text;
            }

            // Skip single line that ends with ':'.
            if (!noTagText.Contains(':'))
            {
                return text;
            }

            // Lena:
            // A ring?!
            // var newLineIdx = noTagText.IndexOf(Environment.NewLine, StringComparison.Ordinal);
            // todo: handle
            //if (!Config.SingleLineNarrator && index + 1 == newLineIdx)
            //{
            //    return text;
            //}

            var lines = text.SplitToLines();
            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                var noTagLine = HtmlUtils.RemoveTags(line, true);

                var colonIdx = noTagLine.IndexOf(':');
                if (colonIdx < 1)
                {
                    continue;
                }

                // Only allow colon at last position if it's 1st line.
                if (colonIdx + 1 == noTagLine.Length)
                {
                    continue;
                }

                if (IsValid(noTagLine, colonIdx))
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

        private static bool IsValid(string noTagsLine, int colonIdx)
        {
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
}