using System;
using System.Collections.Generic;
using System.IO;
namespace Nikse.SubtitleEdit.PluginLogic
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
    }
}
