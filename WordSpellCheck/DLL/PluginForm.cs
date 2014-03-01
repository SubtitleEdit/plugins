using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;

namespace Nikse.SubtitleEdit.PluginLogic
{
    internal partial class PluginForm : Form
    {
        internal string FixedSubtitle { get; private set; }
        private Subtitle _subtitle;
        Microsoft.Office.Interop.Word.Application _wordApp = new Microsoft.Office.Interop.Word.Application();
        List<Microsoft.Office.Interop.Word.Language> _spellCheckLanguages = new List<Microsoft.Office.Interop.Word.Language>();
        Microsoft.Office.Interop.Word.ProofreadingErrors _currentSpellCollection;
        int _currentSpellCollectionIndex = -1;
        Paragraph _currentParagraph;
        int _currentStartIndex = -1;
        List<string> _skipAllList = new List<string>();
        Dictionary<string, string> _useAlwaysList = new Dictionary<string, string>();
        List<string> _namesEtcList = new List<string>();
        List<string> _namesEtcMultiWordList = new List<string>();
        string _namesEtcLocalFileName;
        bool _abort = false;
        string _currentErrorText = string.Empty;
        int _currentErrorStart = 0;

        internal PluginForm(Subtitle subtitle, string name, string description)
        {
            InitializeComponent();

            this.Text = name;
            labelDescription.Text = "Microsoft Word spell && grammer checker"; // description;
            _subtitle = subtitle;
            FillSubtitleListView();
            labelActionInfo.Text = string.Empty;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            FixedSubtitle = _subtitle.ToText(new SubRip()); ;
            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            _abort = true;
            Application.DoEvents();
            DialogResult = DialogResult.Cancel;
        }

        private void AddParagraphToSubtitleListView(Paragraph p)
        {
            var item = new ListViewItem(p.Number.ToString());

            var subItem = new ListViewItem.ListViewSubItem(item, p.StartTime.ToShortString());
            item.SubItems.Add(subItem);
            subItem = new ListViewItem.ListViewSubItem(item, p.EndTime.ToShortString());
            item.SubItems.Add(subItem);
            subItem = new ListViewItem.ListViewSubItem(item, p.Duration.ToShortString());
            item.SubItems.Add(subItem);
            subItem = new ListViewItem.ListViewSubItem(item, p.Text.Replace(Environment.NewLine, "<br/>"));
            item.SubItems.Add(subItem);

            item.Tag = p; // save paragraph in Tag

            listViewSubtitle.Items.Add(item);
        }

        private void FillSubtitleListView()
        {
            for (int i = 0; i < _subtitle.Paragraphs.Count; i++)
            {
                Paragraph p = _subtitle.Paragraphs[i];
                p.Number = i + 1;
                AddParagraphToSubtitleListView(p);
            }
            if (listViewSubtitle.Items.Count > 0)
                listViewSubtitle.Items[0].Selected = true;
        }        

        private void DoStartSpellCheck()
        {
            int index = 0;
            if (listViewSubtitle.SelectedIndices.Count > 0)
                index = listViewSubtitle.SelectedIndices[0];

            while (index < listViewSubtitle.Items.Count)
            {
                Text = "Word spell check - line " + (index + 1).ToString() + " of " + listViewSubtitle.Items.Count.ToString();                
                _currentParagraph = (listViewSubtitle.Items[index].Tag as Paragraph);
                string text = _currentParagraph.Text;
                Application.DoEvents();
                if (_abort)
                    return;
                if (StartSpellCheckParagraph(text) || _abort)
                    return;
                index++;
                if (index < listViewSubtitle.Items.Count)
                {
                    listViewSubtitle.Items[index-1].Selected = false;
                    listViewSubtitle.Items[index].Selected = true;
                    listViewSubtitle.EnsureVisible(index);
                }
            }
            SpellCheckDone();
        }

        private void SpellCheckDone()
        {
            groupBoxSuggestions.Enabled = false;
            groupBoxWordNotFound.Enabled = false;
            buttonEditWholeText.Enabled = false;
            MessageBox.Show("Word spell & grammer check completed");
            labelActionInfo.Text = "Word spell & grammer check completed";
        }

