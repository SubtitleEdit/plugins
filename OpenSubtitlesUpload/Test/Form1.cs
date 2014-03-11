using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Nikse.SubtitleEdit.PluginLogic;

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
            string subtitleFileName = textBox1.Text;
            plugin.DoAction(null, string.Empty, 25, "<br />", subtitleFileName, @"C:\Users\Nikse\Desktop\Trailers\Game Of Thrones Season Trailer.mp4", System.IO.File.ReadAllText(subtitleFileName));
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textBox1.Text = openFileDialog1.FileName ;
                button1_Click(null, null);
            }
        }
    }
}
