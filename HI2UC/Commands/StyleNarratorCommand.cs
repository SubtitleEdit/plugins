using System;
using System.Collections.Generic;
using Nikse.SubtitleEdit.PluginLogic.Strategies;

namespace Nikse.SubtitleEdit.PluginLogic.Commands
{
    public class StyleNarratorCommand : ICommand
    {
        private static readonly char[] LineCloseChars = {'!', '?', '¿', '¡'};
        private static readonly char[] Symbols = {'.', '!', '?', ')', ']'};

        public IStrategy Strategy { get; }

        public StyleNarratorCommand(IStrategy strategy)
        {
            Strategy = strategy;
        }

        public void Convert(IList<Paragraph> paragraph, IController controller)
        {
            foreach (var p in paragraph)
            {
                string input = p.Text;
                string output = NarratorToUppercase(p.Text);
                if (!input.Equals(output, StringComparison.Ordinal))
                {
                    controller.AddResult(input, output, "Narrator converted", p);
                }
            }
        }

        public string NarratorToUppercase(string text)
        {
            string noTagText = HtmlUtils.RemoveTags(text, true).TrimEnd().TrimEnd('"');

            if (noTagText.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                return text;
            }

            // Skip single line that ends with ':'.
            int index = noTagText.IndexOf(':');
            if (index < 0)
            {
                return text;
            }

            // Lena:
            // A ring?!
            int newLineIdx = noTagText.IndexOf(Environment.NewLine, StringComparison.Ordinal);

            // todo: handle
            //if (!Config.SingleLineNarrator && index + 1 == newLineIdx)
            //{
            //    return text;
            //}

            string[] lines = text.SplitToLines();
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                string noTagLine = HtmlUtils.RemoveTags(line, true);

                int colonIdx = noTagLine.IndexOf(':');
                if (colonIdx < 1)
                {
                    continue;
                }

                // Only allow colon at last position if it's 1st line.
                if (colonIdx + 1 == noTagLine.Length)
                {
                    continue;
                }

                if (IsQualifiedNarrator(noTagLine, colonIdx))
                {
                    // Find index from original text.
                    colonIdx = line.IndexOf(':') + 1;
                    string preText = line.Substring(0, colonIdx);
                    preText = Strategy.Execute(preText);
                    lines[i] = preText + line.Substring(colonIdx);
                }
            }

            return string.Join(Environment.NewLine, lines);
        }

        private string ConvertNarrators(string text)
        {
            int j = 0;
            for (int i = text.Length - 1; i >= 0; i--)
            {
                char ch = text[i];
                if (ch == ':')
                {
                    // check if doesn't precede a digit
                    if (i + 1 < text.Length && char.IsDigit(text[i + 1]) == false)
                    {
                        j = i;
                    }
                }
                // skip narrators inside brackets
                else if (ch == '(' || ch == '[' && j > i)
                {
                    j = -1;
                }
                // valid narrator found
                else if (j > 0 && (i == 0 || (ch == '.' || ch == '?' || ch == '!')))
                {
                    // foobar. narattor: hello world!

                    int k = i + 1;
                    while (k < j && text[k] == ' ')
                    {
                        k++;
                    }

                    string textFromRange = Strategy.Execute(text.Substring(k, j - k));
                    text = text.Remove(k, j - k).Insert(k, textFromRange);
                }
            }

            return text;
        }

        private static bool IsQualifiedNarrator(string noTagsLine, int colonIdx)
        {
            string noTagCapturedText = noTagsLine.Substring(0, colonIdx + 1);
            if (string.IsNullOrWhiteSpace(noTagCapturedText))
            {
                return false;
            }

            // e.g: 12:30am...
            if (colonIdx + 1 < noTagsLine.Length)
            {
                char charAfterColon = noTagsLine[colonIdx + 1];
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
            int symbolIdx = noTagCapturedText.LastIndexOfAny(Symbols, colonIdx);
            if (symbolIdx > 0)
            {
                // text before symbol
                string preText = noTagCapturedText.Substring(0, symbolIdx).Trim();
                // text after symbols exclude colon
                string textAfterSymbols = noTagCapturedText.Substring(symbolIdx + 1, colonIdx - symbolIdx - 1).Trim();

                // post symbol is uppercase - pre unnecessary
                if (textAfterSymbols.Equals(textAfterSymbols.ToUpper()) && preText.Equals(preText.ToUpper()) == false)
                {
                    return false;
                }
            }

            // Foobar[?!] Narrator: Hello (Note: not really sure if "." (dot) should be include since there are names
            // that are prefixed with Mr. Ivandro Ismael)
            return !noTagCapturedText.ContainsAny(LineCloseChars) &&
                   (!noTagCapturedText.StartsWith("http", StringComparison.OrdinalIgnoreCase) &&
                    !noTagCapturedText.EndsWith("improved by", StringComparison.OrdinalIgnoreCase) &&
                    !noTagCapturedText.EndsWith("corrected by", StringComparison.OrdinalIgnoreCase) &&
                    !noTagCapturedText.EndsWith("https", StringComparison.OrdinalIgnoreCase) &&
                    !noTagCapturedText.EndsWith("http", StringComparison.OrdinalIgnoreCase));
        }
    }
}