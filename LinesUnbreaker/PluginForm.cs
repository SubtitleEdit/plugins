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
        private readonly Timer _timerUpdater = new Timer();
        private readonly Dictionary<string, string> FixedParagrahs = new Dictionary<string, string>();
        public string FixedSubtitle { get; private set; }
        //private string path = Path.Combine("Plugins", "SeLinesUnbreaker.xml");
        private readonly Subtitle _subtitle;
        private XElement _xmlSetting;
        private bool _allowFixes;
        private int _totalFixed;
        private int _maxLineLength;

        // Var used to track user click.
        private bool _updateListview;

        private readonly char[] MoodsChars = { '(', '[' };
        private readonly Regex NarratorRegex = new Regex(":\\B", RegexOptions.Compiled);

        public PluginForm()
        {
            InitializeComponent();
        }

        public PluginForm(Subtitle subtitle, string name, string description, Form parentForm)
            : this()
        {
            // TODO: Complete member initialization
            _subtitle = subtitle;
            FormClosing += delegate
            {
                LoadSettingsIfThereIs(false); // store in xml file
            };

            LoadSettingsIfThereIs(true);
            GeneratePreview();

            // Timer updater
            _timerUpdater.Tick += _timerUpdater_Tick;
            _timerUpdater.Interval = 500; // 1/5 second.
            _timerUpdater.Start();
        }

        private void _timerUpdater_Tick(object sender, EventArgs e)
        {
            // Ignore event if was changed.
            if (!_updateListview) return;
            GeneratePreview();
            _updateListview = false;
        }

        private void LoadSettingsIfThereIs(bool load)
        {
            var path = GetSettingsFileName();
            if (!File.Exists(path))
                return;
            try
            {
                if (load)
                {
                    // load
                    if (File.Exists(path))
                    {
                        _xmlSetting = XElement.Load(path);
                        int val;
                        int.TryParse(_xmlSetting.Element("Shorterthan").Value, out val);
                        if (val > 0)
                            numericUpDown1.Value = val;

                        checkBoxSkipDialog.Checked = bool.Parse(_xmlSetting.Element("SkipDialog").Value);
                        checkBoxSkipNarrator.Checked = bool.Parse(_xmlSetting.Element("SkipNarrator").Value);
                        checkBoxMoods.Checked = bool.Parse(_xmlSetting.Element("SkipMoods").Value);
                    }
                    else
                    {
                        // create new one & save
                        _xmlSetting = new XElement("SeLinesUnbreaker",
                            new XElement("Shorterthan", numericUpDown1.Value),
                            new XElement("SkipDialog", true),
                            new XElement("SkipNarrator", true),
                            new XElement("SkipMoods", false)
                            );
                        try
                        {
                            _xmlSetting.Save(path);
                        }
                        catch { }
                    }
                }
                else
                {
                    // save settings
                    if (_xmlSetting == null)
                        return;
                    _xmlSetting.Element("Shorterthan").Value = numericUpDown1.Value.ToString();
                    _xmlSetting.Element("SkipMoods").Value = checkBoxMoods.Checked.ToString();
                    _xmlSetting.Element("SkipNarrator").Value = checkBoxSkipNarrator.Checked.ToString();
                    _xmlSetting.Element("SkipDialog").Value = checkBoxSkipDialog.Checked.ToString();
                    _xmlSetting.Save(path);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private string GetSettingsFileName()
        {
            // "%userprofile%Desktop\SubtitleEdit\Plugins\SeLinesUnbreaker.xml"
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);
            if (path.StartsWith("file:\\", StringComparison.Ordinal))
                path = path.Remove(0, 6);
            path = Path.Combine(path, "Plugins");
            if (!Directory.Exists(path))
                path = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Subtitle Edit"), "Plugins");
            return Path.Combine(path, "SeLinesUnbreaker.xml");
        }

        private void GeneratePreview()
        {
            _totalFixed = 0;
            FixedParagrahs.Clear();
            listView1.BeginUpdate();
            listView1.Items.Clear();
            _maxLineLength = (int)numericUpDown1.Value;
            foreach (var p in _subtitle.Paragraphs)
            {
                if (p.NumberOfLines < 2)
                    continue;
                var oldText = p.Text;
                var text = p.Text;

                text = UnbreakLines(text);
                var t = Utilities.RemoveHtmlTags(text, true);
                if (text != oldText && t.Length < numericUpDown1.Value)
                {
                    //text = Regex.Replace(text, " +" + Environment.NewLine, Environment.NewLine).Trim();
                    //text = Regex.Replace(text, Environment.NewLine + " +", Environment.NewLine).Trim();
                    if (!_allowFixes)
                    {
                        FixedParagrahs.Add(p.Id, text);
                        oldText = Utilities.RemoveHtmlTags(oldText, true);
                        AddFixToListView(p, oldText, t, t.Length.ToString());
                        _totalFixed++;
                    }
                }
            }

            if (!_allowFixes)
            {
                labelTotal.Text = string.Format("Total: {0}", _totalFixed);
                labelTotal.ForeColor = _totalFixed < 1 ? Color.Red : Color.Green;
            }
            listView1.EndUpdate();
            //listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            //listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        }

        private void AddFixToListView(Paragraph p, string before, string after, string lineLength)
        {
            var item = new ListViewItem() { Checked = true, UseItemStyleForSubItems = true, Tag = p };
            item.SubItems.Add(p.Number.ToString());
            item.SubItems.Add(lineLength);
            item.SubItems.Add(before.Replace(Environment.NewLine, Configuration.ListViewLineSeparatorString));
            item.SubItems.Add(after.Replace(Environment.NewLine, Configuration.ListViewLineSeparatorString));
            listView1.Items.Add(item);
        }

        private string UnbreakLines(string s)
        {
            var temp = Utilities.RemoveHtmlTags(s, true);
            temp = temp.Replace("  ", " ").Trim();

            if (checkBoxSkipDialog.Checked && (temp.StartsWith('-') || temp.Contains(Environment.NewLine + "-")))
            {
                return s;
            }
            if (checkBoxMoods.Checked && temp.IndexOfAny(MoodsChars) >= 0)
            {
                return s;
            }
            if (checkBoxSkipNarrator.Checked && NarratorRegex.IsMatch(temp))
            {
                return s;
            }


            s = s.Replace(Environment.NewLine, " ");
            if (s.Contains("</")) // Fix tag
            {
                s = s.Replace("</i> <i>", " ");
                s = s.Replace("</i><i>", " ");

                s = s.Replace("</b> <b>", " ");
                s = s.Replace("</b><b>", " ");

                s = s.Replace("</u> <u>", " ");
                s = s.Replace("</u><u>", " ");
            }

            while (s.Contains("  "))
                s = s.Replace("  ", " ");
            return s;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            _allowFixes = true;
            //GeneratePreview();
            foreach (ListViewItem item in listView1.Items)
            {
                var p = item.Tag as Paragraph;
                if (!item.Checked | p == null)
                    continue;
                if (FixedParagrahs.ContainsKey(p.Id))
                    p.Text = FixedParagrahs[p.Id];
            }
            FixedSubtitle = _subtitle.ToText();
            DialogResult = DialogResult.OK;
        }

        private void PluginForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.Cancel;
        }

        private void SelectionHandler(object sender, EventArgs e)
        {
            if (listView1.Items.Count <= 0)
                return;

            listView1.BeginUpdate();
            bool selAll = sender == buttonCheckAll;
            foreach (ListViewItem item in listView1.Items)
            {
                item.Checked = selAll ? selAll : !item.Checked;
            }
            listView1.EndUpdate();
        }

        private void listView1_Resize(object sender, EventArgs e)
        {
            var l = 0;
            for (int i = 0; i < 3; i++)
            {
                l += listView1.Columns[i].Width;
            }

            var newWidth = (listView1.Width - l) >> 1;
            listView1.Columns[3].Width = newWidth;
            listView1.Columns[4].Width = newWidth;
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            _updateListview = true;
        }
    }
}