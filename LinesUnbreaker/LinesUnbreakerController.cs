using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.PluginLogic
{
    internal class LinesUnbreakerController
    {
        private readonly Regex _regexNarrator = new Regex(":\\B", RegexOptions.Compiled);
        private readonly IList<Paragraph> _paragraphs;
        private readonly Configuration _configs;
        private readonly char[] _moodChars = { '(', '[' };

        public event EventHandler<ParagraphEventArgs> TextUnbreaked;

        public LinesUnbreakerController(IList<Paragraph> paragraphs, Configuration configs)
        {
            _paragraphs = paragraphs;
            _configs = configs;
        }

        public void Action()
        {
            foreach (var p in _paragraphs)
            {
                if (p.NumberOfLines >= 2)
                {
                    var oldText = p.Text;
                    var text = UnbreakLines(p.Text);
                    var t = Utilities.RemoveHtmlTags(text, true);
                    if (text.Length != oldText.Length && t.Length < _configs.MaxLineLength)
                    {
                        //TODO: oldText = Utilities.RemoveHtmlTags(oldText, true);
                        OnTextUnbreaked(p, text);
                    }
                }
            }
        }

        private void OnTextUnbreaked(Paragraph p, string newText)
        {
            TextUnbreaked?.Invoke(this, new ParagraphEventArgs(p, newText));
        }

        private string UnbreakLines(string s)
        {
            var temp = Utilities.RemoveHtmlTags(s, true);
            temp = temp.Replace("  ", " ").Trim();

            if (_configs.SkipDialogs && (temp.StartsWith('-') || temp.Contains(Environment.NewLine + "-")))
            {
                return s;
            }
            if (_configs.SkipMoods && temp.IndexOfAny(_moodChars) >= 0)
            {
                return s;
            }
            if (_configs.SkipNarrator && _regexNarrator.IsMatch(temp))
            {
                return s;
            }

            return Utilities.UnbreakLine(s);
        }

    }
}
