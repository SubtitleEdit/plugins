using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.PluginLogic
{
    internal abstract class SubtitleFormat
    {
        protected int _errorCount;

        abstract public string Extension
        {
            get;
        }

        abstract public string Name
        {
            get;
        }

        abstract public bool IsTimeBased
        {
            get;
        }

        public string FriendlyName
        {
            get
            {
                return string.Format("{0} ({1})", Name, Extension);
            }
        }

        public int ErrorCount
        {
            get { return _errorCount; }
        }

        abstract public bool IsMine(List<string> lines, string fileName);

        abstract public string ToText(Subtitle subtitle, string title);

        abstract public void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName);

        public virtual List<string> AlternateExtensions
        {
            get
            {
                return new List<string>();
            }
        }

        public static int FramesToMillisecondsMax999(double frames)
        {
            int ms = (int)Math.Round(frames * (TimeCode.BaseUnit / 25));
            return Math.Min(ms, 999);
        }
    }
}