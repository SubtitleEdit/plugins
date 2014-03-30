using Nikse.SubtitleEdit.PluginLogic;
using System;
using System.Collections.Generic;
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
        private string name;
        private const string from = "abcdefghijlkmnopqrstuvwxyz";
        private string to;

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
        }

        public MainForm(Subtitle sub, string name, string description, Form parentForm)
            : this()
        {
            this._subtitle = sub;
            this.name = name;
            this.description = description;
            this._mainForm = parentForm;
            to = textBoxTo.Text;
            GeneratePreview();
        }

        private void GeneratePreview()
        {
            if (_subtitle == null)
                return;
            this.listView1.BeginUpdate();
            this._dicChanged = new Dictionary<int, string>();
            foreach (Paragraph p in _subtitle.Paragraphs)
            {
                if (_allowFix && _dicChanged.ContainsKey(p.Number))
                {
                    p.Text = _dicChanged[p.Number];
                }
                else
                {
                    var before = Utilities.RemoveHtmlTags(p.Text).ToLower();
                    var after = TranslateToHaxor(before);
                    if (after != before)
                    {
                        AddToListView(p, before, after);
                        _dicChanged.Add(p.Number, after);
                    }
                }
            }
            this.listView1.EndUpdate();
        }

        private string TranslateToHaxor(string text)
        {
            if (text.Trim().Length < 1)
                return text;
            // Todo: check if text is already translated!
            var strBuilder = new StringBuilder(text);
            if (CheckTranslator())
            {
                for (int i = 0; i < from.Length; i++)
                {
                    strBuilder.Replace(from[i], to[i]);
                }
            }
            return strBuilder.ToString();
        }

        private void AddToListView(Paragraph p, string before, string after)
        {
            var item = new ListViewItem(p.Number.ToString());
            var subItem = new ListViewItem.ListViewSubItem(item, before);
            item.SubItems.Add(subItem);
            subItem = new ListViewItem.ListViewSubItem(item, after);
            item.SubItems.Add(subItem);
            item.Tag = p;
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
            to = textBoxTo.Text.Trim();
            to = to.Replace(" ", string.Empty);

            if (string.IsNullOrEmpty(to) || to.Length != from.Length)
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
            this.to = "4b©d3fgH!jlKmñ0pqr$tuvwx¥z"; // this are the default haxor char
            this.textBoxTo.Text = to;
            this.listView1.Items.Clear();

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
    }
}