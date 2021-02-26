using System;
using System.Linq;
using System.Windows.Forms;
using ColorToDialog.Logic;

namespace ColorToDialog
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var fileName = @"J:\Data\Subtitles\ts\Peta\a.srt";
            var text = System.IO.File.ReadAllText(fileName);
            var sub = new Subtitle();
            var srt = new SubRip();
            srt.LoadSubtitle(sub, text.SplitToLines().ToList(), fileName);
            using (var form = new MainForm(sub, "test", "descr", this))
            {
                form.ShowDialog(this);
            }
        }
    }
}
