using System;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using ItalicEachLine.Logic;

namespace ItalicEachLine
{
    public partial class MainForm : Form
    {
        private readonly Subtitle _subtitle;

        public string FixedSubtitle { get; private set; }

        public MainForm()
        {
            InitializeComponent();
            labelChanges.Text = string.Empty;
        }


        public MainForm(Subtitle sub, string title, string description, Form parentForm)
            : this()
        {
            Text = title;
            _subtitle = sub;
            GeneratePreview();
        }

        public static string ItalicEachLine(string text)
        {
            if (!text.Contains("<i", StringComparison.Ordinal))
            {
                return text;
            }

            var sb = new StringBuilder();
            var italicOn = false;
            foreach (var line in text.SplitToLines())
            {
                if (italicOn)
                {
                    sb.Append("<i>");
                }

                var s = line
                    .Replace("<I>", "<i>")
                    .Replace("</I>", "</i>")
                    .Replace("<i></i>", "");
                for (var i = 0; i < s.Length; i++)
                {
                    var ch = s[i];

                    if (ch == '<')
                    {
                        var part = s.Substring(i);
                        if (part.StartsWith("<i>", StringComparison.Ordinal))
                        {
                            italicOn = true;

                            if (sb.ToString() == "<i>")
                            {
                                sb = new StringBuilder();
                            }

                            sb.Append(ch);

                        }
                        else if (part.StartsWith("</i>", StringComparison.Ordinal))
                        {
                            italicOn = false;

                            if (sb.ToString().EndsWith("</i>"))
                            {
                                var s2 = sb.ToString();
                                sb = new StringBuilder(s2.Substring(0, s2.Length - 4));
                            }

                            sb.Append(ch);
                        }
                        else
                        {
                            sb.Append(ch);
                        }
                    }
                    else
                    {
                        sb.Append(ch);
                    }
                }

                if (italicOn)
                {
                    sb.Append("</i>");
                }

                sb.AppendLine();
            }

            return sb.ToString().TrimEnd();
        }

        public sealed override string Text
        {
            get => base.Text;
            set => base.Text = value;
        }

        private void GeneratePreview()
        {
            var numberOfChanges = 0;
            for (var index = 0; index < _subtitle.Paragraphs.Count; index++)
            {
                var p = _subtitle.Paragraphs[index];
                var before = p.Text;
                var after = ItalicEachLine(p.Text);
                if (before != after)
                {
                    AddToListView(p, before, after);
                    numberOfChanges++;
                }
            }

            labelChanges.Text = $"Number of changes: {numberOfChanges}";
        }

        private void AddToListView(Paragraph p, string before, string after)
        {
            var item = new ListViewItem(p.Number.ToString(CultureInfo.InvariantCulture))
            {
                Tag = p,
                Checked = true
            };
            item.SubItems.Add(p.Text.Replace(Environment.NewLine, "<br />"));
            item.SubItems.Add(after.Replace(Environment.NewLine, "<br />"));
            listView1.Items.Add(item);
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            for (var index = 0; index < listView1.Items.Count; index++)
            {
                var item = listView1.Items[index];
                if (item.Checked)
                {
                    var p = (Paragraph)item.Tag;
                    p.Text = ItalicEachLine(p.Text);
                }
            }

            FixedSubtitle = _subtitle.ToText(new SubRip());
            DialogResult = DialogResult.OK;
        }

        private void listView1_Resize(object sender, EventArgs e)
        {
            var size = (listView1.Width - listView1.Columns[0].Width) >> 2;
            listView1.Columns[1].Width = size;
            listView1.Columns[2].Width = -2;
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            var subtract = listView1.Columns[0].Width + 20;
            var width = listView1.Width / 2 - subtract;
            listView1.Columns[1].Width = width;
            listView1.Columns[1].Width = width;
            listView1.Columns[2].Width = width;
            listView1.Columns[1].Width = width;
            listView1.Columns[1].Width = width;
        }

        private void MainForm_ResizeEnd(object sender, EventArgs e)
        {
            MainForm_Resize(sender, e);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            MainForm_Resize(sender, e);
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            MainForm_Resize(sender, e);
        }
    }
}