using System;
using System.Linq;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public class CheckLineWidth : IPlugin
    {
        string IPlugin.Name
        {
            get { return "Check line width"; }
        }

        string IPlugin.Text
        {
            get { return "Check line width..."; }
        }

        decimal IPlugin.Version
        {
            get { return 0.7M; }
        }

        string IPlugin.Description
        {
            get { return "Check width of lines with specific font"; }
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

            var srt = new SubRip();
            var sub = new Subtitle(srt);
            srt.LoadSubtitle(sub, list, subtitleFileName);
            if (srt.ErrorCount > 0)
            {
                MessageBox.Show(srt.ErrorCount + " Errors found while parsing .srt",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            using (var form = new PluginForm(sub, (this as IPlugin).Name, (this as IPlugin).Description, parentForm, srt.ErrorCount))
            {
                if (form.ShowDialog(parentForm) == DialogResult.OK)
                    return form.FixedSubtitle;
            }
            return string.Empty;
        }
    }
}