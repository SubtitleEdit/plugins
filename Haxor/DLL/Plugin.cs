using SubtitleEdit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public class Haxor : IPlugin // dll file name must "<classname>.dll" - e.g. "Haxor.dll"
    {
        string IPlugin.Name => "Haxor";

        string IPlugin.Text => "Translate to Haxor";

        decimal IPlugin.Version => 1.5M;

        string IPlugin.Description => "Translates to haxor";

        string IPlugin.ActionType => "translate"; // Can be one of these: file, tool, sync, translate, spellcheck

        string IPlugin.Shortcut => string.Empty;

        string IPlugin.DoAction(Form parentForm, string subtitle, double frameRate, string listViewLineSeparatorString, string subtitleFileName, string videoFileName, string rawText)
        {
#if DEBUG
            if (!Debugger.IsAttached)
            {
                Debugger.Launch();
            }
#endif
            subtitle = subtitle.Trim();
            if (string.IsNullOrEmpty(subtitle))
            {
                MessageBox.Show("No subtitle loaded", parentForm.Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return string.Empty;
            }

            if (!string.IsNullOrEmpty(listViewLineSeparatorString))
            {
                Configuration.ListViewLineSeparatorString = listViewLineSeparatorString;
            }

            var lines = new List<string>();
            foreach (string line in subtitle.Replace(Environment.NewLine, "\n").Split('\n'))
            {
                lines.Add(line);
            }

            var sub = new Subtitle();
            var srt = new SubRip();
            srt.LoadSubtitle(sub, lines, subtitleFileName);
            using (var mainForm = new MainForm(sub, (this as IPlugin).Text, (this as IPlugin).Description, parentForm))
            {
                if (mainForm.ShowDialog(parentForm) == DialogResult.OK)
                {
                    return mainForm.TransformedSubtitle;
                }
            }
            return string.Empty;
        }
    }
}