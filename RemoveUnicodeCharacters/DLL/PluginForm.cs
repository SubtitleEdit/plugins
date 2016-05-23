using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.PluginLogic
{
    internal sealed partial class PluginForm : Form
    {
        public string FixedSubtitle { get; private set; }
        private readonly Subtitle _subtitle;
        private int _totalFixes;

        private SortedDictionary<string, string> _replaceList = new SortedDictionary<string, string>()
        {
            {"♪", "#"},
            {"♫", "#"}
        };

        public PluginForm(Subtitle subtitle, string name, string description)
        {
            InitializeComponent();
            buttonGoogleIt.Visible = false;
            Text = name;
            labelDescription.Text = description;
            _subtitle = subtitle;
            Resize += delegate
            {
                int idx = listViewFixes.Columns.Count - 1;
                listViewFixes.Columns[idx].Width = -2;
            };
            LoadSettingsIfThereIs();
            FindFixes();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            int count = 0;
            foreach (ListViewItem item in listViewFixes.Items)
            {
                if (item.Checked)
                {
                    var kvp = (KeyValuePair<char, List<Paragraph>>) item.Tag;
                    foreach (var paragraph in kvp.Value)
                    {
                        string replaceWith = string.Empty;
                        if (checkBoxUseReplaceList.Checked && _replaceList.ContainsKey(kvp.Key.ToString()))
                        {
                            replaceWith = _replaceList[kvp.Key.ToString()];
                        }
                        paragraph.Text = paragraph.Text.Replace(kvp.Key.ToString(), replaceWith);
                        count++;
                    }
                }
            }

            if (count > 0)
            {
                FixedSubtitle = _subtitle.ToText();
                DialogResult = DialogResult.OK;
            }
            else
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void AddFixToListView(KeyValuePair<char, List<Paragraph>> kvp, string action, string before,
            string after)
        {
            var item = new ListViewItem(string.Empty) {Checked = true, Tag = kvp};
            item.SubItems.Add(kvp.Value.Count.ToString());
            item.SubItems.Add(action);
            item.SubItems.Add(before);
            item.SubItems.Add(after);
            var sb = new StringBuilder();
            foreach (var paragraph in kvp.Value)
            {
                sb.Append(paragraph.Number + ", ");
            }
            item.SubItems.Add(sb.ToString().Trim().TrimEnd(','));
            listViewFixes.Items.Add(item);
        }

        private void FindFixes()
        {
            listViewFixes.BeginUpdate();
            listViewFixes.Items.Clear();
            _totalFixes = 0;
            var unicodeCharactersUsed = FindUnicodeCharactersUsed(_subtitle);
            var dic = new Dictionary<char, List<Paragraph>>();
            for (int i = 0; i < _subtitle.Paragraphs.Count; i++)
            {
                var p = _subtitle.Paragraphs[i];
                foreach (var uc in unicodeCharactersUsed)
                {
                    if (p.Text.Contains(uc))
                    {
                        _totalFixes += Utilities.CountTagInText(p.Text, uc);
                        if (!dic.ContainsKey(uc))
                        {
                            dic.Add(uc, new List<Paragraph>());
                        }
                        if (!dic[uc].Contains(p))
                        {
                            dic[uc].Add(p);
                        }
                    }
                }
            }
            foreach (var kvp in dic)
            {
                string replaceWith = string.Empty;
                if (checkBoxUseReplaceList.Checked && _replaceList.ContainsKey(kvp.Key.ToString()))
                {
                    replaceWith = _replaceList[kvp.Key.ToString()];
                }
                AddFixToListView(kvp, "\\u" + ((int) kvp.Key).ToString("X4") + "  " + kvp.Key, kvp.Key.ToString(),
                    replaceWith);
            }
            listViewFixes.EndUpdate();
            if (_totalFixes == 0)
            {
                labelTotal.ForeColor = Color.Green;
                labelTotal.Text = "No Unicode characters found";
            }
            else
            {
                labelTotal.ForeColor = Color.Red;
                labelTotal.Text = "Total Unicode characters found: " + _totalFixes;
            }
        }

        private List<char> FindUnicodeCharactersUsed(Subtitle subtitle)
        {
            var list = new List<char>();
            foreach (var p in subtitle.Paragraphs)
            {
                foreach (var c in p.Text)
                {
                    if (c > 255 && !list.Contains(c))
                    {
                        list.Add(c);
                    }
                }
            }
            list.Sort();
            return list;
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

        private void checkBoxUseReplaceList_CheckedChanged(object sender, EventArgs e)
        {
            FindFixes();
        }

        private void EditReplaceList_Click(object sender, EventArgs e)
        {
            var temp = _replaceList.OrderBy(p => p.Key);
            _replaceList = new SortedDictionary<string, string>();
            foreach (var kvp in temp)
            {
                _replaceList.Add(kvp.Key, kvp.Value);
            }

            using (var form = new EditReplaceList(_replaceList))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    _replaceList = form.ReplaceList;
                    FindFixes();
                }
            }
        }

        private string GetSettingsFileName()
        {
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);
            if (path.StartsWith("file:\\"))
                path = path.Remove(0, 6);
            path = Path.Combine(path, "Plugins");
            if (!Directory.Exists(path))
                path =
                    Path.Combine(
                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                            "Subtitle Edit"), "Plugins");
            return Path.Combine(path, "RemoveUnicodeCharacters.xml");
        }

        private void LoadSettingsIfThereIs()
        {
            var fileName = GetSettingsFileName();
            if (File.Exists(fileName))
            {
                XDocument doc = XDocument.Parse(File.ReadAllText(fileName));
                var useReplaceListNode = doc.Root.Element("UseReplaceList");
                if (useReplaceListNode != null)
                    checkBoxUseReplaceList.Checked = Convert.ToBoolean(useReplaceListNode.Value);

                foreach (XElement node in doc.Root.Element("ReplaceList").Elements())
                {
                    string unicodeCharacter = node.Element("Unicode").Value;
                    string ansiCharacter = node.Element("Ansi").Value;
                    if (!string.IsNullOrEmpty(unicodeCharacter) && unicodeCharacter.Length == 1 &&
                        !_replaceList.ContainsKey(unicodeCharacter))
                    {
                        _replaceList.Add(unicodeCharacter, ansiCharacter);
                    }
                }
            }
        }

        private void PluginForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                var fileName = GetSettingsFileName();
                var document = new XDocument(
                    new XDeclaration("1.0", "utf8", "yes"),
                    new XComment(
                        "This XML file defines the settings for the Subtitle Edit 'Remove Unicode characters' plugin"),
                    new XElement("RemoveUnicodeCharactersSettings",
                        new XElement("UseReplaceList", checkBoxUseReplaceList.Checked.ToString()),
                        new XElement("ReplaceList",
                            _replaceList.Where(p => p.Key.Length > 0).Select(kvp => new XElement("Item",
                                new XElement("Unicode", kvp.Key),
                                new XElement("Ansi", kvp.Value))
                                )
                            )
                        )
                    );
                document.Save(fileName);
            }
            catch
            {
                // ignore save errors
            }
        }

        private void listViewFixes_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewFixes.SelectedItems.Count != 1)
            {
                buttonGoogleIt.Visible = false;
                return;
            }

            buttonGoogleIt.Visible = true;
            var kvp = (KeyValuePair<char, List<Paragraph>>)listViewFixes.SelectedItems[0].Tag;
            string hex = ((int)kvp.Key).ToString("X4");
            buttonGoogleIt.Tag = hex;
            buttonGoogleIt.Text = string.Format("Google '{0}'", hex);
        }

        private void buttonGoogleIt_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.google.com/search?q=" + buttonGoogleIt.Tag + "+Unicode");
        }
    }
}