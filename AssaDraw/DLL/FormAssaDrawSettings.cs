using System;
using System.Drawing;
using System.Windows.Forms;

namespace AssaDraw
{
    public partial class FormAssaDrawSettings : Form
    {
        public FormAssaDrawSettings(Color lineColor, Color lineColorActive)
        {
            InitializeComponent();

            panelLineColor.BackColor = lineColor;
            panelLineActiveColor.BackColor = lineColorActive;
        }

        public Color LineColor { get; set; }
        public Color LineColorActive { get; set; }

        private void buttonLineColor_Click(object sender, EventArgs e)
        {
            using (var colorDialog = new ColorDialog())
            {
                colorDialog.Color = panelLineColor.BackColor;
                if (colorDialog.ShowDialog() == DialogResult.OK)
                {
                    panelLineColor.BackColor = colorDialog.Color;
                }
            }
        }

        private void buttonNewLineColor_Click(object sender, EventArgs e)
        {
            using (var colorDialog = new ColorDialog())
            {
                colorDialog.Color = panelLineActiveColor.BackColor;
                if (colorDialog.ShowDialog() == DialogResult.OK)
                {
                    panelLineActiveColor.BackColor = colorDialog.Color;
                }
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            LineColor = panelLineColor.BackColor;
            LineColorActive = panelLineActiveColor.BackColor;
            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void FormAssaDrawSettings_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void panelLineColor_MouseClick(object sender, MouseEventArgs e)
        {
            buttonLineColor_Click(null, null);
        }

        private void panelLineActiveColor_Click(object sender, EventArgs e)
        {
            buttonNewLineColor_Click(null, null);
        }
    }
}
