using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using TMDbLib.Client;
using TMDbLib.Objects.General;
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

            progressBar1.Visible = true;
            progressBar1.Style = ProgressBarStyle.Marquee;

            listViewMovies.BeginUpdate();
            listViewMovies.Items.Clear();

            SearchContainer<SearchMovie> searchResult = await Client.SearchMovieAsync(textBoxSearchQuery.Text).ConfigureAwait(true);

            progressBar1.Style = ProgressBarStyle.Blocks;
            progressBar1.Visible = false;

            if (searchResult == null || searchResult.Results.Count == 0)
            {
                MessageBox.Show("No movie found using the specified query!", "No movie found!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                listViewMovies.EndUpdate();
                return;
            }

            //AppDomain
            //AppDomainSetup
            //Assembly

            IEnumerable<ListViewItem> foundMovies = searchResult.Results
            .Where(movie => ShouldDisplayInfo(movie))
            .Select(movie => new ListViewItem(movie.Title)
            {
                SubItems = { movie.ReleaseDate.Value.Year.ToString() },
                Tag = movie
            });

            listViewMovies.Items.AddRange(foundMovies.ToArray());
            listViewMovies.EndUpdate();
        }

        private bool ShouldDisplayInfo(SearchMovie movie) => movie == null || string.IsNullOrEmpty(movie.Title) || movie.ReleaseDate == null ? false : true;

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

        private void TextBoxSearchQuery_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ButtonSearch_Click(null, EventArgs.Empty);
            }
        }
    }
}
