using System;
using System.Drawing;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Nikse.SubtitleEdit.PluginLogic;
using SubtitleEdit.Logic;

namespace SubtitleEdit
{
    internal sealed partial class PluginForm : Form
    {
        public string FixedSubtitle { get; private set; }
        private readonly Subtitle _subtitle;
        private int _totalFixes;
        private string _language;
        private static readonly Regex NumberStart = new Regex(@"^\d+ [A-Za-z]", RegexOptions.Compiled);
        private static readonly Regex NumberStartInside = new Regex(@"[\.,!] \d+ [A-Za-z]", RegexOptions.Compiled);
        private static readonly Regex NumberStartInside2 = new Regex(@"[\.,!]\r\n\d+ [A-Za-z]", RegexOptions.Compiled);
        private static readonly Regex NumberOneToNine = new Regex(@"\b\d\b", RegexOptions.Compiled);
        private static readonly Regex NumberTen = new Regex(@"\b10\b", RegexOptions.Compiled);
        private readonly GlyphReader _glyphReader;

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
            InitializeLanguages(subtitle);
            _glyphReader = new GlyphReader();
            FindAndListNetflixRulesFixes();
        }

        private void InitializeLanguages(Subtitle subtitle)
        {
            var autoDetectGoogleLanguage = LanguageAutoDetect.AutoDetectGoogleLanguage(subtitle);
            var ci = CultureInfo.GetCultureInfo(autoDetectGoogleLanguage);
            comboBoxLanguage.Items.Clear();
            foreach (var x in CultureInfo.GetCultures(CultureTypes.NeutralCultures))
                comboBoxLanguage.Items.Add(x);
            comboBoxLanguage.Sorted = true;
            int languageIndex = 0;
            int j = 0;
            foreach (var x in comboBoxLanguage.Items)
            {
                var xci = (CultureInfo)x;
                if (xci.TwoLetterISOLanguageName == ci.TwoLetterISOLanguageName)
                {
                    languageIndex = j;
                    break;
                }
                if (xci.TwoLetterISOLanguageName == "en")
                {
                    languageIndex = j;
                }
                j++;
            }
            comboBoxLanguage.SelectedIndex = languageIndex;
            comboBoxLanguage.SelectedIndexChanged += RuleCheckedChanged;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listViewFixes.Items)
            {
                var p = item.Tag as Paragraph;
                if (item.Checked && p != null)
                {
                    p.Text = item.SubItems[4].Text.Replace(Configuration.ListViewLineSeparatorString, Environment.NewLine).Trim();
                }
            }
            FixedSubtitle = _subtitle.ToText();
            DialogResult = listViewFixes.Items.Count > 0 ? DialogResult.OK : DialogResult.Cancel;
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

        private string GetLanguage()
        {
            var idx = comboBoxLanguage.SelectedIndex;
            if (idx >= 0)
            {
                var x = (CultureInfo)comboBoxLanguage.Items[idx];
                if (x != null)
                {
                    return x.TwoLetterISOLanguageName;
                }
            }
            return "en";
        }

