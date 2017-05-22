using System;
using System.Linq;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public class NarratorOutParentheses : IPlugin
    {
        string IPlugin.Name
        {
            get { return "Narrator Out Parentheses"; }
        }

        string IPlugin.Text
        {
            get { return "Narrator Out Parentheses"; }
        }

        decimal IPlugin.Version
        {
            get { return 0.3M; }
        }

        string IPlugin.Description
        {
            get { return "Find and Convert narrator inside ()/[] to: Narrator:"; }
        }

        string IPlugin.ActionType
        {
            get { return "tool"; }
        }

        string IPlugin.Shortcut
        {
            get { return string.Empty; }
        }

        string IPlugin.DoAction(Form parentForm, string subtitle, double frameRate, string listViewLineSeparatorString, string subtitleFileName, string videoFileName, string rawText)
        {
            if (string.IsNullOrWhiteSpace(subtitle))
            {
                MessageBox.Show("No subtitle loaded", parentForm.Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return string.Empty;
            }

            if (!string.IsNullOrEmpty(listViewLineSeparatorString))
                Configuration.ListViewLineSeparatorString = listViewLineSeparatorString;

            var list = subtitle.SplitToLines().ToList();

            var sub = new Subtitle();
            var srt = new SubRip();
            srt.LoadSubtitle(sub, list, subtitleFileName);
            if (srt.ErrorCount > 0)
            {
                MessageBox.Show(srt.Errors + " Errors found while parsing .srt",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            using (var form = new MainForm(sub, (this as IPlugin).Name, (this as IPlugin).Description))
            {
                if (form.ShowDialog(parentForm) == DialogResult.OK)
                    return form.FixedSubtitle;
            }
            return string.Empty;
        }
    }
}
