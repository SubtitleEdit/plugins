using OpenSubtitlesUpload;
using System.IO;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public class OpenSubtitlesUpload : IPlugin // dll file name must "<classname>.dll" - e.g. "SyncViaOtherSubtitle.dll"
    {
        string IPlugin.Name
        {
            get { return "Upload subtitle to OpenSubtitles.org"; } // name
        }

        string IPlugin.Text
        {
            get { return "Upload subtitles to OpenSubtitles.org..."; } // text in interface
        }

        decimal IPlugin.Version
        {
            get { return 1.7M; }
        }

        string IPlugin.Description
        {
            get { return "Upload subtitles to OpenSubtitles.org"; } // description in plugin-window
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
            {
                subtitleFileName = Path.GetFileName(subtitleFileName);
                var form = new PluginForm(subtitleFileName, rawText, videoFileName, (this as IPlugin).Name, (this as IPlugin).Description);
                form.ShowDialog(parentForm);
            }
            else
            {
                MessageBox.Show("No subtitle loaded", parentForm.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return string.Empty;
        }
    }
}