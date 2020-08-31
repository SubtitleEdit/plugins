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
            progressBar1.Visible = false;
            //listViewMovies.Activation = ItemActivation.TwoClick;
            listViewMovies.MouseDoubleClick += (sender, e) =>
            {
                ListViewItem li = listViewMovies.GetItemAt(e.X, e.Y);
                System.Diagnostics.Debug.WriteLine($"clicks: {e.Clicks}");

                if (e.Clicks == 2 && li != null)
                {
                    AddItem(li.Tag as SearchMovie);
                    DialogResult = DialogResult.OK;
                }
            };
            KeyPreview = true;
            KeyUp += (sender, e) =>
            {
                if (e.KeyCode == Keys.Escape)
                {
                    DialogResult = DialogResult.Cancel;
                }
            };

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

        private bool ShouldDisplayInfo(SearchMovie movie) => !(movie == null || string.IsNullOrWhiteSpace(movie.Title) || movie.ReleaseDate == null);

        private void ButtonOK_Click(object sender, EventArgs e)
        {
            if (listViewMovies.SelectedItems.Count == 0)
            {
                return;
            }
            AddItem(listViewMovies.SelectedItems[0].Tag as SearchMovie);
            DialogResult = DialogResult.OK;
        }

        private void AddItem(SearchMovie searchMovie)
        {
            ID = searchMovie.Id;
            if (Configs.Settings.Movies.Any(m => m.Id == searchMovie.Id) == false)
            {
                var movie = new Movie
                {
                    Id = searchMovie.Id,
                    OriginalTitle = searchMovie.OriginalTitle,
                    ReleaseDate = searchMovie.ReleaseDate.Value,
                    Title = searchMovie.Title,
                };
                Configs.Settings.Movies.Add(movie);
            }
        }

        private void TextBoxSearchQuery_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ButtonSearch_Click(null, EventArgs.Empty);
            }
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}
