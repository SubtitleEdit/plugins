using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Nikse.SubtitleEdit.PluginLogic;

namespace plugin_tester
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private string _fileName = string.Empty;
        private void buttonWithFile_Click(object sender, EventArgs e)
        {
            /*if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                _fileName = openFileDialog1.FileName;
                RunPlugin(File.ReadAllText(_fileName, Encoding.UTF8));
            }*/
            var file = @"C:\Users\Ivandrofly\Subtitles\the-world-made-straight_HI_english-1055347\The.World.Made.Straight.2015.720p.BluRay.x264-ROVERS-HI.srt";
            RunPlugin(File.ReadAllText(file, Encoding.UTF8));
        }

        private void buttonFromTextBox_Click(object sender, EventArgs e)
        {
            string text = textBox1.Text.Trim();
            if (text.Length == 0)
                return;
            RunPlugin(text);
        }

        private void RunPlugin(string content)
        {
            IPlugin plugin = new HIColorer();
            string outString = plugin.DoAction(this, content, 23.796, "<br />", _fileName.Length > 0 ? Path.GetFileName(_fileName) : null, null, content);
            if (content.Equals(outString))
                MessageBox.Show("Nothing changed!!!");
        }
    }
}
