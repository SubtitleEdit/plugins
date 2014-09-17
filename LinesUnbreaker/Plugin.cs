using System;
using System.Linq;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public class LinesUnbreaker : IPlugin
    {
        string IPlugin.Name
        {
            get { return "Lines Unbreaker"; }
        }

        string IPlugin.Text
        {
            get { return "Lines Unbreaker"; }
        }

        decimal IPlugin.Version
        {
            get { return 1.2M; }
        }

        string IPlugin.Description
        {
            get { return "Helps breaking short lines"; }
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
            subtitle = subtitle.Trim();
            if (string.IsNullOrEmpty(subtitle))
            {
                MessageBox.Show("No subtitle loaded", parentForm.Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return string.Empty;
            }

            if (!string.IsNullOrEmpty(listViewLineSeparatorString))
                Configuration.ListViewLineSeparatorString = listViewLineSeparatorString;

            var list = subtitle.Replace(Environment.NewLine, "\n").Split('\n').ToList();

            var sub = new Subtitle();
            var srt = new SubRip();
            srt.LoadSubtitle(sub, list, subtitleFileName);
            if (srt.ErrorCount > 0)
            {
                MessageBox.Show(srt.Errors + " Errors found while parsing .srt",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            using (var form = new PluginForm(sub, (this as IPlugin).Name, (this as IPlugin).Description, parentForm))
            {
                if (form.ShowDialog(parentForm) == DialogResult.OK)
                    return form.FixedSubtitle;
            }
            return string.Empty;
        }
    }
}