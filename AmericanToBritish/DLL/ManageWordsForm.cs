using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public partial class ManageWordsForm : Form
    {
        private class WordPair : ListViewItem, IComparer
        {
            private static readonly Dictionary<string, LinkedList<WordPair>> _pairsByUs = new Dictionary<string, LinkedList<WordPair>>();
            private static uint _sortIndexCounter; // to enable stable sort
            private static readonly Color _badBackColor = Color.FromArgb(255, 236, 236);
            private static Color _okBackColor;

            private readonly LinkedListNode<WordPair> _pairNode;
            private readonly uint _sortIndex;
            private readonly string _usWord;
            private readonly string _gbWord;
            private readonly XElement _node;
            private readonly bool _isOk;

            public WordPair(string americanWord, string britishWord, XElement documentNode)
                : base(americanWord)
            {
                _usWord = americanWord.Trim().ToLowerInvariant();
                _gbWord = britishWord.Trim().ToLowerInvariant();
                _sortIndex = _sortIndexCounter++;
                _node = documentNode;
                _isOk = _usWord.Length > 1 && _gbWord.Length > 1 && _usWord != _gbWord && _usWord == americanWord && _gbWord == britishWord;

                LinkedList<WordPair> list;
                if (_pairsByUs.ContainsKey(_usWord))
                {
                    list = _pairsByUs[_usWord];
                }
                else
                {
                    list = new LinkedList<WordPair>();
                    _pairsByUs[_usWord] = list;
                }
                if (list.Count > 0 || !_isOk)
                {
                    if (list.Count == 1)
                        list.First.Value.BackColor = _badBackColor;
                    BackColor = _badBackColor;
                }
                _pairNode = list.AddLast(this);
                SubItems.Add(britishWord);
            }

            public WordPair()
            {
                _okBackColor = BackColor;
            }

            public override void Remove()
            {
                var list = _pairNode.List;
                list.Remove(_pairNode);
                if (list.Count == 0)
                    _pairsByUs.Remove(_usWord);
                else if (list.Count == 1 && list.First.Value._isOk)
                    list.First.Value.BackColor = _okBackColor;
                _node.Remove();
                base.Remove();
            }

            public int Compare(object o1, object o2)
            {
                var wp1 = o1 as WordPair;
                var wp2 = o2 as WordPair;
                int cmp = string.Compare(wp1._usWord, wp2._usWord, StringComparison.InvariantCulture);
                if (cmp == 0)
                {
                    cmp = string.Compare(wp1.Text, wp2.Text, StringComparison.InvariantCultureIgnoreCase);
                    if (cmp == 0)
                        cmp = wp1._sortIndex.CompareTo(wp2._sortIndex);
                }
                return cmp;
            }

        }

        // readonly for built-in list (_path == null)
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
            InitializeListView(XDocument.Load(path));
        }

        public void Initialize(Stream stream)
        {
            labelSource.Text = "Source: Embedded-List";
            textBoxAmerican.Enabled = false;
            textBoxBritish.Enabled = false;
            buttonAdd.Enabled = false;
            InitializeListView(XDocument.Load(stream));
        }

        private void InitializeListView(XDocument xdoc)
        {
            listView1.BeginUpdate();
            listView1.Items.Clear();
            if (xdoc?.Root?.Name == "Words")
            {
                foreach (var node in xdoc.Root.Elements("Word"))
                {
                    if (node.Attribute("us")?.Value.Length > 0 && node.Attribute("br")?.Value.Length > 0)
                    {
                        listView1.Items.Add(new WordPair(node.Attribute("us").Value, node.Attribute("br").Value, node));
                    }
                }
                listView1.ListViewItemSorter = new WordPair();
                _xdoc = xdoc;
            }
            listView1.EndUpdate();
            labelTotalWords.Text = $"Total words: {listView1.Items.Count}";
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (_path != null && _xdoc != null)
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
                if (_xdoc != null)
                {
                    var node = new XElement("Word", new XAttribute("us", americanWord), new XAttribute("br", britishWord));
                    listView1.Items.Add(new WordPair(americanWord, britishWord, node));
                    labelTotalWords.Text = $"Total words: {listView1.Items.Count}";
                    _xdoc.Root.Add(node);
                    textBoxAmerican.Text = string.Empty;
                    textBoxBritish.Text = string.Empty;
                    MessageBox.Show($"Added: American: {americanWord}; British: {britishWord}");
                }
            }
            textBoxAmerican.Select();
        }

        private void toolStripMenuItemRemoveSelected_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                listView1.BeginUpdate();
                var items = new ArrayList(listView1.SelectedItems);
                listView1.SelectedItems.Clear();
                foreach (var item in items)
                {
                    (item as WordPair).Remove();
                }
                listView1.EndUpdate();
                labelTotalWords.Text = $"Total words: {listView1.Items.Count}";
            }
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            if (_path == null)
                e.Cancel = true;
        }

    }
}
