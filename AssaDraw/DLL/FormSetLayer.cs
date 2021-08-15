using System;
using System.Windows.Forms;

namespace AssaDraw
{
    public partial class FormSetLayer : Form
    {
        public int Layer { get; private set; }

        public FormSetLayer(int value)
        {
            InitializeComponent();

            numericUpDownLayer.Value = value;
        }

        private void SetLayer_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            Layer = (int)numericUpDownLayer.Value;
            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}
