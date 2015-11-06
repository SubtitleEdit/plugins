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
        private enum HIStyle
        {
            UpperCase,
            LowerCase,
            FirstUppercase,
            UpperLowerCase
        }

        public string FixedSubtitle { get; private set; }

        private HIStyle _hiStyle = HIStyle.UpperCase;
        private bool _deleteLine = false;
        private bool _moodsMatched;
        private bool _namesMatched;
        private int _totalChanged;
        private readonly Form _parentForm;
        private readonly Subtitle _subtitle;

        private Dictionary<string, string> _fixedTexts = new Dictionary<string, string>();
        private HashSet<string> _notAllowedFixes = new HashSet<string>();
        private readonly List<Paragraph> _deletedParagarphs = new List<Paragraph>();
        private readonly char[] HIChars = { '(', '[' };
        private readonly Regex _regexExtraSpaces = new Regex(@"(?<=[\(\[]) +| +(?=[\)\]])", RegexOptions.Compiled);
        private readonly Regex _regexFirstChar = new Regex(@"\b\w", RegexOptions.Compiled);
        private readonly StringBuilder SB = new StringBuilder();

        public PluginForm(Form parentForm, Subtitle subtitle, string name, string description)
        {
            InitializeComponent();
            _parentForm = parentForm;
            _subtitle = subtitle;
            label1.Text = "Description: " + description;
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

                // (Moods and feelings)
                if (text.ContainsAny(HIChars))
                {
                    //Remove Extra Spaces inside brackets ( foobar ) to (foobar)
                    if (checkBoxRemoveSpaces.Checked)
                        text = _regexExtraSpaces.Replace(text, string.Empty);
                    text = ChangeMoodsToUppercase(text);
                }

                // Narrator:
                if (checkBoxNames.Checked && text.Contains(':'))
                    text = NarratorToUpper(text);

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

        private string ChangeMoodsToUppercase(string text)
        {
            Action<char, int> FindBrackets = delegate (char openBracket, int idx)
            {
                //char? closeBracket = null;
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
                    if (endIdx < idx + 1)
                        break;

                    var moodText = text.Substring(idx, endIdx - idx + 1);
                    moodText = StyleMoodsAndFeelings(moodText);

                    if (_moodsMatched)
                        text = text.Remove(idx, endIdx - idx + 1).Insert(idx, moodText);

                    idx = text.IndexOf(openBracket, endIdx + 1); // ( or [
                }
            };
            text = text.Replace("()", string.Empty);
            text = text.Replace("[]", string.Empty);
            var bIdx = text.IndexOfAny(HIChars);
            if (bIdx >= 0)
                FindBrackets(text[bIdx], bIdx);
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
                    text = _regexFirstChar.Replace(text.ToLower(), x => x.Value.ToUpper()); // foobar to Foobar
                    break;
                case HIStyle.UpperCase:
                    text = text.ToUpper();
                    break;
                case HIStyle.LowerCase:
                    text = text.ToLower();
                    break;
            }
            _moodsMatched = text != before;
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
                var noTagText = Utilities.RemoveHtmlTags(lines[i], true).Trim();
                index = noTagText.IndexOf(':');
                if (!CanUpper(noTagText, index))
                    continue;

                // Ivandro ismael:
                // hello world!
                if (i > 0 && index >= noTagText.Length)
                    continue;

                // Now find : in original text
                index = lines[i].IndexOf(':');
                if (index > 0)
                {
                    lines[i] = ConvertToUpper(lines[i], index);
                }
            }
            text = string.Join(Environment.NewLine, lines);

            if (before != text)
                _namesMatched = true;
            return text;
        }

        private bool CanUpper(string line, int index)
        {
            if (index <= 0 || (index + 1 >= line.Length) || char.IsDigit(line[index + 1]))
                return false;
            line = line.Substring(0, index);
            if (line.EndsWith("improved by", StringComparison.OrdinalIgnoreCase) ||
                line.EndsWith("corrected by", StringComparison.OrdinalIgnoreCase))
                return false;
            return true;
        }

        private string MoveHtmlTagsAfterColon(string narrator)
        {
            // Fix uppercase tags
            int tagIndex = narrator.IndexOf('<');
            while (tagIndex >= 0)
            {
                int closeIndex = narrator.IndexOf('>', tagIndex + 1);
                if (closeIndex > -1 && closeIndex > tagIndex)
                {
                    string temp = narrator.Substring(tagIndex, (closeIndex - tagIndex)).ToLower();
                    narrator = narrator.Remove(tagIndex, (closeIndex - tagIndex)).Insert(tagIndex, temp);
                }
                if (closeIndex > -1)
                    tagIndex = narrator.IndexOf('<', closeIndex);
                else
                    tagIndex = -1;
            }
            return narrator;
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

        private readonly Regex _RegexFirstChar = new Regex("(?<!<)\\w", RegexOptions.Compiled); // Will also avoid matching <i> or <b>...
        private string ConvertToUpper(string s, int colonIdx)
        {
            var pre = s.Substring(0, colonIdx);

            // (Adele: ...)
            if (pre.ContainsAny(HIChars) || s.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                return s;

            if (Utilities.RemoveHtmlTags(pre, true).Trim().Length > 1)
            {
                var firstChr = _RegexFirstChar.Match(pre).Value; // Note: this will prevent to fix tag pre narrator <i>John: Hey!</i>
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