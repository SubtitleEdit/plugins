using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WatTmdb.V3;
using WatTmdb;
using RestSharp;

namespace tmdb_casing
{
    public partial class MovieSeacher : Form
    {
        private const string _key = ""; // this is secret!
        private Tmdb tmdb;
        private TmdbMovie _movie;
        private SearchType _searchType = SearchType.TMDBID;
        public List<string> Names { get; private set; }

        enum SearchType
        {
            TMDBID,
            IMDBID,
            Title
        }

        public MovieSeacher()
        {
            InitializeComponent();
        }

        private void buttonSeach_Click(object sender, EventArgs e)
        {
            string pattern = textBoxMovieInfo.Text.Trim();
            if (string.IsNullOrEmpty(pattern))
            {
                MessageBox.Show("Nothing entered make sure you entered movie title or code!");
                return;
            }
            GetMovieID(pattern);
        }

        private void GetMovieID(string patten)
        {
            tmdb = new Tmdb(_key);
            var movieSearch = tmdb.SearchMovie(patten, 1);
            if (movieSearch != null)
            {
                AddToListView(movieSearch.results);
            }

            //switch (_searchType)
            //{
            //    case SearchType.TMDBID:
            //        break;
            //    case SearchType.IMDBID:
            //        var movie = tmdb.GetMovieByIMDB("19237498172");
            //        break;
            //    case SearchType.Title:
            //        var movieSearch = tmdb.SearchMovie(searchPattern, 1);
            //        _listMovieResult = movieSearch.results;

            //        if (_listMovieResult != null)
            //            AddToListView();

            //        break;
            //    default:
            //        break;
            //}
        }

        private void AddToListView(List<MovieResult> _movieResult)
        {
            foreach (MovieResult movieR in _movieResult)
            {
                var item = new ListViewItem(movieR.id.ToString());
                var subItem = new ListViewItem.ListViewSubItem(item, movieR.title);
                item.Tag = movieR.id;
                listView1.Items.Add(item);
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            var cast = tmdb.GetMovieCast((int)listView1.FocusedItem.Tag);
            if (cast.cast.Count > 0)
            {
                foreach (var castInfo in cast.cast)
                {
                    Names.Add(castInfo.name);
                }
            }
            DialogResult = DialogResult.OK;
        }
    }
}
