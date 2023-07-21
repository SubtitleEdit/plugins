using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Nikse.SubtitleEdit.PluginLogic.Models;

namespace Nikse.SubtitleEdit.PluginLogic
{
    internal partial class PluginForm : Form, IConfigurable
    {
        //private string path = Path.Combine("Plugins", "SeLinesUnbreaker.xml");
        private readonly Subtitle _subtitle;

        public string Subtiitle { get; private set; }

        private readonly RemoveLineBreak _removeLineBreak;

        private UnBreakConfigs _configs;
        private ICollection<RemoveLineBreakResult> _removeLineBreakItems;

        public PluginForm(Subtitle subtitle)
        {
            InitializeComponent();

            _subtitle = subtitle;

            // Save user-configs on form-close.
            FormClosing += delegate
            {
                _configs.SaveConfigurations();
            };

            linkLabelGithub.Click += (sender, e) => System.Diagnostics.Process.Start(linkLabelGithub.Tag.ToString());

            // donate handler
            pictureBoxDonate.Click += (s, e) =>
            {
                System.Diagnostics.Process.Start(StringUtils.DonateUrl);
            };

            LoadConfigurations();

            _removeLineBreak = new RemoveLineBreak(_configs);

            // disable triggers
            ChangeControlsState(false);

            // restore trigger states
            ChangeControlsState(true);
            
            checkBoxSkipDialog.CheckedChanged += ConfigurationChanged;
            checkBoxSkipNarrator.CheckedChanged += ConfigurationChanged;
            checkBoxMoods.CheckedChanged += ConfigurationChanged;
            numericUpDown1.ValueChanged += ConfigurationChanged;
            
            GeneratePreview();
        }

        private void ChangeControlsState(bool state)
        {
            checkBoxMoods.Enabled = state;
            checkBoxSkipDialog.Enabled = state;
            checkBoxSkipNarrator.Enabled = state;
            numericUpDown1.Enabled = state;
        }

        private void GeneratePreview()
        {
            listView1.BeginUpdate();
            listView1.Items.Clear();
            UpdateConfigurations();
            
            _removeLineBreakItems = _removeLineBreak.Remove(_subtitle.Paragraphs);
            foreach (var removeLineBreakResult in _removeLineBreakItems)
            {
                AddToListView(removeLineBreakResult);
            }

            labelTotal.Text = $"Total: {_removeLineBreakItems.Count}";
            labelTotal.ForeColor = _removeLineBreakItems.Count < 1 ? Color.Red : Color.Green;
            listView1.EndUpdate();
            //listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            //listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        }

        private void UpdateConfigurations()
        {
            _configs.MaxLineLength = Convert.ToInt32(numericUpDown1.Value);
            _configs.SkipDialogs = checkBoxSkipDialog.Checked;
            _configs.SkipMoods = checkBoxMoods.Checked;
            _configs.SkipNarrator = checkBoxSkipNarrator.Checked;
        }

        private void AddToListView(RemoveLineBreakResult removeResult)
        {
            var noTagOldText = HtmlUtils.RemoveTags(removeResult.Paragraph.Text);

            // length of only visible characters
            var lineLength = noTagOldText.Length - (StringUtils.CountTagInText(noTagOldText, Environment.NewLine) * Environment.NewLine.Length);

            var item = new ListViewItem(string.Empty)
            {
                Checked = true,
                UseItemStyleForSubItems = true,
                SubItems =
                {
                    removeResult.Paragraph.Number.ToString(),
                    lineLength.ToString(CultureInfo.InvariantCulture),
                    StringUtils.GetListViewString(removeResult.BeforeText, true),
                    StringUtils.GetListViewString(removeResult.AfterText, true)  
                },
                Tag = removeResult
            };
            listView1.Items.Add(item);
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            foreach (var removeLineBreakResult in _removeLineBreakItems)
            {
                removeLineBreakResult.Paragraph.Text = removeLineBreakResult.AfterText;
            }
            Subtiitle = _subtitle.ToText();
            DialogResult = DialogResult.OK;
        }

        private void PluginForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void ListView1_Resize(object sender, EventArgs e)
        {
            var exclusiveWidth = 0;

            // text columns indices
            var indexBeforeChanges = listView1.Columns.Count - 2;
            var indexAfterChanges = listView1.Columns.Count - 1;

            // columns from check-box to line length
            var count = listView1.Columns.Count - 2;

            // all columns width excluding last two
            for (var i = 0; i < count; i++)
            {
                exclusiveWidth += listView1.Columns[i].Width;
            }

            // cut in half
            var remainingWidth = (listView1.Width - exclusiveWidth) >> 1;

            // before changes 
            listView1.Columns[indexBeforeChanges].Width = remainingWidth;
            // after changes
            listView1.Columns[indexAfterChanges].Width = remainingWidth;
        }

        private void ConfigurationChanged(object sender, EventArgs e) => GeneratePreview();

        public void LoadConfigurations()
        {
            var configFile = Path.Combine(FileUtils.Plugins, "linesunbreaker-config.xml");
            _configs = LoadOrCreateConfiguration(configFile);
            ApplyConfigurations();
        }

        private UnBreakConfigs LoadOrCreateConfiguration(string configFile)
        {
            // load
            if (File.Exists(configFile))
            {
                return UnBreakConfigs.LoadConfiguration(configFile);
            }
            
            // create
            var newConfig = new UnBreakConfigs(configFile);
            newConfig.SaveConfigurations();
            return newConfig;
        }

        private void ApplyConfigurations()
        {
            // load configurations
            checkBoxMoods.Checked = _configs.SkipMoods;
            checkBoxSkipDialog.Checked = _configs.SkipDialogs;
            checkBoxSkipNarrator.Checked = _configs.SkipNarrator;

            if (_configs.MaxLineLength < numericUpDown1.Minimum)
            {
                _configs.MaxLineLength = Convert.ToInt32(numericUpDown1.Minimum);
                numericUpDown1.Minimum = 0;
            }

            numericUpDown1.Value = _configs.MaxLineLength;
        }

        public void SaveConfigurations() => _configs.SaveConfigurations();

        private void ReportProblemToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/SubtitleEdit/plugins/issues/new");
        }

        private void SetAllItemsChecked(bool isChecked)
        {
            foreach (ListViewItem lvi in listView1.Items)
            {
                lvi.Checked = isChecked;
            }
        }

        private void CheckAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetAllItemsChecked(true);
        }

        private void UncheckAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetAllItemsChecked(false);
        }

        private void InvertCheckedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem lvi in listView1.Items)
            {
                lvi.Checked = !lvi.Checked;
            }
        }
    }
}