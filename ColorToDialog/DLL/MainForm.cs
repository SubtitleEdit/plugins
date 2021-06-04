using ColorToDialog.Logic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;

namespace ColorToDialog
{
    public partial class MainForm : Form
    {
        private Subtitle _subtitle;
        private string _dash;
        private readonly Subtitle _subtitleOriginal;
        private bool _trackTextChange;
        private readonly Dictionary<int, string> _changedTexts;

        public string FixedSubtitle { get; private set; }

        public MainForm()
        {
            InitializeComponent();
        }

        public MainForm(Subtitle sub, string title, string description, Form parentForm)
            : this()
        {
            Text = title;
            _changedTexts = new Dictionary<int, string>();
            _subtitle = sub;
            _subtitleOriginal = new Subtitle(sub);
            _dash = "- ";
            comboBoxDash.SelectedIndex = 0;
            RestoreSettings();
            GeneratePreview();
            listView1_Resize(null, null);
        }

        public sealed override string Text
        {
            get => base.Text;
            set => base.Text = value;
        }

        private void GeneratePreview()
        {
            var dash = _dash;
            if (_subtitle == null || string.IsNullOrEmpty(dash))
            {
                return;
            }

            _dash = dash;
            try
            {
                listView1.Items.Clear();
                _subtitle = new Subtitle(_subtitleOriginal);
                for (var index = 0; index < _subtitleOriginal.Paragraphs.Count; index++)
                {
                    var p = _subtitle.Paragraphs[index];
                    if (p.Text.Contains("<font ", StringComparison.OrdinalIgnoreCase))
                    {
                        var after = DashAdder.GetFixedText(p.Text, dash);
                        if (_changedTexts.ContainsKey(index))
                        {
                            after = _changedTexts[index]; // use changed text
                        }
                        if (after != p.Text)
                        {
                            AddToListView(p, after);
                            p.Text = after.Trim();
                        }
                    }
                }

                FixedSubtitle = _subtitle.ToText(new SubRip());

            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message + Environment.NewLine + exception.StackTrace);
            }
        }

        private void AddToListView(Paragraph p, string after)
        {
            var item = new ListViewItem(p.Number.ToString(CultureInfo.InvariantCulture)) { Tag = p };
            item.SubItems.Add(p.Text.Replace(Environment.NewLine, "<br />"));
            item.SubItems.Add(after.Replace(Environment.NewLine, "<br />"));
            listView1.Items.Add(item);
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            SaveSettings();
            FixedSubtitle = _subtitle.ToText(new SubRip());
            DialogResult = DialogResult.OK;
        }

        private void listView1_Resize(object sender, EventArgs e)
        {
            var size = listView1.Width / 2 - 10;
            listView1.Columns[1].Width = size;
            listView1.Columns[2].Width = -2;
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private static string GetSettingsFileName()
        {
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);
            if (path != null && path.StartsWith("file:\\", StringComparison.Ordinal))
                path = path.Remove(0, 6);
            path = Path.Combine(path, "Plugins");
            if (!Directory.Exists(path))
                path = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Subtitle Edit"), "Plugins");
            return Path.Combine(path, "ColorToDialog.xml");
        }

        private void RestoreSettings()
        {
            string fileName = GetSettingsFileName();
            try
            {
                var doc = new XmlDocument();
                doc.Load(fileName);
                _dash = doc.DocumentElement.SelectSingleNode("Dash").InnerText;
                comboBoxDash.SelectedIndex = _dash == "-" ? 1 : 0;
            }
            catch
            {
                comboBoxDash.SelectedIndex = 0;
                _dash = "- ";
            }
        }

        private void SaveSettings()
        {
            var fileName = GetSettingsFileName();
            try
            {
                var doc = new XmlDocument();
                doc.LoadXml("<ColorToDialog><Dash/></ColorToDialog>");
                doc.DocumentElement.SelectSingleNode("Dash").InnerText = _dash;
                doc.Save(fileName);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
        }

        private void MainForm_ResizeEnd(object sender, EventArgs e)
        {
            listView1.Columns[2].Width = -2;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            MainForm_ResizeEnd(sender, e);
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            MainForm_ResizeEnd(sender, e);
        }

        private void comboBoxDash_SelectedIndexChanged(object sender, EventArgs e)
        {
            _dash = comboBoxDash.SelectedIndex == 1 ? "-" : "- ";
            GeneratePreview();
        }

        private void comboBoxDash_TextChanged(object sender, EventArgs e)
        {
            GeneratePreview();
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count <= 0)
            {
                _trackTextChange = false;
                textBox1.Text = string.Empty;
                textBox1.Enabled = false;
                return;
            }

            _trackTextChange = false;
            textBox1.Enabled = true;
            var idx = listView1.SelectedItems[0].Index;
            var p = (Paragraph)listView1.Items[idx].Tag;
            textBox1.Text = p.Text;
            _trackTextChange = true;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (!_trackTextChange || listView1.SelectedItems.Count <= 0)
            {
                return;
            }

            var idx = listView1.SelectedItems[0].Index;
            var p = (Paragraph)listView1.Items[idx].Tag;
            p.Text = textBox1.Text;
            var pIdx = _subtitle.Paragraphs.IndexOf(p);
            if (_changedTexts.ContainsKey(pIdx))
            {
                _changedTexts[pIdx] = textBox1.Text;
            }
            else
            {
                _changedTexts.Add(pIdx, textBox1.Text);
            }
        }
    }
}