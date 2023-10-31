using System.Diagnostics;
using Nikse.SubtitleEdit.PluginLogic.Views;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public class TightHyphen : EntryPointBase
    {
        public TightHyphen() :
            base("TightHyphen", "Tight Hyphen", 1, "Remove space after hyphen in a dialog", "tool", string.Empty)
        {
        }

        public override string DoAction(Form parentForm, string srtText, double frameRate, string uiLineBreak,
            string file, string videoFile, string rawText)
        {
#if DEBUG
            /// <summary>
            /// Launch and attach a debugger to the process if it's not already attached.
            /// This is helpful when a need arises to debug code in the startup sequence of an application
            /// or to debug issues that occur when a debugger isn't already attached.
            /// </summary>
            if (!Debugger.IsAttached)
            {
                Debugger.Launch();
            }
#endif
            Init(srtText, uiLineBreak, file);

            using (var main = new Main(_subtitle))
            {
                if (main.ShowDialog() == DialogResult.OK)
                {
                    return main.Result;
                }
            }

            return string.Empty;
        }
    }
}