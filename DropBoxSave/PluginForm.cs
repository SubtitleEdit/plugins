using Dropbox.Api;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace Nikse.SubtitleEdit.PluginLogic
{
    internal sealed partial class PluginForm : Form
    {
        private const string SeAppKey = "CLIENT_KEY";
        private const string SeAppsecret = "CLIENT_SECRET";
        private OAuth2Token _oAuth2Token;
        private string _fileName;
        private readonly string _rawText;
        private DropboxFile _fileList;
        private int _connectTries;

        private static string GetSettingsFileName()
        {
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);
            if (path != null && path.StartsWith("file:\\", StringComparison.Ordinal))
            {
                path = path.Remove(0, 6);
            }

            path = Path.Combine(path, "Plugins");
            if (!Directory.Exists(path))
            {
                path = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Subtitle Edit"), "Plugins");
            }

            return Path.Combine(path, "SeDropbox.xml");
        }

        private static string EncodeTo64(string toEncode)
        {
            var toEncodeAsBytes = Encoding.Unicode.GetBytes(toEncode);
            return Convert.ToBase64String(toEncodeAsBytes);
        }

        public static string DecodeFrom64(string encodedData)
        {
            var encodedDataAsBytes = Convert.FromBase64String(encodedData);
            return Encoding.Unicode.GetString(encodedDataAsBytes);
        }

        private OAuth2Token GetSavedToken()
        {
            string fileName = GetSettingsFileName();
            try
            {
                var doc = new XmlDocument();
                doc.Load(fileName);
                string accessToken = DecodeFrom64(doc.DocumentElement.SelectSingleNode("Accesstoken").InnerText);
                string uid = DecodeFrom64(doc.DocumentElement.SelectSingleNode("UId").InnerText);
                string accountId = DecodeFrom64(doc.DocumentElement.SelectSingleNode("AccountId").InnerText);
                string tokenType = DecodeFrom64(doc.DocumentElement.SelectSingleNode("TokenType").InnerText);
                return new OAuth2Token()
                {
                    access_token = accessToken,
                    uid = uid,
                    account_id = accountId,
                    token_type = tokenType
                };
            }
            catch
            {
                return null;
            }
        }

        private void SaveToken(OAuth2Token token)
        {
            string fileName = GetSettingsFileName();
            try
            {
                var doc = new XmlDocument();
                doc.LoadXml("<SeDropbox><Accesstoken/><UId/><AccountId/><TokenType/></SeDropbox>");
                doc.DocumentElement.SelectSingleNode("Accesstoken").InnerText = EncodeTo64(token.access_token);
                doc.DocumentElement.SelectSingleNode("UId").InnerText = EncodeTo64(token.uid);
                doc.DocumentElement.SelectSingleNode("AccountId").InnerText = EncodeTo64(token.account_id);
                doc.DocumentElement.SelectSingleNode("TokenType").InnerText = EncodeTo64(token.token_type);
                doc.Save(fileName);
            }
            catch
            {
            }
        }

        internal PluginForm(string fileName, string rawText, string name, string description)
        {
            InitializeComponent();
            Text = name;
            buttonOK.Enabled = false;
            labelDescription.Text = "Connecting to Dropbox...";
            _rawText = rawText;
            if (string.IsNullOrEmpty(fileName))
            {
                fileName = "Untitled.srt";
            }

            _fileName = fileName;
            textBoxFileName.Text = fileName;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            var s = textBoxFileName.Text;
            if (s.Length == 0)
            {
                return;
            }

            labelInfo.Text = "Saving...";
            Refresh();
            _oAuth2Token = GetSavedToken();
            try
            {
                var api = new DropboxApi(SeAppKey, SeAppsecret, _oAuth2Token);
                var fileUp = api.UploadFile(s, Encoding.UTF8.GetBytes(_rawText));
                DialogResult = DialogResult.OK;
                labelInfo.Text = string.Empty;
                Refresh();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void PluginForm_Shown(object sender, EventArgs e)
        {
            _oAuth2Token = GetSavedToken();
            var api = new DropboxApi(SeAppKey, SeAppsecret, _oAuth2Token);
            if (_oAuth2Token == null)
            {
                try
                {
                    Process.Start(api.GetPromptForCodeUrl());
                    using (var form = new GetDropBoxCode())
                    {
                        var result = form.ShowDialog(this);
                        if (result == DialogResult.OK && form.Code.Length > 10)
                        {
                            _oAuth2Token = api.GetAccessToken(form.Code);
                        }
                        else
                        {
                            MessageBox.Show("Code skipped - no Dropbox :(");
                            return;
                        }
                    }
                    labelInfo.Text = string.Empty;
                    SaveToken(_oAuth2Token);
                }
                catch (Exception exception)
                {
                    _connectTries++;
                    if (_connectTries < 2)
                    {
                        PluginForm_Shown(sender, e);
                        return;
                    }
                    MessageBox.Show(exception.Message);
                    buttonOK.Enabled = false;
                    return;
                }
            }
            labelInfo.Text = "Getting file list...";
            Refresh();
            try
            {
                Cursor = Cursors.WaitCursor;
                _fileList = api.GetFiles(string.Empty);
                Cursor = Cursors.Default;
            }
            catch (Exception exception)
            {
                Cursor = Cursors.Default;
                _connectTries++;
                PluginForm_Shown(sender, e);
                _connectTries++;
                if (_connectTries < 2)
                {
                    PluginForm_Shown(sender, e);
                    return;
                }
                MessageBox.Show(exception.Message);
                buttonOK.Enabled = false;
                return;
            }
            labelInfo.Text = string.Empty;
            MakeDescriptionPrompt();
        }

        private void MakeDescriptionPrompt()
        {
            bool overWrite = false;
            if (_fileList != null)
            {
                foreach (DropboxFile f in _fileList.Contents)
                {
                    if (f.Path.ToLower() == _fileName.ToLower())
                    {
                        overWrite = true;
                    }
                }
            }

            if (overWrite)
            {
                labelDescription.Text = string.Format("Save (and overwrite) subtitle '{0}' to your Dropbox SubtitleEdit App folder?", Path.GetFileName(_fileName));
            }
            else
            {
                labelDescription.Text = string.Format("Save subtitle '{0}' to your Dropbox SubtitleEdit App folder?", Path.GetFileName(_fileName));
            }

            buttonOK.Enabled = true;
        }

        private void textBoxFileName_TextChanged(object sender, EventArgs e)
        {
            _fileName = textBoxFileName.Text;
            buttonOK.Enabled = !string.IsNullOrEmpty(_fileName);
            MakeDescriptionPrompt();
        }

    }
}
