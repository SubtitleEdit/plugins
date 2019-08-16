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
using System.Xml;
using System.Reflection;

namespace SubtitleEdit
{
    /// <summary>
    /// yandex translate
    /// </summary>
    public partial class MainForm : Form
    {

        public enum FormattingType
        {
            None,
            Italic,
            ItalicTwoLines,
            Parentheses,
            SquareBrackets
        }

        public class FormattingLine
        {
            public FormattingType Formatting { get; set; }
            public string Prefix { get; set; }
        }

        private FormattingLine[] _formattingTypes;
        private bool[] _autoSplit;

        private readonly Subtitle _subtitle;
        private readonly Subtitle _subtitleOriginal;
        private static string _from = "en";
        private static string _to = "ru";
        private const string ParagraphSplitter = "*";
        private bool _abort;
        private bool _tooManyRequests;
        private bool _exit;
        private string _lastLanguage = null;

        private static Dictionary<string, string> _translateLookup = new Dictionary<string, string>
        {
            { "en", "" },
            { "de", "" },
            { "fr", "" },
            { "es", "" },
            { "it", "" },
            { "nl", "" },
            { "pl", "" },
            { "ru", "" },

            { "en...", "..." },
            { "de...", "..." },
            { "fr...", "..." },
            { "es...", "..." },
            { "it...", "..." },
            { "nl...", "..." },
            { "pl...", "..." },
            { "ru...", "..." },
        };

        public class TranslationLanguage
        {
            public string Code { get; set; }
            public string Name { get; set; }

            public TranslationLanguage(string code, string name)
            {
                Code = code;
                Name = name;
            }

            public override string ToString()
            {
                return Name; // will be displayed in combobox
            }
        }

        public string FixedSubtitle { get; private set; }

        public MainForm()
        {
            InitializeComponent();
            textBox1.Visible = false;
            listView1.Columns[2].Width = -2;
            buttonCancelTranslate.Enabled = false;
            RestoreSettings();
        }

