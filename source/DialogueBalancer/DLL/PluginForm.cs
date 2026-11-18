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

        private NumericUpDown _numericUpDownThreshold;
        private Label _labelThreshold;

        internal PluginForm(Subtitle subtitle, string name, string description)
        {
            InitializeComponent();

            labelTotal.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;

            this.Text = name;
            labelDescription.Text = description;
            _subtitle = subtitle;

            this.Resize += (s, e) =>
            {
                if (listViewFixes.Columns.Count > 0)
                    listViewFixes.Columns[listViewFixes.Columns.Count - 1].Width = -2;
            };

            _labelThreshold = new Label();
            _labelThreshold.Text = "Fix if line difference exceeds:";
            _labelThreshold.AutoSize = true;
            _labelThreshold.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;

            _numericUpDownThreshold = new NumericUpDown();
            _numericUpDownThreshold.Size = new Size(50, 20);
            _numericUpDownThreshold.Minimum = 0;
            _numericUpDownThreshold.Maximum = 150;
            _numericUpDownThreshold.Value = 22;
            _numericUpDownThreshold.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;

            LoadSettings();

            _numericUpDownThreshold.ValueChanged += (s, e) =>
            {
                SaveSettings();
                if (_allowFixes) return;
                FindDialogueAndListFixes();
            };

            this.Controls.Add(_labelThreshold);
            this.Controls.Add(_numericUpDownThreshold);

            this.Load += (s, e) =>
            {
                _labelThreshold.Location = new Point(130, labelTotal.Location.Y);
                _numericUpDownThreshold.Location = new Point(380, labelTotal.Location.Y - 2);
            };

            FindDialogueAndListFixes();
        }

        private string GetSettingsFileName()
        {
            string path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Subtitle Edit", "Plugins");
            if (!System.IO.Directory.Exists(path))
            {
                try
                {
                    System.IO.Directory.CreateDirectory(path);
                }
                catch { }
            }
            return System.IO.Path.Combine(path, "DialogueBalancer.xml");
        }

        private void LoadSettings()
        {
            try
            {
                string fileName = GetSettingsFileName();
                if (System.IO.File.Exists(fileName))
                {
                    System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
                    doc.Load(fileName);
                    System.Xml.XmlNode node = doc.DocumentElement.SelectSingleNode("Threshold");
                    if (node != null)
                    {
                        int threshold;
                        if (int.TryParse(node.InnerText, out threshold))
                        {
                            if (threshold >= _numericUpDownThreshold.Minimum && threshold <= _numericUpDownThreshold.Maximum)
                                _numericUpDownThreshold.Value = threshold;
                        }
                    }
                }
            }
            catch { }
        }

        private void SaveSettings()
        {
            try
            {
                string fileName = GetSettingsFileName();
                System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
                doc.LoadXml("<DialogueBalancerSettings><Threshold>" + _numericUpDownThreshold.Value.ToString() + "</Threshold></DialogueBalancerSettings>");
                doc.Save(fileName);
            }
            catch { }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            SaveSettings();
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
            subItem = new ListViewItem.ListViewSubItem(item, before.Replace(Environment.NewLine, Configuration.ListViewLineSeparatorString));
            item.SubItems.Add(subItem);
            subItem = new ListViewItem.ListViewSubItem(item, after.Replace(Environment.NewLine, Configuration.ListViewLineSeparatorString));
            item.SubItems.Add(subItem);

            item.Tag = p;

            listViewFixes.Items.Add(item);
        }

        private void FindDialogueAndListFixes()
        {
            string fixAction = "Balance dialogue";

            if (!_allowFixes)
            {
                _totalFixes = 0;
                listViewFixes.BeginUpdate();
                listViewFixes.Items.Clear();
            }

            for (int i = 0; i < _subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = _subtitle.Paragraphs[i];
                string text = p.Text;
                string newText = BalanceDialogue(text);

                if (text != newText)
                {
                    if (AllowFix(p, fixAction))
                    {
                        p.Text = newText;
                    }
                    else if (!_allowFixes)
                    {
                        _totalFixes++;
                        string oldTextPreview = Utilities.RemoveHtmlTags(text);
                        string newTextPreview = Utilities.RemoveHtmlTags(newText);
                        AddFixToListView(p, fixAction, oldTextPreview, newTextPreview);
                    }
                }
            }

            if (!_allowFixes)
            {
                labelTotal.Text = "Total: " + _totalFixes.ToString();
                labelTotal.ForeColor = _totalFixes > 0 ? Color.Blue : Color.Red;
                listViewFixes.EndUpdate();
            }
        }

        private bool AllowFix(Paragraph p, string action)
        {
            if (!_allowFixes)
                return false;

            string ln = p.Number.ToString();
            foreach (ListViewItem item in listViewFixes.Items)
            {
                if (item.SubItems.Count > 2 && item.SubItems[1].Text == ln && item.SubItems[2].Text == action)
                    return item.Checked;
            }
            return false;
        }

        private string BalanceDialogue(string text)
        {
            string[] lines = text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            if (lines.Length != 2) return text;

            string line1 = lines[0].Trim();
            string line2 = lines[1].Trim();

            string cleanLine1 = Utilities.RemoveHtmlTags(line1);
            string cleanLine2 = Utilities.RemoveHtmlTags(line2);

            if (cleanLine1.StartsWith("-") || !cleanLine2.StartsWith("-"))
                return text;

            int threshold = (int)_numericUpDownThreshold.Value;
            int line1Length = cleanLine1.Length;
            int line2Length = cleanLine2.TrimStart('-').Trim().Length;

            if (Math.Abs(line1Length - line2Length) < threshold)
                return text;

            string merged = line1 + " - " + line2.TrimStart('-').Trim();

            int middle = merged.Length / 2;
            int bestSpace = -1;
            int minDistance = merged.Length;

            for (int i = 0; i < merged.Length; i++)
            {
                if (merged[i] == ' ')
                {
                    string leftSide = merged.Substring(0, i).Trim();
                    if (leftSide.EndsWith(" -") || leftSide == "-")
                        continue;

                    int distance = Math.Abs(i - middle);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        bestSpace = i;
                    }
                }
            }

            if (bestSpace != -1)
            {
                return merged.Substring(0, bestSpace).Trim()
                     + Environment.NewLine
                     + merged.Substring(bestSpace + 1).Trim();
            }

            return text;
        }
    }
}