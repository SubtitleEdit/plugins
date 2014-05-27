using Nikse.SubtitleEdit.PluginLogic;
using System;
using System.IO;
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
            string subtitleFileName = textBox1.Text;
            if (File.Exists(subtitleFileName))
            plugin.DoAction(null, string.Empty, 25, "<br />", subtitleFileName, Path.GetFullPath(subtitleFileName), System.IO.File.ReadAllText(subtitleFileName));
        }

        private void button2_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Subrip|*.srt";
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textBox1.Text = openFileDialog1.FileName;
                button1_Click(null, null);
            }
        }
    }
}