using System;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public static class RemoveHyphens
    {
        public static string RemoveHyphenBeginningOnly(string text)
        {
            // - Get lost!
            // - Mister, I'll sing you any song...
            if (!text.Contains('-'))
            {
                return text;
            }
            // Return already removed at beginning.
            string[] lines = text.SplitToLines();
            string line = lines[0];
            string noTagLine = line;

            bool tagPresent = false;
            if (noTagLine.Length > 0 && noTagLine[0] != '-' && !char.IsLetter(noTagLine[0]))
            {
                Utilities.RemoveHtmlTags(line, true).TrimStart();
                tagPresent = !tagPresent;
            }

            if (noTagLine.StartsWith('-'))
            {
                if (tagPresent)
                {
                    int hyphenIdx = line.IndexOf('-');
                    string post = line.Substring(0, hyphenIdx);
                    line = line.Remove(0, hyphenIdx + 1).Trim('-', ' ');
                    line = post + line;
                }
                else
                {
                    line = line.Trim('-', ' ');
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
