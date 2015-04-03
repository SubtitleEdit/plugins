using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Linq;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public partial class PluginForm : Form
    {
        public string FixedSubtitle { get; set; }
        private Subtitle _subtitle;
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

        public PluginForm(Subtitle subtitle, string name, string description, Form parentForm)
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
                float fontSize = (float)Convert.ToDecimal(doc.Root.Element("FontSize").Value);
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
            var lines = _subtitle.Paragraphs[i].Text.Replace(Environment.NewLine, "\n").Split('\n');
            var sb = new StringBuilder();
            int maxWidth = 0;
            foreach (string line in lines)
            {
                var width = CalcWidth(line);
                if (width > maxWidth)
                    maxWidth = width;
                sb.Append(" " + width + " /");
            }
            subtitleListView1.SetAlternateText(i, sb.ToString().TrimEnd('/').Trim());
            if (maxWidth > numericUpDown1.Value)
            {
                subtitleListView1.SetBackgroundColor(i, Color.MistyRose);
                _tooLongCount++;
            }
            else
            {
                subtitleListView1.SetBackgroundColor(i, Control.DefaultBackColor);
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
                if (_customCharWidths.ContainsKey(c.ToString()))
                {
                    totalWidth += _customCharWidths[c.ToString()];
                }
                else
                { 
                    if (!_customCharNotFound.Contains(c.ToString()))
                    {
                        _customCharNotFound.Add(c.ToString());
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
            FixedSubtitle = _subtitle.ToText(new SubRip());
            DialogResult = DialogResult.OK;
        }

        private void PluginForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }

   
        private void buttonChooseFont_Click(object sender, EventArgs e)
        {
            fontDialog1.ShowEffects = false;
            fontDialog1.ShowDialog();
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
                XDocument document = new XDocument(
                    new XDeclaration("1.0", "utf8", "yes"),
                    new XComment("This XML file defines the settings for the Subtitle Edit CheckLineWidth plugin"),
                    new XComment("XML file generated by CheckLineWidth plugin"),
                    new XElement("CheckLineWidthSettings",
                        new XElement("FontName", fontDialog1.Font.Name),
                        new XElement("FontSize", fontDialog1.Font.Size),
                        new XElement("FontBold", fontDialog1.Font.Bold),
                        new XElement("MaxPixelLengthForLine", numericUpDown1.Value),
                        new XElement("UseCustomCharWidthList", radioButtonUseFont.Checked),
                        new XElement("CustomCharWidthList",
                            _customCharWidths.Where(p=>p.Key.Length > 0).Select(kvp => new XElement("Item", 
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
            labelLongestLine.Text = string.Format("Longest line: Line {0} is {1} pixels", _longestLineIndex + 1,  _longestLine);
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

        private void button1_Click(object sender, EventArgs e)
        {
            _customCharWidths.OrderBy(p => p.Key);
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
            if (text == null || text.Length < 3)
                return text;

            // do not autobreak dialogs
            if (text.Contains('-') && text.Contains(Environment.NewLine, StringComparison.Ordinal))
            {
                var noTagLines = Utilities.RemoveHtmlTags(text).Replace(Environment.NewLine, "\n").Replace("\r", "\n").Split("\n".ToCharArray());
                if (noTagLines.Length == 2)
                {
                    var arr0 = noTagLines[0].Trim().TrimEnd('"').TrimEnd('\'').TrimEnd();
                    if (arr0.StartsWith('-') && noTagLines[1].TrimStart().StartsWith('-') && arr0.Length > 1 && ".?!)]".Contains(arr0[arr0.Length - 1]) || arr0.EndsWith("--", StringComparison.Ordinal) || arr0.EndsWith('–'))
                        return text;
                }
            }


            string noHtml = Utilities.RemoveHtmlTags(text);
            if (text != noHtml)
            {
                return text;
            }

            var lines = text.Replace(Environment.NewLine, "\n").Replace("\r", "\n").Split("\n".ToCharArray());
            if (lines.Length != 2)
            {
                return text;
            }

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

    }
}