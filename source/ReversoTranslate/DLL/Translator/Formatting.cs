using System;
using System.Text;
using SubtitleEdit.Logic;

namespace SubtitleEdit.Translator
{
    public class Formatting
    {
        public bool Italic { get; set; }
        public bool ItalicTwoLines { get; set; }
        public string StartTags { get; set; }
        public bool AutoBreak { get; set; }
        public bool SquareBrackets { get; set; }

        public string SetTagsAndReturnTrimmed(string text, string source)
        {
            text = text.Trim();
            text = text.Replace("'", string.Empty);
            text = text.Replace("\"", string.Empty);

            // SSA/ASS tags
            if (text.StartsWith("{\\"))
            {
                var endIndex = text.IndexOf('}');
                if (endIndex > 0)
                {
                    StartTags = text.Substring(0, endIndex + 1);
                    text = text.Remove(0, endIndex + 1).Trim();
                }
            }

            // Italic tags
            if (text.StartsWith("<i>", StringComparison.Ordinal) && text.EndsWith("</i>", StringComparison.Ordinal) && text.Contains("</i>" + Environment.NewLine + "<i>") && Utilities.GetNumberOfLines(text) == 2 && Utilities.CountTagInText(text, "<i>") == 2)
            {
                ItalicTwoLines = true;
                text = HtmlUtil.RemoveOpenCloseTags(text, HtmlUtil.TagItalic);
            }
            else if (text.StartsWith("<i>", StringComparison.Ordinal) && text.EndsWith("</i>", StringComparison.Ordinal) && Utilities.CountTagInText(text, "<i>") == 1)
            {
                Italic = true;
                text = text.Substring(3, text.Length - 7);
            }

            // Un-break line
            var lines = HtmlUtil.RemoveHtmlTags(text).SplitToLines();
            if (lines.Length == 2 && !string.IsNullOrEmpty(lines[0]) && !string.IsNullOrEmpty(lines[1]) &&
                char.IsLetterOrDigit(lines[0][lines[0].Length - 1]) &&
                char.IsLower(lines[1][0]))
            {
                text = text.Replace(Environment.NewLine, " ").Replace("  ", " ");
                AutoBreak = true;
            }

            if (text.StartsWith("[") && text.EndsWith("]"))
            {
                SquareBrackets = true;
                text = text.TrimStart('[');
                text = text.TrimEnd(']');
            }

            text = Utilities.RemoveHtmlTags(text);
            return text.Trim();
        }

        public string ReAddFormatting(string text)
        {
            // Auto-break line
            if (AutoBreak)
            {
                text = Utilities.AutoBreakLine(text);
            }

            if (SquareBrackets)
            {
                text = "[" + text.Trim() + "]";
            }

            // Italic tags
            if (ItalicTwoLines)
            {
                var sb = new StringBuilder();
                foreach (var line in text.SplitToLines())
                {
                    sb.AppendLine("<i>" + line + "</i>");
                }
                text = sb.ToString().Trim();
            }
            else if (Italic)
            {
                text = "<i>" + text + "</i>";
            }

            // SSA/ASS tags
            text = StartTags + text;

            return text;
        }

    }
}
