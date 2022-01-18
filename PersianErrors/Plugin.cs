using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public class PersianErrors : IPlugin // dll file name must "<classname>.dll" - e.g. "Haxor.dll"
    {
        string IPlugin.Name
        {
            get { return "Persian Subtitle Fixes"; }
        }

        string IPlugin.Text
        {
            get { return "Persian Subtitle Fixes"; }
        }

        decimal IPlugin.Version
        {
            get { return 0.5M; }
        }

        string IPlugin.Description
        {
            get { return "Makes Persian subtitles nice and tidy!"; }
        }

        string IPlugin.ActionType // Can be one of these: file, tool, sync, translate, spellcheck
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
            if (!string.IsNullOrEmpty(subtitle))
            {
                // set newline visualizer for listviews
                if (!string.IsNullOrEmpty(listViewLineSeparatorString))
                    Configuration.ListViewLineSeparatorString = listViewLineSeparatorString;

                // load subtitle text into object
                var list = new List<string>();
                foreach (string line in subtitle.Replace(Environment.NewLine, "|").Split("|".ToCharArray(), StringSplitOptions.None))
                    list.Add(line);
                Subtitle sub = new Subtitle();
                SubRip srt = new SubRip();
                srt.LoadSubtitle(sub, list, subtitleFileName);

                using (var form = new MainForm(sub, (this as IPlugin).Name, (this as IPlugin).Description, parentForm))
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