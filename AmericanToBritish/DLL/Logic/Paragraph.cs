using System;

namespace Nikse.SubtitleEdit.PluginLogic.Logic
{
    internal class Paragraph
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
                var timeCode = new TimeCode(EndTime.TimeSpan);
                timeCode.AddTime(-StartTime.TotalMilliseconds);
                return timeCode;
            }
        }
        public int StartFrame { get; set; }
        public int EndFrame { get; set; }
        public bool Forced { get; set; }
        public string Extra { get; set; }
        public bool IsComment { get; set; }
        public string Actor { get; set; }

        public Paragraph()
        {
            StartTime = new TimeCode(TimeSpan.FromSeconds(0));
            EndTime = new TimeCode(TimeSpan.FromSeconds(0));
            Text = string.Empty;
            Id = GenerateId();
        }

        public Paragraph(TimeCode startTime, TimeCode endTime, string text)
        {
            Id = GenerateId();
            StartTime = startTime;
            EndTime = endTime;
            Text = text;
        }

        public Paragraph(Paragraph paragraph)
        {
            Id = GenerateId();
            Number = paragraph.Number;
            Text = paragraph.Text;
            StartTime = new TimeCode(paragraph.StartTime.TimeSpan);
            EndTime = new TimeCode(paragraph.EndTime.TimeSpan);
            StartFrame = paragraph.StartFrame;
            EndFrame = paragraph.EndFrame;
            Forced = paragraph.Forced;
            Extra = paragraph.Extra;
            IsComment = paragraph.IsComment;
            Actor = paragraph.Actor;
        }

        public Paragraph(int startFrame, int endFrame, string text)
        {
            Id = GenerateId();
            StartTime = new TimeCode(0, 0, 0, 0);
            EndTime = new TimeCode(0, 0, 0, 0);
            StartFrame = startFrame;
            EndFrame = endFrame;
            Text = text;
        }

        public Paragraph(string text, double startTotalMilliseconds, double endTotalMilliseconds)
        {
            Id = GenerateId();
            StartTime = new TimeCode(TimeSpan.FromMilliseconds(startTotalMilliseconds));
            EndTime = new TimeCode(TimeSpan.FromMilliseconds(endTotalMilliseconds));
            Text = text;
        }

        private string GenerateId()
        {
            return Guid.NewGuid().ToString();
        }

        public void Adjust(double factor, double adjust)
        {
            double seconds = StartTime.TimeSpan.TotalSeconds * factor + adjust;
            StartTime.TimeSpan = TimeSpan.FromSeconds(seconds);

            seconds = EndTime.TimeSpan.TotalSeconds * factor + adjust;
            EndTime.TimeSpan = TimeSpan.FromSeconds(seconds);
        }

        public void CalculateFrameNumbersFromTimeCodes(double frameRate)
        {
            StartFrame = (int)Math.Round((StartTime.TotalMilliseconds / 1000.0 * frameRate));
            EndFrame = (int)Math.Round((EndTime.TotalMilliseconds / 1000.0 * frameRate));
        }

        public void CalculateTimeCodesFromFrameNumbers(double frameRate)
        {
            StartTime.TotalMilliseconds = StartFrame * (1000.0D / frameRate);
            EndTime.TotalMilliseconds = EndFrame * (1000.0D / frameRate);
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
                return Utilities.GetNumberOfLines(Text);
            }
        }

        public double WordsPerMinute
        {
            get
            {
                if (string.IsNullOrEmpty(Text))
                    return 0;
                int wordCount = Utilities.RemoveHtmlTags(Text, true).Split(new[] { ' ', ',', '.', '!', '?', ';', ':', '(', ')', '[', ']', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Length;
                return (60.0 / Duration.TotalSeconds) * wordCount;
            }
        }
    }
}