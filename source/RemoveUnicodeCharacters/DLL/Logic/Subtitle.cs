using System;
using System.Collections.Generic;
using System.IO;

namespace SubtitleEdit.Logic
{
    internal class Subtitle
    {
        private List<Paragraph> _paragraphs;
        private readonly SubtitleFormat _format = new SubRip();
        private bool _wasLoadedWithFrameNumbers;
        public string Header { get; set; }
        public string Footer { get; set; }
        public string FileName { get; set; }

        public const int MaximumHistoryItems = 100;

        public SubtitleFormat OriginalFormat => _format;

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

        public List<Paragraph> Paragraphs => _paragraphs;

        public Paragraph GetParagraphOrDefault(int index)
        {
            if (_paragraphs == null || index < 0 || _paragraphs.Count <= index)
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

        /// <summary>
        /// Calculate the time codes from framenumber/framerate
        /// </summary>
        /// <param name="frameRate">Number of frames per second</param>
        /// <returns>True if times could be calculated</returns>
        public bool CalculateTimeCodesFromFrameNumbers(double frameRate)
        {
            if (_format == null)
                return false;

            if (_format.IsTimeBased)
                return false;

            foreach (Paragraph p in Paragraphs)
            {
                p.CalculateTimeCodesFromFrameNumbers(frameRate);
            }
            return true;
        }

        /// <summary>
        /// Calculate the frame numbers from time codes/framerate
        /// </summary>
        /// <param name="frameRate"></param>
        /// <returns></returns>
        public bool CalculateFrameNumbersFromTimeCodes(double frameRate)
        {
            if (_format == null || _format.IsFrameBased)
                return false;

            foreach (Paragraph p in Paragraphs)
            {
                p.CalculateFrameNumbersFromTimeCodes(frameRate);
            }

            FixEqualOrJustOverlappingFrameNumbers();

            return true;
        }

        public void CalculateFrameNumbersFromTimeCodesNoCheck(double frameRate)
        {
            foreach (Paragraph p in Paragraphs)
                p.CalculateFrameNumbersFromTimeCodes(frameRate);

            FixEqualOrJustOverlappingFrameNumbers();
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

        public void ChangeFramerate(double oldFramerate, double newFramerate)
        {
            foreach (Paragraph p in Paragraphs)
            {
                double startFrame = p.StartTime.TotalMilliseconds / TimeCode.BaseUnit * oldFramerate;
                double endFrame = p.EndTime.TotalMilliseconds / TimeCode.BaseUnit * oldFramerate;
                p.StartTime.TotalMilliseconds = startFrame * (TimeCode.BaseUnit / newFramerate);
                p.EndTime.TotalMilliseconds = endFrame * (TimeCode.BaseUnit / newFramerate);

                p.CalculateFrameNumbersFromTimeCodes(newFramerate);
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

        public void AdjustDisplayTimeUsingSeconds(double seconds, System.Windows.Forms.ListView.SelectedIndexCollection selectedIndexes)
        {
            for (int i = 0; i < _paragraphs.Count; i++)
            {
                if (selectedIndexes == null || selectedIndexes.Contains(i))
                {
                    double nextStartMilliseconds = _paragraphs[_paragraphs.Count - 1].EndTime.TotalMilliseconds + 100000;
                    if (i + 1 < _paragraphs.Count)
                        nextStartMilliseconds = _paragraphs[i + 1].StartTime.TotalMilliseconds;

                    double newEndMilliseconds = _paragraphs[i].EndTime.TotalMilliseconds + (seconds * TimeCode.BaseUnit);
                    if (newEndMilliseconds > nextStartMilliseconds)
                        newEndMilliseconds = nextStartMilliseconds - 1;

                    if (seconds < 0)
                    {
                        if (_paragraphs[i].StartTime.TotalMilliseconds + 100 > newEndMilliseconds)
                            _paragraphs[i].EndTime.TotalMilliseconds = _paragraphs[i].StartTime.TotalMilliseconds + 100;
                        else
                            _paragraphs[i].EndTime.TotalMilliseconds = newEndMilliseconds;
                    }
                    else
                    {
                        _paragraphs[i].EndTime.TotalMilliseconds = newEndMilliseconds;
                    }
                }
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

        public int RemoveEmptyLines()
        {
            int count = 0;
            if (_paragraphs.Count > 0)
            {
                int firstNumber = _paragraphs[0].Number;
                for (int i = _paragraphs.Count - 1; i >= 0; i--)
                {
                    Paragraph p = _paragraphs[i];
                    string s = p.Text.Trim();

                    if (s.Length == 0)
                    {
                        _paragraphs.RemoveAt(i);
                        count++;
                    }
                }
                Renumber(firstNumber);
            }
            return count;
        }

        public void InsertParagraphInCorrectTimeOrder(Paragraph newParagraph)
        {
            for (int i = 0; i < Paragraphs.Count; i++)
            {
                Paragraph p = Paragraphs[i];
                if (newParagraph.StartTime.TotalMilliseconds < p.StartTime.TotalMilliseconds)
                {
                    Paragraphs.Insert(i, newParagraph);
                    return;
                }
            }
            Paragraphs.Add(newParagraph);
        }
    }
}