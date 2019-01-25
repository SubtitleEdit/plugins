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

        private bool _isLoading = true;

        public PluginForm(Subtitle subtitle)
        {
            InitializeComponent();

            _subtitle = subtitle;

            // Save user-configuartions on form-close.
            FormClosing += delegate
            {
                _configs.SaveConfigurations();
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
                AddToListView(e.Paragraph, e.NewText);
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

        private void AddToListView(Paragraph paragraph, string newText)
        {
            string noTagOldText = HtmlUtils.RemoveTags(paragraph.Text);

            // length of only visilbe characters
            int lineLength = noTagOldText.Length - (StringUtils.CountTagInText(noTagOldText, Environment.NewLine) * Environment.NewLine.Length);

            var item = new ListViewItem(paragraph.Number.ToString())
            {
                UseItemStyleForSubItems = true,
                SubItems =
                {
                   lineLength.ToString(CultureInfo.InvariantCulture), // line length
                    StringUtils.GetListViewString(paragraph.Text, true), // old text
                    StringUtils.GetListViewString(newText, true) // new text
                }
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
            _lineUnbreakerController.Action();
            Subtiitle = _subtitle.ToText();
            DialogResult = DialogResult.OK;
        }

        private void PluginForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.Cancel;
        }

        private void ListView1_Resize(object sender, EventArgs e)
        {
            var l = 0;
            for (int i = 0; i < 2; i++)
            {
                l += listView1.Columns[i].Width;
            }
            var newWidth = (listView1.Width - l) >> 1;
            listView1.Columns[2].Width = newWidth;
            listView1.Columns[3].Width = newWidth;
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
            string configFile = Path.Combine(FileUtils.Plugins, "linesunbreaker-config.xml");

            // load configuration from file
            if (File.Exists(configFile))
            {
                _configs = UnBreakConfigs.LoadConfiguration(configFile);
            }
            else
            {
                _configs = new UnBreakConfigs(configFile);
                _configs.SaveConfigurations();
            }

            checkBoxMoods.Checked = _configs.SkipMoods;
            checkBoxSkipDialog.Checked = _configs.SkipDialogs;
            checkBoxSkipNarrator.Checked = _configs.SkipNarrator;

            if (_configs.MaxLineLength < numericUpDown1.Minimum)
            {
                _configs.MaxLineLength = Convert.ToInt32(numericUpDown1.Minimum);
                numericUpDown1.Minimum = 0;
            }
            else
            {
                numericUpDown1.Value = _configs.MaxLineLength;
            }
        }

        public void SaveConfigurations() => _configs.SaveConfigurations();
    }
}