using System;
using System.Drawing;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.PluginLogic
{
    internal partial class PluginForm : Form
    {
        internal string FixedSubtitle { get; private set; }
        private Subtitle _subtitle;
        private int _totalFixes = 0;
        private bool _allowFixes = false;

        internal PluginForm(Subtitle subtitle, string name, string description)
        {
            InitializeComponent();

            this.Text = name;
            labelDescription.Text = description;
            _subtitle = subtitle;
            this.Resize += delegate
            {
                int idx = listViewFixes.Columns.Count - 1;
                this.listViewFixes.Columns[idx].Width = -2;
            };
            FindDialogueAndListFixes();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            _allowFixes = true;
            FindDialogueAndListFixes();
            FixedSubtitle = _subtitle.ToText(new SubRip());
            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void AddFixToListView(Paragraph p, string action, string before, string after)
        {
            var item = new ListViewItem(string.Empty) { Checked = true };

            var subItem = new ListViewItem.ListViewSubItem(item, p.Number.ToString());
            item.SubItems.Add(subItem);
            subItem = new ListViewItem.ListViewSubItem(item, action);
            item.SubItems.Add(subItem);
            subItem = new ListViewItem.ListViewSubItem(item, before.Replace(Environment.NewLine, "<br />"));
            item.SubItems.Add(subItem);
            subItem = new ListViewItem.ListViewSubItem(item, after.Replace(Environment.NewLine, "<br />"));
            item.SubItems.Add(subItem);

            listViewFixes.Items.Add(item);
        }

        private void FindDialogueAndListFixes()
        {
            string fixAction = "Remove first hyphen in dialogues";
            for (int i = 0; i < _subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = _subtitle.Paragraphs[i];
                string text = p.Text.Trim();

                if (AnalyzeText(text))
                {
                    Paragraph prev = _subtitle.GetParagraphOrDefault(i - 1);
                    if (prev == null || !Utilities.RemoveHtmlTags(prev.Text).Trim().EndsWith('-'))
                    {
                        // <i>- You delusional?
                        //- I counted.</i>
                        int index = text.IndexOf('-');
                        if (index >= 0)
                        {
                            string oldText = text;
                            text = text.Remove(index, 1);
                            if (text[index] == 0x14)
                                text = RemoveExtraSpaces(text, index); //<i> Word => <i>Word

                            if (text != oldText)
                            {
                                text = text.Replace(" " + Environment.NewLine, Environment.NewLine).Trim();
                                text = text.Replace(Environment.NewLine + " ", Environment.NewLine).Trim();
                                text = text.Replace(Environment.NewLine + " ", Environment.NewLine).Trim();
                                if (AllowFix(p, fixAction))
                                {
                                    p.Text = text;
                                }
                                else
                                {
                                    if (_allowFixes)
                                        continue;
                                    _totalFixes++;
                                    // remove html tags before adding to listview
                                    text = Utilities.RemoveHtmlTags(text);
                                    oldText = Utilities.RemoveHtmlTags(oldText);
                                    AddFixToListView(p, fixAction, oldText, text);
                                }
                            }
                        }
                    }
                }
            }
            if (!_allowFixes)
            {
                labelTotal.Text = "Total: " + _totalFixes.ToString();
                labelTotal.ForeColor = _totalFixes > 0 ? Color.Blue : Color.Red;
            }
        }

        private string RemoveExtraSpaces(string text, int index)
        {
            var temp = text.Substring(0, index);
            temp = temp.Trim();
            text = text.Remove(0, index).TrimStart();
            text = temp + " " + text;
            return text;
        }

        private bool AllowFix(Paragraph p, string action)
        {
            if (!_allowFixes)
                return false;

            string ln = p.Number.ToString();
            foreach (ListViewItem item in listViewFixes.Items)
            {
                if (item.SubItems[1].Text == ln && item.SubItems[2].Text == action)
                    return item.Checked;
            }
            return false;
        }

        private bool AnalyzeText(string s)
        {
            s = Utilities.RemoveHtmlTags(s).Trim();
            s = s.Replace("  ", " ");

            s = s.Replace(Environment.NewLine + " ", Environment.NewLine);
            return s.StartsWith('-') && s.Contains(Environment.NewLine + "-");
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
            this.Refresh();
        }
    }
}