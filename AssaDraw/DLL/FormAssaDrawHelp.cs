using System.Globalization;
using System.Windows.Forms;

namespace AssaDraw
{
    public partial class FormAssaDrawHelp : Form
    {
        public FormAssaDrawHelp()
        {
            InitializeComponent();

            var version = (new Nikse.SubtitleEdit.PluginLogic.AssaDraw() as Nikse.SubtitleEdit.PluginLogic.IPlugin).Version.ToString(CultureInfo.InvariantCulture);
            richTextBoxHelpInfo.Text = richTextBoxHelpInfo.Text.Replace("[VERSION]", version);
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
