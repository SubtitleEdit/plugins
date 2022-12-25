using System.Windows.Forms;
using System.IO;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public class SeDropBoxSave : IPlugin // dll file name must "<classname>.dll" - e.g. "SyncViaOtherSubtitle.dll"
    {

        string IPlugin.Name => "Save to Dropbox";

        string IPlugin.Text => "Save subtitle to Dropbox..."; // text in interface

        decimal IPlugin.Version => 1.7M;

        string IPlugin.Description => "Save subtitle to dropbox";

        string IPlugin.ActionType => "file"; // Can be one of these: file, tool, sync, translate, spellcheck

        string IPlugin.Shortcut => string.Empty;

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
