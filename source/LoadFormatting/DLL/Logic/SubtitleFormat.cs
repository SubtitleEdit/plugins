using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public abstract class SubtitleFormat
    {
        protected int _errorCount;

        internal virtual List<string> AlternateExtensions
        {
            get
            {
                return new List<string>();
            }
        }

        internal int ErrorCount
        {
            get { return _errorCount; }
        }

        abstract internal string Extension
        {
            get;
        }

        abstract internal bool IsTimeBased
        {
            get;
        }

        abstract internal string Name
        {
            get;
        }

        abstract internal bool IsMine(List<string> lines, string fileName);

        abstract internal void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName);

        abstract internal string ToText(Subtitle subtitle, string title);

        public static double GetFrameForCalculation(double frameRate)
        {
            if (Math.Abs(frameRate - 23.976) < 0.001)
            {
                return 24000.0 / 1001.0;
            }
            if (Math.Abs(frameRate - 29.97) < 0.001)
            {
                return 30000.0 / 1001.0;
            }
            if (Math.Abs(frameRate - 59.94) < 0.001)
            {
                return 60000.0 / 1001.0;
            }

            return frameRate;
        }

        public static int MillisecondsToFramesMaxFrameRate(double milliseconds)
        {
            var frames = (int)Math.Round(milliseconds / (1000.0 / GetFrameForCalculation(25)), MidpointRounding.AwayFromZero);
            if (frames >= 25)
            {
                frames = (int)(25 - 0.01);
            }

            return frames;
        }

        public static int FramesToMillisecondsMax999(double frames)
        {
            var ms = (int)Math.Round(frames * (1000.0 / GetFrameForCalculation(25)), MidpointRounding.AwayFromZero);
            return Math.Min(ms, 999);
        }
    }
}