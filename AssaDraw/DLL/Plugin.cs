using System.Windows.Forms;
using SubtitleEdit.Logic;
using AssaDraw;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public class AssaDraw : IPlugin // dll file name must "<classname>.dll" - e.g. "Haxor.dll"
    {
        string IPlugin.Name => "ASSA Draw";

        string IPlugin.Text => "ASSA Draw..."; // will be used in context menu item

        decimal IPlugin.Version => 0.1M;

        string IPlugin.Description => "Draw shapes for Advanced Sub Station Alpha";

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
            var text = string.Empty;
            Paragraph p = null;
            var selectedLines = AdvancedSubStationAlpha.GetTag("SelectedLines", "[Script Info]", sub.Header);
            if (!string.IsNullOrEmpty(selectedLines))
            {
                var arr = selectedLines.Split(new char[] { ',', ':' }, System.StringSplitOptions.RemoveEmptyEntries);
                if (arr.Length > 1 && int.TryParse(arr[1], out var index))
                {
                    p = sub.GetParagraphOrDefault(index);
                    if (p != null)
                    {
                        text = p.Text;
                    }
                }
            }
            using (var form = new FormAssaDrawMain(text))
            {
                if (form.ShowDialog(parentForm) == DialogResult.OK)
                {
                    if (p != null && !string.IsNullOrEmpty(form.AssaDrawCodes))
                    {
                        p.Text = form.AssaDrawCodes;
                    }

                    return sub.ToText(assa);
                }
            }

            return string.Empty;
        }
    }
}