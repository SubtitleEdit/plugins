using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Nikse.SubtitleEdit.PluginLogic.Converters;
using Nikse.SubtitleEdit.PluginLogic.Converters.Strategies;
using Nikse.SubtitleEdit.PluginLogic.Models;

namespace Nikse.SubtitleEdit.PluginLogic
{
    internal partial class PluginForm : Form, IConfigurable
    {
        private static readonly Color Color = Color.FromArgb(41, 57, 85);
        public string Subtitle { get; private set; }
        private readonly Subtitle _subtitle;

        //private Dictionary<string, string> _fixedTexts = new Dictionary<string, string>();
        private HiConfigs _hiConfigs;

        //private Context _hearingImpaired;
        private readonly bool _isLoading;

        public PluginForm(Subtitle subtitle, string name, string description)
        {
            InitializeComponent();

            //SetControlColor(this);
            new LinearGradientBrush(ClientRectangle, Color, Color.White, 0f);

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


            Resize += delegate { listViewFixes.Columns[listViewFixes.Columns.Count - 1].Width = -2; };

            linkLabel1.DoubleClick += LinkLabel1_DoubleClick;

            // force layout
            OnResize(EventArgs.Empty);

            InitComboBoxHiStyle();
            UpdateUiFromConfigs(_hiConfigs);

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
            pictureBoxDonate.Click += (s, e) => { Process.Start(StringUtils.DonateUrl); };
        }

        private void LinkLabel1_DoubleClick(object sender, EventArgs e)
        {
            Process.Start("https://github.com/SubtitleEdit/plugins/issues/new");
        }

        private void UpdateUiFromConfigs(HiConfigs configs)
        {
            checkBoxSingleLineNarrator.Checked = configs.SingleLineNarrator;
            checkBoxRemoveSpaces.Checked = configs.RemoveExtraSpaces;
            checkBoxNames.Checked = configs.NarratorToUppercase;
            checkBoxMoods.Checked = configs.MoodsToUppercase;

            // todo: clean up
            // for (int i = 0; i < comboBoxStyle.Items.Count; i++)
            // {
            //     var cbi = (ComboBoxItem) comboBoxStyle.Items[i];
            //     if (cbi.Style == configs.Style)
            //     {
            //         //MessageBox.Show($"Test {i}");
            //         comboBoxStyle.SelectedIndex = i;
            //         break;
            //     }
            // }
        }

        private void InitComboBoxHiStyle()
        {
            comboBoxStyle.Items.Add(new NoneConverterStrategy());
            comboBoxStyle.Items.Add(new TitleCaseConverterStrategy());
            comboBoxStyle.Items.Add(new UppercaseConverterStrategy());
            comboBoxStyle.Items.Add(new LowercaseConverterStrategy());
            comboBoxStyle.Items.Add(new UpperLowercase());
            comboBoxStyle.Items.Add(new TitleWordsConverterStrategy());
            // set default selected to titlecase
            comboBoxStyle.SelectedIndex = 1;
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
            foreach (ListViewItem listViewItem in listViewFixes.Items)
            {
                if (!listViewItem.Checked)
                {
                    continue;
                }

                var record = (Record) listViewItem.Tag;
                record.Paragraph.Text = record.After;
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
                    string text = ((Paragraph) listViewFixes.FocusedItem.Tag).ToString();
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
            // _hiConfigs.Style = ((ComboBoxItem) comboBoxStyle.SelectedItem).Style;

            listViewFixes.BeginUpdate();
            listViewFixes.Items.Clear();

            var controller = new ConverterContext();
            foreach (ICasingConverter command in GetCommands())
            {
                command.Convert(_subtitle.Paragraphs, controller);
            }

            foreach (Record record in controller.Records.OrderBy(r => r.Paragraph.Number))
            {
                var item = new ListViewItem(string.Empty) {Checked = true};
                item.SubItems.Add(record.Paragraph.Number.ToString());
                item.SubItems.Add(record.Comment);
                item.SubItems.Add(record.Before);
                item.SubItems.Add(record.After);
                item.Tag = record;
                listViewFixes.Items.Add(item);
            }

            //groupBox1.ForeColor = totalConvertParagraphs <= 0 ? Color.Red : Color.Green;
            //groupBox1.Text = $@"Total Found: {totalConvertParagraphs}";
            /*this.listViewFixes.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            this.listViewFixes.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);*/
            //Application.DoEvents();
            listViewFixes.EndUpdate();
            Refresh();
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
            int newWidth = (listViewFixes.Width - (listViewFixes.Columns[0].Width + listViewFixes.Columns[1].Width +
                                                   listViewFixes.Columns[2].Width)) / 2;
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

        private ICollection<ICasingConverter> GetCommands()
        {
            var commands = new List<ICasingConverter>();
            // if (checkBoxRemoveSpaces.Checked)
            // {
            //     commands.Add(new RemoveExtraSpaceCasingConverter());
            // }

            if (checkBoxMoods.Checked)
            {
                commands.Add(new MoodCasingConverter(GetStrategy()));
            }

            if (checkBoxNames.Checked)
            {
                commands.Add(new NarratorCasingConverter(GetStrategy()));
            }

            return commands;
        }

        private IConverterStrategy GetStrategy() => (IConverterStrategy) comboBoxStyle.SelectedItem;

        public void LoadConfigurations()
        {
            string configFile = Path.Combine(FileUtils.Plugins, "hi2uc-config.xml");

            // load from existing file
            if (File.Exists(configFile))
            {
                _hiConfigs = HiConfigs.LoadConfiguration(configFile);
            }
            else
            {
                _hiConfigs = new HiConfigs(configFile);
                _hiConfigs.SaveConfigurations();
            }

            //_hearingImpaired = new Context(_hiConfigs);
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

            //ListViewItem selItem = listViewFixes.SelectedItems[0];
            //textBoxParagraphText.Text = _fixedTexts[((Paragraph)selItem.Tag).Id];
            //selItem.SubItems[4].Text = _fixedTexts[((Paragraph)selItem.Tag).Id];

            return;

            //textBoxParagraphText.DataBindings.Clear();
            //var selParagraph = selItem.Tag as Paragraph;

            // bind Textbox's text property to selected paragraph in listview
            //textBoxParagraphText.DataBindings.Add("Text", selParagraph, "Text");
            // at this point paragraph's text property will be update, then use the updated text to update update _fixtedtext dictionary
            //_fixedTexts[selParagraph.Id] = selParagraph.Text;
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
            //string closeTag = $"</{tag[1]}>";
            //foreach (ListViewItem lvi in listViewFixes.SelectedItems)
            //{
            //    var p = (Paragraph)lvi.Tag;
            //    string value = _fixedTexts[p.Id];
            //    value = HtmlUtils.RemoveOpenCloseTags(value, tag[1].ToString());
            //    value = $"{tag}{value}{closeTag}";
            //    // refresh fixed values
            //    lvi.SubItems[AfterTextIndex].Text = StringUtils.GetListViewString(value, noTag: false);
            //    _fixedTexts[p.Id] = value;
            //}

            //if (listViewFixes.SelectedItems.Count > 0)
            //{
            //    textBoxParagraphText.Text = _fixedTexts[((Paragraph)listViewFixes.SelectedItems[0].Tag).Id];
            //}

            listViewFixes.EndUpdate();
        }
    }
}