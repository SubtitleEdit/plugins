using System.Windows.Forms;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public class SeDropBoxLoad : IPlugin // dll file name must "<classname>.dll" - e.g. "SyncViaOtherSubtitle.dll"
    {

        string IPlugin.Name => "Load from Dropbox";

        string IPlugin.Text => "Load subtitle from Dropbox..."; // text in interface

        decimal IPlugin.Version => 1.7M;

        string IPlugin.Description => "Load subtitle from Dropbox";

        string IPlugin.ActionType => "file"; // Can be one of these: file, tool, sync, translate, spellcheck

        string IPlugin.Shortcut => string.Empty;

        string IPlugin.DoAction(Form parentForm, string subtitle, double frameRate, string listViewLineSeparatorString, string subtitleFileName, string videoFileName, string rawText)
        {
            var form = new PluginForm((this as IPlugin).Name, (this as IPlugin).Description);
            if (form.ShowDialog(parentForm) == DialogResult.OK)
            {
                return form.LoadedSubtitle;
            }
            return string.Empty;
        }
    }
}
