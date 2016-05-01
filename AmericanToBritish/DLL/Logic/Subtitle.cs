using System.Collections.Generic;
using System.IO;

namespace Nikse.SubtitleEdit.PluginLogic.Logic
{
    internal class Subtitle
    {
        private List<Paragraph> _paragraphs;
        private readonly SubRip _format;

        private bool _wasLoadedWithFrameNumbers;
        public string FileName { get; set; }

        public Subtitle(SubRip subrip)
        {
            _paragraphs = new List<Paragraph>();
            FileName = "Untitled";
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
            return _format.ToText(this, Path.GetFileNameWithoutExtension(FileName));
        }

        public void Renumber(int startNumber = 1)
        {
            foreach (Paragraph p in _paragraphs)
            {
                p.Number = startNumber++;
            }
        }
    }
}
