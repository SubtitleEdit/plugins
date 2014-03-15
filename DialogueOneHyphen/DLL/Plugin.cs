using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public class DiaOneHyphen : IPlugin // dll file name must "<classname>.dll" - e.g. "SyncViaOtherSubtitle.dll"
    {

        string IPlugin.Name
        {
            get { return "Dialogue one hyphen only"; }
        }

        string IPlugin.Text
        {
            get { return "Dialogues - remove hyphen in first line in dialogues..."; }
        }

        decimal IPlugin.Version
        {
            get { return 0.3M; }
        }

        string IPlugin.Description
        {
            get { return "Removes hyphens in first line in dialoguees"; }
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
                // set frame rate
                Configuration.CurrentFrameRate = frameRate;

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


                var form = new PluginForm(sub, (this as IPlugin).Name, (this as IPlugin).Description);
                if (form.ShowDialog(parentForm) == DialogResult.OK)
                {
                    return form.FixedSubtitle;
                }
            }
            MessageBox.Show("No subtitle loaded", parentForm.Text,
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return string.Empty;
        }
    }
}
