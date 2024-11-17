using System;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public class Paragraph
    {
        public string ID { get; private set; }
        public int Number { get; set; }
        public string Text { get; set; }
        public TimeCode StartTime { get; set; }
        public TimeCode EndTime { get; set; }

        /// <summary>
        /// Returns the duration in milliseconds.
        /// </summary>
        public double Duration => EndTime.TotalMilliseconds - StartTime.TotalMilliseconds;

        public Paragraph() : this(new TimeCode(), new TimeCode(), string.Empty)
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

        public int NumberOfLines => StringUtils.GetNumberOfLines(Text);

        public Paragraph GetCopy() => new Paragraph(StartTime, EndTime, Text)
        {
            ID = Guid.NewGuid().ToString(),
            Number = Number
        };

    }
}