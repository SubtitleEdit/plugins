using System;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public static class RemoveHyphens
    {
        public static string RemoveHyphenBeginningOnly(string text)
        {
            // - Get lost!
            // - Mister, I'll sing you any song...
            if (!text.Contains('-') || !Utilities.RemoveHtmlTags(text, true).TrimStart().StartsWith("-", StringComparison.Ordinal))
                return text;
            var lines = text.SplitToLines();
            if (lines.Length != 2)
                return text;
            if (!lines[1].Contains(':') && !Utilities.RemoveHtmlTags(lines[1], true).TrimStart().StartsWith("-", StringComparison.Ordinal))
                return text;

            int hyphenIdx = text.IndexOf('-');
            text = text.Remove(hyphenIdx, 1);
            if (hyphenIdx > 0 && hyphenIdx < text.Length + 1 && " >}".Contains(text[hyphenIdx - 1].ToString()) && text[hyphenIdx] == ' ')
                text = text.Remove(hyphenIdx, 1);
            if (hyphenIdx > 0 && hyphenIdx < text.Length + 2 && " >}".Contains(text[hyphenIdx].ToString()) && text[hyphenIdx + 1] == ' ')
                text = text.Remove(hyphenIdx + 1, 1);
            return text.TrimStart();
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
                int hyphenIndex = line.IndexOf('-');
                while (hyphenIndex >= 0)
                {
                    if (hyphenIndex == 0)
                    {
                        line = line.TrimStart('-', ' ');
                    }
                    else if (hyphenIndex > 2 && (line[hyphenIndex - 2] == '!' || line[hyphenIndex - 2] == '?' || line[hyphenIndex - 2] == '.'))
                    {
                        line = line.Remove(hyphenIndex, 1);
                    }
                    hyphenIndex = line.IndexOf('-', hyphenIndex + 1);
                }
                if (line.Length != lines[i].Length)
                {
                    lines[i] = line.Replace("  ", " ");
                }
            }
            return string.Join(Environment.NewLine, lines);
        }
    }
}
