using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public class DialogueBalancer : IPlugin
    {
        string IPlugin.Name { get { return "Dialogue Balancer"; } }
        string IPlugin.Text { get { return "Dialogues - balance short lines..."; } }
        decimal IPlugin.Version { get { return 1.09M; } }
        string IPlugin.Description { get { return "Balances two-line dialogue subtitles: merges question and short reply, then re-splits evenly."; } }
        string IPlugin.ActionType { get { return "tool"; } }
        string IPlugin.Shortcut { get { return string.Empty; } }

        string IPlugin.DoAction(Form parentForm, string subtitle, double frameRate, string listViewLineSeparatorString, string subtitleFileName, string videoFileName, string rawText)
        {
            subtitle = subtitle.Trim();
            if (string.IsNullOrEmpty(subtitle))
            {
                MessageBox.Show("No subtitle loaded", parentForm.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return string.Empty;
            }

            Configuration.CurrentFrameRate = frameRate;

            if (!string.IsNullOrEmpty(listViewLineSeparatorString))
                Configuration.ListViewLineSeparatorString = listViewLineSeparatorString;

            var list = new List<string>();
            foreach (string line in subtitle.Replace(Environment.NewLine, "|").Split("|".ToCharArray(), StringSplitOptions.None))
                list.Add(line);

            Subtitle sub = new Subtitle();
            SubRip srt = new SubRip();
            srt.LoadSubtitle(sub, list, subtitleFileName);

            using (var form = new PluginForm(sub, (this as IPlugin).Name, (this as IPlugin).Description))
            {
                if (form.ShowDialog(parentForm) == DialogResult.OK)
                    return form.FixedSubtitle;
            }
            return string.Empty;
        }
    }
}