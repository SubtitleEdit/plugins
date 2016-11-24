namespace PluginCoreLib.Subtitle
{
    using PluginCoreLib.Utils;
    using System;

    public class Paragraph
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
                return new TimeCode(EndTime.TotalMilliseconds - StartTime.TotalMilliseconds);
            }
        }

        public Paragraph()
            : this(string.Empty, new TimeCode(), new TimeCode())
        {
        }

        public Paragraph(string text, double startSeconds, double endSeconds)
            : this(text, new TimeCode(TimeCode.BaseUnit * startSeconds), new TimeCode(TimeCode.BaseUnit * endSeconds))
        {
        }

        public Paragraph(string text, TimeCode startTime, TimeCode endTime)
        {
            Id = Guid.NewGuid().ToString();
            StartTime = startTime;
            EndTime = endTime;
            Text = text;
        }

        public int NumberOfLines => StringUtils.CountLines(Text);

        public override string ToString() => $"{StartTime} --> {EndTime} {Text}";

    }
}