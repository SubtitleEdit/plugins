using Nikse.SubtitleEdit.PluginLogic.Logic;
using System;
using System.Collections.Generic;
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
        private class Conversion
        {
            public Paragraph Paragraph { get; set; }
            public string ConvertedText { get; set; }
            public string EditedText { get; set; }

            public string Before
            {
                get
                {
                    return Paragraph.Text.Trim().Replace(Environment.NewLine, Configuration.ListViewLineSeparatorString);
                }
            }

            public string After
            {
                get
                {
                    return (EditedText ?? ConvertedText).Trim().Replace(Environment.NewLine, Configuration.ListViewLineSeparatorString);
                }
            }
        }

        internal string FixedSubtitle { get; private set; }

        private readonly Dictionary<Paragraph, Conversion> _editedParagraphs;
        private readonly AmericanToBritishConverter _converter;
        private readonly string _localFileName;
        private readonly Subtitle _subtitle;
        private bool _localFileExtendsBuiltIn;
        private ListViewItem _editorItem;

        internal PluginForm(Subtitle subtitle, string name, string description)
        {
            InitializeComponent();

            Text = name;
            labelDescription.Text = description;

            _subtitle = subtitle;
            _converter = new AmericanToBritishConverter();
            _localFileName = Utilities.GetWordListFileName();
            _editedParagraphs = new Dictionary<Paragraph, Conversion>();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listViewFixes.CheckedItems)
            {
                var conversion = item.Tag as Conversion;
                conversion.Paragraph.Text = conversion.EditedText ?? conversion.ConvertedText;
            }
            FixedSubtitle = _subtitle.ToText();
            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void AddFixToListView(Conversion conversion)
        {
            var item = new ListViewItem(string.Empty) { Checked = true, Tag = conversion };
            item.SubItems.Add(conversion.Paragraph.Number.ToString(CultureInfo.InvariantCulture));
            item.SubItems.Add(conversion.Before);
            item.SubItems.Add(conversion.After);
            listViewFixes.Items.Add(item);
        }

        private void GeneratePreview()
        {
            listViewFixes.BeginUpdate();
            listViewFixes.Items.Clear();
            foreach (var paragraph in _subtitle.Paragraphs)
            {
                var oldText = paragraph.Text;
                var newText = _converter.Convert(oldText);

                Conversion conversion;
                if (_editedParagraphs.TryGetValue(paragraph, out conversion))
                {
                    conversion.ConvertedText = newText;
                }
                else if (newText != oldText)
                {
                    conversion = new Conversion { Paragraph = paragraph, ConvertedText = newText };
                }
                if (conversion != null)
                {
                    AddFixToListView(conversion);
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
            Cursor.Current = Cursors.WaitCursor;
            Enabled = false;
            // force Form display before conversion, which might take considerable time
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
                Cursor.Current = Cursors.Default;
            }
            catch (Exception exception)
            {
                Cursor.Current = Cursors.Default;
                MessageBox.Show(exception.Message);
            }
        }

        private void PluginForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            var extends = radioButtonBothLists.Checked || (radioButtonBuiltInList.Checked && _localFileExtendsBuiltIn);
            if (extends != _localFileExtendsBuiltIn)
            {
                try
                {
                    var xdoc = XDocument.Load(_localFileName);
                    xdoc.Root.SetAttributeValue("ExtendsBuiltInWordList", extends);
                    xdoc.Save(_localFileName);
                }
                catch
                {
                }
            }
        }

        private void linkLabelIssues_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://github.com/SubtitleEdit/plugins/issues/162");
            listViewFixes.Select();
        }

        private void linkLabelWordList_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://github.com/SubtitleEdit/plugins/blob/master/AmericanToBritish/DLL/WordList.xml");
            listViewFixes.Select();
        }

        private void PluginForm_SizeChanged(object sender, EventArgs e)
        {
            var width = Width - (130 + listViewFixes.Left * 2);
            columnHeader3.Width = width / 2;
            columnHeader4.Width = width - columnHeader3.Width;
        }

        private void PluginForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
                e.SuppressKeyPress = true;
            }
        }

        private void listViewFixes_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter || e.KeyData == toolStripMenuItemEdit.ShortcutKeys)
            {
                e.SuppressKeyPress = true;
                toolStripMenuItemEdit_Click(null, null);
            }
            else if (e.KeyData == toolStripMenuItemReset.ShortcutKeys)
            {
                e.SuppressKeyPress = true;
                toolStripMenuItemReset_Click(null, null);
            }
            else if (e.KeyData == (Keys.Control | Keys.A))
            {
                if (listViewFixes.Items.Count > 0)
                {
                    e.SuppressKeyPress = true;
                    listViewFixes.SelectedItems.Clear();
                    foreach (ListViewItem item in listViewFixes.Items)
                    {
                        item.Selected = true;
                    }
                }
            }
            else if (e.KeyData == Keys.Right)
            {
                if (listViewFixes.Items.Count > 0 && _editedParagraphs.Count > 0)
                {
                    int startIndex = (listViewFixes.FocusedItem != null) ? listViewFixes.FocusedItem.Index : 0;
                    int index = startIndex;
                    do
                    {
                        if (++index >= listViewFixes.Items.Count)
                            index = 0;
                        if ((listViewFixes.Items[index].Tag as Conversion).EditedText != null)
                        {
                            e.SuppressKeyPress = true;
                            listViewFixes.SelectedItems.Clear();
                            listViewFixes.Items[index].Focused = true;
                            listViewFixes.Items[index].Selected = true;
                            listViewFixes.Items[index].EnsureVisible();
                            return;
                        }
                    }
                    while (index != startIndex);
                }
            }
            else if (e.KeyData == Keys.Left)
            {
                if (listViewFixes.Items.Count > 0 && _editedParagraphs.Count > 0)
                {
                    int startIndex = (listViewFixes.FocusedItem != null) ? listViewFixes.FocusedItem.Index : 0;
                    int index = startIndex;
                    do
                    {
                        if (--index < 0)
                            index = listViewFixes.Items.Count - 1;
                        if ((listViewFixes.Items[index].Tag as Conversion).EditedText != null)
                        {
                            e.SuppressKeyPress = true;
                            listViewFixes.SelectedItems.Clear();
                            listViewFixes.Items[index].Focused = true;
                            listViewFixes.Items[index].Selected = true;
                            listViewFixes.Items[index].EnsureVisible();
                            return;
                        }
                    }
                    while (index != startIndex);
                }
            }
        }

        private void textBoxEditAfter_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == (Keys.Control | Keys.Back))
            {
                e.SuppressKeyPress = true;
            }
            else if (e.KeyData == (Keys.Control | Keys.A))
            {
                var tb = sender as TextBox;
                if (tb.TextLength > 0)
                {
                    e.SuppressKeyPress = true;
                    tb.SelectAll();
                }
            }
            else if (e.KeyData == (Keys.Control | Keys.C))
            {
                var tb = sender as TextBox;
                if (tb.TextLength > 0 && tb.SelectionLength == 0)
                {
                    e.SuppressKeyPress = true;
                    var eol = tb.SelectionStart;
                    var sol = eol;
                    while (sol > 0 && tb.Text[sol - 1] != '\n')
                        sol--;
                    while (eol < tb.TextLength && tb.Text[eol] != '\n')
                        eol++;
                    if (eol < tb.TextLength)
                        eol++;
                    tb.Select(sol, eol - sol);
                    tb.Copy();
                    tb.Select(sol, 0);
                }
            }
            else if (e.KeyData == (Keys.Control | Keys.X))
            {
                var tb = sender as TextBox;
                if (tb.TextLength > 0 && tb.SelectionLength == 0)
                {
                    e.SuppressKeyPress = true;
                    var eol = tb.SelectionStart;
                    var sol = eol;
                    while (sol > 0 && tb.Text[sol - 1] != '\n')
                        sol--;
                    while (eol < tb.TextLength && tb.Text[eol] != '\n')
                        eol++;
                    if (eol < tb.TextLength)
                        eol++;
                    tb.Select(sol, eol - sol);
                    tb.Cut();
                    if (sol >= tb.TextLength)
                    {
                        while (sol > 0 && tb.Text[sol - 1] != '\n')
                            sol--;
                    }
                    tb.Select(sol, 0);
                }
            }
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

        private void contextMenuStripFixes_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var showSeparator = false;
            var showResetItem = false;
            var showEditItem = false;

            if (listViewFixes.SelectedItems.Count > 0)
            {
                if (_editedParagraphs.Count > 0)
                {
                    foreach (ListViewItem item in listViewFixes.SelectedItems)
                    {
                        if ((item.Tag as Conversion).EditedText != null)
                        {
                            showSeparator = true;
                            showResetItem = true;
                            break;
                        }
                    }
                }
                if (listViewFixes.SelectedItems.Count == 1)
                {
                    showSeparator = true;
                    showEditItem = true;
                }
            }

            toolStripSeparatorFixes1.Visible = showSeparator;
            toolStripMenuItemReset.Visible = showResetItem;
            toolStripMenuItemEdit.Visible = showEditItem;
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

        private void toolStripMenuItemReset_Click(object sender, EventArgs e)
        {
            if (listViewFixes.SelectedItems.Count > 0 && _editedParagraphs.Count > 0)
            {
                foreach (ListViewItem item in listViewFixes.SelectedItems)
                {
                    var conversion = item.Tag as Conversion;
                    if (conversion.EditedText != null)
                    {
                        conversion.EditedText = null;
                        item.SubItems[3].Text = conversion.After;
                        _editedParagraphs.Remove(conversion.Paragraph);
                    }
                }
            }
        }

        private void toolStripMenuItemEdit_Click(object sender, EventArgs e)
        {
            if (listViewFixes.SelectedItems.Count == 1)
            {
                _editorItem = listViewFixes.SelectedItems[0];
                var conversion = _editorItem.Tag as Conversion;
                textBoxEditAfter.Text = conversion.EditedText ?? conversion.ConvertedText;
                panelEditAfter.Left = listViewFixes.Left + (listViewFixes.Width - panelEditAfter.Width) / 2;
                panelEditAfter.Top = listViewFixes.Top + (listViewFixes.Height - panelEditAfter.Height) / 2;
                panelEditAfter.Show();
                textBoxEditAfter.Select();
                textBoxEditAfter.Select(0, 0);
            }
        }

        private void panelEditAfter_Leave(object sender, EventArgs e)
        {
            if (_editorItem != null)
            {
                var conversion = _editorItem.Tag as Conversion;
                var after = textBoxEditAfter.Text;
                if (_editedParagraphs.ContainsKey(conversion.Paragraph))
                {
                    conversion.EditedText = after;
                }
                else if (after != conversion.ConvertedText)
                {
                    conversion.EditedText = after;
                    _editedParagraphs.Add(conversion.Paragraph, conversion);
                }
                _editorItem.SubItems[3].Text = conversion.After;
                _editorItem = null;
                panelEditAfter.Hide();
                listViewFixes.Select();
                panelEditAfter.Top = listViewFixes.Top + 40;
                panelEditAfter.Left = listViewFixes.Left + 20;
            }
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
            if (MessageBox.Show("Local word list does not exist, do you want to create one?", "Local word list not found!", MessageBoxButtons.YesNo) == DialogResult.Yes)
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
