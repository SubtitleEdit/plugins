using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public partial class ManageWordsForm : Form
    {
        // readonly for built-in list
        private Stream _localFileStream;

        public ManageWordsForm()
        {
            InitializeComponent();
        }

        public void Initialize(Stream stream, string source)
        {
            labelSource.Text = $"Source: {source}";
            if (!source.StartsWith("Embadded", StringComparison.Ordinal))
            {
                DisableEditFunctionalityIfIsEmbaddedStream(false);
                _localFileStream = stream;
            }
            else
            {
                DisableEditFunctionalityIfIsEmbaddedStream(true);
            }

            AddStreamToListView(stream);
        }

        private void AddToListView(string americanWord, string britishWord)
        {
            var item = new ListViewItem(americanWord);
            item.SubItems.Add(britishWord);
            listView1.Items.Add(item);
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            // validate both words
            if (string.IsNullOrWhiteSpace(textBoxAmerican.Text) || string.IsNullOrWhiteSpace(textBoxBritish.Text) || (textBoxAmerican.Text == textBoxBritish.Text))
                return;

            //var xr = XmlReader.Create(_localFileStream);
            // store added word in local words-list

            _localFileStream.Seek(0, SeekOrigin.Begin);
            var xdoc = XDocument.Load(_localFileStream);//XmlReader.Create(_localFileStream));
            if (xdoc?.Root?.Name == "Words")
            {
                xdoc.Root.Add(new XElement("Word", new XAttribute("us", textBoxAmerican.Text), new XAttribute("br", textBoxBritish.Text)));

                _localFileStream.Position = 0;
                xdoc.Save(_localFileStream);

                textBoxAmerican.Text = string.Empty;
                textBoxBritish.Text = string.Empty;

                // reload listview
                AddStreamToListView(_localFileStream);
                MessageBox.Show($"Added: American: {textBoxAmerican.Text}; British: {textBoxBritish.Text}");
            }
        }

        private void DisableEditFunctionalityIfIsEmbaddedStream(bool disable)
        {
            textBoxAmerican.Enabled = !disable;
            textBoxBritish.Enabled = !disable;
            buttonAdd.Enabled = !disable;
        }

        private void AddStreamToListView(Stream stream)
        {
            if (stream == null)
                return;

            var totalWords = 0;
            stream.Seek(0, SeekOrigin.Begin);
            var xdoc = XDocument.Load(stream);
            listView1.BeginUpdate();
            listView1.Items.Clear();
            if (xdoc?.Root?.Name == "Words")
            {
                foreach (var item in xdoc.Root.Elements("Word"))
                {
                    if (item.Attribute("us")?.Value.Length > 1 && item.Attribute("br")?.Value.Length > 1)
                    {
                        AddToListView(item.Attribute("us")?.Value, item.Attribute("br")?.Value);
                        totalWords++;
                    }
                }
            }
            listView1.EndUpdate();
            labelTotalWords.Text = $"Total words: {totalWords}";
        }
    }
}
