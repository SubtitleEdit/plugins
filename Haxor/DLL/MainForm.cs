using System.Globalization;
using Nikse.SubtitleEdit.PluginLogic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SubtitleEdit
{
    public partial class MainForm : Form
    {
        private readonly Subtitle _subtitle;
        private readonly StringBuilder _stringBuilder = new StringBuilder();
        private const string From = "abcdefghijlkmnopqrstuvwxyz";
        private readonly string _defaultReplacementChars;

        public string TransformedSubtitle { get; private set; }

        public MainForm(Subtitle sub, string title, string description, Form parentForm)
        {
            InitializeComponent();
            KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Escape)
                {
                    DialogResult = DialogResult.Cancel;
                }
                else if (e.KeyCode == Keys.Enter)
                {
                    OnClick(EventArgs.Empty);
                }
            };
            listViewPreview.Columns[2].Width = -2;
            linkLabelReportBugs.Click += delegate { Process.Start("https://github.com/SubtitleEdit/plugins/issues/new"); };

            Text = title;
            _subtitle = sub;
            _defaultReplacementChars = textBoxTo.Text;
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            GeneratePreview(true);
        }

        public sealed override string Text
        {
            get => base.Text;
            set => base.Text = value;
        }

        private bool CanMapToHaxorChars() => From.Length == textBoxTo.Text.Length;

        private void GeneratePreview(bool isPreviewMode)
        {
            if (!CanMapToHaxorChars())
            {
                MessageBox.Show("'From' and 'To' most have equal number of chars");
                return;
            }

            if (isPreviewMode)
            {
                listViewPreview.BeginUpdate();
            }

            var characterMapping = CreateCharacterMapping();

            foreach (Paragraph p in _subtitle.Paragraphs)
            {
                var before = Utilities.RemoveHtmlTags(p.Text, true).ToLower();
                var after = TranslateToHaxor(before, characterMapping);
                if (before != after)
                {
                    if (isPreviewMode)
                    {
                        AddToListView(p, before, after);
                    }
                    else
                    {
                        p.Text = after;
                    }
                }
            }

            if (isPreviewMode)
            {
                listViewPreview.EndUpdate();
            }
        }

        private Dictionary<char, char> CreateCharacterMapping()
        {
            var fromChars = textBoxFrom.Text;
            var toChars = textBoxTo.Text;
            var characterMapping = new Dictionary<char, char>(fromChars.Length);
            for (int i = 0; i < fromChars.Length; i++)
            {
                characterMapping.Add(fromChars[i], toChars[i]);
            }

            return characterMapping;
        }

        private string TranslateToHaxor(string text, Dictionary<char, char> characterMapping)
        {
            // O(n)
            for (int i = 0; i < text.Length; i++)
            {
                Append(characterMapping.TryGetValue(text[i], out var mapped) ? mapped : text[i]);
            }

            return FlushStringBuilder();
        }

        private void Append(char ch) => _stringBuilder.Append(ch);

        private string FlushStringBuilder()
        {
            var result = _stringBuilder.ToString();
            _stringBuilder.Clear();
            return result;
        }

        private void AddToListView(Paragraph p, string before, string after)
        {
            var item = new ListViewItem(p.Number.ToString(CultureInfo.InvariantCulture)) { Tag = p };
            item.SubItems.Add(before);
            item.SubItems.Add(after);
            listViewPreview.Items.Add(item);
        }

        private void buttonUpdate_Click(object sender, EventArgs e)
        {
            listViewPreview.Items.Clear();
            GeneratePreview(isPreviewMode: true);
        }

        private void buttonReset_Click(object sender, EventArgs e)
        {
            textBoxTo.Text = _defaultReplacementChars;
            listViewPreview.Items.Clear();
            GeneratePreview(isPreviewMode: true);
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            GeneratePreview(isPreviewMode: false);
            TransformedSubtitle = _subtitle.ToText(new SubRip());
            DialogResult = DialogResult.OK;
        }

        private void listView1_Resize(object sender, EventArgs e)
        {
            var size = (listViewPreview.Width - (listViewPreview.Columns[0].Width)) >> 2;
            listViewPreview.Columns[1].Width = size;
            listViewPreview.Columns[2].Width = -2;
        }
    }
}