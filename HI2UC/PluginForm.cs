using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Nikse.SubtitleEdit.PluginLogic.Common;
using Nikse.SubtitleEdit.PluginLogic.Converters;
using Nikse.SubtitleEdit.PluginLogic.Converters.Strategies;
using Nikse.SubtitleEdit.PluginLogic.Models;

namespace Nikse.SubtitleEdit.PluginLogic
{
    internal partial class PluginForm : Form, IConfigurable
    {
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
            // new LinearGradientBrush(ClientRectangle, Color, Color.White, 0f);

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
            // ReSharper disable once VirtualMemberCallInConstructor
            OnResize(EventArgs.Empty);

            InitComboBoxHiStyle();
            UpdateUiFromConfigs(_hiConfigs);

            _isLoading = false;
            GeneratePreview();

            // donate handler
            pictureBoxDonate.Click += (s, e) => { Process.Start(StringUtils.DonateUrl); };
        }

        private void LinkLabel1_DoubleClick(object sender, EventArgs e)
        {
            Process.Start("https://github.com/SubtitleEdit/plugins/issues/new");
        }

        private void UpdateUiFromConfigs(HiConfigs configs)
        {
            checkBoxNames.Checked = configs.NarratorToUppercase;
            checkBoxMoods.Checked = configs.MoodsToUppercase;
        }

        private void InitComboBoxHiStyle()
        {
            var chuckReader = new ChunkReader();
            comboBoxStyle.Items.Add(new NoneConverterStrategy());
            comboBoxStyle.Items.Add(new SentenceCaseConverter());
            comboBoxStyle.Items.Add(new UppercaseConverterStrategy(chuckReader));
            comboBoxStyle.Items.Add(new LowercaseConverterStrategy());
            comboBoxStyle.Items.Add(new UpperLowercase());
            comboBoxStyle.Items.Add(new TitleWordsConverterStrategy(chuckReader));
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
                    for (var i = 0; i < listViewFixes.Items.Count; i++)
                    {
                        listViewFixes.Items[i].Checked = true;
                    }

                    break;
                }
                case "Uncheck all":
                {
                    for (var i = 0; i < listViewFixes.Items.Count; i++)
                    {
                        listViewFixes.Items[i].Checked = false;
                    }

                    break;
                }
                case "Invert check":
                {
                    for (var i = 0; i < listViewFixes.Items.Count; i++)
                    {
                        listViewFixes.Items[i].Checked = !listViewFixes.Items[i].Checked;
                    }

                    break;
                }
                case "Copy":
                {
                    var text = ((Paragraph) listViewFixes.FocusedItem.Tag).ToString();
                    Clipboard.SetText(text, TextDataFormat.UnicodeText);
                    break;
                }
                default:
                {
                    for (var idx = listViewFixes.SelectedIndices.Count - 1; idx >= 0; idx--)
                    {
                        var index = listViewFixes.SelectedIndices[idx];
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

            listViewFixes.BeginUpdate();
            listViewFixes.Items.Clear();

            var converterContext = new ConverterContext();
            foreach (var converter in GetConverters())
            {
                converter.Convert(_subtitle.Paragraphs, converterContext);
            }

            foreach (var record in converterContext.Records.OrderBy(r => r.Paragraph.Number))
            {
                var item = new ListViewItem(string.Empty) { Checked = true };
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
            listViewFixes.EndUpdate();
            Refresh();
        }

        private void ListViewFixes_Resize(object sender, EventArgs e)
        {
            var newWidth = (listViewFixes.Width - (listViewFixes.Columns[0].Width + listViewFixes.Columns[1].Width +
                                                   listViewFixes.Columns[2].Width)) / 2;
            listViewFixes.Columns[3].Width = newWidth;
            listViewFixes.Columns[4].Width = newWidth;
        }

        private void CheckBoxMoods_CheckedChanged(object sender, EventArgs e)
        {
            GeneratePreview();
        }

        private ICollection<ICasingConverter> GetConverters()
        {
            var commands = new List<ICasingConverter>();

            var converterStrategy = GetStrategy();
            if (checkBoxMoods.Checked)
            {
                commands.Add(new MoodCasingConverter(converterStrategy));
            }

            if (checkBoxNames.Checked)
            {
                commands.Add(new NarratorCasingConverter(converterStrategy));
            }

            return commands;
        }

        private IConverterStrategy GetStrategy() => (IConverterStrategy) comboBoxStyle.SelectedItem;

        public void LoadConfigurations()
        {
            var configFile = Path.Combine(FileUtils.Plugins, "hi2uc-config.xml");

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
    }
}