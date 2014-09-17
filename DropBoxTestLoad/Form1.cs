using System;
using System.Windows.Forms;
using Nikse.SubtitleEdit.PluginLogic;

namespace SeDropBoxTestLoad
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            IPlugin plugin = new SeDropBoxLoad();
            textBox1.Text = plugin.DoAction(null, string.Empty, 25, "<br />", "test.srt", "test.avi", "this is a test2");
        }
    }
}
