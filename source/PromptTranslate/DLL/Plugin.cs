using System;
using System.Collections.Generic;
using System.Windows.Forms;
using PromptTranslate.Logic;
using WebViewTranslate;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public class PromptTranslate : IPlugin // dll file name must "<classname>.dll" - e.g. "Haxor.dll"
    {
        string IPlugin.Name
        {
            get { return "PROMPT translate"; }
        }

        string IPlugin.Text
        {
            get { return "Translate with PROMPT..."; } // text in menu
        }

        decimal IPlugin.Version
        {
            get { return 0.3M; }
        }

        string IPlugin.Description
        {
            get { return "PROMPT translate: .net 4.5+"; }
        }

        string IPlugin.ActionType // Can be one of these: file, tool, sync, translate, spellcheck
        {
            get { return "translate"; }
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

            var list = new List<string>();
            foreach (string line in subtitle.Replace(Environment.NewLine, "\n").Split('\n'))
                list.Add(line);

            var sub = new Subtitle();
            var srt = new SubRip();
            srt.LoadSubtitle(sub, list, subtitleFileName);
            using (var form = new MainForm(sub, (this as IPlugin).Text, (this as IPlugin).Description, parentForm))
            {
                if (form.ShowDialog(parentForm) == DialogResult.OK)
                    return form.FixedSubtitle;
            }
            return string.Empty;
        }
    }
}