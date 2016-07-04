using Nikse.SubtitleEdit.PluginLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public static class RemoveHyphens
    {
        private static readonly string[] _lineCloseStrings = { "! -", "? -", ". -" };

        public static string RemoveHyphenBeginningOnly(string text)
        {
            if (!text.Contains('-'))
            {
                return text;
            }
            // Return already removed at beginning.
            string[] lines = text.SplitToLines();
            string line = lines[0];
            string noTagLine = Utilities.RemoveHtmlTags(line, true).TrimStart();

            int hyphensCount = Utilities.CountTagInText(line, '-');
            if (hyphensCount == 1 && noTagLine.StartsWith('-'))
            {
                int hyphenIndex = line.IndexOf('-');
                line = line.Remove(hyphenIndex, 1);
            }
            else
            {
                // Remove first index if next index is after: ./?/!.
                if (noTagLine.StartsWith('-'))
                {
                    int nextHyphenIdx = noTagLine.IndexOf('-', 1);
                    if ((nextHyphenIdx > 2 && line[nextHyphenIdx - 2] == '!' || line[nextHyphenIdx - 2] == '?' || line[nextHyphenIdx - 2] == '.'))
                    {
                        int hyphenIdx = line.IndexOf('-');
                        line = line.Remove(hyphenIdx, 1);
                        // TODO: Remove white-space afte hyphen.
                    }
                }
            }

            if (line.Length != lines[0].Length)
            {
                lines[0] = line;
            }

            return string.Join(Environment.NewLine, lines);
        }

        public static string RemoveAllHyphens(string text)
        {
            if (!text.Contains('-'))
            {
                return text;
            }

            string[] lines = text.SplitToLines();
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                // string noTagLine = Utilities.RemoveHtmlTags(line, true);
                int hyphenStartIdx = line.IndexOf('-');
                while (Utilities.RemoveHtmlTags(line, true).StartsWith('-') || (hyphenStartIdx > 2 && (line[hyphenStartIdx - 2] == '!' || line[hyphenStartIdx - 2] == '?' || line[hyphenStartIdx - 2] == '.')))
                {
                    line = line.Remove(hyphenStartIdx, 1);
                    hyphenStartIdx = line.IndexOf('-', hyphenStartIdx);
                    // TODO: Handle if hyphenStartIdx didn't match and there are still hyphen that should be removed.
                }
                if (line.Length != lines[i].Length)
                    lines[i] = line;
            }
            return string.Join(Environment.NewLine, lines);
        }
    }
}
