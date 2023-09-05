using System.Collections.Generic;
using System.Linq;

// using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.PluginLogic.UnbreakLine
{
    /// <summary>
    /// Line break remover command
    /// </summary>
    internal class RemoveLineBreak
    {
        // private readonly Regex _regexNarrator = new Regex(":\\B", RegexOptions.Compiled);
        private readonly UnBreakConfigs _configs;
        // private readonly char[] _moodChars = { '(', '[' };

        public RemoveLineBreak(UnBreakConfigs configs) => _configs = configs;

        public ICollection<RemoveLineBreakResult> Remove(IList<Paragraph> paragraphs)
        {
            var result = new List<RemoveLineBreakResult>();
            foreach (var paragraph in paragraphs.ToSmartParagraphs())
            {
                if (!paragraph.IsUnbreakable(_configs.MaxLineLength))
                {
                    continue;
                }
                
                var text = Unbreak(paragraph);
                if (!text.Equals(paragraph.Paragraph.Text))
                {
                    result.Add(new RemoveLineBreakResult()
                    {
                        Paragraph = paragraph.Paragraph,
                        AfterText = text
                    });
                }
            }

            return result;
        }

        // ReSharper disable once IdentifierTypo
        private string Unbreak(SmartParagraph paragraph)
        {
            // dialog
            if (_configs.SkipDialogs && paragraph.Lines.Any(line => line.IsDialog))
            {
                return paragraph.Text;
            }

            // mood
            if (_configs.SkipMoods && paragraph.Lines.Any(line => line.HasMood))
            {
                return paragraph.Text;
            }

            // narrator
            if (_configs.SkipNarrator && paragraph.Lines.Any(line => line.HasNarrator))
            {
                return paragraph.Text;
            }

            return StringUtils.UnbreakLine(paragraph.Paragraph.Text);
        }
    }
}
