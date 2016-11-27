namespace PluginCoreLib.Subtitle
{
    using PluginCoreLib.Utils;
    using System;

    public class Paragraph
    {
        public string ID { get; private set; }

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

        public int NumberOfLines => StringUtils.CountLines(Text);


        public Paragraph()
            : this(string.Empty, 0, 0)
        {
        }

        public Paragraph(string text, double startSeconds, double endSeconds)
            : this(new TimeCode(TimeCode.BaseUnit * startSeconds), new TimeCode(TimeCode.BaseUnit * endSeconds), text)
        {
        }

        public Paragraph(TimeCode startTime, TimeCode endTime, string text)
        {
            ID = Guid.NewGuid().ToString();
            StartTime = startTime;
            EndTime = endTime;
            Text = text;
        }

        public override string ToString() => $"{StartTime} --> {EndTime} {Text}";

    }
}