using Nikse.SubtitleEdit.PluginLogic;
using System;
using System.Windows.Forms;

namespace OpenSubtitlesUploadTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            IPlugin plugin = new Nikse.SubtitleEdit.PluginLogic.OpenSubtitlesUpload();
            string subtitleFileName = @"D:\uTorrent\I Will Follow You Into the Dark (2012) [1080p]\I.Will.Follow.You.Into.the.Dark.2012.1080p.BluRay.x264.YIFY.srt"; // stextBox1.Text;
            plugin.DoAction(null, string.Empty, 25, "<br />", subtitleFileName, @"D:\uTorrent\Frozen (2013) [1080p]\Frozen.2013.1080p.BluRay.x264.YIFY.mp4", System.IO.File.ReadAllText(subtitleFileName));
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textBox1.Text = openFileDialog1.FileName;
                button1_Click(null, null);
            }
        }
    }
}