using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.PluginLogic
{
    internal partial class PluginForm : Form
    {
        // TODO: MAKE THIS LOOK AROUND PATTERN
        private const string REGEXHEARINGIMPAIRED = @"\B([\{\(\[])\s*([♪'""#]*\w+[\s\w,'""#\-&,♪/<>\.]*)([\)\]\}])\B|\B([\{\(\[])([♪'""#\s]*\w+[\s\w,'""#\-&,♪/<>\.]*[^\)\]\}]\r\n['""#\s]*\w+[\s\w,'""#\-&,♪/<>\.]*)\s*([\)\]\}])\B";

        private readonly IList<string> _listPatternNames = new List<string>
		{
            @"(?(?<=[\->\.!\?♪])\s*)\b\w+[\s\w'""\-#♪]*\s([\{\(\[])(['""#\-♪]*\s*\w+[\s\w\-',#""&\.:♪]*)([\)\]\}])(?=:)",
			@"(?(?<=[>\.!\?♪] )|)\b\w+[\s\w'""\-#&]*:(?=\s)",
            @"(?<=\r\n)\b\w+[\s\w'""\-#&♪]*:(?=\s)",
            @"(?i)(mrs|mr)?\.\s*(\w+[\s\w'""\-#&♪]*):(?=\s)" // TODO
		};

        private bool _allowFixes = false;
        private bool _deleteLine = false;
        private HIStyle _hiStyle = HIStyle.UpperCase;
        private bool _moodsMatched;
        private bool _namesMatched;
        private Form _parentForm;
        private Subtitle _subtitle;
        private int _totalChanged;

        public PluginForm(Subtitle subtitle, string name, string description, Form parentForm)
        {
            InitializeComponent();
            this._parentForm = parentForm;
            this._subtitle = subtitle;
            label1.Text = "Description: " + description;
            FindHearinImpairedText();
        }

        private enum HIStyle { UpperCase, LowerCase, FirstUppercase, UpperLowerCase }

        internal string FixedSubtitle { get; private set; }

        private void PluginForm_Load(object sender, EventArgs e)
        {
            //SizeLastColumn();
            comboBox1.SelectedIndex = 0;
            this.Resize += (s, arg) =>
            {
                listViewFixes.Columns[listViewFixes.Columns.Count - 1].Width = -2;
                //this.listViewFixes.Columns.Count -1
            };
        }

        private void btn_Cancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void btn_Run_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            _allowFixes = true;
            FindHearinImpairedText();
            if (_deleteLine)
            {
                foreach (ListViewItem item in this.listViewFixes.Items)
                {
                    if (item.BackColor != Color.Red)
                        continue;
                    _subtitle.RemoveLine(((Paragraph)item.Tag).Number);
                }
            }

            FixedSubtitle = _subtitle.ToText(new SubRip());
            //Cursor = Cursors.Default;
            DialogResult = DialogResult.OK;
        }

        private void checkBoxNarrator_CheckedChanged(object sender, EventArgs e)
        {
            if (_subtitle.Paragraphs.Count <= 0)
                return;
            listViewFixes.Items.Clear();
            FindHearinImpairedText();
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
                string text = (listViewFixes.FocusedItem.Tag as Paragraph).ToString();
                Clipboard.SetText(text);
            }
            else
            {
                if (listViewFixes.FocusedItem.BackColor != Color.Red)
                {
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
                listViewFixes.Items.Clear();
                FindHearinImpairedText();
            }
        }

        private void FindHearinImpairedText()
        {
            //char _char = '\u0028'; // (
            _totalChanged = 0;
            listViewFixes.BeginUpdate();
            foreach (Paragraph p in _subtitle.Paragraphs)
            {
                if (Regex.IsMatch(p.Text, REGEXHEARINGIMPAIRED, RegexOptions.Compiled) || Regex.IsMatch(p.Text, ":\\B"))
                {
                    string oldText = p.Text;
                    string text = p.Text;

                    // (Moods and feelings)
                    if (Regex.IsMatch(p.Text, REGEXHEARINGIMPAIRED, RegexOptions.Compiled))
                        text = ConvertMoodsFeelings(text);

                    if (Regex.IsMatch(text, _listPatternNames[0], RegexOptions.Compiled))
                    {
                        // WOMAN (on phone): => WOMAN ON PHONE:
                        // <i>Ella (on phone): It's not about money.</i>
                        text = Regex.Replace(text, _listPatternNames[0], delegate(Match match)
                        {
                            string brackets = @"[\(\[\{]|[\)\]\}]";
                            string newname = Regex.Replace(match.Value, brackets, string.Empty);
                            return newname.ToUpper();
                        });
                    }

                    // Narrator: switch ♪ :
                    if (checkBoxNames.Checked && Regex.IsMatch(text, @"\b[A-Z]\w+:\B"))
                        text = NamesOfPeoples(text);

                    if (text != oldText)
                    {
                        text = text.Replace(" " + Environment.NewLine, Environment.NewLine).Trim();
                        text = text.Replace(Environment.NewLine + " ", Environment.NewLine).Trim();

                        if (AllowFix(p))
                        {
                            p.Text = text;
                        }
                        else
                        {
                            if (!_allowFixes)
                            {
                                oldText = Utilities.RemoveHtmlTags(oldText);
                                text = Utilities.RemoveHtmlTags(text);
                                AddFixToListView(p, oldText, text);
                                _totalChanged++;
                            }
                        }
                    }
                    _namesMatched = false;
                    _moodsMatched = false;
                }
            }

            if (!_allowFixes)
            {
                groupBox1.ForeColor = _totalChanged <= 0 ? Color.Red : Color.Green;
                groupBox1.Text = "Total Found: " + _totalChanged;
            }
            listViewFixes.EndUpdate();
        }

        private bool AllowFix(Paragraph p)
        {
            if (!_allowFixes)
                return false;
            string ln = p.Number.ToString();
            foreach (ListViewItem item in listViewFixes.Items)
            {
                if (item.SubItems[1].Text == ln)
                    return item.Checked;
            }
            return false;
        }

        private void AddFixToListView(Paragraph p, string before, string after)
        {
            var item = new ListViewItem() { Checked = true, UseItemStyleForSubItems = true };
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
            item.Tag = p; // save paragraph in Tag
            // Note: sinde the 'after' doesn't contain <html tags> it safe to check this way!
            if (after.IndexOf(": ") > 14 || after.IndexOf(":\r\n") > 14)
                item.BackColor = Color.Pink;
            listViewFixes.Items.Add(item);
        }

        private string ConvertMoodsFeelings(string text)
        {
            if (!Regex.IsMatch(text, REGEXHEARINGIMPAIRED))
                return text;

            string before = text;
            string helper = string.Empty;
            switch (_hiStyle)
            {
                case HIStyle.UpperLowerCase:
                    text = Regex.Replace(text, REGEXHEARINGIMPAIRED, delegate(Match match)
                    {
                        helper = string.Empty;
                        bool isUpperTime = true;
                        foreach (char myChar in match.Value)
                        {
                            helper += isUpperTime ? char.ToUpper(myChar) : char.ToLower(myChar);
                            isUpperTime = !isUpperTime;
                        }
                        return helper;
                    }, RegexOptions.Compiled);
                    break;

                case HIStyle.FirstUppercase:
                    text = Regex.Replace(text, REGEXHEARINGIMPAIRED, delegate(Match match)
                    {
                        helper = match.Value.ToLower();
                        helper = Regex.Replace(helper, @"\b\w", x => x.Value.ToUpper());
                        return helper;
                    }, RegexOptions.Compiled);
                    break;

                case HIStyle.UpperCase:
                    text = Regex.Replace(text, REGEXHEARINGIMPAIRED, delegate(Match match)
                    {
                        return match.Value.ToUpper();
                    }, RegexOptions.Compiled);
                    break;

                case HIStyle.LowerCase:
                    text = Regex.Replace(text, REGEXHEARINGIMPAIRED, delegate(Match match) { return match.Value.ToLower(); }, RegexOptions.Compiled);
                    break;
            }

            if (text != before)
                _moodsMatched = true;
            return text;
        }

        private string NamesOfPeoples(string text)
        {
            foreach (string pattern in _listPatternNames)
            {
                if (Regex.IsMatch(text, pattern))
                {
                    _namesMatched = true;
                    text = Regex.Replace(text, pattern, delegate(Match match)
                    {
                        // Note this will not match ♪: swap : ♪
                        if (Regex.IsMatch(match.Value, @"\b[A-Z]\w+:\B", RegexOptions.Compiled))
                            return match.Value.ToUpper();
                        else
                            return match.Value;
                    });
                    break;
                }
            }
            return text;
        }
    }
}