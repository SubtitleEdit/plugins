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
            public LanguageItem(string threeLettercode, string englishName)
            {
                ThreeLetterCode = threeLettercode;
                EnglishName = englishName;
            }

            public string EnglishName { get; set; }
            public string ThreeLetterCode { get; set; }

            public override string ToString()
            {
                return EnglishName;
            }
        }

        private void LoadLogin()
        {
            string fileName = Utils.GetSettingsFileName();
            try
            {
                var doc = new XmlDocument();
                doc.Load(fileName);
                textBoxUserName.Text = Utils.DecodeFrom64(doc.DocumentElement.SelectSingleNode("Username").InnerText);
                textBoxPassword.Text = Utils.DecodeFrom64(doc.DocumentElement.SelectSingleNode("Password").InnerText);
            }
            catch { }
        }

        private void SaveLogin(string userName, string password)
        {
            var fileName = Utils.GetSettingsFileName();
            try
            {
                var doc = new XmlDocument();
                doc.LoadXml("<OpenSubtitles><Username/><Password/></OpenSubtitles>");
                doc.DocumentElement.SelectSingleNode("Username").InnerText = Utils.EncodeTo64(userName);
                doc.DocumentElement.SelectSingleNode("Password").InnerText = Utils.EncodeTo64(password);
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
                return li.ThreeLetterCode;
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

            FillLanguages(rawText);

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

        private static List<LanguageItem> OSLanguages = new List<LanguageItem>
        {
            new LanguageItem("eng", "English"),
            new LanguageItem("alb", "Albanian"),
            new LanguageItem("afr", "Afrikaans"),
            new LanguageItem("ara", "Arabic"),
            new LanguageItem("arm", "Armenian"),
            new LanguageItem("baq", "Basque"),
            new LanguageItem("bel", "Belarusian"),
            new LanguageItem("ben", "Bengali"),
            new LanguageItem("bos", "Bosnian"),
            new LanguageItem("bre", "Breton"),
            new LanguageItem("bul", "Bulgarian"),
            new LanguageItem("bur", "Burmese"),
            new LanguageItem("cat", "Catalan"),
            new LanguageItem("chi", "Chinese (simplified)"),
            new LanguageItem("zht", "Chinese (traditional)"),
            new LanguageItem("zhe", "Chinese bilingual"),
            new LanguageItem("hrv", "Croatian"),
            new LanguageItem("cze", "Czech"),
            new LanguageItem("dan", "Danish"),
            new LanguageItem("dut", "Dutch"),
            new LanguageItem("epo", "Esperanto"),
            new LanguageItem("est", "Estonian"),
            new LanguageItem("fin", "Finnish"),
            new LanguageItem("fre", "French"),
            new LanguageItem("glg", "Galician"),
            new LanguageItem("geo", "Georgian"),
            new LanguageItem("ger", "German"),
            new LanguageItem("ell", "Greek"),
            new LanguageItem("heb", "Hebrew"),
            new LanguageItem("hin", "Hindi"),
            new LanguageItem("hun", "Hungarian"),
            new LanguageItem("ice", "Icelandic"),
            new LanguageItem("ind", "Indonesian"),
            new LanguageItem("ita", "Italian"),
            new LanguageItem("jpn", "Japanese"),
            new LanguageItem("kaz", "Kazakh"),
            new LanguageItem("khm", "Khmer"),
            new LanguageItem("kor", "Korean"),
            new LanguageItem("lav", "Latvian"),
            new LanguageItem("lit", "Lithuanian"),
            new LanguageItem("ltz", "Luxembourgish"),
            new LanguageItem("mac", "Macedonian"),
            new LanguageItem("may", "Malay"),
            new LanguageItem("mal", "Malayalam"),
            new LanguageItem("mni", "Manipuri"),
            new LanguageItem("mon", "Mongolian"),
            new LanguageItem("mne", "Montenegrin"),
            new LanguageItem("nor", "Norwegian"),
            new LanguageItem("oci", "Occitan"),
            new LanguageItem("per", "Persian"),
            new LanguageItem("pol", "Polish"),
            new LanguageItem("por", "Portuguese"),
            new LanguageItem("pob", "Portuguese (BR)"),
            new LanguageItem("rum", "Romanian"),
            new LanguageItem("rus", "Russian"),
            new LanguageItem("scc", "Serbian"),
            new LanguageItem("sin", "Sinhalese"),
            new LanguageItem("slo", "Slovak"),
            new LanguageItem("slv", "Slovenian"),
            new LanguageItem("spa", "Spanish"),
            new LanguageItem("swa", "Swahili"),
            new LanguageItem("swe", "Swedish"),
            new LanguageItem("syr", "Syriac"),
            new LanguageItem("tgl", "Tagalog"),
            new LanguageItem("tam", "Tamil"),
            new LanguageItem("tel", "Telugu"),
            new LanguageItem("tha", "Thai"),
            new LanguageItem("tur", "Turkish"),
            new LanguageItem("ukr", "Ukrainian"),
            new LanguageItem("urd", "Urdu"),
            new LanguageItem("vie", "Vietnamese"),
        };

        private void FillLanguages(string rawText)
        {
            var twoLetterLanguageId = Utils.AutoDetectGoogleLanguage(rawText);
            string threeLetterLanguageId = "eng";
            try
            {
                foreach (CultureInfo x in CultureInfo.GetCultures(CultureTypes.NeutralCultures))
                {
                    if (string.IsNullOrEmpty(x.Name)) // To skip culture like: Invariante Language
                        continue;
                    if (twoLetterLanguageId == x.TwoLetterISOLanguageName)
                    {
                        threeLetterLanguageId = x.ThreeLetterISOLanguageName;
                        break;
                    }
                }
            }
            catch
            {
                // ignore
            }


            foreach (var item in OSLanguages)
            {
                comboBoxLanguage.Items.Add(item);
                if (item.ThreeLetterCode == threeLetterLanguageId)
                {
                    comboBoxLanguage.SelectedIndex = comboBoxLanguage.Items.Count - 1;
                }
            }

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
                labelStatus.ForeColor = System.Drawing.Color.Yellow;
                labelStatus.Text = "Calculating frame-rate...";
                Cursor = Cursors.WaitCursor;
                Task.Factory.StartNew(SetFrameRateFromVideoFile);
                EnableDisableActionButtons(false);
                //Task.Factory.StartNew(SetFrameRateFromVideoFile).ContinueWith((_) => comboBoxFrameRate.SelectedIndex = idx, TaskScheduler.FromCurrentSynchronizationContext()); // where 'idx' is global volatile
            }
        }

        private void SetFrameRateFromVideoFile()
        {
            double frameRate = 0;
            // Cursor = Cursors.WaitCursor; will Invalid operation exection (Note: Belong to UI thread, the expection won't crash the app 'CAREFUL') 
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

            // Restore controls stats
            Invoke(new MethodInvoker(() =>
            {
                Cursor = Cursors.Default;
                labelStatus.Text = string.Empty;
                EnableDisableActionButtons(true);
                labelStatus.ForeColor = System.Drawing.Color.Black;
            }));
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
            //SetFrameRateFromVideoFile();
        }

        private void EnableDisableActionButtons(bool enableDisable)
        {
            buttonUpload.Enabled = enableDisable;
            buttonCancel.Enabled = enableDisable;
            buttonOpenVideo.Enabled = enableDisable;
            textBoxMovieFileName.Enabled = enableDisable;
            textBoxMovieFileName.ReadOnly = !enableDisable;
        }

    }
}