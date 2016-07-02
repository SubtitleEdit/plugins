using System.Collections.Generic;

namespace Nikse.SubtitleEdit.PluginLogic
{
    internal class Subtitle
    {
        private List<Paragraph> _paragraphs;
        private readonly SubRip _format;

        public Subtitle(SubRip subrip)
        {
            _paragraphs = new List<Paragraph>();
            _format = subrip;
        }

        public List<Paragraph> Paragraphs
        {
            get
            {
                return _paragraphs;
            }
        }

        public string ToText()
        {
            return _format.ToText(_paragraphs);
        }

        public void Renumber(int startNumber = 1)
        {
            if (startNumber < 0)
                startNumber = 1;
            foreach (Paragraph p in _paragraphs)
            {
                p.Number = startNumber++;
            }
        }
    }
}
