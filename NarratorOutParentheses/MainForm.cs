using System;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.PluginLogic
{
    internal partial class MainForm : Form
    {
        public string Subtitle { get; private set; }

        private readonly Subtitle _subtitle;
        private bool _allowFixes;

        public MainForm(Subtitle sub, string fileName, string description)
        {
            InitializeComponent();
            Resize += delegate
            {
                listViewFixes.Columns[listViewFixes.Columns.Count - 1].Width = -2;
            };
            listViewFixes.SizeChanged += delegate
            {
                int width = listViewFixes.Width / 2 - 100;
                columnHeaderActual.Width = width;
                columnHeaderAfter.Width = width;
            };
            _subtitle = sub;
            GeneratePreview();
        }

        private static readonly char[] ExpectedChars = { '(', '[' };

        public void GeneratePreview()
        {
            listViewFixes.BeginUpdate();
            listViewFixes.Items.Clear();
            for (int i = 0; i < _subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = _subtitle.Paragraphs[i];
                string text = p.Text;
                string before = text;

                int idx = text.IndexOfAny(ExpectedChars);
                if (idx < 0)
                {
                    continue;
                }

                char tagClose = text[idx] == '(' ? ')' : ']';
                while (idx >= 0)
                {
                    int endIdx = text.IndexOf(tagClose, idx + 1);
                    if (endIdx < idx)
                    {
                        break;
                    }

                    char tagOpen = text[idx];
                    string nameFromSubtitile = text.Substring(idx, endIdx - idx + 1).Trim(tagOpen, ' ', tagClose);
                    if (NameList.Contains(nameFromSubtitile))
                    {
                        // Todo: if name contains <i>: note that there may be italic tag at begining
                        text = text.Remove(idx, endIdx - idx + 1).TrimStart(':', ' ');

                        // add colon ad the end of name if there is none, to indicate the narrator
                        nameFromSubtitile += ": ";

                        // TODO: Title case the name before insering it in paragraph
                        //nameFromSubtitile = [call somethinig to capitalize the text]
                        text = text.Insert(idx, nameFromSubtitile);

                        // look for next
                        idx = text.IndexOf(tagOpen, idx + nameFromSubtitile.Length);
                    }
                    else
                    {
                        idx = text.IndexOf(tagOpen, endIdx + 1);
                    }
                }

                // insert hyphen in both lines
                text = AddHyphenInBothLine(text);

                if (text.Equals(before, StringComparison.Ordinal) == false)
                {
                    if (_allowFixes == false)
                    {
                        AddFixToListView(p, before, text);
                    }
                    else
                    {
                        p.Text = text;
                    }
                }
            }
            listViewFixes.EndUpdate();
        }

        private const string EndLineChars = ".?)]!";

        private string AddHyphenInBothLine(string text)
        {
            if (!text.Contains(Environment.NewLine) || StringUtils.CountTagInText(text, ':') == 0)
            {
                return text;
            }

            string noTagText = HtmlUtils.RemoveTags(text);
            bool addHyphen = false;
            string[] noTagLines = noTagText.SplitToLines();
            for (int i = 0; i < noTagLines.Length; i++)
            {
                string line = noTagLines[i];
                string preLine = i - 1 < 0 ? null : noTagLines[i - 1];
                int idx = line.IndexOf(':');
                // (John): Day's getting on.
                // We got work to do.
                addHyphen = ((idx > 0 && !StringUtils.IsBetweenNumbers(line, idx)) && (preLine == null || EndLineChars.IndexOf(preLine[preLine.Length - 1]) >= 0)) ? true : false;
            }
            /*
            foreach (var noTagLine in noTagText.Replace("\r\n", "\n").Split('\n'))
            {
                if (noTagLine.Length == 0)
                    return text;
                var idx = noTagLine.IndexOf(':');
                addHyphen = (idx >= 0 && !Utilities.IsBetweenNumbers(noTagLine, idx)) && (endLineChars.IndexOf(noTagLine[noTagLine.Length - 1]) >= 0) ? true : false;
            }*/
            if (addHyphen && (noTagLines[0].Length > 2 && noTagLines[1].Length > 2))
            {
                if (noTagLines[0][0] != '-')
                {
                    text = "- " + text;
                }

                if (noTagLines[1][0] != '-')
                {
                    text = text.Insert(text.IndexOf(Environment.NewLine) + Environment.NewLine.Length, "- ");
                }
            }
            return text;
        }

        private void buttonToNarrator_Click(object sender, EventArgs e)
        {
            string name = textBoxName.Text.Trim();
            // invalid name
            if (!name.ContainsLetter())
            {
                return;
            }
            int preLen = NameList.ListNames.Count;
            NameList.AddNameToList(name);
            if (preLen != NameList.ListNames.Count)
            {
                GeneratePreview();
            }
        }

        private void AddFixToListView(Paragraph p, string before, string after)
        {
            ListViewItem item = new ListViewItem { Checked = true, UseItemStyleForSubItems = true, Tag = p };
            item.SubItems.Add(p.Number.ToString());
            item.SubItems.Add(before.Replace(Environment.NewLine, Options.UILineBreak));
            item.SubItems.Add(after.Replace(Environment.NewLine, Options.UILineBreak));
            listViewFixes.Items.Add(item);
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            _allowFixes = true;
            Applyfixes();
            Subtitle = _subtitle.ToText();
        }

        private void Applyfixes()
        {
            const int ColumnFixedText = 3;
            foreach (ListViewItem lvi in listViewFixes.Items)
            {
                if (!lvi.Checked)
                {
                    continue;
                }
                var p = (Paragraph)lvi.Tag;
                p.Text = lvi.SubItems[ColumnFixedText].Text.Replace(Options.UILineBreak, Environment.NewLine);
            }
        }

        private void buttonGetNames_Click(object sender, EventArgs e)
        {
            // store the names in list on constructor runtime instead of loading it each time
            using (GetNamesForm formGetName = new GetNamesForm(this, _subtitle)) // send the loaded list
            {
                if (formGetName.ShowDialog(this) == DialogResult.OK)
                {
                    GeneratePreview();
                }
            }
        }

        private void buttonApply_Click(object sender, EventArgs e)
        {
            Applyfixes();
            Subtitle = _subtitle.ToText();
            _allowFixes = false;
            // reload subtitle
            new SubRip().LoadSubtitle(_subtitle, Subtitle.SplitToLines(), string.Empty);
            GeneratePreview();
        }

        private void checkAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem clickedItem = (ToolStripMenuItem)sender;
            switch (clickedItem.Text)
            {
                case "Check all": // Check all
                    foreach (ListViewItem item in listViewFixes.Items)
                    {
                        item.Checked = true;
                    }

                    break;
                case "Uncheck all": // Uncheck all
                    foreach (ListViewItem item in listViewFixes.Items)
                    {
                        item.Checked = false;
                    }

                    break;
                case "Invert check": // Invert check
                    foreach (ListViewItem item in listViewFixes.Items)
                    {
                        item.Checked = !item.Checked;
                    }

                    break;
                case "Copy": // Copy
                    Clipboard.SetText(listViewFixes.SelectedItems[0].Tag.ToString());
                    break;
            }
        }

        private void contextMenuStrip1_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (listViewFixes.Items.Count == 0)
            {
                e.Cancel = true;
            }
        }
    }
}
