using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Nikse.SubtitleEdit.PluginLogic.Logic;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public class HIColorer : IPlugin
    {
        string IPlugin.Name
        {
            get { return "HI Colorer"; }
        }

        string IPlugin.Text
        {
            get { return "HI Colorer"; }
        }

        decimal IPlugin.Version
        {
            get { return 0.4M; }
        }

        string IPlugin.Description
        {
            get { return "Set color for Hearing Impaired annotations (by Ivandrofly)"; }
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
            var lines = subtitle.SplitToLines();
            var sub = new Subtitle();
            var subRip = new SubRip();
            subRip.LoadSubtitle(sub, lines, subtitleFileName);
            if (sub.Paragraphs.Count < 1)
                return string.Empty;

            using (var mainForm = new MainForm(sub, subtitleFileName, (this as IPlugin).Version.ToString()))
            {
                if (mainForm.ShowDialog() == DialogResult.OK)
                    return mainForm.FixedSubtitle;
            }
            return string.Empty;
        }
    }
}
