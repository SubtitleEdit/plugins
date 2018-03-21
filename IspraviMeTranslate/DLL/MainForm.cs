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
        public static readonly string SplitChars = " -.,?!:;\"“”()[]{}|<>/+\r\n¿¡…—–♪♫„«»‹›؛،؟";
        private readonly Subtitle _subtitle;
        private readonly Subtitle _original;
        private bool _abort;
        private List<string> _skipAllList;
        private Dictionary<string, string> _changeAllDictionary;
        private string _currentWord;
        private Dictionary<string, IspraviResult> _cache;
        private int _grammarParagraphIndex = -1;
        private int _grammarErrorIndex = -1;
        BackgroundWorker _bw2 = new BackgroundWorker();
        BackgroundWorker _bw3 = new BackgroundWorker();
        BackgroundWorker _bw4 = new BackgroundWorker();


        public string FixedSubtitle { get; private set; }

        public MainForm()
        {
            InitializeComponent();
            _skipAllList = new List<string>();
            _changeAllDictionary = new Dictionary<string, string>();
            _cache = new Dictionary<string, IspraviResult>();
            _translator = new IspraviMeApi("SubtitleEdit");
            textBox1.Visible = false;
            listView1.Columns[2].Width = -2;
            buttonCancelTranslate.Enabled = false;
            RestoreSettings();

            _bw2 = new BackgroundWorker();
            _bw2.DoWork += OnBwOnDoWork;
            _bw2.RunWorkerCompleted += OnBwRunWorkerCompletedForward;

            _bw3 = new BackgroundWorker();
            _bw3.DoWork += OnBwOnDoWork;
            _bw3.RunWorkerCompleted += OnBwRunWorkerCompletedForward;

            _bw4 = new BackgroundWorker();
            _bw4.DoWork += OnBwOnDoWork;
            _bw4.RunWorkerCompleted += OnBwRunWorkerCompletedForward;
        }

        public MainForm(Subtitle sub, string title, string description, Form parentForm)
            : this()
        {
            linkLabelPoweredBy.Text = "Powered by " + _translator.GetName();
            Text = title;
            _subtitle = sub;
            _original = new Subtitle(sub);
            var languageCode = LanguageAutoDetect.AutoDetectGoogleLanguage(_subtitle);
            InitializeListView();
        }

        private void InitializeListView()
        {
            int start = 0;
            if (listView1.SelectedItems.Count > 0)
            {
                start = listView1.SelectedItems[0].Index;
            }

            for (int index = start; index < _subtitle.Paragraphs.Count; index++)
            {
                Paragraph p = _subtitle.Paragraphs[index];
                var item = new ListViewItem(p.Number.ToString(CultureInfo.InvariantCulture)) { Tag = p };
                item.SubItems.Add(p.Text.Replace(Environment.NewLine, "<br />"));
                item.SubItems.Add(string.Empty);
                listView1.Items.Add(item);
            }

            if (listView1.Items.Count > 0)
                listView1.Items[0].Selected = true;
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


        private void CheckGrammar()
        {
            if (_subtitle == null)
                return;

            try
            {
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
                if (listView1.SelectedItems.Count > 0)
                {
                    start = listView1.SelectedItems[0].Index;
                }

                for (int index = start; index < _subtitle.Paragraphs.Count; index++)
                {
                    Paragraph p = _subtitle.Paragraphs[index];
                    var arg = new BackgroundWorkerParameter { Text = textToTranslate.ToString().TrimEnd(), Indexes = indexesToTranslate, Log = new StringBuilder() };
                    textToTranslate = new StringBuilder();
                    indexesToTranslate = new List<int>();
                    threadPool.First(bw => !bw.IsBusy).RunWorkerAsync(arg);

                    // add some multi threading 
                    CacheForward(index + 2, _bw2);
                    CacheForward(index + 4, _bw3);
                    CacheForward(index + 6, _bw4);

                    while (threadPool.All(bw => bw.IsBusy))
                    {
                        Application.DoEvents();
                        System.Threading.Thread.Sleep(100);
                    }
                    textToTranslate.AppendLine(p.Text);
                    indexesToTranslate.Add(index);
                    if (_abort)
                    {
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
                    var arg = new BackgroundWorkerParameter { Text = textToTranslate.ToString().TrimEnd(), Indexes = indexesToTranslate, Log = new StringBuilder() };
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

        private void CacheForward(int index, BackgroundWorker bw)
        {
            if (!bw.IsBusy && index + 1 < _subtitle.Paragraphs.Count)
            {
                var p2 = _subtitle.Paragraphs[index];
                var parameter = new BackgroundWorkerParameter { Text = p2.Text, Indexes = new List<int> { index }, Log = new StringBuilder() };
                bw.RunWorkerAsync(parameter);
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
                if (parameter.Indexes.Count > 0 && !_cache.ContainsKey(parameter.Text))
                    _cache.Add(parameter.Text, parameter.Result);

                if (_abort)
                    return;
                if (parameter.Indexes.Count > 0)
                {
                    var idx = parameter.Indexes[0];
                    var item = listView1.Items[idx];
                    item.Tag = parameter.Result.response;
                    listView1.EnsureVisible(idx);
                    listView1.SelectedItems.Clear();
                    item.Selected = true;
                    Text = "Ispravi.me grammar checker - " + (idx + 1) + "/" + _subtitle.Paragraphs.Count;
                }

                if (parameter.Result != null && parameter.Result.response != null && parameter.Result.response.errors > 0)
                {
                    _abort = true;
                    _grammarErrorIndex = -1;
                    int i = 0;
                    foreach (var index in parameter.Indexes)
                    {
                        var item = listView1.Items[index];
                        item.Tag = parameter.Result.response;
                        listView1.EnsureVisible(index);
                        listView1.SelectedItems.Clear();
                        item.Selected = true;
                        if (parameter.Result.response.errors > 0)
                        {
                            _grammarParagraphIndex = index;
                            _grammarErrorIndex = 0;
                            ShowGrammarError();

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


        private void OnBwRunWorkerCompletedForward(object sender, RunWorkerCompletedEventArgs runWorkerCompletedEventArgs)
        {
            var parameter = (BackgroundWorkerParameter)runWorkerCompletedEventArgs.Result;
            lock (_myLock)
            {
                if (parameter.Indexes.Count > 0 && !_cache.ContainsKey(parameter.Text))
                    _cache.Add(parameter.Text, parameter.Result);
            }
        }

        private void ShowGrammarError()
        {
            if (_grammarParagraphIndex < 0)
                return;

            var r = (IspraviResponse)listView1.Items[_grammarParagraphIndex].Tag;
            if (r == null || r.error == null || r.error.Count == 0)
                return;

            richTextBoxParagraph.Text = _subtitle.Paragraphs[_grammarParagraphIndex].Text;
            richTextBoxParagraph.SelectAll();
            richTextBoxParagraph.SelectionColor = DefaultForeColor;
            richTextBoxParagraph.SelectionLength = 0;

            if (r.error != null && _grammarErrorIndex >= 0 && _grammarErrorIndex < r.error.Count)
            {
                var error = r.error[_grammarErrorIndex];
                if (_skipAllList.Contains(error.suspicious))
                {
                    ShowNextGrammarError();
                    return;
                }

                if (_changeAllDictionary.ContainsKey(error.suspicious))
                {
                    CorrectWord(error.suspicious, _changeAllDictionary[error.suspicious], -1);
                    ShowNextGrammarError();
                    return;
                }

                groupBoxWordNotFound.Enabled = true;
                HighLightWord(richTextBoxParagraph, error.suspicious);
                textBoxWord.Text = error.suspicious;
                _currentWord = error.suspicious;

                groupBoxWordNotFound.Text = "Suspicious word (" + error.@class + ")";

                listBoxSuggestions.Items.Clear();
                if (error.suggestions != null)
                {
                    foreach (var item in error.suggestions)
                    {
                        listBoxSuggestions.Items.Add(item);
                    }
                }
                if (listBoxSuggestions.Items.Count > 0)
                {
                    listBoxSuggestions.SelectedIndex = 0;
                    groupBoxSuggestions.Enabled = true;
                }
                else
                {
                    groupBoxSuggestions.Enabled = false;
                }
            }
        }

        private void ShowNextGrammarError()
        {
            groupBoxWordNotFound.Enabled = false;
            groupBoxSuggestions.Enabled = false;
            richTextBoxParagraph.Text = string.Empty;
            textBoxWord.Text = string.Empty;
            _currentWord = null;
            var idx = _grammarParagraphIndex;
            if (idx < 0 || idx >= _subtitle.Paragraphs.Count)
            {
                return;
            }

            var r = listView1.Items[idx].Tag as IspraviResponse;
            if (r == null || r.error == null || r.error.Count == 0)
            {
                return;
            }

            _grammarErrorIndex++;
            if (_grammarErrorIndex >= r.error.Count)
            {
                _grammarParagraphIndex++;
                if (_grammarParagraphIndex >= _subtitle.Paragraphs.Count)
                {
                    return; // done
                }
                _grammarErrorIndex = 0;
                listView1.EnsureVisible(_grammarParagraphIndex);
                listView1.SelectedItems.Clear();
                listView1.Items[_grammarParagraphIndex].Selected = true;
                listView1.FocusedItem = listView1.Items[_grammarParagraphIndex];
                buttonTranslate_Click(null, null);
                return;
            }

            ShowGrammarError();
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

        private void OnBwOnDoWork(object sender, DoWorkEventArgs args)
        {
            var parameter = (BackgroundWorkerParameter)args.Argument;
            if (_cache.ContainsKey(parameter.Text))
            {
                parameter.Log.AppendLine("Using cache from '" + parameter.Text + "'");
                parameter.Result = _cache[parameter.Text];
            }
            else
            {
                parameter.Result = CheckGrammer(parameter.Text, parameter.Log);
            }
            args.Result = parameter;
        }

        private IspraviResult CheckGrammer(string text, StringBuilder log)
        {
            var result = _translator.CheckGrammer(text, log);
            log.AppendLine();
            return result;
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
            _abort = false;
            try
            {
                CheckGrammar();
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
            ShowNextGrammarError();
        }

        private void buttonSkipAll_Click(object sender, EventArgs e)
        {
            var s = textBoxWord.Text.Trim();
            if (!string.IsNullOrEmpty(s))
                _skipAllList.Add(s);
            ShowNextGrammarError();
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F1)
            {
                if (textBox1.Visible)
                {
                    textBox1.Visible = false;
                    listView1.Visible = true;
                }
                else
                {
                    textBox1.Visible = true;
                    listView1.Visible = false;
                }
            }
        }

        private void buttonChangeAll_Click(object sender, EventArgs e)
        {
            if (!_changeAllDictionary.ContainsKey(_currentWord))
                _changeAllDictionary.Add(_currentWord, textBoxWord.Text.Trim());
        }

        private void textBoxWord_TextChanged(object sender, EventArgs e)
        {
            buttonChange.Enabled = _currentWord != null && textBoxWord.Text.Trim() != _currentWord;
            buttonChangeAll.Enabled = _currentWord != null && textBoxWord.Text.Trim() != _currentWord;
        }

        public void CorrectWord(string changeWord, string oldWord, int wordIndex)
        {
            var p = _subtitle.Paragraphs[_grammarParagraphIndex];
            int startIndex = p.Text.IndexOf(oldWord, StringComparison.Ordinal);
            if (wordIndex >= 0)
            {
                startIndex = p.Text.IndexOf(oldWord, GetPositionFromWordIndex(p.Text, wordIndex), StringComparison.Ordinal);
            }
            while (startIndex >= 0 && startIndex < p.Text.Length && p.Text.Substring(startIndex).Contains(oldWord))
            {
                bool startOk = startIndex == 0 ||
                               "«»“” <>-—+/'\"[](){}¿¡….,;:!?%&$£\r\n؛،؟".Contains(p.Text[startIndex - 1]) ||
                               startIndex == p.Text.Length - oldWord.Length;
                if (startOk)
                {
                    int end = startIndex + oldWord.Length;
                    if (end <= p.Text.Length && end == p.Text.Length || ("«»“” ,.!?:;'()<>\"-—+/[]{}%&$£…\r\n؛،؟").Contains(p.Text[end]))
                    {
                        p.Text = p.Text.Remove(startIndex, oldWord.Length).Insert(startIndex, changeWord);
                    }
                }
                if (startIndex + 2 >= p.Text.Length)
                    startIndex = -1;
                else
                    startIndex = p.Text.IndexOf(oldWord, startIndex + 2, StringComparison.Ordinal);

                // stop if using index
                if (wordIndex >= 0)
                    startIndex = -1;
            }
            listView1.Items[_grammarParagraphIndex].SubItems[1].Text = p.Text.Replace(Environment.NewLine, "<br />");
        }

        private int GetPositionFromWordIndex(string text, int wordIndex)
        {
            var sb = new StringBuilder();
            int index = -1;
            for (int i = 0; i < text.Length; i++)
            {
                if (SplitChars.Contains(text[i]))
                {
                    if (sb.Length > 0)
                    {
                        index++;
                        if (index == wordIndex)
                        {
                            int pos = i - sb.Length;
                            if (pos > 0)
                                pos--;
                            if (pos >= 0)
                                return pos;
                        }
                    }
                    sb.Clear();
                }
                else
                {
                    sb.Append(text[i]);
                }
            }
            if (sb.Length > 0)
            {
                index++;
                if (index == wordIndex)
                {
                    int pos = text.Length - 1 - sb.Length;
                    if (pos >= 0)
                        return pos;
                }
            }
            return 0;
        }

        private void buttonChange_Click(object sender, EventArgs e)
        {
            CorrectWord(textBoxWord.Text.Trim(), _currentWord, -1);
            ShowNextGrammarError();
        }

        private void buttonUseSuggestion_Click(object sender, EventArgs e)
        {
            if (listBoxSuggestions.SelectedIndex < 0)
                return;

            CorrectWord(listBoxSuggestions.Items[listBoxSuggestions.SelectedIndex].ToString().Trim(), _currentWord, -1);
            ShowNextGrammarError();
        }

        private void buttonUseSuggestionAlways_Click(object sender, EventArgs e)
        {
            if (listBoxSuggestions.SelectedIndex < 0)
                return;

            var newWord = listBoxSuggestions.Items[listBoxSuggestions.SelectedIndex].ToString().Trim();
            if (!_changeAllDictionary.ContainsKey(_currentWord))
                _changeAllDictionary.Add(_currentWord, newWord);
            CorrectWord(newWord, _currentWord, -1);
            ShowNextGrammarError();
        }

        private void MainForm_ResizeEnd(object sender, EventArgs e)
        {
            listView1.Columns[listView1.Columns.Count - 1].Width = -2;
        }
    }
}