        private List<TranslationLanguage> GetTranslationLanguages()
        {
            var list = new List<TranslationLanguage>
            {
                 new TranslationLanguage("az", "Azerbaijan"),
                new TranslationLanguage("ml", "Malayalam"),
                new TranslationLanguage("sq", "Albanian"),
                new TranslationLanguage("mt", "Maltese"),
                new TranslationLanguage("am", "Amharic"),
                new TranslationLanguage("mk", "Macedonian"),
                new TranslationLanguage("en", "English"),
                new TranslationLanguage("mi", "Maori"),
                new TranslationLanguage("ar", "Arabic"),
                new TranslationLanguage("mr", "Marathi"),
                new TranslationLanguage("hy", "Armenian"),
                new TranslationLanguage("af", "Afrikaans"),
                new TranslationLanguage("mn", "Mongolian"),
                new TranslationLanguage("eu", "Basque"),
                new TranslationLanguage("de", "German"),
                new TranslationLanguage("ba", "Bashkir"),
                new TranslationLanguage("ne", "Nepali"),
                new TranslationLanguage("be", "Belarusian"),
                new TranslationLanguage("no", "Norwegian"),
                new TranslationLanguage("bn", "Bengali"),
                new TranslationLanguage("pa", "Punjabi"),
                new TranslationLanguage("my", "Burmese"),
                new TranslationLanguage("bg", "Bulgarian"),
                new TranslationLanguage("fa", "Persian"),
                new TranslationLanguage("bs", "Bosnian"),
                new TranslationLanguage("pl", "Polish"),
                new TranslationLanguage("cy", "Welsh"),
                new TranslationLanguage("pt", "Portuguese"),
                new TranslationLanguage("hu", "Hungarian"),
                new TranslationLanguage("ro", "Romanian"),
                new TranslationLanguage("vi", "Vietnamese"),
                new TranslationLanguage("ru", "Russian"),
                new TranslationLanguage("ht", "Haitian(Creole)"),
                new TranslationLanguage("gl", "Galician"),
                new TranslationLanguage("sr", "Serbian"),
                new TranslationLanguage("nl", "Dutch"),
                new TranslationLanguage("si", "Sinhala"),
                new TranslationLanguage("sk", "Slovakian"),
                new TranslationLanguage("el", "Greek"),
                new TranslationLanguage("sl", "Slovenian"),
                new TranslationLanguage("ka", "Georgian"),
                new TranslationLanguage("sw", "Swahili"),
                new TranslationLanguage("gu", "Gujarati"),
                new TranslationLanguage("su", "Sundanese"),
                new TranslationLanguage("da", "Danish"),
                new TranslationLanguage("tg", "Tajik"),
                new TranslationLanguage("he", "Hebrew"),
                new TranslationLanguage("th", "Thai"),
                new TranslationLanguage("yi", "Yiddish"),
                new TranslationLanguage("tl", "Tagalog"),
                new TranslationLanguage("id", "Indonesian"),
                new TranslationLanguage("ta", "Tamil"),
                new TranslationLanguage("ga", "Irish"),
                new TranslationLanguage("tt", "Tatar"),
                new TranslationLanguage("it", "Italian"),
                new TranslationLanguage("te", "Telugu"),
                new TranslationLanguage("is", "Icelandic"),
                new TranslationLanguage("tr", "Turkish"),
                new TranslationLanguage("es", "Spanish"),
                new TranslationLanguage("kk", "Kazakh"),
                new TranslationLanguage("uz", "Uzbek"),
                new TranslationLanguage("kn", "Kannada"),
                new TranslationLanguage("uk", "Ukrainian"),
                new TranslationLanguage("ca", "Catalan"),
                new TranslationLanguage("ur", "Urdu"),
                new TranslationLanguage("ky", "Kyrgyz"),
                new TranslationLanguage("fi", "Finnish"),
                new TranslationLanguage("zh", "Chinese"),
                new TranslationLanguage("fr", "French"),
                new TranslationLanguage("ko", "Korean"),
                new TranslationLanguage("hi", "Hindi"),
                new TranslationLanguage("xh", "Xhosa"),
                new TranslationLanguage("hr", "Croatian"),
                new TranslationLanguage("km", "Khmer"),
                new TranslationLanguage("cs", "Czech"),
                new TranslationLanguage("lo", "Laotian"),
                new TranslationLanguage("sv", "Swedish"),
                new TranslationLanguage("la", "Latin"),
                new TranslationLanguage("gd", "Scottish"),
                new TranslationLanguage("lv", "Latvian"),
                new TranslationLanguage("et", "Estonian"),
                new TranslationLanguage("lt", "Lithuanian"),
                new TranslationLanguage("eo", "Esperanto"),
                new TranslationLanguage("lb", "Luxembourgish"),
                new TranslationLanguage("jv", "Javanese"),
                new TranslationLanguage("mg", "Malagasy"),
                new TranslationLanguage("ja", "Japanese"),
                new TranslationLanguage("ms", "Malay"),
                new TranslationLanguage("udm", "Udmurt"),
                new TranslationLanguage("mrj", "Hill Mari"),
                new TranslationLanguage("ceb", "Cebuano"),
                new TranslationLanguage("mhr", "Mari"),
                new TranslationLanguage("pap", "Papiamento")
            };
            return list.OrderBy(p => p.Name).ToList();
        }

        private void SetLanguages(ComboBox comboBox, string language)
        {
            comboBox.Items.Clear();
            foreach (var tl in GetTranslationLanguages())
                comboBox.Items.Add(tl);

            int i = 0;
            foreach (var l in comboBox.Items)
            {
                var tl = l as TranslationLanguage;
                if (tl.Code.Equals(language, StringComparison.OrdinalIgnoreCase))
                {
                    comboBox.SelectedIndex = i;
                    return;
                }
                i++;
            }
            comboBox.SelectedIndex = 0;
        }

        public MainForm(Subtitle sub, string title, string description, Form parentForm)
            : this()
        {
            Text = title;
            _subtitle = sub;
            _subtitleOriginal = new Subtitle(sub);
            foreach (var p in sub.Paragraphs)
                p.Text = string.Empty;
            var languageCode = LanguageAutoDetect.AutoDetectGoogleLanguage(_subtitleOriginal);
            _formattingTypes = new FormattingLine[_subtitle.Paragraphs.Count];
            for (int i = 0; i < _subtitle.Paragraphs.Count; i++)
            {
                _formattingTypes[i] = new FormattingLine { Formatting = FormattingType.None, Prefix = string.Empty };
            }
            _autoSplit = new bool[_subtitle.Paragraphs.Count];
            SetLanguages(comboBoxLanguageFrom, languageCode);
            SetLanguages(comboBoxLanguageTo, _to);
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
            internal string Text { get; set; }
            internal List<int> Indexes { get; set; }
            internal string Result { get; set; }
        }

