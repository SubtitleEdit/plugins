using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using Microsoft.Toolkit.Forms.UI.Controls;
using WebViewTranslate.Logic;
using WebViewTranslate.Translator;

namespace WebViewTranslate
{
    /// <summary>
    /// 
    /// </summary>
    public partial class MainForm : Form
    {
        private readonly Subtitle _subtitle;
        private readonly Subtitle _subtitleOriginal;
        private static string _from = "en";
        private static string _to;
        private bool _abort;
        private WebView _webView;

        public string FixedSubtitle { get; private set; }

        public MainForm()
        {
            InitializeComponent();
            textBoxLog.Visible = false;
            buttonCancelTranslate.Enabled = false;
            comboBoxGoogleTranslateUrl.SelectedIndex = 0;
        }

        private void SetLanguages(ComboBox comboBox, string language)
        {
            comboBox.Items.Clear();
            foreach (var pair in GoogleScreenScraper2.GetTranslationPairs())
            {
                comboBox.Items.Add(pair);
            }
            int i = 0;
            foreach (var l in comboBox.Items)
            {
                if (l is TranslationPair tl && tl.Code.Equals(language, StringComparison.OrdinalIgnoreCase))
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
            _from = LanguageAutoDetect.AutoDetectGoogleLanguage(_subtitleOriginal);
            SetLanguages(comboBoxLanguageFrom, _from);
            GeneratePreview();
            RestoreSettings();
            if (string.IsNullOrEmpty(_to) || _to == _from)
            {
                _to = Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName;
                if (_to == _from)
                {
                    foreach (InputLanguage language in InputLanguage.InstalledInputLanguages)
                    {
                        _to = language.Culture.TwoLetterISOLanguageName;
                        if (_to != _from)
                            break;
                    }
                }
            }
            if (_to == _from && _from == "en")
            {
                _to = "es";
            }
            if(_to == _from && _from == "es")
            {
                _to = "en";
            }
            SetLanguages(comboBoxLanguageTo, _to);
        }

        public sealed override string Text
        {
            get => base.Text;
            set => base.Text = value;
        }

        private void Translate(string source, string target, GoogleScreenScraper2 translator, int maxTextSize, int maximumRequestArrayLength = 100)
        {
            buttonOk.Enabled = false;
            buttonCancel.Enabled = false;
            _abort = false;
            Cursor.Current = Cursors.WaitCursor;
            progressBar1.Maximum = _subtitleOriginal.Paragraphs.Count;
            progressBar1.Value = 0;
            progressBar1.Visible = true;
            var sourceParagraphs = new List<Paragraph>();
            try
            {
                var log = new StringBuilder();
                var sourceLength = 0;
                var selectedItems = listView1.SelectedItems;
                var startIndex = selectedItems.Count <= 0 ? 0 : selectedItems[0].Index;
                var start = startIndex;
                int index = startIndex;
                for (int i = startIndex; i < _subtitleOriginal.Paragraphs.Count; i++)
                {
                    var p = _subtitleOriginal.Paragraphs[i];
                    sourceLength += Uri.EscapeDataString(p.Text).Length;
                    if ((sourceLength >= maxTextSize || sourceParagraphs.Count >= maximumRequestArrayLength) && sourceParagraphs.Count > 0)
                    {
                        translator.Translate(source, target, sourceParagraphs, log);
                        List<string> result = null;
                        var before = DateTime.UtcNow;
                        int delay = 50;
                        while (result == null)
                        {
                            result = translator.GetTranslationResult(target, sourceParagraphs);
                            Thread.Sleep(10);
                            Application.DoEvents();
                            var seconds = (DateTime.UtcNow - before).TotalSeconds;
                            if (seconds > 15 || _abort)
                            {
                                log.AppendLine("No response from webview!" + Environment.NewLine);
                                result = new List<string>();
                            }
                            else if (result == null)
                            {
                                Application.DoEvents();
                                Thread.Sleep(delay);
                                delay += 50;
                                Application.DoEvents();
                            }
                        }

                        textBoxLog.Text = log.ToString().Trim();
                        if (log.Length > 1000000)
                            log.Clear();
                        FillTranslatedText(result, start, index - 1);
                        sourceLength = 0;
                        sourceParagraphs.Clear();
                        progressBar1.Refresh();
                        Application.DoEvents();
                        start = index;
                    }
                    sourceParagraphs.Add(p);
                    index++;
                    progressBar1.Value = index;
                    if (_abort)
                        break;
                }

                if (sourceParagraphs.Count > 0)
                {
                    translator.Translate(source, target, sourceParagraphs, log);
                    List<string> result = null;
                    var T = DateTime.UtcNow;
                    while (result == null)
                    {
                        result = translator.GetTranslationResult(target, sourceParagraphs);
                        if (translator.AcceptCookiePage)
                        {
                            translator.AcceptCookiePage = false;
                            textBoxLog.Visible = false;
                            _webView.Visible = true;
                            listView1.Visible = false;
                            MessageBox.Show("Please accept cookies (you might need to scroll down)");
                            return;
                        }
                        Application.DoEvents();
                        var seconds = (DateTime.UtcNow - T).TotalSeconds;
                        if (seconds > 15 && (result == null || result.Count < sourceParagraphs.Count) || _abort)
                        {
                            log.AppendLine("No response from webview!" + Environment.NewLine);
                            result = new List<string>();
                        }
                    }
                    textBoxLog.Text = log.ToString().Trim();
                    FillTranslatedText(result, start, index - 1);
                }
            }
            catch (WebException webException)
            {
                MessageBox.Show(webException.Source + ": " + webException.Message);
            }
            finally
            {
                progressBar1.Visible = false;
                Cursor.Current = Cursors.Default;
                buttonTranslate.Enabled = true;
                buttonOk.Enabled = true;
                buttonCancel.Enabled = true;
            }
        }

        private void FillTranslatedText(List<string> translatedLines, int start, int end)
        {
            int index = start;
            listView1.BeginUpdate();
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                item.Selected = false;
            }
            foreach (string s in translatedLines)
            {
                if (index < listView1.Items.Count)
                {
                    var item = listView1.Items[index];
                    _subtitle.Paragraphs[index].Text = s;
                    item.SubItems[2].Text = s.Replace(Environment.NewLine, "<br />");
                    if (listView1.CanFocus)
                        listView1.EnsureVisible(index);
                }
                index++;
            }

            if (index > 0 && index < listView1.Items.Count + 1)
            {
                listView1.EnsureVisible(index - 1);
                listView1.Items[index - 1].Selected = true;
            }

            listView1.EndUpdate();
        }

