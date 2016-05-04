using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.PluginLogic
{
    internal partial class PluginForm : Form
    {
        public string FixedSubtitle { get; private set; }
        private int _totalChanged;
        private readonly Subtitle _subtitle;

        private Dictionary<string, string> _fixedTexts = new Dictionary<string, string>();
        private HashSet<string> _notAllowedFixes = new HashSet<string>();
        private readonly List<Paragraph> _deletedParagarphs = new List<Paragraph>();
        private static readonly StringBuilder SB = new StringBuilder();

        private readonly HearingImpaired _hearingImpaired;

        public PluginForm(Subtitle subtitle, string name, string description)
        {
            InitializeComponent();
            _subtitle = subtitle;
            labelDesc.Text = "Description: " + description;
            _hearingImpaired = new HearingImpaired(new Configuration()
            {
                MoodsToUppercase = true,
                NarratorToUppercase = true,
                Style = HIStyle.UpperCase

            });
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
            comboBoxStyle.SelectedIndexChanged -= comboBoxStyle_SelectedIndexChanged;
            comboBoxStyle.SelectedIndex = 0;
            comboBoxStyle.SelectedIndexChanged += comboBoxStyle_SelectedIndexChanged;
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
            _hearingImpaired.Config.NarratorToUppercase = checkBoxNames.Checked;
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

        private void comboBoxStyle_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewFixes.Items.Count < 0)
            {
                return;
            }
            // Is this using ref passing? o.O
            HIStyle currentStyle = _hearingImpaired.Config.Style;
            if ((int)currentStyle != comboBoxStyle.SelectedIndex)
            {
                currentStyle = (HIStyle)comboBoxStyle.SelectedIndex;
                _hearingImpaired.Config.Style = currentStyle;
                GeneratePreview();
            }
        }

        private void GeneratePreview()
        {
            // One of the options most be checked.
            if (!(checkBoxMoods.Checked || checkBoxNames.Checked || checkBoxRemoveSpaces.Checked))
            {
                return;
            }
            _totalChanged = 0;
            listViewFixes.BeginUpdate();
            listViewFixes.Items.Clear();

            _fixedTexts = new Dictionary<string, string>();
            _notAllowedFixes = new HashSet<string>();

            listViewFixes.ItemChecked -= listViewFixes_ItemChecked;
            foreach (Paragraph p in _subtitle.Paragraphs)
            {
                var text = p.Text;
                bool containsMood = false;
                bool containsNarrator = false;

                // Remove Extra Spaces inside brackets ( foobar ) to (foobar)
                string beforeChanges = text;
                if (checkBoxRemoveSpaces.Checked)
                {
                    _hearingImpaired.RemoveExtraSpacesInsideTag(text);
                }
                // (Moods and feelings)
                if (checkBoxMoods.Checked)
                {
                    beforeChanges = text;
                    text = _hearingImpaired.MoodsToUppercase(text);
                    if (beforeChanges != text)
                    {
                        text = text.FixExtraSpaces();
                        containsMood = true;
                    }
                }
                // Narrator:
                if (checkBoxNames.Checked)
                {
                    beforeChanges = text;
                    text = _hearingImpaired.NarratorToUppercase(text);
                    containsNarrator = beforeChanges != text;
                }

                if (containsMood || containsNarrator)
                {
                    _fixedTexts.Add(p.Id, text);
                    string oldText = Utilities.RemoveHtmlTags(p.Text, true);
                    text = Utilities.RemoveHtmlTags(text, true);
                    AddFixToListView(p, oldText, text, containsMood, containsNarrator);
                    _totalChanged++;
                }

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

        private void AddFixToListView(Paragraph p, string before, string after, bool containsMood, bool containsNarrator)
        {
            var item = new ListViewItem() { Checked = true, UseItemStyleForSubItems = true, Tag = p };
            item.SubItems.Add(p.Number.ToString(CultureInfo.InvariantCulture));
            if (containsMood && containsNarrator)
            {
                item.SubItems.Add("Name & Mood");
            }
            else if (containsMood)
            {
                item.SubItems.Add("Mood");
            }
            else if (containsNarrator)
            {
                item.SubItems.Add("Narrator");
            }
            item.SubItems.Add(before.Replace(Environment.NewLine, Configuration.ListViewLineSeparatorString));
            item.SubItems.Add(after.Replace(Environment.NewLine, Configuration.ListViewLineSeparatorString));

            int idx = after.IndexOf(Environment.NewLine, StringComparison.Ordinal);
            if (idx > 2)
            {
                string firstLine = after.Substring(0, idx).Trim();
                string secondLine = after.Substring(idx + Environment.NewLine.Length).Trim();
                int idx1 = firstLine.IndexOf(':');
                int idx2 = secondLine.IndexOf(':');
                if (idx1 > 0xE || idx2 > 0xE)
                {
                    item.BackColor = Color.Pink;
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

        private void checkBoxMoods_CheckedChanged(object sender, EventArgs e)
        {
            _hearingImpaired.Config.MoodsToUppercase = checkBoxMoods.Checked;
            GeneratePreview();
        }
    }
}