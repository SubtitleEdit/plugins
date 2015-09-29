using Nikse.SubtitleEdit.PluginLogic;
using System;
using System.Windows.Forms;

namespace HaxorTester
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private static string Tester(string s)
        {
            IPlugin plugin = new Haxor();
            return plugin.DoAction(null, s, 23, "<br />", null, null, s);
        }

        private void buttonTest_Click(object sender, EventArgs e)
        {
            if (radioButtonWithFile.Checked)
            {
                using (var op = new OpenFileDialog() { Filter = "Subrip|*.srt" })
                {
                    if (op.ShowDialog() == DialogResult.OK)
                    {
                        string rawTExt = System.IO.File.ReadAllText(op.FileName);
                        textBoxOutput.Text = Tester(rawTExt);
                    }
                }
            }
            else
            {
                textBoxOutput.Text = Tester(textBox1.Text);
            }
        }
    }
}