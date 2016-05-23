using System.Collections.Generic;
using System.Windows.Forms;
using SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public class RemoveUnicodeCharacters : IPlugin // dll file name must "<classname>.dll" - e.g. "SyncViaOtherSubtitle.dll"
    {
        string IPlugin.Name => "Remove Unicode characters";

        string IPlugin.Text => "Remove Unicode characters...";

        decimal IPlugin.Version => 0.4M;

        string IPlugin.Description => "Remove Unicode characters - or replace them";

        // Can be one of these: file, tool, sync, translate, spellcheck
        string IPlugin.ActionType => "tool";

        string IPlugin.Shortcut => string.Empty;

        string IPlugin.DoAction(Form parentForm, string subtitle, double frameRate, string listViewLineSeparatorString, string subtitleFileName,
            string videoFileName, string rawText)
        {
            if (string.IsNullOrWhiteSpace(subtitle))
            {
                MessageBox.Show("No subtitle loaded", parentForm.Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return string.Empty;
            }
            // set frame rate
            Configuration.CurrentFrameRate = frameRate;

            // set newline visualizer for listviews
            if (!string.IsNullOrEmpty(listViewLineSeparatorString))
                Configuration.ListViewLineSeparatorString = listViewLineSeparatorString;

            // load subtitle text into object
            var list = new List<string>();
            foreach (string line in subtitle.SplitToLines())
                list.Add(line);
            var sub = new Subtitle();
            var srt = new SubRip();
            srt.LoadSubtitle(sub, list, subtitleFileName);

            IPlugin plugin = this;
            using (var form = new PluginForm(sub, plugin.Name, plugin.Description))
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