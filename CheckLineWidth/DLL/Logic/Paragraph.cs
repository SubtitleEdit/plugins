using System;

namespace Nikse.SubtitleEdit.PluginLogic
{
    internal class Paragraph
    {
        internal int Number { get; set; }
        internal string Text { get; set; }
        internal TimeCode StartTime { get; set; }
        internal TimeCode EndTime { get; set; }
        internal TimeCode Duration
        {
            get
            {
                var timeCode = new TimeCode(EndTime.TimeSpan);
                timeCode.AddTime(-StartTime.TotalMilliseconds);
                return timeCode;
            }
        }
        internal int StartFrame { get; set; }
        internal int EndFrame { get; set; }
        internal bool Forced { get; set; }
        internal string Extra { get; set; }
        internal bool IsComment { get; set; }
        internal string Actor { get; set; }

        internal Paragraph()
        {
            StartTime = new TimeCode(TimeSpan.FromSeconds(0));
            EndTime = new TimeCode(TimeSpan.FromSeconds(0));
            Text = string.Empty;
        }

        internal Paragraph(TimeCode startTime, TimeCode endTime, string text)
        {
            StartTime = startTime;
            EndTime = endTime;
            Text = text;
        }

        internal Paragraph(Paragraph paragraph)
        {
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

        internal Paragraph(int startFrame, int endFrame, string text)
        {
            StartTime = new TimeCode(0, 0, 0, 0);
            EndTime = new TimeCode(0, 0, 0, 0);
            StartFrame = startFrame;
            EndFrame = endFrame;
            Text = text;
        }

        internal Paragraph(string text, double startTotalMilliseconds, double endTotalMilliseconds)
        {
            StartTime = new TimeCode(TimeSpan.FromMilliseconds(startTotalMilliseconds));
            EndTime = new TimeCode(TimeSpan.FromMilliseconds(endTotalMilliseconds));
            Text = text;
        }

        internal void Adjust(double factor, double adjust)
        {
            double seconds = StartTime.TimeSpan.TotalSeconds * factor + adjust;
            StartTime.TimeSpan = TimeSpan.FromSeconds(seconds);

            seconds = EndTime.TimeSpan.TotalSeconds * factor + adjust;
            EndTime.TimeSpan = TimeSpan.FromSeconds(seconds);
        }

        internal void CalculateFrameNumbersFromTimeCodes(double frameRate)
        {
            StartFrame = (int)Math.Round((StartTime.TotalMilliseconds / 1000.0 * frameRate));
            EndFrame = (int)Math.Round((EndTime.TotalMilliseconds / 1000.0 * frameRate));
        }

        internal void CalculateTimeCodesFromFrameNumbers(double frameRate)
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

        internal int NumberOfLines
        {
            get
            {
                if (string.IsNullOrEmpty(Text))
                    return 0;
                return Text.Length - Text.Replace(Environment.NewLine, string.Empty).Length;
            }
        }

        internal double WordsPerMinute
        {
            get
            {
                if (string.IsNullOrEmpty(Text))
                    return 0;
                int wordCount = Utilities.RemoveHtmlTags(Text).Split((" ,.!?;:()[]" + Environment.NewLine).ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Length;
                return (60.0 / Duration.TotalSeconds) * wordCount;
            }
        }
    }
}