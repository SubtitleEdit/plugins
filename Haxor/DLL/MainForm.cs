﻿using System.Globalization;
using Nikse.SubtitleEdit.PluginLogic;
using System;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;

namespace SubtitleEdit
{
    public partial class MainForm : Form
    {
        private readonly Subtitle _subtitle;
        private readonly StringBuilder _stringBuilder = new StringBuilder();
        private const string From = "abcdefghijlkmnopqrstuvwxyz";
        private string _to;

        public string FixedSubtitle { get; private set; }

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
            listView1.Columns[2].Width = -2;
            linkLabel1.Click += delegate { Process.Start("https://github.com/SubtitleEdit/plugins/issues/new"); };

            Text = title;
            _subtitle = sub;
            _to = textBoxTo.Text;
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            GeneratePreview(false);
        }

        public sealed override string Text
        {
            get => base.Text;
            set => base.Text = value;
        }

        private void GeneratePreview(bool setText)
        {
            if (_subtitle == null)
            {
                listView1.BeginUpdate();
            }

            foreach (Paragraph p in _subtitle.Paragraphs)
            {
                var before = Utilities.RemoveHtmlTags(p.Text, true).ToLower();
                var after = TranslateToHaxor(before);
                AddToListView(p, before, after);
                if (setText)
                {
                    p.Text = after;
                }
            }

            listView1.EndUpdate();
        }

        private string TranslateToHaxor(string text)
        {
            _stringBuilder.Append(text);
            for (int i = 0; i < From.Length; i++)
            {
                _stringBuilder.Replace(From[i], _to[i]);
            }

            return FlushStringBuilder();
        }
        
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
            listView1.Items.Add(item);
        }

        private void buttonUpdate_Click(object sender, EventArgs e)
        {
            listView1.Items.Clear();
            GeneratePreview(false);
        }

        private void buttonReset_Click(object sender, EventArgs e)
        {
            _to = "4b©d3fgH!jlKmñ0pqr$tuvwx¥z"; // these are the default haxor char
            textBoxTo.Text = _to;
            listView1.Items.Clear();
            GeneratePreview(false);
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            GeneratePreview(true);
            FixedSubtitle = _subtitle.ToText(new SubRip());
            DialogResult = DialogResult.OK;
        }

        private void listView1_Resize(object sender, EventArgs e)
        {
            var size = (listView1.Width - (listView1.Columns[0].Width)) >> 2;
            listView1.Columns[1].Width = size;
            listView1.Columns[2].Width = -2;
        }
    }
}