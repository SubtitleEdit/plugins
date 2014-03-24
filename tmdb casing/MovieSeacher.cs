using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using WatTmdb.V3;

namespace Nikse
{
    public partial class MovieSeacher : Form
    {
        public List<string> Characters { get; set; }
        private Tmdb _tmdb;
        private string _apiKey = "9ca57d5af5c9f182fc0dfe32dcdca40f";

        public MovieSeacher()
        {
            InitializeComponent();
            _tmdb = new Tmdb(_apiKey); // TODO: If dll wasn't found in Subtitle\Plugins\ this will bow up!
            this.Resize += (s, e) =>
            {
                this.listView1.Columns[2].Width = -2;
            };
        }

        private void buttonSeach_Click(object sender, EventArgs e)
        {
            string pattern = textBoxMovieInfo.Text.Trim();
            if (pattern.Length > 0)
                GetMovieID(pattern);
        }

        private void GetMovieID(string patten)
        {
            TmdbMovieSearch _movieSearch;
            if (this.listView1.Items != null)
                this.listView1.Items.Clear();
            ThreadPool.QueueUserWorkItem((state) =>
            {
                _movieSearch = _tmdb.SearchMovie(patten, 1);
                listView1.BeginInvoke(new MethodInvoker(() => AddToListView(_movieSearch.results)));
            });
        }

        private void AddToListView(List<MovieResult> results)
        {
            foreach (MovieResult movieR in results)
            {
                var item = new ListViewItem(movieR.id.ToString()) { Tag = movieR.id };
                var subItem = new ListViewItem.ListViewSubItem(item, movieR.title);
                item.SubItems.Add(subItem);
                subItem = new ListViewItem.ListViewSubItem(item, movieR.release_date);
                item.SubItems.Add(subItem);
                listView1.Items.Add(item);
            }
            this.OnResize(null);
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            var cast = _tmdb.GetMovieCast((int)listView1.FocusedItem.Tag);
            if (cast.cast.Count > 0)
            {
                this.Characters = new List<string>();
                foreach (var c in cast.cast)
                {
                    this.Characters.Add(c.character);
                }
            }
            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }
    }
}