using System;
using System.Linq;
using System.Windows.Forms;
using Nikse.SubtitleEdit.PluginLogic.Logic;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public class AmericanToBritish : IPlugin
    {
        string IPlugin.Name
        {
            get { return "American to British"; }
        }

        string IPlugin.Text
        {
            get { return "Convert American to British..."; }
        }

        decimal IPlugin.Version
        {
            get { return 0.4M; }
        }

        string IPlugin.Description
        {
            get { return "Converts American English to British English"; }
        }

        string IPlugin.ActionType
        {
            get { return "translate"; }
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

            var list = subtitle.Replace(Environment.NewLine, "\n").Split('\n').ToList();

            var sub = new Subtitle();
            var srt = new SubRip();
            srt.LoadSubtitle(sub, list, subtitleFileName);
            if (srt.ErrorCount > 0)
            {
                MessageBox.Show(srt.Errors + " Errors found while parsing .srt",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            using (var form = new PluginForm(sub, (this as IPlugin).Name, (this as IPlugin).Description))
            {
                if (form.ShowDialog(parentForm) == DialogResult.OK)
                    return form.FixedSubtitle;
            }
            return string.Empty;
        }
    }
}