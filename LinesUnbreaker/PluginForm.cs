using System;
using System.Drawing;
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

            LoadConfigurations();
            _lineUnbreakerController = new LinesUnbreakerController(subtitle.Paragraphs, _configs);
            _lineUnbreakerController.TextUnbreaked += LineUnbreakerControllerTextUnbreaked;
            _isLoading = false;
            GeneratePreview();
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

            labelTotal.Text = string.Format("Total: {0}", _totalFixed);
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
            var item = new ListViewItem(paragraph.Number.ToString()) { UseItemStyleForSubItems = true };
            item.SubItems.Add(paragraph.Text.Length.ToString());
            item.SubItems.Add(HtmlUtils.RemoveTags(paragraph.Text, true).Replace(Environment.NewLine, Options.UILineBreak));
            item.SubItems.Add(HtmlUtils.RemoveTags(newText, true).Replace(Environment.NewLine, Options.UILineBreak));
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
            // TODO: fix this!
            if (_isLoading)
            {
                return;
            }

            GeneratePreview();
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