        private bool StartSpellCheckParagraph(string text)
        {
            //var cdic =  _wordApp.Application.CustomDictionaries.Add(GetCustomDicFileName());
            //_wordApp.Application.CustomDictionaries.ActiveCustomDictionary = cdic;
            Microsoft.Office.Interop.Word.Range range;
            range = _wordApp.ActiveDocument.Range();
            range.Text = string.Empty;

            //insert textbox data after the content of range of active document

            var lines = text.Replace(Environment.NewLine, "\n").Split('\n');
            if (lines.Length == 2 && lines[0].Length > 1)
            {
                string last = lines[0].Substring(lines[0].Length - 1);
                if ("ABCDEFGHIJKLMNOPQRSTUVWZYXÆØÃÅÄÖÉÈÁÂÀÇÊÍÓÔÕÚŁАБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯĞİŞÜÙÁÌÑ".ToLower().Contains(last.ToLower()))
                {
                    text = text.Replace(Environment.NewLine, " ");
                }
            }
            text = Utilities.RemoveHtmlTags(text);
            text.Replace("  ", " ");
            range.InsertAfter(text);

            if (comboBoxDictionaries.Visible)
            {
                if (comboBoxDictionaries.SelectedIndex > 0)
                {
                    _wordApp.ActiveDocument.Content.LanguageID = _spellCheckLanguages[comboBoxDictionaries.SelectedIndex - 1].ID;
                }
                else
                {
                    try
                    {
                        _wordApp.ActiveDocument.Content.DetectLanguage();
                    }
                    catch
                    { 
                    }
                }
            }
            _currentSpellCollection = range.SpellingErrors;
            _currentSpellCollectionIndex = -1;
            if (_currentSpellCollection.Count == 0)
                return false;
            ShowNextSpellingError();
            return true;
        }

        private void AutoDetectLanguage()
        {
            try
            {
                int min = 2;
                int max = 4;
                int i = 0;
                var sb = new StringBuilder();
                foreach (Paragraph p in _subtitle.Paragraphs)
                {
                    i++;
                    if (i >= min)
                        sb.AppendLine(p.Text);
                    if (i > max)
                        break;
                }

                Microsoft.Office.Interop.Word.Range range;
                range = _wordApp.ActiveDocument.Range();
                range.Text = Utilities.RemoveHtmlTags(sb.ToString().Trim());
                range.LanguageDetected = false;
                range.DetectLanguage();
                if (range.LanguageDetected)
                {
                    labelLanguage.Visible = true;
                    comboBoxDictionaries.Visible = true;
                    comboBoxDictionaries.Items.Add("Auto");
                    foreach (Microsoft.Office.Interop.Word.Language language in _spellCheckLanguages)
                    {
                        comboBoxDictionaries.Items.Add(language.NameLocal);
                        if (language.ID == range.LanguageID)
                            comboBoxDictionaries.SelectedIndex = comboBoxDictionaries.Items.Count - 1;
                    }
                    if (comboBoxDictionaries.SelectedIndex < 0)
                        comboBoxDictionaries.SelectedIndex = 0;
                }
            }
            catch
            {
                labelLanguage.Visible = false;
                comboBoxDictionaries.Visible = false;
            }
        }

        private void ShowActiveWordWithColor(string word, int start)
        {
            richTextBoxParagraph.SelectAll();
            richTextBoxParagraph.SelectionColor = Color.Black;
            richTextBoxParagraph.SelectionLength = 0;

            var regEx = Utilities.MakeWordSearchRegex(word);
            Match match = regEx.Match(richTextBoxParagraph.Text, start);
            if (!match.Success)
                match = regEx.Match(richTextBoxParagraph.Text);
            if (!match.Success)
            {
                regEx = Utilities.MakeWordSearchEndRegex(word);
                match = regEx.Match(richTextBoxParagraph.Text, start);
            }

            if (match.Success)
            {
                richTextBoxParagraph.SelectionStart = match.Index;
                richTextBoxParagraph.SelectionLength = word.Length;
                while (richTextBoxParagraph.SelectedText != word && richTextBoxParagraph.SelectionStart > 0)
                {
                    richTextBoxParagraph.SelectionStart = richTextBoxParagraph.SelectionStart - 1;
                    richTextBoxParagraph.SelectionLength = word.Length;
                }
                richTextBoxParagraph.SelectionColor = Color.Red;
            }
        }

