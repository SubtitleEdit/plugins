using System.Windows.Forms;
using System.IO;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public class SeDropBoxSave : IPlugin // dll file name must "<classname>.dll" - e.g. "SyncViaOtherSubtitle.dll"
    {

        string IPlugin.Name
        {
            get { return "Save to Dropbox"; }
        }

        string IPlugin.Text
        {
            get { return "Save subtitle to Dropbox..."; } // text in interface
        }

        decimal IPlugin.Version
        {
            get { return 1.6M; }
        }

        string IPlugin.Description
        {
            get { return "Save subtitle to dropbox"; }
        }

        string IPlugin.ActionType // Can be one of these: file, tool, sync, translate, spellcheck
        {
            get { return "file"; }
        }

        string IPlugin.Shortcut
        {
            get { return string.Empty; }
        }

        string IPlugin.DoAction(Form parentForm, string subtitle, double frameRate, string listViewLineSeparatorString, string subtitleFileName, string videoFileName, string rawText)
        {
            if (!string.IsNullOrEmpty(subtitleFileName))
                subtitleFileName = Path.GetFileName(subtitleFileName);

            var form = new PluginForm(subtitleFileName, rawText, (this as IPlugin).Name, (this as IPlugin).Description);
            form.ShowDialog(parentForm);
            return string.Empty;
        }
    }
}
