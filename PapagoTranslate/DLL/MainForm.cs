using System.Globalization;
using Nikse.SubtitleEdit.PluginLogic;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Xml;

namespace SubtitleEdit
{
    /// <summary>
    /// https://papago.naver.com/ translate
    /// </summary>
    public partial class MainForm : Form
    {

        private ITranslator _translator;

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
        private static string _from = "EN";
        private static string _to = "FR";
        private const char ParagraphSplitter = '*';
        private bool _abort;

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
            RestoreSettings();
        }

        private void SetLanguages(ComboBox comboBox, string language)
        {
            comboBox.Items.Clear();
            foreach (var pair in _translator.GetTranslationPairs())
            {
                comboBox.Items.Add(pair);
            }
            int i = 0;
            foreach (var l in comboBox.Items)
            {
                var tl = l as TranslationPair;
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
            _translator = new PapagoTranslator();
            linkLabelPoweredBy.Text = "Powered by " + _translator.GetName();
            Text = title;
            _subtitle = sub;
            var languageCode = LanguageAutoDetect.AutoDetectGoogleLanguage(_subtitle);
            _formattingTypes = new FormattingType[_subtitle.Paragraphs.Count];
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
                    Paragraph p = _subtitle.Paragraphs[index];
                    string text = SetFormattingTypeAndSplitting(index, p.Text, (comboBoxLanguageFrom.SelectedItem as TranslationPair).Code.StartsWith("ZH", StringComparison.OrdinalIgnoreCase));
                    var before = text;
                    var after = string.Empty;
                    if (setText)
                    {
                        //if (text.Length + textToTranslate.Length > max) - max is too low for merging texts to really have any effect
                        {
                            var arg = new BackgroundWorkerParameter { Text = textToTranslate.ToString().TrimEnd().TrimEnd(ParagraphSplitter).TrimEnd(), Indexes = indexesToTranslate, Log = new StringBuilder() };
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
                        textToTranslate.AppendLine(ParagraphSplitter.ToString());
                        indexesToTranslate.Add(index);
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
                if (textToTranslate.Length > 0)
                {
                    while (threadPool.All(bw => bw.IsBusy))
                    {
                        Application.DoEvents();
                        System.Threading.Thread.Sleep(100);
                    }
                    var arg = new BackgroundWorkerParameter { Text = textToTranslate.ToString().TrimEnd().TrimEnd(ParagraphSplitter).TrimEnd(), Indexes = indexesToTranslate, Log = new StringBuilder() };
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
                    if (Utilities.GetNumberOfLines(cleanText) == 1 && Utilities.GetNumberOfLines(_subtitle.Paragraphs[index].Text) == 2)
                    {
                        if (!_autoSplit[index])
                        {
                            cleanText = Utilities.AutoBreakLine(cleanText);
                        }
                    }

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

                    _subtitle.Paragraphs[index].Text = cleanText;
                    var item = listView1.Items[index];
                    item.SubItems[2].Text = _subtitle.Paragraphs[index].Text;
                    i++;
                }
            }
        }

        private List<string> GetTextResults(string text, int count)
        {
            if (text == null)
                text = string.Empty;

            var result = new string[count];
            var sb = new StringBuilder();
            int index = 0;
            text = text.Replace("<br />", Environment.NewLine);
            text = text.Replace("<br/>", Environment.NewLine);
            foreach (var line in text.SplitToLines())
            {
                if (line == ParagraphSplitter.ToString())
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
            parameter.Result = Translate(parameter.Text, parameter.Log);
            args.Result = parameter;
        }

        private string Translate(string text, StringBuilder log)
        {
            var result = _translator.Translate(_from, _to, text, log);
            log.AppendLine();
            return result;
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
            Process.Start(_translator.GetUrl());
        }

        private void buttonTranslate_Click(object sender, EventArgs e)
        {
            _translator.ClientId = textBoxClientId.Text.Trim();
            _translator.ClientSecret = textBoxClientSecret.Text.Trim();

            if (string.IsNullOrEmpty(_translator.ClientId) || string.IsNullOrEmpty(_translator.ClientSecret))
            {
                MessageBox.Show("Please obtain 'Client ID' + 'Client Secret' fro mhttps://developers.naver.com");
                return;
            }

            buttonTranslate.Enabled = false;
            buttonCancelTranslate.Enabled = true;
            progressBar1.Maximum = _subtitle.Paragraphs.Count;
            progressBar1.Value = 0;
            progressBar1.Visible = true;
            try
            {
                _from = ((TranslationPair)comboBoxLanguageFrom.Items[comboBoxLanguageFrom.SelectedIndex]).Code;
                _to = ((TranslationPair)comboBoxLanguageTo.Items[comboBoxLanguageTo.SelectedIndex]).Code;
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

            return text;
        }

        private string GetSettingsFileName()
        {
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);
            if (path != null && path.StartsWith("file:\\", StringComparison.Ordinal))
                path = path.Remove(0, 6);
            path = Path.Combine(path, "Plugins");
            if (!Directory.Exists(path))
                path = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Subtitle Edit"), "Plugins");
            return Path.Combine(path, "PapagoTranslator.xml");
        }

        private void RestoreSettings()
        {
            string fileName = GetSettingsFileName();
            try
            {
                var doc = new XmlDocument();
                doc.Load(fileName);
                textBoxClientId.Text = DecodeFrom64(doc.DocumentElement.SelectSingleNode("ClientId").InnerText);
                textBoxClientSecret.Text = DecodeFrom64(doc.DocumentElement.SelectSingleNode("ClientSecret").InnerText);
                _from = doc.DocumentElement.SelectSingleNode("Source").InnerText;
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
                doc.LoadXml("<PapagoTranslator><ClientId/><ClientSecret/></PapagoTranslator>");
                doc.DocumentElement.SelectSingleNode("ClientId").InnerText = EncodeTo64(textBoxClientId.Text.Trim());
                doc.DocumentElement.SelectSingleNode("ClientSecret").InnerText = EncodeTo64(textBoxClientSecret.Text.Trim());
                doc.DocumentElement.SelectSingleNode("Source").InnerText = _from;
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

    }
}