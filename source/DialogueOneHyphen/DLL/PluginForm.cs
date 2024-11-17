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
            if (_fixedParagraphs?.Count > 0)
            {
                foreach (ListViewItem item in listViewFixes.Items)
                {
                    var p = (Paragraph)item.Tag;
                    if (item.Checked && _fixedParagraphs.ContainsKey(p.ID))
                    {
                        p.Text = _fixedParagraphs[p.ID];
                    }
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
                var text = p.Text.TrimStart();
                // <i>- You delusional?
                // - I counted.</i>
                string oldText = text;
                if (checkBoxStripAll.Checked)
                {
                    text = RemoveHyphens.RemoveAllHyphens(text);
                }
                else
                {
                    text = RemoveHyphens.RemoveHyphenBeginningOnly(text);
                }
                if (text != oldText)
                {
                    if (text.StartsWith("<i> "))
                        text = text.Remove(3, 1);
                    text = text.Replace("  ", " ");
                    text = text.Replace(" " + Environment.NewLine, Environment.NewLine);
                    text = text.Replace(Environment.NewLine + " ", Environment.NewLine);
                    text = text.Replace(Environment.NewLine + " ", Environment.NewLine).Trim();
                    _totalFixes++;
                    _fixedParagraphs.Add(p.ID, text);
                    AddFixToListView(p, fixAction, oldText, text);
                }
            }

            listViewFixes.EndUpdate();
            labelTotal.Text = "Total: " + _totalFixes.ToString();
            labelTotal.ForeColor = _totalFixes > 0 ? Color.Blue : Color.Red;
        }

        private string RemoveExtraSpaces(string text, int index)
        {
            return text.Substring(0, index).Trim() + " " + text.Substring(index).TrimStart();
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

        private bool SubtitleLoaded() => _subtitle?.Paragraphs.Count > 0;

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

        private void PluginForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }
    }
}