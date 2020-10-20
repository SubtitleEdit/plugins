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
    /// <summary>
    /// DeepL.com translate
    /// SE github issue - https://github.com/SubtitleEdit/subtitleedit/issues/2574
    /// Example code:
    ///   https://github.com/vsetka/deepl-translator/blob/master/index.js
    ///   https://github.com/chriskonnertz/DeepLy/blob/master/src/ChrisKonnertz/DeepLy/DeepLy.php
    /// </summary>
    public partial class MainForm : Form
    {

        private enum FormattingType
        {
            None,
            Italic,
            ItalicTwoLines,
            Parentheses,
            SquareBrackets
        }

        private FormattingType[] _formattingTypes;
        private bool[] _autoSplit;

        private readonly Subtitle _subtitle;
        private readonly Subtitle _subtitleOriginal;
        private static string _from = "EN";
        private static string _to = "DE";
        private const string ParagraphSplitter = "*";
        private bool _abort;
        private bool _tooManyRequests;
        private bool _exit;
        private static Dictionary<string, string> _translateLookup = new Dictionary<string, string>
        {
            { "EN", "" },
            { "DE", "" },
            { "FR", "" },
            { "ES", "" },
            { "IT", "" },
            { "NL", "" },
            { "PL", "" },
            { "PT", "" },
            { "RU", "" },

            { "EN...", "..." },
            { "DE...", "..." },
            { "FR...", "..." },
            { "ES...", "..." },
            { "IT...", "..." },
            { "NL...", "..." },
            { "PL...", "..." },
            { "PT...", "..." },
            { "RU...", "..." },
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
        }

        private void SetLanguages(ComboBox comboBox, string language)
        {
            comboBox.Items.Clear();
            comboBox.Items.AddRange(new object[]
            {
                new TranslationLanguage("EN", "English"),
                new TranslationLanguage("DE", "German"),
                new TranslationLanguage("FR", "French"),
                new TranslationLanguage("ES", "Spanish"),
                new TranslationLanguage("IT", "Italian"),
                new TranslationLanguage("NL", "Dutch"),
                new TranslationLanguage("PL", "Polish"),
                new TranslationLanguage("PT", "Portuguese"),
                new TranslationLanguage("RU", "Russian"),
            });
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
            _formattingTypes = new FormattingType[_subtitle.Paragraphs.Count];
            _autoSplit = new bool[_subtitle.Paragraphs.Count];
            SetLanguages(comboBoxLanguageFrom, languageCode);
            SetLanguages(comboBoxLanguageTo, "DE");
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
            //  const int max = 90; // DeepL uses max 90 chars at a time
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
                int start = 0;
                if (listView1.SelectedItems.Count > 0)
                {
                    start = listView1.SelectedItems[0].Index;
                }
                for (int index = start; index < _subtitle.Paragraphs.Count; index++)
                {
                    if (index < listView1.Items.Count)
                    {
                        var listViewItem = listView1.Items[index];
                        if (!string.IsNullOrWhiteSpace(listViewItem.SubItems[2].Text))
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
                        MessageBox.Show("DeepL returned 'Too many requests' - please wait a while!");
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
            if (_formattingTypes[index] == FormattingType.ItalicTwoLines || _formattingTypes[index] == FormattingType.Italic)
            {
                _subtitle.Paragraphs[index].Text = "<i>" + cleanText + "</i>";
            }
            else if (_formattingTypes[index] == FormattingType.Parentheses)
            {
                _subtitle.Paragraphs[index].Text = "(" + cleanText + ")";
            }
            else if (_formattingTypes[index] == FormattingType.SquareBrackets)
            {
                _subtitle.Paragraphs[index].Text = "[" + cleanText + "]";
            }
            else
            {
                _subtitle.Paragraphs[index].Text = cleanText;
            }
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
            var result = DeeplTranslate(Utilities.RemoveHtmlTags(text, true), _from, _to, log);
            log.AppendLine();
            return result;
        }

        private string GetPostBody()
        {
            return @"{'jsonrpc':'2.0','method':'LMT_handle_jobs','params':{'jobs':[{'kind':'default','raw_en_sentence':'TEXT_TO_TRANSLATE'}],'lang':{'user_preferred_langs':['EN'],'source_lang_user_selected':'FROM_LANGUAGE','target_lang':'TO_LANGUAGE'},'priority':-1},'id':1}';".Replace("'", "\"");
        }

        private string DeeplTranslate(string text, string from, string to, StringBuilder log, int retryLevel = 0)
        {

            var payload = GetPostBody()
                .Replace("TEXT_TO_TRANSLATE", EncodeJsonText(text))
                .Replace("FROM_LANGUAGE", from)
                .Replace("TO_LANGUAGE", to);

            const int maxNumberOfRetries = 2;
            string url = "https://www2.deepl.com/jsonrpc";
            var req = WebRequest.Create(url);
            req.Method = "POST";
            req.Headers.Add("cache-control", "no-cache");
            req.ContentType = "application/json";
            (req as HttpWebRequest).Accept = "*/*";
            (req as HttpWebRequest).KeepAlive = false;
            (req as HttpWebRequest).ServicePoint.Expect100Continue = false;

            log.AppendLine("Input to DeepL: " + payload);
            byte[] bytes = Encoding.UTF8.GetBytes(payload);
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
                            return DeeplTranslate(text, from, to, log, retryLevel);
                        }
                        return null;
                    }

                    using (var reader = new StreamReader(responseStream, Encoding.UTF8))
                    {
                        string s = reader.ReadToEnd();
                        log.AppendLine("Result from DeepL: " + s);
                        var tag = "postprocessed_sentence\":";
                        var idx = s.IndexOf(tag, StringComparison.Ordinal);
                        if (idx > 0)
                        {
                            var sb = new StringBuilder();
                            if (idx > 0)
                            {
                                s = s.Substring(idx + tag.Length).TrimStart().TrimStart('"');
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
                        s = DecodeEncodedNonAsciiCharacters(s);
                        return s;
                    }
                }
            }
            catch (Exception exception)
            {
                if (exception.Message.Contains("(429) Too Many Requests."))
                {
                    _tooManyRequests = true;
                }
                log.AppendLine(exception.Message + ": " + exception.StackTrace);
                if (retryLevel < maxNumberOfRetries)
                {
                    retryLevel++;
                    log.AppendLine("Retry: " + retryLevel);
                    return DeeplTranslate(text, from, to, log, retryLevel);
                }
            }
            log.AppendLine();
            return null;
        }

        public static string EncodeJsonText(string text)
        {
            var sb = new StringBuilder(text.Length);
            foreach (var c in text)
            {
                switch (c)
                {
                    case '\\':
                        sb.Append("\\\\");
                        break;
                    case '"':
                        sb.Append("\\\"");
                        break;
                    default:
                        sb.Append(c);
                        break;
                }
            }
            return sb.ToString().Replace(Environment.NewLine, "<br />");
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
            item.SubItems.Add(p.Text.Replace(Environment.NewLine, "<br />"));
            item.SubItems.Add(after.Replace(Environment.NewLine, "<br />"));
            listView1.Items.Add(item);
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
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
            Process.Start("https://www.deepl.com/");
        }

        private void buttonTranslate_Click(object sender, EventArgs e)
        {
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
            if (text.StartsWith("<i>", StringComparison.Ordinal) && text.EndsWith("</i>", StringComparison.Ordinal) && text.Contains("</i>" + Environment.NewLine + "<i>") && Utilities.GetNumberOfLines(text) == 2 && Utilities.CountTagInText(text, "<i>") == 2)
            {
                _formattingTypes[i] = FormattingType.ItalicTwoLines;
                text = HtmlUtil.RemoveOpenCloseTags(text, HtmlUtil.TagItalic);
            }
            else if (text.StartsWith("<i>", StringComparison.Ordinal) && text.EndsWith("</i>", StringComparison.Ordinal) && Utilities.CountTagInText(text, "<i>") == 1)
            {
                _formattingTypes[i] = FormattingType.Italic;
                text = text.Substring(3, text.Length - 7);
            }
            else if (text.StartsWith("(", StringComparison.Ordinal) && text.EndsWith(")", StringComparison.Ordinal))
            {
                _formattingTypes[i] = FormattingType.Parentheses;
                text = text.Substring(1, text.Length - 2);
            }
            else if (text.StartsWith("[", StringComparison.Ordinal) && text.EndsWith("]", StringComparison.Ordinal))
            {
                _formattingTypes[i] = FormattingType.SquareBrackets;
                text = text.Substring(1, text.Length - 2);
            }
            else
            {
                _formattingTypes[i] = FormattingType.None;
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
    }
}
