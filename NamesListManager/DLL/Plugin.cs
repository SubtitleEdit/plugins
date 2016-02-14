using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public class NamesListManager : IPlugin // dll file name must "<classname>.dll" - e.g. "Haxor.dll"
    {
        string IPlugin.Name
        {
            get { return "NamesListManager"; }
        }

        string IPlugin.Text
        {
            get { return "Names list manager"; }
        }

        decimal IPlugin.Version
        {
            get { return 1.0M; }
        }

        string IPlugin.Description
        {
            get { return "Add/delete names from names_etc list"; }
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

            if (!string.IsNullOrEmpty(listViewLineSeparatorString))
                Configuration.ListViewLineSeparatorString = listViewLineSeparatorString;

            var list = new List<string>();
            foreach (string line in subtitle.Replace(Environment.NewLine, "\n").Split('\n'))
                list.Add(line);

            var sub = new Subtitle();
            var srt = new SubRip();
            srt.LoadSubtitle(sub, list, subtitleFileName);
            using (var form = new Form1())
            {
                form.ShowDialog(parentForm);
            }
            return string.Empty;
        }
    }
}