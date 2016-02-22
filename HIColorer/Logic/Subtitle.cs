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

        public Paragraph GetParagraphOrDefault(int index)
        {
            if (_paragraphs == null || _paragraphs.Count <= index || index < 0)
                return null;
            return _paragraphs[index];
        }

        public string ToText(SubRip format)
        {
            return format.ToText(this, Path.GetFileNameWithoutExtension(FileName));
        }

        private void FixEqualOrJustOverlappingFrameNumbers()
        {
            for (int i = 0; i < Paragraphs.Count - 1; i++)
            {
                Paragraph p = Paragraphs[i];
                Paragraph next = Paragraphs[i + 1];
                if (next != null && p.EndFrame == next.StartFrame || p.EndFrame == next.StartFrame + 1)
                    p.EndFrame = next.StartFrame - 1;
            }
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