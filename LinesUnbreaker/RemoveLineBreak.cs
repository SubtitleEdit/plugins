using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.PluginLogic
{
    internal class RemoveLineBreak
    {
        private readonly Regex _regexNarrator = new Regex(":\\B", RegexOptions.Compiled);
        private readonly IList<Paragraph> _paragraphs;
        private readonly UnBreakConfigs _configs;
        private readonly char[] _moodChars = { '(', '[' };

        public event EventHandler<ParagraphEventArgs> TextUnbreaked;

        public RemoveLineBreak(IList<Paragraph> paragraphs, UnBreakConfigs configs)
        {
            _paragraphs = paragraphs;
            _configs = configs;
        }

        public void Remove()
        {
            foreach (var p in _paragraphs)
            {
                if (ShouldRemoveLineBreaks(p))
                {
                    var text = RemoveLineBreaks(p.Text);
                    OnTextUnbreaked(p, text);
                }
            }
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

        private void OnTextUnbreaked(Paragraph p, string newText) => TextUnbreaked?.Invoke(this, new ParagraphEventArgs(p, newText));

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
