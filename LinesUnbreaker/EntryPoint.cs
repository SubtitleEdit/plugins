using System.Windows.Forms;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public class LinesUnbreaker : IPlugin
    {
        // Metadata
        string IPlugin.Name => "Lines Unbreaker";
        string IPlugin.Text => "Lines Unbreaker";
        decimal IPlugin.Version => 2.3M;
        string IPlugin.Description => "Helps unbreaking unnecessary shorten lines.";
        string IPlugin.ActionType => "tool";
        string IPlugin.Shortcut => string.Empty;

        string IPlugin.DoAction(Form parentForm, string subtitle, double frameRate, string listViewLineSeparatorString, string subtitleFileName, string videoFileName, string rawText)
        {
            if (string.IsNullOrWhiteSpace(subtitle))
            {
                MessageBox.Show("No subtitle loaded", parentForm.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return string.Empty;
            }

            if (!string.IsNullOrEmpty(listViewLineSeparatorString))
            {
                Options.UILineBreak = listViewLineSeparatorString;
            }

            var list = subtitle.SplitToLines();

            var srt = new SubRip();
            var sub = new Subtitle(srt);
            srt.LoadSubtitle(sub, list, subtitleFileName);
            using (var form = new PluginForm(sub))
            {
                if (form.ShowDialog(parentForm) == DialogResult.OK)
                {
                    return form.Subtiitle;
                }
            }
            return string.Empty;
        }
    }
}