using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Linq;

namespace Nikse.SubtitleEdit.PluginLogic
{
    internal partial class PluginForm : Form
    {
        private const string BreakChars = " \".!?,)([]<>:;♪{}-/#*|¿¡\r\n\t";
        public string FixedSubtitle { get; set; }
        private readonly Subtitle _subtitle;
        private SortedDictionary<string, int> _customCharWidths = new SortedDictionary<string, int>();
        private int _selectedIndex = -1;
        private int _longestLine;
        private int _longestLineIndex;
        private int _tooLongCount;
        private List<string> _customCharNotFound = new List<string>();

        public PluginForm()
        {
            InitializeComponent();
        }

        public PluginForm(Subtitle subtitle, string name, string description, Form parentForm, int errorCount)
            : this()
        {
            _subtitle = subtitle;
            subtitleListView1.InitializeLanguage();
            subtitleListView1.ShowAlternateTextColumn("Line width");

            labelStatus.Text = string.Empty;
            labelLongestLine.Text = string.Empty;
            labelCustomCharNotFound.Text = string.Empty;

            LoadSettingsIfThereIs();
            subtitleListView1.Fill(subtitle);
            for (int i = 0; i < _subtitle.Paragraphs.Count; i++)
            {
                ShowLineLengths(i);
            }
            if (subtitle.Paragraphs.Count > 0)
                subtitleListView1.SelectIndexAndEnsureVisible(0);
            if (errorCount > 0)
            {
                labelErrorCount.ForeColor = Color.Red;
                labelErrorCount.Text += errorCount.ToString(CultureInfo.InvariantCulture);
            }
            else
            {
                labelErrorCount.ForeColor = DefaultForeColor;
                labelErrorCount.Text += errorCount.ToString(CultureInfo.InvariantCulture);
            }
            radioButtonUseFont_CheckedChanged(null, null);
        }

        private void LoadSettingsIfThereIs()
        {
            fontDialog1.Font = new Font("Tahoma", 9, FontStyle.Regular);

            var fileName = GetSettingsFileName();
            if (File.Exists(fileName))
            {
                XDocument doc = XDocument.Parse(File.ReadAllText(fileName));

                string fontName = doc.Root.Element("FontName").Value;
                float fontSize = (float)Convert.ToDecimal(doc.Root.Element("FontSize").Value, CultureInfo.InvariantCulture);
                bool fontBold = Convert.ToBoolean(doc.Root.Element("FontBold").Value);
                FontStyle fontStyle = fontBold ? FontStyle.Bold : FontStyle.Regular;
                fontDialog1.Font = new Font(fontName, fontSize, fontStyle);

                numericUpDown1.Value = Convert.ToDecimal(doc.Root.Element("MaxPixelLengthForLine").Value);

                radioButtonUseFont.Checked = Convert.ToBoolean(doc.Root.Element("UseCustomCharWidthList").Value);
                radioButtonUseCustom.Checked = !radioButtonUseFont.Checked;

                foreach (XElement node in doc.Root.Element("CustomCharWidthList").Elements())
                {
                    string character = node.Element("Char").Value;
                    int value = Convert.ToInt32(node.Element("Width").Value);
                    if (!_customCharWidths.ContainsKey(character))
                    {
                        _customCharWidths.Add(character, value);
                    }
                }
            }
            labelFont.Text = string.Format("{0}, size {1}", fontDialog1.Font.Name, fontDialog1.Font.Size);
        }

        private void ShowLineLengths(int i)
        {
            var lines = Utilities.RemoveHtmlTags(_subtitle.Paragraphs[i].Text, true).Replace(Environment.NewLine, "\n").Split('\n');
            var sb = new StringBuilder();
            int maxWidth = 0;
            foreach (string line in lines)
            {
                var width = CalcWidth(line);
                if (width > maxWidth)
                    maxWidth = width;
                sb.Append(" " + width + " /");
            }
            if (maxWidth > numericUpDown1.Value)
            {
                subtitleListView1.SetAlternateText(i, sb.ToString().TrimEnd('/').Trim() + " - " + (maxWidth - numericUpDown1.Value) + " pixels too long!!!");
                subtitleListView1.SetBackgroundColor(i, Color.FromArgb(255, 180, 150));
                _tooLongCount++;
            }
            else
            {
                subtitleListView1.SetAlternateText(i, sb.ToString().TrimEnd('/').Trim());
                subtitleListView1.SetBackgroundColor(i, DefaultBackColor);
            }
            if (maxWidth > _longestLine)
            {
                _longestLine = maxWidth;
                _longestLineIndex = i;
            }
        }

