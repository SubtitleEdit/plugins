using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public class DialogueAutoMarker : IPlugin
    {
        string IPlugin.ActionType // Can be one of these: file, tool, sync, translate, spellcheck
        {
            get { return "tool"; }
        }

        string IPlugin.Description
        {
            get { return "[Beta]"; }
        }

        string IPlugin.Name
        {
            get { return "Dialogue AutoMarker"; }
        }

        string IPlugin.Shortcut
        {
            get { return string.Empty; }
        }

        string IPlugin.Text
        {
            get { return "Dialogue AutoMarker"; }
        }

        //Gets or sets the major, minor, build, and revision numbers of the assembly.
        decimal IPlugin.Version
        {
            get { return 0.2M; }
        }

        string IPlugin.DoAction(Form parentForm, string subtitle, double frameRate, string listViewLineSeparatorString, string subtitleFileName, string videoFileName, string rawText)
        {
            if (string.IsNullOrEmpty(subtitle))
            {
                MessageBox.Show("No subtitle loaded", parentForm.Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return string.Empty;
            }

            if (!string.IsNullOrEmpty(listViewLineSeparatorString))
            {
                Options.UILineBreak = listViewLineSeparatorString;
            }

            var list = new List<string>();
            foreach (string line in subtitle.SplitToLines())
            {
                list.Add(line);
            }

            var srt = new SubRip();
            var sub = new Subtitle(srt);
            srt.LoadSubtitle(sub, list, subtitleFileName);
            using (var form = new Main(parentForm, sub, (this as IPlugin).Name, (this as IPlugin).Description))
            {
                if (form.ShowDialog(parentForm) == DialogResult.OK)
                {
                    return form.FixedSubtitle;
                }
            }
            return string.Empty;
        }
    }
}
