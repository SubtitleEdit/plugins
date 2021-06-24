using SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;

namespace SubtitleEdit
{
    public partial class MainForm : Form
    {
        private readonly Subtitle _subtitle;
        private readonly int[] _selectedIndices;
        private int[] _advancedIndices;
        public string FixedSubtitle { get; private set; }

        public MainForm()
        {
            InitializeComponent();
            KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Escape)
                {
                    DialogResult = DialogResult.Cancel;
                }
                else if (e.KeyCode == Keys.Enter)
                {
                    OnClick(EventArgs.Empty);
                }
            };
            RestoreSettings();
        }

        public MainForm(Subtitle sub, string title, string description, Form parentForm)
            : this()
        {
            Text = title;
            _subtitle = sub;

            _advancedIndices = new int[0];
            radioButtonSelectedLines.Checked = true;
            labelAdvancedSelection.Text = string.Empty;
            var selectedLines = AdvancedSubStationAlpha.GetTag("SelectedLines", "[Script Info]", _subtitle.Header);
            var selectedIndices = new List<int>();
            foreach (var selectedLine in selectedLines.Split(',', ' ', ':'))
            {
                if (int.TryParse(selectedLine, out var number))
                {
                    selectedIndices.Add(number);
                }
            }

            _selectedIndices = selectedIndices.ToArray();
            radioButtonSelectedLines.Text = $"Selected lines: {_selectedIndices.Length}";
            if (_selectedIndices.Length > 0)
            {
                radioButtonSelectedLines.Checked = true;
            }
            else
            {
                radioButtonAllLines.Checked = true;
            }

            radioButtonAdvancedSelection_CheckedChanged(null, null);

            GeneratePreview();
        }

        public sealed override string Text
        {
            get => base.Text;
            set => base.Text = value;
        }

        private void GeneratePreview()
        {
            if (_subtitle == null)
            {
                return;
            }
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            SaveSettings();

            var styleToApply = "{\\fad(" + numericUpDownFadeIn.Value + "," + numericUpDownFadeOut.Value + ")}";

            var indices = _selectedIndices;
            if (radioButtonAllLines.Checked)
            {
                indices = Enumerable.Range(0, _subtitle.Paragraphs.Count).ToArray();
            }
            else if (radioButtonAdvancedSelection.Checked)
            {
                indices = _advancedIndices;
            }

            var s = new Subtitle(_subtitle);
            for (int i = 0; i < s.Paragraphs.Count; i++)
            {
                if (!indices.Contains(i))
                {
                    continue;
                }

                var p = s.Paragraphs[i];

                // remove fade tags 
                if (p.Text.Contains("\\fad"))
                {
                    p.Text = Regex.Replace(p.Text, @"{\\fad\([\d\.,]*\)}", string.Empty);
                    p.Text = Regex.Replace(p.Text, @"\\fad\([\d\.,]*\)", string.Empty);
                    p.Text = Regex.Replace(p.Text, @"{\\fade\([\d\.,]*\)}", string.Empty);
                    p.Text = Regex.Replace(p.Text, @"\\fade\([\d\.,]*\)", string.Empty);
                }

                if (p.Text.StartsWith("{\\", StringComparison.Ordinal) && styleToApply.EndsWith('}'))
                {
                    p.Text = styleToApply.TrimEnd('}') + p.Text.Remove(0, 1);
                }
                else
                {
                    p.Text = styleToApply + p.Text;
                }
            }

            FixedSubtitle = s.ToText(new AdvancedSubStationAlpha());
            DialogResult = DialogResult.OK;
        }

        private string GetSettingsFileName()
        {
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);
            if (path != null && path.StartsWith("file:\\", StringComparison.Ordinal))
            {
                path = path.Remove(0, 6);
            }

            path = Path.Combine(path, "Plugins");
            if (!Directory.Exists(path))
            {
                path = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Subtitle Edit"), "Plugins");
            }

            return Path.Combine(path, "AssaFade.xml");
        }

        private void RestoreSettings()
        {
            try
            {
                var doc = new XmlDocument();
                doc.Load(GetSettingsFileName());
                numericUpDownFadeIn.Value = decimal.Parse(doc.DocumentElement.SelectSingleNode("FadeInMs").InnerText, CultureInfo.InvariantCulture);
                numericUpDownFadeOut.Value = decimal.Parse(doc.DocumentElement.SelectSingleNode("FadeOutMs").InnerText, CultureInfo.InvariantCulture);
            }
            catch
            {
                // ignore
            }
        }

        private void SaveSettings()
        {
            try
            {
                var doc = new XmlDocument();
                doc.LoadXml("<AssaFade><FadeInMs/><FadeOutMs/></AssaFade>");
                doc.DocumentElement.SelectSingleNode("FadeInMs").InnerText = numericUpDownFadeIn.Value.ToString(CultureInfo.InvariantCulture);
                doc.DocumentElement.SelectSingleNode("FadeOutMs").InnerText = numericUpDownFadeOut.Value.ToString(CultureInfo.InvariantCulture);
                doc.Save(GetSettingsFileName());
            }
            catch
            {
                // ignore
            }
        }

        private void buttonAdvancedSelection_Click(object sender, EventArgs e)
        {
            using (var form = new AdvancedSelectionHelper(_subtitle))
            {
                if (form.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                _advancedIndices = form.Indices;
                labelAdvancedSelection.Text = string.Format("Lines selected: {0}", _advancedIndices.Length);
            }
        }

        private void radioButtonAdvancedSelection_CheckedChanged(object sender, EventArgs e)
        {
            buttonAdvancedSelection.Enabled = radioButtonAdvancedSelection.Checked;
            labelAdvancedSelection.Enabled = radioButtonAdvancedSelection.Checked;
        }
    }
}