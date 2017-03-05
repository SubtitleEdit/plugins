using System.Collections.Generic;

namespace Nikse.SubtitleEdit.PluginLogic
{
    internal class Subtitle
    {
        private List<Paragraph> _paragraphs;
        private readonly SubRip _format;

        public int Errors { get; set; }

        public Subtitle(SubRip subrip)
        {
            _paragraphs = new List<Paragraph>();
            _format = subrip;
        }

        public List<Paragraph> Paragraphs => _paragraphs;

        public string ToText() =>  _format.ToText(_paragraphs);
    }
}
