using Nikse.SubtitleEdit.PluginLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using TMDbLib.Client;

namespace OnlineCasing.Forms
{
    public partial class Main : Form
    {
        private readonly TMDbClient _client;
        private readonly Subtitle _subtitle;
        private Dictionary<string, string> _changedParagraphs;
        private readonly string APIKey = "";

        public Main(Subtitle subtitle)
        {
            InitializeComponent();
            APIKey = Environment.GetEnvironmentVariable("themoviedbapikey");
            StartPosition = FormStartPosition.CenterParent;
            _client = new TMDbClient(APIKey, true);
            _subtitle = subtitle;
        }

        public string Subtitle { get; private set; }

        private async void ButtonGetMovieID_Click(object sender, System.EventArgs e)
        {
            int id = 0;
            using (var getMovieID = new GetMovieID(_client))
            {
                if (getMovieID.ShowDialog() == DialogResult.OK)
                {
                    textBox1.Text = getMovieID.ID.ToString();
                    id = getMovieID.ID;
                }
            }

            var result = await _client.GetMovieAsync(id, TMDbLib.Objects.Movies.MovieMethods.Credits).ConfigureAwait(true);

            //result.Credits.Cast.

            var names = new HashSet<string>();
            foreach (var name in result.Credits.Cast.SelectMany(cast => cast.Character.Split(' ')))
            {
                string tempName = name.Trim();
                if (string.IsNullOrEmpty(tempName))
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

            DoCasing(names);
            SaveToXml(names);
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

        private void DoCasing(IEnumerable<string> names)
        {
            //_changedParagraphs = new Dictionary<string, string>();
            //int fixCount = 0;

            listViewFixes.BeginUpdate();
            listViewFixes.Items.Clear();
            foreach (Paragraph p in _subtitle.Paragraphs)
            {
                string text = p.Text;
                foreach (string name in names)
                {
                    if (!IsValid(name))
                    {
                        continue;
                    }

                    int idx = text.IndexOf(name, StringComparison.OrdinalIgnoreCase);

                    int nameLen = name.Length;
                    while (idx >= 0)
                    {
                        // check pre char
                        if (idx > 0)
                        {
                            char preChar = text[idx - 1];
                            if (!IsClose(preChar))
                            {
                                idx = text.IndexOf(name, idx + nameLen, StringComparison.OrdinalIgnoreCase);
                                continue;
                            }
                        }

                        // check post char
                        if (idx + name.Length < text.Length)
                        {
                            char postChar = text[idx + nameLen];
                            if (!IsClose(postChar))
                            {
                                idx = text.IndexOf(name, idx + nameLen, StringComparison.OrdinalIgnoreCase);
                                continue;
                            }
                        }

                        string nameInSubtitle = text.Substring(idx, nameLen);
                        if (name.Equals(nameInSubtitle, StringComparison.Ordinal) == false)
                        {
                            text = text.Remove(idx, nameLen);
                            text = text.Insert(idx, name);
                        }

                        idx = text.IndexOf(name, idx + nameLen, StringComparison.OrdinalIgnoreCase);
                    }
                }

                if (text.Equals(p.Text, StringComparison.Ordinal) == false)
                {
                    //_changedParagraphs.Add(p.ID, text);
                    listViewFixes.Items.Add(new ListViewItem(p.Number.ToString())
                    {
                        SubItems = { p.Text, text },
                    });
                    p.Text = text;
                }
            }

            listViewFixes.EndUpdate();

        }

        private static bool IsClose(char ch)
        {
            if (char.IsLetter(ch))
            {
                return false;
            }
            return true;
            //UnicodeCategory uc = char.GetUnicodeCategory(ch);
        }

        private bool IsValid(string name)
        {
            if (name.Length == 1)
                return false;
            return true;
        }

        private void ButtonOK_Click(object sender, System.EventArgs e)
        {
            Subtitle = _subtitle.ToText();
            DialogResult = DialogResult.OK;
        }
    }
}
