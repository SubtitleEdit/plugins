using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.PluginLogic
{
    internal partial class PluginForm : Form
    {
        public string FixedSubtitle { get; private set; }
        private readonly IHearingImpaired _hiStyler;

        private bool _moodsMatched;
        private bool _namesMatched;
        private int _totalChanged;
        private readonly Form _parentForm;
        private readonly Subtitle _subtitle;

        private Dictionary<string, string> _fixedTexts = new Dictionary<string, string>();
        private HashSet<string> _notAllowedFixes = new HashSet<string>();
        private readonly List<Paragraph> _deletedParagarphs = new List<Paragraph>();

        private readonly Regex _regexExtraSpaces = new Regex(@"(?<=[\(\[]) +| +(?=[\)\]])", RegexOptions.Compiled);
        private readonly StringBuilder SB = new StringBuilder();

        public PluginForm(Form parentForm, Subtitle subtitle, string name, string description)
        {
            InitializeComponent();
            _parentForm = parentForm;
            _subtitle = subtitle;
            labelDesc.Text = "Description: " + description;
            _hiStyler = new HearingImpaired();

            /*
            this.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Escape)
                    this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            };
             */
        }

        private void PluginForm_Load(object sender, EventArgs e)
        {
            comboBoxStyle.SelectedIndex = 0;
            Resize += delegate
            {
                listViewFixes.Columns[listViewFixes.Columns.Count - 1].Width = -2;
            };
        }

        private void btn_Cancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void btn_Run_Click(object sender, EventArgs e)
        {
            if (listViewFixes.Items.Count > 0)
            {
                Cursor = Cursors.WaitCursor;
                listViewFixes.Resize -= listViewFixes_Resize;
                ApplyChanges(false);
                Cursor = Cursors.Default;
            }
            FixedSubtitle = _subtitle.ToText();
            DialogResult = DialogResult.OK;
        }

        private void ApplyChanges(bool generatePreview)
        {
            if (_subtitle == null || _subtitle.Paragraphs == null || _subtitle.Paragraphs.Count == 0)
                return;
            for (int i = _subtitle.Paragraphs.Count - 1; i >= 0; i--)
            {
                var p = _subtitle.Paragraphs[i];
                if (!_notAllowedFixes.Contains(p.Id) && _fixedTexts.ContainsKey(p.Id))
                    p.Text = _fixedTexts[p.Id];
            }
            if (generatePreview)
            {
                GeneratePreview();
            }
        }

        private void checkBoxNarrator_CheckedChanged(object sender, EventArgs e)
        {
            if (_subtitle.Paragraphs.Count <= 0)
                return;
            GeneratePreview();
        }

        private void CheckTypeStyle(object sender, EventArgs e)
        {
            var menuItem = sender as ToolStripMenuItem;
            if (listViewFixes.Items.Count <= 0 || menuItem == null)
                return;
            if (menuItem.Text == "Check all")
            {
                for (int i = 0; i < listViewFixes.Items.Count; i++)
                    listViewFixes.Items[i].Checked = true;
            }
            else if (menuItem.Text == "Uncheck all")
            {
                for (int i = 0; i < listViewFixes.Items.Count; i++)
                    listViewFixes.Items[i].Checked = false;
            }
            else if (menuItem.Text == "Invert check")
            {
                for (int i = 0; i < listViewFixes.Items.Count; i++)
                    listViewFixes.Items[i].Checked = !listViewFixes.Items[i].Checked;
            }
            else if (menuItem.Text == "Copy")
            {
                var text = (listViewFixes.FocusedItem.Tag as Paragraph).ToString();
                Clipboard.SetText(text);
            }
            else
            {
                for (int idx = listViewFixes.SelectedIndices.Count - 1; idx >= 0; idx--)
                {
                    var index = listViewFixes.SelectedIndices[idx];
                    var p = listViewFixes.Items[idx].Tag as Paragraph;
                    if (p != null)
                    {
                        _subtitle.RemoveLine(p.Number);
                    }
                    listViewFixes.Items.RemoveAt(index);
                }
                _subtitle.Renumber();
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxStyle.SelectedIndex < 0 || listViewFixes.Items.Count < 0)
                return;
            if ((int)_hiStyler.Style != comboBoxStyle.SelectedIndex)
            {
                _hiStyler.Style = (HIStyle)comboBoxStyle.SelectedIndex;
                GeneratePreview();
            }
        }

        private void GeneratePreview()
        {
            _totalChanged = 0;
            listViewFixes.BeginUpdate();
            listViewFixes.Items.Clear();

            _fixedTexts = new Dictionary<string, string>();
            _notAllowedFixes = new HashSet<string>();

            listViewFixes.ItemChecked -= listViewFixes_ItemChecked;
            foreach (Paragraph p in _subtitle.Paragraphs)
            {
                var text = p.Text;
                var oldText = text;

                // (Moods and feelings)
                if (text.ContainsAny(HearingImpaired.ExpectedHIChars))
                {
                    //Remove Extra Spaces inside brackets ( foobar ) to (foobar)
                    if (checkBoxRemoveSpaces.Checked)
                        text = _regexExtraSpaces.Replace(text, string.Empty);
                    text = _hiStyler.ChangeMoodsToUppercase(text);// Todo: call this through instance
                }

                // Narrator:
                if (checkBoxNames.Checked && text.Contains(':'))
                    text = _hiStyler.NarratorToUpper(text);

                if (text != oldText)
                {
                    _fixedTexts.Add(p.Id, text);
                    oldText = Utilities.RemoveHtmlTags(oldText, true);
                    text = Utilities.RemoveHtmlTags(text, true);
                    AddFixToListView(p, oldText, text);
                    _totalChanged++;
                }

                // Reset
                _namesMatched = false;
                _moodsMatched = false;

            }

            listViewFixes.ItemChecked += listViewFixes_ItemChecked;
            groupBox1.ForeColor = _totalChanged <= 0 ? Color.Red : Color.Green;
            groupBox1.Text = string.Format("Total Found: {0}", _totalChanged);
            /*this.listViewFixes.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            this.listViewFixes.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);*/
            //Application.DoEvents();
            listViewFixes.EndUpdate();
            Refresh();
        }

        private void AddFixToListView(Paragraph p, string before, string after)
        {
            var item = new ListViewItem() { Checked = true, UseItemStyleForSubItems = true, Tag = p };
            item.SubItems.Add(p.Number.ToString());
            if (_moodsMatched && _namesMatched)
                item.SubItems.Add("Name & Mood");
            else if (_moodsMatched && !_namesMatched)
                item.SubItems.Add("Mood");
            else if (_namesMatched && !_moodsMatched)
                item.SubItems.Add("Name");
            item.SubItems.Add(before.Replace(Environment.NewLine, Configuration.ListViewLineSeparatorString));
            item.SubItems.Add(after.Replace(Environment.NewLine, Configuration.ListViewLineSeparatorString));

            if (after.Replace(Environment.NewLine, string.Empty).Length != after.Length)
            {
                int idx = after.IndexOf(Environment.NewLine, StringComparison.Ordinal);
                if (idx > 2)
                {
                    string firstLine = after.Substring(0, idx).Trim();
                    string secondLine = after.Substring(idx).Trim();
                    int idx1 = firstLine.IndexOf(':');
                    int idx2 = secondLine.IndexOf(':');
                    if (idx1 > 0xE || idx2 > 0xE)
                    {
                        item.BackColor = Color.Pink;
                    }
                }
            }
            else
            {
                if (after.IndexOf(':') > 0xE)
                    item.BackColor = Color.Pink;
            }

            listViewFixes.Items.Add(item);
        }

        private void buttonApply_Click(object sender, EventArgs e)
        {
            if (listViewFixes.Items.Count == 0)
                return;

            Cursor = Cursors.WaitCursor;
            listViewFixes.Resize -= listViewFixes_Resize;
            GeneratePreview();
            ApplyChanges(true);
            listViewFixes.Resize += listViewFixes_Resize;
            Cursor = Cursors.Default;
        }

        private void checkBoxRemoveSpaces_CheckedChanged(object sender, EventArgs e)
        {
            if (listViewFixes.Items.Count < 1 || _subtitle.Paragraphs.Count == 0)
                return;

            GeneratePreview();
        }

        private void PluginForm_Shown(object sender, EventArgs e)
        {
            GeneratePreview();
        }

        private void listViewFixes_Resize(object sender, EventArgs e)
        {
            var newWidth = (listViewFixes.Width - (listViewFixes.Columns[0].Width + listViewFixes.Columns[1].Width + listViewFixes.Columns[2].Width)) / 2;
            listViewFixes.Columns[3].Width = newWidth;
            listViewFixes.Columns[4].Width = newWidth;
        }

        private void listViewFixes_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            Paragraph p = null;
            if (e.Item == null || (p = e.Item.Tag as Paragraph) == null)
                return;
            if (e.Item.Checked)
            {
                _notAllowedFixes.Remove(p.Id);
            }
            else
            {
                _notAllowedFixes.Add(p.Id);
            }
        }
    }
}