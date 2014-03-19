using Nikse.SubtitleEdit.PluginLogic.HI2UC;
using Nikse.SubtitleEdit.PluginLogic.LinesUnbreaker;
using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace TmdbCasingTester
{
    public partial class IvandroflyTester : Form
    {
        private string _file;
        private string _rawText;
        private string _path;
        private Nikse.SubtitleEdit.PluginLogic.HI2UC.IPlugin _hi;
        private Nikse.SubtitleEdit.PluginLogic.LinesUnbreaker.IPlugin _line;
        private Nikse.SubtitleEdit.PluginLogic.HI2UC.IPlugin _tmdb;

        public IvandroflyTester()
        {
            InitializeComponent();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var openfile = new OpenFileDialog() { Filter = "Subrip(*.srt)|*.srt" })
            {
                if (openfile.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    _file = openfile.FileName;
                    _path = Path.GetFileName(_file);
                    _rawText = File.ReadAllText(_file, Encoding.UTF8);
                }
            }
        }

        private void buttonRun_Click(object sender, EventArgs e)
        {
            //var type = Assembly.GetAssembly(int).GetType("ismael").GetInterface("IPlugin");

            if (radioButtonHI2UC.Checked)
            {
                _hi = new HI2UC();
                _hi.DoAction(null, _rawText, 23, "<br />", _path, null, _rawText);
            }
            else if (radioButtonLinesunbreaker.Checked)
            {
                _line = new LinesUnbreaker();
                _line.DoAction(null, _rawText, 23, "<br />", _path, null, _rawText);
            }
        }
    }
}