using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using OpenSubtitles;

namespace OpenSubtitlesUpload
{
    public partial class ImdbSearch : Form
    {
        public string ImdbId { get; private set; }
        private readonly BackgroundWorker backGroundWoker;
        private readonly Action<bool> MethodInvoker;

        public ImdbSearch(string title, OpenSubtitlesApi api)
        {
            InitializeComponent();
            labelStatus.Text = string.Empty;
            textBoxSearchQuery.Text = title;

            MethodInvoker = (b) => buttonSearch.Enabled = b;
            backGroundWoker = new BackgroundWorker();

            #region Event Handlers
            backGroundWoker.DoWork += (s, ev) =>
            {
                //this.buttonSearch.BeginInvoke(new MethodInvoker(() => buttonSearch.Enabled = false));
                Invoke(MethodInvoker, false);
                var query = ev.Argument as string;
                ev.Result = api.SearchMoviesOnIMDB(query);
            };

            backGroundWoker.RunWorkerCompleted += (s, ev) =>
            {
                var dic = ev.Result as Dictionary<string, string>;
                Invoke(MethodInvoker, true);
                if (dic == null || dic.Count == 0)
                {
                    MessageBox.Show("Movie/Tv-Show not found!!!", "Not found!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                listViewSearchResults.Items.Clear();
                foreach (KeyValuePair<string, string> kvp in dic)
                {
                    var item = new ListViewItem(kvp.Key);
                    item.SubItems.Add(kvp.Value);
                    listViewSearchResults.Items.Add(item);
                }
                labelStatus.Text = "Done searching";
            };
            #endregion
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (listViewSearchResults.SelectedItems.Count == 1)
            {
                ImdbId = listViewSearchResults.SelectedItems[0].Text;
                DialogResult = DialogResult.OK;
            }
            else
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void buttonSearch_Click(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(textBoxSearchQuery.Text))
                {
                    labelStatus.Text = "Searching...";
                    backGroundWoker.RunWorkerAsync(textBoxSearchQuery.Text);
                }
                else
                {
                    labelStatus.Text = "Invalid search query";
                }
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