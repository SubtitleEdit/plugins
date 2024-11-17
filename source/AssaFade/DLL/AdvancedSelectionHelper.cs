﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using SubtitleEdit.Logic;

namespace SubtitleEdit
{
    public partial class AdvancedSelectionHelper : Form
    {
        public int[] Indices { get; private set; }
        private readonly Subtitle _subtitle;
        private bool _loading;
        private const int FunctionStyle = 0;
        private const int FunctionActor = 1;

        public AdvancedSelectionHelper(Subtitle subtitle)
        {
            InitializeComponent();
            UiUtil.FixFonts(this);

            _loading = true;
            _subtitle = subtitle;
            Indices = Array.Empty<int>();

            comboBoxRule.Items.Clear();
            comboBoxRule.Items.Add("Style");
            comboBoxRule.Items.Add("Actor");
            comboBoxRule.SelectedIndex = 0;

            _loading = false;
            Preview();
        }

        private void Preview()
        {
            if (_loading)
            {
                return;
            }

            var listViewItems = new List<ListViewItem>();
            listViewFixes.BeginUpdate();
            listViewFixes.Items.Clear();
            var styles = new List<string>();
            var actors = new List<string>();
            if (comboBoxRule.SelectedIndex == FunctionStyle) // Select styles
            {
                foreach (ListViewItem item in listViewStyles.Items)
                {
                    if (item != null && item.Checked)
                    {
                        styles.Add(item.Text);
                    }
                }
            }
            else if (comboBoxRule.SelectedIndex == FunctionActor) // Select actors
            {
                foreach (ListViewItem item in listViewStyles.Items)
                {
                    if (item != null && item.Checked)
                    {
                        actors.Add(item.Text);
                    }
                }
            }

            var indices = new List<int>();
            for (int i = 0; i < _subtitle.Paragraphs.Count; i++)
            {
                var p = _subtitle.Paragraphs[i];

                if (comboBoxRule.SelectedIndex == FunctionStyle) // Select styles
                {
                    if (styles.Contains(string.IsNullOrEmpty(p.Style) ? p.Extra : p.Style))
                    {
                        listViewItems.Add(MakeListViewItem(p, i));
                        indices.Add(i);
                    }
                }
                else if (comboBoxRule.SelectedIndex == FunctionActor) // Select actors
                {
                    if (actors.Contains(p.Actor))
                    {
                        listViewItems.Add(MakeListViewItem(p, i));
                        indices.Add(i);
                    }
                }
            }

            Indices = indices.ToArray();
            listViewFixes.ItemChecked -= listViewFixes_ItemChecked;
            listViewFixes.Items.AddRange(listViewItems.ToArray());
            listViewFixes.ItemChecked += listViewFixes_ItemChecked;
            listViewFixes.EndUpdate();
            groupBoxPreview.Text = $"Matching lines: {listViewFixes.Items.Count:#,##0}";
        }

        private ListViewItem MakeListViewItem(Paragraph p, int index)
        {
            var item = new ListViewItem(string.Empty) { Tag = index, Checked = true };
            item.SubItems.Add(p.Number.ToString());
            item.SubItems.Add(UiUtil.GetListViewTextFromString(p.Text));
            item.SubItems.Add(string.IsNullOrEmpty(p.Style) ? p.Extra : p.Style);
            return item;
        }

        private void comboBoxRule_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (comboBoxRule.SelectedIndex == FunctionStyle)
            {
                listViewStyles.Visible = true;
                listViewStyles.BringToFront();
                FillStyles();
            }
            else if (comboBoxRule.SelectedIndex == FunctionActor)
            {
                listViewStyles.Visible = true;
                listViewStyles.BringToFront();
                FillActors();
            }

            Preview();
        }

        private void FillStyles()
        {
            listViewStyles.AutoSizeLastColumn();
            var styles = AdvancedSubStationAlpha.GetStylesFromHeader(_subtitle.Header);
            var oldLoading = _loading;
            _loading = true;
            listViewStyles.Items.Clear();
            listViewStyles.Items.AddRange(styles.OrderBy(p => p).Select(p => new ListViewItem { Text = p }).ToArray());
            _loading = oldLoading;
        }

        private void FillActors()
        {
            listViewStyles.AutoSizeLastColumn();
            var actors = new List<string>();
            foreach (var paragraph in _subtitle.Paragraphs)
            {
                if (!string.IsNullOrEmpty(paragraph.Actor) && !actors.Contains(paragraph.Actor))
                {
                    actors.Add(paragraph.Actor);
                }
            }

            var oldLoading = _loading;
            _loading = true;
            listViewStyles.Items.Clear();
            listViewStyles.Items.AddRange(actors.OrderBy(p => p).Select(p => new ListViewItem { Text = p }).ToArray());
            _loading = oldLoading;
        }

        private void listViewStyles_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            Preview();
        }

        private void buttonCancel_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void buttonOK_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void AdvancedSelectionHelper_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void listViewFixes_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (e.Item == null)
            {
                return;
            }

            if (int.TryParse(e.Item.Tag.ToString(), out var index))
            {
                Indices = Indices.ToList().Where(p => p != index).ToArray();
            }
        }
    }
}
