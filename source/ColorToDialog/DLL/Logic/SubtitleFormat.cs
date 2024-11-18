using System.Collections.Generic;

namespace ColorToDialog.Logic
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
    }
}