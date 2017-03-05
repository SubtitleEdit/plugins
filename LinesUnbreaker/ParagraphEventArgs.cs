using System;

namespace Nikse.SubtitleEdit.PluginLogic
{
    class ParagraphEventArgs : EventArgs
    {
        public Paragraph Paragraph { get; }
        public string NewText { get; }

        public ParagraphEventArgs(Paragraph paragraph, string newText)
        {
            Paragraph = paragraph;
            NewText = newText;
        }
    }
}
