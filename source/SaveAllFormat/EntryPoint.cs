using System.IO;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public class ExportAllFormats : IPlugin
    {
        public string Name { get; } = "Export all formats";
        public string Text { get; } = "Export all formats";
        public decimal Version { get; } = 3.0m;
        public string Description { get; } = "Export current subtitle to all available text format.";
        public string ActionType { get; } = "file";
        public string Shortcut { get; } = string.Empty;

        public string DoAction(Form parentForm, string srtText, double frameRate, string uiLineBreak, string file, string videoFile, string rawText)
        {
            // subtitle not loaded
            if (string.IsNullOrWhiteSpace(srtText))
            {
                MessageBox.Show(parentForm, "Empty subtitle... make sure you have load a subtitle file before trying to export!", "Empty subtitle", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return string.Empty;
            }

            if (!File.Exists(file))
            {
                MessageBox.Show("File not found: " + file, "File not found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return string.Empty;
            }

            var subtitle = Subtitle.Parse(file);
            if (subtitle is null)
            {
                MessageBox.Show("Could not parse subtitle file: " + file, "Could not parse subtitle file", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return string.Empty;
            }
            // initialize context
            // Init(srtText, uiLineBreak, file);

            // show main form
            using (var main = new Main(parentForm, subtitle))
            {
                if (main.ShowDialog(parentForm) == DialogResult.Cancel)
                {
                    return string.Empty;
                }
            }

            return string.Empty;
        }
    }

    public interface IPlugin
    {
        /// <summary>
        /// Name of the plug-in
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Text used in Subtitle Edit menu
        /// </summary>
        string Text { get; }

        /// <summary>
        /// Version number of plugin
        /// </summary>
        decimal Version { get; }

        /// <summary>
        /// Description of what plugin does
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Can be one of these: file, tool, sync, translate, spellcheck
        /// </summary>
        string ActionType { get; }

        /// <summary>
        /// Shortcut used to active plugin - e.g. Control+Shift+F9
        /// </summary>
        string Shortcut { get; }

        /// <summary>
        /// This action of callsed when Subtitle Edit calls plugin
        /// </summary>
        string DoAction(Form parentForm,
            string srtText,
            double frameRate,
            string uiLineBreak,
            string file,
            string videoFile,
            string rawText);
    }
}