using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.PluginLogic
{
    internal partial class PluginForm : Form, IConfigurable
    {
        private const int AfterTextIndex = 4;
        private static readonly Color color = Color.FromArgb(41, 57, 85);
        private readonly LinearGradientBrush gradientBrush;
        public string Subtitle { get; private set; }
        private readonly Subtitle _subtitle;

        private Dictionary<string, string> _fixedTexts = new Dictionary<string, string>();
        private HIConfigs _hiConfigs;
        private HearingImpaired _hearingImpaired;

        private readonly bool _isLoading = true;

        public PluginForm(Subtitle subtitle, string name, string description)
        {
            InitializeComponent();

            //SetControlColor(this);
            gradientBrush = new LinearGradientBrush(ClientRectangle, color, Color.White, 0f);

            Text = $@"{name} - v{System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(2)}";

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

            linkLabel1.DoubleClick += LinkLabel1_DoubleClick;

            // force layout
            OnResize(EventArgs.Empty);

            InitComboBoxHIStyle();
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

            buttonNext.Click += (sender, e) =>
            {
                if (listViewFixes.Items.Count == 0 || listViewFixes.SelectedItems.Count == 0)
                {
                    return;
                }

                ListViewItem lvi = listViewFixes.SelectedItems[0];
                int nextIdx = lvi.Index + 1;
                // handle boundary
                if (nextIdx == listViewFixes.Items.Count)
                {
                    nextIdx = 0;
                }

                lvi.Selected = false;
                listViewFixes.Select();
                listViewFixes.Items[nextIdx].EnsureVisible();
                listViewFixes.Items[nextIdx].Selected = true;
            };

            buttonPrev.Click += (sender, e) =>
            {
                if (listViewFixes.Items.Count == 0 || listViewFixes.SelectedItems.Count == 0)
                {
                    return;
                }

                ListViewItem lvi = listViewFixes.SelectedItems[0];
                int preIdx = lvi.Index - 1;
                // handle boundary
                if (preIdx < 0)
                {
                    preIdx = listViewFixes.Items.Count - 1;
                }

                lvi.Selected = false;
                listViewFixes.Select();
                listViewFixes.Items[preIdx].EnsureVisible();
                listViewFixes.Items[preIdx].Selected = true;

            };

            // donate handler
            pictureBoxDonate.Click += (s, e) =>
            {
                Process.Start(StringUtils.DonateUrl);
            };
        }

        private void LinkLabel1_DoubleClick(object sender, EventArgs e)
        {
            Process.Start("https://github.com/SubtitleEdit/plugins/issues/new");
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

        private void InitComboBoxHIStyle()
        {
            comboBoxStyle.Items.Add(new ComboBoxItem("Upper case", "(HELLO)", HIStyle.UpperCase));
            comboBoxStyle.Items.Add(new ComboBoxItem("Lower case", "(hello)", HIStyle.LowerCase));
            comboBoxStyle.Items.Add(new ComboBoxItem("Title case", "(Hello World)", HIStyle.TitleCase));
            comboBoxStyle.Items.Add(new ComboBoxItem("Upper/Lower case", "(HeLlo WoRlD)", HIStyle.UpperLowerCase));
        }

        private void Btn_Cancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void Btn_Run_Click(object sender, EventArgs e)
        {
            if (listViewFixes.Items.Count > 0)
            {
                Cursor = Cursors.WaitCursor;
                listViewFixes.Resize -= ListViewFixes_Resize;
                ApplyChanges();
                Cursor = Cursors.Default;
            }
            Subtitle = _subtitle.ToText();
            DialogResult = DialogResult.OK;
        }

        private void ApplyChanges()
        {
            if (_subtitle?.Paragraphs == null || _subtitle.Paragraphs.Count == 0)
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
                var p = item.Tag as Paragraph;
                if (_fixedTexts.ContainsKey(p.ID))
                {
                    p.Text = _fixedTexts[p.ID];
                    _fixedTexts.Remove(p.ID);
                    listViewFixes.Items.Remove(item);
                }
            }
            listViewFixes.EndUpdate();
        }

        private void CheckBoxNarrator_CheckedChanged(object sender, EventArgs e)
        {
            GeneratePreview();
        }

        private void CheckTypeStyle(object sender, EventArgs e)
        {
            if (listViewFixes.Items.Count <= 0 || !(sender is ToolStripMenuItem menuItem))
            {
                return;
            }

            switch (menuItem.Text)
            {
                case "Check all":
                    {
                        for (int i = 0; i < listViewFixes.Items.Count; i++)
                        {
                            listViewFixes.Items[i].Checked = true;
                        }

                        break;
                    }
                case "Uncheck all":
                    {
                        for (int i = 0; i < listViewFixes.Items.Count; i++)
                        {
                            listViewFixes.Items[i].Checked = false;
                        }

                        break;
                    }
                case "Invert check":
                    {
                        for (int i = 0; i < listViewFixes.Items.Count; i++)
                        {
                            listViewFixes.Items[i].Checked = !listViewFixes.Items[i].Checked;
                        }

                        break;
                    }
                case "Copy":
                    {
                        string text = ((Paragraph)listViewFixes.FocusedItem.Tag).ToString();
                        Clipboard.SetText(text, TextDataFormat.UnicodeText);
                        break;
                    }
                default:
                    {
                        for (int idx = listViewFixes.SelectedIndices.Count - 1; idx >= 0; idx--)
                        {
                            int index = listViewFixes.SelectedIndices[idx];
                            if (listViewFixes.Items[idx].Tag is Paragraph p)
                            {
                                _subtitle.RemoveLine(p.Number);
                            }
                            listViewFixes.Items.RemoveAt(index);
                        }
                        _subtitle.Renumber();
                        break;
                    }
            }
        }

        private void ComboBoxStyle_SelectedIndexChanged(object sender, EventArgs e)
        {
            GeneratePreview();
        }

        private void GeneratePreview()
        {
            if (_isLoading)
            {
                return;
            }

            // update options
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
                string procText = p.Text;
                bool containsMood = false;
                bool containsNarrator = false;

                // Remove Extra Spaces inside brackets ( foobar ) to (foobar)
                if (checkBoxRemoveSpaces.Checked)
                {
                    procText = _hearingImpaired.RemoveExtraSpacesInsideTag(procText);
                }
                // (Moods and feelings)
                if (checkBoxMoods.Checked)
                {
                    string tempText = procText;
                    //Debug.WriteLine($"{tempText} => {procText}");
                    procText = _hearingImpaired.MoodsToUppercase(procText);
                    //Debug.WriteLine($"{tempText} => {procText}");
                    // only fix extra spaces if moods been changed
                    if (!procText.Equals(tempText, StringComparison.Ordinal))
                    {
                        procText = procText.FixExtraSpaces();
                        containsMood = true;
                    }
                }
                // Narrator:
                if (checkBoxNames.Checked)
                {
                    string tempText = procText;
                    procText = _hearingImpaired.NarratorToUppercase(procText);
                    if (!tempText.Equals(procText, StringComparison.Ordinal))
                    {
                        containsNarrator = true;
                    }
                }
                // changes have been made
                if (p.Text.Equals(procText, StringComparison.Ordinal) == false)
                {
                    _fixedTexts.Add(p.ID, procText);
                    AddFixToListView(p, p.Text, procText, containsMood, containsNarrator);
                }
            }
            //groupBox1.ForeColor = totalConvertParagraphs <= 0 ? Color.Red : Color.Green;
            //groupBox1.Text = $@"Total Found: {totalConvertParagraphs}";
            /*this.listViewFixes.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            this.listViewFixes.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);*/
            //Application.DoEvents();
            listViewFixes.EndUpdate();
            Refresh();
        }

        private void AddFixToListView(Paragraph p, string before, string after, bool containsMood, bool containsNarrator)
        {
            var item = new ListViewItem
            {
                UseItemStyleForSubItems = true,
                Checked = true,
                Tag = p
            };
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
            else
            {
                item.SubItems.Add("N/A");
            }

            item.SubItems.Add(StringUtils.GetListViewString(before, false));
            item.SubItems.Add(StringUtils.GetListViewString(after, false));

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
                {
                    item.BackColor = Color.Pink;
                }
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
            listViewFixes.Resize -= ListViewFixes_Resize;
            ApplyChanges();
            listViewFixes.Resize += ListViewFixes_Resize;
            Cursor = Cursors.Default;
        }

        private void CheckBoxRemoveSpaces_CheckedChanged(object sender, EventArgs e)
        {
            GeneratePreview();
        }

        private void ListViewFixes_Resize(object sender, EventArgs e)
        {
            int newWidth = (listViewFixes.Width - (listViewFixes.Columns[0].Width + listViewFixes.Columns[1].Width + listViewFixes.Columns[2].Width)) / 2;
            listViewFixes.Columns[3].Width = newWidth;
            listViewFixes.Columns[4].Width = newWidth;
        }

        private void CheckBoxMoods_CheckedChanged(object sender, EventArgs e)
        {
            GeneratePreview();
        }

        private void CheckBoxSingleLineNarrator_CheckedChanged(object sender, EventArgs e)
        {
            GeneratePreview();
        }

        public void LoadConfigurations()
        {
            string configFile = Path.Combine(FileUtils.Plugins, "hi2uc-config.xml");

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

        public void SaveConfigurations()
        {
            _hiConfigs.SaveConfigurations();
        }

        private void PluginForm_Paint(object sender, PaintEventArgs e)
        {
            // ignore form painting operation
            return;
            //e.Graphics.FillRectangle(gradientBrush, ClientRectangle);
        }

        private void ListViewFixes_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewFixes.SelectedItems.Count == 0)
            {
                return;
            }

            // todo: get updated text from text-box

            ListViewItem selItem = listViewFixes.SelectedItems[0];
            textBoxParagraphText.Text = _fixedTexts[((Paragraph)selItem.Tag).ID];
            selItem.SubItems[4].Text = _fixedTexts[((Paragraph)selItem.Tag).ID];

            return;

            textBoxParagraphText.DataBindings.Clear();
            var selParagraph = selItem.Tag as Paragraph;

            // bind Textbox's text property to selected paragraph in listview
            textBoxParagraphText.DataBindings.Add("Text", selParagraph, "Text");
            // at this point paragraph's text property will be update, then use the updated text to update update _fixtedtext dictionary
            _fixedTexts[selParagraph.ID] = selParagraph.Text;
            //textBoxParagraphText.DataBindings.Add("Text", selItem.SubItems[3], "Text", false, DataSourceUpdateMode.OnPropertyChanged);

            // todo: issues
            // when set italic/bold/underline is clicked it will use listview.subitems text 
            // to update _fixedTexts...
            // the binding is becoming complicated at this point, because we bound Paragraph.Text => textBoxParagraphText.Text
        }

        private void ButtonItalic_Click(object sender, EventArgs e)
        {
            SetTag("<i>");
        }

        private void ButtonBold_Click(object sender, EventArgs e)
        {
            SetTag("<b>");
        }

        private void ButtonUnderline_Click(object sender, EventArgs e)
        {
            SetTag("<u>");
        }

        private void SetTag(string tag)
        {
            if (listViewFixes.SelectedItems.Count == 0)
            {
                return;
            }
            listViewFixes.BeginUpdate();
            string closeTag = $"</{tag[1]}>";
            foreach (ListViewItem lvi in listViewFixes.SelectedItems)
            {
                var p = (Paragraph)lvi.Tag;
                string value = _fixedTexts[p.ID];
                value = HtmlUtils.RemoveOpenCloseTags(value, tag[1].ToString());
                value = $"{tag}{value}{closeTag}";

                // refresh fixed values
                lvi.SubItems[AfterTextIndex].Text = StringUtils.GetListViewString(value, noTag: false);
                _fixedTexts[p.ID] = value;
            }
            if (listViewFixes.SelectedItems.Count > 0)
            {
                textBoxParagraphText.Text = _fixedTexts[((Paragraph)listViewFixes.SelectedItems[0].Tag).ID];
            }
            listViewFixes.EndUpdate();
        }
    }
}
