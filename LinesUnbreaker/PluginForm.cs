using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.PluginLogic
{
    internal partial class PluginForm : Form, IConfigurable
    {
        //private string path = Path.Combine("Plugins", "SeLinesUnbreaker.xml");
        private readonly Subtitle _subtitle;
        private int _totalFixed;

        public string Subtiitle { get; private set; }

        // Var used to track user click.
        private bool _updateListview;

        private readonly LinesUnbreakerController _lineUnbreakerController;

        private UnBreakConfigs _configs;

        public PluginForm(Subtitle subtitle)
        {
            InitializeComponent();

            _subtitle = subtitle;

            // Save user-configuartions on form-close.
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

            // disable triggerer controls
            ChangeControlsState(false);

            LoadConfigurations();
            _lineUnbreakerController = new LinesUnbreakerController(subtitle.Paragraphs, _configs);
            _lineUnbreakerController.TextUnbreaked += LineUnbreakerControllerTextUnbreaked;

            // restore trigger states
            ChangeControlsState(true);
            GeneratePreview();
        }

        /// <summary>
        /// Disable all the control in order to prevent triggering changes event.
        /// </summary>
        /// <param name="state"></param>
        private void ChangeControlsState(bool state)
        {
            checkBoxMoods.Enabled = state;
            checkBoxSkipDialog.Enabled = state;
            checkBoxSkipNarrator.Enabled = state;
            numericUpDown1.Enabled = state;
        }

        private void LineUnbreakerControllerTextUnbreaked(object sender, ParagraphEventArgs e)
        {
            // Update View
            if (_updateListview)
            {
                _totalFixed++;
                AddToListView(e);
            }
            else // Invoked by button OK.
            {
                e.Paragraph.Text = e.NewText;
            }
        }

        private void GeneratePreview()
        {
            _totalFixed = 0;
            listView1.BeginUpdate();
            listView1.Items.Clear();
            UpdateConfigurations();
            _updateListview = true;
            _lineUnbreakerController.Action();

            labelTotal.Text = $"Total: {_totalFixed}";
            labelTotal.ForeColor = _totalFixed < 1 ? Color.Red : Color.Green;
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

        private void AddToListView(ParagraphEventArgs prgEventArgs)
        {
            var noTagOldText = HtmlUtils.RemoveTags(prgEventArgs.Paragraph.Text);

            // length of only visible characters
            var lineLength = noTagOldText.Length - (StringUtils.CountTagInText(noTagOldText, Environment.NewLine) * Environment.NewLine.Length);

            var item = new ListViewItem(string.Empty)
            {
                Checked = true,
                UseItemStyleForSubItems = true,
                SubItems =
                {
                    prgEventArgs.Paragraph.Number.ToString(),
                    lineLength.ToString(CultureInfo.InvariantCulture), // line length
                    StringUtils.GetListViewString(prgEventArgs.Paragraph.Text, true), // old text
                    StringUtils.GetListViewString(prgEventArgs.NewText, true) // new text
                },
                Tag = prgEventArgs
            };
            listView1.Items.Add(item);
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            _updateListview = false;
            //_lineUnbreakerController.Action();
            foreach (ListViewItem lvi in listView1.Items)
            {
                // changes not accepted
                if (!lvi.Checked)
                {
                    continue;
                }

                // update paragraph with new text
                var prgEventArgs = (ParagraphEventArgs)lvi.Tag;
                prgEventArgs.Paragraph.Text = prgEventArgs.NewText;
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

            // text columns indeces
            var IndexBeforeChanges = listView1.Columns.Count - 2;
            var IndexAfterChanges = listView1.Columns.Count - 1;

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
            listView1.Columns[IndexBeforeChanges].Width = remainingWidth;
            // after changes
            listView1.Columns[IndexAfterChanges].Width = remainingWidth;
        }

        private void ConfigurationChanged(object sender, EventArgs e)
        {
            if (_updateListview)
            {
                GeneratePreview();
            }
        }

        public void LoadConfigurations()
        {
            var configFile = Path.Combine(FileUtils.Plugins, "linesunbreaker-config.xml");
            _configs = LoadOrCreateConfiguration(configFile);
            ApplyConfigurations();
        }

        private UnBreakConfigs LoadOrCreateConfiguration(string configFile)
        {
            // load configuration from file
            if (File.Exists(configFile))
            {
                return UnBreakConfigs.LoadConfiguration(configFile);
            }

            // unable to load configuration file
            // generate and save new configuration file
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

        public void SaveConfigurations()
        {
            _configs.SaveConfigurations();
        }

        private void ReportProblemToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/SubtitleEdit/plugins/issues/new");
        }

        private void CheckAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem lvi in listView1.Items)
            {
                lvi.Checked = true;
            }
        }

        private void UncheckAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem lvi in listView1.Items)
            {
                lvi.Checked = false;
            }
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