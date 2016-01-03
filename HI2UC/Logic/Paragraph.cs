using System;

namespace Nikse.SubtitleEdit.PluginLogic
{
    internal class Paragraph
    {
        public string Id { get; private set; }
        public int Number { get; set; }
        public string Text { get; set; }
        public TimeCode StartTime { get; set; }
        public TimeCode EndTime { get; set; }
        public TimeCode Duration
        {
            get
            {
                var timeCode = new TimeCode(EndTime.TimeSpan);
                timeCode.AddTime(-StartTime.TotalMilliseconds);
                return timeCode;
            }
        }
        public int StartFrame { get; set; }
        public int EndFrame { get; set; }
        public bool Forced { get; set; }
        public string Extra { get; set; }
        public bool IsComment { get; set; }
        public string Actor { get; set; }

        public Paragraph()
        {
            Id = GenerateId();
            StartTime = new TimeCode(TimeSpan.FromSeconds(0));
            EndTime = new TimeCode(TimeSpan.FromSeconds(0));
            Text = string.Empty;
        }

        public Paragraph(TimeCode startTime, TimeCode endTime, string text)
        {
            Id = GenerateId();
            StartTime = startTime;
            EndTime = endTime;
            Text = text;
        }

        public Paragraph(Paragraph paragraph)
        {
            Id = GenerateId();
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
            Id = GenerateId();
            StartTime = new TimeCode(0, 0, 0, 0);
            EndTime = new TimeCode(0, 0, 0, 0);
            StartFrame = startFrame;
            EndFrame = endFrame;
            Text = text;
        }

        public override string ToString() => $"{StartTime} --> {EndTime} {Text}";

        public int NumberOfLines => Utilities.GetNumberOfLines(Text);

        private string GenerateId() => Guid.NewGuid().ToString();
    }
}