using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.PluginLogic
{
    internal partial class PluginForm : Form, IConfigurable
    {
        public string Subtitle { get; private set; }
        private readonly Subtitle _subtitle;

        private Dictionary<string, string> _fixedTexts = new Dictionary<string, string>();
        private HIConfigs _hiConfigs;
        private HearingImpaired _hearingImpaired;

        private readonly bool _isLoading = true;

        public PluginForm(Subtitle subtitle, string name, string description)
        {
            InitializeComponent();
            _subtitle = subtitle;
            labelDesc.Text = "Description: " + description;

            LoadConfigurations();

            FormClosed += (s, e) =>
            {
                // update config
                //_hiConfigs.NarratorToUppercase = checkBoxNames.Checked;
                //_hiConfigs.MoodsToUppercase = checkBoxMoods.Checked;
                //_hiConfigs.RemoveExtraSpaces = checkBoxRemoveSpaces.Checked;
                //_hiConfigs.Style = (HIStyle)Enum.Parse(typeof(HIStyle), comboBoxStyle.SelectedValue.ToString());
                //_hiConfigs.Style = ((ComboBoxItem)comboBoxStyle.SelectedItem).Style;
                // TypeConverter converter = TypeDescriptor.GetConverter(typeof(HIStyle));

                SaveConfigurations();
            };

            Resize += delegate
            {
                listViewFixes.Columns[listViewFixes.Columns.Count - 1].Width = -2;
            };

            InitComboBoxHITyle();
            UpdateUIFromConfigs(_hiConfigs);

            _isLoading = false;
            GeneratePreview();
            /*
            this.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Escape)
                    this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            };
             */
        }

        private void UpdateUIFromConfigs(HIConfigs configs)
        {
            checkBoxSingleLineNarrator.Checked = configs.SingleLineNarrator;
            checkBoxRemoveSpaces.Checked = configs.RemoveExtraSpaces;
            checkBoxNames.Checked = configs.NarratorToUppercase;
            checkBoxMoods.Checked = configs.MoodsToUppercase;

            for (int i = 0; i < comboBoxStyle.Items.Count; i++)
            {
                var cbi = (ComboBoxItem)comboBoxStyle.Items[i];
                if (cbi.Style == configs.Style)
                {
                    //MessageBox.Show($"Test {i}");
                    comboBoxStyle.SelectedIndex = i;
                    break;
                }
            }
        }

        private void InitComboBoxHITyle()
        {
            comboBoxStyle.Items.Add(new ComboBoxItem("Upper case", "(HELLO)", HIStyle.UpperCase));
            comboBoxStyle.Items.Add(new ComboBoxItem("Lower case", "(hello)", HIStyle.LowerCase));
            comboBoxStyle.Items.Add(new ComboBoxItem("Title case", "(Hello World)", HIStyle.TitleCase));
            comboBoxStyle.Items.Add(new ComboBoxItem("Upper/Lower case", "(HeLlo WoRlD)", HIStyle.UpperLowerCase));
        }

        private void btn_Cancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void btn_Run_Click(object sender, EventArgs e)
        {
            if (listViewFixes.Items.Count > 0)
            {
                Cursor = Cursors.WaitCursor;
                listViewFixes.Resize -= listViewFixes_Resize;
                ApplyChanges();
                Cursor = Cursors.Default;
            }
            Subtitle = _subtitle.ToText();
            DialogResult = DialogResult.OK;
        }

        private void ApplyChanges()
        {
            if (_subtitle == null || _subtitle.Paragraphs == null || _subtitle.Paragraphs.Count == 0)
            {
                return;
            }

            listViewFixes.BeginUpdate();
            int count = listViewFixes.Items.Count;
            for (int i = count - 1; i >= 0; i--)
            {
                ListViewItem item = listViewFixes.Items[i];
                if (!item.Checked)
                {
                    continue;
                }
                var p = GetParagraph(item);
                if (_fixedTexts.ContainsKey(p.ID))
                {
                    p.Text = _fixedTexts[p.ID];
                    _fixedTexts.Remove(p.ID);
                    listViewFixes.Items.Remove(item);
                }
            }
            listViewFixes.EndUpdate();
        }

        private void checkBoxNarrator_CheckedChanged(object sender, EventArgs e) => GeneratePreview();

        private void CheckTypeStyle(object sender, EventArgs e)
        {
            var menuItem = sender as ToolStripMenuItem;
            if (listViewFixes.Items.Count <= 0 || menuItem == null)
                return;
            if (menuItem.Text == "Check all")
            {
                for (int i = 0; i < listViewFixes.Items.Count; i++)
                    listViewFixes.Items[i].Checked = true;
            }
            else if (menuItem.Text == "Uncheck all")
            {
                for (int i = 0; i < listViewFixes.Items.Count; i++)
                    listViewFixes.Items[i].Checked = false;
            }
            else if (menuItem.Text == "Invert check")
            {
                for (int i = 0; i < listViewFixes.Items.Count; i++)
                    listViewFixes.Items[i].Checked = !listViewFixes.Items[i].Checked;
            }
            else if (menuItem.Text == "Copy")
            {
                var text = (listViewFixes.FocusedItem.Tag as Paragraph).ToString();
                Clipboard.SetText(text);
            }
            else
            {
                for (int idx = listViewFixes.SelectedIndices.Count - 1; idx >= 0; idx--)
                {
                    var index = listViewFixes.SelectedIndices[idx];
                    var p = listViewFixes.Items[idx].Tag as Paragraph;
                    if (p != null)
                    {
                        _subtitle.RemoveLine(p.Number);
                    }
                    listViewFixes.Items.RemoveAt(index);
                }
                _subtitle.Renumber();
            }
        }

        private void comboBoxStyle_SelectedIndexChanged(object sender, EventArgs e) => GeneratePreview();

        private void GeneratePreview()
        {
            if (_isLoading)
            {
                return;
            }

            // update configuration
            _hiConfigs.NarratorToUppercase = checkBoxNames.Checked;
            _hiConfigs.MoodsToUppercase = checkBoxMoods.Checked;
            _hiConfigs.RemoveExtraSpaces = checkBoxRemoveSpaces.Checked;
            _hiConfigs.SingleLineNarrator = checkBoxSingleLineNarrator.Checked;
            _hiConfigs.Style = ((ComboBoxItem)comboBoxStyle.SelectedItem).Style;

            listViewFixes.BeginUpdate();
            listViewFixes.Items.Clear();

            _fixedTexts = new Dictionary<string, string>();

            foreach (Paragraph p in _subtitle.Paragraphs)
            {
                var text = p.Text;
                bool containsMood = false;
                bool containsNarrator = false;

                // Remove Extra Spaces inside brackets ( foobar ) to (foobar)
                string beforeChanges = text;
                if (checkBoxRemoveSpaces.Checked)
                {
                    _hearingImpaired.RemoveExtraSpacesInsideTag(text);
                }
                // (Moods and feelings)
                if (checkBoxMoods.Checked)
                {
                    beforeChanges = text;
                    text = _hearingImpaired.MoodsToUppercase(text);
                    if (beforeChanges != text)
                    {
                        text = text.FixExtraSpaces();
                        containsMood = true;
                    }
                }
                // Narrator:
                if (checkBoxNames.Checked)
                {
                    beforeChanges = text;
                    text = _hearingImpaired.NarratorToUppercase(text);
                    containsNarrator = !beforeChanges.Equals(text, StringComparison.Ordinal);
                }

                if (containsMood || containsNarrator)
                {
                    _fixedTexts.Add(p.ID, text);
                    string oldText = HtmlUtils.RemoveTags(p.Text, true);
                    text = HtmlUtils.RemoveTags(text, true);
                    AddFixToListView(p, oldText, text, containsMood, containsNarrator);
                }

            }

            int totalConvertParagraphs = _fixedTexts.Count;

            groupBox1.ForeColor = totalConvertParagraphs <= 0 ? Color.Red : Color.Green;
            groupBox1.Text = string.Format("Total Found: {0}", totalConvertParagraphs);
            /*this.listViewFixes.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            this.listViewFixes.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);*/
            //Application.DoEvents();
            listViewFixes.EndUpdate();
            Refresh();
        }

        private void AddFixToListView(Paragraph p, string before, string after, bool containsMood, bool containsNarrator)
        {
            var item = new ListViewItem() { Checked = true, UseItemStyleForSubItems = true, Tag = p };
            item.SubItems.Add(p.Number.ToString(CultureInfo.InvariantCulture));
            if (containsMood && containsNarrator)
            {
                item.SubItems.Add("Name & Mood");
            }
            else if (containsMood)
            {
                item.SubItems.Add("Mood");
            }
            else if (containsNarrator)
            {
                item.SubItems.Add("Narrator");
            }
            item.SubItems.Add(before.Replace(Environment.NewLine, Options.UILineBreak));
            item.SubItems.Add(after.Replace(Environment.NewLine, Options.UILineBreak));

            int idx = after.IndexOf(Environment.NewLine, StringComparison.Ordinal);
            if (idx > 2)
            {
                string firstLine = after.Substring(0, idx).Trim();
                string secondLine = after.Substring(idx + Environment.NewLine.Length).Trim();
                int idx1 = firstLine.IndexOf(':');
                int idx2 = secondLine.IndexOf(':');
                if (idx1 > 0xE || idx2 > 0xE)
                {
                    item.BackColor = Color.Pink;
                }
            }
            else
            {
                if (after.IndexOf(':') > 0xE)
                    item.BackColor = Color.Pink;
            }

            listViewFixes.Items.Add(item);
        }

        private void ButtonApply_Click(object sender, EventArgs e)
        {
            if (listViewFixes.Items.Count == 0)
            {
                return;
            }

            Cursor = Cursors.WaitCursor;
            listViewFixes.Resize -= listViewFixes_Resize;
            ApplyChanges();
            listViewFixes.Resize += listViewFixes_Resize;
            Cursor = Cursors.Default;
        }

        private void checkBoxRemoveSpaces_CheckedChanged(object sender, EventArgs e) => GeneratePreview();

        private void listViewFixes_Resize(object sender, EventArgs e)
        {
            var newWidth = (listViewFixes.Width - (listViewFixes.Columns[0].Width + listViewFixes.Columns[1].Width + listViewFixes.Columns[2].Width)) / 2;
            listViewFixes.Columns[3].Width = newWidth;
            listViewFixes.Columns[4].Width = newWidth;
        }

        private void checkBoxMoods_CheckedChanged(object sender, EventArgs e) => GeneratePreview();

        private void checkBoxSingleLineNarrator_CheckedChanged(object sender, EventArgs e) => GeneratePreview();

        private static Paragraph GetParagraph(ListViewItem lvi) => lvi.Tag as Paragraph;

        public void LoadConfigurations()
        {
            string configFile = FileUtils.GetConfigFile("hi2uc-config.xml");

            // load from existing file
            if (File.Exists(configFile))
            {
                _hiConfigs = HIConfigs.LoadConfiguration(configFile);
            }
            else
            {
                _hiConfigs = new HIConfigs(configFile);
                _hiConfigs.SaveConfigurations();
            }
            _hearingImpaired = new HearingImpaired(_hiConfigs);
        }

        public void SaveConfigurations() => _hiConfigs.SaveConfigurations();
    }
}