        private void ShowNextSpellingError()
        {
            listBoxSuggestions.Items.Clear();
            if (_currentSpellCollection != null && _currentSpellCollection.Count > 0)
            {
                _currentSpellCollectionIndex++;
                if (_currentSpellCollectionIndex < _currentSpellCollection.Count)
                {                    
                    var spell = _currentSpellCollection[_currentSpellCollectionIndex+1];
                    if (_useAlwaysList.ContainsKey(spell.Text))
                    {
                        FixWord(spell.Text, _useAlwaysList[spell.Text]);
                    }
                    else if (checkBoxUseNamesEtc.Checked && _namesEtcList.Contains(spell.Text))
                    {
                        labelActionInfo.Text = "Skipping name/noise word '" + spell.Text + "'...";
                    }
                    else if (!_skipAllList.Contains(spell.Text))
                    {
                        _currentErrorText = spell.Text;
                        _currentErrorStart = spell.Start;
                        textBoxWord.Text = spell.Text;
                        _currentStartIndex = spell.Start;
                        labelActionInfo.Text = string.Empty;
                        if (spell.SpellingErrors.Count > 0)
                            labelActionInfo.Text = "Found error regarding '" + spell.Text + "'...";
                        else if (spell.GrammaticalErrors.Count > 0)
                            labelActionInfo.Text = "Found gramitical error regarding '" + spell.Text + "'...";

                        foreach (Microsoft.Office.Interop.Word.SpellingSuggestion suggestion in _wordApp.GetSpellingSuggestions(spell.Text))
                        {
                            listBoxSuggestions.Items.Add(suggestion.Name);
                        }
                        if (listBoxSuggestions.Items.Count > 0)
                            listBoxSuggestions.SelectedIndex = 0;
                        textBoxWord.Focus();
                        textBoxWord.SelectAll();
                        ShowActiveWordWithColor(spell.Text, spell.Start);
                        return;
                    }
                }
            }

            int index = 0;
            if (listViewSubtitle.SelectedIndices.Count > 0)
                index = listViewSubtitle.SelectedIndices[0];                
            listViewSubtitle.Items[index].Selected = false;
            index++;
            if (index >= listViewSubtitle.Items.Count)
            {
                SpellCheckDone();
                return;
            }
            listViewSubtitle.Items[index].Selected = true;
            listViewSubtitle.EnsureVisible(index);
            DoStartSpellCheck();
        }       

        private void listViewSubtitle_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBoxWord.Text = string.Empty;
            if (listViewSubtitle.SelectedItems.Count > 0)
            {
                _currentParagraph = listViewSubtitle.SelectedItems[0].Tag as Paragraph;
                richTextBoxParagraph.Text = _currentParagraph.Text;
            }
            if (textBoxWholeText.Visible || !buttonEditWholeText.Enabled)
            {
                richTextBoxParagraph.BringToFront();
                richTextBoxParagraph.Visible = true;
                textBoxWholeText.Visible = false;
                buttonUpdateWholeText.Visible = false;
                groupBoxSuggestions.Enabled = true;
                groupBoxWordNotFound.Enabled = true;
                buttonEditWord.Visible = false;
                buttonEditWholeText.Enabled = true;
            }
        }

        private void buttonSkipOnce_Click(object sender, EventArgs e)
        {
            labelActionInfo.Text = "Skip '" + _currentSpellCollection[_currentSpellCollectionIndex + 1].Text + "' once...";
            ShowNextSpellingError();
        }

        private void buttonSkipAll_Click(object sender, EventArgs e)
        {
            var s = textBoxWord.Text.Trim();
            if (s.Length > 0)
            {
                labelActionInfo.Text = "Skip all '" + _currentSpellCollection[_currentSpellCollectionIndex + 1].Text + "'...";

                _skipAllList.Add(s);
                _skipAllList.Add(s.ToUpper());
                if (s.Length > 1)
                    _skipAllList.Add(s.Substring(0, 1).ToUpper() + s.Substring(1));
            }
            ShowNextSpellingError();
        }

