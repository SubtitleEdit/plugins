using System;
using System.Text;
using System.Windows.Forms;

namespace OnlineCasing.Forms
{
    public partial class ApiForm : Form
    {
        public ApiForm()
        {
            InitializeComponent();
        }

        private void ButtonSet_Click(object sender, EventArgs e)
        {
            string apiKey = textBoxApiKey.Text.Trim();

            // select the setting file
            var apiKeyBuffer = Encoding.Default.GetBytes(apiKey);
            string base64ApiKey = Convert.ToBase64String(apiKeyBuffer);

            SettingUtils.UpdateApiKey(base64ApiKey);

            MessageBox.Show("API key stored successfully!", "Api stored!", MessageBoxButtons.OK, MessageBoxIcon.Information);

            DialogResult = DialogResult.OK;
        }
    }
}
