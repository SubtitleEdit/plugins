using System.Globalization;
using Nikse.SubtitleEdit.PluginLogic;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SubtitleEdit
{
    public partial class MainForm : Form
    {
        private readonly Subtitle _subtitle;
        private static string _baiduParameterT = "1471889368713";
        private static string _baiduParameterToken = "b1067b2fc50ed1ef7813c4547f26f482";
        private static string _from = "en";
        private static string _to = "zh";
        private bool _abort;

        public class BaiduTranslationPair
        {
            public string From { get; set; }
            public string To { get; set; }
            public string ToCode { get; set; }
            public string FromCode { get; set; }

            public BaiduTranslationPair(string from, string fromCode, string to, string toCode)
            {
                From = from;
                FromCode = fromCode;
                To = to;
                ToCode = toCode;
            }

            public override string ToString()
            {
                return From + " -> " + To; // will be displayed in combobox
            }
        }

        public string FixedSubtitle { get; private set; }

        public MainForm()
        {
            InitializeComponent();
            KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Escape)
                    DialogResult = DialogResult.Cancel;
                else if (e.KeyCode == Keys.F1)
                    textBox1.Visible = !textBox1.Visible;
            };
            textBox1.Visible = false;
            listView1.Columns[2].Width = -2;
            buttonCancelTranslate.Enabled = false;
            comboBoxLanguagePair.Items.Clear();
            comboBoxLanguagePair.Items.AddRange(new object[]
                {
                    new BaiduTranslationPair("English", "en", "Chinese", "zh"),
                    new BaiduTranslationPair("Chinese", "zh", "English", "en"),

                    new BaiduTranslationPair("English", "en", "Thai", "th"),
                    new BaiduTranslationPair("Thai", "th", "English", "en"),

                    new BaiduTranslationPair("English", "en", "Japanese", "jp"),
                    new BaiduTranslationPair("Japanese", "jp", "English", "en"),

                    new BaiduTranslationPair("English", "en", "Portuguese", "pt"),
                    new BaiduTranslationPair("Portuguese", "pt", "English", "en"),

                    new BaiduTranslationPair("English", "en", "Spanish", "spa"),
                    new BaiduTranslationPair("Spanish", "spa", "English", "en"),

                    new BaiduTranslationPair("Chinese", "zh", "Japanese", "jp"),
                    new BaiduTranslationPair("Japanese", "jp", "Chinese", "zh"),
                });
            comboBoxLanguagePair.SelectedIndex = 0;

            var wc = new WebClient();
            wc.DownloadStringCompleted += (sender, args) =>
            {
                if (args != null && args.Error == null && !string.IsNullOrWhiteSpace(args.Result))
                {
                    var idx = args.Result.IndexOf("TOKEN=", StringComparison.Ordinal);
                    if (idx > 0)
                    {
                        var token = args.Result.Substring(idx + 7, 50).Trim().Trim('"');
                        idx = token.IndexOf('"');
                        if (idx > 0)
                        {
                            token = token.Substring(0, idx);
                            _baiduParameterToken = token;
                            textBox1.AppendText("Token: " + token + Environment.NewLine + Environment.NewLine);
                        }
                    }
                }
            };
            wc.DownloadStringAsync(new Uri("http://translate.baidu.com/"));
        }

        public MainForm(Subtitle sub, string title, string description, Form parentForm)
            : this()
        {
            Text = title;
            _subtitle = sub;
            GeneratePreview(false);
        }

        public sealed override string Text
        {
            get { return base.Text; }
            set { base.Text = value; }
        }

        internal class BackgroundWorkerParameter
        {
            internal StringBuilder Log { get; set; }
            internal Paragraph P { get; set; }
            internal int Index { get; set; }
            internal string Result { get; set; }
        }

        readonly object _myLock = new object();

        private void GeneratePreview(bool setText)
        {
            if (_subtitle == null)
                return;
            _abort = false;
            int numberOfThreads = 3;
            var threadPool = new List<BackgroundWorker>();
            for (int i = 0; i < numberOfThreads; i++)
            {
                var bw = new BackgroundWorker();
                bw.DoWork += OnBwOnDoWork;
                bw.RunWorkerCompleted += OnBwRunWorkerCompleted;
                threadPool.Add(bw);
            }
            for (int index = 0; index < _subtitle.Paragraphs.Count; index++)
            {
                Paragraph p = _subtitle.Paragraphs[index];
                var before = p.Text;
                var after = string.Empty;
                if (setText)
                {
                    var arg = new BackgroundWorkerParameter { P = p, Index = index, Log = new StringBuilder() };
                    threadPool.First(bw => !bw.IsBusy).RunWorkerAsync(arg);
                    while (threadPool.All(bw => bw.IsBusy))
                    {
                        Application.DoEvents();
                        System.Threading.Thread.Sleep(100);
                    }
                }
                else
                {
                    AddToListView(p, before, after);
                }
                if (_abort)
                {
                    _abort = false;
                    return;
                }
            }
            while (threadPool.Any(bw => bw.IsBusy))
            {
                Application.DoEvents();
                System.Threading.Thread.Sleep(100);
            }
            foreach (var backgroundWorker in threadPool)
            {
                backgroundWorker.Dispose();
            }
        }

        private void OnBwRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs runWorkerCompletedEventArgs)
        {
            progressBar1.Value++;
            var parameter = (BackgroundWorkerParameter)runWorkerCompletedEventArgs.Result;
            parameter.P.Text = parameter.Result;
            textBox1.AppendText(parameter.Log.ToString());
            lock (_myLock)
            {
                var item = listView1.Items[parameter.Index];
                item.SubItems[2].Text = parameter.Result;
            }
        }

        private void OnBwOnDoWork(object sender, DoWorkEventArgs args)
        {
            var parameter = (BackgroundWorkerParameter)args.Argument;
            parameter.Result = Translate(parameter.P, parameter.Log);
            args.Result = parameter;
        }

        private string Translate(Paragraph p, StringBuilder log)
        {
            var startAndEndsWithItalic = p.Text.StartsWith("<i>", StringComparison.Ordinal) && p.Text.EndsWith("</i>", StringComparison.Ordinal);
            string s = p.Text.Replace("<i>", string.Empty).Replace("</i>", string.Empty);
            var result = BaiduTranslate(UrlEncode(Utilities.RemoveHtmlTags(s, true)), _from, _to, log);
            if (startAndEndsWithItalic)
                result = "<i>" + result + "</i>";
            log.AppendLine();
            return result;
        }

        private string UrlEncode(string s)
        {
            return s
                .Replace("%", "%25")
                .Replace("?", "%3F")
                .Replace("/", "%2F")
                .Replace("&", "%26")
                .Replace(":", "%3A")
                .Replace("=", "%3D")
                .Replace("@", "%40")
                .Replace("#", "%23")
                .Replace(",", "%2C")
                .Replace(Environment.NewLine, "%0A");
        }

        private string BaiduTranslate(string text, string from, string to, StringBuilder log, int retryLevel = 0)
        {
            const int maxNumberOfRetries = 2;
            string url = "http://translate.baidu.com/v2transapi";
            var req = WebRequest.Create(url);
            req.Method = "POST";
            req.Headers.Add("cache-control", "no-cache");
            req.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            (req as HttpWebRequest).Accept = "*/*";
            (req as HttpWebRequest).KeepAlive = false;
            (req as HttpWebRequest).ServicePoint.Expect100Continue = false;
            var data = "from=" + from + "&to=" + to + "&query=" + text + "&simple_means_flag=3";
            log.AppendLine("Input to Baidu: " + data);
            byte[] bytes = Encoding.UTF8.GetBytes(data);
            req.ContentLength = bytes.Length;
            var os = req.GetRequestStream();
            os.Write(bytes, 0, bytes.Length);
            os.Close();

            try
            {
                using (var response = req.GetResponse() as HttpWebResponse)
                {
                    if (response == null)
                    {
                        return null;
                    }
                    var responseStream = response.GetResponseStream();
                    if (responseStream == null)
                    {
                        if (retryLevel < maxNumberOfRetries)
                        {
                            retryLevel++;
                            log.AppendLine("Retry: " + retryLevel);
                            return BaiduTranslate(text, from, to, log, retryLevel);
                        }
                        return null;
                    }

                    using (var reader = new StreamReader(responseStream, Encoding.UTF8))
                    {
                        string s = reader.ReadToEnd();
                        log.AppendLine("Result from Baidu: " + s);
                        var idx = s.IndexOf("dst\":", StringComparison.Ordinal);
                        if (idx > 0)
                        {
                            var sb = new StringBuilder();
                            while (idx > 0)
                            {
                                s = s.Substring(idx + 6).TrimStart().TrimStart('"');
                                idx = s.IndexOf('"');
                                if (idx > 0)
                                {
                                    sb.AppendLine(s.Substring(0, idx) + Environment.NewLine);
                                }
                                else
                                {
                                    break;
                                }
                                idx = s.IndexOf("dst\":", StringComparison.Ordinal);
                            }
                            s = sb.ToString().TrimEnd();
                        }
                        else
                        {
                            idx = s.IndexOf("cont\\\":", StringComparison.Ordinal);
                            if (idx > 0)
                            {
                                s = s.Substring(idx + 7).TrimStart().TrimStart('{').TrimStart().TrimStart('\\').TrimStart().TrimStart('"');
                                idx = s.IndexOfAny(new[] { '"', '，' });
                                if (idx > 0)
                                {
                                    s = s.Substring(0, idx).TrimEnd('\\');
                                }
                            }
                        }
                        log.AppendLine();
                        s = DecodeEncodedNonAsciiCharacters(s);
                        return s;
                    }
                }
            }
            catch (Exception exception)
            {
                log.AppendLine(exception.Message + ": " + exception.StackTrace);
                if (retryLevel < maxNumberOfRetries)
                {
                    retryLevel++;
                    log.AppendLine("Retry: " + retryLevel);
                    return BaiduTranslate(text, from, to, log, retryLevel);
                }
            }
            log.AppendLine();
            return null;
        }

        static string DecodeEncodedNonAsciiCharacters(string value)
        {
            return Regex.Replace(value,@"\\u(?<Value>[a-zA-Z0-9]{4})", m => 
            {
                return ((char)int.Parse(m.Groups["Value"].Value, NumberStyles.HexNumber)).ToString();
            });
        }

        private string BaiduTranslateOld(string text, string from, string to, StringBuilder log, int retryLevel = 0)
        {
            const int maxNumberOfRetries = 2;
            string url = "http://translate.baidu.com/transcontent?monLang=en";
            var req = WebRequest.Create(url);
            req.Method = "POST";
            req.Headers.Add("cache-control", "no-cache");
            req.ContentType = "application/x-www-form-urlencoded";
            (req as HttpWebRequest).Accept = "application/json";
            (req as HttpWebRequest).KeepAlive = false;
            (req as HttpWebRequest).ServicePoint.Expect100Continue = false;
            var data = "ie=utf-8&source=txt&query=" + text + "&t=" + _baiduParameterT + "&token=" + _baiduParameterToken + "&from=" + from + "&to=" + to;
            log.AppendLine("Input to Baidu: " + data);
            byte[] bytes = Encoding.UTF8.GetBytes(data);
            req.ContentLength = bytes.Length;
            var os = req.GetRequestStream();
            os.Write(bytes, 0, bytes.Length);
            os.Close();

            try
            {
                using (var response = req.GetResponse() as HttpWebResponse)
                {
                    if (response == null)
                    {
                        return null;
                    }
                    var responseStream = response.GetResponseStream();
                    if (responseStream == null)
                    {
                        if (retryLevel < maxNumberOfRetries)
                        {
                            retryLevel++;
                            log.AppendLine("Retry: " + retryLevel);
                            return BaiduTranslate(text, from, to, log, retryLevel);
                        }
                        return null;
                    }

                    using (var reader = new StreamReader(responseStream, Encoding.UTF8))
                    {
                        string s = reader.ReadToEnd();
                        log.AppendLine("Result from Baidu: " + s);
                        var idx = s.IndexOf("dst\":", StringComparison.Ordinal);
                        if (idx > 0)
                        {
                            var sb = new StringBuilder();
                            while (idx > 0)
                            {
                                s = s.Substring(idx + 6).TrimStart().TrimStart('"');
                                idx = s.IndexOf('"');
                                if (idx > 0)
                                {
                                    sb.AppendLine(s.Substring(0, idx) + Environment.NewLine);
                                }
                                else
                                {
                                    break;
                                }
                                idx = s.IndexOf("dst\":", StringComparison.Ordinal);
                            }
                            s = sb.ToString().TrimEnd();
                        }
                        else
                        {
                            idx = s.IndexOf("cont\\\":", StringComparison.Ordinal);
                            if (idx > 0)
                            {
                                s = s.Substring(idx + 7).TrimStart().TrimStart('{').TrimStart().TrimStart('\\').TrimStart().TrimStart('"');
                                idx = s.IndexOfAny(new[] { '"', '，' });
                                if (idx > 0)
                                {
                                    s = s.Substring(0, idx).TrimEnd('\\');
                                }
                            }
                        }
                        return s;
                    }
                }
            }
            catch (Exception exception)
            {
                log.AppendLine(exception.Message + ": " + exception.StackTrace);
                if (retryLevel < maxNumberOfRetries)
                {
                    retryLevel++;
                    log.AppendLine("Retry: " + retryLevel);
                    return BaiduTranslate(text, from, to, log, retryLevel);
                }
            }
            return null;
        }

        private void AddToListView(Paragraph p, string before, string after)
        {
            var item = new ListViewItem(p.Number.ToString(CultureInfo.InvariantCulture)) { Tag = p };
            item.SubItems.Add(before);
            item.SubItems.Add(after);
            listView1.Items.Add(item);
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            FixedSubtitle = _subtitle.ToText(new SubRip());
            DialogResult = DialogResult.OK;
        }

        private void listView1_Resize(object sender, EventArgs e)
        {
            var size = (listView1.Width - (listView1.Columns[0].Width)) >> 2;
            listView1.Columns[1].Width = size;
            listView1.Columns[2].Width = -2;
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://translate.baidu.com/");
        }

        private void buttonTranslate_Click(object sender, EventArgs e)
        {
            var index = comboBoxLanguagePair.SelectedIndex;
            if (index < 0)
                return;

            buttonTranslate.Enabled = false;
            buttonCancelTranslate.Enabled = true;
            progressBar1.Maximum = _subtitle.Paragraphs.Count;
            progressBar1.Value = 0;
            progressBar1.Visible = true;
            try
            {
                var tp = (BaiduTranslationPair)comboBoxLanguagePair.Items[index];
                _from = tp.FromCode;
                _to = tp.ToCode;
                GeneratePreview(true);
            }
            finally
            {
                buttonTranslate.Enabled = true;
                buttonCancelTranslate.Enabled = false;
                progressBar1.Visible = false;
            }
        }

        private void buttonCancelTranslate_Click(object sender, EventArgs e)
        {
            _abort = true;
        }

    }
}