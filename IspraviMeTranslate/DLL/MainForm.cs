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
using System.Drawing;

namespace SubtitleEdit
{
    /// <summary>
    /// https://ispravi.me/info/api/
    /// </summary>
    public partial class MainForm : Form
    {

        private IspraviMeApi _translator;

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
        private const char ParagraphSplitter = '*';
        private bool _abort;

        public string FixedSubtitle { get; private set; }

        public MainForm()
        {
            InitializeComponent();
            _translator = new IspraviMeApi("SubtitleEdit");
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

        public MainForm(Subtitle sub, string title, string description, Form parentForm)
            : this()
        {
            linkLabelPoweredBy.Text = "Powered by " + _translator.GetName();
            Text = title;
            _subtitle = sub;
            var languageCode = LanguageAutoDetect.AutoDetectGoogleLanguage(_subtitle);
            _formattingTypes = new FormattingType[_subtitle.Paragraphs.Count];
            _autoSplit = new bool[_subtitle.Paragraphs.Count];
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
            internal IspraviResult Result { get; set; }
        }

        private readonly object _myLock = new object();

        private void GeneratePreview(bool setText)
        {
            if (_subtitle == null)
                return;

            try
            {
                _abort = false;
                int numberOfThreads = 1;
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
                if (setText && listView1.SelectedItems.Count > 0)
                {
                    start = listView1.SelectedItems[0].Index;
                }

                for (int index = start; index < _subtitle.Paragraphs.Count; index++)
                {
                    Paragraph p = _subtitle.Paragraphs[index];
                    string text = SetFormattingTypeAndSplitting(index, p.Text, false);
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

            textBox1.AppendText(parameter.Log.ToString());
            lock (_myLock)
            {
                if (parameter.Result != null && parameter.Result.response != null && parameter.Result.response.errors > 0)
                {
                    _abort = true;
                    _grammerErrorIndex = -1;
                    int i = 0;
                    foreach (var index in parameter.Indexes)
                    {
                        if (parameter.Result.response.errors > 0)
                        {
                            var item = listView1.Items[index];
                            item.Tag = parameter.Result.response;
                            if (listView1.CanFocus)
                                listView1.EnsureVisible(index);

                            _grammerParagraphIndex = index;
                            _grammerErrorIndex = 0;

                            ShowGrammerError();

                            var sb = new StringBuilder();
                            foreach (var error in parameter.Result.response.error)
                            {
                                sb.Append(error.suspicious + " ");
                            }
                            item.SubItems[2].Text = parameter.Result.response.errors.ToString() + ": " + sb.ToString();
                        }
                    }

                    i++;
                }
            }
        }

        private void ShowGrammerError()
        {
            if (_grammerParagraphIndex < 0)
                return;

            var r = (IspraviResponse)listView1.Items[_grammerParagraphIndex].Tag;
            if (r == null || r.error == null || r.error.Count == 0)
                return;

            richTextBoxParagraph.Text = _subtitle.Paragraphs[_grammerParagraphIndex].Text;
            richTextBoxParagraph.SelectAll();
            richTextBoxParagraph.SelectionColor = Control.DefaultForeColor;
            richTextBoxParagraph.SelectionLength = 0;

            if (r.error != null && _grammerErrorIndex >= 0 && _grammerErrorIndex < r.error.Count)
            {
                var error = r.error[_grammerErrorIndex];
                HighLightWord(richTextBoxParagraph, error.suspicious);
                textBoxWord.Text = error.suspicious;

                groupBoxWordNotFound.Text = "Suspicious word (" + error.@class + ")";

                listBoxSuggestions.Items.Clear();
                if (error.suggestions != null)
                {
                    foreach (var item in error.suggestions)
                    {
                        listBoxSuggestions.Items.Add(item);
                    }
                }
            }
        }

        private void ShowNextGrammerError()
        {
            var idx = _grammerParagraphIndex;
            if (idx < 0 || idx >= _subtitle.Paragraphs.Count)
            {
                return;
            }

            var r = listView1.Items[idx].Tag as IspraviResponse;
            if (r == null || r.error == null || r.error.Count == 0)
            {
                return;
            }

            _grammerErrorIndex++;
            if (_grammerErrorIndex >= r.error.Count)
            {
                _grammerParagraphIndex++;
                if (_grammerParagraphIndex >= _subtitle.Paragraphs.Count)
                {
                    return; // done
                }
                _grammerErrorIndex = 0;
                listView1.EnsureVisible(_grammerParagraphIndex);
                listView1.SelectedItems.Clear();
                listView1.Items[_grammerParagraphIndex].Selected = true;
                listView1.FocusedItem = listView1.Items[_grammerParagraphIndex];
                buttonTranslate_Click(null, null);
                return;
            }

            ShowGrammerError();
        }


        private static void HighLightWord(RichTextBox richTextBoxParagraph, string word)
        {
            if (word != null && richTextBoxParagraph.Text.Contains(word))
            {
                const string expectedWordBoundaryChars = " <>-\"”„“«»[]'‘`´¶()♪¿¡.…—!?,:;/\r\n؛،؟";
                for (int i = 0; i < richTextBoxParagraph.Text.Length; i++)
                {
                    if (richTextBoxParagraph.Text.Substring(i).StartsWith(word, StringComparison.Ordinal))
                    {
                        bool startOk = i == 0;
                        if (!startOk)
                            startOk = expectedWordBoundaryChars.Contains(richTextBoxParagraph.Text[i - 1]);
                        if (startOk)
                        {
                            bool endOk = (i + word.Length == richTextBoxParagraph.Text.Length);
                            if (!endOk)
                                endOk = expectedWordBoundaryChars.Contains(richTextBoxParagraph.Text[i + word.Length]);
                            if (endOk)
                            {
                                richTextBoxParagraph.SelectionStart = i + 1;
                                richTextBoxParagraph.SelectionLength = word.Length;
                                while (richTextBoxParagraph.SelectedText != word && richTextBoxParagraph.SelectionStart > 0)
                                {
                                    richTextBoxParagraph.SelectionStart = richTextBoxParagraph.SelectionStart - 1;
                                    richTextBoxParagraph.SelectionLength = word.Length;
                                }
                                if (richTextBoxParagraph.SelectedText == word)
                                {
                                    richTextBoxParagraph.SelectionColor = Color.Red;
                                }
                            }
                        }
                    }
                }

                richTextBoxParagraph.SelectionLength = 0;
                richTextBoxParagraph.SelectionStart = 0;
            }
        }

        private int _grammerParagraphIndex = -1;
        private int _grammerErrorIndex = -1;

        private void OnBwOnDoWork(object sender, DoWorkEventArgs args)
        {
            var parameter = (BackgroundWorkerParameter)args.Argument;
            parameter.Result = CheckGrammer(parameter.Text, parameter.Log);
            args.Result = parameter;
        }

        private IspraviResult CheckGrammer(string text, StringBuilder log)
        {
            var result = _translator.CheckGrammer(text, log);
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
            buttonTranslate.Enabled = false;
            buttonCancelTranslate.Enabled = true;
            progressBar1.Maximum = _subtitle.Paragraphs.Count;
            progressBar1.Value = 0;
            progressBar1.Visible = true;
            try
            {
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
            if (text.StartsWith("<i>", StringComparison.Ordinal) && text.EndsWith("</i>", StringComparison.Ordinal) && text.Contains("</i>" + Environment.NewLine + "<i>") && Utilities.GetNumberOfLines(text) == 2 && Utilities.CountTagInText(text, "<i>") == 1)
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
            return Path.Combine(path, "IspraviMe.xml");
        }

        private void RestoreSettings()
        {
            string fileName = GetSettingsFileName();
            try
            {
                var doc = new XmlDocument();
                doc.Load(fileName);
                // textBoxKey.Text = DecodeFrom64(doc.DocumentElement.SelectSingleNode("Key").InnerText);
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
                doc.LoadXml("<IspraviMe></IspraviMe>");
                // doc.DocumentElement.SelectSingleNode("Key").InnerText = EncodeTo64(textBoxKey.Text.Trim());
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
            return Encoding.Unicode.GetString(encodedDataAsBytes);
        }

        private void buttonGoogleIt_Click(object sender, EventArgs e)
        {
            string text = textBoxWord.Text.Trim();
            if (!string.IsNullOrWhiteSpace(text))
                Process.Start("https://www.google.com/search?q=" + Uri.EscapeDataString(text));
        }

        private void buttonSkipOnce_Click(object sender, EventArgs e)
        {
            ShowNextGrammerError();
        }

        private void buttonSkipAll_Click(object sender, EventArgs e)
        {
            var s = textBoxWord.Text.Trim();
            if (!string.IsNullOrEmpty(s))
                _skipAll.Add(s);
            ShowNextGrammerError();
        }

        private List<string> _skipAll = new List<string>();
    }
}