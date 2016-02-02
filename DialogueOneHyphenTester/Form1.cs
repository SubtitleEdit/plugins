using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Nikse.SubtitleEdit.PluginLogic;

namespace DialogueOneHyphenTester
{
    public partial class Form1 : Form
    {
        private readonly IPlugin _plugin = new DiaOneHyphen();
        public Form1()
        {
            InitializeComponent();
        }

        private void RunPlugin(string rawText, string path)
        {
            _plugin.DoAction(null, rawText, 25, "<br />", null, path, rawText);
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
                        RunPlugin(System.IO.File.ReadAllText(fileName, Encoding.UTF8), fileName);
                }
            }
        }
    }
}
