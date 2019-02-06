using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public partial class Form1 : Form
    {
        private string _ocrReplaceListFileName;
        private readonly Dictionary<string, string> _namesList = new Dictionary<string, string>();
        private XDocument _ocrReplaceListXml;
        private bool _changed;
        private const string SplitString = " -> ";

        public Form1()
        {
            InitializeComponent();

            labelCount.Text = string.Empty;
            labelImportCount.Text = string.Empty;
            labelTransfered.Text = string.Empty;
            string defaultFileName = @"C:\Data\SubtitleEdit\subtitleedit\Dictionaries\eng_OCRFixReplaceList.xml";
            if (File.Exists(defaultFileName))
            {
                LoadOcrReplaceListFile(defaultFileName);
            }
        }

        private void LoadOcrReplaceListFile(string fileName)
        {
            textBoxFileName.Text = fileName;
            _ocrReplaceListFileName = fileName;
            _ocrReplaceListXml = XDocument.Load(fileName);
            if (_ocrReplaceListXml.Document != null)
            {
                var xElement = _ocrReplaceListXml.Document.Element("OCRFixReplaceList");
                if (xElement != null)
                {
                    var element = xElement.Element("WholeWords");
                    if (element != null)
                    {
                        foreach (var nameNode in element.Elements("Word"))
                        {
                            var xAttribute = nameNode.Attribute("from");
                            if (xAttribute != null)
                            {
                                string k = xAttribute.Value;
                                if (!_namesList.ContainsKey(k))
                                {
                                    var attribute = nameNode.Attribute("to");
                                    if (attribute != null)
                                    {
                                        string v = attribute.Value;
                                        _namesList.Add(k, v);
                                    }
                                }
                            }
                        }
                    }
                }
                FillListView();
            }
            _changed = false;
        }

        private void FillListView()
        {
            listBoxNames.Items.Clear();
            listBoxNames.BeginUpdate();
            foreach (var name in _namesList)
            {
                listBoxNames.Items.Add(name.Key + SplitString + name.Value);
            }
            listBoxNames.EndUpdate();
            labelCount.Text = $"{_namesList.Count:###,##0}";
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            string name = textBoxNewWord.Text.Trim();
            if (name.Length < 1 || _namesList == null || _ocrReplaceListXml?.Document == null || _ocrReplaceListFileName == null)
            {
                return;
            }

            if (_namesList.ContainsKey(name))
            {
                MessageBox.Show($"The name {name} already exists in the names list");
                return;
            }

            _namesList.Add(name, name); //TODO: fix
            FillListView();
            textBoxNewWord.Text = string.Empty;
            textBoxNewWord.Focus();
            //TODO:            listBoxNames.SelectedIndex = _namesList. .(name);
            _changed = true;
        }

        private void SaveOcrReplaceList()
        {
            try
            {
                if (File.Exists(_ocrReplaceListFileName))
                {
                    File.Copy(_ocrReplaceListFileName, _ocrReplaceListFileName + ".bak", true);
                }
            }
            catch (Exception)
            {
                // ignore
            }

            try
            {
                var xElement = _ocrReplaceListXml.Document?.Element("OCRFixReplaceList");
                if (xElement != null)
                {
                    var root = xElement?.Element("WholeWords");
                    if (root != null)
                    {
                        root.Descendants("Word").Remove();
                        foreach (var pair in _namesList)
                        {
                            var e = new XElement("Word", new XAttribute("from", pair.Key),
                                                         new XAttribute("to", pair.Value));
                            root.Add(e);
                        }
                    }
                    _ocrReplaceListXml.Save(_ocrReplaceListFileName);
                    MessageBox.Show(_ocrReplaceListFileName + " saved!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void listBoxNames_KeyDown(object sender, KeyEventArgs e)
        {
            int index = listBoxNames.SelectedIndex;
            if (index < 0)
            {
                return;
            }

            string name = listBoxNames.Items[index].ToString();
            if (e.KeyCode == Keys.Delete)
            {
                DeleteOcrPair(name);
            }
        }

        private void DeleteOcrPair(string name)
        {
            if (MessageBox.Show(this, "Delete '" + name + "' ?", string.Empty, MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                var arr = name.Replace(SplitString, "\n").Split('\n');
                if (arr.Length == 2)
                    _namesList.Remove(arr[0]);
                FillListView();
                _changed = true;
            }
        }

        private void listBoxNames_DoubleClick(object sender, EventArgs e)
        {
            var lb = (sender as ListBox);
            if (lb == null)
            {
                return;
            }
            int index = lb.SelectedIndex;
            if (index < 0)
            {
                return;
            }

            string name = lb.Items[index].ToString();
            Process.Start("https://www.google.com/search?q=" + Uri.EscapeUriString(name));
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            SaveOcrReplaceList();
            _changed = false;
        }

        private void buttonBrowse_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Names etc files|*names.xml";
            openFileDialog1.FileName = string.Empty;
            if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                LoadOcrReplaceListFile(openFileDialog1.FileName);
            }
        }

        private void buttonImport_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "OCR Fix List files|*_OCRFixReplaceList_User.xml|Xml files|*.xml";
            openFileDialog1.FileName = string.Empty;
            if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                listBoxImport.Items.Clear();
                int countSkippedExisting = 0;
                int countSkippedOneLetter = 0;
                var importedList = new SortedDictionary<string, string>();
                if (openFileDialog1.FileName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                {
                    var xdoc = XDocument.Load(openFileDialog1.FileName);
                    if (xdoc.Document != null)
                    {
                        var xElement = xdoc.Document.XPathSelectElement("ReplaceList");
                        if (xElement == null)
                        {
                            xElement = xdoc.Document.XPathSelectElement("OCRFixReplaceList");                            
                        }
                        if (xElement != null)
                        {
                            var element = xElement.Element("WholeWords");
                            if (element != null)
                            {
                                foreach (var wordNode in element.Elements("Word"))
                                {
                                    var xAttribute = wordNode.Attribute("from");
                                    if (xAttribute != null)
                                    {
                                        string s = xAttribute.Value;
                                        while (s.Contains("  "))
                                            s = s.Replace("  ", " ");
                                        if (_namesList.ContainsKey(s))
                                        {
                                            countSkippedExisting++;
                                        }
                                        else if (string.IsNullOrWhiteSpace(s) || s.Length < 2)
                                        {
                                            countSkippedOneLetter++;
                                        }
                                        else
                                        {
                                            if (!importedList.ContainsKey(s))
                                            {
                                                var attribute = wordNode.Attribute("to");
                                                if (attribute != null)
                                                    importedList.Add(s, attribute.Value);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                listBoxImport.BeginUpdate();
                foreach (var s in importedList)
                {
                    listBoxImport.Items.Add(s.Key + SplitString + s.Value);
                }
                listBoxImport.EndUpdate();

                labelImportCount.Text = $"{listBoxImport.Items.Count:###,##0}";
                MessageBox.Show("Words found: " + listBoxImport.Items.Count + Environment.NewLine +
                                "Existing words skipped: " + countSkippedExisting + Environment.NewLine +
                                "One letter word skipped: " + countSkippedOneLetter);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (_namesList == null || _ocrReplaceListXml?.Document == null || _ocrReplaceListFileName == null)
            {
                return;
            }

            int numberOfWordsAdded = 0;
            foreach (int index in listBoxImport.SelectedIndices)
            {
                string name = listBoxImport.Items[index].ToString();
                var arr = name.Replace(SplitString, "\n").Split('\n');
                if (arr.Length == 2)
                {
                    if (!_namesList.ContainsKey(arr[0]))
                    {
                        _namesList.Add(arr[0], arr[1]);
                        numberOfWordsAdded++;
                        _changed = true;
                    }
                }
            }
            FillListView();
            labelTransfered.Text = "Replace pairs added: " + numberOfWordsAdded;
            timer1.Start();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            var result = DialogResult.No;
            if (_changed)
            {
                result = MessageBox.Show("Do you want to save changes?", string.Empty, MessageBoxButtons.YesNoCancel);
            }
            if (result == DialogResult.Cancel)
            {
                e.Cancel = true;
                return;
            }
            if (result == DialogResult.Yes)
            {
                SaveOcrReplaceList();
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            labelTransfered.Text = string.Empty;
            timer1.Stop();
        }

        private void contextMenuStrip1_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            deleteCurrentNameToolStripMenuItem.Enabled = listBoxNames.SelectedIndex >= 0;
        }

        private void deleteCurrentNameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int index = listBoxNames.SelectedIndex;
            if (index < 0)
            {
                return;
            }

            string name = listBoxNames.Items[index].ToString();
            DeleteOcrPair(name);
        }

        private void contextMenuStripImport_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            toolStripMenuItemRemoveImport.Enabled = listBoxImport.SelectedIndex >= 0;
            var name = listBoxImport.Items[listBoxImport.SelectedIndex].ToString().Replace(SplitString, "\n").Split('\n').First();
            googleItToolStripMenuItem.Text = "Google '" + name + "'";
        }

        private void toolStripMenuItemRemoveImport_Click(object sender, EventArgs e)
        {
            int index = listBoxImport.SelectedIndex;
            if (index < 0)
            {
                return;
            }

            var indices = new List<int>();
            foreach (int ix in listBoxImport.SelectedIndices)
            {
                indices.Add(ix);
            }
            foreach (var ix in indices.OrderByDescending(p=>p))
            {
                listBoxImport.Items.RemoveAt(ix);
            }

            if (index < listBoxImport.Items.Count)
            {
                listBoxImport.SelectedIndex = index;
            }
            else if (index > 0)
            {
                listBoxImport.SelectedIndex = index - 1;
            }
        }

        private void listBoxImport_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                toolStripMenuItemRemoveImport_Click(sender, e);
            }
        }

        private void googleItToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int index = listBoxImport.SelectedIndex;
            if (index < 0)
            {
                return;
            }
            var name = listBoxImport.Items[listBoxImport.SelectedIndex].ToString().Replace(SplitString, "\n").Split('\n').First();
            Process.Start("https://www.google.com/search?q=" + Uri.EscapeUriString(name));
        }
    }
}
