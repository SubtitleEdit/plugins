using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ColorToDialog.Logic
{
    public class Subtitle
    {
        private List<Paragraph> _paragraphs;
        private bool _wasLoadedWithFrameNumbers;
        internal const int MaximumHistoryItems = 100;
        internal string Header { get; set; }
        internal string Footer { get; set; }
        internal string FileName { get; set; }
        internal bool IsHearingImpaired { get; private set; }

        internal Subtitle()
        {
            _paragraphs = new List<Paragraph>();
            FileName = "Untitled";
            //_format = new SubRip();
        }

        /// <summary>
        /// Copy constructor (only paragraphs)
        /// </summary>
        /// <param name="subtitle">Subtitle to copy</param>
        internal Subtitle(Subtitle subtitle)
            : this()
        {
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                _paragraphs.Add(new Paragraph(p));
            }
            _wasLoadedWithFrameNumbers = subtitle.WasLoadedWithFrameNumbers;
        }

        internal List<Paragraph> Paragraphs
        {
            get
            {
                return _paragraphs;
            }
        }

        internal string ToText(SubtitleFormat format)
        {
            return format.ToText(this, Path.GetFileNameWithoutExtension(FileName));
        }

        internal bool WasLoadedWithFrameNumbers
        {
            get
            {
                return _wasLoadedWithFrameNumbers;
            }
            set
            {
                _wasLoadedWithFrameNumbers = value;
            }
        }

        internal void Renumber(int startNumber = 1)
        {
            if (startNumber < 1)
                startNumber = 1;
            foreach (Paragraph p in _paragraphs)
            {
                p.Number = startNumber++;
            }
        }

        public string GetAllTexts()
        {
            var sb = new StringBuilder();
            foreach (Paragraph p in Paragraphs)
            {
                sb.AppendLine(p.Text);
            }
            return sb.ToString();
        }

        public int RemoveEmptyLines()
        {
            int count = _paragraphs.Count;
            if (count > 0)
            {
                int firstNumber = _paragraphs[0].Number;
                for (int i = _paragraphs.Count - 1; i >= 0; i--)
                {
                    Paragraph p = _paragraphs[i];
                    if (string.IsNullOrWhiteSpace(p.Text))
                        _paragraphs.RemoveAt(i);
                }
                if (count != _paragraphs.Count)
                    Renumber(firstNumber);
            }
            return count - _paragraphs.Count;
        }
    }
}
