using System;

namespace Nikse.SubtitleEdit.PluginLogic
{
    internal class Paragraph
    {
        internal int Number { get; set; }
        internal string Text { get; set; }
        internal TimeCode StartTime { get; set; }
        internal TimeCode EndTime { get; set; }
        internal TimeCode Duration
        {
            get
            {
                var timeCode = new TimeCode(EndTime.TimeSpan);
                timeCode.AddTime(-StartTime.TotalMilliseconds);
                return timeCode;
            }
        }
        internal int StartFrame { get; set; }
        internal int EndFrame { get; set; }
        internal bool Forced { get; set; }
        internal string Extra { get; set; }
        internal bool IsComment { get; set; }
        internal string Actor { get; set; }

        internal Paragraph()
        {
            StartTime = new TimeCode(TimeSpan.FromSeconds(0));
            EndTime = new TimeCode(TimeSpan.FromSeconds(0));
            Text = string.Empty;
        }

        internal Paragraph(TimeCode startTime, TimeCode endTime, string text)
        {
            StartTime = startTime;
            EndTime = endTime;
            Text = text;
        }

        internal Paragraph(Paragraph paragraph)
        {
            Number = paragraph.Number;
            Text = paragraph.Text;
            StartTime = new TimeCode(paragraph.StartTime.TimeSpan);
            EndTime = new TimeCode(paragraph.EndTime.TimeSpan);
            StartFrame = paragraph.StartFrame;
            EndFrame = paragraph.EndFrame;
            Forced = paragraph.Forced;
            Extra = paragraph.Extra;
            IsComment = paragraph.IsComment;
            Actor = paragraph.Actor;
        }

        internal Paragraph(int startFrame, int endFrame, string text)
        {
            StartTime = new TimeCode(0, 0, 0, 0);
            EndTime = new TimeCode(0, 0, 0, 0);
            StartFrame = startFrame;
            EndFrame = endFrame;
            Text = text;
        }

        public override string ToString()
        {
            return StartTime + " --> " + EndTime + "\r\n" + Text;
        }

        internal int NumberOfLines
        {
            get
            {
                return Utilities.NumberOfLines(Text);
            }
        }
    }
}