        private void FindAndListNetflixRulesFixes()
        {
            _language = GetLanguage();
            listViewFixes.BeginUpdate();
            listViewFixes.Items.Clear();
            _totalFixes = 0;

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

                // Minimum duration: 5/6 second (833 ms) - also see https://github.com/SubtitleEdit/plugins/issues/129
                if (checkBoxMinDuration.Checked)
                {
                    if (p.Duration.TotalMilliseconds < 833)
                    {
                        p.Duration.TotalMilliseconds = 834;
                        _totalFixes++;
                        AddFixToListView(p, "Minimum duration: 5/6 second (833 ms)", text, text);
                    }
                }

                // Two lines maximum
                if (checkBoxTwoLinesMax.Checked)
                {
                    if (p.Text.SplitToLines().Length > 2)
                    {                        
                        _totalFixes++;
                        AddFixToListView(p, "Two lines maximum", text, AutoBreaker.AutoBreakLine(text));
                    }
                }

                // Two frames gap minimum
                if (checkBoxGapMinTwoFrames.Checked)
                { 
                    double frameRate = 25.0;
                    double twoFramesGap = 1000.0 / frameRate * 2.0;
                    if (next != null && p.EndTime.TotalMilliseconds + twoFramesGap > next.StartTime.TotalMilliseconds)
                    {
                        p.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds - twoFramesGap;
                        //TODO: check for min time/speed
                        _totalFixes++;
                        AddFixToListView(p, "Mininum two frames gap", text, text);
                    }
                }

                //- Speed - 17 characters per second --- is it max 17 characters per second
                if (checkBox17CharsPerSecond.Checked)
                {
                    var charactersPerSeconds = Utilities.GetCharactersPerSecond(p);
                    if (charactersPerSeconds > 17)
                    {
                        var tempP = new Paragraph(p);
                        while (Utilities.GetCharactersPerSecond(tempP) > 17)
                        {
                            tempP.EndTime.TotalMilliseconds++;
                        }
                        _totalFixes++;
                        AddFixToListView(p, "Maximum 17 characters per second", text, text);
                    }
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
                            arr[1] = "-[" + arr[1].Substring(2, arr[1].Length - 3) + "]";
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

                    var m = NumberStart.Match(newText);
                    while (m.Success)
                    {
                        int length = m.Length - 2;
                        newText = newText.Remove(m.Index, length).Insert(m.Index, ConvertNumberToString(m.Value.Substring(0, length), true));
                        m = NumberStart.Match(newText, m.Index + 1);
                    }

                    m = NumberStartInside.Match(newText);
                    while (m.Success)
                    {
                        int length = m.Length - 4;
                        newText = newText.Remove(m.Index + 2, length).Insert(m.Index + 2, ConvertNumberToString(m.Value.Substring(2, length), true));
                        m = NumberStartInside.Match(newText, m.Index + 1);
                    }

                    m = NumberStartInside2.Match(newText);
                    while (m.Success)
                    {
                        int length = m.Length - 5;
                        newText = newText.Remove(m.Index + 3, length).Insert(m.Index + 3, ConvertNumberToString(m.Value.Substring(3, length), true));
                        m = NumberStartInside2.Match(newText, m.Index + 1);
                    }

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
                    var m = NumberOneToNine.Match(newText);
                    while (m.Success)
                    {
                        bool ok = newText.Length > m.Index + 1 && !":.".Contains(newText[m.Index + 1].ToString()) || newText.Length <= m.Index + 1;
                        if (ok && m.Index > 0 && ":.".Contains(newText[m.Index - 1].ToString()))
                            ok = false;
                        if (ok)
                            newText = newText.Remove(m.Index, 1).Insert(m.Index, ConvertNumberToString(m.Value.Substring(0, 1), false));
                        m = NumberOneToNine.Match(newText, m.Index + 1);
                    }

                    m = NumberTen.Match(newText);
                    while (m.Success)
                    {
                        bool ok = newText.Length > m.Index + 2 && newText[m.Index + 2] != ':' || newText.Length <= m.Index + 2;
                        if (ok && m.Index > 0 && ":.".Contains(newText[m.Index - 1].ToString()))
                            ok = false;
                        if (ok)
                            newText = newText.Remove(m.Index, 2).Insert(m.Index, "ten");
                        m = NumberTen.Match(newText, m.Index + 1);
                    }

                    if (newText != text)
                    {
                        _totalFixes++;
                        AddFixToListView(p, "From 1 to 10, numbers should be written out: one, two, three, etc", text, newText);
                    }
                }

                // Glyph List - Only text/ characters included in the NETFLIX Glyph List (version 2) can be used.
                if (checkBoxCheckValidGlyphs.Checked)
                {
                    string badGlyphs;
                    if (_glyphReader.ContainsIllegalGlyphs(p.Text, out badGlyphs))
                    {                        
                        _totalFixes++;
                        AddFixToListView(p, "Only use valid characters (not " + badGlyphs + ")", text, _glyphReader.CleanText(text));
                    }
                }
            }

