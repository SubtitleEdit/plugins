using System;
using System.Collections.Generic;
using System.IO;

namespace Nikse.SubtitleEdit.PluginLogic
{
    internal class Subtitle
    {
        private List<Paragraph> _paragraphs;
        private SubtitleFormat _format;
        private bool _wasLoadedWithFrameNumbers;
        public string Header { get; set; }
        public string Footer { get; set; }

        public string FileName { get; set; }

        public const int MaximumHistoryItems = 100;

        public SubtitleFormat OriginalFormat
        {
            get
            {
                return _format;
            }
        }

        public Subtitle()
        {
            _paragraphs = new List<Paragraph>();
            FileName = "Untitled";
        }

        /// <summary>
        /// Copy constructor (only paragraphs)
        /// </summary>
        /// <param name="subtitle">Subtitle to copy</param>
        public Subtitle(Subtitle subtitle)
            : this()
        {
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                _paragraphs.Add(new Paragraph(p));
            }
            _wasLoadedWithFrameNumbers = subtitle.WasLoadedWithFrameNumbers;
        }

        public List<Paragraph> Paragraphs
        {
            get
            {
                return _paragraphs;
            }
        }

        public bool WasLoadedWithFrameNumbers
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

        public void AdjustDisplayTimeUsingPercent(double percent, System.Windows.Forms.ListView.SelectedIndexCollection selectedIndexes)
        {
            for (int i = 0; i < _paragraphs.Count; i++)
            {
                if (selectedIndexes == null || selectedIndexes.Contains(i))
                {
                    double nextStartMilliseconds = _paragraphs[_paragraphs.Count - 1].EndTime.TotalMilliseconds + 100000;
                    if (i + 1 < _paragraphs.Count)
                        nextStartMilliseconds = _paragraphs[i + 1].StartTime.TotalMilliseconds;

                    double newEndMilliseconds = _paragraphs[i].EndTime.TotalMilliseconds;
                    newEndMilliseconds = _paragraphs[i].StartTime.TotalMilliseconds + (((newEndMilliseconds - _paragraphs[i].StartTime.TotalMilliseconds) * percent) / 100);
                    if (newEndMilliseconds > nextStartMilliseconds)
                        newEndMilliseconds = nextStartMilliseconds - 1;
                    _paragraphs[i].EndTime.TotalMilliseconds = newEndMilliseconds;
                }
            }
        }

        public void Renumber(int startNumber = 1)
        {
            foreach (Paragraph p in _paragraphs)
            {
                p.Number = startNumber;
            }
        }

        public int RemoveEmptyLines()
        {
            int count = _paragraphs.Count;
            for (int i = _paragraphs.Count - 1; i >= 0; i--)
            {
                var text = Utilities.RemoveHtmlTags(_paragraphs[i].Text).Trim();
                if (text.Length == 0)
                {
                    _paragraphs.RemoveAt(i);
                }
            }
            if (count != _paragraphs.Count)
                Renumber();
            return count - _paragraphs.Count;
        }
    }
}
