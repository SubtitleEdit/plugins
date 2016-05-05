using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Nikse.SubtitleEdit.PluginLogic
{
    internal class Subtitle
    {
        private List<Paragraph> _paragraphs;
        private readonly SubRip _format;
        public string Header { get; set; }
        public string Footer { get; set; }
        public string FileName { get; set; }
        public bool IsHearingImpaired { get; private set; }
        public const int MaximumHistoryItems = 100;

        public Subtitle(SubRip subrip)
        {
            _paragraphs = new List<Paragraph>();
            FileName = "Untitled";
            _format = subrip;
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

        public string ToText()
        {
            return _format.ToText(this, Path.GetFileNameWithoutExtension(FileName));
        }

        public void AddTimeToAllParagraphs(TimeSpan time)
        {
            foreach (Paragraph p in Paragraphs)
            {
                p.StartTime.AddTime(time);
                p.EndTime.AddTime(time);
            }
        }

        public void Renumber(int startNumber)
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

    }
}
