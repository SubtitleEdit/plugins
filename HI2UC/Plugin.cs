namespace Nikse.SubtitleEdit.PluginLogic
{
    using PluginCoreLib.Subtitle;
    using System.Collections.Generic;
    using System.Windows.Forms;

    public class HI2UC : IPlugin // dll file name must "<classname>.dll" - e.g. "Haxor.dll"
    {
        #region Metadata

        public string ActionType => "tool";// Can be one of these: file, tool, sync, translate, spellcheck

        public string Description => "Convert moods and Narrator to Uppercase";

        public string Name => "Hearing Impaired to Uppercase";

        public string Shortcut => string.Empty;

        public string Text => "Hearing Impaired to Uppercase";

        public decimal Version => 3.3M; //Gets or sets the major, minor, build, and revision numbers of the assembly

        #endregion

        public string DoAction(Form parentForm, string subtitleText, double frameRate,
            string listViewLineSeparatorString, string subtitleFileName, string videoFileName, string rawText)
        {
            // Make sure subtitle isn't null or empty
            if (string.IsNullOrWhiteSpace(subtitleText))
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
            list.AddRange(subtitleText.SplitToLines());

            var srt = new SubRip();
            var sub = new Subtitle();

            // Load raws subtitle lines into Subtitle object
            srt.LoadSubtitle(sub, list, subtitleFileName);

            IPlugin HI2UC = this;
            using (var form = new PluginForm(sub, HI2UC.Name, HI2UC.Description))
            {
                if (form.ShowDialog(parentForm) == DialogResult.OK)
                    return form.FixedSubtitle;
            }
            return string.Empty;
        }
    }
}