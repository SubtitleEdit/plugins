using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Nikse.SubtitleEdit.PluginLogic.Logic.SubtitleFormats;

namespace Nikse.SubtitleEdit.PluginLogic.Logic
{
    public class Subtitle
    {
        private List<Paragraph> _paragraphs;
        private SubtitleFormat _format;
        private bool _wasLoadedWithFrameNumbers;
        internal string Header { get; set; }
        internal string Footer { get; set; }
        internal string FileName { get; set; }
        internal bool IsHearingImpaired { get; private set; }
        internal const int MaximumHistoryItems = 100;

        internal SubtitleFormat OriginalFormat
        {
            get
            {
                return _format;
            }
        }

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

        internal void AddTimeToAllParagraphs(TimeSpan time)
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
        internal bool CalculateTimeCodesFromFrameNumbers(double frameRate)
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
        internal bool CalculateFrameNumbersFromTimeCodes(double frameRate)
        {
            if (_format == null)
                return false;

            if (_format.IsFrameBased)
                return false;

            foreach (Paragraph p in Paragraphs)
            {
                p.CalculateFrameNumbersFromTimeCodes(frameRate);
            }

            FixEqualOrJustOverlappingFrameNumbers();

            return true;
        }

        internal void CalculateFrameNumbersFromTimeCodesNoCheck(double frameRate)
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

        internal void ChangeFramerate(double oldFramerate, double newFramerate)
        {
            foreach (Paragraph p in Paragraphs)
            {
                double startFrame = p.StartTime.TotalMilliseconds / 1000.0 * oldFramerate;
                double endFrame = p.EndTime.TotalMilliseconds / 1000.0 * oldFramerate;
                p.StartTime.TotalMilliseconds = startFrame * (1000.0 / newFramerate);
                p.EndTime.TotalMilliseconds = endFrame * (1000.0 / newFramerate);
                p.CalculateFrameNumbersFromTimeCodes(newFramerate);
            }
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

        internal void AdjustDisplayTimeUsingPercent(double percent, System.Windows.Forms.ListView.SelectedIndexCollection selectedIndexes)
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

        internal void AdjustDisplayTimeUsingSeconds(double seconds, System.Windows.Forms.ListView.SelectedIndexCollection selectedIndexes)
        {
            for (int i = 0; i < _paragraphs.Count; i++)
            {
                if (selectedIndexes == null || selectedIndexes.Contains(i))
                {
                    double nextStartMilliseconds = _paragraphs[_paragraphs.Count - 1].EndTime.TotalMilliseconds + 100000;
                    if (i + 1 < _paragraphs.Count)
                        nextStartMilliseconds = _paragraphs[i + 1].StartTime.TotalMilliseconds;

                    double newEndMilliseconds = _paragraphs[i].EndTime.TotalMilliseconds + (seconds * 1000.0);
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

        internal int RemoveEmptyLines()
        {
            int count = 0;
            if (_paragraphs.Count > 0)
            {
                int firstNumber = _paragraphs[0].Number;
                for (int i = _paragraphs.Count - 1; i >= 0; i--)
                {
                    Paragraph p = _paragraphs[i];
                    if (string.IsNullOrEmpty(p.Text.Trim()))
                    {
                        _paragraphs.RemoveAt(i);
                        count++;
                    }
                }
                Renumber(firstNumber);
            }
            return count;
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

        internal void InsertParagraphInCorrectTimeOrder(Paragraph newParagraph)
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
