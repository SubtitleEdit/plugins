using System;

namespace Nikse.SubtitleEdit.PluginLogic
{
    internal class Paragraph
    {
        public string ID { get; private set; } = Guid.NewGuid().ToString();
        public int Number { get; set; }
        public string Text { get; set; }
        public TimeCode StartTime { get; set; }
        public TimeCode EndTime { get; set; }

        public Paragraph()
        {
            StartTime = new TimeCode(0);
            EndTime = new TimeCode(0);
            Text = string.Empty;
        }

        public override string ToString() => $"{StartTime} --> {EndTime} {Text}";

        public int NumberOfLines => Utilities.GetNumberOfLines(Text);
    }
}
