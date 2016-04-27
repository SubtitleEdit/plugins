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
            // validate both words
            if (string.IsNullOrWhiteSpace(textBoxAmerican.Text) || string.IsNullOrWhiteSpace(textBoxBritish.Text) || (textBoxAmerican.Text == textBoxBritish.Text))
                return;

            if (_xdoc?.Root?.Name == "Words")
            {
                _xdoc.Root.Add(new XElement("Word", new XAttribute("us", textBoxAmerican.Text), new XAttribute("br", textBoxBritish.Text)));
                textBoxAmerican.Text = string.Empty;
                textBoxBritish.Text = string.Empty;
                // reload listview
                GeneratePreview();
                MessageBox.Show($"Added: American: {textBoxAmerican.Text}; British: {textBoxBritish.Text}");
            }
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
