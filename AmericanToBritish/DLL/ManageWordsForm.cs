using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public partial class ManageWordsForm : Form
    {
        // readonly for built-in list
        private XDocument _xdoc;
        private string _path;

        public ManageWordsForm()
        {
            InitializeComponent();
        }

        public void Initialize(string path)
        {
            labelSource.Text = "Source: Local-List";
            _path = path;
            _xdoc = XDocument.Load(path);
            GeneratePreview();
        }

        public void Initialize(Stream stream)
        {
            labelSource.Text = "Source: Embedded-List";
            textBoxAmerican.Enabled = false;
            textBoxBritish.Enabled = false;
            buttonAdd.Enabled = false;
            _xdoc = XDocument.Load(stream);
            GeneratePreview();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(_path))
                _xdoc.Save(_path);
            DialogResult = DialogResult.OK;
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            var americanWord = textBoxAmerican.Text.Trim().ToLowerInvariant();
            var britishWord = textBoxBritish.Text.Trim().ToLowerInvariant();

            if (americanWord.Length > 0)
            {
                foreach (ListViewItem item in listView1.Items)
                {
                    if (americanWord == item.Text.ToLowerInvariant())
                    {
                        listView1.SelectedItems.Clear();
                        item.EnsureVisible();
                        item.Selected = true;
                        item.Focused = true;
                        listView1.Select();
                        return;
                    }
                }
                if (britishWord.Length == 0 || americanWord == britishWord)
                {
                    textBoxBritish.Select();
                    return;
                }
                if (_xdoc?.Root?.Name == "Words")
                {
                    _xdoc.Root.Add(new XElement("Word", new XAttribute("us", americanWord), new XAttribute("br", britishWord)));
                    textBoxAmerican.Text = string.Empty;
                    textBoxBritish.Text = string.Empty;
                    GeneratePreview();
                    MessageBox.Show($"Added: American: {americanWord}; British: {britishWord}");
                }
            }
            textBoxAmerican.Select();
        }

        private void GeneratePreview()
        {
            listView1.BeginUpdate();
            listView1.Items.Clear();
            if (_xdoc?.Root?.Name == "Words")
            {
                foreach (var item in _xdoc.Root.Elements("Word"))
                {
                    if (item.Attribute("us")?.Value.Length > 0 && item.Attribute("br")?.Value.Length > 0)
                    {
                        AddToListView(item.Attribute("us").Value, item.Attribute("br").Value, item);
                    }
                }
            }
            listView1.EndUpdate();
            labelTotalWords.Text = $"Total words: {listView1.Items.Count}";
        }

        private void AddToListView(string americanWord, string britishWord, XElement elem)
        {
            var item = new ListViewItem(americanWord) { Tag = elem };
            item.SubItems.Add(britishWord);
            listView1.Items.Add(item);
        }

        private void toolStripMenuItemRemoveSelected_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                foreach (ListViewItem item in listView1.SelectedItems)
                {
                    (item.Tag as XElement).Remove();
                }
                GeneratePreview();
            }
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            if (_path == null)
                e.Cancel = true;
        }

    }
}
