using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Web;
using System.Windows.Forms;
using System.Xml;
using Dropbox.Api;
using OAuthProtocol;

namespace Nikse.SubtitleEdit.PluginLogic
{
    internal partial class PluginForm : Form
    {
        private const string SeConsumerKey = "CLIENT_KEY";
        private const string SeConsumerSecret = "CLIENT_SECRET";

        private string _fileName;
        private string _rawText;
        private DropboxFile _fileList;
        private int _connectTries = 0;

        private static OAuthToken GetRequestToken()
        {
            var uri = new Uri("https://api.dropbox.com/1/oauth/request_token");

            // Generate a signature
            OAuthBase oAuth = new OAuthBase();
            string nonce = oAuth.GenerateNonce();
            string timeStamp = oAuth.GenerateTimeStamp();
            string parameters;
            string normalizedUrl;
            string signature = oAuth.GenerateSignature(uri, SeConsumerKey, SeConsumerSecret, String.Empty, String.Empty, "GET", timeStamp, nonce, OAuthBase.SignatureTypes.HMACSHA1,
                                                       out normalizedUrl, out parameters);

            signature = HttpUtility.UrlEncode(signature);

            StringBuilder requestUri = new StringBuilder(uri.ToString());
            requestUri.AppendFormat("?oauth_consumer_key={0}&", SeConsumerKey);
            requestUri.AppendFormat("oauth_nonce={0}&", nonce);
            requestUri.AppendFormat("oauth_timestamp={0}&", timeStamp);
            requestUri.AppendFormat("oauth_signature_method={0}&", "HMAC-SHA1");
            requestUri.AppendFormat("oauth_version={0}&", "1.0");
            requestUri.AppendFormat("oauth_signature={0}", signature);

            var request = (HttpWebRequest)WebRequest.Create(new Uri(requestUri.ToString()));
            request.Method = WebRequestMethods.Http.Get;

            var response = request.GetResponse();
            var queryString = new StreamReader(response.GetResponseStream()).ReadToEnd();
            var parts = queryString.Split('&');
            var token = parts[1].Substring(parts[1].IndexOf('=') + 1);
            var secret = parts[0].Substring(parts[0].IndexOf('=') + 1);

            return new OAuthToken(token, secret);
        }

        private static OAuthToken GetAccessToken(OAuthToken oauthToken)
        {
            var uri = "https://api.dropbox.com/1/oauth/access_token";

            OAuthBase oAuth = new OAuthBase();

            var nonce = oAuth.GenerateNonce();
            var timeStamp = oAuth.GenerateTimeStamp();
            string parameters;
            string normalizedUrl;
            var signature = oAuth.GenerateSignature(new Uri(uri), SeConsumerKey, SeConsumerSecret,
                oauthToken.Token, oauthToken.Secret, "GET", timeStamp, nonce,
                OAuthBase.SignatureTypes.HMACSHA1, out normalizedUrl, out parameters);

            signature = HttpUtility.UrlEncode(signature);

            var requestUri = new StringBuilder(uri);
            requestUri.AppendFormat("?oauth_consumer_key={0}&", SeConsumerKey);
            requestUri.AppendFormat("oauth_token={0}&", oauthToken.Token);
            requestUri.AppendFormat("oauth_nonce={0}&", nonce);
            requestUri.AppendFormat("oauth_timestamp={0}&", timeStamp);
            requestUri.AppendFormat("oauth_signature_method={0}&", "HMAC-SHA1");
            requestUri.AppendFormat("oauth_version={0}&", "1.0");
            requestUri.AppendFormat("oauth_signature={0}", signature);

            var request = (HttpWebRequest)WebRequest.Create(requestUri.ToString());
            request.Method = WebRequestMethods.Http.Get;

            var response = request.GetResponse();
            var reader = new StreamReader(response.GetResponseStream());
            var accessToken = reader.ReadToEnd();

            var parts = accessToken.Split('&');
            var token = parts[1].Substring(parts[1].IndexOf('=') + 1);
            var secret = parts[0].Substring(parts[0].IndexOf('=') + 1);

            return new OAuthToken(token, secret);
        }

