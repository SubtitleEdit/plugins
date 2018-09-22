using Nikse.SubtitleEdit.PluginLogic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
                        progressBar1.Visible = true;
                        progressBar1.Style = ProgressBarStyle.Marquee;
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

            //result.Credits.Cast.
            progressBar1.Style = ProgressBarStyle.Blocks;
            progressBar1.Visible = false;

            HashSet<string> names = null;
            foreach (var name in movie.Credits.Cast.SelectMany(cast => cast.Character.Split(' ')))
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

            checkedListBoxNames.BeginUpdate();
            foreach (var name in names)
            {
                checkedListBoxNames.Items.Add(name);
                checkedListBoxNames.SetItemChecked(checkedListBoxNames.Items.Count - 1, true);
            }
            checkedListBoxNames.EndUpdate();

            // invoke SubtitleEdit method for casing
            DoCasingViaAPI(names);

            buttonGetMovieID.Enabled = true;
        }

        private void DoCasingViaAPI(IEnumerable<string> names)
        {
            var paragraphOld = new List<Paragraph>();
            foreach (var p in _subtitle.Paragraphs)
            {
                paragraphOld.Add(new Paragraph(p.StartTime, p.EndTime, p.Text)
                {
                    Number = p.Number
                });
            }

            var seCasingApi = new SECasingApi();
            seCasingApi.DoCasing(_subtitle.Paragraphs, names.ToList());
            UpdateListView(paragraphOld, _subtitle.Paragraphs);
        }

        private void UpdateListView(List<Paragraph> paragraphsOld, List<Paragraph> paragraphs)
        {
            listViewFixes.BeginUpdate();
            listViewFixes.Items.Clear();

            int count = paragraphs.Count;
            for (int i = 0; i < count; i++)
            {
                // nothng changed
                if (paragraphsOld[i].Text.Equals(paragraphs[i].Text))
                {
                    continue;
                }

                var lvi = new ListViewItem(paragraphs[i].Number.ToString())
                {
                    SubItems = { paragraphsOld[i].Text.Replace(Environment.NewLine, _uILineBreak),
                        paragraphs[i].Text.Replace(Environment.NewLine, _uILineBreak) }
                };

                listViewFixes.Items.Add(lvi);
            }
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

        private static bool IsClose(char ch)
        {
            return char.IsLetter(ch) ? false : true;
            //UnicodeCategory uc = char.GetUnicodeCategory(ch);
        }

        private void ButtonOK_Click(object sender, System.EventArgs e)
        {
            Subtitle = _subtitle.ToText();
            DialogResult = DialogResult.OK;
        }

        private void CheckedListBoxNames_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            //UpdateListView();
        }

        private void ButtonUpdate_Click(object sender, EventArgs e)
        {
            var checkedNames = new List<string>();

            for (int i = 0; i < checkedListBoxNames.Items.Count; i++)
            {
                if (checkedListBoxNames.GetItemChecked(i) == false)
                {
                    continue;
                }

                checkedNames.Add(checkedListBoxNames.Items[i].ToString());
            }

            DoCasingViaAPI(checkedNames);
        }

        private void LinkLabelSignUP_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(linkLabelSignUP.Tag as string);
        }

        private void aPIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var apiForm = new ApiForm())
            {
                apiForm.ShowDialog(this);
            }
        }
    }
}
