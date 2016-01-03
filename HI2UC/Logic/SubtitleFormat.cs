using System.Collections.Generic;

namespace Nikse.SubtitleEdit.PluginLogic
{
    internal abstract class SubtitleFormat
    {
        protected int _errorCount;

        public int ErrorCount => _errorCount;

        abstract public string Extension
        {
            get;
        }

        public string FriendlyName => $"{Name} ({Extension})";

        public virtual bool HasStyleSupport => false;

        public bool IsFrameBased => !IsTimeBased;

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