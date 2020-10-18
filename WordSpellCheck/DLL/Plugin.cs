using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public class WordSpellCheck : IPlugin // dll file name must "<classname>.dll" - e.g. "SyncViaOtherSubtitle.dll"
    {

        string IPlugin.Name => "Word spell check";

        string IPlugin.Text => "Spell and grammar check in Microsoft Word...";

        decimal IPlugin.Version => 1.7M;

        string IPlugin.Description => "Word spell check (requires Microsoft Word)";

        string IPlugin.ActionType => "spellcheck"; // Can be one of these: file, tool, sync, translate, spellcheck

        string IPlugin.Shortcut => string.Empty;

        string IPlugin.DoAction(Form parentForm, string subtitle, double frameRate, string listViewLineSeparatorString, string subtitleFileName, string videoFileName, string rawText)
        {
            subtitle = subtitle.Trim();
            if (string.IsNullOrWhiteSpace(subtitle))
            {
                MessageBox.Show("No subtitle loaded", parentForm.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return string.Empty;
            }
            if (!IsOfficeInstalled())
            {
                MessageBox.Show(@"Microsoft Office (Word) is not installed in this system.", "Office is not installed!!!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return string.Empty;
            }

            // set frame rate
            Configuration.CurrentFrameRate = frameRate;

            // set newline visualizer for listviews
            if (!string.IsNullOrEmpty(listViewLineSeparatorString))
            {
                Configuration.ListViewLineSeparatorString = listViewLineSeparatorString;
            }

            // load subtitle text into object
            var list = new List<string>();
            foreach (string line in subtitle.Replace(Environment.NewLine, "\n").Split('\n'))
            {
                list.Add(line);
            }

            var sub = new Subtitle();
            var srt = new SubRip();
            srt.LoadSubtitle(sub, list, subtitleFileName);
            var form = new PluginForm(sub, (this as IPlugin).Name, (this as IPlugin).Description);
            if (form.ShowDialog(parentForm) == DialogResult.OK)
            {
                return form.FixedSubtitle;
            }
            return string.Empty;
        }

        private bool IsOfficeInstalled()
        {
            var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\Winword.exe");
            key?.Close();
            return key != null;
        }
    }
}
