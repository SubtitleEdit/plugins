using Nikse.SubtitleEdit.PluginLogic;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private string APIKey = "";

        public Main(Subtitle subtitle, string UILineBreak)
        {
            InitializeComponent();
            StartPosition = FormStartPosition.CenterParent;

#if DEBUG
            //APIKey = Environment.GetEnvironmentVariable("themoviedbapikey");
#endif
            // load apiKey
            APIKey = SettingUtils.GetApiKey();

            if (!string.IsNullOrEmpty(APIKey))
            {
                _client = new TMDbClient(APIKey, true);
            }

            labelCount.Text = "Total: 0";
            _subtitle = subtitle;
            _uILineBreak = UILineBreak;

            UIInit();
        }

        private void UIInit()
        {
            listViewFixes.BeginUpdate();
            int optimalWidth = (listViewFixes.Width - listViewFixes.Columns[0].Width) / (listViewFixes.Columns.Count - 1);
            for (int i = listViewFixes.Columns.Count - 1; i > 0; i--)
            {
                listViewFixes.Columns[i].Width = optimalWidth;
            }
            listViewFixes.EndUpdate();
        }

        public string Subtitle { get; private set; }

        private async void ButtonGetMovieID_Click(object sender, EventArgs e)
        {
            APIKey = SettingUtils.GetApiKey();

            if (string.IsNullOrEmpty(APIKey))
            {
                MessageBox.Show("Api key not found!\r\nPlease sign up https://www.themoviedb.org/account/signup",
                    "Api key", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            _client = _client ?? new TMDbClient(APIKey);


            buttonGetMovieID.Enabled = false;
            int movieId = 0;

            string movieIdS = comboBoxMovieID.Text.Trim();

            if (movieIdS.Length == 0)
            {
                using (var getMovieID = new GetMovieID(_client))
                {
                    if (getMovieID.ShowDialog(this) == DialogResult.OK)
                    {
                        comboBoxMovieID.Items.Add(getMovieID.ID.ToString().Trim());
                        comboBoxMovieID.SelectedIndex = comboBoxMovieID.Items.Count - 1;
                    }
                    else
                    {
                        return;
                    }
                }
            }
            else
            {
                movieId = int.Parse(movieIdS);
            }

            Movie movie = await _client.GetMovieAsync(movieId, MovieMethods.Credits).ConfigureAwait(true);

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
                names.Add(tempName);
            }

            //foreach (var cast in result.Credits.Cast)
            //{
            //    listView1.Items.Add(cast.Name);
            //}

            // TODO: Hack MAIN and find way to send these names to casing form

            if (names.Count == 0)
            {
                return;
            }

            UpdateListView(names);

            // invoke SubtitleEdit method for casing
            DoCasingViaAPI(names.ToList());

            buttonGetMovieID.Enabled = true;
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
            IEnumerable<Paragraph> copyParagraphs = _subtitle.Paragraphs.Select(p => new Paragraph(p.StartTime, p.EndTime, p.Text)
            {
                Number = p.Number
            });

            var paragraphs = new List<Paragraph>(copyParagraphs);

            var seCasingApi = new SECasingApi();

            var context = new CasingContext
            {
                Names = names,
                CheckLastLine = checkBoxCheckLastLine.Checked,
                Paragraphs = paragraphs,
                UppercaseAfterLineBreak = checkBoxUppercaseAfterBreak.Checked,
            };

            seCasingApi.DoCasing(context);
            // seCasingApi.DoCasing(paragraphs, names.ToList());

            UpdateListView(_subtitle.Paragraphs, paragraphs);
        }

        private void UpdateListView(List<Paragraph> paragaphs, List<Paragraph> newParagraphs)
        {
            listViewFixes.BeginUpdate();
            listViewFixes.Items.Clear();
            int count = paragaphs.Count;

            for (int i = 0; i < count; i++)
            {
                // nothng changed
                if (paragaphs[i].Text.Equals(newParagraphs[i].Text, StringComparison.Ordinal))
                {
                    continue;
                }

                ListViewItem lvi = new ListViewItem(newParagraphs[i].Number.ToString())
                {
                    SubItems = { paragaphs[i].Text.Replace(Environment.NewLine, _uILineBreak), newParagraphs[i].Text.Replace(Environment.NewLine, _uILineBreak) }
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

    }
}

// TODO: 
// - save names 
// - load saved names