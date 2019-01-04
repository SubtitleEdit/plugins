using System.Linq;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public class NarratorOutParentheses : IPlugin
    {
        string IPlugin.Name
        {
            get { return "Narrator Out Parentheses"; }
        }

        string IPlugin.Text
        {
            get { return "Narrator Out Parentheses"; }
        }

        decimal IPlugin.Version
        {
            get { return 0.4M; }
        }

        string IPlugin.Description
        {
            get { return "Find and Convert narrator inside ()/[] to: Narrator:"; }
        }

        string IPlugin.ActionType
        {
            get { return "tool"; }
        }

        string IPlugin.Shortcut
        {
            get { return string.Empty; }
        }

        string IPlugin.DoAction(Form parentForm, string content, double frameRate, string uiLineBreak, string fileName, string videoFileName, string rawText)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                MessageBox.Show("No subtitle loaded", parentForm.Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return string.Empty;
            }

            if (string.IsNullOrEmpty(uiLineBreak))
            {
                Options.UILineBreak = uiLineBreak;
            }

            var list = content.SplitToLines().ToList();
            var subrip = new SubRip();
            var subtitle = new Subtitle(subrip);
            subrip.LoadSubtitle(subtitle, list, fileName);
            using (var form = new MainForm(subtitle, (this as IPlugin).Name, (this as IPlugin).Description))
            {
                if (form.ShowDialog(parentForm) == DialogResult.OK)
                {
                    return form.Subtitle;
                }
            }
            return string.Empty;
        }
    }
}