        private void GeneratePreview()
        {
            if (_subtitle == null)
                return;

            try
            {
                _abort = false;
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
                    string text = p.Text;
                    var before = text;
                    var after = string.Empty;
                    AddToListView(p, before, after);
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message + Environment.NewLine + exception.StackTrace);
            }
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
            Process.Start("https://translate.google.com");
        }

        private void buttonTranslate_Click(object sender, EventArgs e)
        {
            textBoxLog.Visible = false;
            if (_webView != null) _webView.Visible = false;
            listView1.Visible = true;

            _abort = false;
            buttonTranslate.Enabled = false;
            buttonCancelTranslate.Enabled = true;
            progressBar1.Maximum = _subtitle.Paragraphs.Count;
            progressBar1.Value = 0;
            progressBar1.Visible = true;
            if (_webView == null)
            {
                _webView = new WebView();
                ((ISupportInitialize)_webView).BeginInit();
                _webView.Dock = DockStyle.None;
                Controls.Add(_webView);
                _webView.IsJavaScriptEnabled = true;
                _webView.IsScriptNotifyAllowed = true;
                _webView.IsPrivateNetworkClientServerCapabilityEnabled = true;
                _webView.IsIndexedDBEnabled = true;
                ((ISupportInitialize)_webView).EndInit();
                _webView.Width = listView1.Width;
                _webView.Height = listView1.Height;
                _webView.Left = listView1.Left;
                _webView.Top = listView1.Top;
                _webView.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            }
            try
            {
                _from = ((TranslationPair)comboBoxLanguageFrom.Items[comboBoxLanguageFrom.SelectedIndex]).Code;
                _to = ((TranslationPair)comboBoxLanguageTo.Items[comboBoxLanguageTo.SelectedIndex]).Code;
                Translate(_from, _to, new GoogleScreenScraper2(_webView, comboBoxGoogleTranslateUrl.SelectedItem.ToString(), _from, _to), (int)numericUpDownMaxBytes.Value);
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

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
            else if (e.KeyCode == Keys.F2)
            {
                if (textBoxLog.Visible)
                {
                    if (_webView != null)
                    {
                        textBoxLog.Visible = false;
                        _webView.Visible = true;
                        listView1.Visible = false;
                    }
                    else
                    {
                        textBoxLog.Visible = false;
                        listView1.Visible = true;
                    }
                }
                else if (listView1.Visible)
                {
                    textBoxLog.Visible = true;
                    if (_webView != null) _webView.Visible = false;
                    listView1.Visible = false;
                }
                else 
                {
                    textBoxLog.Visible = false;
                    if (_webView != null) _webView.Visible = false;
                    listView1.Visible = true;
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
            return Path.Combine(path, "GoogleScreenScraperTranslate.xml");
        }

        private void RestoreSettings()
        {
            string fileName = GetSettingsFileName();
            try
            {
                var doc = new XmlDocument();
                doc.Load(fileName);
                _to = doc.DocumentElement.SelectSingleNode("Target").InnerText;
                comboBoxGoogleTranslateUrl.SelectedIndex = int.Parse(doc.DocumentElement.SelectSingleNode("UrlIndex").InnerText);
                numericUpDownMaxBytes.Value = decimal.Parse(doc.DocumentElement.SelectSingleNode("BulkSize").InnerText);
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
                doc.LoadXml("<Translator><Target/><UrlIndex/><BulkSize/></Translator>");
                doc.DocumentElement.SelectSingleNode("Target").InnerText = _to;
                doc.DocumentElement.SelectSingleNode("UrlIndex").InnerText = comboBoxGoogleTranslateUrl.SelectedIndex.ToString();
                doc.DocumentElement.SelectSingleNode("BulkSize").InnerText = ((int)numericUpDownMaxBytes.Value).ToString();
                doc.Save(fileName);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            var subtract = listView1.Columns[0].Width + 20;
            var width = listView1.Width / 2 - subtract;
            listView1.Columns[1].Width = width;
            listView1.Columns[1].Width = width;
            listView1.Columns[2].Width = width;
            listView1.Columns[1].Width = width;
            listView1.Columns[1].Width = width;
        }

        private void MainForm_ResizeEnd(object sender, EventArgs e)
        {
            MainForm_Resize(sender, e);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            MainForm_Resize(sender, e);
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            MainForm_Resize(sender, e);
        }
    }
}