using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.PluginLogic
{
    internal class LinesUnbreakerController
    {
        private readonly Regex _regexNarrator = new Regex(":\\B", RegexOptions.Compiled);
        private readonly IList<Paragraph> _paragraphs;
        private readonly UnBreakConfigs _configs;
        private readonly char[] _moodChars = { '(', '[' };

        public event EventHandler<ParagraphEventArgs> TextUnbreaked;

        public LinesUnbreakerController(IList<Paragraph> paragraphs, UnBreakConfigs configs)
        {
            _paragraphs = paragraphs;
            _configs = configs;
        }

        public void Action()
        {
            foreach (var p in _paragraphs)
            {
                if (p.NumberOfLines == 1)
                {
                    continue;
                }

                var unbreakText = UnbreakLines(p.Text);

                // only unbreak if text length <= MaxLineLenth
                if (HtmlUtils.RemoveTags(unbreakText, true).Length > _configs.MaxLineLength)
                {
                    continue;
                }

                if (unbreakText.Length != p.Text.Length)
                {
                    //TODO: oldText = Utilities.RemoveHtmlTags(oldText, true);
                    OnTextUnbreaked(p, unbreakText);
                }
            }
        }

        private void OnTextUnbreaked(Paragraph p, string newText) => TextUnbreaked?.Invoke(this, new ParagraphEventArgs(p, newText));

        private string UnbreakLines(string s)
        {
            var noTagText = HtmlUtils.RemoveTags(s, true);
            s = s.FixExtraSpaces().Trim();

            if (_configs.SkipDialogs && (noTagText.StartsWith('-') || noTagText.Contains(Environment.NewLine + "-")))
            {
                return s;
            }
            if (_configs.SkipMoods && noTagText.IndexOfAny(_moodChars) >= 0)
            {
                return s;
            }
            if (_configs.SkipNarrator && _regexNarrator.IsMatch(noTagText))
            {
                return s;
            }

            return StringUtils.UnbreakLine(s);
        }

    }
}
