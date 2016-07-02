using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Nikse.SubtitleEdit.PluginLogic
{
    internal class Subtitle
    {
        private List<Paragraph> _paragraphs;
        public string FileName { get; set; }
        private readonly SubRip _format;

        public Subtitle(SubRip subrip)
        {
            _paragraphs = new List<Paragraph>();
            FileName = "Untitled";
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
            return _format.ToText(this, Path.GetFileNameWithoutExtension(FileName));
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
