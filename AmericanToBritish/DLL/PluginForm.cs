using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml.Linq;
using Nikse.SubtitleEdit.PluginLogic.Logic;
using System.Reflection;

namespace Nikse.SubtitleEdit.PluginLogic
{
    internal sealed partial class PluginForm : Form
    {
        internal string FixedSubtitle { get; private set; }
        private readonly Subtitle _subtitle;
        private int _totalFixes;
        private bool _allowFixes;
        private AmericanToBritishConverter _converter;
        private string _localFile;
        private readonly SubRip _subrip = new SubRip();

        internal PluginForm(Subtitle subtitle, string name, string description)
        {
            InitializeComponent();

            Text = name;
            labelDescription.Text = description;
            _subtitle = subtitle;

            _localFile = Path.Combine(Utilities.GetWordListFileName(), "AmericanToBritish.xml");
            if (File.Exists(_localFile))
            {
                _converter = new AmericanToBritishConverter(_localFile);
            }
            else
            {
                _converter = new AmericanToBritishConverter();
            }

            SizeChanged += delegate
            {
                var width = (Width - (130 + listViewFixes.Left * 2)) / 2;
                columnHeader7.Width = width;
                columnHeader8.Width = width;
            };

            // deserialize checkbox status
            try
            {
                var xmldoc = new System.Xml.XmlDocument();
                xmldoc.Load(_localFile);
                var xnode = xmldoc.SelectSingleNode("Words");
                var state = Convert.ToBoolean(xnode.Attributes["NoBuiltIn"].InnerText);
                checkBox1.Checked = state;
            }
            catch (Exception)
            {
            }

            //Convert.ToBoolean()
            FormClosed += delegate
            {
                try
                {
                    var xmldoc = new System.Xml.XmlDocument();
                    xmldoc.Load(_localFile);
                    var xnode = xmldoc.SelectSingleNode("Words");
                    var attrib = xmldoc.CreateAttribute("NoBuiltIn");
                    attrib.InnerText = checkBox1.Checked.ToString();
                    xnode.Attributes.Append(attrib);
                    xmldoc.Save(_localFile);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            };
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            _allowFixes = true;
            GeneratePreview();
            FixedSubtitle = _subtitle.ToText(_subrip);
            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void AddFixToListView(Paragraph p, string before, string after)
        {
            var item = new ListViewItem(string.Empty) { Checked = true };
            item.SubItems.Add(p.Number.ToString(CultureInfo.InvariantCulture));
            item.SubItems.Add(before.Replace(Environment.NewLine, Configuration.ListViewLineSeparatorString));
            item.SubItems.Add(after.Replace(Environment.NewLine, Configuration.ListViewLineSeparatorString));
            listViewFixes.Items.Add(item);
        }

        private void GeneratePreview()
        {
            _totalFixes = 0;
            ListType listType = radioButtonBuiltInList.Checked ? ListType.BuiltIn : ListType.Local;
            listViewFixes.BeginUpdate();
            listViewFixes.Items.Clear();
            for (int i = 0; i < _subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = _subtitle.Paragraphs[i];
                string text = p.Text.Trim();
                string oldText = text;
                text = _converter.FixText(text, listType);

                if (text != oldText)
                {
                    if (AllowFix(p))
                    {
                        p.Text = text;
                    }
                    else
                    {
                        if (_allowFixes)
                            continue;
                        _totalFixes++;
                        // remove html tags before adding to listview
                        // text = Utilities.RemoveHtmlTags(text);
                        // oldText = Utilities.RemoveHtmlTags(oldText);
                        AddFixToListView(p, oldText, text);
                    }
                }
            }
            listViewFixes.EndUpdate();
            if (!_allowFixes)
            {
                labelTotal.Text = "Total: " + _totalFixes;
                labelTotal.ForeColor = _totalFixes > 0 ? Color.Blue : Color.Red;
            }
        }

        private bool AllowFix(Paragraph p)
        {
            if (!_allowFixes)
                return false;

            string ln = p.Number.ToString(CultureInfo.InvariantCulture);
            foreach (ListViewItem item in listViewFixes.Items)
            {
                if (item.SubItems[1].Text == ln)
                    return item.Checked;
            }
            return false;
        }

        private bool SubtitleLoaded()
        {
            if (_subtitle == null || _subtitle.Paragraphs.Count < 1)
                return false;
            return true;
        }

        private void DoSelection(bool selectAll)
        {
            listViewFixes.BeginUpdate();
            foreach (ListViewItem item in listViewFixes.Items)
                item.Checked = selectAll || !item.Checked;
            listViewFixes.EndUpdate();
            Refresh();
        }

        private void PluginForm_Load(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                GeneratePreview();
                if (listViewFixes.Items.Count > 0)
                {
                    listViewFixes.Items[0].Selected = true;
                    listViewFixes.Items[0].Focused = true;
                }
                listViewFixes.Select();
                listViewFixes.Focus();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://github.com/SubtitleEdit/plugins/issues/new");
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://github.com/SubtitleEdit/plugins/blob/master/AmericanToBritish/DLL/WordList.xml");
        }

        private void PluginForm_Resize(object sender, EventArgs e)
        {
            listViewFixes.Columns[listViewFixes.Columns.Count - 1].Width = -1;
        }

        private void PluginForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.Cancel;
        }

        private void manageLocalwordsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!File.Exists(_localFile))
                return;
            using (var manageWords = new ManageWordsForm())
            using (var fs = File.Open(_localFile, FileMode.Open, FileAccess.ReadWrite, FileShare.Read))
            {
                manageWords.Initialize(fs, "Local list");
                manageWords.ShowDialog(this);
            }
            if (radioButtonLocalList.Checked)
                GeneratePreview();
        }

        private void viewBuiltinWordsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var manageWords = new ManageWordsForm())
            using (var resouceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Nikse.SubtitleEdit.PluginLogic.WordList.xml"))
            {
                manageWords.Initialize(resouceStream, "Embadded list");
                manageWords.ShowDialog(this);
            }
        }

        private void ExtractWordsFromBuiltInLIst()
        {
            using (var resouce = Assembly.GetExecutingAssembly().GetManifestResourceStream("Nikse.SubtitleEdit.PluginLogic.WordList.xml"))
            {
                var xdoc = XDocument.Load(resouce);
                xdoc.Save(_localFile);
            }
        }

        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!SubtitleLoaded())
                return;
            DoSelection(true);
        }

        private void invertToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!SubtitleLoaded())
                return;
            DoSelection(false);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            radioButtonLocalList.Enabled = !checkBox1.Checked;
            radioButtonBuiltInList.Enabled = !checkBox1.Checked;
            if (checkBox1.Checked)
            {
                radioButtonLocalList.Checked = true;
                radioButtonLocalList_Click(this, EventArgs.Empty);
            }
        }

        private void radioButtonLocalList_Click(object sender, EventArgs e)
        {
            var generate = false;
            if (File.Exists(_localFile))
            {
                generate = true;
            }
            else
            {
                // prompt to create words list if it doesn't exist
                if (MessageBox.Show("Local list wasn't found, do you want to create one?", "Word list not found!", MessageBoxButtons.OKCancel) == DialogResult.OK)
                {
                    try
                    {
                        ExtractWordsFromBuiltInLIst();
                        generate = true;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        generate = false;
                    }
                }
            }
            if (generate)
            {
                _converter.LoadLocalWords(_localFile);
                GeneratePreview();
            }
        }

        private void radioButtonBuiltInList_Click(object sender, EventArgs e)
        {
            // load built-in list name
            // update list view
            GeneratePreview();
        }
    }
}