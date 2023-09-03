using System.Collections.Generic;
using System.Linq;
using Nikse.SubtitleEdit.PluginLogic.UnbreakLine;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public static class ParagraphExtensions
    {
        public static bool IsUnbreakable(this SmartParagraph paragraph, int maxSingleLineLength)
        {
            if (!paragraph.IsMultiLined)
            {
                return false;
            }

            if (paragraph.Lines.Any(line => line.Content.Length >= maxSingleLineLength))
            {
                return false;
            }

            return true;
        }

        public static IEnumerable<SmartParagraph> ToSmartParagraphs(this IEnumerable<Paragraph> paragraphs) =>
            paragraphs.Select(p => new SmartParagraph(p));
    }
}