        private void PluginForm_Load(object sender, EventArgs e)
        {
        }

        private void buttonChange_Click(object sender, EventArgs e)
        {        
            string newText = textBoxWord.Text;
            if (newText.Trim().Length == 0)
                return;
            labelActionInfo.Text = "Change '" + _currentSpellCollection[_currentSpellCollectionIndex + 1].Text + "' to '" + newText + "'...";
            if (_currentSpellCollection[_currentSpellCollectionIndex + 1].Text != newText)
                FixWord(_currentSpellCollection[_currentSpellCollectionIndex + 1], newText);
            ShowNextSpellingError(); 
        }

        private void UpdateWholeText()
        {
            if (listViewSubtitle.SelectedItems.Count > 0)
            {
                _currentParagraph.Text = textBoxWholeText.Text;
                listViewSubtitle.SelectedItems[0].SubItems[4].Text = _currentParagraph.Text.Replace(Environment.NewLine, "<br/>");
            }
        }

        private void buttonChangeAll_Click(object sender, EventArgs e)
        {
            string newText = textBoxWord.Text;
            if (newText.Trim().Length == 0 || newText == _currentSpellCollection[_currentSpellCollectionIndex + 1].Text)
                return;
            labelActionInfo.Text = "Change all '" + _currentSpellCollection[_currentSpellCollectionIndex + 1].Text + "' to '" + newText + "'...";
            _useAlwaysList.Add(_currentSpellCollection[_currentSpellCollectionIndex + 1].Text, newText);
            if (_currentSpellCollection[_currentSpellCollectionIndex + 1].Text != newText)
                FixWord(_currentSpellCollection[_currentSpellCollectionIndex + 1], newText);
            ShowNextSpellingError();
        }

        private void PluginForm_Shown(object sender, EventArgs e)
        {
            listViewSubtitle.Columns[4].Width = -2;
            textBoxWord.Focus();
            labelActionInfo.Text = "Starting spell && grammer check...";
            _wordApp.Documents.Add();
            foreach (Microsoft.Office.Interop.Word.Language language in _wordApp.Languages)
            {
                if (language.ActiveSpellingDictionary != null && language.ActiveGrammarDictionary != null)
                    _spellCheckLanguages.Add(language);
            }

            AutoDetectLanguage();
            labelActionInfo.Text = "Running spell && grammer check...";
            DoStartSpellCheck();
        }

        private void PluginForm_ResizeEnd(object sender, EventArgs e)
        {
            listViewSubtitle.Columns[4].Width = -2;
        }

        private void buttonUseSuggestion_Click(object sender, EventArgs e)
        {
            if (listBoxSuggestions.SelectedItems.Count == 1)
            {
                string newText = listBoxSuggestions.Items[listBoxSuggestions.SelectedIndex].ToString();
                labelActionInfo.Text = "Used suggestion '" + newText + "'...";    
                FixWord(_currentSpellCollection[_currentSpellCollectionIndex+1], newText);
                ShowNextSpellingError();
            }
        }

        private void FixWord(Microsoft.Office.Interop.Word.Range range, string newText)
        {
            FixWord(range.Text, newText);
        }

