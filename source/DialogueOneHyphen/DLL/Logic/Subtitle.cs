using System;
using System.Collections.Generic;
using System.IO;

namespace Nikse.SubtitleEdit.PluginLogic
{
    internal class Subtitle
    {
        private List<Paragraph> _paragraphs;
        private readonly SubRip _format;

        public Subtitle(SubRip format)
        {
            _paragraphs = new List<Paragraph>();
            _format = format;
        }

        public List<Paragraph> Paragraphs => _paragraphs;

        public Paragraph GetParagraphOrDefault(int index)
        {
            if (_paragraphs == null || index < 0 || _paragraphs.Count <= index)
                return null;

            return _paragraphs[index];
        }

        public string ToText()
        {
            return _format.ToText(this);
        }

        public void Renumber(int startNumber)
        {
            foreach (Paragraph p in _paragraphs)
            {
                p.Number = startNumber++;
            }
        }
    }
}