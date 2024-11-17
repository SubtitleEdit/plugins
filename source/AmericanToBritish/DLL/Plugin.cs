using Nikse.SubtitleEdit.PluginLogic.Logic;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public class AmericanToBritish : IPlugin
    {
        string IPlugin.Name
        {
            get { return "American to British"; }
        }

        string IPlugin.Text
        {
            get { return "Convert American to British..."; }
        }

        decimal IPlugin.Version
        {
            get { return 0.5M; }
        }

        string IPlugin.Description
        {
            get { return "Converts American English to British English"; }
        }

        string IPlugin.ActionType
        {
            get { return "translate"; }
        }

        string IPlugin.Shortcut
        {
            get { return string.Empty; }
        }

        string IPlugin.DoAction(Form parentForm, string subtitle, double frameRate, string listViewLineSeparatorString, string subtitleFileName, string videoFileName, string rawText)
        {
            if (string.IsNullOrWhiteSpace(subtitle))
            {
                MessageBox.Show("No subtitle loaded", parentForm.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return string.Empty;
            }

            if (!string.IsNullOrEmpty(listViewLineSeparatorString))
                Configuration.ListViewLineSeparatorString = listViewLineSeparatorString;

            var srt = new SubRip();
            var sub = new Subtitle(srt);
            srt.LoadSubtitle(sub, subtitle.SplitToLines(), subtitleFileName);
            if (srt.Errors > 0)
            {
                var s = srt.Errors > 1 ? "s" : string.Empty;
                MessageBox.Show($"{srt.Errors} error{s} found while parsing .srt", $"SubRip Parse Error{s}", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return string.Empty;
            }

            using (var form = new PluginForm(sub, (this as IPlugin).Name, (this as IPlugin).Description))
            {
                if (form.ShowDialog(parentForm) == DialogResult.OK)
                    return form.FixedSubtitle;
            }
            return string.Empty;
        }

    }
}
