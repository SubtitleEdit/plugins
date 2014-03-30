using System;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.PluginLogic
{
    public partial class Main : Form
    {
        public string FixedSubtitle { get; private set; }
        private Subtitle _subtitle;
        private Form _parentForm;
        private string _name;
        private string _description;

        public Main()
        {
            InitializeComponent();
        }

        public Main(Form parentForm, Subtitle sub, string name, string description)
            : this()
        {
            // TODO: Complete member initialization
            this._parentForm = parentForm;
            this._subtitle = sub;
            this._name = name;
            this._description = description;
            this.FindDialogue();
        }

        private void FindDialogue()
        {
            foreach (Paragraph p in _subtitle.Paragraphs)
            {
                if (AnalyzeText(p.Text))
                {
                    AddToListDialogue(p, p.Text);
                }
            }
            if (_subtitle.Paragraphs.Count > 0)
                this.textBox1.Text = _subtitle.Paragraphs[0].Text;
        }

        private bool AnalyzeText(string text)
        {
            if (text.Replace(Environment.NewLine, string.Empty).Length == text.Length)
                return false;

            var s = Utilities.RemoveHtmlTags(text);
            if (s.StartsWith("-") && s.Contains(Environment.NewLine + "-"))
                return false;
            return true;
        }

        private void AddToListDialogue(Paragraph p, string after)
        {
            // Checked 
            ListViewItem item = new ListViewItem(string.Empty) { Checked = true, Tag = p };

            // Line #
            var subItem = new ListViewItem.ListViewSubItem(item, p.Number.ToString());
            item.SubItems.Add(subItem);

            // Before
            subItem = new ListViewItem.ListViewSubItem(item, p.Text.Replace(Environment.NewLine, "<br />"));
            item.SubItems.Add(subItem);

            // After
            subItem = new ListViewItem.ListViewSubItem(item, after.Replace(Environment.NewLine, "<br />"));
            item.SubItems.Add(subItem);

            // Add to listView
            this.listViewDialogue.Items.Add(item);
        }

        private void listViewDialogue_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            var paragraph = e.Item.Tag as Paragraph;
            if (paragraph != null)
            {
                this.textBox1.Text = paragraph.Text;
                this.textBox1.Tag = e.Item.Tag;
            }
        }

        private void buttonDash_Click(object sender, EventArgs e)
        {
            string old = textBox1.Text.Trim();
            string text = old;

            if (!string.IsNullOrEmpty(text))
            {
                text = "- " + text.Replace(Environment.NewLine, Environment.NewLine + "- ");
                this.textBox1.Text = text;

                if (text != old)
                {
                    ((Paragraph)textBox1.Tag).Text = text;
                }
            }
        }

        private void buttonItalic_Click(object sender, EventArgs e)
        {

        }

        private void buttonPrev_Click(object sender, EventArgs e)
        {

        }

        private void buttonNext_Click(object sender, EventArgs e)
        {

        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            this.FixedSubtitle = _subtitle.ToText(new SubRip());
            if (!string.IsNullOrEmpty(FixedSubtitle.Trim()))
            {
                DialogResult = System.Windows.Forms.DialogResult.OK;
            }
            DialogResult = DialogResult.Cancel;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {

        }
    }
}