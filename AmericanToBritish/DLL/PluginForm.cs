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

namespace Nikse.SubtitleEdit.PluginLogic
{
    internal sealed partial class PluginForm : Form
    {
        internal string FixedSubtitle { get; private set; }
        private readonly Subtitle _subtitle;
        private int _totalFixes;
        private bool _allowFixes;
        private readonly List<Regex> _regexList = new List<Regex>();
        private readonly List<string> _replaceList = new List<string>();

        internal PluginForm(Subtitle subtitle, string name, string description)
        {
            InitializeComponent();

            Text = name;
            labelDescription.Text = description;
            _subtitle = subtitle;
            SizeChanged += delegate
            {
                var width = (Width - (130 + listViewFixes.Left * 2)) / 2;
                columnHeader7.Width = width;
                columnHeader8.Width = width;
            };
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            _allowFixes = true;
            AmericanToBritish();
            FixedSubtitle = _subtitle.ToText(new SubRip());
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
            item.SubItems.Add(before.Replace(Environment.NewLine, "<br />"));
            item.SubItems.Add(after.Replace(Environment.NewLine, "<br />"));
            listViewFixes.Items.Add(item);
        }

        private void AmericanToBritish()
        {
            for (int i = 0; i < _subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = _subtitle.Paragraphs[i];
                string text = p.Text.Trim();
                string oldText = text;
                text = FixText(text);

                var idx = text.IndexOf("<font", StringComparison.OrdinalIgnoreCase);
                while (idx >= 0) // Fix colour => color
                {
                    var endIdx = text.IndexOf('>', idx + 5);
                    if (endIdx < 5)
                        break;
                    var tag = text.Substring(idx, endIdx - idx).Replace("colour", "color");
                    text = text = text.Remove(idx, endIdx - idx).Insert(idx, tag);
                    idx = text.IndexOf("<font", endIdx + 1, StringComparison.OrdinalIgnoreCase);
                }

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
                        //text = Utilities.RemoveHtmlTags(text);
                        //oldText = Utilities.RemoveHtmlTags(oldText);
                        AddFixToListView(p, oldText, text);
                    }
                }
            }
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

        private string FixText(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return s;

            for (int index = 0; index < _regexList.Count; index++)
            {
                var regex = _regexList[index];
                if (regex.IsMatch(s))
                {
                    s = regex.Replace(s, _replaceList[index]);
                }
            }
            return s;
        }

        private void buttonSelectAll_Click(object sender, EventArgs e)
        {
            if (!SubtitleLoaded())
                return;
            DoSelection(true);
        }

        private void buttonInverseSelection_Click(object sender, EventArgs e)
        {
            if (!SubtitleLoaded())
                return;
            DoSelection(false);
        }

        private bool SubtitleLoaded()
        {
            if (_subtitle == null)
                return false;
            if (_subtitle.Paragraphs.Count < 1)
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
                using (var stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Nikse.SubtitleEdit.PluginLogic.WordList.xml"))
                {
                    if (stream != null)
                    {
                        using (var reader = new StreamReader(stream))
                        {
                            var xdoc = XDocument.Parse(reader.ReadToEnd());
                            foreach (XElement xElement in xdoc.Descendants("Word"))
                            {
                                string american = xElement.Attribute("us").Value;
                                string british = xElement.Attribute("br").Value;
                                if (!string.IsNullOrWhiteSpace(american) || !string.IsNullOrWhiteSpace(british))
                                {
                                    _regexList.Add(new Regex("\\b" + american + "\\b", RegexOptions.Compiled));
                                    _replaceList.Add(british);

                                    _regexList.Add(new Regex("\\b" + american.ToUpperInvariant() + "\\b", RegexOptions.Compiled));
                                    _replaceList.Add(british.ToUpperInvariant());

                                    if (american.Length > 1)
                                    {
                                        _regexList.Add(new Regex("\\b" + char.ToUpperInvariant(american[0]) + american.Substring(1) + "\\b", RegexOptions.Compiled));
                                        if (british.Length > 1)
                                            _replaceList.Add(char.ToUpperInvariant(british[0]) + british.Substring(1));
                                        else
                                            _replaceList.Add(british.ToUpper());
                                    }
                                }
                            }

                        }
                    }
                }
                AmericanToBritish();
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
    }
}