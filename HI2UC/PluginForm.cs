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
        private bool _allowFixes = false;
        private bool _deleteLine = false;
        private bool _moodsMatched;
        private bool _namesMatched;
        private int _totalChanged;
        private readonly Form _parentForm;
        private readonly Subtitle _subtitle;

        private readonly List<Paragraph> _listDeletedParagraphs = new List<Paragraph>();
        private readonly char[] HIChars = { '(', '[' };
        private readonly Regex _regexExtraSpaces = new Regex(@"(?<=[\(\[]) +| +(?=[\)\]])", RegexOptions.Compiled);
        private readonly Regex _regexFirstChar = new Regex(@"\b\w", RegexOptions.Compiled);
        private readonly StringBuilder SB = new StringBuilder();

        public PluginForm(Form parentForm, Subtitle subtitle, string name, string description)
        {
            InitializeComponent();
            this._parentForm = parentForm;
            this._subtitle = subtitle;
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
            comboBox1.SelectedIndex = 0;
            this.Resize += delegate
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
                _allowFixes = true;
                listViewFixes.Resize -= listViewFixes_Resize;
                FindHearingImpairedText();
                RemoveDeletedParagraphs();

                Cursor = Cursors.Default;
            }
            FixedSubtitle = _subtitle.ToText();
            DialogResult = DialogResult.OK;
        }

        private void checkBoxNarrator_CheckedChanged(object sender, EventArgs e)
        {
            if (_subtitle.Paragraphs.Count <= 0)
                return;
            FindHearingImpairedText();
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
                if (listViewFixes.FocusedItem.BackColor != Color.Red)
                {
                    _listDeletedParagraphs.Add(listViewFixes.FocusedItem.Tag as Paragraph);
                    this.listViewFixes.FocusedItem.UseItemStyleForSubItems = true;
                    this.listViewFixes.FocusedItem.BackColor = Color.Red;
                    _deleteLine = true;
                }
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex < 0 || listViewFixes.Items.Count < 0)
                return;
            if ((int)_hiStyle != comboBox1.SelectedIndex)
            {
                _hiStyle = (HIStyle)comboBox1.SelectedIndex;
                FindHearingImpairedText();
            }
        }

        private void FindHearingImpairedText()
        {
            _totalChanged = 0;
            listViewFixes.BeginUpdate();
            if (!_allowFixes)
                listViewFixes.Items.Clear();
            foreach (Paragraph p in _subtitle.Paragraphs)
            {
                var text = p.Text;
                var oldText = p.Text;

                // (Moods and feelings)
                if (text.ContainsAny(HIChars))
                {
                    //Remove Extra Spaces inside brackets ( foobar ) to (foobar)
                    if (checkBoxRemoveSpaces.Checked)
                        text = _regexExtraSpaces.Replace(text, string.Empty);
                    text = ChangeMoodsToUppercase(text, p);
                }

                // Narrator:
                if (checkBoxNames.Checked && text.Contains(':'))
                    text = NarratorToUpper(text);

                if (text != oldText)
                {
                    if (AllowFix(p))
                    {
                        p.Text = text;
                    }
                    else
                    {
                        if (!_allowFixes)
                        {
                            oldText = Utilities.RemoveHtmlTags(oldText, true);
                            text = Utilities.RemoveHtmlTags(text, true);
                            AddFixToListView(p, oldText, text);
                            _totalChanged++;
                        }
                        // Reset
                        _namesMatched = false;
                        _moodsMatched = false;
                    }
                }
            }

            if (!_allowFixes)
            {
                groupBox1.ForeColor = _totalChanged <= 0 ? Color.Red : Color.Green;
                groupBox1.Text = string.Format("Total Found: {0}", _totalChanged);
                /*this.listViewFixes.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                this.listViewFixes.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);*/
                //Application.DoEvents();
            }
            listViewFixes.EndUpdate();
            this.Refresh();
        }

        private void AddFixToListView(Paragraph p, string before, string after)
        {
            var item = new ListViewItem() { Checked = true, UseItemStyleForSubItems = true, Tag = p };
            var subItem = new ListViewItem.ListViewSubItem(item, p.Number.ToString());
            item.SubItems.Add(subItem);

            if (_moodsMatched && _namesMatched)
                subItem = new ListViewItem.ListViewSubItem(item, "Name & Mood");
            else if (_moodsMatched && !_namesMatched)
                subItem = new ListViewItem.ListViewSubItem(item, "Mood");
            else if (_namesMatched && !_moodsMatched)
                subItem = new ListViewItem.ListViewSubItem(item, "Name");

            item.SubItems.Add(subItem);
            subItem = new ListViewItem.ListViewSubItem(item, before.Replace(Environment.NewLine,
                Configuration.ListViewLineSeparatorString));
            item.SubItems.Add(subItem);
            subItem = new ListViewItem.ListViewSubItem(item, after.Replace(Environment.NewLine,
                Configuration.ListViewLineSeparatorString));
            item.SubItems.Add(subItem);

            if (after.Replace(Environment.NewLine, string.Empty).Length != after.Length)
            {
                int idx = after.IndexOf(Environment.NewLine, System.StringComparison.Ordinal);
                if (idx > 2)
                {
                    string firstLine = after.Substring(0, idx).Trim();
                    string secondLine = after.Substring(idx).Trim();
                    int idx1 = firstLine.IndexOf(":", System.StringComparison.Ordinal);
                    int idx2 = secondLine.IndexOf(":", System.StringComparison.Ordinal);
                    if (idx1 > 0xE || idx2 > 0xE)
                    {
                        item.BackColor = Color.Pink;
                    }
                }
            }
            else
            {
                if (after.IndexOf(":", System.StringComparison.Ordinal) > 0xE)
                    item.BackColor = Color.Pink;
            }

            listViewFixes.Items.Add(item);
        }
        private string ChangeMoodsToUppercase(string text, Paragraph p)
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

        private DialogResult PrintErrorMessage(Paragraph p)
        {
            var diagResult = MessageBox.Show(string.Format("Error while reading Line#: {0}", p.Number.ToString()),
                "Error!!!", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Error);
            return diagResult;
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
                    text = _regexFirstChar.Replace(text, x => x.Value.ToUpper()); // foobar to Foobar
                    break;
                case HIStyle.UpperCase:
                    text = text.ToUpper();
                    break;
                case HIStyle.LowerCase:
                    text = text.ToLower();
                    break;
            }

            if (text != before)
                _moodsMatched = true;
            return text;
        }

        //delegate void RefAction<in T>(ref T obj);
        private string NarratorToUpper(string text)
        {
            string before = text;
            var t = Utilities.RemoveHtmlTags(text, true);
            var index = t.IndexOf(':');

            // like: "Ivandro Says:"
            if (index == t.Length - 1 || text.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                return text;
            }

            Func<string, string> ToUpper = (string s) =>
            {
                string pre = s.Substring(0, index);

                // (Adele: ...)
                if (pre.Contains("(") || pre.Contains("[") || pre.Contains("{") || s.StartsWith("http"))
                    return s;

                if (Utilities.RemoveHtmlTags(pre, true).Trim().Length > 1)
                {
                    // Do not change HTML tags to upper
                    string firstChr = Regex.Match(pre, "(?<!<)\\w", RegexOptions.Compiled).Value;
                    int idx = pre.IndexOf(firstChr, StringComparison.Ordinal);
                    if (idx > -1)
                    {
                        string narrator = pre.Substring(idx, index - idx);
                        // You don't want to change http to uppercase :)!
                        if (narrator.Trim().EndsWith("HTTPS", StringComparison.OrdinalIgnoreCase) || narrator.Trim().EndsWith("HTTP", StringComparison.OrdinalIgnoreCase))
                            return s;

                        narrator = narrator.ToUpper();
                        if (narrator == narrator.ToLower())
                            return s;

                        if (narrator.Contains("<"))
                            narrator = FixUpperTagInNarrator(narrator);
                        pre = pre.Remove(idx, index - idx).Insert(idx, narrator);
                        if (pre.Contains("<"))
                            pre = FixUpperTagInNarrator(pre);
                        s = s.Remove(0, index).Insert(0, pre);
                    }
                }
                return s;
            };

            if (text.IndexOf(Environment.NewLine, StringComparison.Ordinal) > -1)
            {
                var lines = text.Replace(Environment.NewLine, "\n").Split('\n');
                for (int i = 0; i < lines.Length; i++)
                {
                    var noTagText = Utilities.RemoveHtmlTags(lines[i], true).Trim();
                    index = noTagText.IndexOf(":");

                    if ((index + 1 < noTagText.Length - 1) && char.IsDigit(noTagText[index + 1]))
                        continue;

                    // Ivandro ismael:
                    // hello world!
                    if (i > 0 && index == noTagText.Length - 1)
                        continue;

                    if (index > 0)
                    {
                        index = lines[i].IndexOf(':');
                        if (index > 0)
                        {
                            lines[i] = ToUpper(lines[i]);
                        }
                    }
                }
                text = string.Join(Environment.NewLine, lines);
            }
            else
            {
                index = text.IndexOf(':');
                if (index > 0)
                {
                    text = ToUpper(text);
                }
            }

            if (before != text)
                _namesMatched = true;
            return text;
        }

        private string FixUpperTagInNarrator(string narrator)
        {
            // Fix uppercase tags
            int tagIndex = narrator.IndexOf('<');
            while (tagIndex > -1)
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
            _allowFixes = true;
            listViewFixes.Resize -= listViewFixes_Resize;

            FindHearingImpairedText();
            RemoveDeletedParagraphs();

            _allowFixes = false;
            FindHearingImpairedText();
            listViewFixes.Resize += listViewFixes_Resize;
            Cursor = Cursors.Default;
        }

        private void RemoveDeletedParagraphs()
        {
            if (_deleteLine && _listDeletedParagraphs.Count > 0)
            {
                for (int i = 0; i < _listDeletedParagraphs.Count; i++)
                {
                    var p = _listDeletedParagraphs[i];
                    _subtitle.Paragraphs.Remove(p);
                }
                _listDeletedParagraphs.Clear();
            }
        }

        private void checkBoxRemoveSpaces_CheckedChanged(object sender, EventArgs e)
        {
            if (listViewFixes.Items.Count < 1 || _subtitle.Paragraphs.Count == 0)
                return;

            _allowFixes = false;
            FindHearingImpairedText();
        }

        private void PluginForm_Shown(object sender, EventArgs e)
        {
            FindHearingImpairedText();
        }

        private bool AllowFix(Paragraph p)
        {
            if (!_allowFixes)
                return false;
            var ln = p.Number.ToString();
            foreach (ListViewItem item in listViewFixes.Items)
            {
                if (item.SubItems[1].Text == ln)
                    return item.Checked;
            }
            return false;
        }

        private void listViewFixes_Resize(object sender, EventArgs e)
        {
            var newWidth = (listViewFixes.Width - (listViewFixes.Columns[0].Width + listViewFixes.Columns[1].Width + listViewFixes.Columns[2].Width)) / 2;
            listViewFixes.Columns[3].Width = newWidth;
            listViewFixes.Columns[4].Width = newWidth;
        }
    }
}