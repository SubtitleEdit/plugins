using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SubtitleEdit.Logic
{
    internal static class UiUtil
    {
        private static readonly Dictionary<string, Keys> AllKeys = new Dictionary<string, Keys>();
        private static Keys _helpKeys;

        public static Keys GetKeys(string keysInString)
        {
            if (string.IsNullOrEmpty(keysInString))
            {
                return Keys.None;
            }

            if (AllKeys.Count == 0)
            {
                foreach (Keys val in Enum.GetValues(typeof(Keys)))
                {
                    var k = val.ToString().ToLowerInvariant();
                    if (!AllKeys.ContainsKey(k))
                    {
                        AllKeys.Add(k, val);
                    }
                }
                if (!AllKeys.ContainsKey("pagedown"))
                {
                    AllKeys.Add("pagedown", Keys.RButton | Keys.Space);
                }

                if (!AllKeys.ContainsKey("home"))
                {
                    AllKeys.Add("home", Keys.MButton | Keys.Space);
                }

                if (!AllKeys.ContainsKey("capslock"))
                {
                    AllKeys.Add("capslock", Keys.CapsLock);
                }
            }

            var parts = keysInString.ToLowerInvariant().Split(new[] { '+' }, StringSplitOptions.RemoveEmptyEntries);
            var resultKeys = Keys.None;
            foreach (var k in parts)
            {
                if (AllKeys.ContainsKey(k))
                {
                    resultKeys |= AllKeys[k];
                }
            }

            return resultKeys;
        }

        public static Keys HelpKeys
        {
            get
            {
                if (_helpKeys == Keys.None)
                {
                    _helpKeys = GetKeys("F1");
                }
                return _helpKeys;
            }
            set => _helpKeys = value;
        }

        public static void SetButtonHeight(Control control, int newHeight, int level)
        {
            if (level > 6)
            {
                return;
            }

            if (control.HasChildren)
            {
                foreach (Control subControl in control.Controls)
                {
                    if (subControl.HasChildren)
                    {
                        SetButtonHeight(subControl, newHeight, level + 1);
                    }
                    else if (subControl is Button)
                    {
                        subControl.Height = newHeight;
                    }
                }
            }
            else if (control is Button)
            {
                control.Height = newHeight;
            }
        }

        private const string BreakChars = "\",:;.¡!¿?()[]{}<>♪♫-–—/#*|؟،";

        public static void ApplyControlBackspace(TextBox textBox)
        {
            if (textBox.SelectionLength == 0)
            {
                var text = textBox.Text;
                var deleteUpTo = textBox.SelectionStart;
                if (deleteUpTo > 0 && deleteUpTo <= text.Length)
                {
                    text = text.Substring(0, deleteUpTo);
                    var textElementIndices = StringInfo.ParseCombiningCharacters(text);
                    var index = textElementIndices.Length;
                    var textIndex = deleteUpTo;
                    var deleteFrom = -1;
                    while (index > 0)
                    {
                        index--;
                        textIndex = textElementIndices[index];
                        if (!IsSpaceCategory(CharUnicodeInfo.GetUnicodeCategory(text, textIndex)))
                        {
                            break;
                        }
                    }
                    if (index > 0) // HTML tag?
                    {
                        if (text[textIndex] == '>')
                        {
                            var openingBracketIndex = text.LastIndexOf('<', textIndex - 1);
                            if (openingBracketIndex >= 0 && text.IndexOf('>', openingBracketIndex + 1) == textIndex)
                            {
                                deleteFrom = openingBracketIndex; // delete whole tag
                            }
                        }
                        else if (text[textIndex] == '}')
                        {
                            var startIdx = text.LastIndexOf(@"{\", textIndex - 1, StringComparison.Ordinal);
                            if (startIdx >= 0 && text.IndexOf('}', startIdx + 1) == textIndex)
                            {
                                deleteFrom = startIdx;
                            }
                        }
                    }
                    if (deleteFrom < 0)
                    {
                        if (Enumerable.Contains(BreakChars, text[textIndex]))
                        {
                            deleteFrom = -2;
                        }

                        while (index > 0)
                        {
                            index--;
                            textIndex = textElementIndices[index];
                            if (IsSpaceCategory(CharUnicodeInfo.GetUnicodeCategory(text, textIndex)))
                            {
                                if (deleteFrom > -2)
                                {
                                    if (deleteFrom < 0)
                                    {
                                        deleteFrom = textElementIndices[index + 1];
                                    }
                                    break;
                                }
                                deleteFrom = textElementIndices[index + 1];
                                if (!Enumerable.Contains(":!?", text[deleteFrom]))
                                {
                                    break;
                                }
                            }
                            else if (Enumerable.Contains(BreakChars, text[textIndex]))
                            {
                                if (deleteFrom > -2)
                                {
                                    if (deleteFrom < 0)
                                    {
                                        deleteFrom = textElementIndices[index + 1];
                                    }
                                    break;
                                }
                            }
                            else
                            {
                                deleteFrom = -1;
                            }
                        }
                    }
                    if (deleteFrom < deleteUpTo)
                    {
                        if (deleteFrom < 0)
                        {
                            deleteFrom = 0;
                        }
                        textBox.Select(deleteFrom, deleteUpTo - deleteFrom);
                        textBox.Paste(string.Empty);
                    }
                }
            }
        }

        public static void SelectWordAtCaret(TextBox textBox)
        {
            var text = textBox.Text;
            var endIndex = textBox.SelectionStart;
            var startIndex = endIndex;

            while (startIndex > 0 && !IsSpaceCategory(CharUnicodeInfo.GetUnicodeCategory(text[startIndex - 1])) && !Enumerable.Contains(BreakChars, text[startIndex - 1]))
            {
                startIndex--;
            }
            textBox.SelectionStart = startIndex;

            while (endIndex < text.Length && !IsSpaceCategory(CharUnicodeInfo.GetUnicodeCategory(text[endIndex])) && !Enumerable.Contains(BreakChars, text[endIndex]))
            {
                endIndex++;
            }
            textBox.SelectionLength = endIndex - startIndex;
        }

        private static bool IsSpaceCategory(UnicodeCategory c)
        {
            return c == UnicodeCategory.SpaceSeparator || c == UnicodeCategory.Control || c == UnicodeCategory.LineSeparator || c == UnicodeCategory.ParagraphSeparator;
        }

        private static void AddExtension(StringBuilder sb, string extension)
        {
            if (!sb.ToString().Contains("*" + extension + ";", StringComparison.OrdinalIgnoreCase))
            {
                sb.Append('*');
                sb.Append(extension.TrimStart('*'));
                sb.Append(';');
            }
        }

        public static string ListViewLineSeparatorString = "<br />";

        public static string GetListViewTextFromString(string s) =>
            s.Replace(Environment.NewLine, ListViewLineSeparatorString);

        public static string GetStringFromListViewText(string lviText) =>
            lviText.Replace(ListViewLineSeparatorString, Environment.NewLine);

        public static void AutoSizeLastColumn(this ListView listView) =>
            listView.Columns[listView.Columns.Count - 1].Width = -2;

        public static void SelectAll(this ListView lv)
        {
            lv.BeginUpdate();
            foreach (ListViewItem item in lv.Items)
            {
                item.Selected = true;
            }
            lv.EndUpdate();
        }

        public static void SelectFirstSelectedItemOnly(this ListView lv)
        {
            int itemsCount = lv.SelectedItems.Count - 1;
            if (itemsCount > 0)
            {
                lv.BeginUpdate();
                do
                {
                    lv.SelectedItems[itemsCount--].Selected = false;
                }
                while (itemsCount > 0);
                if (lv.SelectedItems.Count > 0)
                {
                    lv.EnsureVisible(lv.SelectedItems[0].Index);
                    lv.FocusedItem = lv.SelectedItems[0];
                }
                else if (lv.Items.Count > 0)
                {
                    lv.EnsureVisible(0);
                    lv.FocusedItem = lv.Items[0];
                }
                lv.EndUpdate();
            }
        }

        public static void InverseSelection(this ListView lv)
        {
            lv.BeginUpdate();
            foreach (ListViewItem item in lv.Items)
            {
                item.Selected = !item.Selected;
            }
            lv.EndUpdate();
        }

        public static void SelectAll(this ListBox listbox)
        {
            listbox.BeginUpdate();
            for (int i = 0; i < listbox.Items.Count; i++)
            {
                listbox.SetSelected(i, true);
            }
            listbox.EndUpdate();
        }

        public static void InverseSelection(this ListBox listbox)
        {
            listbox.BeginUpdate();
            for (int i = 0; i < listbox.Items.Count; i++)
            {
                listbox.SetSelected(i, !listbox.GetSelected(i));
            }
            listbox.EndUpdate();
        }

        internal static void CleanUpMenuItemPlugin(ToolStripMenuItem tsmi)
        {
            if (tsmi == null)
            {
                return;
            }
            for (int k = tsmi.DropDownItems.Count - 1; k > 0; k--)
            {
                ToolStripItem x = tsmi.DropDownItems[k];
                var fileName = (string)x.Tag;
                if (!string.IsNullOrEmpty(fileName) && fileName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                {
                    tsmi.DropDownItems.Remove(x);
                }
            }
        }

        public static string DecimalSeparator => CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
    }
}
