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

        public Paragraph()
        {
            StartTime = new TimeCode(new TimeSpan(0));
            EndTime = new TimeCode(new TimeSpan(0));
            Text = string.Empty;
        }

        public Paragraph(Paragraph paragraph)
        {
            Number = paragraph.Number;
            Text = paragraph.Text;
            StartTime = new TimeCode(paragraph.StartTime.TimeSpan);
            EndTime = new TimeCode(paragraph.EndTime.TimeSpan);
            StartFrame = paragraph.StartFrame;
            EndFrame = paragraph.EndFrame;
        }

        public override string ToString()
        {
            const string format = "{1}{0}{2} --> {3}{0}{4}";
            return string.Format(format, Environment.NewLine, Number, StartTime, EndTime, Text);
        }
    }
}
