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

        private static void Tester(string s)
        {
            // IPlugin plugin = new Nikse.SubtitleEdit.PluginLogic.Haxor();
            IPlugin plugin = new Nikse.SubtitleEdit.PluginLogic.Haxor();
            //MessageBox.Show(plugin.DoAction(null, s, 25, "<br />", null, @"C:\Users\Nikse\Desktop\Trailers\Game Of Thrones Season Trailer.mp4", s));
            plugin.DoAction(null, s, 23, "<br />", null, null, s);
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
                        Tester(rawTExt);
                    }
                }
            }
            else
            {
                string str = textBox1.Text.Trim();
                if (str.Length > 0)
                {
                    string p = @"
1
00:00:26,763 --> 00:00:29,381";
                    p += Environment.NewLine + str;
                    Tester(p);
                }
            }
        }
    }
}