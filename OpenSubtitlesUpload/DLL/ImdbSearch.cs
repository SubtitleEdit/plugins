using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using OpenSubtitles;
using System.Threading;

namespace OpenSubtitlesUpload
{
    public partial class ImdbSearch : Form
    {
        OpenSubtitlesApi _api;
        public string ImdbId;

        public ImdbSearch(string title, OpenSubtitlesApi api)
        {
            InitializeComponent();
            labelStatus.Text = string.Empty;
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
                labelStatus.Text = "Searching...";
                var backGroundWoker = new BackgroundWorker();
                backGroundWoker.DoWork += (s, ev) =>
                {
                    //this.buttonSearch.BeginInvoke(new MethodInvoker(() => buttonSearch.Enabled = false));
                    this.Invoke(new MethodInvoker(() => buttonSearch.Enabled = false));
                    string query = ev.Argument as string;
                    Dictionary<string, string> dic = null;
                    dic = _api.SearchMoviesOnIMDB(textBoxSearchQuery.Text);
                    ev.Result = dic;
                };

                backGroundWoker.RunWorkerCompleted += (s, ev) =>
                {
                    var dic = ev.Result as Dictionary<string, string>;
                    if (dic.Count == 0)
                    {
                        MessageBox.Show("Movie/Tv-Show not found!!!", "Not fond", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        this.Invoke(new MethodInvoker(() => buttonSearch.Enabled = true));
                        return;
                    }

                    listViewSearchResults.Items.Clear();
                    foreach (KeyValuePair<string, string> kvp in dic)
                    {
                        ListViewItem item = new ListViewItem(kvp.Key);
                        item.SubItems.Add(kvp.Value);
                        listViewSearchResults.Items.Add(item);
                    }
                    this.Invoke(new MethodInvoker(() => buttonSearch.Enabled = true));
                    labelStatus.Text = string.Empty;
                };

                backGroundWoker.RunWorkerAsync(textBoxSearchQuery.Text);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
                labelStatus.Text = string.Empty;
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
