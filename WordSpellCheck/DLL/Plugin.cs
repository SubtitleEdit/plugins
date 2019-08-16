using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public class WordSpellCheck : IPlugin // dll file name must "<classname>.dll" - e.g. "SyncViaOtherSubtitle.dll"
    {

        string IPlugin.Name
        {
            get { return "Word spell check"; }
        }

        string IPlugin.Text
        {
            get { return "Spell and grammer check in Microsoft Word..."; }
        }

        decimal IPlugin.Version
        {
            get { return 1.4M; }
        }

        string IPlugin.Description
        {
            get { return "Word spell check (requires Microsoft Word)"; }
        }

        string IPlugin.ActionType // Can be one of these: file, tool, sync, translate, spellcheck
        {
            get { return "spellcheck"; }
        }

        string IPlugin.Shortcut
        {
            get { return string.Empty; }
        }

        string IPlugin.DoAction(Form parentForm, string subtitle, double frameRate, string listViewLineSeparatorString, string subtitleFileName, string videoFileName, string rawText)
        {
            subtitle = subtitle.Trim();
            if (string.IsNullOrWhiteSpace(subtitle))
            {
                MessageBox.Show("No subtitle loaded", parentForm.Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                Configuration.ListViewLineSeparatorString = listViewLineSeparatorString;

            // load subtitle text into object
            var list = new List<string>();
            foreach (string line in subtitle.Replace(Environment.NewLine, "\n").Split('\n'))
                list.Add(line);
            Subtitle sub = new Subtitle();
            SubRip srt = new SubRip();
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
            RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\Winword.exe");
            if (key != null)
            {
                key.Close();
            }
            return key != null;
        }
    }
}
