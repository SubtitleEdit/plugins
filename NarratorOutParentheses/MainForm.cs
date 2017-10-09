using Nikse.SubtitleEdit.PluginLogic.Models;
using System;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.PluginLogic
{
    internal partial class MainForm : Form
    {
        private readonly Configs _configs;
        public string FixedSubtitle { get; private set; }

        private Subtitle _subtitle;
        private string _fileName;
        private bool _allowFixes;

        public MainForm(Subtitle sub, Configs configs, string fileName, string description)
        {
            // TODO: Complete member initialization
            InitializeComponent();
            Resize += delegate
            {
                listViewFixes.Columns[listViewFixes.Columns.Count - 1].Width = -2;
            };
            listViewFixes.SizeChanged += delegate
            {
                var width = listViewFixes.Width / 2 - 100;
                columnHeaderActual.Width = width;
                columnHeaderAfter.Width = width;
            };
            _subtitle = sub;
            _fileName = fileName;
            _configs = configs;
            GeneratePreview();
        }

        private static readonly char[] ExpectedChars = { '(', '[' };

        public void GeneratePreview()
        {
            listViewFixes.BeginUpdate();
            for (int i = 0; i < _subtitle.Paragraphs.Count; i++)
            {
                var p = _subtitle.Paragraphs[i];
                var text = p.Text;
                var before = text;

                var idx = text.IndexOfAny(ExpectedChars);
                if (idx < 0)
                    continue;
                char tagClose = text[idx] == '(' ? ')' : ']';
                while (idx >= 0)
                {
                    var endIdx = text.IndexOf(tagClose, idx + 1);
                    if (endIdx < idx)
                        break;
                    char tagOpen = text[idx];
                    var mood = text.Substring(idx, endIdx - idx + 1).Trim(tagOpen, ' ', tagClose);
                    if (NameList.IsInListName(mood))
                    {
                        // Todo: if name contains <i>: note that there may be italic tag at begining
                        text = text.Remove(idx, endIdx - idx + 1).TrimStart(':', ' ');
                        if (text.Length > idx && text[idx] != ':')
                            text = text.Insert(idx, mood + ": ");
                        else
                            text = text.Insert(idx, mood);
                        idx = text.IndexOf(tagOpen, idx);
                    }
                    else
                    {
                        idx = text.IndexOf(tagOpen, endIdx + 1);
                    }
                }
                text = AddHyphenOnBothLine(text);
                if (text != before && !AllowFix(p))
                {
                    // add hyphen is both contains narrator
                    AddFixToListView(p, before, text);
                }
                else
                {
                    p.Text = text;
                }
            }
            listViewFixes.EndUpdate();
        }

        private const string EndLineChars = ".?)]!";
        private string AddHyphenOnBothLine(string text)
        {
            if (!text.Contains(Environment.NewLine) || StringUtils.CountTagInText(text, ':') < 1)
                return text;

            var noTagText = HtmlUtils.RemoveTags(text);
            bool addHyphen = false;
            var noTagLines = noTagText.SplitToLines();
            for (int i = 0; i < noTagLines.Length; i++)
            {
                var line = noTagLines[i];
                var preLine = i - 1 < 0 ? null : noTagLines[i - 1];
                var idx = line.IndexOf(':');
                // (John): Day's getting on.
                // We got work to do.
                addHyphen = ((idx >= 0 && !StringUtils.IsBetweenNumbers(line, idx)) && (preLine == null || EndLineChars.IndexOf(preLine[preLine.Length - 1]) >= 0)) ? true : false;
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
                    text = "- " + text;

                if (noTagLines[1][0] != '-')
                    text = text.Insert(text.IndexOf(Environment.NewLine) + 2, "- ");
            }
            return text;
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

        private void buttonToNarrator_Click(object sender, EventArgs e)
        {
            var len = NameList.ListNames.Count;
            var name = textBoxName.Text.Trim();
            if (name.Length == 0)
                return;
            NameList.AddNameToList(name);
            if (len != NameList.ListNames.Count)
            {
                listViewFixes.BeginUpdate();
                listViewFixes.Items.Clear();
                listViewFixes.EndUpdate();
                GeneratePreview();
            }
            //TODO: Update list view after adding new naem
        }

        private void AddFixToListView(Paragraph p, string before, string after)
        {
            var item = new ListViewItem() { Checked = true, UseItemStyleForSubItems = true, Tag = p };
            item.SubItems.Add(p.Number.ToString());
            item.SubItems.Add(before.Replace(Environment.NewLine, _configs.UILineBreak));
            item.SubItems.Add(after.Replace(Environment.NewLine, _configs.UILineBreak));
            listViewFixes.Items.Add(item);
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            _allowFixes = true;
            GeneratePreview();
            FixedSubtitle = _subtitle.ToText();
        }

        private void buttonGetNames_Click(object sender, EventArgs e)
        {
            // store the names in list on constructor runtime instead of loading it each time
            using (var formGetName = new GetNames(this, _subtitle)) // send the loaded list 
            {
                if (formGetName.ShowDialog(this) == DialogResult.OK)
                {
                    GeneratePreview();
                }
            }
        }

        private void buttonApply_Click(object sender, EventArgs e)
        {
            _allowFixes = true;
            GeneratePreview();
            FixedSubtitle = _subtitle.ToText();
            _allowFixes = !_allowFixes;
            listViewFixes.Items.Clear();
            GeneratePreview();
        }

        private void checkAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var clickedItem = (ToolStripMenuItem)sender;
            switch (clickedItem.Text)
            {
                case "Check all": // Check all
                    foreach (ListViewItem item in listViewFixes.Items)
                        item.Checked = true;
                    break;
                case "Uncheck all": // Uncheck all
                    foreach (ListViewItem item in listViewFixes.Items)
                        item.Checked = false;
                    break;
                case "Invert check": // Invert check
                    foreach (ListViewItem item in listViewFixes.Items)
                        item.Checked = !item.Checked;
                    break;
                case "Copy": // Copy
                    Clipboard.SetText(listViewFixes.SelectedItems[0].Tag.ToString());
                    break;
            }
        }

        private void contextMenuStrip1_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (listViewFixes.Items.Count == 0)
                e.Cancel = true;
        }
    }
}