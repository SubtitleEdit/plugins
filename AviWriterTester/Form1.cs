using System;
using System.Windows.Forms;
using Nikse.SubtitleEdit.PluginLogic;
using System.IO;
namespace AviWriterTester
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void buttonRun_Click(object sender, EventArgs e)
        {
            var filePath = @"C:\Users\ivandro\Subtitles\legendas_tv_20140610074215\Devils.Knot.2013.1080p.BluRay.x264.YIFY.srt";
            if (File.Exists(filePath))
            {
                var content = File.ReadAllText(filePath);
                IPlugin plugins = new Nikse.SubtitleEdit.PluginLogic.AviWriter();
                var output = plugins.DoAction(null, content, Configuration.CurrentFrameRate, Configuration.ListViewLineSeparatorString, filePath, null, content);
                if (content != output)
                {
                    MessageBox.Show("Something changed");
                }
            }
        }
    }
}
