using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public class TriggerOnLoad : IPlugin
    {
        public string Name => "Trigger";

        public string Text => "Trigger";

        public decimal Version => 1.0m;

        public string Description => "Run on loaded";

        public string ActionType => "tool";

        public string Shortcut { get; }

        public TriggerOnLoad()
        {
            File.WriteAllText("d:/file.txt", "hello world");
            System.Diagnostics.Debug.WriteLine("instance created...");
        }

        public string DoAction(Form parentForm, string srtText, double frameRate, string uiLineBreak, string file, string videoFile, string rawText)
        {
            return string.Empty;
        }
    }
}
