using System;

namespace Nikse.SubtitleEdit.PluginLogic
{
    internal class Paragraph
    {
        public string Id { get; } = Guid.NewGuid().ToString();
        public int Number { get; set; }
        public string Text { get; set; }
        public TimeCode StartTime { get; set; }
        public TimeCode EndTime { get; set; }
        public TimeCode Duration => new TimeCode(EndTime.TotalMilliseconds - StartTime.TotalMilliseconds);

        public Paragraph()
        {
            StartTime = new TimeCode();
            EndTime = new TimeCode();
            Text = string.Empty;
        }

        public override string ToString() => $"{Number}\r\n{StartTime} --> {EndTime}\r\n{Text}";

        public int NumberOfLines => Utilities.GetNumberOfLines(Text);
    }
}