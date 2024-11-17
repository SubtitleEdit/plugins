using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public class Subtitle
    {
        private List<Paragraph> _paragraphs;
        private readonly SubRip _subFormat;

        public string Header { get; set; }
        public string Footer { get; set; }
        public string FileName { get; set; }
        
        // todo: change this in future to IEnumerable to avoid external source to change the internal values
        public List<Paragraph> Paragraphs => _paragraphs;

        public Subtitle(SubRip subrip)
        {
            _subFormat = subrip;
            _paragraphs = new List<Paragraph>();
            FileName = "Untitled";
        }

        public string ToText() => _subFormat.ToText(this, Path.GetFileNameWithoutExtension(FileName));

        public void Renumber(int startNumber = 1)
        {
            startNumber = Math.Max(1, startNumber);
            foreach (Paragraph p in _paragraphs)
            {
                p.Number = startNumber++;
            }
        }

        public void RemoveLine(int lineNumber)
        {
            if (_paragraphs == null || lineNumber < 0)
            {
                return;
            }

            if (_paragraphs.Remove(_paragraphs.Single(p => p.Number == lineNumber)))
            {
                Renumber();
            }
        }
    }
}