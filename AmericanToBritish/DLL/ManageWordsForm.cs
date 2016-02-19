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

        public ManageWordsForm()
        {
            InitializeComponent();
        }

        public void Initialize(Stream stream, string source)
        {
            labelSource.Text = $"Source: {source}";
            DisableEditFunctionalityIfIsEmbaddedStream(source.StartsWith("Embadded", StringComparison.Ordinal));
            var totalWords = 0;
            var xdoc = XDocument.Load(stream);
            if (xdoc.Root?.Name == "Words")
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
            labelTotalWords.Text = $"Total words: {totalWords}";
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
            // add words to listview
            // store added word in local words-list
        }

        private void DisableEditFunctionalityIfIsEmbaddedStream(bool disable)
        {
            textBoxAmerican.Enabled = disable;
            textBoxBritish.Enabled = disable;
            buttonAdd.Enabled = disable;
        }
    }
}
