using Nikse.SubtitleEdit.Logic;
using OpenSubtitles;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace OpenSubtitlesUpload
{
    public partial class PluginForm : Form
    {
        private string _subtitleFileName;
        private string _videoFileName;
        private string _rawText;
        private OpenSubtitlesApi _api;

        public class LanguageItem
        {
            public CultureInfo CI { private set; get; }

            public LanguageItem(CultureInfo ci)
            {
                CI = ci;
            }

            public override string ToString()
            {
                return CI.EnglishName;
            }
        }

        private string GetSettingsFileName()
        {
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);
            if (path.StartsWith("file:\\"))
                path = path.Remove(0, 6);
            path = Path.Combine(path, "Plugins");
            if (!Directory.Exists(path))
                path = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Subtitle Edit"), "Plugins");
            return Path.Combine(path, "OpenSubtitles.xml");
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

        private void LoadLogin()
        {
            string fileName = GetSettingsFileName();
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(fileName);
                textBoxUserName.Text = DecodeFrom64(doc.DocumentElement.SelectSingleNode("Username").InnerText);
                textBoxPassword.Text = DecodeFrom64(doc.DocumentElement.SelectSingleNode("Password").InnerText);
            }
            catch { }
        }

        private void SaveLogin(string userName, string password)
        {
            string fileName = GetSettingsFileName();
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml("<OpenSubtitles><Username/><Password/></OpenSubtitles>");
                doc.DocumentElement.SelectSingleNode("Username").InnerText = EncodeTo64(userName);
                doc.DocumentElement.SelectSingleNode("Password").InnerText = EncodeTo64(password);
                doc.Save(fileName);
            } catch { }
        }

        public string GetLanguageCode()
        {
            int idx = comboBoxLanguage.SelectedIndex;
            if (idx >= 0)
            {
                LanguageItem li = (LanguageItem)comboBoxLanguage.Items[idx];
                return li.CI.ThreeLetterISOLanguageName;
            }
            return "eng";
        }

        public PluginForm(string subtitleFileName, string rawText, string videoFileName, string name, string description)
        {
            InitializeComponent();
            this.Text = name;
            labelStatus.Text = string.Empty;
            _subtitleFileName = subtitleFileName;
            _videoFileName = videoFileName;
            _rawText = rawText;

            textBoxSubtitleFileName.Text = System.IO.Path.GetFileName(subtitleFileName);
            textBoxMovieFileName.Text = System.IO.Path.GetFileName(videoFileName);
            textBoxReleaseName.Text = string.Empty;

            string twoLetterLanguageId = Utilities.AutoDetectGoogleLanguage(rawText);
            foreach (CultureInfo x in CultureInfo.GetCultures(CultureTypes.NeutralCultures))
            {
                comboBoxLanguage.Items.Add(new LanguageItem(x));
                if (x.Name.ToLower() == twoLetterLanguageId.ToLower())
                    comboBoxLanguage.SelectedIndex = comboBoxLanguage.Items.Count - 1;
            }
            textBoxUserName.Text = string.Empty;
            textBoxPassword.Text = string.Empty;
            LoadLogin();
            _api = new OpenSubtitlesApi("SubtitleEdit");

            string temp = rawText.Replace("[", string.Empty).Replace("]", string.Empty).Replace("(", string.Empty).Replace(")", string.Empty).Replace(": ", string.Empty);
            if (temp.Length + 18 < rawText.Length)
                checkBoxTextForHI.Checked = true;

            comboBoxEncoding.Items.Clear();
            int encodingSelectedIndex = 0;
            comboBoxEncoding.Items.Add(Encoding.UTF8.EncodingName);
            foreach (EncodingInfo ei in Encoding.GetEncodings())
            {
                if (ei.Name != Encoding.UTF8.BodyName && ei.CodePage >= 949 && !ei.DisplayName.Contains("EBCDIC") && ei.CodePage != 1047)
                {
                    comboBoxEncoding.Items.Add(ei.CodePage + ": " + ei.DisplayName);
                    //if (ei.Name == Configuration.Settings.General.DefaultEncoding)
                    //    encodingSelectedIndex = comboBoxEncoding.Items.Count - 1;
                }
            }
            comboBoxEncoding.SelectedIndex = encodingSelectedIndex;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private bool CheckLogin()
        {
            if (_api != null && !string.IsNullOrEmpty(_api.Token))
                return true;

            if (textBoxUserName.Text.Trim().Length == 0)
            {
                MessageBox.Show("Please enter username");
                return false;
            }
            if (textBoxPassword.Text.Trim().Length == 0)
            {
                MessageBox.Show("Please enter password");
                return false;
            }

            try
            {
                Cursor = Cursors.WaitCursor;
                try
                {
                    // TODO: HASH TO USER PASSWORD
                    SaveLogin(textBoxUserName.Text, textBoxPassword.Text);
                } catch { }

                labelStatus.Text = "Logging in...";
                this.Refresh();
                if (_api.Login(textBoxUserName.Text, textBoxPassword.Text, GetLanguageCode()))
                {
                    Cursor = Cursors.Default;
                    labelStatus.Text = string.Empty;
                    return true;
                }
                else
                {
                    Cursor = Cursors.Default;
                    MessageBox.Show("Login failed");
                }
                labelStatus.Text = string.Empty;
                Cursor = Cursors.Default;
            }
            catch (Exception exception)
            {
                labelStatus.Text = string.Empty;
                Cursor = Cursors.Default;
                MessageBox.Show(exception.Message);
            }
            Cursor = Cursors.Default;
            labelStatus.Text = string.Empty;
            return false;
        }

        private Encoding GetCurrentEncoding()
        {
            if (comboBoxEncoding.Text == Encoding.UTF8.BodyName || comboBoxEncoding.Text == Encoding.UTF8.EncodingName || comboBoxEncoding.Text == "utf-8")
            {
                return Encoding.UTF8;
            }

            foreach (EncodingInfo ei in Encoding.GetEncodings())
            {
                if (ei.CodePage + ": " + ei.DisplayName == comboBoxEncoding.Text)
                    return ei.GetEncoding();
            }

            return Encoding.UTF8;
        }

        private void UploadClick(object sender, EventArgs e)
        {
            if (textBoxUserName.Text.Trim().Length == 0)
            {
                MessageBox.Show("Please enter a user name");
                textBoxUserName.Focus();
                return;
            }

            if (textBoxPassword.Text.Trim().Length == 0)
            {
                MessageBox.Show("Please enter a password");
                textBoxPassword.Focus();
                return;
            }

            if (textBoxImdbId.Text.Trim().Length == 0)
            {
                MessageBox.Show("Please enter IMDB id");
                textBoxImdbId.Focus();
                return;
            }

            if (CheckLogin())
            {
                try
                {
                    Cursor = Cursors.WaitCursor;
                    labelStatus.Text = "Checking db...";
                    this.Refresh();
                    var res = _api.TryUploadSubtitles(_rawText, textBoxSubtitleFileName.Text, textBoxMovieFileName.Text, _videoFileName, GetLanguageCode(), GetCurrentEncoding());
                    if (res)
                    {
                        labelStatus.Text = "Uploading subtitle...";
                        this.Refresh();
                        string textForHi = string.Empty;
                        if (checkBoxTextForHI.Checked)
                            textForHi = "1";
                        string hd = string.Empty;
                        if (checkBoxHD.Checked)
                            hd = "1";

                        if (_api.UploadSubtitles(_rawText, textBoxSubtitleFileName.Text, textBoxMovieFileName.Text, _videoFileName, GetLanguageCode(), textBoxReleaseName.Text, textBoxImdbId.Text, textBoxComment.Text, textForHi, hd, GetCurrentEncoding()))
                        {
                            Cursor = Cursors.Default;
                            labelStatus.Text = "Subtitle uploaded :)";
                            this.Refresh();
                            MessageBox.Show("Subtitle uploaded");
                            return;
                        }
                        else
                        {
                            Cursor = Cursors.Default;
                            MessageBox.Show(_api.LastStatus);
                        }
                        labelStatus.Text = string.Empty;
                    }
                    else
                    {
                        labelStatus.Text = string.Empty;
                        Cursor = Cursors.Default;
                        if (_api.LastStatus == "200 OK")
                            MessageBox.Show("Already in db!");
                        else
                            MessageBox.Show(_api.LastStatus);
                    }
                }
                catch (Exception exception)
                {
                    labelStatus.Text = string.Empty;
                    Cursor = Cursors.Default;
                    MessageBox.Show(exception.Message);
                }
            }
            Cursor = Cursors.Default;
        }

        private void linkLabelSearchImdb_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.opensubtitles.org/en/imdblook/short-on");
        }

        private void linkLabelUploadManually_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string path = "http://www.opensubtitles.org/en/upload";
            if (textBoxImdbId.Text.Trim().Length > 0)
                path += "/idmovieimdb-" + textBoxImdbId.Text.Trim();
            path += "/sublanguageid-" + GetLanguageCode();
            System.Diagnostics.Process.Start(path);
        }

        private void PluginForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.Cancel;
        }

        public static IEnumerable<string> GetMovieFileExtensions()
        {
            return new List<string> { ".avi", ".mkv", ".wmv", ".mpg", ".mpeg", ".divx", ".mp4", ".asf", ".flv", ".mov", ".m4v", ".vob", ".ogv", ".webm", ".ts", ".m2ts" };
        }

        public static string GetVideoFileFilter()
        {
            var sb = new StringBuilder();
            sb.Append("video files|");
            int i = 0;
            foreach (string extension in GetMovieFileExtensions())
            {
                if (i > 0)
                    sb.Append(";");
                sb.Append("*" + extension);
                i++;
            }
            sb.Append("|All files|*.*");
            return sb.ToString();
        }

        private void buttonOpenVideo_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(_subtitleFileName))
                openFileDialogVideo.InitialDirectory = Path.GetDirectoryName(_subtitleFileName);
            openFileDialogVideo.Title = "Open video file...";
            openFileDialogVideo.FileName = string.Empty;
            openFileDialogVideo.Filter = GetVideoFileFilter();
            if (openFileDialogVideo.ShowDialog() == DialogResult.OK)
            {
                textBoxMovieFileName.Text = Path.GetFileName(openFileDialogVideo.FileName);
                _videoFileName = openFileDialogVideo.FileName;
            }
        }

        private void buttonSearchIMDb_Click(object sender, EventArgs e)
        {
            if (CheckLogin())
            {
                try
                {
                    string s = string.Empty;
                    if (!string.IsNullOrEmpty(_videoFileName))
                        s = System.IO.Path.GetFileNameWithoutExtension(_videoFileName).Replace(".", " ");
                    var form = new ImdbSearch(s, _api);
                    if (form.ShowDialog(this) == DialogResult.OK)
                    {
                        textBoxImdbId.Text = form.ImdbId;
                    }
                }
                catch (Exception exception)
                {
                    labelStatus.Text = string.Empty;
                    MessageBox.Show(exception.Message);
                }
            }
        }

        private void linkLabelRegister_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.opensubtitles.org/en/newuser");
        }
    }
}