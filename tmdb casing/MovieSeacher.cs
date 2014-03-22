using System;
using System.Collections.Generic;
using System.Windows.Forms;
using RestSharp;
using WatTmdb.V3;

namespace Nikse
{
    public partial class MovieSeacher : Form
    {
        public List<string> Characters { get; set; }
        private Tmdb _tmdb;
        private string _apiKey = "";

        public MovieSeacher()
        {
            InitializeComponent();
            _tmdb = new Tmdb(_apiKey); // TODO: If dll wasn't found in Subtitle\Plugins\ this will bow up!
        }

        private void buttonSeach_Click(object sender, EventArgs e)
        {
            string pattern = textBoxMovieInfo.Text.Trim();
            if (pattern.Length > 0)
                GetMovieID(pattern);
        }

        private void GetMovieID(string patten)
        {
            var movieSearch = _tmdb.SearchMovie(patten, 1);
            if (movieSearch.total_results > 0)
            {
                AddToListView(movieSearch.results);
            }
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
    }
}