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

        public override string DoAction(Form parentForm, string srtText, double frameRate, string uiLineBreak, string file, string videoFile, string rawText)
        {
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
