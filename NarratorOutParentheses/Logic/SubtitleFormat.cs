using System.Collections.Generic;

namespace Nikse.SubtitleEdit.PluginLogic
{
    internal abstract class SubtitleFormat
    {
        protected int _errorCount;

        public virtual List<string> AlternateExtensions
        {
            get
            {
                return new List<string>();
            }
        }

        public int ErrorCount
        {
            get { return _errorCount; }
        }

        abstract public string Extension
        {
            get;
        }

        public bool IsFrameBased
        {
            get
            {
                return !IsTimeBased;
            }
        }

        abstract public bool IsTimeBased
        {
            get;
        }

        abstract public string Name
        {
            get;
        }

        public static int MillisecondsToFrames(double milliseconds)
        {
            return (int)System.Math.Round(milliseconds / (1000.0 / Configuration.CurrentFrameRate));
        }

        abstract public bool IsMine(List<string> lines, string fileName);

        abstract public void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName);

        abstract public string ToText(Subtitle subtitle, string title);
    }
}