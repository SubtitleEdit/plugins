using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public class Subtitle
    {
        private List<Paragraph> _paragraphs;
        private bool _wasLoadedWithFrameNumbers;
        public string Header { get; set; }
        public string Footer { get; set; }
        public string FileName { get; set; }
        private readonly SubtitleFormat _subFormat = new SubRip();
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
        public Subtitle(Subtitle subtitle)
            : this()
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
            return _subFormat.ToText(this, Path.GetFileNameWithoutExtension(FileName));
        }

        public bool WasLoadedWithFrameNumbers { get { return _wasLoadedWithFrameNumbers; } set { _wasLoadedWithFrameNumbers = value; } }

        public void Renumber(int startNumber = 1)
        {
            foreach (Paragraph p in _paragraphs)
            {
                p.Number = startNumber++;
            }
        }

        public int GetIndex(Paragraph p)
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

        public Paragraph GetFirstAlike(Paragraph p)
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

        public Paragraph GetFirstParagraphByLineNumber(int number)
        {
            foreach (Paragraph p in _paragraphs)
            {
                if (p.Number == number)
                    return p;
            }
            return null;
        }

        public void RemoveLine(int lineNumber)
        {
            if (_paragraphs == null || lineNumber < 0)
                return;
            _paragraphs.Remove(_paragraphs.Single(p => p.Number == lineNumber));
            Renumber();
        }

        /// <summary>
        /// Removes paragrahs by a list of IDs
        /// </summary>
        /// <param name="ids">IDs of pargraphs/lines to delete</param>
        /// <returns>Number of lines deleted</returns>
        public int RemoveParagraphsByIds(IEnumerable<string> ids)
        {
            int beforeCount = _paragraphs.Count;
            _paragraphs = _paragraphs.Where(p => !ids.Contains(p.Id)).ToList();
            return beforeCount - _paragraphs.Count;
        }
    }
}