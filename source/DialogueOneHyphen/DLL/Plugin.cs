using System.Collections.Generic;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public class DiaOneHyphen : IPlugin // dll file name must "<classname>.dll" - e.g. "SyncViaOtherSubtitle.dll"
    {
        string IPlugin.Name => "Dialogue one hyphen only";

        string IPlugin.Text => "Dialogues - remove hyphen in first line in dialogues...";

        decimal IPlugin.Version => 0.7M;

        string IPlugin.Description => "Removes hyphens in first line in dialogues";

        // Can be one of these: file, tool, sync, translate, spellcheck
        string IPlugin.ActionType => "tool";

        string IPlugin.Shortcut => string.Empty;

        string IPlugin.DoAction(Form parentForm, string subtitle, double frameRate, string listViewLineSeparatorString,
                                string subtitleFileName, string videoFileName, string rawText)
        {
            if (string.IsNullOrWhiteSpace(subtitle))
            {
                MessageBox.Show("No subtitle loaded", parentForm.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return string.Empty;
            }

            // set frame rate
            Configuration.CurrentFrameRate = frameRate;

            // set newline visualizer for listviews
            if (!string.IsNullOrEmpty(listViewLineSeparatorString))
                Configuration.ListViewLineSeparatorString = listViewLineSeparatorString;

            // load subtitle text into object
            var list = new List<string>(subtitle.SplitToLines());

            var srt = new SubRip();
            var sub = new Subtitle(srt);
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