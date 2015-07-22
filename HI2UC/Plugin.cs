using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public class HI2UC : IPlugin // dll file name must "<classname>.dll" - e.g. "Haxor.dll"
    {
        string IPlugin.ActionType // Can be one of these: file, tool, sync, translate, spellcheck
        {
            get { return "tool"; }
        }

        string IPlugin.Description
        {
            get { return "Convert moods and Narrator to Uppercase"; }
        }

        string IPlugin.Name
        {
            get { return "Hearing Impaired to Uppercase"; }
        }

        string IPlugin.Shortcut
        {
            get { return string.Empty; }
        }

        string IPlugin.Text
        {
            get { return "Hearing Impaired to Uppercase"; }
        }

        //Gets or sets the major, minor, build, and revision numbers of the assembly.
        decimal IPlugin.Version
        {
            get { return 3.1M; }
        }

        string IPlugin.DoAction(Form parentForm, string subtitleText, double frameRate,
            string listViewLineSeparatorString, string subtitleFileName, string videoFileName, string rawText)
        {
            subtitleText = subtitleText.Trim();

            // Make sure subtitle isn't null or empty
            if (string.IsNullOrEmpty(subtitleText))
            {
                MessageBox.Show("No subtitle loaded", parentForm.Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return string.Empty;
            }

            // Check if subtitle contains Hearing Impaired notations
            if (subtitleText.IndexOf('(') < 0 && subtitleText.IndexOf(']') < 0)
            {
                var result = MessageBox.Show("Subtitle doesn't contians Hearing Impaired anotations!" + Environment.NewLine +
                    "Do you want to continue?",
                    "Hearing Impaired not found!", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.No)
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

            // Load raws subtitle lines into object
            srt.LoadSubtitle(sub, list, subtitleFileName);
            using (var form = new PluginForm(parentForm, sub, (this as IPlugin).Name, (this as IPlugin).Description))
            {
                if (form.ShowDialog(parentForm) == DialogResult.OK)
                    return form.FixedSubtitle;
            }
            return string.Empty;
        }
    }
}