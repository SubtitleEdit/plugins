using OneDriveLoad;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public class OneDriveLoad : IPlugin // dll file name must "<classname>.dll" - e.g. "SyncViaOtherSubtitle.dll"
    {
        string IPlugin.Name
        {
            get { return "Load from OneDrive"; }
        }

        string IPlugin.Text
        {
            get { return "Load subtitle from OneDrive..."; } // text in interface
        }

        decimal IPlugin.Version
        {
            get { return 1.2M; }
        }

        string IPlugin.Description
        {
            get { return "Load subtitle from OneDrive"; }
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
                if (!string.IsNullOrEmpty(form.LoadedSubtitle))
                    return form.LoadedSubtitle;
            }
            return string.Empty;
        }
    }
}