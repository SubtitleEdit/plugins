using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public class HIColorer : IPlugin
    {
        string IPlugin.Name
        {
            get { throw new NotImplementedException(); }
        }

        string IPlugin.Text
        {
            get { throw new NotImplementedException(); }
        }

        decimal IPlugin.Version
        {
            get { throw new NotImplementedException(); }
        }

        string IPlugin.Description
        {
            get { throw new NotImplementedException(); }
        }

        string IPlugin.ActionType
        {
            get { throw new NotImplementedException(); }
        }

        string IPlugin.Shortcut
        {
            get { throw new NotImplementedException(); }
        }

        string IPlugin.DoAction(System.Windows.Forms.Form parentForm, string subtitle, double frameRate, string listViewLineSeparatorString, string subtitleFileName, string videoFileName, string rawText)
        {
            subtitle = subtitle.Trim();
            if (string.IsNullOrEmpty(subtitle))
            {
                MessageBox.Show("No subtitle loaded", parentForm.Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return string.Empty;
            }
            var lines = new List<String>();
            lines = subtitle.Replace(Environment.NewLine, "\n").Split('\n').ToList();
            if (lines.Count < 1)
            {
                return string.Empty;
            }
            var sub = new Subtitle();
            var subRip = new SubRip();
            subRip.LoadSubtitle(sub, lines, subtitleFileName);
            if (sub.Paragraphs.Count < 1)
            {
                return string.Empty;
            }
            using (var mainForm = new MainForm(sub, subtitleFileName))
            {
                if (mainForm.ShowDialog() == DialogResult.OK)
                {
                    return mainForm.FixedSubtitle;
                }
            }
            return string.Empty;
        }
    }
}
