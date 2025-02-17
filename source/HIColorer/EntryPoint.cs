using System.Diagnostics;
using System.Globalization;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace Nikse.SubtitleEdit.PluginLogic
{
    /// <summary>
    /// Provides functionality for highlighting and modifying subtitles specifically for Hearing Impaired (HI) purposes.
    /// </summary>
    public class HIColorer : IPlugin
    {
        string IPlugin.Name => "HI Colorer";

        string IPlugin.Text => "HI Colorer";

        decimal IPlugin.Version => 2M;

        string IPlugin.Description => "Set color for Hearing Impaired annotations (by Ivandrofly)";

        string IPlugin.ActionType => "tool";

        string IPlugin.Shortcut => string.Empty;

        string IPlugin.DoAction(Form parentForm, string subtitle, double frameRate, string listViewLineSeparatorString, string subtitleFileName, string videoFileName, string rawText)
        {
#if DEBUG
            if (!Debugger.IsAttached)
            {
                Debugger.Launch();
            }
#endif
            subtitle = subtitle.Trim();
            if (string.IsNullOrEmpty(subtitle))
            {
                MessageBox.Show("No subtitle loaded", parentForm.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return string.Empty;
            }

            var subRip = new SubRip();
            var sub = new Subtitle();
            subRip.LoadSubtitle(sub, subtitle.SplitToLines(), subtitleFileName);
            if (sub.Paragraphs.Count < 1)
            {
                return string.Empty;
            }

            using (var mainForm = new MainForm(sub, subtitleFileName, (this as IPlugin).Version.ToString(CultureInfo.InvariantCulture)))
            {
                if (mainForm.ShowDialog() == DialogResult.OK) return mainForm.Subtitle;
            }

            return string.Empty;
        }
    }

    /// <summary>
    /// Represents a plugin interface for subtitle editing functionality.
    /// </summary>
    public interface IPlugin
    {
        string Name { get; }
        string Text { get; }
        decimal Version { get; }
        string Description { get; }
        string ActionType { get; }
        string Shortcut { get; }
        string DoAction(Form parentForm, string subtitle, double frameRate, string listViewLineSeparatorString, string subtitleFileName, string videoFileName, string rawText);
    }
}