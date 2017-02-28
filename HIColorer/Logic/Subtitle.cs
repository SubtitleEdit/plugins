using System.Collections;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.PluginLogic
{
    internal class Subtitle
    {
        private IList<Paragraph> _paragraphs;
        private SubRip _format;

        public IList<Paragraph> Paragraphs { get => _paragraphs; }

        public Subtitle(SubRip subrip)
        {
            _paragraphs = new List<Paragraph>();
            _format = subrip;
        }

        public string ToText() => _format.ToText(this);

        public void Renumber(int lineNum = 1)
        {
            foreach (Paragraph p in _paragraphs)
            {
                p.Number = lineNum++;
            }
        }
    }
}