using SubtitleEdit;
using System.Windows.Forms;
using SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public class AssaFade : IPlugin // dll file name must "<classname>.dll" - e.g. "Haxor.dll"
    {
        string IPlugin.Name => "ASSA Fade";

        string IPlugin.Text => "Fade..."; // will be used in context menu item

        decimal IPlugin.Version => 0.4M;

        string IPlugin.Description => "Fade effect for Advanced Sub Station Alpha";

        // Can be one of these: File, Tool, Sync, Translate, SpellCheck, AssaTool
        string IPlugin.ActionType => "AssaTool";

        string IPlugin.Shortcut => string.Empty;

        string IPlugin.DoAction(Form parentForm, string subtitle, double frameRate, string listViewLineSeparatorString, string subtitleFileName, string videoFileName, string rawText)
        {
            subtitle = subtitle.Trim();
            if (string.IsNullOrEmpty(subtitle))
            {
                MessageBox.Show("No subtitle loaded", parentForm.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return string.Empty;
            }

            if (!string.IsNullOrEmpty(listViewLineSeparatorString))
            {
                Configuration.ListViewLineSeparatorString = listViewLineSeparatorString;
            }

            var sub = new Subtitle();
            var assa = new AdvancedSubStationAlpha();
            assa.LoadSubtitle(sub, rawText.SplitToLines(), subtitleFileName);
            using (var form = new MainForm(sub, (this as IPlugin).Name, (this as IPlugin).Description, parentForm))
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