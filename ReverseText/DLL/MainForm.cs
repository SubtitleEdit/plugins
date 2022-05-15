using System;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using WebViewTranslate.Logic;

namespace ReverseText
{
    /// <summary>
    /// 
    /// </summary>
    public partial class MainForm : Form
    {
        private readonly Subtitle _subtitle;

        public string FixedSubtitle { get; private set; }

        public MainForm()
        {
            InitializeComponent();
        }


        public MainForm(Subtitle sub, string title, string description, Form parentForm)
            : this()
        {
            Text = title;
            _subtitle = sub;
            GeneratePreview();
        }

        public static string Reverse(string s)
        {
            var sb = new StringBuilder();
            var pre = string.Empty;
            var post = string.Empty;

            if (s.StartsWith("{\\") && s.IndexOf("}") > 0)
            {
                var end = s.IndexOf("}");
                pre += s.Substring(0, end + 1);
                s = s.Remove(0, end + 1);
            }

            foreach (var line in s.SplitToLines())
            {
                var charArray = line.ToCharArray();
                Array.Reverse(charArray);
                sb.AppendLine(new string(charArray));
            }

            s = sb.ToString().Trim();
            s = s.Replace(">i<", "</i>");
            s = s.Replace(">b<", "</b>");
            s = s.Replace(">u<", "</u>");

            s = s.Replace(">i/<", "<i>");
            s = s.Replace(">b/<", "<b>");
            s = s.Replace(">u/<", "<u>");

            return pre + s;
        }

        public sealed override string Text
        {
            get => base.Text;
            set => base.Text = value;
        }

        private void GeneratePreview()
        {
            for (var index = 0; index < _subtitle.Paragraphs.Count; index++)
            {
                var p = _subtitle.Paragraphs[index];
                var before = p.Text;
                var after = Reverse(p.Text);
                AddToListView(p, before, after);
            }
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
            for (var index = 0; index < _subtitle.Paragraphs.Count; index++)
            {
                var item = listView1.Items[index];
                if (item.Checked)
                {
                    var p = _subtitle.Paragraphs[index];
                    p.Text = Reverse(p.Text);
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