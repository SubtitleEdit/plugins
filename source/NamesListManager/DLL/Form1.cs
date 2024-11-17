using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public partial class Form1 : Form
    {
        private string _namesFileName;
        private List<string> _namesList = new List<string>();
        private XDocument _namesXml;
        private bool _changed;

        public Form1()
        {
            InitializeComponent();

            labelCount.Text = string.Empty;
            labelImportCount.Text = string.Empty;
            labelTransfered.Text = string.Empty;
            string defaultFileName = @"C:\git\SubtitleEdit\Dictionaries\names.xml";
            if (File.Exists(defaultFileName))
            {
                LoadNamesFile(defaultFileName);
            }
        }

        private void LoadNamesFile(string fileName)
        {
            textBoxFileName.Text = fileName;
            _namesFileName = fileName;
            _namesXml = XDocument.Load(fileName);
            if (_namesXml.Document != null)
            {
                foreach (var nameNode in _namesXml.Document.Descendants("name"))
                {
                    var s = nameNode.Value;
                    if (nameNode.Parent?.Name == "blacklist")
                    {
                        // skip
                    }
                    else if (!_namesList.Contains(s))
                    {
                        _namesList.Add(s);
                    }
                }

                FillNames();
            }

            _changed = false;
        }

        private void FillNames()
        {
            _namesList.Sort();
            listBoxNames.Items.Clear();
            listBoxNames.BeginUpdate();
            foreach (var name in _namesList)
            {
                listBoxNames.Items.Add(name);
            }
            listBoxNames.EndUpdate();
            labelCount.Text = $"{_namesList.Count:###,##0}";
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            string name = textBoxNewWord.Text.Trim();
            if (name.Length < 1 || _namesList == null || _namesXml?.Document == null || _namesFileName == null)
            {
                return;
            }

            if (_namesList.Contains(name))
            {
                MessageBox.Show($"The name {name} already exists in the names list");
                return;
            }

            _namesList.Add(name);
            FillNames();
            textBoxNewWord.Text = string.Empty;
            textBoxNewWord.Focus();
            listBoxNames.SelectedIndex = _namesList.IndexOf(name);
            _changed = true;
        }

        private void SaveNames()
        {
            try
            {
                if (File.Exists(_namesFileName))
                {
                    File.Copy(_namesFileName, _namesFileName + ".bak", true);
                }
            }
            catch (Exception)
            {
                // ignore
            }

            try
            {
                _namesXml.Document?.Root.Elements("name").Remove();
                // will throw an exception if no element satisfy the predicate/empty
                var root = _namesXml.Descendants().First(p => p.NodeType == XmlNodeType.Element);
                foreach (var name in _namesList)
                {
                    root.Add(new XElement("name", name));
                }
                _namesXml.Save(_namesFileName);
                MessageBox.Show(_namesFileName + " saved!");
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
                DeleteName(name);
            }
        }

        private void DeleteName(string name)
        {
            if (MessageBox.Show(this, "Delete '" + name + "' ?", string.Empty, MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                _namesList.Remove(name);
                FillNames();
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
            SaveNames();
            _changed = false;
        }

        private void buttonBrowse_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Names etc files|*names*.xml";
            openFileDialog1.FileName = string.Empty;
            if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                _namesList.Clear();
                LoadNamesFile(openFileDialog1.FileName);
            }
        }

        private void buttonImport_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Names files|*names*.xml|Text files|*.txt";
            openFileDialog1.FileName = string.Empty;
            if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                listBoxImport.Items.Clear();
                int countSkippedExisting = 0;
                int countSkippedOneLetter = 0;
                List<string> importedList = new List<string>();
                if (openFileDialog1.FileName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                {
                    var xdoc = XDocument.Load(openFileDialog1.FileName);
                    if (xdoc.Document != null)
                    {
                        foreach (var nameNode in xdoc.Document.Descendants("name"))
                        {
                            string s = nameNode.Value;
                            while (s.Contains("  "))
                                s = s.Replace("  ", " ");
                            if (_namesList.Contains(s))
                            {
                                countSkippedExisting++;
                            }
                            else if (s.Length < 2)
                            {
                                countSkippedOneLetter++;
                            }
                            else
                            {
                                if (!importedList.Contains(s))
                                {
                                    importedList.Add(s);
                                }
                            }
                        }
                    }
                }
                else
                {
                    foreach (var line in File.ReadAllLines(openFileDialog1.FileName))
                    {
                        string s = line.Trim();
                        while (s.Contains("  "))
                            s = s.Replace("  ", " ");
                        if (_namesList.Contains(s))
                        {
                            countSkippedExisting++;
                        }
                        else if (s.Length < 2)
                        {
                            countSkippedOneLetter++;
                        }
                        else
                        {
                            if (!importedList.Contains(s) && !Utilities.IsInteger(s))
                            {
                                importedList.Add(s);
                            }
                        }
                    }
                }
                importedList.Sort();
                listBoxImport.BeginUpdate();
                foreach (var s in importedList)
                {
                    listBoxImport.Items.Add(s);
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
            if (_namesList == null || _namesXml?.Document == null || _namesFileName == null)
            {
                return;
            }

            int numberOfWordsAdded = 0;
            foreach (int index in listBoxImport.SelectedIndices)
            {
                string name = listBoxImport.Items[index].ToString();
                if (!_namesList.Contains(name))
                {
                    _namesList.Add(name);
                    numberOfWordsAdded++;
                    _changed = true;
                }
            }
            FillNames();
            labelTransfered.Text = "Words added: " + numberOfWordsAdded;
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
                SaveNames();
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            labelTransfered.Text = string.Empty;
            timer1.Stop();
        }

        private void removeNamesContainedInFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Names etc files|*names*.xml";
            openFileDialog1.FileName = string.Empty;
            if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                int countExisting = 0;
                List<string> namesToDelete = new List<string>();
                var xdoc = XDocument.Load(openFileDialog1.FileName);
                if (xdoc.Document != null)
                {
                    foreach (var nameNode in xdoc.Document.Descendants("name"))
                    {
                        string s = nameNode.Value;
                        while (s.Contains("  "))
                            s = s.Replace("  ", " ");
                        if (_namesList.Contains(s))
                        {
                            countExisting++;
                            namesToDelete.Add(s);
                        }                       
                    }
                    if (countExisting == 0)
                    {
                        MessageBox.Show("Nothing to delete");
                        return;
                    }
                    var result = MessageBox.Show(string.Format("Delete {0} names?", countExisting), null, MessageBoxButtons.YesNoCancel);
                    if (result != DialogResult.Yes)
                    {
                        return;
                    }
                    foreach (var s in namesToDelete)
                    {
                        _namesList.Remove(s);
                    }
                    FillNames();
                }
            }
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
            DeleteName(name);
        }

    }
}
