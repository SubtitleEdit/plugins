using Nikse.SubtitleEdit.Logic;
using OpenSubtitles;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
            if (path.StartsWith("file:\\", StringComparison.Ordinal))
                path = path.Remove(0, 6);
            path = Path.Combine(path, "Plugins");
            if (!Directory.Exists(path))
                path = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Subtitle Edit"), "Plugins");
            return Path.Combine(path, "OpenSubtitles.xml");
        }

        private static string EncodeTo64(string toEncode)
        {
            byte[] toEncodeAsBytes = Encoding.Unicode.GetBytes(toEncode);
            return Convert.ToBase64String(toEncodeAsBytes);
        }

        public static string DecodeFrom64(string encodedData)
        {
            byte[] encodedDataAsBytes = Convert.FromBase64String(encodedData);
            return Encoding.Unicode.GetString(encodedDataAsBytes);
        }

        private void LoadLogin()
        {
            string fileName = GetSettingsFileName();
            try
            {
                var doc = new XmlDocument();
                doc.Load(fileName);
                textBoxUserName.Text = DecodeFrom64(doc.DocumentElement.SelectSingleNode("Username").InnerText);
                textBoxPassword.Text = DecodeFrom64(doc.DocumentElement.SelectSingleNode("Password").InnerText);
            }
            catch { }
        }

        private void SaveLogin(string userName, string password)
        {
            var fileName = GetSettingsFileName();
            try
            {
                var doc = new XmlDocument();
                doc.LoadXml("<OpenSubtitles><Username/><Password/></OpenSubtitles>");
                doc.DocumentElement.SelectSingleNode("Username").InnerText = EncodeTo64(userName);
                doc.DocumentElement.SelectSingleNode("Password").InnerText = EncodeTo64(password);
                doc.Save(fileName);
            }
            catch { }
        }

        public string GetLanguageCode()
        {
            int idx = comboBoxLanguage.SelectedIndex;
            if (idx >= 0)
            {
                var li = (LanguageItem)comboBoxLanguage.Items[idx];
                return li.CI.ThreeLetterISOLanguageName;
            }
            return "eng";
        }

        public PluginForm(string subtitleFileName, string rawText, string videoFileName, string name, string description)
        {
            InitializeComponent();
            Text = name;
            labelStatus.Text = string.Empty;
            _subtitleFileName = subtitleFileName;
            _rawText = rawText;

            if (videoFileName != null && File.Exists(videoFileName))
            {
                var videoFileSafeName = Path.GetFileName(videoFileName);
                textBoxMovieFileName.Text = videoFileSafeName;
                _videoFileName = videoFileName;
            }

            // 720p | 1080p
            if (Regex.IsMatch(_subtitleFileName, @"\d{3,4}p\b"))
                checkBoxHD.Checked = true;

            comboBoxFrameRate.SelectedIndex = 0;
            textBoxSubtitleFileName.Text = Path.GetFileName(subtitleFileName);
            textBoxReleaseName.Text = Path.GetFileNameWithoutExtension(_subtitleFileName);

            var twoLetterLanguageId = Utilities.AutoDetectGoogleLanguage(rawText);
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
            {
                checkBoxTextForHI.Checked = true;
            }
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

            if (string.IsNullOrWhiteSpace(textBoxUserName.Text))
            {
                MessageBox.Show("Please enter username");
                return false;
            }
            if (string.IsNullOrWhiteSpace(textBoxPassword.Text))
            {
                MessageBox.Show("Please enter password");
                return false;
            }

            try
            {
                Cursor = Cursors.WaitCursor;
                try
                {
                    // TODO: ENCRYPT USER PASSWORD
                    SaveLogin(textBoxUserName.Text, textBoxPassword.Text);
                }
                catch { }

                labelStatus.Text = "Logging in...";
                Refresh();
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
                    Refresh();
                    string fps = null;
                    if (comboBoxFrameRate.SelectedIndex > 0)
                        fps = comboBoxFrameRate.SelectedItem.ToString();
                    var res = _api.TryUploadSubtitles(_rawText, textBoxSubtitleFileName.Text, textBoxMovieFileName.Text, _videoFileName, GetLanguageCode(), fps, GetCurrentEncoding());
                    if (res)
                    {
                        labelStatus.Text = "Uploading subtitle...";
                        Refresh();

                        var textForHi = (checkBoxTextForHI.Checked) ? "1" : string.Empty;
                        var hd = (checkBoxHD.Checked) ? "1" : string.Empty;

                        if (_api.UploadSubtitles(_rawText, textBoxSubtitleFileName.Text, textBoxMovieFileName.Text, _videoFileName, GetLanguageCode(), textBoxReleaseName.Text, textBoxImdbId.Text, textBoxComment.Text, textForHi, hd, fps, GetCurrentEncoding()))
                        {
                            Cursor = Cursors.Default;
                            labelStatus.Text = "Subtitle uploaded :)";
                            Refresh();
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

        public static ICollection<string> GetMovieFileExtensions()
        {
            return new List<string> { ".avi", ".mkv", ".wmv", ".mpg", ".mpeg", ".divx", ".mp4", ".asf", ".flv", ".mov", ".m4v", ".vob", ".ogv", ".webm", ".ts", ".m2ts" };
        }

        public static string GetVideoFileFilter()
        {
            var sb = new StringBuilder();
            sb.Append("Video files|");
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
            if (string.IsNullOrEmpty(openFileDialogVideo.Filter))
                openFileDialogVideo.Filter = GetVideoFileFilter();
            if (openFileDialogVideo.ShowDialog() == DialogResult.OK)
            {
                textBoxMovieFileName.Text = Path.GetFileName(openFileDialogVideo.FileName);
                _videoFileName = openFileDialogVideo.FileName;
                labelStatus.Text = "Calculating frame-rate...";
                Task.Factory.StartNew(SetFrameRateFromVideoFile);

                buttonOpenVideo.Enabled = false;
                textBoxMovieFileName.Enabled = true;
                textBoxMovieFileName.ReadOnly = true;
                //Task.Factory.StartNew(SetFrameRateFromVideoFile).ContinueWith((_) => comboBoxFrameRate.SelectedIndex = idx, TaskScheduler.FromCurrentSynchronizationContext()); // where 'idx' is global volatile
            }
        }

        private void SetFrameRateFromVideoFile()
        {
            double frameRate = 0;
            Cursor = Cursors.WaitCursor;
            try
            {
                if (!string.IsNullOrEmpty(_videoFileName) && File.Exists(_videoFileName))
                {
                    var ext = Path.GetExtension(_videoFileName).ToLowerInvariant();
                    if (ext == ".mkv")
                    {
                        bool isValid = false;
                        using (var mkv = new VideoFormats.Mkv())
                        {
                            bool hasConstantFrameRate = true;
                            int pixelWidth = 0;
                            int pixelHeight = 0;
                            double millisecsDuration = 0;
                            string videoCodec = string.Empty;
                            mkv.GetMatroskaInfo(_videoFileName, ref isValid, ref hasConstantFrameRate, ref frameRate, ref pixelWidth, ref pixelHeight, ref millisecsDuration, ref videoCodec);
                        }
                        if (!isValid)
                            return;
                    }
                    if (frameRate < 1 && (ext == ".mp4" || ext == ".mov" || ext == ".m4v"))
                    {
                        var mp4 = new VideoFormats.MP4(_videoFileName);
                        frameRate = mp4.FrameRate;
                    }
                    if (frameRate < 1)
                    {
                        using (var rp = new VideoFormats.RiffParser())
                        {
                            var dh = new VideoFormats.RiffDecodeHeader(rp);
                            rp.OpenFile(_videoFileName);
                            if (VideoFormats.RiffParser.ckidAVI == rp.FileType)
                            {
                                dh.ProcessMainAVI();
                                frameRate = dh.FrameRate;
                            }
                        }
                    }
                }
                //CheckForIllegalCrossThreadCalls = false;
                Invoke(new MethodInvoker(() =>
                {
                    double minDiff = 100;
                    var index = 0;
                    for (int i = 1; i < comboBoxFrameRate.Items.Count; i++)
                    {
                        var element = comboBoxFrameRate.Items[i];
                        double d;
                        if (double.TryParse(element.ToString(), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out d))
                        {
                            double diff = Math.Abs(d - frameRate);
                            if (diff < 0.01 && diff < minDiff)
                            {
                                index = i;
                                minDiff = diff;
                            }
                        }
                    }
                    comboBoxFrameRate.SelectedIndex = index;
                }));
            }
            catch
            {
            }
            Cursor = Cursors.Default;
            labelStatus.Text = string.Empty;
        }

        private void buttonSearchIMDb_Click(object sender, EventArgs e)
        {
            if (CheckLogin())
            {
                try
                {
                    var title = string.Empty;
                    if (!string.IsNullOrEmpty(_videoFileName))
                    {
                        title = Path.GetFileNameWithoutExtension(_videoFileName).Replace('.', ' ');
                        title = ConvertToTitle(title);

                    }
                    using (var form = new ImdbSearch(title, _api))
                    {
                        if (form.ShowDialog(this) == DialogResult.OK)
                        {
                            textBoxImdbId.Text = form.ImdbId;
                        }
                    }
                }
                catch (Exception exception)
                {
                    labelStatus.Text = string.Empty;
                    MessageBox.Show(exception.Message);
                }
            }
        }

        private string ConvertToTitle(string release)
        {
            // Minority Report 2002
            //Better.Living.Through.Chemistry.2014.1080p.BluRay.x264.AAC.Ozlem
            // Veep - 03x05 - Fishing Subtitle
            release = release.Replace('[', ' ');
            release = release.Replace(']', ' ');
            release = release.Replace('_', ' ');
            release = release.Replace(" - ", " ");
            release = release.Replace('-', ' ');
            while (release.Contains("  "))
                release = release.Replace("  ", " ");
            release = release.Replace(":", string.Empty);
            release = release.Replace('(', ' ');
            release = release.Replace(')', ' ');

            var regexMovie = new Regex("(.+\\d{4}\\b)");
            var regexTvShow1 = new Regex("(.+[Ss]\\d{2}[eE]\\d{2})");
            var regexTvShow2 = new Regex("(.+\\d{2}[xX]\\d{2})");

            // Californication.S07E01.HDTV.x264-EXCELLENCE
            // Zulu.2013.1080p.BluRay.x264-Friday11th [PublicHD]
            // Zulu (2013) 720p BrRip x264 - YIFY
            // Tv show
            if (regexTvShow1.IsMatch(release)) // Californication.S07E01.HDTV.x264-EXCELLENCE
            {
                release = Regex.Match(release, "(.+?)[Ss]\\d{2}[eE]\\d{2}", RegexOptions.Compiled).Groups[1].Value;
            }
            else if (Regex.IsMatch(release, "\\d{2}[xX]\\d{2}"))
            {
                release = Regex.Match(release, "(.+?)\\d{2}[xX]\\d{2}", RegexOptions.Compiled).Groups[1].Value;
            }
            else if (Regex.IsMatch(release, "(.+?)\\d{4}\\b")) // Zulu.2013.1080p.BluRay.x264-Friday11th [PublicHD]
            {
                release = Regex.Match(release, "(.+\\d{4}\\b)", RegexOptions.Compiled).Groups[1].Value;
            }
            release = release.Trim().Replace("  ", " ");
            return release;
        }

        private void linkLabelRegister_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.opensubtitles.org/en/newuser");
        }

        private void PluginForm_Load(object sender, EventArgs e)
        {
            SetFrameRateFromVideoFile();
        }

    }
}