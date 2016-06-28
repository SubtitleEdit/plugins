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
            StartFrame = paragraph.StartFrame;
            EndFrame = paragraph.EndFrame;
            ID = GenerateId();
        }

        public void CalculateFrameNumbersFromTimeCodes(double frameRate)
        {
            StartFrame = (int)Math.Round(StartTime.TotalMilliseconds / TimeCode.BaseUnit * frameRate);
            EndFrame = (int)Math.Round(EndTime.TotalMilliseconds / TimeCode.BaseUnit * frameRate);
        }

        public void CalculateTimeCodesFromFrameNumbers(double frameRate)
        {
            StartTime.TotalMilliseconds = StartFrame * (TimeCode.BaseUnit / frameRate);
            EndTime.TotalMilliseconds = EndFrame * (TimeCode.BaseUnit / frameRate);
        }

        public override string ToString() => $"{StartTime} --> {EndTime} {Text}";

        public int NumberOfLines => Utilities.GetNumberOfLines(Text);
    }
}
