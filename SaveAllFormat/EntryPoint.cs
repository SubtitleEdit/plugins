using System.Windows.Forms;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public class ExportAllFormats : EntryPointBase
    {
        public ExportAllFormats()
            : base("Export to all formats", "Export to all formats (non binary)", 1.1m, "Export current subtitle to all available text format.", "file", string.Empty)
        {
        }

        public override string DoAction(Form parentForm, string srtText, double frameRate, string uiLineBreak, string file, string videoFile, string rawText)
        {
            // subtitle not loaded
            if (string.IsNullOrWhiteSpace(srtText))
            {
                MessageBox.Show(parentForm, "Empty subtitle... make sure you have load a subtitle file before trying to export!", "Empty subtitle", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return string.Empty;
            }

            // initialize context
            Init(srtText, uiLineBreak, file);

            // show main form
            using (var main = new Main(parentForm, file))
            {
                if (main.ShowDialog(parentForm) == DialogResult.Cancel)
                {
                    return string.Empty;
                }
            }

            return string.Empty;

        }

    }
}