        private readonly object _myLock = new object();
        private readonly object _lookupLock = new object();

        private void GeneratePreview(bool setText)
        {
            if (_subtitle == null)
                return;

            try
            {
                _abort = false;
                int numberOfThreads = 4;
                var threadPool = new List<BackgroundWorker>();
                for (int i = 0; i < numberOfThreads; i++)
                {
                    var bw = new BackgroundWorker();
                    bw.DoWork += OnBwOnDoWork;
                    bw.RunWorkerCompleted += OnBwRunWorkerCompleted;
                    threadPool.Add(bw);
                }
                var textToTranslate = new StringBuilder();
                var indexesToTranslate = new List<int>();
                for (int index = 0; index < _subtitle.Paragraphs.Count; index++)
                {
                    if (index < listView1.Items.Count)
                    {
                        var listViewItem = listView1.Items[index];
                        if (!string.IsNullOrWhiteSpace(listViewItem.SubItems[2].Text) && _from + _to == _lastLanguage)
                        {
                            if (progressBar1.Value < progressBar1.Maximum)
                                progressBar1.Value++;
                            continue;
                        }
                    }

                    Paragraph p = _subtitleOriginal.Paragraphs[index];
                    string text = SetFormattingTypeAndSplitting(index, p.Text, (comboBoxLanguageFrom.SelectedItem as TranslationLanguage).Code.StartsWith("ZH"));
                    var before = text;
                    var after = string.Empty;
                    if (setText)
                    {
                        //if (text.Length + textToTranslate.Length > max) - max is too low for merging texts to really have any effect
                        {
                            var arg = new BackgroundWorkerParameter { Text = textToTranslate.ToString().TrimEnd().TrimEnd('*').TrimEnd(), Indexes = indexesToTranslate, Log = new StringBuilder() };
                            textToTranslate = new StringBuilder();
                            indexesToTranslate = new List<int>();
                            threadPool.First(bw => !bw.IsBusy).RunWorkerAsync(arg);
                            while (threadPool.All(bw => bw.IsBusy))
                            {
                                Application.DoEvents();
                                System.Threading.Thread.Sleep(100);
                            }
                        }
                        textToTranslate.AppendLine(text);
                        textToTranslate.AppendLine(ParagraphSplitter);
                        indexesToTranslate.Add(index);
                    }
                    else
                    {
                        AddToListView(p, before, after);
                    }
                    if (_abort)
                    {
                        _abort = false;
                        _exit = true;
                        return;
                    }
                    if (_tooManyRequests)
                    {
                        _tooManyRequests = false;
                        _exit = true;
                        MessageBox.Show("Yandex returned 'Too many requests' - please wait a while!");
                        return;
                    }
                }
                if (textToTranslate.Length > 0)
                {
                    while (threadPool.All(bw => bw.IsBusy))
                    {
                        Application.DoEvents();
                        System.Threading.Thread.Sleep(100);
                    }
                    var arg = new BackgroundWorkerParameter { Text = textToTranslate.ToString().TrimEnd().TrimEnd('*').TrimEnd(), Indexes = indexesToTranslate, Log = new StringBuilder() };
                    threadPool.First(bw => !bw.IsBusy).RunWorkerAsync(arg);
                }
                while (threadPool.Any(bw => bw.IsBusy))
                {
                    Application.DoEvents();
                    System.Threading.Thread.Sleep(100);
                }
                try
                {
                    foreach (var backgroundWorker in threadPool)
                    {
                        backgroundWorker.Dispose();
                    }
                }
                catch
                {
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message + Environment.NewLine + exception.StackTrace);
            }
        }

