using System;
using System.Windows.Forms;
using TMDbLib.Client;
using TMDbLib.Objects.Search;

namespace OnlineCasing.Forms
{
    public partial class GetMovieID : Form
    {
        private readonly TMDbClient Client;

        public GetMovieID(TMDbClient client)
        {
            InitializeComponent();
            Client = client;
        }

        public int ID { get; private set; }

        private async void ButtonSearch_Click(object sender, EventArgs e)
        {
            if (Client == null)
            {
                return;
            }

            var searchResult = await Client.SearchMovieAsync(textBoxSearchQuery.Text);

            listViewMovies.BeginUpdate();
            listViewMovies.Items.Clear();

            //AppDomain
            //AppDomainSetup
            //Assembly

            foreach (SearchMovie movieResult in searchResult.Results)
            {
                if (!ShouldDisplayInfo(movieResult))
                    continue;
                var lvi = new ListViewItem(movieResult.Title);
                lvi.SubItems.Add(movieResult.ReleaseDate.Value.Year.ToString());
                lvi.Tag = movieResult;
                listViewMovies.Items.Add(lvi);
            }
            listViewMovies.EndUpdate();
        }

        private bool ShouldDisplayInfo(SearchMovie movie)
        {
            return movie == null || string.IsNullOrEmpty(movie.Title) || movie.ReleaseDate == null ? false : true;
        }

        private void ButtonOK_Click(object sender, EventArgs e)
        {
            //listview
            if (listViewMovies.SelectedItems.Count == 0)
            {
                return;
            }

            var movieSearch = listViewMovies.SelectedItems[0].Tag as SearchMovie;
            ID = movieSearch.Id;
            DialogResult = DialogResult.OK;
        }
    }
}
