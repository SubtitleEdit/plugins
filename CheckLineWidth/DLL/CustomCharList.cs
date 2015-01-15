using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public partial class CustomCharList : Form
    {
        public SortedDictionary<string, int> CharList = new SortedDictionary<string, int>();

        public CustomCharList(SortedDictionary<string, int> customList)
        {
            InitializeComponent();

            foreach (var kvp in customList)
            {
                dataGridView1.Rows.Add(kvp.Key, kvp.Value.ToString());
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
                    int v;
                    if (key.Length == 1 && int.TryParse(value, out v))
                    {
                        if (!CharList.ContainsKey(key))
                            CharList.Add(key, v);
                    }
                }
            }

            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}
