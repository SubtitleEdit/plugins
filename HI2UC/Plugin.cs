using System.Collections.Generic;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public class HI2UC : IPlugin // dll file name must "<classname>.dll" - e.g. "Haxor.dll"
    {
        #region Metadata

        string IPlugin.ActionType => "tool";// Can be one of these: file, tool, sync, translate, spellcheck

        string IPlugin.Description => "Convert moods and Narrator to Uppercase";

        string IPlugin.Name => "Hearing Impaired to Uppercase";

        string IPlugin.Shortcut => string.Empty;

        string IPlugin.Text => "Hearing Impaired to Uppercase";

        decimal IPlugin.Version => 3.2M; //Gets or sets the major, minor, build, and revision numbers of the assembly

        #endregion

        string IPlugin.DoAction(Form parentForm, string subtitleText, double frameRate,
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
            foreach (string line in subtitleText.SplitToLines())
                list.Add(line);

            var sub = new Subtitle();
            var srt = new SubRip();

            // Load raws subtitle lines into Subtitle object
            srt.LoadSubtitle(sub, list, subtitleFileName);

            IPlugin HI2UC = (this as IPlugin);
            using (var form = new PluginForm(parentForm, sub, HI2UC.Name, HI2UC.Description))
            {
                if (form.ShowDialog(parentForm) == DialogResult.OK)
                    return form.FixedSubtitle;
            }
            return string.Empty;
        }
    }
}