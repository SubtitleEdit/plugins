using System;
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
            FindDialogueAndListFixes();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            _allowFixes = true;
            FindDialogueAndListFixes();
            FixedSubtitle = _subtitle.ToText(new SubRip()); ;
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

            item.Tag = p; // save paragraph in Tag

            listViewFixes.Items.Add(item);
        }

        private void FindDialogueAndListFixes()
        {
            string fixAction = "Remove first hyphen in dialogues";
            for (int i = 0; i < _subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = _subtitle.Paragraphs[i];
                string text = p.Text;

                if (AnalyzeText(text))
                {
                    Paragraph prev = _subtitle.GetParagraphOrDefault(i - 1);

                    if (prev == null || !Utilities.RemoveHtmlTags(prev.Text).Trim().EndsWith("-"))
                    {
                        int index = text.IndexOf('-');
                        if (index >= 0)
                        {
                            string oldText = text;
                            text = text.Remove(index, 1);

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
                                    _totalFixes++;
                                    // clean both text before adding them to Listview
                                    text = Utilities.RemoveHtmlTags(text);
                                    oldText = Utilities.RemoveHtmlTags(oldText);
                                    AddFixToListView(p, fixAction, oldText, text);
                                }
                            }
                        }
                    }
                }
            }
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

            if (s.StartsWith("-") && s.Contains(Environment.NewLine + "-"))
                return true;
            return false;
        }
    }
}