using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.PluginLogic
{
    internal enum DialogueType
    {
        SingleDash,
        DoubleDash
    }

    public partial class Main : Form
    {
        public string FixedSubtitle { get; private set; }

        private Subtitle _subtitle;
        private Form _parentForm;
        private string _name;
        private string _description;
        private int lastSelectedIndex = -1;
        private Color backColor;

        public Main()
        {
            InitializeComponent();
        }

        public Main(Form parentForm, Subtitle sub, string name, string description)
            : this()
        {
            // TODO: Complete member initialization
            _parentForm = parentForm;
            _subtitle = sub;
            _name = name;
            _description = description;
            FindDialogue();
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
                textBox1.Text = _subtitle.Paragraphs[0].Text;
        }

        private bool AnalyzeText(string text)
        {
            if (text.Replace(Environment.NewLine, string.Empty).Length == text.Length)
                return false;

            var s = HtmlUtils.RemoveTags(text);
            if (s.StartsWith("-") && s.Contains(Environment.NewLine + "-"))
                return false;
            int index = text.IndexOf(Environment.NewLine);

            if (index > 0)
            {
                string firstLine = text.Substring(0, index).Trim();
                string secondLine = text.Substring(index).Trim();
                firstLine = HtmlUtils.RemoveTags(firstLine).Trim();
                secondLine = HtmlUtils.RemoveTags(secondLine).Trim();
                if (!firstLine.Contains(".") && !firstLine.Contains("!") && !firstLine.Contains("?"))
                { return false; }
            }
            return true;
        }

        private void AddToListDialogue(Paragraph p, string after)
        {
            // Line #
            ListViewItem item = new ListViewItem(p.Number.ToString()) { Tag = p };

            // Before
            var subItem = new ListViewItem.ListViewSubItem(item, p.Text.Replace(Environment.NewLine, "<br />"));
            item.SubItems.Add(subItem);

            // After
            subItem = new ListViewItem.ListViewSubItem(item, after.Replace(Environment.NewLine, "<br />"));
            item.SubItems.Add(subItem);

            // Add to listView
            listViewDialogue.Items.Add(item);
        }

        private void listViewDialogue_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            // this event will occur two times!
            if (e.IsSelected)
            {
                var paragraph = e.Item.Tag as Paragraph;
                if (paragraph != null)
                {
                    textBox1.Text = paragraph.Text;
                    textBox1.Tag = e.Item.Tag;
                    SetLinesLength(textBox1.Text);
                }
            }
            else
            {
                var p = e.Item.Tag as Paragraph;
                if (p.Text != textBox1.Text)
                {
                    p.Text = textBox1.Text;
                    e.Item.SubItems[2].Text = p.Text.Replace(Environment.NewLine, "<br />");
                    e.Item.BackColor = System.Drawing.Color.GreenYellow;
                    //e.Item.ForeColor = System.Drawing.Color.Green;
                }
            }
        }

        private void SetLinesLength(string text)
        {
            string formatTotal = "Total lines length: {0}";
            string formatSingle = "Single line length: {0}";

            if (text.Contains(Environment.NewLine))
            {
                var vector = text.Replace(Environment.NewLine, "\n").Split('\n');
                int totalNewLine = (text.Length - text.Replace(Environment.NewLine, string.Empty).Length);
                string[] lengths = new string[totalNewLine];

                for (int i = 0; i < vector.Length; i++)
                {
                    lengths[i] = vector[i].Length.ToString();
                }

                labelLinesTotalLength.Text = string.Format(formatSingle, string.Join("/", lengths));
                labelSingleLineLength.Text = string.Format(formatTotal, text.Length - 2); // \r\n == 2
            }
            else
            {
                labelLinesTotalLength.Text = string.Format(formatTotal, text.Length);
                labelSingleLineLength.Text = string.Format(formatSingle, text.Length);
            }
        }

        private void buttonLower_Click(object sender, EventArgs e)
        {
            if (IsThereText())
            {
                string selected = textBox1.SelectedText;
                if (!string.IsNullOrEmpty(selected))
                {
                    textBox1.Text = textBox1.Text.Replace(selected, selected.ToLower());
                }
                else
                {
                    string text = textBox1.Text.Trim();
                    if (HtmlUtils.RemoveTags(text).Length > 0)
                        textBox1.Text = text.ToLower();
                }
            }
            // TODO: fix tag chasing before proceed
        }

        private void buttonUpper_Click(object sender, EventArgs e)
        {
            if (IsThereText())
            {
                string selected = textBox1.SelectedText;
                if (!string.IsNullOrEmpty(selected))
                {
                    textBox1.Text = textBox1.Text.Replace(selected, selected.ToUpper());
                }
                else
                {
                    string text = textBox1.Text.Trim();
                    text = text.ToUpper();
                    if (text.Contains("<"))
                        text = FixTag(text);
                    textBox1.Text = text;
                }
            }
        }

        private string FixTag(string text)
        {
            text = Regex.Replace(text, "(?i)(?<=<)/?([ibu])(?=>)", x => x.Groups[1].Value.ToLower());

            if (text.Contains("<FONT"))
            {
                text = text.Replace("</FONT>", "</font>"); // not finished...
                int index = text.IndexOf("<FONT");
                if (index > -1)
                {
                    int closeTag = Math.Max(index + 6, text.IndexOf(">"));

                    while (text.Contains("<FONT"))
                    {
                        text = text.Remove(index, (closeTag - index) + 1);
                        index = text.IndexOf("<FONT");
                        closeTag = Math.Max(index + 6, text.IndexOf(">"));
                    }
                }
            }
            // Todo: this operation may cause <<'  '>>!

            text = text.Replace("  ", " ");
            return text;
        }

        private void buttonDash_Click(object sender, EventArgs e)
        {
            AddDash(DialogueType.DoubleDash);
        }

        private void buttonSingleDash_Click(object sender, EventArgs e)
        {
            AddDash(DialogueType.SingleDash);
        }

        private void AddDash(DialogueType dialogueStyle)
        {
            string text = textBox1.Text.Trim();

            if (text.Replace(Environment.NewLine, string.Empty).Length != text.Length)
            {
                string temp = string.Empty;
                // TODO: REMOVE
                /*
                if (num < 1)
                {
                    if (!string.IsNullOrEmpty(text))
                    {
                        temp = Utilities.RemoveHtmlTags(text).Trim();

                        if (temp.StartsWith("-"))
                        {
                            text = Regex.Replace(text, @"\B-\B", string.Empty);
                            text = Regex.Replace(text, Environment.NewLine + @"\B-\B", Environment.NewLine);
                        }
                    }
                }*/
                switch (dialogueStyle)
                {
                    case DialogueType.SingleDash:
                        // removes the first dash
                        temp = HtmlUtils.RemoveTags(text).Trim();
                        var val = text.Substring(0, text.IndexOf(Environment.NewLine));
                        if (HtmlUtils.RemoveTags(val).Trim().Length > 2 && val.StartsWith("-"))
                        {
                            int index = text.IndexOf("-");
                            text = text.Remove(index, 1);
                            if (text.Length > index && text[index] == 0x20)
                            {
                                text = text.Remove(index, 1);
                            }
                        }

                        // add a new dash
                        if (!temp.Contains(Environment.NewLine + "-"))
                        {
                            text = text.Replace(Environment.NewLine, Environment.NewLine + "- ");
                            textBox1.Text = text;
                        }
                        break;

                    case DialogueType.DoubleDash:
                        temp = HtmlUtils.RemoveTags(text);
                        if (!temp.StartsWith("-"))
                        {
                            text = text.Insert(0, "- ");
                        }

                        if (!temp.Contains(Environment.NewLine + "-"))
                        {
                            text = text.Replace(Environment.NewLine, Environment.NewLine + "- ");
                        }
                        textBox1.Text = text;
                        break;
                }
            }
        }

        private void buttonItalic_Click(object sender, EventArgs e)
        {
            if (IsThereText())
            {
                string selected = textBox1.SelectedText;
                if (!string.IsNullOrEmpty(selected))
                {
                    textBox1.Text = textBox1.Text.Replace(selected, "<i>" + selected + "</i>");
                }
                else
                {
                    string text = textBox1.Text.Trim();
                    // TODO: Use regex to remove italic tags
                    text = text.Replace("<i>", string.Empty);
                    text = text.Replace("</i>", string.Empty);

                    text = "<i>" + text + "</i>";
                    textBox1.Text = text;
                }
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

        private void buttonBold_Click(object sender, EventArgs e)
        {
            if (IsThereText())
            {
                string text = textBox1.Text.Trim();
                // TODO: Use regex to remove italic tags
                text = Regex.Replace(text, "(?i)</?b>", string.Empty);

                text = "<b>" + text + "</b>";
                textBox1.Text = text;
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
            listViewDialogue.Focus();
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
            //if (focus)
            //    listViewDialogue.Items[index].Focused = true;
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
            string text = textBox1.Text.Trim();
            if (!string.IsNullOrEmpty(text))
            {
                text = HtmlUtils.RemoveTags(text);
                if (text.Length > 0)
                {
                    return true;
                }
            }
            return false;
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            buttonNext_Click(null, null);
            FixedSubtitle = _subtitle.ToText();
            if (!string.IsNullOrEmpty(FixedSubtitle.Trim()))
            {
                DialogResult = System.Windows.Forms.DialogResult.OK;
            }
            else
                DialogResult = DialogResult.Cancel;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }

        private void buttonClear_Click(object sender, EventArgs e)
        {
            string text = textBox1.Text;
            if (!string.IsNullOrEmpty(text))
            {
                text = HtmlUtils.RemoveTags(text).Trim();

                if (text.Length > 2)
                {
                    text = Regex.Replace(text, @"\B-\B", string.Empty).Trim();

                    while (text.Contains(Environment.NewLine + " "))
                    {
                        text = text.Replace(Environment.NewLine + " ", Environment.NewLine);
                    }
                    textBox1.Text = text.Trim();
                }
            }
        }

        private void buttonBreakCursorPos_Click(object sender, EventArgs e)
        {
            if (IsThereText())
            {
                int newLineIdx = textBox1.Text.IndexOf(Environment.NewLine);
                int index = textBox1.SelectionStart;
                int len = 0;
                string str = textBox1.Text; ;

                if (index > 1 && index < newLineIdx && index + 1 < textBox1.Text.Length - 1)
                {
                    str = str.Replace(Environment.NewLine, " ");
                    len = str.Length;
                    str = str.Replace("  ", " ");
                    index = (index--) + (len - str.Length);
                    str = str.Substring(0, index).Trim() + Environment.NewLine + str.Substring(index).Trim();
                    textBox1.Text = str;
                }
                else if (index > 1 && index > newLineIdx & index + 1 < textBox1.Text.Length - 1)
                {
                    // ivnadro ismael gomes jao!
                    str = str.Replace(Environment.NewLine, " ");
                    len = str.Length;
                    str = str.Replace("  ", " ");
                    //index--; // this has to be executed if cursor were set in second line!
                    index = (index--) + (len - str.Length);
                    str = str.Substring(0, index).Trim() + Environment.NewLine + str.Substring(index).Trim();
                    textBox1.Text = str;
                }
            }
        }

        private void buttonUnBreak_Click(object sender, EventArgs e)
        {
            if (IsThereText())
            {
                string text = HtmlUtils.RemoveTags(textBox1.Text).Trim();
                if (text.Length > 0 && text.Length < 47)
                {
                    textBox1.Text = textBox1.Text.Replace(Environment.NewLine, " ");
                }
                else
                {
                    MessageBox.Show("You are trying to unbreak line > 46 chars");
                }
            }
        }

        private void listViewDialogue_Leave(object sender, EventArgs e)
        {
            if (listViewDialogue.SelectedItems != null && listViewDialogue.SelectedItems.Count > 0)
            {
                lastSelectedIndex = listViewDialogue.SelectedItems[0].Index;
                //var myColor = new System.Drawing.Color();
                //myColor = System.Drawing.Color.Blue;
                //myColor = Color.FromArgb(255, Color.Blue.R - 10, Color.Blue.G - 100, Color.Blue.B - 100);
                backColor = listViewDialogue.SelectedItems[0].BackColor;
                listViewDialogue.SelectedItems[0].BackColor = Color.CadetBlue;
            }
        }

        private void listViewDialogue_Enter(object sender, EventArgs e)
        {
            if (lastSelectedIndex > -1)
            {
                listViewDialogue.Items[lastSelectedIndex].BackColor = backColor;
                //listViewDialogue.Items[lastSelectedIndex].Selected = true;
                listViewDialogue.EnsureVisible(lastSelectedIndex);
            }
        }

        private void Main_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != (Keys.Down | Keys.Up))
                return;
            listViewDialogue.Focus();
            if (e.KeyCode == Keys.Up)
            {
                buttonPrev_Click(null, EventArgs.Empty);
            }
            else
            {
                buttonNext_Click(null, EventArgs.Empty);
            }
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == (Keys.Control | Keys.Up))
            {
                listViewDialogue.Focus();
                buttonPrev_Click(null, EventArgs.Empty);
            }
            else if (e.KeyData == (Keys.Control | Keys.Down))
            {
                listViewDialogue.Focus();
                buttonNext_Click(null, EventArgs.Empty);
            }
        }
    }
}
