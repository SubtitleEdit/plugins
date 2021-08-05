using System.Windows.Forms;

namespace AssaDraw
{
    public partial class FormAssaDrawHelp : Form
    {
        public FormAssaDrawHelp()
        {
            InitializeComponent();
        }

        private void FormAssaDrawHelp_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void buttonOK_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }
    }
}
