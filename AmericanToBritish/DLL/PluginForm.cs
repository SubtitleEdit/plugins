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
        private readonly Dictionary<string, string> _fixedText = new Dictionary<string, string>();

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
            var item = new ListViewItem(string.Empty) { Checked = true, Tag = p };
            item.SubItems.Add(p.Number.ToString(CultureInfo.InvariantCulture));
            item.SubItems.Add(before.Replace(Environment.NewLine, Configuration.ListViewLineSeparatorString));
            item.SubItems.Add(after.Replace(Environment.NewLine, Configuration.ListViewLineSeparatorString));
            listViewFixes.Items.Add(item);
        }

        private void AmericanToBritish()
        {
            if (!_allowFixes)
            {
                listViewFixes.BeginUpdate();
                for (int i = 0; i < _subtitle.Paragraphs.Count; i++)
                {
                    Paragraph p = _subtitle.Paragraphs[i];
                    var text = p.Text.Trim();
                    var oldText = text;
                    text = FixText(text);

                    var idx = text.IndexOf("<font", StringComparison.OrdinalIgnoreCase);
                    while (idx >= 0) // Fix colour => color
                    {
                        var endIdx = text.IndexOf('>', idx + 5);
                        if (endIdx < 5)
                            break;
                        var tag = text.Substring(idx, endIdx - idx);
                        tag = tag.Replace("colour", "color");
                        tag = tag.Replace("COLOUR", "COLOR");
                        tag = tag.Replace("Colour", "Color");
                        text = text.Remove(idx, endIdx - idx).Insert(idx, tag);
                        idx = text.IndexOf("<font", endIdx + 1, StringComparison.OrdinalIgnoreCase);
                    }

                    if (text != oldText)
                    {
                        _totalFixes++;
                        // remove html tags before adding to listview
                        //text = Utilities.RemoveHtmlTags(text);
                        //oldText = Utilities.RemoveHtmlTags(oldText);
                        _fixedText.Add(p.Id, text);
                        AddFixToListView(p, oldText, text);

                    }
                }
                listViewFixes.EndUpdate();
                if (!_allowFixes)
                {
                    labelTotal.Text = "Total: " + _totalFixes;
                    labelTotal.ForeColor = _totalFixes > 0 ? Color.Blue : Color.Red;
                }

                listViewFixes.EndUpdate();
            }
            else
            {
                foreach (ListViewItem item in listViewFixes.Items)
                {
                    if (!item.Checked)
                        continue;
                    var p = item.Tag as Paragraph;
                    if (p != null && _fixedText.ContainsKey(p.Id))
                        p.Text = _fixedText[p.Id];
                }
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

        private readonly System.ComponentModel.BackgroundWorker Worker = new System.ComponentModel.BackgroundWorker();
        private void PluginForm_Load(object sender, EventArgs e)
        {
            //Cursor = Cursors.WaitCursor;
            // start the progress bar
            progressBar1.Style = ProgressBarStyle.Marquee;
            progressBar1.MarqueeAnimationSpeed = 100;

            try
            {
                using (var stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Nikse.SubtitleEdit.PluginLogic.UsGb.xml"))
                {
                    if (stream != null)
                    {
                        using (var reader = new StreamReader(stream))
                        {
                            var s = string.Empty; // raw xml text
                            System.Threading.Tasks.Task.Factory.StartNew(() => reader.ReadToEnd());
                            var xdoc = XDocument.Parse(reader.ReadToEnd());
                            foreach (XElement xElement in xdoc.Root.Elements("Word"))
                            {
                                string american = xElement.Attribute("us").Value;
                                string british = xElement.Attribute("br").Value;
                                if (!string.IsNullOrWhiteSpace(american) || !string.IsNullOrWhiteSpace(british) && american != british)
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

            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
            /*finally
            {
                Cursor = Cursors.Default;
            }*/

            // Worker
            Worker.DoWork += delegate
            {
                AmericanToBritish();
            };
            Worker.RunWorkerCompleted += delegate
            {
                // stop progress bar
                progressBar1.Style = ProgressBarStyle.Continuous;
                progressBar1.MarqueeAnimationSpeed = 0;

                if (listViewFixes.Items.Count > 0)
                {
                    listViewFixes.Items[0].Selected = true;
                    listViewFixes.Items[0].Focused = true;
                }
                listViewFixes.Select();
                listViewFixes.Focus();
            };
            Worker.RunWorkerAsync();
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