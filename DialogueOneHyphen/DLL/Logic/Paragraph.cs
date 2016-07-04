using System;

namespace Nikse.SubtitleEdit.PluginLogic
{
    internal class Paragraph
    {
        public string ID { get; private set; }
        public int Number { get; set; }
        public string Text { get; set; }
        public TimeCode StartTime { get; set; }
        public TimeCode EndTime { get; set; }

        public Paragraph()
        {
            StartTime = new TimeCode(0);
            EndTime = new TimeCode(0);
            Text = string.Empty;
            ID = GenerateId();
        }

        private string GenerateId() => Guid.NewGuid().ToString();

        public Paragraph(Paragraph paragraph)
        {
            Number = paragraph.Number;
            Text = paragraph.Text;
            StartTime = new TimeCode(paragraph.StartTime.TimeSpan);
            EndTime = new TimeCode(paragraph.EndTime.TimeSpan);
            ID = GenerateId();
        }

        public override string ToString() => $"{StartTime} --> {EndTime} {Text}";

        public int NumberOfLines => Utilities.GetNumberOfLines(Text);
    }
}
