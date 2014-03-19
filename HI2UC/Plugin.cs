using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.PluginLogic.HI2UC
{
    public class HI2UC : IPlugin // dll file name must "<classname>.dll" - e.g. "Haxor.dll"
    {
        string IPlugin.ActionType // Can be one of these: file, tool, sync, translate, spellcheck
        {
            get { return "tool"; }
        }

        string IPlugin.Description
        {
            get { return "Convert all the text inside: \"[]\", \"()\" and \"{}\" to Uppercase"; }
        }

        string IPlugin.Name
        {
            get { return "Hearing Impaired to Uppercase"; }
        }

        string IPlugin.Shortcut
        {
            get { return string.Empty; }
        }

        string IPlugin.Text
        {
            get { return "Hearing Impaired to Uppercase"; }
        }

        //Gets or sets the major, minor, build, and revision numbers of the assembly.
        decimal IPlugin.Version
        {
            get { return 2.8M; }
        }

        string IPlugin.DoAction(Form parentForm, string subtitle, double frameRate, string listViewLineSeparatorString, string subtitleFileName, string videoFileName, string rawText)
        {
            subtitle = subtitle.Trim();
            if (!string.IsNullOrEmpty(subtitle))
            {
                if (!string.IsNullOrEmpty(listViewLineSeparatorString))
                    Configuration.ListViewLineSeparatorString = listViewLineSeparatorString;

                var list = new List<string>();
                foreach (string line in subtitle.Replace(Environment.NewLine, "|").Split("|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
                    list.Add(line);

                Subtitle sub = new Subtitle();
                SubRip srt = new SubRip();
                srt.LoadSubtitle(sub, list, subtitleFileName);
                using (var form = new PluginForm(parentForm, sub, (this as IPlugin).Name, (this as IPlugin).Description))
                {
                    if (form.ShowDialog(parentForm) == DialogResult.OK)
                        return form.FixedSubtitle;
                }
                return string.Empty;
            }

            MessageBox.Show("No subtitle loaded", parentForm.Text,
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return string.Empty;
        }
    }
}