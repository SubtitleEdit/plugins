using System.Collections.Generic;

namespace Nikse.SubtitleEdit.PluginLogic
{
    internal abstract class SubtitleFormat
    {
       
        protected int _errorCount;

        abstract internal string Extension
        {
            get;
        }

        abstract internal string Name
        {
            get;
        }

        abstract internal bool IsTimeBased
        {
            get;
        }

        internal bool IsFrameBased
        {
            get
            {
                return !IsTimeBased;
            }
        }

        internal string FriendlyName
        {
            get
            {
                return string.Format("{0} ({1})", Name, Extension);
            }
        }

        internal int ErrorCount
        {
            get { return _errorCount; }
        }

        abstract internal bool IsMine(List<string> lines, string fileName);

        abstract internal string ToText(Subtitle subtitle, string title);

        abstract internal void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName);

        internal virtual void RemoveNativeFormatting(Subtitle subtitle)
        {
        }

        internal virtual List<string> AlternateExtensions
        {
            get
            {
                return new List<string>();
            }
        }

        internal static int MillisecondsToFrames(double milliseconds)
        {
            return (int)System.Math.Round(milliseconds / (1000.0 / Configuration.CurrentFrameRate));
        }

        internal static int FramesToMilliseconds(double frames)
        {
            return (int)System.Math.Round(frames * (1000.0 / Configuration.CurrentFrameRate));
        }

        internal virtual bool HasStyleSupport
        {
            get
            {
                return false;
            }
        }

    }
}
