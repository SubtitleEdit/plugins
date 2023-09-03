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

        public RemoveLineBreak(UnBreakConfigs configs)
        {
            _configs = configs;
        }

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

        private string Unbreak(SmartParagraph smartParagraph)
        {
            // var noTagText = HtmlUtils.RemoveTags(smartParagraph, true);
            // smartParagraph = smartParagraph.FixExtraSpaces().Trim();
            var lines = smartParagraph.Lines.ToArray();

            // dialog
            if (_configs.SkipDialogs && lines.Any(line => line.IsDialog))
            {
                return smartParagraph.Text;
            }

            // mood
            if (_configs.SkipMoods && lines.Any(line => line.HasMood))
            {
                return smartParagraph.Text;
            }

            // narrator
            // if (_configs.SkipNarrator && _regexNarrator.IsMatch(noTagText))
            if (_configs.SkipNarrator && lines.Any(line => line.HasNarrator))
            {
                return smartParagraph.Text;
            }

            return StringUtils.UnbreakLine(smartParagraph.Paragraph.Text);
        }
    }
}