        private string GetSettingsFileName()
        {
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);
            if (path.StartsWith("file:\\"))
                path = path.Remove(0, 6);
            path = Path.Combine(path, "Plugins");
            if (!Directory.Exists(path))
                path = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Subtitle Edit"), "Plugins");
            return Path.Combine(path, "SeDropbox.xml");
        }

        private static string EncodeTo64(string toEncode)
        {
            byte[] toEncodeAsBytes = System.Text.Encoding.Unicode.GetBytes(toEncode);
            return System.Convert.ToBase64String(toEncodeAsBytes);
        }

        public static string DecodeFrom64(string encodedData)
        {
            byte[] encodedDataAsBytes = System.Convert.FromBase64String(encodedData);
            return System.Text.Encoding.Unicode.GetString(encodedDataAsBytes);
        }

        private OAuthToken GetSavedToken()
        {
            string fileName = GetSettingsFileName();
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(fileName);
                string token = DecodeFrom64(doc.DocumentElement.SelectSingleNode("Token").InnerText);
                string secret = DecodeFrom64(doc.DocumentElement.SelectSingleNode("Secret").InnerText);
                return new OAuthToken(token, secret);
            }
            catch
            {
                return null;
            }
        }

        private void SaveToken(string token, string secret)
        {
            string fileName = GetSettingsFileName();
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml("<SeDropbox><Token/><Secret/></SeDropbox>");
                doc.DocumentElement.SelectSingleNode("Token").InnerText = EncodeTo64(token);
                doc.DocumentElement.SelectSingleNode("Secret").InnerText = EncodeTo64(secret);
                doc.Save(fileName);
            }
            catch
            {
            }
        }

        internal PluginForm(string fileName, string rawText, string name, string description)
        {
            InitializeComponent();
            this.Text = name;
            buttonOK.Enabled = false;
            labelDescription.Text = "Connecting to DropBox...";
            _rawText = rawText;
            if (string.IsNullOrEmpty(fileName))
                fileName = "Untitled.srt";
            _fileName = fileName;
            textBoxFileName.Text = fileName;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            labelInfo.Text = "Saving...";
            this.Refresh();
            OAuthToken accessToken = GetSavedToken();
            try
            {
                var api = new DropboxApi(SeConsumerKey, SeConsumerSecret, accessToken);
                var fileUp = api.UploadFile("sandbox", System.IO.Path.GetFileName(_fileName), Encoding.UTF8.GetBytes(_rawText));
                DialogResult = DialogResult.OK;
                labelInfo.Text = string.Empty;
                this.Refresh();
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
            this.Refresh();
            OAuthToken accessToken = GetSavedToken();
            if (accessToken == null)
            {
                try
                {
                    // Step 1/3: Get request token
                    OAuthToken oauthToken = GetRequestToken();

                    // Step 2/3: Authorize application
                    var queryString = String.Format("oauth_token={0}", oauthToken.Token);
                    var authorizeUrl = "https://www.dropbox.com/1/oauth/authorize?" + queryString;
                    Process.Start(authorizeUrl);
                    MessageBox.Show("Accept App request from Subtitle Edit - and press OK");
                    labelInfo.Text = string.Empty;
                    accessToken = GetAccessToken(oauthToken);
                    SaveToken(accessToken.Token, accessToken.Secret);
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
            this.Refresh();
            var api = new DropboxApi(SeConsumerKey, SeConsumerSecret, accessToken);
            try
            {
                Cursor = Cursors.WaitCursor;
                _fileList = api.GetFiles("sandbox", string.Empty);
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
                labelDescription.Text = string.Format("Save (and overwrite) subtitle '{0}' to your Dropbox SubtitleEdit App folder?", System.IO.Path.GetFileName(_fileName));
            else
                labelDescription.Text = string.Format("Save subtitle '{0}' to your Dropbox SubtitleEdit App folder?", System.IO.Path.GetFileName(_fileName));
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
