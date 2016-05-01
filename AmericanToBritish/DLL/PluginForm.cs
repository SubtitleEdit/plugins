using Nikse.SubtitleEdit.PluginLogic.Logic;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Nikse.SubtitleEdit.PluginLogic
{
    internal sealed partial class PluginForm : Form
    {
        internal string FixedSubtitle { get; private set; }

        private readonly AmericanToBritishConverter _converter;
        private readonly string _localFileName;
        private readonly Subtitle _subtitle;
        private bool _localFileExtendsBuiltIn;

        internal PluginForm(Subtitle subtitle, string name, string description)
        {
            InitializeComponent();

            Text = name;
            labelDescription.Text = description;

            _subtitle = subtitle;
            _converter = new AmericanToBritishConverter();
            _localFileName = Utilities.GetWordListFileName();

            SizeChanged += delegate
            {
                var width = (Width - (130 + listViewFixes.Left * 2)) / 2;
                columnHeader7.Width = width;
                columnHeader8.Width = width;
            };

            FormClosed += delegate
            {
                var extends = radioButtonBothLists.Checked || (radioButtonBuiltInList.Checked && _localFileExtendsBuiltIn);
                if (extends != _localFileExtendsBuiltIn)
                {
                    try
                    {
                        var xmldoc = new System.Xml.XmlDocument { XmlResolver = null };
                        xmldoc.Load(_localFileName);
                        xmldoc.DocumentElement.SetAttribute("ExtendsBuiltInWordList", extends.ToString(CultureInfo.InvariantCulture));
                        xmldoc.Save(_localFileName);
                    }
                    catch
                    {
                    }
                }
            };
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            try
            {
                foreach (ListViewItem item in listViewFixes.Items)
                {
                    if (item.Checked)
                    {
                        var paragraph = item.Tag as Paragraph;
                        paragraph.Text = _converter.Convert(paragraph.Text);
                    }
                }
                FixedSubtitle = _subtitle.ToText();
                DialogResult = DialogResult.OK;
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void AddFixToListView(Paragraph p, string before, string after)
        {
            var item = new ListViewItem(string.Empty) { Checked = true, Tag = p };
            item.SubItems.Add(p.Number.ToString(CultureInfo.InvariantCulture));
            item.SubItems.Add(before.Replace(Environment.NewLine, Configuration.ListViewLineSeparatorString));
            item.SubItems.Add(after.Replace(Environment.NewLine, Configuration.ListViewLineSeparatorString));
            listViewFixes.Items.Add(item);
        }

        private void GeneratePreview()
        {
            listViewFixes.BeginUpdate();
            listViewFixes.Items.Clear();
            foreach (var paragraph in _subtitle.Paragraphs)
            {
                var oldText = paragraph.Text.Trim();
                var newText = _converter.Convert(oldText);
                if (newText != oldText)
                {
                    AddFixToListView(paragraph, oldText, newText);
                }
            }

            var totalFixes = listViewFixes.Items.Count;
            if (totalFixes > 0)
            {
                listViewFixes.Items[0].Selected = true;
                listViewFixes.Items[0].Focused = true;
            }
            listViewFixes.Select();
            listViewFixes.EndUpdate();
            labelTotal.Text = "Total: " + totalFixes;
            labelTotal.ForeColor = totalFixes > 0 ? Color.Blue : Color.Red;
        }

        private bool SubtitleLoaded()
        {
            return _subtitle != null && _subtitle.Paragraphs.Count > 0;
        }

        private void DoSelection(bool selectAll)
        {
            listViewFixes.BeginUpdate();
            foreach (ListViewItem item in listViewFixes.Items)
                item.Checked = selectAll || !item.Checked;
            listViewFixes.EndUpdate();
            Refresh();
        }

        private void PluginForm_Shown(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            Enabled = false;
            Refresh();
            try
            {
                if (_converter.LoadLocalWords(_localFileName))
                {
                    if (_converter.ExtendsBuiltInWordList)
                    {
                        radioButtonBothLists.Checked = true;
                        _localFileExtendsBuiltIn = true;
                        _converter.AddBuiltInWords();
                    }
                    else
                    {
                        radioButtonLocalList.Checked = true;
                    }
                }
                else
                {
                    _converter.LoadBuiltInWords();
                }
                GeneratePreview();
                Enabled = true;
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

        private void linkLabelIssues_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://github.com/SubtitleEdit/plugins/issues/new");
            listViewFixes.Select();
        }

        private void linkLabelWordList_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://github.com/SubtitleEdit/plugins/blob/master/AmericanToBritish/DLL/WordList.xml");
            listViewFixes.Select();
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

        private void toolStripMenuItemManageLocalwords_Click(object sender, EventArgs e)
        {
            if (!File.Exists(_localFileName))
                return;
            using (var manageWords = new ManageWordsForm())
            {
                manageWords.Initialize(_localFileName);
                manageWords.ShowDialog(this);
            }

            if (radioButtonLocalList.Checked)
                radioButtonLocalList_Click(null, null);
            if (radioButtonBothLists.Checked)
                radioButtonBothLists_Click(null, null);
        }

        private void toolStripMenuItemViewBuiltInWords_Click(object sender, EventArgs e)
        {
            using (var manageWords = new ManageWordsForm())
            using (var resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Nikse.SubtitleEdit.PluginLogic.WordList.xml"))
            {
                manageWords.Initialize(resourceStream);
                manageWords.ShowDialog(this);
            }
        }

        private void toolStripMenuItemSelectAll_Click(object sender, EventArgs e)
        {
            if (!SubtitleLoaded())
                return;
            DoSelection(true);
        }

        private void toolStripMenuItemInvert_Click(object sender, EventArgs e)
        {
            if (!SubtitleLoaded())
                return;
            DoSelection(false);
        }

        private void radioButtonBuiltInList_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            try
            {
                _converter.LoadBuiltInWords();
                GeneratePreview();
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void radioButtonLocalList_Click(object sender, EventArgs e)
        {
            if (File.Exists(_localFileName) || CreateLocalWordList(emptyList: false))
            {
                Cursor = Cursors.WaitCursor;
                try
                {
                    if (_converter.LoadLocalWords(_localFileName))
                    {
                        GeneratePreview();
                        return;
                    }
                }
                finally
                {
                    Cursor = Cursors.Default;
                }
            }
            radioButtonBuiltInList.Checked = true;
        }

        private void radioButtonBothLists_Click(object sender, EventArgs e)
        {
            if (File.Exists(_localFileName) || CreateLocalWordList(emptyList: true))
            {
                Cursor = Cursors.WaitCursor;
                try
                {
                    if (_converter.LoadLocalWords(_localFileName))
                    {
                        _converter.AddBuiltInWords();
                        GeneratePreview();
                        return;
                    }
                }
                finally
                {
                    Cursor = Cursors.Default;
                }
            }
            radioButtonBuiltInList.Checked = true;
        }

        private bool CreateLocalWordList(bool emptyList)
        {
            if (MessageBox.Show("Local word list does not exist, do you want to create one?", "Local word list not found!", MessageBoxButtons.YesNo) == DialogResult.OK)
            {
                try
                {
                    using (var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream("Nikse.SubtitleEdit.PluginLogic.WordList.xml"))
                    {
                        var xdoc = XDocument.Load(resource);
                        if (emptyList)
                        {
                            xdoc.Root.RemoveAll();
                            xdoc.Root.SetAttributeValue("ExtendsBuiltInWordList", true);
                            _localFileExtendsBuiltIn = true;
                        }
                        xdoc.Save(_localFileName);
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            return false;
        }

    }
}
