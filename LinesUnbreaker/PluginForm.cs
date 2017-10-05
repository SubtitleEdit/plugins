using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Nikse.SubtitleEdit.PluginLogic
{
    internal partial class PluginForm : Form
    {
        // Used to generate previews automatically.
        private readonly Dictionary<string, string> FixedParagrahs = new Dictionary<string, string>();
        public string FixedSubtitle { get; private set; }
        //private string path = Path.Combine("Plugins", "SeLinesUnbreaker.xml");
        private readonly Subtitle _subtitle;
        private int _totalFixed;

        // Var used to track user click.
        private bool _updateListview;

        private readonly LinesUnbreakerController _lineUnbreakerController;

        private readonly Configuration _configs;
        private bool _loading;

        public PluginForm(Subtitle subtitle)
        {
            InitializeComponent();
            _subtitle = subtitle;

            // Save user-configuartions on form-close.
            FormClosing += delegate
            {
                _configs.SaveConfiguration();
            };
            _configs = new Configuration();
            _lineUnbreakerController = new LinesUnbreakerController(subtitle.Paragraphs, _configs);
            _lineUnbreakerController.TextUnbreaked += _lineUnbreakerController_TextUnbreaked;

            _loading = true;
            // Load user configurations.
            checkBoxMoods.Checked = _configs.SkipMoods;
            checkBoxSkipDialog.Checked = _configs.SkipDialogs;
            checkBoxSkipNarrator.Checked = _configs.SkipNarrator;
            if (_configs.MaxLineLength < numericUpDown1.Minimum)
            {
                _configs.MaxLineLength = Convert.ToInt32(numericUpDown1.Minimum);
            }
            _loading = false;
        }

        private void _lineUnbreakerController_TextUnbreaked(object sender, ParagraphEventArgs e)
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
            FixedParagrahs.Clear();
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
            item.SubItems.Add(Utilities.RemoveHtmlTags(paragraph.Text, true).Replace(Environment.NewLine, Configuration.ListViewLineSeparatorString));
            item.SubItems.Add(Utilities.RemoveHtmlTags(newText, true).Replace(Environment.NewLine, Configuration.ListViewLineSeparatorString));
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
            FixedSubtitle = _subtitle.ToText();
            DialogResult = DialogResult.OK;
        }

        private void PluginForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.Cancel;
        }

        private void listView1_Resize(object sender, EventArgs e)
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
            if (_loading)
            {
                return;
            }

            GeneratePreview();
        }
    }
}