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

        public bool IsFrameBased => !IsTimeBased;

        public string FriendlyName => $"{Name} ({Extension})";

        public int ErrorCount => _errorCount;

        abstract public bool IsMine(List<string> lines, string fileName);

        abstract public string ToText(Subtitle subtitle, string title);

        abstract public void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName);

        public virtual void RemoveNativeFormatting(Subtitle subtitle)
        {
        }

        public virtual List<string> AlternateExtensions => new List<string>();

        public static int MillisecondsToFrames(double milliseconds)
        {
            return (int)System.Math.Round(milliseconds / (TimeCode.BaseUnit / Configuration.CurrentFrameRate));
        }

        public static int FramesToMilliseconds(double frames)
        {
            return (int)System.Math.Round(frames * (TimeCode.BaseUnit / Configuration.CurrentFrameRate));
        }

        public virtual bool HasStyleSupport => false;

    }
}