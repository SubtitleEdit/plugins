using System;
using System.Linq;
using System.Windows.Forms;
using Nikse.SubtitleEdit.PluginLogic.Logic;
using Nikse.SubtitleEdit.PluginLogic.Logic.SubtitleFormats;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public class MergeTwoSrtToAss : IPlugin
    {
        string IPlugin.Name
        {
            get { return "Merge two SRT files to one ASS/SSA"; } // name in plugin window
        }

        string IPlugin.Text
        {
            get { return "Merge two SRT to one ASS/SSA..."; } // text in tools menu
        }

        decimal IPlugin.Version
        {
            get { return 1.1M; }
        }

        string IPlugin.Description
        {
            get { return "Merge two SRT subtitles to one ASS (eg. for different languages)"; }
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
            subtitle = subtitle.Trim();
            if (string.IsNullOrEmpty(subtitle))
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
            using (var form = new PluginForm(sub, (this as IPlugin).Name, (this as IPlugin).Description, parentForm))
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