using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Nikse.SubtitleEdit.PluginLogic
{
    internal class Subtitle
    {
        private List<Paragraph> _paragraphs;
        public string Header { get; set; }
        public string Footer { get; set; }
        public string FileName { get; set; }
        private readonly ITextConverter _subFormat;
        public List<Paragraph> Paragraphs { get { return _paragraphs; } }

        public Subtitle(SubRip subrip)
        {
            _subFormat = subrip;
            _paragraphs = new List<Paragraph>();
            FileName = "Untitled";
        }

        public string ToText()
        {
            return _subFormat.ToText(this, Path.GetFileNameWithoutExtension(FileName));
        }

        public void Renumber(int startNumber = 1)
        {
            foreach (Paragraph p in _paragraphs)
            {
                p.Number = startNumber++;
            }
        }

        public void RemoveLine(int lineNumber)
        {
            if (_paragraphs == null || lineNumber < 0)
                return;
            _paragraphs.Remove(_paragraphs.Single(p => p.Number == lineNumber));
            Renumber();
        }
    }
}