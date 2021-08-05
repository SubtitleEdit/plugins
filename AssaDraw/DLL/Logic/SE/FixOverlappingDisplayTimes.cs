using System;

namespace SubtitleEdit.Logic
{
    public class FixOverlappingDisplayTimes
    {
        public static class Language
        {
            public static string FixOverlappingDisplayTime { get; set; } = "Fix overlapping display times";
            public static string StartTimeLaterThanEndTime { get; set; } = "Text number {0}: Start time is later than end time: {4}{1} -> {2} {3}";
            public static string UnableToFixStartTimeLaterThanEndTime { get; set; } = "Unable to fix text number {0}: Start time is later than end time: {1}";
            public static string FixOverlappingDisplayTimes { get; set; } = "Fix overlapping display times";
            public static string XFixedToYZ { get; set; } = "{0} fixed to: {1}{2}";
            public static string UnableToFixTextXY { get; set; } = "Unable to fix text number {0}: {1}";
        }
        
        public void Fix(Subtitle subtitle)
        {
            // negative display time
            string fixAction = Language.FixOverlappingDisplayTime;
            int noOfOverlappingDisplayTimesFixed = 0;
            for (int i = 0; i < subtitle.Paragraphs.Count; i++)
            {
                var p = subtitle.Paragraphs[i];
                var oldP = new Paragraph(p);
                if (p.Duration.TotalMilliseconds < 0) // negative display time...
                {
                    var prev = subtitle.GetParagraphOrDefault(i - 1);
                    var next = subtitle.GetParagraphOrDefault(i + 1);

                    double wantedDisplayTime = Utilities.GetOptimalDisplayMilliseconds(p.Text) * 0.9;

                    if (next == null || next.StartTime.TotalMilliseconds > p.StartTime.TotalMilliseconds + wantedDisplayTime)
                    {
                        p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + wantedDisplayTime;
                    }
                    else if (next.StartTime.TotalMilliseconds > p.StartTime.TotalMilliseconds + 500.0)
                    {
                        p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + 500.0;
                    }
                    else if (prev == null || next.StartTime.TotalMilliseconds - wantedDisplayTime > prev.EndTime.TotalMilliseconds)
                    {
                        p.StartTime.TotalMilliseconds = next.StartTime.TotalMilliseconds - wantedDisplayTime;
                        p.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds - 1;
                    }
                }
            }

            // overlapping display time
            for (int i = 1; i < subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = subtitle.Paragraphs[i];
                Paragraph prev = subtitle.GetParagraphOrDefault(i - 1);
                Paragraph target = prev;
                string oldCurrent = p.ToString();
                string oldPrevious = prev.ToString();
                double prevWantedDisplayTime = Utilities.GetOptimalDisplayMilliseconds(prev.Text);
                double currentWantedDisplayTime = Utilities.GetOptimalDisplayMilliseconds(p.Text);
                double prevOptimalDisplayTime = Utilities.GetOptimalDisplayMilliseconds(prev.Text);
                double currentOptimalDisplayTime = Utilities.GetOptimalDisplayMilliseconds(p.Text);
                bool canBeEqual = false;

                double diff = prev.EndTime.TotalMilliseconds - p.StartTime.TotalMilliseconds;
                if (!prev.StartTime.IsMaxTime && !p.StartTime.IsMaxTime && diff >= 0 && !(canBeEqual && Math.Abs(diff) < 0.001))
                {
                    int diffHalf = (int)(diff / 2);
                    if (Math.Abs(p.StartTime.TotalMilliseconds - prev.EndTime.TotalMilliseconds) < 0.001 &&
                        prev.Duration.TotalMilliseconds > 100)
                    {
                        if (!canBeEqual)
                        {
                            bool okEqual = true;
                            if (prev.Duration.TotalMilliseconds > Utilities.SubtitleMinimumDisplayMilliseconds)
                            {
                                prev.EndTime.TotalMilliseconds--;
                            }
                            else if (p.Duration.TotalMilliseconds > Utilities.SubtitleMinimumDisplayMilliseconds)
                            {
                                p.StartTime.TotalMilliseconds++;
                            }
                            else
                            {
                                okEqual = false;
                            }

                            if (okEqual)
                            {
                                noOfOverlappingDisplayTimesFixed++;
                            }
                        }
                    }
                    else if (prevOptimalDisplayTime <= (p.StartTime.TotalMilliseconds - prev.StartTime.TotalMilliseconds))
                    {
                        prev.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds - 1;
                        if (canBeEqual)
                        {
                            prev.EndTime.TotalMilliseconds++;
                        }

                        noOfOverlappingDisplayTimesFixed++;
                    }
                    else if (diff > 0 && currentOptimalDisplayTime <= p.Duration.TotalMilliseconds - diffHalf &&
                             prevOptimalDisplayTime <= prev.Duration.TotalMilliseconds - diffHalf)
                    {
                        prev.EndTime.TotalMilliseconds -= diffHalf;
                        p.StartTime.TotalMilliseconds = prev.EndTime.TotalMilliseconds + 1;
                        noOfOverlappingDisplayTimesFixed++;
                    }
                    else if (currentOptimalDisplayTime <= p.EndTime.TotalMilliseconds - prev.EndTime.TotalMilliseconds)
                    {
                        p.StartTime.TotalMilliseconds = prev.EndTime.TotalMilliseconds + 1;
                        if (canBeEqual)
                        {
                            p.StartTime.TotalMilliseconds = prev.EndTime.TotalMilliseconds;
                        }

                        noOfOverlappingDisplayTimesFixed++;
                    }
                    else if (diff > 0 && currentWantedDisplayTime <= p.Duration.TotalMilliseconds - diffHalf &&
                             prevWantedDisplayTime <= prev.Duration.TotalMilliseconds - diffHalf)
                    {
                        prev.EndTime.TotalMilliseconds -= diffHalf;
                        p.StartTime.TotalMilliseconds = prev.EndTime.TotalMilliseconds + 1;
                        noOfOverlappingDisplayTimesFixed++;
                    }
                    else if (prevWantedDisplayTime <= (p.StartTime.TotalMilliseconds - prev.StartTime.TotalMilliseconds))
                    {
                        prev.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds - 1;
                        if (canBeEqual)
                        {
                            prev.EndTime.TotalMilliseconds++;
                        }

                        noOfOverlappingDisplayTimesFixed++;
                    }
                    else if (currentWantedDisplayTime <= p.EndTime.TotalMilliseconds - prev.EndTime.TotalMilliseconds)
                    {
                        p.StartTime.TotalMilliseconds = prev.EndTime.TotalMilliseconds + 1;
                        if (canBeEqual)
                        {
                            p.StartTime.TotalMilliseconds = prev.EndTime.TotalMilliseconds;
                        }

                        noOfOverlappingDisplayTimesFixed++;
                    }
                    else if (Math.Abs(p.StartTime.TotalMilliseconds - prev.EndTime.TotalMilliseconds) < 10 && p.Duration.TotalMilliseconds > 1)
                    {
                        prev.EndTime.TotalMilliseconds -= 2;
                        p.StartTime.TotalMilliseconds = prev.EndTime.TotalMilliseconds + 1;
                        if (canBeEqual)
                        {
                            p.StartTime.TotalMilliseconds = prev.EndTime.TotalMilliseconds;
                        }

                        noOfOverlappingDisplayTimesFixed++;
                    }
                    else if (Math.Abs(p.StartTime.TotalMilliseconds - prev.StartTime.TotalMilliseconds) < 10 && Math.Abs(p.EndTime.TotalMilliseconds - prev.EndTime.TotalMilliseconds) < 10)
                    { // merge lines with same time codes
                        prev.Text = prev.Text.Replace(Environment.NewLine, " ");
                        p.Text = p.Text.Replace(Environment.NewLine, " ");

                        string stripped = Utilities.RemoveHtmlTags(prev.Text).TrimStart();
                        if (!stripped.StartsWith("- ", StringComparison.Ordinal))
                        {
                            prev.Text = "- " + prev.Text.TrimStart();
                        }

                        stripped = Utilities.RemoveHtmlTags(p.Text).TrimStart();
                        if (!stripped.StartsWith("- ", StringComparison.Ordinal))
                        {
                            p.Text = "- " + p.Text.TrimStart();
                        }

                        prev.Text = prev.Text.Trim() + Environment.NewLine + p.Text;
                        p.Text = string.Empty;
                        noOfOverlappingDisplayTimesFixed++;

                        p.StartTime.TotalMilliseconds = prev.EndTime.TotalMilliseconds + 1;
                        p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + 1;
                        if (canBeEqual)
                        {
                            p.StartTime.TotalMilliseconds = prev.EndTime.TotalMilliseconds;
                            p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds;
                        }
                    }

                }
            }
        }

    }
}
