using System;
using System.Windows.Forms;
using Nikse.SubtitleEdit.PluginLogic;
using System.IO;
using System.Text;

namespace WordSpellCheckTester
{
    public partial class Form1 : Form
    {
        private readonly IPlugin _plugin = new CheckLineWidth();

        const string RawText = @"1
-00:00:24,275 --> 00:00:27,362
-iii iii iii ii ii iiii ii
-WWW WWW WWW ww ww WWWW WW.
-
-2
-00:02:30,944 --> 00:02:33,279
-Don't apologize.
-
-3
-00:02:44,833 --> 00:02:47,001
-I favor the flavor!
-
-4
-00:02:47,126 --> 00:02:49,170
-apologize
-apologize";

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Subrip (.srt)|*.srt";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    var fileName = ofd.FileName;
                    if (fileName.EndsWith(".srt", StringComparison.OrdinalIgnoreCase))
                        RunPlugin(File.ReadAllText(fileName, Encoding.UTF8), fileName);
                }
            }
        }


        private void RunPlugin(string rawText, string path)
        {
            _plugin.DoAction(null, rawText, 25, "<br />", null, path, rawText);
        }
    }
}
