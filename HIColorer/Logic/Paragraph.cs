using System;

namespace Nikse.SubtitleEdit.PluginLogic
{
    internal class Paragraph
    {
        // Paragraph write format
        private const string format = "{0}\r\n{1} --> {2}\r\n{3}";
        public int Number { get; set; }
        public string Text { get; set; }
        public TimeCode StartTime { get; set; }
        public TimeCode EndTime { get; set; }

        public Paragraph()
        {
            StartTime = new TimeCode();
            EndTime = new TimeCode();
            Text = string.Empty;
        }

        public override string ToString() => string.Format(format, Number, StartTime, EndTime, Text);

        public int NumberOfLines
        {
            get
            {
                if (string.IsNullOrEmpty(Text))
                    return 0;
                return Utilities.CountTagInText(Text, '\n') + 1;
            }
        }
    }
}