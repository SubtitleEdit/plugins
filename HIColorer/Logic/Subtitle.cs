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

        public List<Paragraph> Paragraphs { get { return _paragraphs; } }

        public Subtitle()
        {
            _paragraphs = new List<Paragraph>();
            FileName = "Untitled";
            //_format = new SubRip();
        }

        public string ToText(SubRip format)
        {
            return format.ToText(this, Path.GetFileNameWithoutExtension(FileName));
        }

        public void Renumber(int start = 1)
        {
            foreach (var p in _paragraphs)
            {
                p.Number = start++;
            }
        }
    }
}