        private void FixWord(string oldWord, string changeWord)
        {
            if (listViewSubtitle.SelectedItems.Count > 0)
            {
                if (_currentErrorStart >= _currentParagraph.Text.Length)
                {
                    if (_currentParagraph.Text.Length > 1)
                        _currentErrorStart = _currentParagraph.Text.Length - 1;
                    else
                        _currentErrorStart = 0;
                }
                int startIndex = _currentParagraph.Text.IndexOf(oldWord, _currentErrorStart);
                if (startIndex == -1 && _currentErrorStart > 2)
                    startIndex = _currentParagraph.Text.IndexOf(oldWord, _currentErrorStart - 2);
                if (startIndex == -1)
                    startIndex = _currentParagraph.Text.IndexOf(oldWord);
                if (startIndex >= 0 && startIndex < _currentParagraph.Text.Length && _currentParagraph.Text.Substring(startIndex).Contains(oldWord)) // while
                {
                    bool startOk = (startIndex == 0) || (" <>-[]/()-¿¡\"'‘`´♪—”“`´¶♪:…" + Environment.NewLine).Contains(_currentParagraph.Text[startIndex - 1].ToString());
                    
                    if (startOk)
                    {
                        int end = startIndex + oldWord.Length;
                        if (end <= _currentParagraph.Text.Length)
                        {
                            if ((end == _currentParagraph.Text.Length) || ((" ,.!?:;')<-[]/\"%'‘`´♪—”“`´¶♪:…" + Environment.NewLine).Contains(_currentParagraph.Text[end].ToString())))
                            {
                                _currentParagraph.Text = _currentParagraph.Text.Remove(startIndex, oldWord.Length).Insert(startIndex, changeWord);
                                _currentErrorStart = startIndex+1;
                            }
                        }
                    }
                }
                listViewSubtitle.SelectedItems[0].SubItems[4].Text = _currentParagraph.Text.Replace(Environment.NewLine, "<br/>");
                richTextBoxParagraph.Text = _currentParagraph.Text;
            }
        }

        private void buttonUseSuggestionAlways_Click(object sender, EventArgs e)
        {
            if (listBoxSuggestions.SelectedItems.Count == 1)
            {
                string newText = listBoxSuggestions.Items[listBoxSuggestions.SelectedIndex].ToString();
                labelActionInfo.Text = "Use allways suggestion '" + newText + "'...";    
                _useAlwaysList.Add(_currentSpellCollection[_currentSpellCollectionIndex + 1].Text, newText);
                FixWord(_currentSpellCollection[_currentSpellCollectionIndex+1], newText);
                ShowNextSpellingError();
            }
        }

        private void startSpellCheckFromCurrentLineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DoStartSpellCheck();
        }

        private void PluginForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            object saveOptionsObject = Microsoft.Office.Interop.Word.WdSaveOptions.wdDoNotSaveChanges;
            _wordApp.ActiveDocument.Close(ref saveOptionsObject);
            _wordApp.Quit(ref saveOptionsObject);
        }

        private void buttonGoogleIt_Click(object sender, EventArgs e)
        {
            string text = textBoxWord.Text;
            if (text.Trim().Length > 0)
            {
                labelActionInfo.Text = "Google '" + text + "'...";
                System.Diagnostics.Process.Start("http://www.google.com/search?q=" + System.Uri.EscapeDataString(text));
            }
        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            //FixWord(_currentSpellCollection[_currentSpellCollectionIndex + 1].Text, string.Empty);
            string oldWord = _currentSpellCollection[_currentSpellCollectionIndex + 1].Text;
            if (listViewSubtitle.SelectedItems.Count > 0)
            {
                int startIndex = _currentParagraph.Text.IndexOf(oldWord, _currentStartIndex);
                if (startIndex == -1)
                    startIndex = _currentParagraph.Text.IndexOf(oldWord);
                if (startIndex >= 0 && startIndex < _currentParagraph.Text.Length && _currentParagraph.Text.Substring(startIndex).Contains(oldWord)) // while
                {
                    bool startOk = (startIndex == 0) || (_currentParagraph.Text[startIndex - 1] == ' ') ||
                                    (Environment.NewLine.EndsWith(_currentParagraph.Text[startIndex - 1].ToString()));

                    if (startOk)
                    {
                        int end = startIndex + oldWord.Length;
                        if (end <= _currentParagraph.Text.Length)
                        {
                            if ((end == _currentParagraph.Text.Length) || ((" ,.!?:;')" + Environment.NewLine).Contains(_currentParagraph.Text[end].ToString())))
                                _currentParagraph.Text = _currentParagraph.Text.Remove(startIndex, oldWord.Length);
                            if (startIndex > 0 && _currentParagraph.Text[startIndex-1] == ' ')
                                _currentParagraph.Text = _currentParagraph.Text.Remove(startIndex-1, 1);
                            else if (_currentParagraph.Text.Length > startIndex+1 && _currentParagraph.Text[startIndex] == ' ')
                                _currentParagraph.Text = _currentParagraph.Text.Remove(startIndex, 1);
                        }
                    }
                    //startIndex = _currentParagraph.Text.IndexOf(oldWord, startIndex + 2);
                }
                _currentParagraph.Text = _currentParagraph.Text.Replace("  ", " ");
                listViewSubtitle.SelectedItems[0].SubItems[4].Text = _currentParagraph.Text.Replace(Environment.NewLine, "<br/>");
                richTextBoxParagraph.Text = _currentParagraph.Text;
            }

            labelActionInfo.Text = "Delete word '" + _currentSpellCollection[_currentSpellCollectionIndex + 1].Text + "'...";
            ShowNextSpellingError();
        }

        private void buttonEditWholeText_Click(object sender, EventArgs e)
        {
            textBoxWholeText.Left = richTextBoxParagraph.Left;
            textBoxWholeText.Top = richTextBoxParagraph.Top;
            textBoxWholeText.BringToFront();
            textBoxWholeText.Text = _currentParagraph.Text;
            textBoxWholeText.Visible = true;
            richTextBoxParagraph.Visible = false;
            textBoxWholeText.Focus();
            buttonUpdateWholeText.Visible = true;
            groupBoxSuggestions.Enabled = false;
            groupBoxWordNotFound.Enabled = false;
            buttonEditWord.Visible = true;
            buttonEditWholeText.Enabled = false;
        }

        private void buttonEditWord_Click(object sender, EventArgs e)
        {
            richTextBoxParagraph.BringToFront();
            textBoxWholeText.Text = richTextBoxParagraph.Text;
            richTextBoxParagraph.Visible = true;
            textBoxWholeText.Visible = false;
            buttonUpdateWholeText.Visible = false;
            groupBoxSuggestions.Enabled = true;
            groupBoxWordNotFound.Enabled = true;
            buttonEditWord.Visible = false;
            buttonEditWholeText.Enabled = true;
        }

        private void buttonUpdateWholeText_Click(object sender, EventArgs e)
        {
            if (textBoxWholeText.Visible)
            {
                UpdateWholeText();
                ShowNextSpellingError();
            }
        }

        private void listBoxSuggestions_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            buttonUseSuggestion_Click(null, null);
        }

        private string GetNamesEtcFileName()
        {
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);
            if (path.StartsWith("file:\\"))
                path = path.Remove(0, 6);
            path = Path.Combine(path, "Dictionaries");
            if (!Directory.Exists(path))
                path = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Subtitle Edit"), "Dictionaries");
            return Path.Combine(path, "names_etc.xml");
        }

        private void LoadNamesEtcWithCulture()
        {
            _namesEtcList = new List<string>();
            _namesEtcMultiWordList = new List<string>();
            string namesEtcFileName = GetNamesEtcFileName();
            LoadNamesEtc(_namesEtcList, _namesEtcMultiWordList, namesEtcFileName);            
            _namesEtcLocalFileName = null;
            if (comboBoxDictionaries.SelectedIndex > 0)
            {
                if (_spellCheckLanguages[comboBoxDictionaries.SelectedIndex - 1].Name.StartsWith("English"))
                {
                    _namesEtcLocalFileName = namesEtcFileName.Replace("names_ect.xml", "en_US_names_etc.xml");
                    LoadNamesEtc(_namesEtcList, _namesEtcMultiWordList, _namesEtcLocalFileName);
                }
                else if (_spellCheckLanguages[comboBoxDictionaries.SelectedIndex - 1].Name.StartsWith("Dansk"))
                {
                    _namesEtcLocalFileName = namesEtcFileName.Replace("names_ect.xml", "da_DK_names_etc.xml");
                    LoadNamesEtc(_namesEtcList, _namesEtcMultiWordList, _namesEtcLocalFileName);
                }
            }
            buttonAddToNamesEtcList.Visible = !string.IsNullOrEmpty(_namesEtcLocalFileName);
        }

        private void LoadNamesEtc(List<string> namesEtcList, List<string> namesEtcMultiWordList, string namesEtcFileName)
        {
            try
            {
                var namesEtcDoc = new XmlDocument();
                if (File.Exists(namesEtcFileName))
                {
                    namesEtcDoc.Load(namesEtcFileName);

                    if (namesEtcDoc.DocumentElement != null)
                        foreach (XmlNode node in namesEtcDoc.DocumentElement.SelectNodes("name"))
                        {
                            string s = node.InnerText.Trim();
                            if (s.Contains(" "))
                            {
                                if (!namesEtcMultiWordList.Contains(s))
                                    namesEtcMultiWordList.Add(s);
                            }
                            else
                            {
                                if (!namesEtcList.Contains(s))
                                    namesEtcList.Add(s);
                            }
                        }
                }
            }
            catch
            { 
            }
        }

        private string GetCustomDicFileName()
        {
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);
            if (path.StartsWith("file:\\"))
                path = path.Remove(0, 6);
            path = Path.Combine(path, "Plugins");
            if (!Directory.Exists(path))
                path = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Subtitle Edit"), "Plugins");
            return Path.Combine(path, "customDic.xml");
        }

        private void buttonAddToDictionary_Click(object sender, EventArgs e)
        {
            string newText = textBoxWord.Text;
            if (newText.Trim().Length == 0)
                return;

            if (_wordApp.Application.CustomDictionaries.ActiveCustomDictionary != null)
            {
                labelActionInfo.Text = "Added '" + newText + "' to user dictionary...";    
                string fileName = Path.Combine(_wordApp.Application.CustomDictionaries.ActiveCustomDictionary.Path, _wordApp.Application.CustomDictionaries.ActiveCustomDictionary.Name);
                File.AppendAllText(fileName, newText + Environment.NewLine, Encoding.Unicode);
            }
            _skipAllList.Add(newText);
            _skipAllList.Add(newText.ToUpper());
            if (newText.Length > 1)
                _skipAllList.Add(newText.Substring(0, 1).ToUpper() + newText.Substring(1));            

            ShowNextSpellingError();
        }

        private void comboBoxDictionaries_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadNamesEtcWithCulture();
        }

        private void buttonAddToNamesEtcList_Click(object sender, EventArgs e)
        {
            string newText = textBoxWord.Text;
            if (newText.Trim().Length == 0)
                return;

            labelActionInfo.Text = "Added '" + newText + "' to names/noise list...";    
            if (!string.IsNullOrEmpty(_namesEtcLocalFileName))
            {
                var localNamesEtc = new List<string>();
                LoadNamesEtc(localNamesEtc, localNamesEtc, _namesEtcLocalFileName);

                if (localNamesEtc.Contains(newText))
                    return;
                localNamesEtc.Add(newText);
                localNamesEtc.Sort();

                var namesEtcDoc = new XmlDocument();
                if (File.Exists(_namesEtcLocalFileName))
                    namesEtcDoc.Load(_namesEtcLocalFileName);
                else
                    namesEtcDoc.LoadXml("<ignore_words />");

                XmlNode de = namesEtcDoc.DocumentElement;
                if (de != null)
                {
                    de.RemoveAll();
                    foreach (var name in localNamesEtc)
                    {
                        XmlNode node = namesEtcDoc.CreateElement("name");
                        node.InnerText = name;
                        de.AppendChild(node);
                    }
                    namesEtcDoc.Save(_namesEtcLocalFileName);
                }
                LoadNamesEtcWithCulture();

                ShowNextSpellingError();
            }
        }

        private void textBoxWord_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                buttonChange_Click(null, null);
            }
        }

        private void textBoxWord_TextChanged(object sender, EventArgs e)
        {
            bool changed = _currentErrorText != textBoxWord.Text;
            buttonChange.Enabled = changed;
            buttonChangeAll.Enabled = changed;
        }

        private void textBoxWord_KeyUp(object sender, KeyEventArgs e)
        {
            textBoxWord_TextChanged(sender, e);
        }

    }
}
