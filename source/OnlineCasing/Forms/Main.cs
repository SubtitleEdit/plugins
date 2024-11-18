using Nikse.SubtitleEdit.PluginLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using TMDbLib.Client;
using TMDbLib.Objects.Movies;

namespace OnlineCasing.Forms
{
    public partial class Main : Form
    {
        private TMDbClient _client;
        private readonly Subtitle _subtitle;
        private readonly string _uILineBreak;
        public string Subtitle { get; private set; }

        public Main(Subtitle subtitle, string UILineBreak)
        {
            InitializeComponent();

            KeyPreview = true;
            KeyDown += (sender, e) =>
            {
                if (e.KeyCode == Keys.Escape)
                {
                    Close();
                }
            };

            StartPosition = FormStartPosition.CenterParent;

#if DEBUG
            //APIKey = Environment.GetEnvironmentVariable("themoviedbapikey");
#endif

            // load apikey
            if (!string.IsNullOrEmpty(Configs.Settings.ApiKey))
            {
                _client = new TMDbClient(Configs.Settings.ApiKey, true);
            }

            contextMenuStrip1.Opening += (sender, e) =>
            {
                if (checkedListBoxNames.Items.Count == 0 || checkedListBoxNames.SelectedItems.Count == 0)
                {
                    e.Cancel = true;
                }
            };

            // bind commands
            const int RemoveItem = 0;
            const int RemoveItemAndIgnore = 1;
            contextMenuStrip1.Items[RemoveItem].Click += (sender, e) =>
            {
                RemoveNameAtSelectedIndex();
            };

            contextMenuStrip1.Items[RemoveItemAndIgnore].Click += (sender, e) =>
            {
                if (checkedListBoxNames.SelectedIndex < 0)
                {
                    return;
                }

                string selName = checkedListBoxNames.SelectedItem.ToString();
                // returns item index
                //string selName2 = checkedListBoxNames.GetItemText(checkedListBoxNames.SelectedIndex);

                Configs.Settings.IgnoreWords.Add(selName);
                RemoveNameAtSelectedIndex();
            };

            checkedListBoxNames.SelectedIndexChanged += (sende, e) =>
            {
                if (checkedListBoxNames.SelectedIndex < 0)
                {
                    return;
                }

                const int TextBeforeFix = 1;
                string selName = checkedListBoxNames.SelectedItem.ToString();
                var itemsWithSelName = listViewFixes.Items.Cast<ListViewItem>()
                .Where(li => li.SubItems[TextBeforeFix].Text.IndexOf(selName, StringComparison.OrdinalIgnoreCase) >= 0);
                foreach (var item in itemsWithSelName)
                {
                    // TODO: the color is kinda greyish which, make it blue by overriding listview item drawing...
                    item.Selected = true; //  System.Drawing.Color.Green;
                }
            };

            buttonCancel.Click += delegate
            {
                DialogResult = DialogResult.Cancel;
            };

            selectAllToolStripMenuItem.Click += delegate
            {
                checkedListBoxNames.BeginUpdate();
                for (int i = 0; i < checkedListBoxNames.Items.Count; i++)
                {
                    checkedListBoxNames.SetItemChecked(i, true);
                }
                checkedListBoxNames.EndUpdate();
            };

            invertSelectionToolStripMenuItem.Click += delegate
            {
                checkedListBoxNames.BeginUpdate();
                for (int i = 0; i < checkedListBoxNames.Items.Count; i++)
                {
                    checkedListBoxNames.SetItemChecked(i, !checkedListBoxNames.GetItemChecked(i));
                }
                checkedListBoxNames.EndUpdate();
            };


            labelCount.Text = "Total: 0";
            _subtitle = subtitle;
            _uILineBreak = UILineBreak;
            UIInit();
        }

        private void RemoveNameAtSelectedIndex()
        {
            checkedListBoxNames.Items.RemoveAt(checkedListBoxNames.SelectedIndex);
            // hate doing this
            var tempList = new List<string>();
            for (int i = 0; i < checkedListBoxNames.Items.Count; i++)
            {
                if (checkedListBoxNames.GetItemCheckState(i) == CheckState.Checked)
                {
                    tempList.Add(checkedListBoxNames.Items[i].ToString());
                }
            }
            DoCasingViaAPI(tempList);
        }

