using System.Collections.Generic;
using System.Windows.Forms;

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

        public string DoAction(Form parentForm, string subtitle, double frameRate, string listViewLineSeparatorString, string subtitleFileName, string videoFileName, string rawText)
        {
            // Make sure subtitle isn't null or empty
            if (string.IsNullOrWhiteSpace(subtitle))
            {
                MessageBox.Show("No subtitle loaded", parentForm.Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return string.Empty;
            }

            // Use custom separator for list view new lines
            if (!string.IsNullOrEmpty(listViewLineSeparatorString))
                Configuration.ListViewLineSeparatorString = listViewLineSeparatorString;

            // Get subtitle raw lines
            var list = new List<string>();
            list.AddRange(subtitle.SplitToLines());

            var srt = new SubRip();
            var sub = new Subtitle(srt);

            // Load raws subtitle lines into Subtitle object
            srt.LoadSubtitle(sub, list, subtitleFileName);

            IPlugin wordCensor = this;
            using (var form = new Main(sub, wordCensor.Name, wordCensor.Description))
            {
                if (form.ShowDialog(parentForm) == DialogResult.OK)
                    return form.FixedSubtitle;
            }
            return string.Empty;
        }
    }
}
