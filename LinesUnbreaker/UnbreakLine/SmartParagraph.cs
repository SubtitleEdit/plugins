using System;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.PluginLogic.UnbreakLine
{
    public class SmartParagraph
    {
        public Paragraph Paragraph { get; }

        // private int _currentIndex = 0;
        private Line[] _lines;

        public IEnumerable<Line> Lines => _lines;

        public bool IsMultiLined => _lines.Length > 1;
        
        public string Text
        {
            set => _lines = ReadLines(value).ToArray();
            get => string.Join(Environment.NewLine, _lines.Select(line => line));
        }

        public SmartParagraph(Paragraph paragraph)
        {
            Paragraph = paragraph;
            _lines = ReadLines(paragraph.Text).ToArray();
        }
        
        private IEnumerable<Line> ReadLines(string text) => text.SplitToLines().Select(line => new Line(line));
    }
}