        private void UIInit()
        {
            checkedListBoxNames.ContextMenuStrip = contextMenuStrip1;

            listViewFixes.BeginUpdate();
            int optimalWidth = (listViewFixes.Width - listViewFixes.Columns[0].Width) / (listViewFixes.Columns.Count - 1);
            for (int i = listViewFixes.Columns.Count - 1; i > 0; i--)
            {
                listViewFixes.Columns[i].Width = optimalWidth;
            }
            listViewFixes.EndUpdate();

            if (Configs.Settings.Movies.Count > 0)
            {
                comboBoxMovieID.BeginUpdate();
                comboBoxMovieID.Items.AddRange(Configs.Settings.Movies.ToArray<Movie>());
                comboBoxMovieID.BeginUpdate();
            }

            // combobox movie id handlers
            comboBoxMovieID.SelectedIndexChanged += async (sende, e) =>
            {
                var movie = (Movie)comboBoxMovieID.SelectedItem;
                await GetNewIDAsync(movie.Id);
            };

            // user press enter e.g: after typing move id
            comboBoxMovieID.KeyUp += (sender, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    ButtonGetMovieID_Click(this, default);
                }
            };
        }

        private async Task GetNewIDAsync(int movieId)
        {
            if (string.IsNullOrEmpty(Configs.Settings.ApiKey))
            {
                using (var apiForm = new ApiForm())
                {
                    if (apiForm.ShowDialog(this) == DialogResult.OK)
                    {
                        UpdateComboboxMovieId(false);
                    }
                    else
                    {
                        return;
                    }
                }
            }

            // check if still null then return...
            if (string.IsNullOrEmpty(Configs.Settings.ApiKey))
            {
                MessageBox.Show("Api key not found!\r\nPlease sign up https://www.themoviedb.org/account/signup",
                          "Api key", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            //var guest = await _client.AuthenticationCreateGuestSessionAsync();

            _client = _client ?? new TMDbClient(Configs.Settings.ApiKey, true);

            ChangeControlsState(false);

            // invalid movie id
            if (movieId <= 0)
            {
                using (var getMovieID = new GetMovieID(_client))
                {
                    if (getMovieID.ShowDialog(this) == DialogResult.OK)
                    {
                        UpdateComboboxMovieId(true);
                    }
                    else
                    {
                        ChangeControlsState(true);
                    }
                    return;
                }
            }

            string movieIdString = comboBoxMovieID.Text;
            if (string.IsNullOrEmpty(movieIdString))
            {
                ChangeControlsState(true);
                return;
            }

            var movie = await _client.GetMovieAsync(movieId, MovieMethods.Credits).ConfigureAwait(true);

            HashSet<string> names = null;
            foreach (string name in movie.Credits.Cast.SelectMany(cast => cast.Character.Split(' ')))
            {
                names = names ?? new HashSet<string>();

                string tempName = name.Trim();
                tempName = tempName.Trim('\'', '"');
                tempName = tempName.Trim();

                if (string.IsNullOrEmpty(tempName))
                {
                    continue;
                }
                if (tempName.StartsWith('#'))
                {
                    continue;
                }
                if (tempName.Length == 1)
                {
                    continue;
                }
                if (tempName.StartsWith('('))
                {
                    continue;
                }
                // ignore nouns
                if (Configs.Settings.IgnoreWords?.Contains(tempName.ToLower()) == true)
                {
                    continue;
                }
                names.Add(tempName);
            }

            if (names.Count == 0)
            {
                return;
            }

            UpdateListView(names);

            // invoke SubtitleEdit method for casing
            DoCasingViaAPI(names.ToList());
            ChangeControlsState(true);

        }

        private void ChangeControlsState(bool state)
        {
            buttonGetMovieID.Enabled = state;
            buttonGetNewID.Enabled = state;
        }

        // WARNING: Careful calling this method.
        private void UpdateComboboxMovieId(bool selectLastIndex)
        {
            comboBoxMovieID.BeginUpdate();
            if (comboBoxMovieID.Items.Count > 0)
            {
                comboBoxMovieID.Items.Clear();
            }
            comboBoxMovieID.Items.AddRange(Configs.Settings.Movies.ToArray());
            if (selectLastIndex)
            {
                // this will re-fire this method to return after this lien to avoid deadlock
                comboBoxMovieID.SelectedIndex = comboBoxMovieID.Items.Count - 1;
            }
            comboBoxMovieID.EndUpdate();
        }

        private async void ButtonGetMovieID_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(comboBoxMovieID.Text) && comboBoxMovieID.SelectedItem == null)
            {
                MessageBox.Show("Select a movie from combobox or get a new one by clicking \"Get new ID\" button!",
                    "Missing ID", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (int.TryParse(comboBoxMovieID.Text, out int movieId))
            {
                await GetNewIDAsync(movieId);
            }
            if (comboBoxMovieID.SelectedItem != null)
            {
                await GetNewIDAsync(((Movie)comboBoxMovieID.SelectedItem).Id);
            }
            if (comboBoxMovieID.Text.Length > 0)
            {
                MessageBox.Show("Invalid movie id");
            }
            // todo: should clear controls?
        }

        private void UpdateListView(IEnumerable<string> names)
        {
            checkedListBoxNames.BeginUpdate();
            checkedListBoxNames.Items.Clear();
            foreach (var name in names.OrderBy(name => name))
            {
                checkedListBoxNames.Items.Add(name);
                checkedListBoxNames.SetItemChecked(checkedListBoxNames.Items.Count - 1, true);
            }
            checkedListBoxNames.EndUpdate();
        }

        private void DoCasingViaAPI(List<string> names)
        {
            Cursor.Current = Cursors.WaitCursor;
            var paragraphs = _subtitle.Paragraphs.Select(p => p.GetCopy()).ToList();
            var seCasingApi = new SECasingApi();

            var context = new CasingContext
            {
                Names = names,
                Paragraphs = paragraphs,
            };

            seCasingApi.DoCasing(context);
            // seCasingApi.DoCasing(paragraphs, names.ToList());

            UpdateListView(_subtitle.Paragraphs, paragraphs);
            Cursor.Current = Cursors.Default;
        }

        private void UpdateListView(List<Paragraph> paragaphs, List<Paragraph> paragraphsNew)
        {
            listViewFixes.BeginUpdate();
            listViewFixes.Items.Clear();
            int count = paragaphs.Count;

            for (int i = 0; i < count; i++)
            {
                // nothng changed
                if (paragaphs[i].Text.Equals(paragraphsNew[i].Text, StringComparison.Ordinal))
                {
                    continue;
                }

                ListViewItem lvi = new ListViewItem(paragraphsNew[i].Number.ToString())
                {
                    SubItems = { paragaphs[i].Text.Replace(Environment.NewLine, _uILineBreak), paragraphsNew[i].Text.Replace(Environment.NewLine, _uILineBreak) }
                };

                lvi.Tag = paragaphs[i];

                listViewFixes.Items.Add(lvi);
            }

            labelCount.Text = $"Total: {count}";
            listViewFixes.EndUpdate();
        }

        private void SaveToXml(IEnumerable<string> names)
        {
            var xdoc = new XDocument(new XElement("names"));
            foreach (var name in names.OrderBy(n => n))
            {
                bool isMultiWord = name.Contains(' ');
                var el = new XElement("name", name);
                el.SetAttributeValue("ismultiword", isMultiWord);
                xdoc.Root.Add(el);
            }
            xdoc.Save("d:\\names.xml");
        }

        private void ButtonOK_Click(object sender, EventArgs e)
        {
            const int fixedTextIndex = 2;

            foreach (ListViewItem lvi in listViewFixes.Items.Cast<ListViewItem>())
            {
                string fixedText = lvi.SubItems[fixedTextIndex].Text;
                ((Paragraph)lvi.Tag).Text = fixedText.Replace(_uILineBreak, Environment.NewLine);
            }

            Subtitle = _subtitle.ToText();
            Configs.Save();
            DialogResult = DialogResult.OK;
        }

        private void ButtonUpdate_Click(object sender, EventArgs e)
        {
            var checkedNames = new List<string>();

            int count = checkedListBoxNames.Items.Count;
            for (int i = 0; i < count; i++)
            {
                if (checkedListBoxNames.GetItemChecked(i) == false)
                {
                    continue;
                }

                checkedNames.Add(checkedListBoxNames.Items[i].ToString());
            }

            Configs.Save();
            // do/redo casing with filtered names
            DoCasingViaAPI(checkedNames);
        }

        private void aPIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var apiForm = new ApiForm())
            {
                apiForm.ShowDialog(this);
            }
        }

        private void ButtonFixNames_Click(object sender, EventArgs e)
        {
            var names = checkedListBoxNames.Items.Cast<string>()
            .Select(name => name.Trim('#', '"', '\'').Trim())
            .Where(name => name.Length > 0).ToList();

            UpdateListView(names);
            DoCasingViaAPI(names);
        }

        private async void ButtonGetNewID_Click(object sender, EventArgs e)
        {
            await GetNewIDAsync(0);
        }
    }
}

// TODO:
// - save names
// - load saved names
