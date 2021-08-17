using AssaDraw.Logic;
using System;
using System.Windows.Forms;

namespace AssaDraw
{
    public partial class FormAssaDrawSettings : Form
    {
        public FormAssaDrawSettings()
        {
            InitializeComponent();

            panelLineColor.BackColor = DrawSettings.ShapeLineColor;
            panelLineActiveColor.BackColor = DrawSettings.ActiveShapeLineColor;
            panelBackgroundColor.BackColor = DrawSettings.BackgroundColor;
            panelOffScreenColor.BackColor = DrawSettings.OffScreenColor;
        }

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
            DrawSettings.ShapeLineColor = panelLineColor.BackColor;
            DrawSettings.ActiveShapeLineColor = panelLineActiveColor.BackColor;
            DrawSettings.BackgroundColor = panelBackgroundColor.BackColor;
            DrawSettings.OffScreenColor = panelOffScreenColor.BackColor;
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

        private void buttonBackgroundColor_Click(object sender, EventArgs e)
        {
            using (var colorDialog = new ColorDialog())
            {
                colorDialog.Color = panelBackgroundColor.BackColor;
                if (colorDialog.ShowDialog() == DialogResult.OK)
                {
                    panelBackgroundColor.BackColor = colorDialog.Color;
                }
            }
        }

        private void buttonOffScreenColor_Click(object sender, EventArgs e)
        {
            using (var colorDialog = new ColorDialog())
            {
                colorDialog.Color = panelOffScreenColor.BackColor;
                if (colorDialog.ShowDialog() == DialogResult.OK)
                {
                    panelOffScreenColor.BackColor = colorDialog.Color;
                }
            }
        }

        private void panelBackgroundColor_Click(object sender, EventArgs e)
        {
            buttonBackgroundColor_Click(null, null);
        }

        private void panelOffScreenColor_Click(object sender, EventArgs e)
        {
            buttonOffScreenColor_Click(null, null);
        }
    }
}
