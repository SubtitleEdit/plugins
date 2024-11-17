using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Nikse.SubtitleEdit.PluginLogic.Extensions;
using Nikse.SubtitleEdit.PluginLogic.UnbreakLine;

namespace Nikse.SubtitleEdit.PluginLogic
{
    internal partial class PluginForm : Form, IConfigurable
    {
        //private string path = Path.Combine("Plugins", "SeLinesUnbreaker.xml");
        private readonly Subtitle _subtitle;

        public string Subtitle { get; private set; }

        private readonly RemoveLineBreak _removeLineBreak;

        private UnBreakConfigs _configs;
        private ICollection<RemoveLineBreakResult> _removeLineBreakItems;

        public PluginForm(Subtitle subtitle)
        {
            InitializeComponent();

            _subtitle = subtitle;

            // Save user-configs on form-close.
            FormClosing += delegate { SaveConfigurations(); };

            linkLabelGithub.Click += (sender, e) => System.Diagnostics.Process.Start(linkLabelGithub.Tag.ToString());

            // donate handler
            pictureBoxDonate.Click += (s, e) => { System.Diagnostics.Process.Start(StringUtils.DonateUrl); };

            LoadConfigurations();
            _removeLineBreak = new RemoveLineBreak(_configs);

            // hook event handler after configuration and line-unbreaker read
            checkBoxSkipDialog.CheckedChanged += ConfigurationChanged;
            checkBoxSkipNarrator.CheckedChanged += ConfigurationChanged;
            checkBoxMoods.CheckedChanged += ConfigurationChanged;
            numericUpDown1.ValueChanged += ConfigurationChanged;

            // trigger first preview
            GeneratePreview();
        }

        private void GeneratePreview()
        {
            if (_removeLineBreak is null)
            {
                return;
            }

            listView1.BeginUpdate();
            listView1.Items.Clear();
            UpdateConfigurations();

            _removeLineBreakItems = _removeLineBreak.Remove(_subtitle.Paragraphs);
            foreach (var removeLineBreakResult in _removeLineBreakItems)
            {
                listView1.Items.Add(removeLineBreakResult.ToListViewItem());
            }

            labelTotal.Text = $"Total: {_removeLineBreakItems.Count}";
            labelTotal.ForeColor = _removeLineBreakItems.Count < 1 ? Color.Red : Color.Green;
            listView1.EndUpdate();
        }

        private void UpdateConfigurations()
        {
            _configs.MaxLineLength = Convert.ToInt32(numericUpDown1.Value);
            _configs.SkipDialogs = checkBoxSkipDialog.Checked;
            _configs.SkipMoods = checkBoxMoods.Checked;
            _configs.SkipNarrator = checkBoxSkipNarrator.Checked;
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

            Subtitle = _subtitle.ToText();
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
            _configs = LoadOrCreateFromFile(configFile);
            ApplyConfigurations();
        }

        private UnBreakConfigs LoadOrCreateFromFile(string configFile)
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

        private void CheckAllToolStripMenuItem_Click(object sender, EventArgs e) => listView1.CheckAll();
        private void UncheckAllToolStripMenuItem_Click(object sender, EventArgs e) => listView1.UncheckAll();
        private void InvertCheckedToolStripMenuItem_Click(object sender, EventArgs e) => listView1.InvertCheck();
    }
}