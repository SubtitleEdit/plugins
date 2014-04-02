using Nikse.SubtitleEdit.PluginLogic;
using System;
using System.IO;
using System.Windows.Forms;

namespace TmdbCasingTester
{
    public partial class Form1 : Form
    {
        private string file = null;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //IPlugin plugin = new TmdbCasing();
            IPlugin plugin = new DialogueAutoMarker();
            string content = @"1
00:01:02,939 --> 00:01:05,349
(THUNDER RUMBLING)

2
00:01:16,119 --> 00:01:18,070
(CHATTERING)

3
00:01:51,946 --> 00:01:55,775
MAN 1: I'm dying of thirst over here.
Come on! Come on!

4
00:01:56,451 --> 00:01:58,319
- Here you are.
MAN 2: Thank you kindly.

5
00:01:58,494 --> 00:01:59,494
(LAUGHING)";
            if (this.file == null)
                plugin.DoAction(null, content, 23D, "</br>", null, null, content);
            else
            {
                string nContent = File.ReadAllText(file);
                plugin.DoAction(null, nContent, 23, "</br>", nContent, null, content);
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var opf = new OpenFileDialog() { Filter = "Subrip(*.srt)|*.srt" })
            {
                if (opf.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    this.file = opf.FileName;
            }
        }
    }
}