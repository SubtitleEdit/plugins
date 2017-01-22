using System.Windows.Forms;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public class OcrListManager : IPlugin // dll file name must "<classname>.dll" - e.g. "Haxor.dll"
    {
        string IPlugin.Name
        {
            get { return "OcrListManager"; }
        }

        string IPlugin.Text
        {
            get { return "OCR Fix replace list manager"; }
        }

        decimal IPlugin.Version
        {
            get { return 1.0M; }
        }

        string IPlugin.Description
        {
            get { return "Add/delete element from the OCR fix replace list"; }
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
            using (var form = new Form1())
            {
                form.ShowDialog(parentForm);
            }
            return string.Empty;
        }
    }
}