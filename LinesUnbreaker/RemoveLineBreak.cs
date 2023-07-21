using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Nikse.SubtitleEdit.PluginLogic.Models;

namespace Nikse.SubtitleEdit.PluginLogic
{
    internal class RemoveLineBreak
    {
        private readonly Regex _regexNarrator = new Regex(":\\B", RegexOptions.Compiled);
        private readonly UnBreakConfigs _configs;
        private readonly char[] _moodChars = { '(', '[' };

        public RemoveLineBreak(UnBreakConfigs configs)
        {
            _configs = configs;
        }

        public ICollection<RemoveLineBreakResult> Remove(IList<Paragraph> paragraphs)
        {
            var result = new List<RemoveLineBreakResult>();
            foreach (var paragraph in paragraphs)
            {
                if (ShouldRemoveLineBreaks(paragraph))
                {
                    var text = RemoveLineBreaks(paragraph.Text);
                    
                    result.Add(new RemoveLineBreakResult()
                    {
                        Paragraph = paragraph,
                        AfterText = text
                    });
                }
            }

            return result;
        }
        
        private bool ShouldRemoveLineBreaks(Paragraph p)
        {
            if (p.NumberOfLines == 1)
            {
                return false;
            }

            var text = RemoveLineBreaks(p.Text);
            if (HtmlUtils.RemoveTags(text, true).Length > _configs.MaxLineLength)
            {
                return false;
            }

            return text.Length != p.Text.Length;
        }

        private string RemoveLineBreaks(string text)
        {
            var noTagText = HtmlUtils.RemoveTags(text, true);
            text = text.FixExtraSpaces().Trim();

            // dialog
            if (_configs.SkipDialogs && (noTagText.StartsWith('-') || noTagText.Contains(Environment.NewLine + "-")))
            {
                return text;
            }
            // mood
            if (_configs.SkipMoods && noTagText.IndexOfAny(_moodChars) >= 0)
            {
                return text;
            }
            // narrator
            if (_configs.SkipNarrator && _regexNarrator.IsMatch(noTagText))
            {
                return text;
            }

            return StringUtils.UnbreakLine(text);
        }

    }
}
