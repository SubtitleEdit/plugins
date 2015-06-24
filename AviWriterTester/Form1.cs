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
            var path = textBoxPath.Text.Trim('\"').Trim();
            if (File.Exists(path))
            {
                var content = File.ReadAllText(path);
                IPlugin plugins = new Nikse.SubtitleEdit.PluginLogic.AviWriter();
                var output = plugins.DoAction(null, content, Configuration.CurrentFrameRate, Configuration.ListViewLineSeparatorString, path, null, content);
                if (content != output)
                {
                    MessageBox.Show("Something changed");
                }
            }
        }
    }
}
