using System.Globalization;
using Nikse.SubtitleEdit.PluginLogic;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;
using System.Collections.Generic;
using SubtitleEdit.Translator;
using System.Reflection;
using System.Xml;

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
        private readonly Subtitle _subtitle;
        private readonly Subtitle _subtitleOriginal;
        private static string _from = "EN";
        private static string _to = "DE";
        private bool _abort;
        private bool _tooManyRequests;

        public string FixedSubtitle { get; private set; }

        public MainForm()
        {
            InitializeComponent();
            textBoxLog.Visible = false;
            listView1.Columns[2].Width = -2;
            buttonCancelTranslate.Enabled = false;
            comboBoxApiUrl.SelectedIndex = 0;
        }

        private static void SetLanguages(ComboBox comboBox, string language)
        {
            comboBox.Items.Clear();
            foreach (var pair in new DeepLTranslator2("", "").GetTranslationPairs())
            {
                comboBox.Items.Add(pair);
            }

            var i = 0;
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
            var languageCode = LanguageAutoDetect.AutoDetectGoogleLanguage(_subtitleOriginal);
            SetLanguages(comboBoxLanguageFrom, languageCode);
            SetLanguages(comboBoxLanguageTo, "DE");
            GeneratePreview(false);
            RestoreSettings();
        }

        public sealed override string Text
        {
            get => base.Text;
            set => base.Text = value;
        }

        private void Translate(string source, string target, DeepLTranslator2 translator, int maxTextSize, int maximumRequestArrayLength = 100)
        {
            buttonOk.Enabled = false;
            buttonCancel.Enabled = false;
            _abort = false;
            Cursor.Current = Cursors.WaitCursor;
            progressBar1.Maximum = _subtitleOriginal.Paragraphs.Count;
            progressBar1.Value = 0;
            progressBar1.Visible = true;
            try
            {
                var log = new StringBuilder();
                var selectedItems = listView1.SelectedItems;
                var startIndex = selectedItems.Count <= 0 ? 0 : selectedItems[0].Index;
                var index = startIndex;
                var skipIndices = new List<int>();
                var autoMergeAndSplit = checkBoxAutoMergeSplit.Checked;
                for (var i = startIndex; i < _subtitleOriginal.Paragraphs.Count; i++)
                {
                    if (skipIndices.Contains(i))
                    {
                        index++;
                        continue;
                    }

                    var p = new Paragraph(_subtitleOriginal.Paragraphs[i]);
                    var mergeCount = 0;
                    if (autoMergeAndSplit)
                    {
                        if (MergeWithThreeNext(_subtitleOriginal, i, source))
                        {
                            skipIndices.Add(i + 1);
                            skipIndices.Add(i + 2);
                            skipIndices.Add(i + 3);
                            mergeCount = 3;
                            p.Text = Utilities.RemoveLineBreaks(p.Text + Environment.NewLine +
                                                                _subtitleOriginal.Paragraphs[i + 1].Text + Environment.NewLine +
                                                                _subtitleOriginal.Paragraphs[i + 2].Text);
                        }
                        else if (MergeWithTwoNext(_subtitleOriginal, i, source))
                        {
                            skipIndices.Add(i + 1);
                            skipIndices.Add(i + 2);
                            mergeCount = 2;
                            p.Text = Utilities.RemoveLineBreaks(p.Text + Environment.NewLine +
                                                                _subtitleOriginal.Paragraphs[i + 1].Text + Environment.NewLine +
                                                                _subtitleOriginal.Paragraphs[i + 2].Text);
                        }
                        else if (MergeWithNext(_subtitleOriginal, i, source))
                        {
                            skipIndices.Add(i + 1);
                            mergeCount = 1;
                            p.Text = Utilities.RemoveLineBreaks(p.Text + Environment.NewLine + _subtitleOriginal.Paragraphs[i + 1].Text);
                        }
                    }

                    var translateResult = translator.Translate(source, target, new List<Paragraph> { p }, log);
                    var result = SplitResult(translateResult, mergeCount, source);
                    
                    textBoxLog.Text = log.ToString();
                    FillTranslatedText(result, index);
                    progressBar1.Refresh();
                    Application.DoEvents();

                    index++;
                    progressBar1.Value = index;
                    if (_abort)
                    {
                        break;
                    }
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

        private static List<string> SplitResult(List<string> result, int mergeCount, string language)
        {
            if (result.Count != 1)
            {
                return result;
            }

            if (mergeCount == 1)
            {
                var arr = Utilities.AutoBreakLine(result[0], 84, 1, language).SplitToLines();
                if (arr.Length == 1)
                {
                    arr = Utilities.AutoBreakLine(result[0], 42, 1, language).SplitToLines();
                }

                if (arr.Length == 1)
                {
                    arr = Utilities.AutoBreakLine(result[0], 22, 1, language).SplitToLines();
                }

                if (arr.Length == 2)
                {
                    return new List<string>
                    {
                        Utilities.AutoBreakLine(arr[0], 42, language == "zh" ? 0 : 25, language),
                        Utilities.AutoBreakLine(arr[1], 42, language == "zh" ? 0 : 25, language),
                    };
                }

                if (arr.Length == 1)
                {
                    return new List<string>
                    {
                        Utilities.AutoBreakLine(arr[0], 42, language == "zh" ? 0 : 25, language),
                        string.Empty,
                    };
                }

                return result;
            }

            if (mergeCount == 2)
            {
                var arr = SplitHelper.SplitToXLines(3, result[0], 84).ToArray();

                if (arr.Length == 3)
                {
                    return new List<string>
                    {
                        Utilities.AutoBreakLine(arr[0], 42, language == "zh" ? 0 : 25, language),
                        Utilities.AutoBreakLine(arr[1], 42, language == "zh" ? 0 : 25, language),
                        Utilities.AutoBreakLine(arr[2], 42, language == "zh" ? 0 : 25, language),
                    };
                }

                if (arr.Length == 2)
                {
                    return new List<string>
                    {
                        Utilities.AutoBreakLine(arr[0], 42, language == "zh" ? 0 : 25, language),
                        Utilities.AutoBreakLine(arr[1], 42, language == "zh" ? 0 : 25, language),
                        string.Empty,
                    };
                }

                if (arr.Length == 1)
                {
                    return new List<string>
                    {
                        Utilities.AutoBreakLine(arr[0], 42, language == "zh" ? 0 : 25, language),
                        string.Empty,
                        string.Empty,
                    };
                }

                return result;
            }

            if (mergeCount == 3)
            {
                var arr = SplitHelper.SplitToXLines(4, result[0], 84).ToArray();

                if (arr.Length == 4)
                {
                    return new List<string>
                    {
                        Utilities.AutoBreakLine(arr[0], 42, language == "zh" ? 0 : 25, language),
                        Utilities.AutoBreakLine(arr[1], 42, language == "zh" ? 0 : 25, language),
                        Utilities.AutoBreakLine(arr[2], 42, language == "zh" ? 0 : 25, language),
                        Utilities.AutoBreakLine(arr[3], 42, language == "zh" ? 0 : 25, language),
                    };
                }

                if (arr.Length == 3)
                {
                    return new List<string>
                    {
                        Utilities.AutoBreakLine(arr[0], 42, language == "zh" ? 0 : 25, language),
                        Utilities.AutoBreakLine(arr[1], 42, language == "zh" ? 0 : 25, language),
                        Utilities.AutoBreakLine(arr[2], 42, language == "zh" ? 0 : 25, language),
                        string.Empty,
                    };
                }

                if (arr.Length == 2)
                {
                    return new List<string>
                    {
                        Utilities.AutoBreakLine(arr[0], 42, language == "zh" ? 0 : 25, language),
                        Utilities.AutoBreakLine(arr[1], 42, language == "zh" ? 0 : 25, language),
                        string.Empty,
                        string.Empty,
                    };
                }

                if (arr.Length == 1)
                {
                    return new List<string>
                    {
                        Utilities.AutoBreakLine(arr[0], 42, language == "zh" ? 0 : 25, language),
                        string.Empty,
                        string.Empty,
                        string.Empty,
                    };
                }

                return result;
            }

            return result;
        }

        private static bool MergeWithNext(Subtitle subtitle, int i, string source)
        {
            if (i + 1 >= subtitle.Paragraphs.Count || source.ToLowerInvariant() == "zh" || source.ToLowerInvariant() == "ja")
            {
                return false;
            }

            var p = subtitle.Paragraphs[i];
            var text = Utilities.RemoveHtmlTags(p.Text, true).TrimEnd('"');
            if (text.EndsWith(".", StringComparison.Ordinal) ||
                text.EndsWith("!", StringComparison.Ordinal) ||
                text.EndsWith("?", StringComparison.Ordinal))
            {
                return false;
            }

            var next = subtitle.Paragraphs[i + 1];
            return next.StartTime.TotalMilliseconds - p.EndTime.TotalMilliseconds < 500;
        }

        private static bool MergeWithTwoNext(Subtitle subtitle, int i, string source)
        {
            if (i + 2 >= subtitle.Paragraphs.Count || source.ToLowerInvariant() == "zh" || source.ToLowerInvariant() == "ja")
            {
                return false;
            }

            return MergeWithNext(subtitle, i, source) && MergeWithNext(subtitle, i + 1, source);
        }

        private static bool MergeWithThreeNext(Subtitle subtitle, int i, string source)
        {
            if (i + 3 >= subtitle.Paragraphs.Count || source.ToLowerInvariant() == "zh" || source.ToLowerInvariant() == "ja")
            {
                return false;
            }

            return MergeWithNext(subtitle, i, source) && MergeWithNext(subtitle, i + 1, source) && MergeWithNext(subtitle, i + 2, source);
        }

        private void FillTranslatedText(List<string> translatedLines, int start)
        {
            var index = start;
            listView1.BeginUpdate();
            foreach (var s in translatedLines)
            {
                if (index < listView1.Items.Count)
                {
                    var item = listView1.Items[index];
                    var text = s;
                    if (checkBoxUnbreakLines.Checked)
                    {
                        text = Utilities.RemoveLineBreaks(s);
                    }
                    _subtitle.Paragraphs[index].Text = text;

                    item.SubItems[2].Text = text.Replace(Environment.NewLine, "<br />");
                    if (listView1.CanFocus)
                    {
                        listView1.EnsureVisible(index);
                    }
                }

                index++;
            }

            if (index > 0 && index < listView1.Items.Count - 1)
            {
                listView1.EnsureVisible(index - 1);
            }

            listView1.EndUpdate();
        }

        private void GeneratePreview(bool setText)
        {
            if (_subtitle == null)
            {
                return;
            }

            try
            {
                _abort = false;
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

                    var p = _subtitleOriginal.Paragraphs[index];
                    var text = p.Text;
                    var before = text;
                    var after = string.Empty;
                    AddToListView(p, before, after);
                    if (_abort)
                    {
                        _abort = false;
                        return;
                    }

                    if (_tooManyRequests)
                    {
                        _tooManyRequests = false;
                        MessageBox.Show("DeepL returned 'Too many requests' - please wait a while!");
                        return;
                    }
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
            Process.Start("https://www.deepl.com/");
        }

        private void buttonTranslate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBoxApiKey.Text))
            {
                MessageBox.Show("Please enter API key from DeepL.com");
                textBoxApiKey.Focus();
                return;
            }

            _tooManyRequests = false;
            _abort = false;
            buttonTranslate.Enabled = false;
            buttonCancelTranslate.Enabled = true;
            progressBar1.Maximum = _subtitle.Paragraphs.Count;
            progressBar1.Value = 0;
            progressBar1.Visible = true;
            try
            {
                _from = ((TranslationPair)comboBoxLanguageFrom.Items[comboBoxLanguageFrom.SelectedIndex]).Code;
                _to = ((TranslationPair)comboBoxLanguageTo.Items[comboBoxLanguageTo.SelectedIndex]).Code;
                Translate(_from, _to, new DeepLTranslator2(textBoxApiKey.Text, comboBoxApiUrl.Text), 1);
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
            else if (e.KeyCode == Keys.F1)
            {
                if (textBoxLog.Visible)
                {
                    textBoxLog.Visible = false;
                }
                else
                {
                    textBoxLog.Visible = true;
                    textBoxLog.BringToFront();
                }
            }
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://www.deepl.com/pro.html#pricing");
        }

        private static string GetSettingsFileName()
        {
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);
            if (path != null && path.StartsWith("file:\\", StringComparison.Ordinal))
            {
                path = path.Remove(0, 6);
            }

            path = Path.Combine(path, "Plugins");
            if (!Directory.Exists(path))
            {
                path = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Subtitle Edit"), "Plugins");
            }

            return Path.Combine(path, "DeepLProTranslator.xml");
        }

        private void RestoreSettings()
        {
            var fileName = GetSettingsFileName();
            try
            {
                var doc = new XmlDocument();
                doc.Load(fileName);
                textBoxApiKey.Text = DecodeFrom64(doc.DocumentElement.SelectSingleNode("ApiKey").InnerText);
                comboBoxApiUrl.Text = doc.DocumentElement.SelectSingleNode("ApiUrl").InnerText;
                _to = doc.DocumentElement.SelectSingleNode("Target").InnerText;
            }
            catch
            {
                // ignore
            }
        }

        private void SaveSettings()
        {
            var fileName = GetSettingsFileName();
            try
            {
                var doc = new XmlDocument();
                doc.LoadXml("<Translator><ApiKey/><Target/><ApiUrl/></Translator>");
                doc.DocumentElement.SelectSingleNode("ApiKey").InnerText = EncodeTo64(textBoxApiKey.Text.Trim());
                doc.DocumentElement.SelectSingleNode("Target").InnerText = _to;
                doc.DocumentElement.SelectSingleNode("ApiUrl").InnerText = comboBoxApiUrl.Text;
                doc.Save(fileName);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }

        private static string EncodeTo64(string toEncode)
        {
            var toEncodeAsBytes = Encoding.Unicode.GetBytes(toEncode);
            return Convert.ToBase64String(toEncodeAsBytes);
        }

        public static string DecodeFrom64(string encodedData)
        {
            var encodedDataAsBytes = Convert.FromBase64String(encodedData);
            return Encoding.Unicode.GetString(encodedDataAsBytes);
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveSettings();
        }
    }
}