using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public class LibSePlugin : IPlugin
    {
        public string Name { get; } = "LibSePlugin";
        public string Text { get; } = "LibSePlugin";
        public decimal Version { get; } = 1.0m;
        public string Description { get; } = "Plugin based on libse nuget";
        public string ActionType { get; } = "tool";
        public string Shortcut { get; } = "";

        public string DoAction(Form parentForm, string srtText, double frameRate, string uiLineBreak, string file,
            string videoFile,
            string rawText)
        {
#if DEBUG
            if (!Debugger.IsAttached)
            {
                Debugger.Launch();
            }
#endif
            var subrip = new SubRip();
            var subtitle = new Subtitle();
            subrip.LoadSubtitle(subtitle, srtText.SplitToLines(), file);
            
            
            // var subtitle = new Subtitle();
            // var format = subtitle.LoadSubtitle(rawText, out _, Encoding.UTF8);
            // if (format == null) return string.Empty;


            return subrip.ToText(subtitle, "");
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

        string DoAction(Form parentForm,
            string srtText,
            double frameRate,
            string uiLineBreak,
            string file,
            string videoFile,
            string rawText);
    }
}