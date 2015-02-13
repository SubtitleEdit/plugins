using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.PluginLogic
{
    internal partial class GetNames : Form
    {
        public GetNames(Form mainForm, Subtitle sub)
        {
            InitializeComponent();
            this.FormClosed += delegate { mainForm.Show(); };
            FindNames(sub);
        }

        private void FindNames(Subtitle sub)
        {
            var totalFound = 0;
            for (int i = 0; i < sub.Paragraphs.Count; i++)
            {
                var p = sub.Paragraphs[i];
                var text = p.Text;
                if (text.IndexOf('(') < 0 && text.IndexOf('[') < 0)
                    continue;

                text = text.Replace("[", "(").Replace("]", ")");
                var idx = text.IndexOf('(');
                while (idx >= 0)
                {
                    var endIdx = text.IndexOf(')', idx + 1);
                    if (endIdx < 0)
                        break;
                    var name = text.Substring(idx, endIdx - idx + 1);
                    AddToList(name, ref totalFound);
                    idx = text.IndexOf('(', endIdx + 1);
                }
            }
            if (totalFound > 0)
            {
                this.label1.Text = "Found Names: " + totalFound;
            }
        }

        private void AddToList(string name, ref int totalFound)
        {
            name = name.Substring(1);
            name = name.Substring(0, name.Length - 1).ToUpper();
            if (!listBox1.Items.Contains(name) && !Utilities.ListNames.Contains(name))
            {
                this.listBox1.Items.Add(name);
                totalFound++;
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex >= 0)
            {
                var idx = listBox1.SelectedIndex;
                this.textBox1.Text = listBox1.Items[idx].ToString();
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
                name = name.Substring(1);
                name = name.Substring(0, name.Length - 1);

                Utilities.AddNameToList(name);
                listBox1.Items.RemoveAt(idx);
                if (idx < listBox1.Items.Count)
                {
                    this.textBox1.Text = listBox1.Items[idx].ToString();
                    listBox1.SelectedIndex = idx;
                }
                else
                    textBox1.Text = "";
            }
        }
    }
}
