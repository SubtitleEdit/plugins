using Nikse.SubtitleEdit.PluginLogic;
using System;
using System.Text;
using System.Windows.Forms;

namespace SubtitleEdit
{
    public partial class MainForm : Form
    {
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
            };

            this.buttonOk.Click += (s, e) =>
            {
                _allowFix = true;
                GeneratePreview();
                this.FixedSubtitle = _subtitle.ToText(new SubRip());
                DialogResult = DialogResult.OK;
            };

            this.buttonReset.Click += (s, e) =>
            {
                this.to = "4b©d3fgH!jlKmñ0pqr$tuvwx¥z";
                this.textBoxTo.Text = to;
                this.listView1.Items.Clear();
                GeneratePreview();
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
            foreach (Paragraph p in _subtitle.Paragraphs)
            {
                // Todo: check of paragraph is null
                var before = Utilities.RemoveHtmlTags(p.Text).ToLower();
                var after = string.Empty;
                after = TranslateToHaxor(before);

                if (after != before)
                {
                    if (_allowFix)
                    {
                        p.Text = after;
                    }
                    else
                    {
                        AddToListView(p, before, after);
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

            if (string.IsNullOrEmpty(from) || string.IsNullOrEmpty(to))
            {
                MessageBox.Show("Neither one of textbox can be null");
                this.textBoxTo.Focus();
                return false;
            }

            if (from.Length > 27 || to.Length > 27)
            {
                MessageBox.Show("Length can't be longer than 27");
                this.textBoxTo.Focus();
                return false;
            }

            if (from.Length > to.Length)
            {
                MessageBox.Show("FROM can't be longer than TO!");
                textBoxFrom.Focus();
                return false;
            }

            if (from.Length < to.Length)
            {
                MessageBox.Show("TO can't be longer than FROM!");
                textBoxTo.Focus();
                return false;
            }
            return true;
        }
    }
}