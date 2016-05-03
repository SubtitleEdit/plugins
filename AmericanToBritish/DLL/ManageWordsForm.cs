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
            private static readonly Dictionary<string, LinkedList<WordPair>> PairsByUs = new Dictionary<string, LinkedList<WordPair>>();
            private static uint _sortIndexCounter; // to emulate a stable sort
            private static readonly Color HighlightColor = Color.FromArgb(255, 236, 236);
            private static Color _defaultColor;

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
                if (!PairsByUs.TryGetValue(_usWord, out list))
                {
                    list = new LinkedList<WordPair>();
                    PairsByUs.Add(_usWord, list);
                }
                if (list.Count > 0 || !_isOk)
                {
                    if (list.Count == 1)
                        list.First.Value.BackColor = HighlightColor;
                    BackColor = HighlightColor;
                }
                _pairNode = list.AddLast(this);
                SubItems.Add(britishWord);
            }

            public WordPair()
            {
                _defaultColor = BackColor;
            }

            public static WordPair GetValueOrNull(string americanWord)
            {
                LinkedList<WordPair> wpl;
                return PairsByUs.TryGetValue(americanWord.Trim().ToLowerInvariant(), out wpl) ? wpl.First.Value : null;
            }

            public static void Shutdown()
            {
                _sortIndexCounter = 0;
                PairsByUs.Clear();
            }

            public bool IsHighlighted
            {
                get
                {
                    return BackColor.ToArgb().Equals(HighlightColor.ToArgb());
                }
            }

            public override void Remove()
            {
                var list = _pairNode.List;
                list.Remove(_pairNode);
                if (list.Count == 0)
                    PairsByUs.Remove(_usWord);
                else if (list.Count == 1 && list.First.Value._isOk)
                    list.First.Value.BackColor = _defaultColor;
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

        private const int Down = 1;
        private const int Up = -1;

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
            textBoxBritish.Enabled = false;
            buttonAdd.Text = "Find";
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
                var item = WordPair.GetValueOrNull(americanWord);
                if (item != null)
                {
                    listView1.SelectedItems.Clear();
                    item.EnsureVisible();
                    item.Selected = true;
                    item.Focused = true;
                    listView1.Select();
                    return;
                }
                if (britishWord.Length == 0 || americanWord == britishWord)
                {
                    if (textBoxBritish.Enabled)
                    {
                        textBoxBritish.Select();
                        return;
                    }
                    textBoxAmerican.SelectAll();
                }
                else if (americanWord.Length > 1 && britishWord.Length > 1 && _xdoc != null)
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
                foreach (WordPair wp in items)
                {
                    wp.Remove();
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

        private void ManageWordsForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            WordPair.Shutdown();
        }

        private void ManageWordsForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Insert)
            {
                buttonAdd_Click(null, null);
                e.SuppressKeyPress = true;
            }
            else if (e.KeyData == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
                e.SuppressKeyPress = true;
            }
        }

        private void textBoxBritishAmerican_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == (Keys.Control | Keys.Back))
            {
                e.SuppressKeyPress = true;
            }
            else if (e.KeyData == (Keys.Control | Keys.C))
            {
                var tb = sender as TextBox;
                if (tb.TextLength > 0 && tb.SelectionLength == 0)
                {
                    var caret = tb.SelectionStart;
                    if (caret > 0)
                        tb.Select(0, caret);
                    else
                        tb.SelectAll();
                    tb.Copy();
                    tb.Select(caret, 0);
                    e.SuppressKeyPress = true;
                }
            }
            else if (e.KeyData == (Keys.Control | Keys.X))
            {
                var tb = sender as TextBox;
                if (tb.TextLength > 0 && tb.SelectionLength == 0)
                {
                    var caret = tb.SelectionStart;
                    if (caret > 0)
                        tb.Select(0, caret);
                    else
                        tb.SelectAll();
                    tb.Cut();
                    tb.Select(0, 0);
                    e.SuppressKeyPress = true;
                }
            }
        }

        private void listView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Delete && _path != null)
            {
                toolStripMenuItemRemoveSelected_Click(null, null);
                if (!string.IsNullOrWhiteSpace(textBoxAmerican.Text))
                    textBoxBritish.Select();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyData == Keys.Right)
            {
                SetFocusToHighlighted(Down);
                e.SuppressKeyPress = true;
            }
            else if (e.KeyData == Keys.Left)
            {
                SetFocusToHighlighted(Up);
                e.SuppressKeyPress = true;
            }
        }

        private void SetFocusToHighlighted(int direction)
        {
            var items = listView1.Items;
            var item = listView1.FocusedItem as WordPair;
            var index = item != null ? item.Index : direction > 0 ? 0 : items.Count - 1;

            if (item != null && item.IsHighlighted)
            {
                var start = index;
                do
                {
                    if ((index = (index + direction + items.Count) % items.Count) == start)
                        return;
                    item = items[index] as WordPair;
                }
                while (item.IsHighlighted);
            }
            if (0 <= index && index < items.Count)
            {
                var start = index;
                while (!(item = items[index] as WordPair).IsHighlighted)
                {
                    if ((index = (index + direction + items.Count) % items.Count) == start)
                        return;
                }
                listView1.SelectedItems.Clear();
                item.EnsureVisible();
                item.Selected = true;
                item.Focused = true;
            }
        }

    }
}
