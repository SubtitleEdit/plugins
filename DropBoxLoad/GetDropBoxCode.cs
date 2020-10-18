using System;
using System.Windows.Forms;

namespace SeDropBoxLoad
{
    public partial class GetDropBoxCode : Form
    {
        public string Code { get; set; }

        public GetDropBoxCode()
        {
            InitializeComponent();
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            Code = textBoxCode.Text;
            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}