        private void OnBwRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs runWorkerCompletedEventArgs)
        {
            if (_exit)
                return;

            try
            {
                if (progressBar1.Value < progressBar1.Maximum)
                    progressBar1.Value++;

                var parameter = (BackgroundWorkerParameter)runWorkerCompletedEventArgs.Result;
                var results = GetTextResults(parameter.Result, parameter.Indexes.Count);
                textBox1.AppendText(parameter.Log.ToString());
                lock (_myLock)
                {
                    int i = 0;
                    foreach (var index in parameter.Indexes)
                    {
                        var cleanText = results[i];
                        if (_autoSplit[index])
                        {
                            cleanText = Utilities.AutoBreakLine(cleanText);
                        }
                        if (Utilities.GetNumberOfLines(cleanText) == 1 && Utilities.GetNumberOfLines(_subtitleOriginal.Paragraphs[index].Text) == 2)
                        {
                            if (!_autoSplit[index])
                            {
                                cleanText = Utilities.AutoBreakLine(cleanText);
                            }
                        }

                        SetFormatting(index, cleanText);

                        // follow newly translated lines
                        var item = listView1.Items[index];
                        item.SubItems[2].Text = _subtitle.Paragraphs[index].Text.Replace(Environment.NewLine, "<br />");
                        if (listView1.CanFocus)
                            listView1.EnsureVisible(index);
                    }

                    i++;
                }
            }
            catch
            {
                // ignore
            }
        }

        private void SetFormatting(int index, string cleanText)
        {
            if (_formattingTypes[index].Formatting == FormattingType.ItalicTwoLines || _formattingTypes[index].Formatting == FormattingType.Italic)
            {
                _subtitle.Paragraphs[index].Text = "<i>" + cleanText + "</i>";
            }
            else if (_formattingTypes[index].Formatting == FormattingType.Parentheses)
            {
                _subtitle.Paragraphs[index].Text = "(" + cleanText + ")";
            }
            else if (_formattingTypes[index].Formatting == FormattingType.SquareBrackets)
            {
                _subtitle.Paragraphs[index].Text = "[" + cleanText + "]";
            }
            else
            {
                _subtitle.Paragraphs[index].Text = cleanText;
            }
            _subtitle.Paragraphs[index].Text = _formattingTypes[index].Prefix + _subtitle.Paragraphs[index].Text;
        }

        private List<string> GetTextResults(string text, int count)
        {
            if (string.IsNullOrEmpty(text))
                return new List<string> { string.Empty };

            var result = new string[count];
            var sb = new StringBuilder();
            int index = 0;
            text = text.Replace("<br />", Environment.NewLine);
            text = text.Replace("<br/>", Environment.NewLine);
            foreach (var line in text.SplitToLines())
            {
                if (line == ParagraphSplitter)
                {
                    if (index < count)
                        result[index] = sb.ToString().Trim();
                    index++;
                    sb.Clear();
                }
                else
                {
                    sb.AppendLine(line.Trim());
                }
            }
            if (sb.Length > 0 && index < count)
            {
                result[index] = sb.ToString().Trim();
            }
            return result.ToList();
        }

        private void OnBwOnDoWork(object sender, DoWorkEventArgs args)
        {
            var parameter = (BackgroundWorkerParameter)args.Argument;

            if (_translateLookup.ContainsKey(_from + parameter.Text))
            {
                parameter.Result = _translateLookup[_from + parameter.Text];
                parameter.Log.AppendLine("Using cache: " + _from + parameter.Text + " -> " + parameter.Result);
                parameter.Log.AppendLine();
            }
            else
            {
                parameter.Result = Translate(parameter.Text, parameter.Log);

                if (parameter.Indexes.Count == 1)
                {
                    lock (_lookupLock)
                    {
                        var index = parameter.Indexes.First();
                        if (!_translateLookup.ContainsKey(_from + _subtitleOriginal.Paragraphs[index].Text))
                            _translateLookup.Add(_from + _subtitleOriginal.Paragraphs[index].Text, parameter.Result);
                    }
                }
            }
            args.Result = parameter;
        }

        private string Translate(string text, StringBuilder log)
        {
            var result = YandexTranslate(Utilities.RemoveHtmlTags(text, true), _from, _to, log);
            log.AppendLine();
            return result;
        }