        private int CalcWidth(string line)
        {
            if (radioButtonUseFont.Checked)
            {
                using (var g = CreateGraphics())
                {
                    return (int)Math.Round(g.MeasureString(line, fontDialog1.Font).Width);
                }
            }
            int totalWidth = 0;
            foreach (char c in line)
            {
                if (_customCharWidths.ContainsKey(c.ToString(CultureInfo.InvariantCulture)))
                {
                    totalWidth += _customCharWidths[c.ToString(CultureInfo.InvariantCulture)];
                }
                else
                {
                    if (!_customCharNotFound.Contains(c.ToString(CultureInfo.InvariantCulture)))
                    {
                        _customCharNotFound.Add(c.ToString(CultureInfo.InvariantCulture));
                    }
                }
            }
            return totalWidth;
        }

        private string GetSettingsFileName()
        {
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);
            if (path.StartsWith("file:\\"))
                path = path.Remove(0, 6);
            path = Path.Combine(path, "Plugins");
            if (!Directory.Exists(path))
                path = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Subtitle Edit"), "Plugins");
            return Path.Combine(path, "CheckLineWidth.xml");
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            FixedSubtitle = _subtitle.ToText();
            DialogResult = DialogResult.OK;
        }

        private void PluginForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.Cancel;
        }

        private void buttonChooseFont_Click(object sender, EventArgs e)
        {
            fontDialog1.ShowEffects = false;
            if (fontDialog1.ShowDialog() == DialogResult.OK)
            {
                labelFont.Text = string.Format("{0}, size {1}", fontDialog1.Font.Name, fontDialog1.Font.Size);
                RefreshLineWidth();
            }
        }

        private void subtitleListView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (subtitleListView1.SelectedItems.Count == 0)
                return;

            _selectedIndex = subtitleListView1.SelectedItems[0].Index;
            Paragraph p = _subtitle.GetParagraphOrDefault(_selectedIndex);
            if (p != null)
            {
                textBoxText.Text = _subtitle.Paragraphs[_selectedIndex].Text;
            }
        }

        private void PluginForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                var fileName = GetSettingsFileName();
                var document = new XDocument(
                    new XDeclaration("1.0", "utf8", "yes"),
                    new XComment("This XML file defines the settings for the Subtitle Edit CheckLineWidth plugin"),
                    new XComment("XML file generated by CheckLineWidth plugin"),
                    new XElement("CheckLineWidthSettings",
                        new XElement("FontName", fontDialog1.Font.Name),
                        new XElement("FontSize", fontDialog1.Font.Size.ToString(CultureInfo.InvariantCulture)),
                        new XElement("FontBold", fontDialog1.Font.Bold),
                        new XElement("MaxPixelLengthForLine", numericUpDown1.Value.ToString(CultureInfo.InvariantCulture)),
                        new XElement("UseCustomCharWidthList", radioButtonUseFont.Checked),
                        new XElement("CustomCharWidthList",
                            _customCharWidths.Where(p => p.Key.Length > 0).Select(kvp => new XElement("Item",
                                new XElement("Char", kvp.Key),
                                new XElement("Width", kvp.Value))
                            )
                        )
                    )
                );
                document.Save(fileName);
            }
            catch
            {
            }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            RefreshLineWidth();
        }

        private void RefreshLineWidth()
        {
            _customCharNotFound = new List<string>();
            _longestLine = 0;
            _tooLongCount = 0;
            for (int i = 0; i < _subtitle.Paragraphs.Count; i++)
            {
                ShowLineLengths(i);
            }
            labelLongestLine.Text = string.Format("Longest line: Line {0} is {1} pixels", _longestLineIndex + 1, _longestLine);
            labelStatus.Text = string.Format("{0} lines are longer than {1} pixels", _tooLongCount, numericUpDown1.Value);
            if (radioButtonUseCustom.Checked)
            {
                var sb = new StringBuilder();
                foreach (var s in _customCharNotFound)
                {
                    sb.Append(s + " ");
                }
                labelCustomCharNotFound.Text = "Char not found in custom list";
                textBoxCustomCharNotFound.Text = sb.ToString();
                textBoxCustomCharNotFound.Visible = true;
            }
            else
            {
                labelCustomCharNotFound.Text = string.Empty;
                textBoxCustomCharNotFound.Visible = false;
            }
        }

        private void buttonEditChars_Click(object sender, EventArgs e)
        {
            var temp = _customCharWidths.OrderBy(p => p.Key);
            _customCharWidths = new SortedDictionary<string, int>();
            foreach (var kvp in temp)
            {
                _customCharWidths.Add(kvp.Key, kvp.Value);
            }

            using (var form = new CustomCharList(_customCharWidths))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    _customCharWidths = form.CharList;
                    RefreshLineWidth();
                }
            }
        }

        private void radioButtonUseFont_CheckedChanged(object sender, EventArgs e)
        {
            buttonChooseFont.Enabled = radioButtonUseFont.Checked;
            labelFont.Enabled = radioButtonUseFont.Checked;

            buttonEditCustomCharWidthList.Enabled = !radioButtonUseFont.Checked;
            RefreshLineWidth();
        }

        private void textBoxText_TextChanged(object sender, EventArgs e)
        {
            if (_selectedIndex >= 0)
            {
                subtitleListView1.SetText(_selectedIndex, textBoxText.Text.TrimEnd());
                _subtitle.Paragraphs[_selectedIndex].Text = textBoxText.Text.TrimEnd();
            }
            if (Utilities.GetNumberOfLines(textBoxText.Text) > 3)
                textBoxText.ScrollBars = ScrollBars.Vertical;
            else
                textBoxText.ScrollBars = ScrollBars.None;
            GeneratePreview();
        }

        private void buttonUnBreak_Click(object sender, EventArgs e)
        {
            if (subtitleListView1.SelectedItems.Count >= 1)
            {
                subtitleListView1.BeginUpdate();
                foreach (int index in subtitleListView1.SelectedIndices)
                {
                    Paragraph p = _subtitle.GetParagraphOrDefault(index);
                    if (p != null)
                    {
                        p.Text = AutoBreakLine(p.Text);
                        subtitleListView1.SetText(index, p.Text);

                        if (index == _selectedIndex)
                        {
                            textBoxText.Text = p.Text;
                        }
                    }
                }
                subtitleListView1.EndUpdate();
            }
            RefreshLineWidth();
        }

        private string AutoBreakLine(string text)
        {
            if (text == null || text.Length < 3 || Utilities.GetNumberOfLines(text) > 2)
                return text;

            if (Utilities.GetNumberOfLines(text) == 1 && numericUpDown1.Value > CalcWidth(text))
                return text;

            if (Utilities.GetNumberOfLines(text) == 2 && numericUpDown1.Value > CalcWidth(text.Replace(Environment.NewLine, " ")))
            {
                var line0 = text.Replace(Environment.NewLine, "\n").Replace('\r', '\n').Split('\n')[0].TrimEnd();
                if (!line0.EndsWith(".") &&
                    !line0.EndsWith("!") &&
                    !line0.EndsWith("?") &&
                    !line0.EndsWith(":") &&
                    !line0.EndsWith("♪") &&
                    !line0.EndsWith("♫") &&
                    !line0.EndsWith(".\"") &&
                    !line0.EndsWith("!\"") &&
                    !line0.EndsWith("?\"") &&
                    !line0.EndsWith(":\"") &&
                    !line0.EndsWith(":\"") &&
                    !line0.EndsWith("♪</i>") &&
                    !line0.EndsWith("♫</i>") &&
                    !line0.EndsWith(".</i>") &&
                    !line0.EndsWith("!</i>") &&
                    !line0.EndsWith("?</i>") &&
                    !line0.EndsWith(".</font>") &&
                    !line0.EndsWith("!</font>") &&
                    !line0.EndsWith("?</font>") &&
                    !line0.EndsWith(".</b>"))
                {
                    return text.Replace(Environment.NewLine, " ").Replace("  ", " ").Trim();
                }
            }



            var noTagText = Utilities.RemoveHtmlTags(text, true);
            // do not autobreak dialogs
            if (noTagText.Contains('-') && noTagText.Contains(Environment.NewLine, StringComparison.Ordinal))
            {
                var noTagLines = noTagText.Replace(Environment.NewLine, "\n").Replace('\r', '\n').Split('\n');
                if (noTagLines.Length == 2)
                {
                    var arr0 = noTagLines[0].Trim().TrimEnd('"', '\'').TrimEnd();
                    if (arr0.StartsWith('-') && noTagLines[1].TrimStart().StartsWith('-') &&
                       (arr0.Length > 1 && (".?!)]".Contains(arr0[arr0.Length - 1]) || arr0.EndsWith("--", StringComparison.Ordinal) || arr0.EndsWith('–'))))
                        return text;
                }
            }

            // Todo:
            if (text != noTagText)
            {
                return text;
            }

            var lines = text.Replace(Environment.NewLine, "\n").Replace('\r', '\n').Split('\n');
            if (lines.Length == 1)
                lines = (text.Replace(Environment.NewLine, "\n").Replace('\r', '\n') + "\n").Split('\n');
            int initialWidthLine1 = CalcWidth(lines[0]);
            int initialWidthLine2 = CalcWidth(lines[1]);
            bool better = true;
            int widthLine1 = initialWidthLine1;
            int widthLine2 = initialWidthLine2;
            while (better)
            {
                if (widthLine1 > widthLine2)
                {
                    lines = MoveLastWordDown(lines);
                }
                else
                {
                    lines = MoveLastWordUp(lines);
                }
                widthLine1 = CalcWidth(lines[0]);
                widthLine2 = CalcWidth(lines[1]);

                better = Math.Abs(initialWidthLine1 - initialWidthLine2) > Math.Abs(widthLine1 - widthLine2);
                if (better)
                {
                    text = lines[0] + Environment.NewLine + lines[1];
                    initialWidthLine1 = widthLine1;
                    initialWidthLine2 = widthLine2;
                }
            }
            return text;
        }

        private string[] MoveLastWordUp(string[] lines)
        {
            int firstSpace = lines[1].IndexOf(' ');
            if (firstSpace > 0)
            {
                string word = lines[1].Substring(0, firstSpace).Trim();
                lines[1] = lines[1].Remove(0, firstSpace).Trim();
                lines[0] = lines[0].Trim() + " " + word;
            }
            return lines;
        }

        private string[] MoveLastWordDown(string[] lines)
        {
            int lastSpace = lines[0].LastIndexOf(' ');
            if (lastSpace > 0)
            {
                string word = lines[0].Substring(lastSpace).Trim();
                lines[0] = lines[0].Remove(lastSpace).Trim();
                lines[1] = word + " " + lines[1].Trim();
            }
            return lines;
        }

        private void subtitleListView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == (Keys.Control | Keys.A))
            {
                foreach (ListViewItem item in subtitleListView1.Items)
                {
                    item.Selected = true;
                }
            }
        }

        private void textBoxText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Modifiers == Keys.Control && e.KeyCode == Keys.Back)
            {
                int index = textBoxText.SelectionStart;
                if (textBoxText.SelectionLength == 0)
                {
                    string s = textBoxText.Text;
                    int deleteFrom = index - 1;

                    if (deleteFrom > 0 && deleteFrom < s.Length)
                    {
                        if (s[deleteFrom] == ' ')
                            deleteFrom--;
                        while (deleteFrom > 0 && !BreakChars.Contains(s[deleteFrom]))
                        {
                            deleteFrom--;
                        }
                        if (deleteFrom == index - 1)
                        {
                            var breakCharsNoSpace = BreakChars.Substring(1);
                            while (deleteFrom > 0 && breakCharsNoSpace.Contains(s[deleteFrom - 1]))
                            {
                                deleteFrom--;
                            }
                        }
                        if (s[deleteFrom] == ' ')
                            deleteFrom++;
                        textBoxText.Text = s.Remove(deleteFrom, index - deleteFrom);
                        textBoxText.SelectionStart = deleteFrom;
                    }
                }
                e.SuppressKeyPress = true;
            }
        }
        private void GeneratePreview()
        {
            if (timer1.Enabled)
            {
                timer1.Stop();
                timer1.Start();
            }
            else
            {
                timer1.Start();
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (CanFocus)
            {
                timer1.Stop();
                RefreshLineWidth();
            }
        }
    }
}