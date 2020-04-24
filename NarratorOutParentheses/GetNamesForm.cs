using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Nikse.SubtitleEdit.PluginLogic
{
    internal partial class GetNamesForm : Form
    {
        private List<string> _list;
        public GetNamesForm(Form mainForm, Subtitle sub)
        {
            InitializeComponent();
            FormClosed += delegate { mainForm.Show(); };
            GeneratePreview(sub);
        }

        private void GeneratePreview(Subtitle sub)
        {
            _list = new List<string>();

            listBox1.Items.Clear();
            int count = sub.Paragraphs.Count;
            for (int i = 0; i < count; i++)
            {
                var p = sub.Paragraphs[i];
                var text = p.Text;
                // normalize parentheses
                text = text.Replace('[', '(').Replace(']', ')');
                var idx = text.IndexOf('(');
                if (idx < 0)
                {
                    continue;
                }

                var noTagLines = HtmlUtils.RemoveTags(text).SplitToLines();

                // single line (Sighs)
                if (noTagLines.Length == 1)
                {
                    if (IsStartEndBraces(noTagLines[0], idx))
                        continue;

                    FindNamesInText(text, idx);
                }
                else
                {
                    //- (SKIPPER) Flaps.
                    //- KOWALSKI:  Check.

                    //(LIVELY AFRICAN
                    //TRIBAL THEME PLAYS)

                    //(CLICKING) (SNARLING)

                    //- (CLICKING) UASOIDUFA;
                    //- Oh! (LAUGHS) (SQUEALS)

                    //- (SKIPPER) initiate warp drive.
                    //- (WHIRRING)

                    for (int k = 0; k < noTagLines.Length; k++)
                    {
                        var noTagLine = noTagLines[k];
                        idx = noTagLine.IndexOf('(');
                        if (idx < 0)
                            continue;
                        if (IsStartEndBraces(noTagLine, idx))
                        {
                            if (k > 0) // continue is second line starts and ends with '(' & ')'
                                continue;
                            if (k == 0 && noTagLine.StartsWith("-", StringComparison.Ordinal)) // Continue if it's dialog
                                continue;
                        }
                        FindNamesInText(noTagLine, idx);
                    }
                }
            }
            if (listBox1.Items.Count > 0)
            {
                label1.Text = "Total found names: " + listBox1.Items.Count; ;
            }
        }

        char[] _trimChars = { '♯', '♯', '♯', '♯', '♯' , ' ', '.', '!', '?', '-', '"' };

        private bool IsStartEndBraces(string noTagLine, int startIdx)
        {
            noTagLine = noTagLine.TrimEnd(' ', '.', '!', '?', '-', '"');
            var endIdx = noTagLine.IndexOf(')', startIdx + 1);
            if (endIdx + 1 == noTagLine.Length)
                return true;
            return false;
        }

        private void FindNamesInText(string text, int idx)
        {
            while (idx >= 0)
            {
                //(ismael)
                var endIdx = text.IndexOf(')', idx + 1);
                if (endIdx < idx)
                    break; // break while
                var name = text.Substring(idx, endIdx - idx + 1); // (Jonh)
                TryAddToListBox(text, name);
                idx = text.IndexOf('(', endIdx + 1);
            }
        }

        private void TryAddToListBox(string text, string name)
        {
            if (name.Length > 2)
            {
                // For listbox
                name = System.Text.RegularExpressions.Regex.Replace(name, "\\b(\\w)", (x) => x.Value.ToUpperInvariant());
            }

            // todo: check if name is in ignore list if: true return
            var noTagName = name.Trim('(', ')');
            if (!listBox1.Items.Contains(name) && !NameList.ListNames.Contains(noTagName.ToUpperInvariant()))
            {
                listBox1.Items.Add(name);
                _list.Add(text);
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex >= 0)
            {
                var idx = listBox1.SelectedIndex;
                textBox1.Text = _list[idx];
                label2.Text = "Index: " + idx;
            }
            // Todo: use regex for names like: (WELLS OVER RADIO)
        }

        private void buttonAddToList_Click(object sender, EventArgs e)
        {
            if (listBox1.Items.Count == 0)
            {
                MessageBox.Show("There is no item to add");
                return;
            }

            var idx = listBox1.SelectedIndex;
            if (idx < 0)
            {
                MessageBox.Show("Nothing selected, please select something from List box and try again!");
                return;
            }
            else
            {
                var name = listBox1.Items[idx].ToString();
                name = name.Trim('(', ')');
                NameList.AddNameToList(name);
                listBox1.Items.RemoveAt(idx);
                _list.RemoveAt(idx);
                if (idx < listBox1.Items.Count)
                {
                    listBox1.SelectedIndex = idx;
                    textBox1.Text = listBox1.Items[idx].ToString();
                }
                else
                    textBox1.Text = "";
            }
        }

        private void buttonAddToIgnore_Click(object sender, EventArgs e)
        {
            if (listBox1.Items.Count <= 0)
                return;

            // if file doesn't exist create a new one, check iisn't already in list
            var settingFile = Path.Combine(FileUtils.Dictionaries, "moodsIgnore.xml");
            XDocument xdoc = null;
            if (File.Exists(settingFile))
            {
                xdoc = XDocument.Load(settingFile);
            }
            else
            {
                var listIgnore = new List<string>();
                foreach (var item in listBox1.Items)
                {
                    if (!NameList.ListNames.Contains(item.ToString().ToUpperInvariant()))
                    {
                        listIgnore.Add(item.ToString());
                    }
                }

                if (listIgnore.Count > 0)
                {
                    xdoc = new XDocument(new XElement("ignoreList"));

                    foreach (var item in listIgnore)
                    {
                        xdoc.Root.Add(new XElement("word", item));
                    }

                    try
                    {
                        xdoc.Save(settingFile, SaveOptions.None);
                    }
                    catch
                    {
                    }
                }
            }
        }

        private void StoreItemsIntoXmlFile(string path)
        {

        }

        private void GetNames_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Escape)
                DialogResult = DialogResult.Cancel;
        }
    }
}
