using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public partial class EditReplaceList : Form
    {
        public SortedDictionary<string, string> ReplaceList = new SortedDictionary<string, string>();

        public EditReplaceList(SortedDictionary<string, string> customList)
        {
            InitializeComponent();

            foreach (var kvp in customList)
            {
                dataGridView1.Rows.Add(kvp.Key, kvp.Value);
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Cells[0].Value != null && row.Cells[1].Value != null)
                {
                    string key = row.Cells[0].Value.ToString().Trim();
                    string value = row.Cells[1].Value.ToString();
                    if (key.Length == 1)
                    {
                        if (!ReplaceList.ContainsKey(key))
                            ReplaceList.Add(key, value);
                    }
                }
            }

            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void EditReplaceList_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

    }
}
