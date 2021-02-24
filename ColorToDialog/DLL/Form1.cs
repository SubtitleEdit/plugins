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
            var fileName = @"C:\Users\WinX\Desktop\auto-br\Peta\The Amazing Race Australia 5x08-TENRip-PG-raw-coloured.srt";
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