        private string YandexTranslate(string text, string from, string to, StringBuilder log, int retryLevel = 0)
        {
            // see https://tech.yandex.com/translate/doc/dg/reference/translate-docpage/
            const int maxNumberOfRetries = 2;
            string url = "https://translate.yandex.net/api/v1.5/tr.json/translate?key=" + textBoxApiKey.Text.Trim() + "&lang=" + from + "-" + to; // + "&format=html";
            var req = WebRequest.Create(url);
            req.Method = "POST";
            req.Headers.Add("cache-control", "no-cache");
            req.ContentType = "application/x-www-form-urlencoded";
            (req as HttpWebRequest).Accept = "*/*";
            (req as HttpWebRequest).KeepAlive = false;
            (req as HttpWebRequest).ServicePoint.Expect100Continue = false;
            string postData = $"text={System.Web.HttpUtility.UrlEncode(Utilities.RemoveHtmlTags(text, true), Encoding.UTF8)}";
            log.AppendLine("Input to Yandex: " + postData);
            byte[] bytes = Encoding.UTF8.GetBytes(postData);
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
                            return YandexTranslate(text, from, to, log, retryLevel);
                        }
                        return null;
                    }

                    using (var reader = new StreamReader(responseStream, Encoding.UTF8))
                    {
                        string s = reader.ReadToEnd();
                        log.AppendLine("Result from Yandex: " + s);
                        var tag = "text\":";
                        var idx = s.IndexOf(tag, StringComparison.Ordinal);
                        if (idx > 0)
                        {
                            var sb = new StringBuilder();
                            if (idx > 0)
                            {
                                s = s.Substring(idx + tag.Length).TrimStart().TrimStart('"');
                                s = s.TrimStart('[');
                                s = s.TrimStart('"');
                                idx = s.IndexOf('"');
                                while (idx > 0 && s.Substring(idx - 1, 1) == "\\" && idx + 2 < s.Length)
                                {
                                    idx = s.IndexOf('"', idx + 1);
                                }
                                if (idx > 0)
                                {
                                    sb.AppendLine(s.Substring(0, idx).Replace("\\\"", "\"") + Environment.NewLine);
                                }
                            }
                            s = sb.ToString().TrimEnd();
                        }
                        log.AppendLine();
                        s = s.Replace("\\n", Environment.NewLine);
                        s = DecodeEncodedNonAsciiCharacters(s);
                        return string.Join(Environment.NewLine, s.SplitToLines());
                    }
                }
            }
            catch (Exception exception)
            {
                if (exception.Message.Contains("Exceeded the daily limit"))
                {
                    _tooManyRequests = true;
                }
                log.AppendLine(exception.Message + ": " + exception.StackTrace);
                if (retryLevel < maxNumberOfRetries)
                {
                    retryLevel++;
                    log.AppendLine("Retry: " + retryLevel);
                    return YandexTranslate(text, from, to, log, retryLevel);
                }
            }
            log.AppendLine();
            return null;
        }

        static string DecodeEncodedNonAsciiCharacters(string value)
        {
            return Regex.Replace(value, @"\\u(?<Value>[a-zA-Z0-9]{4})", m =>
            {
                return ((char)int.Parse(m.Groups["Value"].Value, NumberStyles.HexNumber)).ToString();
            });
        }

        private void AddToListView(Paragraph p, string before, string after)
        {
            var item = new ListViewItem(p.Number.ToString(CultureInfo.InvariantCulture)) { Tag = p };
            item.SubItems.Add(before.Replace(Environment.NewLine, "<br />"));
            item.SubItems.Add(after.Replace(Environment.NewLine, "<br />"));
            listView1.Items.Add(item);
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            SaveSettings();
            FixedSubtitle = _subtitle.ToText(new SubRip());
            DialogResult = DialogResult.OK;
        }

        private void listView1_Resize(object sender, EventArgs e)
        {
            var size = (listView1.Width - listView1.Columns[0].Width) >> 2;
            listView1.Columns[1].Width = size;
            listView1.Columns[2].Width = -2;
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://www.yandex.com");
        }

        private void buttonTranslate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBoxApiKey.Text))
            {
                MessageBox.Show("Please fill out api key from yandex.com");
                return;
            }

            _tooManyRequests = false;
            _exit = false;
            _abort = false;
            buttonTranslate.Enabled = false;
            buttonCancelTranslate.Enabled = true;
            progressBar1.Maximum = _subtitle.Paragraphs.Count;
            progressBar1.Value = 0;
            progressBar1.Visible = true;
            try
            {
                _from = ((TranslationLanguage)comboBoxLanguageFrom.Items[comboBoxLanguageFrom.SelectedIndex]).Code;
                _to = ((TranslationLanguage)comboBoxLanguageTo.Items[comboBoxLanguageTo.SelectedIndex]).Code;
                GeneratePreview(true);
                _lastLanguage = _from + _to;
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

        private string SetFormattingTypeAndSplitting(int i, string text, bool skipSplit)
        {
            text = text.Trim();

            if (text.StartsWith("{\\") && text.Contains("}"))
            {
                int endIndex = text.IndexOf("}") + 1;
                _formattingTypes[i].Prefix = text.Substring(0, endIndex).Trim();
                text = text.Remove(0, endIndex).Trim();
            }

            if (text.StartsWith("<i>", StringComparison.Ordinal) && text.EndsWith("</i>", StringComparison.Ordinal) && text.Contains("</i>" + Environment.NewLine + "<i>") && Utilities.GetNumberOfLines(text) == 2 && Utilities.CountTagInText(text, "<i>") == 2)
            {
                _formattingTypes[i].Formatting = FormattingType.ItalicTwoLines;
                text = HtmlUtil.RemoveOpenCloseTags(text, HtmlUtil.TagItalic);
            }
            else if (text.StartsWith("<i>", StringComparison.Ordinal) && text.EndsWith("</i>", StringComparison.Ordinal) && Utilities.CountTagInText(text, "<i>") == 1)
            {
                _formattingTypes[i].Formatting = FormattingType.Italic;
                text = text.Substring(3, text.Length - 7);
            }
            else if (text.StartsWith("(", StringComparison.Ordinal) && text.EndsWith(")", StringComparison.Ordinal))
            {
                _formattingTypes[i].Formatting = FormattingType.Parentheses;
                text = text.Substring(1, text.Length - 2);
            }
            else if (text.StartsWith("[", StringComparison.Ordinal) && text.EndsWith("]", StringComparison.Ordinal))
            {
                _formattingTypes[i].Formatting = FormattingType.SquareBrackets;
                text = text.Substring(1, text.Length - 2);
            }
            else
            {
                _formattingTypes[i].Formatting = FormattingType.None;
            }

            if (skipSplit)
            {
                return text;
            }

            var lines = text.SplitToLines();
            if (lines.Length == 2 && !string.IsNullOrEmpty(lines[0]) && (Utilities.AllLettersAndNumbers + ",").Contains(lines[0].Substring(lines[0].Length - 1)))
            {
                _autoSplit[i] = true;
                text = Utilities.RemoveLineBreaks(text);
            }

            return text.TrimEnd();
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
            else if (e.KeyCode == Keys.F1)
            {
                if (textBox1.Visible)
                {
                    textBox1.Visible = false;
                }
                else
                {
                    textBox1.Visible = true;
                    textBox1.BringToFront();
                }
            }
        }

        private string GetSettingsFileName()
        {
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);
            if (path != null && path.StartsWith("file:\\", StringComparison.Ordinal))
                path = path.Remove(0, 6);
            path = Path.Combine(path, "Plugins");
            if (!Directory.Exists(path))
                path = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Subtitle Edit"), "Plugins");
            return Path.Combine(path, "YandexTranslator.xml");
        }

        private void RestoreSettings()
        {
            string fileName = GetSettingsFileName();
            try
            {
                var doc = new XmlDocument();
                doc.Load(fileName);
                textBoxApiKey.Text = DecodeFrom64(doc.DocumentElement.SelectSingleNode("ApiKey").InnerText);
                _to = doc.DocumentElement.SelectSingleNode("Target").InnerText;
            }
            catch
            {
            }
        }

        private void SaveSettings()
        {
            string fileName = GetSettingsFileName();
            try
            {
                var doc = new XmlDocument();
                doc.LoadXml("<Translator><ApiKey/><Target/></Translator>");
                doc.DocumentElement.SelectSingleNode("ApiKey").InnerText = EncodeTo64(textBoxApiKey.Text.Trim());
                doc.DocumentElement.SelectSingleNode("Target").InnerText = _to;
                doc.Save(fileName);
            }
            catch
            {
            }
        }

        private static string EncodeTo64(string toEncode)
        {
            byte[] toEncodeAsBytes = System.Text.Encoding.Unicode.GetBytes(toEncode);
            return Convert.ToBase64String(toEncodeAsBytes);
        }

        public static string DecodeFrom64(string encodedData)
        {
            byte[] encodedDataAsBytes = Convert.FromBase64String(encodedData);
            return System.Text.Encoding.Unicode.GetString(encodedDataAsBytes);
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://translate.yandex.com/developers/keys");
        }
    }
}