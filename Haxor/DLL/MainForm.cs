using Nikse.SubtitleEdit.PluginLogic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;

namespace SubtitleEdit
{
    public partial class MainForm : Form
    {
        private Dictionary<int, string> _dicChanged;
        private Subtitle _subtitle;
        private Form _mainForm;
        private bool _allowFix = false;
        private string description;
        private const string _from = "abcdefghijlkmnopqrstuvwxyz";
        private string _to;

        public string FixedSubtitle { get; private set; }

        public MainForm()
        {
            InitializeComponent();
            this.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Escape)
                    DialogResult = DialogResult.Cancel;
                else if (e.KeyCode == Keys.Enter)
                    this.OnClick(EventArgs.Empty);
            };

            listView1.Columns[2].Width = -2;

            var link = new LinkLabel.Link();
            link.LinkData = "https://github.com/SubtitleEdit/plugins/issues/new";
            linkLabel1.Links.Add(link);
            linkLabel1.Click += delegate { Process.Start(link.LinkData as string); };
        }

        public MainForm(Subtitle sub, string title, string description, Form parentForm)
            : this()
        {
            this.Text = title;
            this._subtitle = sub;
            this.description = description;
            this._mainForm = parentForm;
            _to = textBoxTo.Text;
            GeneratePreview();
        }

        private void GeneratePreview()
        {
            if (_subtitle == null)
                return;
            this.listView1.BeginUpdate();
            _dicChanged = new Dictionary<int, string>();
            foreach (Paragraph p in _subtitle.Paragraphs)
            {
                if (_allowFix && _dicChanged.ContainsKey(p.Number))
                {
                    p.Text = _dicChanged[p.Number];
                }
                else
                {
                    var before = Utilities.RemoveHtmlTags(p.Text, true).ToLower();
                    if (before.IndexOf("http", StringComparison.Ordinal) >= 0)
                        continue;
                    var after = TranslateToHaxor(before);
                    if (after != before)
                    {
                        AddToListView(p, before, after);
                        _dicChanged.Add(p.Number, after);
                    }
                }
            }
            this.listView1.EndUpdate();
            Application.DoEvents();
        }

        private string TranslateToHaxor(string text)
        {
            if (text.Trim().Length < 1)
                return text;
            // Todo: check if text is already translated!
            var strBuilder = new StringBuilder(text);
            if (CheckTranslator())
            {
                for (int i = 0; i < _from.Length; i++)
                {
                    strBuilder.Replace(_from[i], _to[i]);
                }
            }
            return strBuilder.ToString();
        }

        private void AddToListView(Paragraph p, string before, string after)
        {
            var item = new ListViewItem(p.Number.ToString()) { Tag = p };
            item.SubItems.Add(before);
            item.SubItems.Add(after);
            listView1.Items.Add(item);
        }

        private void buttonUpdate_Click(object sender, EventArgs e)
        {
            if (CheckTranslator())
            {
                this.listView1.Items.Clear();
                GeneratePreview();
            }
        }

        private bool CheckTranslator()
        {
            _to = textBoxTo.Text.Trim();
            _to = _to.Replace(" ", string.Empty);

            if (string.IsNullOrEmpty(_to) || _to.Length != _from.Length)
            {
                textBoxTo.SelectAll();
                //textBoxTo.SelectionStart = 0;
                //textBoxTo.SelectionLength = textBoxTo.Text.Length;
                this.textBoxTo.Focus();
                return false;
            }
            return true;
        }

        private void buttonReset_Click(object sender, EventArgs e)
        {
            this._to = "4b©d3fgH!jlKmñ0pqr$tuvwx¥z"; // these are the default haxor char
            this.textBoxTo.Text = _to;
            this.listView1.Items.Clear();
            this._dicChanged = null;
            GeneratePreview();
            Application.DoEvents();
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            _allowFix = true;
            GeneratePreview();
            this.FixedSubtitle = _subtitle.ToText(new SubRip());
            DialogResult = DialogResult.OK;
        }

        private void listView1_Resize(object sender, EventArgs e)
        {
            var size = (this.listView1.Width - (listView1.Columns[0].Width)) >> 2;
            listView1.Columns[1].Width = size;
            listView1.Columns[2].Width = size;
        }
    }
}