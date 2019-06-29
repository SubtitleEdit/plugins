using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public class NarratorArtist : Artist
    {
        private static readonly Regex _regexColonNonWord = new Regex(":\\B", RegexOptions.Compiled | RegexOptions.ExplicitCapture);

        /// <summary>
        /// This regex pattern is written to not capture characters inside html tags
        /// </summary>
        private static readonly Regex _regexFirtCharNotTag = new Regex("(?<!<)\\w", RegexOptions.Compiled | RegexOptions.ExplicitCapture);

        /// <summary>
        /// Narrator ignore words
        /// </summary>
        private readonly string[] _ignoreWords = { "by", "http", "https", };

        private readonly Palette _palette;

        public NarratorArtist(Palette palette)
        {
            _palette = palette;
        }

        public override void Paint(Subtitle subtitle)
        {
            IList<Paragraph> paragraphs = subtitle.Paragraphs;
            for (int i = paragraphs.Count - 1; i >= 0; i--)
            {
                var p = paragraphs[i];
                string text = p.Text;
                if (_regexColonNonWord.IsMatch(text))
                {
                    text = ProcessText(text);
                }
                if (text.Length != p.Text.Length)
                {
                    p.Text = text;
                }
            }
        }

        private string ProcessText(string text)
        {
            char[] trimChars = { '"', '\\' };
            string[] lines = text.SplitToLines();
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];

                //TODO: if text contains 2 hearing text
                string lineNoTags = HtmlUtils.RemoveTags(line, true).TrimEnd(trimChars).TrimEnd();

                if (!ShouldApplyColor(lineNoTags, isFirstLine: i == 0))
                {
                    continue;
                }

                //- MAN: Baby, I put it right over there.
                //- JUNE: No, you did not.
                int colonIdx = line.IndexOf(':');

                string preText = line.Substring(0, colonIdx);

                string firstChr = _regexFirtCharNotTag.Match(preText).Value;
                if (string.IsNullOrEmpty(firstChr))
                {
                    continue;
                }

                int idxFirstChar = preText.IndexOf(firstChr, StringComparison.Ordinal);

                // get only text that should be colored
                string narrator = preText.Substring(idxFirstChar, colonIdx - idxFirstChar).Trim();
                narrator = ApplyColor(_palette.Color, narrator);

                preText = preText.Remove(idxFirstChar, colonIdx - idxFirstChar);
                preText = preText.Insert(idxFirstChar, narrator);

                line = line.Remove(0, colonIdx);
                line = line.Insert(0, preText);

                lines[i] = line;

            }
            // re-construct text
            return string.Join(Environment.NewLine, lines);
        }

        private bool ShouldApplyColor(string textNoTags, bool isFirstLine)
        {
            int colonIdx = textNoTags.IndexOf(':');

            // too short
            if (colonIdx <= 1)
            {
                return false;
            }

            // (foobar) foobar: hello world!
            string pre = textNoTags.Substring(0, colonIdx).TrimStart();
            if (pre.ContainsAny(new[] { ')', ']' }))
            {
                return false;
            }

            // ignore hour 00:00 a.m
            if (colonIdx + 1 < textNoTags.Length && char.IsDigit(textNoTags[colonIdx + 1]))
            {
                return false;
            }

            // only allow if ':' is in first line's last character
            if (!isFirstLine && colonIdx + 1 == textNoTags.Length)
            {
                return false;
            }

            // Foobar:; Hello[.?!])] Foobar:
            string narrator = textNoTags.Substring(0, colonIdx + 1);

            // e.g; Correct by: Ivandro Ismael
            foreach (var ignoreWord in _ignoreWords)
            {
                if (narrator.Contains(ignoreWord, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
