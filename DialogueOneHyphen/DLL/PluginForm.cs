using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.PluginLogic
{
    internal partial class PluginForm : Form
    {
        public string FixedSubtitle { get; private set; }
        private Subtitle _subtitle;
        private int _totalFixes = 0;
        private Dictionary<string, string> _fixedParagraphs;

        public PluginForm(Subtitle subtitle, string name, string description)
        {
            InitializeComponent();

            Text = name;
            labelDescription.Text = description;
            _subtitle = subtitle;
            Resize += delegate
            {
                int idx = listViewFixes.Columns.Count - 1;
                listViewFixes.Columns[idx].Width = -2;
            };
            FindDialogueAndListFixes();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (_fixedParagraphs != null && _fixedParagraphs.Count > 0)
            {
                foreach (ListViewItem item in listViewFixes.Items)
                {
                    var p = item.Tag as Paragraph;
                    if (!item.Checked || p == null || !_fixedParagraphs.ContainsKey(p.ID))
                        continue;

                    p.Text = _fixedParagraphs[p.ID];
                }
                FixedSubtitle = _subtitle.ToText();
                DialogResult = DialogResult.OK;
            }
            else
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void AddFixToListView(Paragraph p, string action, string before, string after)
        {
            var item = new ListViewItem(string.Empty) { Checked = true, Tag = p };
            item.SubItems.Add(p.Number.ToString());
            item.SubItems.Add(action);
            item.SubItems.Add(before.Replace(Environment.NewLine, Configuration.ListViewLineSeparatorString));
            item.SubItems.Add(after.Replace(Environment.NewLine, Configuration.ListViewLineSeparatorString));
            listViewFixes.Items.Add(item);
        }

        private void FindDialogueAndListFixes()
        {
            var fixAction = !checkBoxStripAll.Checked ? "Remove first hyphen in dialogues" : "Remove all beginning hyphens/dashes";
            listViewFixes.BeginUpdate();
            listViewFixes.Items.Clear();
            _totalFixes = 0;
            _fixedParagraphs = new Dictionary<string, string>();

            for (int i = 0; i < _subtitle.Paragraphs.Count; i++)
            {
                var p = _subtitle.Paragraphs[i];
                var text = p.Text.Trim();

                if (AnalyzeText(text))
                {
                    var prev = _subtitle.GetParagraphOrDefault(i - 1);
                    if (prev == null || !Utilities.RemoveHtmlTags(prev.Text).TrimEnd().EndsWith('-'))
                    {
                        // <i>- You delusional?
                        //- I counted.</i>
                        int index = text.IndexOf('-');
                        if (index >= 0)
                        {
                            string oldText = text;
                            text = RemoveHyphens(text, checkBoxStripAll.Checked);

                            if (text[index] == 0x14)
                                text = RemoveExtraSpaces(text, index); //<i> Word => <i>Word

                            if (text != oldText)
                            {
                                text = text.Replace(" " + Environment.NewLine, Environment.NewLine);
                                text = text.Replace(Environment.NewLine + " ", Environment.NewLine);
                                text = text.Replace(Environment.NewLine + " ", Environment.NewLine).Trim();
                                _totalFixes++;
                                // remove html tags before adding to listview
                                _fixedParagraphs.Add(p.ID, text);
                                text = Utilities.RemoveHtmlTags(text, true);
                                oldText = Utilities.RemoveHtmlTags(oldText, true);
                                AddFixToListView(p, fixAction, oldText, text);
                            }
                        }
                    }
                }
            }

            listViewFixes.EndUpdate();
            labelTotal.Text = "Total: " + _totalFixes.ToString();
            labelTotal.ForeColor = _totalFixes > 0 ? Color.Blue : Color.Red;
        }

        private string RemoveHyphens(string text, bool bothLines)
        {
            var lines = text.SplitToLines();
            for (int i = 0; i < lines.Length; i++)
            {
                // Todo: check if line was ended with: ., !, ?.
                var removeHyphen = Utilities.RemoveHtmlTags(lines[i], true).StartsWith('-');
                if (removeHyphen)
                {
                    var hyphenIdx = lines[i].IndexOf('-');
                    lines[i] = lines[i].Remove(hyphenIdx, 1);
                }

                // Quit at first loop
                if (!bothLines)
                    break;
            }
            return string.Join(Environment.NewLine, lines);
        }

        private string RemoveExtraSpaces(string text, int index)
        {
            var temp = text.Substring(0, index).Trim();
            text = text.Substring(index).TrimStart();
            return temp + " " + text;
        }

        private bool AnalyzeText(string s)
        {
            s = Utilities.RemoveHtmlTags(s, true);
            s = s.Replace("  ", " ");
            s = s.Replace(Environment.NewLine + " ", Environment.NewLine);
            return checkBoxStripAll.Checked ? s.StartsWith('-') || s.Contains(Environment.NewLine + "-") : s.StartsWith('-') && s.Contains(Environment.NewLine + "-");
        }

        private void buttonSelectAll_Click(object sender, EventArgs e)
        {
            if (!SubtitleLoaded())
                return;
            DoSelection(true);
        }

        private void buttonInverseSelection_Click(object sender, EventArgs e)
        {
            if (!SubtitleLoaded())
                return;
            DoSelection(false);
        }

        private bool SubtitleLoaded()
        {
            if (_subtitle == null || _subtitle.Paragraphs.Count < 1)
                return false;
            return true;
        }

        private void DoSelection(bool selectAll)
        {
            listViewFixes.BeginUpdate();
            foreach (ListViewItem item in listViewFixes.Items)
                item.Checked = selectAll ? selectAll : !item.Checked;
            listViewFixes.EndUpdate();
            Refresh();
        }

        private void checkBoxStripAll_CheckedChanged(object sender, EventArgs e)
        {
            // Todo: Warn the user that this action is irreversible!
            FindDialogueAndListFixes();
        }
    }
}