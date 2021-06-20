using System;
using System.Windows.Forms;
using SubtitleEdit.Logic;

namespace SubtitleEdit
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SubtitleFormat assa = new AdvancedSubStationAlpha();
            var sub = new Subtitle();
            assa.LoadSubtitle(sub, textBox1.Text.SplitToLines(), null);
            using (var form = new MainForm(sub, "ASSA fade", "ASSA fade description", this))
            {
                form.ShowDialog(this);
            }
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            button1.Focus();
        }
    }
}
