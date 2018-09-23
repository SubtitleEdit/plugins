using Nikse.SubtitleEdit.PluginLogic;
using System.Collections.Generic;

namespace OnlineCasing.Forms
{
    internal class CasingContext
    {
        public List<string> Names { get; set; }
        public bool CheckLastLine { get; set; }
        public bool UppercaseAfterLineBreak { get; set; }
        public List<Paragraph> Paragraphs { get; set; }
    }
}