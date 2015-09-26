using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Nikse.SubtitleEdit.PluginLogic
{
    internal class Subtitle
    {
        private List<Paragraph> _paragraphs;
        private bool _wasLoadedWithFrameNumbers;
        private static readonly SubRip _format = new SubRip();

        public string Header { get; set; }
        public string Footer { get; set; }
        public string FileName { get; set; }
        public bool IsHearingImpaired { get; private set; }

        public const int MaximumHistoryItems = 100;

        public List<Paragraph> Paragraphs { get { return _paragraphs; } }

        public Subtitle()
        {
            _paragraphs = new List<Paragraph>();
            FileName = "Untitled";
            //_format = new SubRip();
        }

        /// <summary>
        /// Copy constructor (only paragraphs)
        /// </summary>
        /// <param name="subtitle">Subtitle to copy</param>
        public Subtitle(Subtitle subtitle) : this()
        {
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                _paragraphs.Add(new Paragraph(p));
            }
            _wasLoadedWithFrameNumbers = subtitle.WasLoadedWithFrameNumbers;
        }

        public Paragraph GetParagraphOrDefault(int index)
        {
            if (_paragraphs == null || _paragraphs.Count <= index || index < 0)
                return null;
            return _paragraphs[index];
        }

        public string ToText()
        {
            return _format.ToText(this, Path.GetFileNameWithoutExtension(FileName));
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

        public void Renumber(int startNumber = 1)
        {
            if (startNumber <= 0)
                startNumber = 1;
            foreach (Paragraph p in _paragraphs)
            {
                p.Number = startNumber++;
            }
        }

        public void RemoveLine(int lineNumber)
        {
            if (_paragraphs == null || _paragraphs.Count == 0)
                return;

            if (lineNumber >= 0)
            {
                _paragraphs.Remove(_paragraphs.Single(p => p.Number == lineNumber));
            }
            Renumber();
        }
    }
}