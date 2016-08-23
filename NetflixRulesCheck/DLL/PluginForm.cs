using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Nikse.SubtitleEdit.PluginLogic;

namespace SubtitleEdit
{
    internal sealed partial class PluginForm : Form
    {
        public string FixedSubtitle { get; private set; }
        private readonly Subtitle _subtitle;
        private int _totalFixes;
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
            FindAndListNetflixRulesFixes();
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

        private void FindAndListNetflixRulesFixes()
        {
            listViewFixes.BeginUpdate();
            listViewFixes.Items.Clear();
            _totalFixes = 0;
            _fixedParagraphs = new Dictionary<string, string>();


            // https://backlothelp.netflix.com/hc/en-us/articles/215758617-Timed-Text-Style-Guide-General-Requirements

            for (int i = 0; i < _subtitle.Paragraphs.Count; i++)
            {
                var prev = _subtitle.GetParagraphOrDefault(i - 1);
                var p = _subtitle.Paragraphs[i];
                var next = _subtitle.GetParagraphOrDefault(i + 1);
                var text = p.Text.Trim();

                // Maximum duration: 7 seconds per subtitle event
                if (checkBoxMaxDuration.Checked)
                {
                    if (p.Duration.TotalMilliseconds > 7000)
                    {
                        p.Duration.TotalMilliseconds = 7000;
                        _totalFixes++;
                        AddFixToListView(p, "Maximum duration: 7 seconds per subtitle event", text, text);
                    }
                }

                // Minimum duration: 5/6 second (833.3 ms)
                if (checkBoxMinDuration.Checked)
                {
                    if (p.Duration.TotalMilliseconds < 833.333)
                    {
                        p.Duration.TotalMilliseconds = 834;
                        _totalFixes++;
                        AddFixToListView(p, "Minimum duration: 5/6 second (833.3 ms)", text, text);
                    }
                }

                // Two lines maximum
                if (checkBoxTwoLinesMax.Checked)
                {
                    if (p.Text.SplitToLines().Length > 2)
                    {
                        //TODO- auto-break
                        _totalFixes++;
                        AddFixToListView(p, "Two lines maximum", text, text);
                    }
                }

                //- Two frames gap


                ////- Speed - 17 characters per second --- is it max 17 characters per second
                var charactersPerSeconds = Utilities.GetCharactersPerSecond(p);
                if (charactersPerSeconds > 17)
                {
                    var tempP = new Paragraph(p);
                    while (Utilities.GetCharactersPerSecond(tempP) > 17)
                    {
                        tempP.EndTime.TotalMilliseconds++;
                    }
                    _totalFixes++;
                    AddFixToListView(p, "Minimum 17 characters per second", text, text);
                }

                //- Dual Speakers: Use a hyphen without a space
                if (checkBoxDialogHypenNoSpace.Checked)
                {
                    var arr = p.Text.SplitToLines();
                    if (arr.Length == 2 && p.Text.Contains("-"))
                    {
                        string newText = p.Text;
                        if (arr[0].StartsWith("- ") && arr[1].StartsWith("- "))
                        {
                            newText = "-" + arr[0].Remove(0, 2) + Environment.NewLine + "-" + arr[1].Remove(0, 2);
                        }
                        else if (arr[0].StartsWith("<i>- ") && arr[1].StartsWith("<i>- "))
                        {
                            newText = "<i>-" + arr[0].Remove(0, 5) + Environment.NewLine + "<i>-" + arr[1].Remove(0, 5);
                        }
                        else if (arr[0].StartsWith("<i>- ") && arr[1].StartsWith("- "))
                        {
                            newText = "<i>-" + arr[0].Remove(0, 5) + Environment.NewLine + "-" + arr[1].Remove(0, 2);
                        }
                        else if (arr[0].StartsWith("- ") && arr[1].StartsWith("<i>- "))
                        {
                            newText = "-" + arr[0].Remove(0, 2) + Environment.NewLine + "<i>-" + arr[1].Remove(0, 5);
                        }
                        else if ((arr[0].StartsWith("-") || arr[0].StartsWith("<i>-")) && arr[1].StartsWith("- "))
                        {
                            newText = "-" + arr[0] + Environment.NewLine + "-" + arr[1].Remove(0, 2);
                        }
                        else if (arr[0].StartsWith("- ") && (arr[1].StartsWith("-") || arr[1].StartsWith("<i>-")))
                        {
                            newText = "-" + arr[0].Remove(0, 2) + Environment.NewLine + "-" + arr[1];
                        }

                        if (newText != text)
                        {
                            _totalFixes++;
                            AddFixToListView(p, "Dual Speakers: Use a hyphen without a space", text, newText);
                        }
                    }
                }

                //Use brackets[] to enclose speaker IDs or sound effects
                if (checkBoxSquareBracketForHi.Checked)
                {
                    string newText = p.Text;
                    var arr = p.Text.SplitToLines();
                    if (newText.StartsWith("(", StringComparison.Ordinal) && newText.EndsWith(")", StringComparison.Ordinal))
                    {
                        newText = "[" + newText.Substring(1, newText.Length - 2) + "]";
                    }
                    else if (newText.StartsWith("{", StringComparison.Ordinal) && newText.EndsWith("}", StringComparison.Ordinal))
                    {
                        newText = "[" + newText.Substring(1, newText.Length - 2) + "]";
                    }
                    else if (arr.Length == 2 && arr[0].StartsWith("-") && arr[1].StartsWith("-"))
                    {
                        if ((arr[0].StartsWith("-(") && arr[0].EndsWith(")")) || (arr[0].StartsWith("-{") && arr[0].EndsWith("}")))
                        {
                            arr[0] = "-[" + newText.Substring(2, newText.Length - 3) + "]";
                        }
                        if ((arr[1].StartsWith("-(") && arr[1].EndsWith(")")) || (arr[1].StartsWith("-{") && arr[1].EndsWith("}")))
                        {
                            arr[1] = "-[" + newText.Substring(2, newText.Length - 3) + "]";
                        }
                        newText = arr[0] + Environment.NewLine + arr[1];
                    }

                    if (newText != text)
                    {
                        _totalFixes++;
                        AddFixToListView(p, "Use brackets [ ] to enclose speaker IDs or sound effects", text, newText);
                    }
                }

                //- When a number begins a sentence, it should always be spelled out.
                if (checkBoxSpellOutStartNumbers.Checked)
                {
                    string newText = p.Text;
                    var arr = p.Text.SplitToLines();

                    if (newText != text)
                    {
                        _totalFixes++;
                        AddFixToListView(p, "When a number begins a sentence, it should always be spelled out", text, newText);
                    }
                }

                //- From 1 to 10, numbers should be written out: en, to, tre, etc.                
                if (checkBoxWriteOutOneToTen.Checked)
                {
                    string newText = p.Text;
                    var arr = p.Text.SplitToLines();

                    if (newText != text)
                    {
                        _totalFixes++;
                        AddFixToListView(p, "From 1 to 10, numbers should be written out: one, two, three, etc", text, newText);
                    }
                }
            }

            listViewFixes.EndUpdate();
            labelTotal.Text = "Total: " + _totalFixes;
            labelTotal.ForeColor = _totalFixes > 0 ? Color.Red : Color.Green;
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

        private void RuleCheckedChanged(object sender, EventArgs e)
        {
            FindAndListNetflixRulesFixes();
        }
    }
}