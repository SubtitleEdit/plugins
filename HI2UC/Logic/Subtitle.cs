using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Nikse.SubtitleEdit.PluginLogic
{
    internal class Subtitle
    {
        private List<Paragraph> _paragraphs;
        private SubtitleFormat _format;
        private bool _wasLoadedWithFrameNumbers;

        internal string Header { get; set; }
        internal string Footer { get; set; }
        internal string FileName { get; set; }
        internal bool IsHearingImpaired { get; private set; }

        internal const int MaximumHistoryItems = 100;

        internal SubtitleFormat OriginalFormat { get { return _format; } }
        internal List<Paragraph> Paragraphs { get { return _paragraphs; } }

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

        internal Paragraph GetParagraphOrDefault(int index)
        {
            if (_paragraphs == null || _paragraphs.Count <= index || index < 0)
                return null;
            return _paragraphs[index];
        }

        internal string ToText(SubtitleFormat format)
        {
            return format.ToText(this, Path.GetFileNameWithoutExtension(FileName));
        }

        internal bool WasLoadedWithFrameNumbers { get { return _wasLoadedWithFrameNumbers; } set { _wasLoadedWithFrameNumbers = value; } }

        internal void Renumber(int startNumber)
        {
            int i;
            if (startNumber < 0)
                i = 0;
            else
                i = startNumber;
            foreach (Paragraph p in _paragraphs)
            {
                p.Number = i;
                i++;
            }
        }

        internal int GetIndex(Paragraph p)
        {
            if (p == null)
                return -1;

            int index = _paragraphs.IndexOf(p);
            if (index >= 0)
                return index;

            for (int i = 0; i < _paragraphs.Count; i++)
            {
                if (p.StartTime.TotalMilliseconds == _paragraphs[i].StartTime.TotalMilliseconds &&
                    p.EndTime.TotalMilliseconds == _paragraphs[i].EndTime.TotalMilliseconds)
                    return i;
                if (p.Number == _paragraphs[i].Number && (p.StartTime.TotalMilliseconds == _paragraphs[i].StartTime.TotalMilliseconds ||
                    p.EndTime.TotalMilliseconds == _paragraphs[i].EndTime.TotalMilliseconds))
                    return i;
                if (p.Text == _paragraphs[i].Text && (p.StartTime.TotalMilliseconds == _paragraphs[i].StartTime.TotalMilliseconds ||
                    p.EndTime.TotalMilliseconds == _paragraphs[i].EndTime.TotalMilliseconds))
                    return i;
            }
            return -1;
        }

        internal Paragraph GetFirstAlike(Paragraph p)
        {
            foreach (Paragraph item in _paragraphs)
            {
                if (p.StartTime.TotalMilliseconds == item.StartTime.TotalMilliseconds &&
                    p.EndTime.TotalMilliseconds == item.EndTime.TotalMilliseconds &&
                    p.Text == item.Text)
                    return item;
            }
            return null;
        }

        internal Paragraph GetFirstParagraphByLineNumber(int number)
        {
            foreach (Paragraph p in _paragraphs)
            {
                if (p.Number == number)
                    return p;
            }
            return null;
        }

        internal void RemoveLine(int lineNumber)
        {
            if (_paragraphs == null)
                return;

            int startNumber = _paragraphs[0].Number;
            if (lineNumber >= 0)
            {
                _paragraphs.Remove(_paragraphs.Single(p => p.Number == lineNumber));
            }
            Renumber(startNumber);
        }
    }
}