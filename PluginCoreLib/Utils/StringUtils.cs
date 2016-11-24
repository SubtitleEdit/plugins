namespace PluginCoreLib.Utils
{
    using System;

    public static class StringUtils
    {
        public static int CountLines(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return 0;
            }
            int lineCount = 0;
            int index = input.IndexOf('\n');
            while (index >= 0)
            {
                lineCount++;
                if ((index + 1) == input.Length)
                    return lineCount;
                index = input.IndexOf('\n', index + 1);
            }
            return lineCount;
        }

        public static int CountTagInText(string text, string tag)
        {
            int count = 0;
            int index = text.IndexOf(tag, StringComparison.Ordinal);
            while (index >= 0)
            {
                count++;
                index = index + tag.Length;
                if (index >= text.Length)
                    return count;
                index = text.IndexOf(tag, index, StringComparison.Ordinal);
            }
            return count;
        }

        public static int CountTagInText(string text, char tag)
        {
            int count = 0;
            int index = text.IndexOf(tag);
            while (index >= 0)
            {
                count++;
                if ((index + 1) == text.Length)
                    return count;
                index = text.IndexOf(tag, index + 1);
            }
            return count;
        }

        public static bool IsInteger(string s)
        {
            int i;
            return int.TryParse(s, out i);
        }

    }
}