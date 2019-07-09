using System.Globalization;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public class HIColorer : IPlugin
    {
        string IPlugin.Name => "HI Colorer";

        string IPlugin.Text => "HI Colorer";

        decimal IPlugin.Version => 0.5M;

        string IPlugin.Description => "Set color for Hearing Impaired annotations (by Ivandrofly)";

        string IPlugin.ActionType => "tool";

        string IPlugin.Shortcut => string.Empty;

        string IPlugin.DoAction(Form parentForm, string subtitle, double frameRate, string listViewLineSeparatorString, string subtitleFileName, string videoFileName, string rawText)
        {
            subtitle = subtitle.Trim();
            if (string.IsNullOrEmpty(subtitle))
            {
                MessageBox.Show("No subtitle loaded", parentForm.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return string.Empty;
            }

            var subRip = new SubRip();
            var sub = new Subtitle(subRip);
            subRip.LoadSubtitle(sub, subtitle.SplitToLines(), subtitleFileName);
            if (sub.Paragraphs.Count < 1)
            {
                return string.Empty;
            }

            using (var mainForm = new MainForm(sub, subtitleFileName, (this as IPlugin).Version.ToString(CultureInfo.InvariantCulture)))
            {
                if (mainForm.ShowDialog() == DialogResult.OK) return mainForm.Subtitle;
            }

            return string.Empty;
        }
    }
}
