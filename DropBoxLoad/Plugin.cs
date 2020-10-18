using System.Windows.Forms;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public class SeDropBoxLoad : IPlugin // dll file name must "<classname>.dll" - e.g. "SyncViaOtherSubtitle.dll"
    {

        string IPlugin.Name
        {
            get { return "Load from Dropbox"; }
        }

        string IPlugin.Text
        {
            get { return "Load subtitle from Dropbox..."; } // text in interface
        }

        decimal IPlugin.Version
        {
            get { return 1.6M; }
        }

        string IPlugin.Description
        {
            get { return "Load subtitle from Dropbox"; }
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
            var form = new PluginForm((this as IPlugin).Name, (this as IPlugin).Description);
            if (form.ShowDialog(parentForm) == DialogResult.OK)
            {
                return form.LoadedSubtitle;
            }
            return string.Empty;
        }
    }
}
