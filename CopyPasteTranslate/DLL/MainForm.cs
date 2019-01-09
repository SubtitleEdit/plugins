using CopyPasteTranslate;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
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

        public string FixedSubtitle { get; private set; }

        public MainForm()
        {
            InitializeComponent();
            textBoxLog.Visible = false;
            buttonCancelTranslate.Enabled = false;
        }

        private void SetLanguages(ComboBox comboBox, string language)
        {
            comboBox.Items.Clear();
            foreach (var pair in CopyPasteTranslator.GetTranslationPairs())
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
            if (_to == _from && _from == "es")
            {
                _to = "en";
            }
            SetLanguages(comboBoxLanguageTo, _to);
            if (listView1.Items.Count > 0)
            {
                listView1.Items[0].Selected = true;
                listView1.Items[0].Focused = true;
            }
        }

        public sealed override string Text
        {
            get { return base.Text; }
            set { base.Text = value; }
        }

        private void Translate(string source, string target, CopyPasteTranslator translator, int maxTextSize)
        {
            buttonOk.Enabled = false;
            buttonCancel.Enabled = false;
            _abort = false;
            progressBar1.Maximum = _subtitleOriginal.Paragraphs.Count;
            progressBar1.Value = 0;
            progressBar1.Visible = true;
            
            try
            {
                var log = new StringBuilder();
                var selectedItems = listView1.SelectedItems;
                var startIndex = selectedItems.Count <= 0 ? 0 : selectedItems[0].Index;
                var start = startIndex;
                int index = startIndex;
                var blocks = translator.BuildBlocks(maxTextSize, source, startIndex);
                for (int i = 0; i < blocks.Count; i++)
                {
                    var block = blocks[i];
                    using (var form = new TranslateBlock(block, "Translate block " + (i + 1) + " of " + blocks.Count, checkBoxAutoCopyToClipboard.Checked))
                    {
                        if (form.ShowDialog(this) == DialogResult.OK)
                        {
                            var result = translator.GetTranslationResult(target, form.TargetText, block);
                            FillTranslatedText(result, start, index);
                            progressBar1.Refresh();
                            Application.DoEvents();
                            index += block.Paragraphs.Count;
                            start = index;
                            progressBar1.Value = Math.Min(index, progressBar1.Maximum);
                        }
                        else
                        {
                            _abort = true;
                        }
                    }
                    if (_abort)
                        break;
                }
            }
            finally
            {
                progressBar1.Visible = false;
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

        private void buttonTranslate_Click(object sender, EventArgs e)
        {
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
                var translator = new CopyPasteTranslator(_subtitleOriginal.Paragraphs, textBoxLineSeparator.Text);
                Translate(_from, _to, translator, (int)numericUpDownMaxBytes.Value);
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

        private string GetSettingsFileName()
        {
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);
            if (path != null && path.StartsWith("file:\\", StringComparison.Ordinal))
                path = path.Remove(0, 6);
            path = Path.Combine(path, "Plugins");
            if (!Directory.Exists(path))
                path = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Subtitle Edit"), "Plugins");
            return Path.Combine(path, "CopyPasteTranslate.xml");
        }

        private void RestoreSettings()
        {
            string fileName = GetSettingsFileName();
            try
            {
                var doc = new XmlDocument();
                doc.Load(fileName);
                _to = doc.DocumentElement.SelectSingleNode("Target").InnerText;
                textBoxLineSeparator.Text = doc.DocumentElement.SelectSingleNode("Separator").InnerText;
                numericUpDownMaxBytes.Value = decimal.Parse(doc.DocumentElement.SelectSingleNode("MaxSize").InnerText);
                checkBoxAutoCopyToClipboard.Checked = Convert.ToBoolean(doc.DocumentElement.SelectSingleNode("AutoCopyToClipboard").InnerText);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }

        private void SaveSettings()
        {
            string fileName = GetSettingsFileName();
            try
            {
                var doc = new XmlDocument();
                doc.LoadXml("<Translator><Target/><Separator/><MaxSize/><AutoCopyToClipboard/></Translator>");
                doc.DocumentElement.SelectSingleNode("Target").InnerText = _to;
                doc.DocumentElement.SelectSingleNode("Separator").InnerText = textBoxLineSeparator.Text;
                doc.DocumentElement.SelectSingleNode("MaxSize").InnerText = ((int)numericUpDownMaxBytes.Value).ToString();
                doc.DocumentElement.SelectSingleNode("AutoCopyToClipboard").InnerText = checkBoxAutoCopyToClipboard.Checked.ToString();
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