            listViewFixes.EndUpdate();
            labelTotal.Text = "Total: " + _totalFixes;
            labelTotal.ForeColor = _totalFixes > 0 ? Color.Red : Color.Green;
        }

        private string ConvertNumberToString(string value, bool startWithUppercase)
        {
            value = value.Trim();
            if (_language == "en")
            {
                if (value.Equals("0", StringComparison.Ordinal)) value = "zero";
                if (value.Equals("1", StringComparison.Ordinal)) value = "one";
                if (value.Equals("2", StringComparison.Ordinal)) value = "two";
                if (value.Equals("3", StringComparison.Ordinal)) value = "three";
                if (value.Equals("4", StringComparison.Ordinal)) value = "four";
                if (value.Equals("5", StringComparison.Ordinal)) value = "five";
                if (value.Equals("6", StringComparison.Ordinal)) value = "six";
                if (value.Equals("7", StringComparison.Ordinal)) value = "seven";
                if (value.Equals("8", StringComparison.Ordinal)) value = "eight";
                if (value.Equals("9", StringComparison.Ordinal)) value = "nine";
                if (value.Equals("10", StringComparison.Ordinal)) value = "ten";
                if (value.Equals("11", StringComparison.Ordinal)) value = "eleven";
                if (value.StartsWith("12", StringComparison.Ordinal)) value = "twelve";
                if (value.StartsWith("13", StringComparison.Ordinal)) value = "thirteen";
                if (value.StartsWith("14", StringComparison.Ordinal)) value = "fourteen";
                if (value.StartsWith("15", StringComparison.Ordinal)) value = "fifteen";
                if (value.StartsWith("16", StringComparison.Ordinal)) value = "sixteen";
                if (value.StartsWith("17", StringComparison.Ordinal)) value = "seventeen";
                if (value.StartsWith("18", StringComparison.Ordinal)) value = "eighteen";
                if (value.StartsWith("19", StringComparison.Ordinal)) value = "nineteen";
                if (value.StartsWith("20", StringComparison.Ordinal)) value = "twenty";
                if (value.StartsWith("30", StringComparison.Ordinal)) value = "thirty";
                if (value.StartsWith("40", StringComparison.Ordinal)) value = "forty";
                if (value.StartsWith("50", StringComparison.Ordinal)) value = "fifty";
                if (value.StartsWith("60", StringComparison.Ordinal)) value = "sixty";
                if (value.StartsWith("70", StringComparison.Ordinal)) value = "seventy";
                if (value.StartsWith("80", StringComparison.Ordinal)) value = "eighty";
                if (value.StartsWith("90", StringComparison.Ordinal)) value = "ninety";
                if (value.StartsWith("100", StringComparison.Ordinal)) value = "one hundred";
            }
            if (_language == "da")
            {
                if (value.Equals("0", StringComparison.Ordinal)) value = "nul";
                if (value.Equals("1", StringComparison.Ordinal)) value = "en";
                if (value.Equals("2", StringComparison.Ordinal)) value = "to";
                if (value.Equals("3", StringComparison.Ordinal)) value = "tre";
                if (value.Equals("4", StringComparison.Ordinal)) value = "fire";
                if (value.Equals("5", StringComparison.Ordinal)) value = "fem";
                if (value.Equals("6", StringComparison.Ordinal)) value = "seks";
                if (value.Equals("7", StringComparison.Ordinal)) value = "syv";
                if (value.Equals("8", StringComparison.Ordinal)) value = "otte";
                if (value.Equals("9", StringComparison.Ordinal)) value = "ni";
                if (value.Equals("10", StringComparison.Ordinal)) value = "ti";
                if (value.Equals("11", StringComparison.Ordinal)) value = "elve";
                if (value.StartsWith("12", StringComparison.Ordinal)) value = "tolv";
                if (value.StartsWith("13", StringComparison.Ordinal)) value = "tretten";
                if (value.StartsWith("14", StringComparison.Ordinal)) value = "fjorten";
                if (value.StartsWith("15", StringComparison.Ordinal)) value = "femten";
                if (value.StartsWith("16", StringComparison.Ordinal)) value = "seksten";
                if (value.StartsWith("17", StringComparison.Ordinal)) value = "sytten";
                if (value.StartsWith("18", StringComparison.Ordinal)) value = "atten";
                if (value.StartsWith("19", StringComparison.Ordinal)) value = "nitten";
                if (value.StartsWith("20", StringComparison.Ordinal)) value = "tyve";
                if (value.StartsWith("30", StringComparison.Ordinal)) value = "tredieve";
                if (value.StartsWith("40", StringComparison.Ordinal)) value = "fyrre";
                if (value.StartsWith("50", StringComparison.Ordinal)) value = "halvtreds";
                if (value.StartsWith("60", StringComparison.Ordinal)) value = "treds";
                if (value.StartsWith("70", StringComparison.Ordinal)) value = "halvfjerds";
                if (value.StartsWith("80", StringComparison.Ordinal)) value = "first";
                if (value.StartsWith("90", StringComparison.Ordinal)) value = "halvfems";
                if (value.StartsWith("100", StringComparison.Ordinal)) value = "ethunderede";
            }
            if (startWithUppercase && value.Length > 0)
            {
                value = value[0].ToString().ToUpper()[0] + value.Remove(0, 1);
            }
            return value;
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