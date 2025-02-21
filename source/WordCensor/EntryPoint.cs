using System.Collections.Generic;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public class WordCensor : IPlugin
    {
        public string Name => "Word censor";

        public string Text => "Word censor";

        public decimal Version => 1m;

        public string Description => "Censor offensive words";

        public string ActionType => "tool";

        public string Shortcut => string.Empty;

        public string DoAction(Form parentForm, string content, double frameRate, string listViewLineSeparatorString, string subtitleFileName, string videoFileName, string rawText)
        {
            // Make sure subtitle isn't null or empty
            if (string.IsNullOrWhiteSpace(content))
            {
                MessageBox.Show("No subtitle loaded", parentForm.Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return string.Empty;
            }

            // Use custom separator for list view new lines
            if (!string.IsNullOrEmpty(listViewLineSeparatorString))
            {
                Configuration.ListViewLineSeparatorString = listViewLineSeparatorString;
            }

            var subRip = new SubRip();
            var subrip = new Subtitle();
            subRip.LoadSubtitle(subrip, content.SplitToLines(), subtitleFileName);
            if (subrip.Paragraphs.Count < 1)
            {
                return string.Empty;
            }

            using (var form = new Main(subrip, Name, Description))
            {
                if (form.ShowDialog(parentForm) == DialogResult.OK)
                {
                    return form.FixedSubtitle;
                }
            }

            return string.Empty;
        }
    }

    /// <summary>
    /// Represents a plugin interface for subtitle editing functionality.
    /// </summary>
    public interface IPlugin
    {
        string Name { get; }
        string Text { get; }
        decimal Version { get; }
        string Description { get; }
        string ActionType { get; }
        string Shortcut { get; }
        string DoAction(Form parentForm, string content, double frameRate, string listViewLineSeparatorString, string subtitleFileName, string videoFileName, string rawText);
    }
}