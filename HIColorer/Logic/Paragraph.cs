using System;

namespace Nikse.SubtitleEdit.PluginLogic
{
    internal class Paragraph
    {
        public int Number { get; set; }
        public string Text { get; set; }
        public TimeCode StartTime { get; set; }
        public TimeCode EndTime { get; set; }
        public TimeCode Duration
        {
            get
            {
                return new TimeCode(EndTime.TotalMilliseconds - StartTime.TotalMilliseconds);
            }
        }
        public int StartFrame { get; set; }
        public int EndFrame { get; set; }

        public Paragraph()
        {
            StartTime = new TimeCode(TimeSpan.FromSeconds(0));
            EndTime = new TimeCode(TimeSpan.FromSeconds(0));
            Text = string.Empty;
        }

        public override string ToString()
        {
            const string format = "{0}\r\n{1} --> {2}\r\n{3}";
            return string.Format(format, Number, StartTime, EndTime, Text);
            //return StartTime + " --> " + EndTime + " " + Text;
        }

        public int NumberOfLines
        {
            get
            {
                if (string.IsNullOrEmpty(Text))
                    return 0;
                return Text.Length - Text.Replace(Environment.NewLine, string.Empty).Length;
            }
        }
    }
}