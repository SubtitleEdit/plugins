using System;
using System.Diagnostics;
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
            //var apiKeyBuffer = Encoding.Default.GetBytes(apiKey);
            //string base64ApiKey = Convert.ToBase64String(apiKeyBuffer);

            //SettingUtils.UpdateApiKey(base64ApiKey);

            // TODO: Hash
            Configs.Settings.ApiKey = apiKey;

            MessageBox.Show("API key stored successfully!", "Api stored!", MessageBoxButtons.OK, MessageBoxIcon.Information);

            DialogResult = DialogResult.OK;
        }

        private void LinkLabelSignUP_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(linkLabelSignUP.Tag as string);
        }
    }
}
