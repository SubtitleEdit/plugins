using System;

namespace Nikse.SubtitleEdit.PluginLogic.Logic
{
    internal class Paragraph
    {
        public int Number { get; set; }
        public string Text { get; set; }
        public TimeCode StartTime { get; set; }
        public TimeCode EndTime { get; set; }
        public int StartFrame { get; set; }
        public int EndFrame { get; set; }
        public bool Forced { get; set; }
        public string Extra { get; set; }
        public bool IsComment { get; set; }
        public string Actor { get; set; }

        public Paragraph()
        {
            StartTime = new TimeCode(TimeSpan.FromSeconds(0));
            EndTime = new TimeCode(TimeSpan.FromSeconds(0));
            Text = string.Empty;
        }

        public Paragraph(TimeCode startTime, TimeCode endTime, string text)
        {
            StartTime = startTime;
            EndTime = endTime;
            Text = text;
        }

        public Paragraph(Paragraph paragraph)
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

        public Paragraph(int startFrame, int endFrame, string text)
        {
            StartTime = new TimeCode(0, 0, 0, 0);
            EndTime = new TimeCode(0, 0, 0, 0);
            StartFrame = startFrame;
            EndFrame = endFrame;
            Text = text;
        }

        public Paragraph(string text, double startTotalMilliseconds, double endTotalMilliseconds)
        {
            StartTime = new TimeCode(TimeSpan.FromMilliseconds(startTotalMilliseconds));
            EndTime = new TimeCode(TimeSpan.FromMilliseconds(endTotalMilliseconds));
            Text = text;
        }

        public override string ToString()
        {
            const string format = "{0}\r\n{1} --> {2}\r\n{3}";
            return string.Format(format, Number, StartTime, EndTime, Text);
            //return StartTime + " --> " + EndTime + " " + Text;
        }
    }
}