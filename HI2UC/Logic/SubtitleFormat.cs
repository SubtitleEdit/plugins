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

        public string FriendlyName
        {
            get
            {
                return string.Format("{0} ({1})", Name, Extension);
            }
        }

        public virtual bool HasStyleSupport
        {
            get
            {
                return false;
            }
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

        abstract public bool IsMine(List<string> lines, string fileName);

        abstract public void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName);

        public virtual void RemoveNativeFormatting(Subtitle subtitle)
        {
        }

        abstract public string ToText(Subtitle subtitle, string title);
    }
}