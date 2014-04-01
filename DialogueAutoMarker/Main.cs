using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public partial class Main : Form
    {
        public string FixedSubtitle { get; private set; }
        private Subtitle _subtitle;
        private Form _parentForm;
        private string _name;
        private string _description;

        public Main()
        {
            InitializeComponent();
        }

        public Main(Form parentForm, Subtitle sub, string name, string description)
            : this()
        {
            // TODO: Complete member initialization
            this._parentForm = parentForm;
            this._subtitle = sub;
            this._name = name;
            this._description = description;
            this.FindDialogue();
        }

        private void FindDialogue()
        {
            foreach (Paragraph p in _subtitle.Paragraphs)
            {
                if (AnalyzeText(p.Text))
                {
                    AddToListDialogue(p, p.Text);
                }
            }
            if (_subtitle.Paragraphs.Count > 0)
                this.textBox1.Text = _subtitle.Paragraphs[0].Text;
        }

        private bool AnalyzeText(string text)
        {
            if (text.Replace(Environment.NewLine, string.Empty).Length == text.Length)
                return false;

            var s = Utilities.RemoveHtmlTags(text);
            if (s.StartsWith("-") && s.Contains(Environment.NewLine + "-"))
                return false;
            return true;
        }

        private void AddToListDialogue(Paragraph p, string after)
        {
            // Checked
            ListViewItem item = new ListViewItem(string.Empty) { Checked = true, Tag = p };

            // Line #
            var subItem = new ListViewItem.ListViewSubItem(item, p.Number.ToString());
            item.SubItems.Add(subItem);

            // Before
            subItem = new ListViewItem.ListViewSubItem(item, p.Text.Replace(Environment.NewLine, "<br />"));
            item.SubItems.Add(subItem);

            // After
            subItem = new ListViewItem.ListViewSubItem(item, after.Replace(Environment.NewLine, "<br />"));
            item.SubItems.Add(subItem);

            // Add to listView
            this.listViewDialogue.Items.Add(item);
        }

        private void listViewDialogue_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            // this event will occur two times!
            if (e.IsSelected)
            {
                var paragraph = e.Item.Tag as Paragraph;
                if (paragraph != null)
                {
                    this.textBox1.Text = paragraph.Text;
                    this.textBox1.Tag = e.Item.Tag;
                }
            }
            else
            {
                if (e.Item.Text != this.textBox1.Text)
                {
                    var p = e.Item.Tag as Paragraph;
                    p.Text = textBox1.Text;
                    e.Item.SubItems[3].Text = p.Text;
                }
            }
        }

        private void buttonDash_Click(object sender, EventArgs e)
        {
            AddDash(2);
        }


        private void buttonSingleDash_Click(object sender, EventArgs e)
        {
            AddDash(1);
        }

        private void AddDash(int num)
        {
            if (num < 1)
                return;
            string text = textBox1.Text.Trim();

            if (num < 1)
                if (!string.IsNullOrEmpty(text))
                {
                    string temp = Utilities.RemoveHtmlTags(text).Trim();

                    if (temp.StartsWith("-"))
                    {

                        text = Regex.Replace(text, @"\B-\B", string.Empty);
                        text = Regex.Replace(text, Environment.NewLine + @"\B-\B", Environment.NewLine);
                    }
                }

            if (num == 1)
            {
                text = "- " + text;
                this.textBox1.Text = text;
            }
            else if (num == 2)
            {


                text = "- " + text.Replace(Environment.NewLine, Environment.NewLine + "- ");
                this.textBox1.Text = text;

                //if (text != old)
                //{
                //    ((Paragraph)textBox1.Tag).Text = text;
                //}
            }
        }

        private void buttonItalic_Click(object sender, EventArgs e)
        {
            if (IsThereText())
            {
                string text = this.textBox1.Text.Trim();
                // TODO: Use regex to remove italic tags
                text = text.Replace("<i>", string.Empty);
                text = text.Replace("</i>", string.Empty);

                text = "<i>" + text + "</i>";
                this.textBox1.Text = text;
            }
        }

        private void buttonPrev_Click(object sender, EventArgs e)
        {
            if (_subtitle.Paragraphs.Count > 0)
            {
                int firstSelectedIndex = 1;
                if (listViewDialogue.SelectedItems.Count > 0)
                    firstSelectedIndex = listViewDialogue.SelectedItems[0].Index;

                firstSelectedIndex--;
                //Paragraph p = _subtitle.GetParagraphOrDefault(firstSelectedIndex);
                //if (p != null)
                SelectIndexAndEnsureVisible(firstSelectedIndex);
            }
        }

        private void buttonNext_Click(object sender, EventArgs e)
        {
            if (_subtitle.Paragraphs.Count > 0)
            {
                int firstSelectedIndex = 0;
                if (listViewDialogue.SelectedItems.Count > 0)
                    firstSelectedIndex = listViewDialogue.SelectedItems[0].Index;

                firstSelectedIndex++;
                //Paragraph p = _subtitle.GetParagraphOrDefault(firstSelectedIndex);
                //if (p != null)
                SelectIndexAndEnsureVisible(firstSelectedIndex);
            }

            /*
            int index = 0;
            if (listViewDialogue.Visible && listViewDialogue.Items.Count > 0)
            {
                index = listViewDialogue.SelectedIndices[0];
                index = index + 1;
                this.listViewDialogue.Items[index].Selected = true;
                this.listViewDialogue.Items[index].Focused = true;
                this.listViewDialogue.Items[index].EnsureVisible();
            }
             */
        }

        public void SelectIndexAndEnsureVisible(int index)
        {
            this.listViewDialogue.Focus();
            SelectIndexAndEnsureVisible(index, false);
        }

        public void SelectIndexAndEnsureVisible(int index, bool focus)
        {
            if (index < 0 || index >= listViewDialogue.Items.Count || listViewDialogue.Items.Count == 0)
                return;
            if (listViewDialogue.TopItem == null)
                return;

            int bottomIndex = listViewDialogue.TopItem.Index + ((listViewDialogue.Height - 25) / 16);
            int itemsBeforeAfterCount = ((bottomIndex - listViewDialogue.TopItem.Index) / 2) - 1;
            if (itemsBeforeAfterCount < 0)
                itemsBeforeAfterCount = 1;

            int beforeIndex = index - itemsBeforeAfterCount;
            if (beforeIndex < 0)
                beforeIndex = 0;

            int afterIndex = index + itemsBeforeAfterCount;
            if (afterIndex >= listViewDialogue.Items.Count)
                afterIndex = listViewDialogue.Items.Count - 1;

            SelectNone();
            if (listViewDialogue.TopItem.Index <= beforeIndex && bottomIndex > afterIndex)
            {
                listViewDialogue.Items[index].Selected = true;
                listViewDialogue.Items[index].EnsureVisible();
                if (focus)
                    listViewDialogue.Items[index].Focused = true;
                return;
            }

            listViewDialogue.Items[beforeIndex].EnsureVisible();
            listViewDialogue.EnsureVisible(beforeIndex);
            listViewDialogue.Items[afterIndex].EnsureVisible();
            listViewDialogue.EnsureVisible(afterIndex);
            listViewDialogue.Items[index].Selected = true;
            listViewDialogue.Items[index].EnsureVisible();
            if (focus)
                listViewDialogue.Items[index].Focused = true;
        }

        public void SelectNone()
        {
            if (listViewDialogue.SelectedItems == null)
                return;
            foreach (ListViewItem item in listViewDialogue.SelectedItems)
                item.Selected = false;
        }

        private bool IsThereText()
        {
            string text = this.textBox1.Text.Trim();
            if (!string.IsNullOrEmpty(text))
            {
                text = Utilities.RemoveHtmlTags(text);
                if (text.Length > 0)
                {
                    return true;
                }
            }
            return false;
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            this.FixedSubtitle = _subtitle.ToText(new SubRip());
            if (!string.IsNullOrEmpty(FixedSubtitle.Trim()))
            {
                DialogResult = System.Windows.Forms.DialogResult.OK;
            }
            DialogResult = DialogResult.Cancel;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }

        private void buttonClear_Click(object sender, EventArgs e)
        {
            string text = this.textBox1.Text;
            if (!string.IsNullOrEmpty(text))
            {
                text = Utilities.RemoveHtmlTags(text).Trim();

                if (text.Length > 2)
                {
                    text = Regex.Replace(text, @"\B-\B", string.Empty).Trim();

                    while (text.Contains(Environment.NewLine + " "))
                    {
                        text = text.Replace(Environment.NewLine + " ", Environment.NewLine);
                    }
                    this.textBox1.Text = text.Trim();
                }
            }
        }
    }

}