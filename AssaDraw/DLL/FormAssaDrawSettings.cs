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
            panelScreenRes.BackColor = DrawSettings.ScreenSizeColor;

            checkBoxHideSettingsAndTreeView.Checked = DrawSettings.HideSettingsAndTreeView;
            checkBoxAutoLoadBackgroundFromSE.Visible = !DrawSettings.Standalone;
            checkBoxAutoLoadBackgroundFromSE.Checked = DrawSettings.UseScreenShotFromSe;
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
            DrawSettings.ScreenSizeColor = panelScreenRes.BackColor;
            DrawSettings.HideSettingsAndTreeView = checkBoxHideSettingsAndTreeView.Checked;
            DrawSettings.UseScreenShotFromSe = checkBoxAutoLoadBackgroundFromSE.Checked;
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

        private void panelBackgroundColor_Click(object sender, EventArgs e)
        {
            buttonBackgroundColor_Click(null, null);
        }

        private void buttonScreenRes_Click(object sender, EventArgs e)
        {
            using (var colorDialog = new ColorDialog())
            {
                colorDialog.Color = panelScreenRes.BackColor;
                if (colorDialog.ShowDialog() == DialogResult.OK)
                {
                    panelScreenRes.BackColor = colorDialog.Color;
                }
            }
        }
    }
}
