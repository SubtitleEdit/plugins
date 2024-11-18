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

        public Paragraph GetParagraphOrDefault(int index)
        {
            if (_paragraphs == null || _paragraphs.Count <= index || index < 0)
                return null;
            return _paragraphs[index];
        }

        public string ToText(SubtitleFormat format)
        {
            return format.ToText(this, Path.GetFileNameWithoutExtension(FileName));
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

        public void Renumber(int startNumber)
        {
            int i = startNumber;
            foreach (Paragraph p in _paragraphs)
            {
                p.Number = i;
                i++;
            }
        }
    }
}