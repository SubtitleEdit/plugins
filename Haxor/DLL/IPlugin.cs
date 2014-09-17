using System.Windows.Forms;

namespace Nikse.SubtitleEdit.PluginLogic
{
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
        /// <param name="parentForm">Main form in Subtitle Edit</param>
        /// <param name="subtitle">SubRip text</param>
        /// <param name="frameRate">Current frame rate</param>
        /// <param name="subtitleFileName">Current subtitle file name</param>
        /// <param name="videoFileName">Current video file name</param>
        /// <param name="rawText">Subtitle raw format</param>
        /// <returns>Dialog</returns>
        string DoAction(Form parentForm, string subtitle, double frameRate, string listViewLineSeparatorString, string subtitleFileName, string videoFileName, string rawText);
    }
}
