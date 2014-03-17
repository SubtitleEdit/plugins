using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using OpenSubtitles;

namespace OpenSubtitlesUpload
{
    public partial class ImdbSearch : Form
    {
        OpenSubtitlesApi _api;
        public string ImdbId;

        public ImdbSearch(string title, OpenSubtitlesApi api)
        {
            InitializeComponent();
            _api = api;
            textBoxSearchQuery.Text = title;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (listViewSearchResults.SelectedItems.Count == 1)
            {
                ImdbId = listViewSearchResults.SelectedItems[0].Text;
                DialogResult = DialogResult.OK;
            }            
        }

        private void buttonSearch_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                var dic = _api.SearchMoviesOnIMDB(textBoxSearchQuery.Text);
                listViewSearchResults.Items.Clear();
                foreach (KeyValuePair<string, string> kvp in dic)
                {
                    ListViewItem item = new ListViewItem(kvp.Key);
                    item.SubItems.Add(kvp.Value);
                    listViewSearchResults.Items.Add(item);
                }
                Cursor = Cursors.Default;
            }
            catch (Exception exception)
            {
                Cursor = Cursors.Default;
                MessageBox.Show(exception.Message);
            }
        }

        private void listViewSearchResults_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listViewSearchResults.SelectedItems.Count == 1)
            {
                buttonOK_Click(sender, e);
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void textBoxSearchQuery_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                buttonSearch_Click(sender, e);
            else if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.Cancel;
        }
    }
}
