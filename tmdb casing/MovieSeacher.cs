using System;
using System.Collections;
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
        private string _apiKey = "";

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
            TmdbMovieSearch _movieSearch = null;
            if (this.listView1.Items != null)
                this.listView1.Items.Clear();
            /*
            ThreadPool.QueueUserWorkItem((state) =>
            {
                _movieSearch = _tmdb.SearchMovie(patten, 1);
                listView1.BeginInvoke(new MethodInvoker(() => AddToListView(_movieSearch.results)));
            });
             */

            // TODO:
            var dowork = new System.ComponentModel.BackgroundWorker();
            dowork.DoWork += (s, e) =>
                {
                    try
                    {
                        _movieSearch = _tmdb.SearchMovie(patten, 1);
                    }
                    catch (Exception ex) { MessageBox.Show(ex.Message); }
                };
            dowork.RunWorkerCompleted += (s, e) =>
            {
                if (_movieSearch.results != null)
                    AddToListView(_movieSearch.results);
                else
                    MessageBox.Show("Unexpected return type");
            };
            dowork.RunWorkerAsync();
            //System.Threading.Tasks.Task.Factory.FromAsync(IAsyncResult)
        }

        class MyClass : IAsyncResult
        {

            public object AsyncState
            {
                get { throw new NotImplementedException(); }
            }

            public WaitHandle AsyncWaitHandle
            {
                get { throw new NotImplementedException(); }
            }

            public bool CompletedSynchronously
            {
                get { throw new NotImplementedException(); }
            }

            public bool IsCompleted
            {
                get { throw new NotImplementedException(); }
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
            this.OnResize(null);
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            var castCollection = _tmdb.GetMovieCast((int)listView1.FocusedItem.Tag);
            if (castCollection.cast.Count > 0)
            {
                this.Characters = new List<string>();
                foreach (var c in castCollection.cast)
                {
                    string name = c.character.Trim();

                    if (name.Replace(" ", string.Empty).Length != name.Length)
                    {
                        var listName = NamesAnalyer(name);
                        // todo: check if listName is null
                        foreach (var n in listName)
                            if (!Characters.Contains(n))
                                this.Characters.Add(n);
                        continue;
                    }
                    this.Characters.Add(c.character);
                }
            }
            DialogResult = DialogResult.OK;
        }

        private IEnumerable<string> NamesAnalyer(string name)
        {
            IList<string> ignoreList = new List<string>() { "the", "or", "and", "of" };
            IList<string> onlyNames = new List<string>();

            var listWords = name.Replace(" ", "|").Split('|');
            foreach (var word in listWords)
            {
                string w = System.Text.RegularExpressions.Regex.Replace(word, "\\W", string.Empty);
                if (!string.IsNullOrWhiteSpace(w))
                    if (!ignoreList.Contains(w.ToLower()))
                        onlyNames.Add(word);
            }
            return onlyNames;
        }
        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }
    }
}