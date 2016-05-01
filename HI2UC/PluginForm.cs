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
        private HIStyle _hiStyle = HIStyle.UpperCase;
        private int _totalChanged;
        private readonly Form _parentForm;
        private readonly Subtitle _subtitle;

        private Dictionary<string, string> _fixedTexts = new Dictionary<string, string>();
        private HashSet<string> _notAllowedFixes = new HashSet<string>();
        private readonly List<Paragraph> _deletedParagarphs = new List<Paragraph>();
        private static readonly char[] HIChars = { '(', '[' };
        private static readonly Regex RegexExtraSpaces = new Regex(@"(?<=[\(\[]) +| +(?=[\)\]])", RegexOptions.Compiled);
        private static readonly Regex RegexFirstChar1 = new Regex(@"\b\w", RegexOptions.Compiled);
        private static readonly Regex RegexFirstChar2 = new Regex("(?<!<)\\w", RegexOptions.Compiled); // Will also avoid matching <i> or <b>...
        private readonly StringBuilder SB = new StringBuilder();

        public PluginForm(Form parentForm, Subtitle subtitle, string name, string description)
        {
            InitializeComponent();
            _parentForm = parentForm;
            _subtitle = subtitle;
            labelDesc.Text = "Description: " + description;
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

        private void comboBoxStyle_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxStyle.SelectedIndex < 0 || listViewFixes.Items.Count < 0)
                return;
            if ((int)_hiStyle != comboBoxStyle.SelectedIndex)
            {
                _hiStyle = (HIStyle)comboBoxStyle.SelectedIndex;
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
                bool containsMood = false;
                bool containsNarrator = false;

                // (Moods and feelings)
                if (text.ContainsAny(HIChars))
                {
                    //Remove Extra Spaces inside brackets ( foobar ) to (foobar)
                    if (checkBoxRemoveSpaces.Checked)
                        text = RegexExtraSpaces.Replace(text, string.Empty);
                    string beforeChanges = text;
                    text = ChangeMoodsToUppercase(text);
                    containsMood = beforeChanges != text;
                }

                // Narrator:
                if (checkBoxNames.Checked)
                {
                    string beforeChanges = text;
                    text = NarratorToUpper(text);
                    containsNarrator = beforeChanges != text;
                }

                if (text != oldText)
                {
                    _fixedTexts.Add(p.Id, text);
                    oldText = Utilities.RemoveHtmlTags(oldText, true);
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
            item.SubItems.Add(p.Number.ToString());
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
                string secondLine = after.Substring(idx).Trim();
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

        public int BracketsIndex(string text, int idx)
        {
            if (idx < 0 || idx + 1 >= text.Length)
                return -1;

            char openBracket = text[idx];
            char closeBracket = '\0';
            switch (openBracket)
            {
                case '(':
                    closeBracket = ')';
                    break;
                case '[':
                    closeBracket = ']';
                    break;
            }

            while (idx >= 0)
            {
                int endIdx = text.IndexOf(closeBracket, idx + 1); // ] or )
                if (endIdx < idx + 2)
                {
                    break;
                }
                string moodText = text.Substring(idx, endIdx - idx + 1);
                moodText = StyleMoodsAndFeelings(moodText);
                text = text.Remove(idx, endIdx - idx + 1).Insert(idx, moodText);
                idx = text.IndexOf(openBracket, endIdx + 1); // ( or [
            }
            return idx;
        }


        private string ChangeMoodsToUppercase(string text)
        {
            text = text.Replace("()", string.Empty);
            text = text.Replace("[]", string.Empty);
            BracketsIndex(text, text.IndexOfAny(HIChars));
            return text.FixExtraSpaces();
        }

        private string StyleMoodsAndFeelings(string text)
        {
            var before = text;
            switch (_hiStyle)
            {
                case HIStyle.UpperLowerCase:
                    SB.Clear();
                    bool isUpperTime = true;
                    foreach (char myChar in text)
                    {
                        if (!char.IsLetter(myChar))
                        {
                            SB.Append(myChar);
                        }
                        else
                        {
                            SB.Append(isUpperTime ? char.ToUpper(myChar) : char.ToLower(myChar));
                            isUpperTime = !isUpperTime;
                        }
                    }
                    text = SB.ToString();
                    break;
                case HIStyle.FirstUppercase:
                    text = RegexFirstChar1.Replace(text.ToLower(), x => x.Value.ToUpper()); // foobar to Foobar
                    break;
                case HIStyle.UpperCase:
                    text = text.ToUpper();
                    break;
                case HIStyle.LowerCase:
                    text = text.ToLower();
                    break;
            }
            return text;
        }

        //delegate void RefAction<in T>(ref T obj);
        private string NarratorToUpper(string text)
        {
            string before = text;
            var t = Utilities.RemoveHtmlTags(text, true).TrimEnd().TrimEnd('"');
            var index = t.IndexOf(':');

            // like: "Ivandro Says:"
            if (index == t.Length - 1 || text.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                return text;
            }

            var lines = text.SplitToLines();
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                var noTagText = Utilities.RemoveHtmlTags(line, true).Trim();
                index = noTagText.IndexOf(':');
                if (index < 1 || !CanUpper(noTagText, index))
                {
                    continue;
                }

                // Ivandro ismael:
                // hello world!
                if (i > 0 && index + 1 >= noTagText.Length)
                {
                    continue;
                }

                // Now find : in original text
                lines[i] = ConvertToUpper(line, line.IndexOf(':'));
            }
            return string.Join(Environment.NewLine, lines);
        }

        private bool CanUpper(string line, int index)
        {
            if ((index + 1 >= line.Length) || char.IsDigit(line[index + 1]))
                return false;
            line = line.Substring(0, index);
            if (line.EndsWith("improved by", StringComparison.OrdinalIgnoreCase) ||
                line.EndsWith("corrected by", StringComparison.OrdinalIgnoreCase))
                return false;
            return true;
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

        private static string ConvertToUpper(string s, int colonIdx)
        {
            var pre = s.Substring(0, colonIdx);

            // (Adele: ...)
            if (pre.ContainsAny(HIChars) || s.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                return s;

            if (Utilities.RemoveHtmlTags(pre, true).Trim().Length > 1)
            {
                var firstChr = RegexFirstChar2.Match(pre).Value; // Note: this will prevent to fix tag pre narrator <i>John: Hey!</i>
                int idx = pre.IndexOf(firstChr, StringComparison.Ordinal);
                if (idx >= 0)
                {
                    var narrator = pre.Substring(idx, colonIdx - idx);
                    var oldNarrator = narrator;
                    // Filter http protocols
                    if (narrator.TrimEnd().EndsWith("HTTPS", StringComparison.OrdinalIgnoreCase) ||
                        narrator.TrimEnd().EndsWith("HTTP", StringComparison.OrdinalIgnoreCase))
                        return s;

                    narrator = narrator.ToUpper();
                    // Return if narrator is already uppercase
                    if (narrator == oldNarrator)
                        return s;
                    pre = pre.Remove(idx, colonIdx - idx).Insert(idx, narrator);
                    s = s.Remove(0, colonIdx).Insert(0, pre);
                }
            }
            return s;
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